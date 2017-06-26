using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AzureIoTHubConnectedService;
using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.Azure.Devices;
using System.Globalization;
using Microsoft.VisualStudio.WindowsAzure.Authentication;
using System.Windows.Input;

using Microsoft.Azure.Devices.Common;
using Microsoft.Azure.Devices.Common.Security;
using Microsoft.ServiceBus.Messaging;

using System.Text;



namespace AzureIoTHubConnectedService
{
    public enum WizardMode
    {
        UseTpm,
        EmbedConnectionString,
        ProvisionConnectionString
    }

    public partial class WizardMain : ConnectedServiceWizard
    {
        public WizardMain(IAzureIoTHubAccountManager accountManager, IServiceProvider serviceProvider, bool canUseTpm)
        {
            this._IoTHubAccountManager = accountManager;
            this._ServiceProvider = serviceProvider;

            CanUseTPM = canUseTpm;

            _PageLogin = new WizardPageLogin(this, Authenticator);
            _PageHubSelection = new WizardPageHubSelection(this);
            _PageDeviceSelection = new WizardPageDeviceSelection(this);
            _PageDeviceTwin = new WizardPageDeviceTwin(this);
            _PageDeviceMethod = new WizardPageDeviceMethod(this);
            _PageInjectConnectionString = new WizardPageInjectConnectionString(this);
            _PageSummary = new WizardPageSummary(this);

            this.Pages.Add(_PageLogin);
            this.Pages.Add(_PageHubSelection);
            this.Pages.Add(_PageDeviceSelection);
            this.Pages.Add(_PageInjectConnectionString);
            this.Pages.Add(_PageSummary);

            _DeviceTwinProperties.Add(new DeviceTwinProperty("SampleProperty1", "Desired", "String"));
            _DeviceTwinProperties.Add(new DeviceTwinProperty("SampleProperty2", "Reported", "String"));

            _DeviceMethods.Add(new DeviceMethodDescription("SampleMethod1"));
            _DeviceMethods.Add(new DeviceMethodDescription("SampleMethod2"));
        }

        private Authenticator Authenticator
        {
            get
            {
                if ((this._Authenticator == null) && (_ServiceProvider != null))
                {
                    this._Authenticator = new Authenticator(this._ServiceProvider);
                    this._Authenticator.PropertyChanged += this.OnAuthenticatorPropertyChanged;
                    this._Authenticator.AuthenticationChanged += this.OnAuthenticatorAuthenticationChanged;
                }
                return this._Authenticator;
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (this._Authenticator != null)
                    {
                        this._Authenticator.PropertyChanged -= this.OnAuthenticatorPropertyChanged;
                        this._Authenticator.AuthenticationChanged -= this.OnAuthenticatorAuthenticationChanged;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void OnAuthenticatorAuthenticationChanged(object sender, AuthenticationChangedEventArgs e)
        {
        }

        private void OnAuthenticatorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName == nameof(Authenticator.IsAuthenticated))
            {
                PopulateHubs();
                ConfigurePages();
            }
        }

        public override Task<ConnectedServiceInstance> GetFinishedServiceInstanceAsync()
        {
            return Task.Run<ConnectedServiceInstance>(() =>
              {
                  ConnectedServiceInstance instance = new ConnectedServiceInstance();

                  instance.InstanceId = _SelectedHub.Id;
                  instance.Name = _SelectedHub.Properties["IoTHubName"];

                  foreach (var property in _SelectedHub.Properties)
                  {
                      instance.Metadata.Add(property.Key, property.Value);
                  }

                  instance.Metadata.Add("IoTHubAccount", _SelectedHub);
                  instance.Metadata.Add("Cancel", false);
                  instance.Metadata.Add("TPM", false);
                  instance.Metadata.Add("Device", m_SelectedDevice);
                  instance.Metadata.Add("ProvisionedDevice", _WizardMode == WizardMode.ProvisionConnectionString);

                  if (DeviceMethodEnabled)
                  {
                      instance.Metadata.Add("DeviceMethods", DeviceMethods.ToArray<DeviceMethodDescription>());
                  }

                  if (DeviceTwinEnabled)
                  {
                      instance.Metadata.Add("DeviceTwinProperties", DeviceTwinProperties.ToArray<DeviceTwinProperty>());
                  }

                  return instance;
              });
        }

        /*--------------------------------------------------------------------------------------------------------------------
         * PUBLIC API
         *--------------------------------------------------------------------------------------------------------------------*/

        public async void CreateNewHub(string subscriptionName, string resourceGroupName, string iotHubName)
        {
            IncrementBusyCounter();

            try
            {
                IAzureIoTHub hub = await Authenticator.CreateIoTHub(_IoTHubAccountManager, subscriptionName, resourceGroupName, iotHubName, new CancellationToken());

                // insert hub into the list
                AddHub(hub);

                // select hub automatically
                _PageHubSelection.SelectHub(hub);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create new IoT Hub: " + ex.Message);
            }

            DecrementBusyCounter();
        }

        public async void QueryResourceGroups(string subscriptionName)
        {
            List<ResourceGroup> response = await Authenticator.GetResourceGroups(_IoTHubAccountManager, subscriptionName, new CancellationToken());

            ResourceGroups = new ObservableCollection<ResourceGroup>(response);
        }

        /*--------------------------------------------------------------------------------------------------------------------
         * PUBLIC PROPERTIES
         *--------------------------------------------------------------------------------------------------------------------*/

        public WizardMode Mode
        {
            get { return _WizardMode; }
            set
            {
                _WizardMode = value;
                ConfigurePages();
            }
        }

        public ObservableCollection<IAzureRMSubscription> Subscriptions
        {
            get { return _Subscriptions; }
            set { _Subscriptions = value; OnPropertyChanged("Subscriptions"); }
        }

        public ObservableCollection<ResourceGroup> ResourceGroups
        {
            get { return _ResourceGroups; }
            set { _ResourceGroups = value; OnPropertyChanged("ResourceGroups"); }
        }

        public bool DeviceTwinEnabled {
            get
            {
                return _DeviceMethodEnabled;
            }
            set
            {
                _DeviceMethodEnabled = value;
            }
        }
        public bool DeviceMethodEnabled
        {
            get
            {
                return _DeviceTwinEnabled;
            }
            set
            {
                _DeviceTwinEnabled = value;
            }
        }
    
        public ObservableCollection<DeviceTwinProperty> DeviceTwinProperties { get { return _DeviceTwinProperties; } }

        public ObservableCollection<DeviceMethodDescription> DeviceMethods { get { return _DeviceMethods; } }

        public bool SummaryVisible
        {
            set
            {
                IsFinishEnabled = value;
            }
        }


        /*--------------------------------------------------------------------------------------------------------------------
         * INTERNAL IMPLEMENTATION
         *--------------------------------------------------------------------------------------------------------------------*/

        private async void HandleHubSelected()
        {
            PrimaryKeys keys = await _SelectedHub.GetPrimaryKeysAsync(new CancellationToken());

            m_SelectedHubConnectionString = string.Format(CultureInfo.InvariantCulture,
                "HostName={0};SharedAccessKeyName=iothubowner;SharedAccessKey={1}",
                _SelectedHub.Properties["iotHubUri"], keys.IoTHubOwner);

            ConfigurePages();
            PopulateDevices();
        }

        private async void PopulateHubs()
        {
            IncrementBusyCounter();

            try
            {
                // [ZKK] todo: fix cancellation token issue
                Task<IEnumerable<IAzureIoTHub>> task = Authenticator.GetAzureIoTHubs(_IoTHubAccountManager, new CancellationToken());
                Hubs = new ObservableCollection<IAzureIoTHub>(await task);
                Subscriptions = new ObservableCollection<IAzureRMSubscription>(Authenticator.Subscriptions);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to query IoT hubs: " + ex.Message);
            }

            DecrementBusyCounter();
        }

        private void ConfigurePages()
        {
            if (Mode == WizardMode.ProvisionConnectionString)
            {
                if (this.Pages.IndexOf(_PageInjectConnectionString) < 0)
                {
                    int index = this.Pages.IndexOf(_PageSummary);
                    this.Pages.Insert(index, _PageInjectConnectionString);
                }
            }
            else
            {
                if (this.Pages.IndexOf(_PageInjectConnectionString) >= 0)
                {
                    this.Pages.Remove(_PageInjectConnectionString);
                }
            }

            if (Mode == WizardMode.UseTpm)
            {
                Authenticator.View.IsEnabled = false;
                _PageHubSelection.IsEnabled = false;
                _PageDeviceSelection.IsEnabled = false;
                _PageSummary.IsEnabled = false;
            }
            else
            {
                Authenticator.View.IsEnabled = true;
                _PageHubSelection.IsEnabled = Authenticator.IsAuthenticated;
                _PageDeviceSelection.IsEnabled = (IoTHubName != "");
                _PageDeviceTwin.IsEnabled = (DeviceId != "");

                _PageDeviceMethod.IsEnabled = (DeviceId != "");
                _PageInjectConnectionString.IsEnabled = (DeviceId != "");
                _PageSummary.IsEnabled = (DeviceId != "");
            }
        }


        public void ProvisionDevice()
        {
            _ProvisioningDevice = true;

            try
            {
                DeviceProvisionerBase provisioner = null;

                provisioner.ProvisionDevice(SelectedDevice.Id, SelectedDevice.Authentication.SymmetricKey.PrimaryKey);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to provision device: " + ex.Message);
            }

            _ProvisioningDevice = false;
        }

        /*--------------------------------------------------------------------------------------------------------------------
         * INTERNAL DATA
         *--------------------------------------------------------------------------------------------------------------------*/
        private WizardPageLogin _PageLogin = null;
        private WizardPageHubSelection _PageHubSelection = null;
        private WizardPageDeviceSelection _PageDeviceSelection = null;
        private WizardPageDeviceTwin _PageDeviceTwin = null;
        private WizardPageDeviceMethod _PageDeviceMethod = null;
        private WizardPageInjectConnectionString _PageInjectConnectionString = null;
        private WizardPageSummary _PageSummary = null;

        private WizardMode _WizardMode = WizardMode.EmbedConnectionString;
        private ObservableCollection<IAzureRMSubscription> _Subscriptions = null;
        private ObservableCollection<ResourceGroup> _ResourceGroups = null;

        private IServiceProvider _ServiceProvider = null;
        private IAzureIoTHubAccountManager _IoTHubAccountManager = null;
        private Authenticator _Authenticator = null;

        private bool _DeviceTwinEnabled = false;
        private ObservableCollection<DeviceTwinProperty> _DeviceTwinProperties = new ObservableCollection<DeviceTwinProperty>();


        private bool _DeviceMethodEnabled = false;
        private ObservableCollection<DeviceMethodDescription> _DeviceMethods = new ObservableCollection<DeviceMethodDescription>();

        private bool _ProvisioningDevice = false;
    }
}
