using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.ConnectedServices;
using System.Globalization;
using Microsoft.VisualStudio.WindowsAzure.Authentication;



namespace AzureIoTHubConnectedService
{
    public enum WizardMode
    {
        UseTpm,
        EmbedConnectionString,
        ProvisionConnectionString
    }

    /// <summary>
    /// This part of the class implements all stuff related to Visual Studio & Connected Services framework
    /// </summary>
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

            // XXX - this shoudl be moved to generic
            _DeviceTwinProperties.Add(new DeviceTwinProperty("SampleProperty1", "Desired", "String"));
            _DeviceTwinProperties.Add(new DeviceTwinProperty("SampleProperty2", "Reported", "String"));

            _DeviceMethods.Add(new DeviceMethodDescription("SampleMethod1"));
            _DeviceMethods.Add(new DeviceMethodDescription("SampleMethod2"));
            // XXX - <<<
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

        // XXX - check why we need it
        private void OnAuthenticatorAuthenticationChanged(object sender, AuthenticationChangedEventArgs e)
        {
        }

        private void OnAuthenticatorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Authenticator.IsAuthenticated))
            {
                if (Authenticator.IsAuthenticated)
                {
                    PopulateHubs();
                }
                else
                {
                    // XXX - clear hubs
                }

                ConfigurePages();
            }
        }

        /// <summary>
        /// Creates and returnes instace of the service to Connected Services framework.
        /// </summary>
        /// <returns></returns>
        public override Task<ConnectedServiceInstance> GetFinishedServiceInstanceAsync()
        {
            return Task.Run<ConnectedServiceInstance>(() =>
              {
                  ConnectedServiceInstance instance = new ConnectedServiceInstance();

                  instance.InstanceId = _CurrentHub.Id;
                  instance.Name = _CurrentHub.Properties["IoTHubName"];

                  foreach (var property in _CurrentHub.Properties)
                  {
                      instance.Metadata.Add(property.Key, property.Value);
                  }

                  instance.Metadata.Add("IoTHubAccount", _CurrentHub);
                  instance.Metadata.Add("Cancel", false);
                  instance.Metadata.Add("TPM", false);
                  instance.Metadata.Add("Device", _CurrentDevice);
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
                // check if resource group exists
                if (!ResourceGroups.Any<ResourceGroup> (rg => (rg.Name == resourceGroupName)))
                {
                    ResourceGroup group = await Authenticator.CreateResourceGroup(_IoTHubAccountManager, subscriptionName, resourceGroupName, new CancellationToken());
                }

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

        /// <summary>
        /// Selected wizard mode
        /// </summary>
        public WizardMode Mode
        {
            get { return _WizardMode; }
            set
            {
                _WizardMode = value;
                ConfigurePages();
            }
        }

        /// <summary>
        /// Can TPM option be used?
        /// </summary>
        public bool CanUseTPM
        {
            get
            {
                return _CanUseTPM;
            }
            set
            {
                _CanUseTPM = value;
            }
        }

        private bool _CanUseTPM = false;

        public ObservableCollection<string> Subscriptions
        {
            get { return _Subscriptions; }
            set { _Subscriptions = value; OnPropertyChanged("Subscriptions"); }
        }

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
            PrimaryKeys keys = await _CurrentHub.GetPrimaryKeysAsync(new CancellationToken());

            _CurrentHub_ConnectionString = string.Format(CultureInfo.InvariantCulture,
                "HostName={0};SharedAccessKeyName=iothubowner;SharedAccessKey={1}",
                _CurrentHub.Properties["iotHubUri"], keys.IoTHubOwner);

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

                Subscriptions = new ObservableCollection<string>(Authenticator.Subscriptions.ToList<IAzureRMSubscription>().ConvertAll<string>(obj => obj.SubscriptionName));
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
                _PageDeviceSelection.IsEnabled = (CurrentHub_Name != "");
                _PageDeviceTwin.IsEnabled = (CurrentDevice_Id != "");

                _PageDeviceMethod.IsEnabled = (CurrentDevice_Id != "");
                _PageInjectConnectionString.IsEnabled = (CurrentDevice_Id != "");
                _PageSummary.IsEnabled = (CurrentDevice_Id != "");
            }
        }

        // XXX - should go to generic??
        public void ProvisionDevice()
        {
            _ProvisioningDevice = true;

            try
            {
                DeviceProvisionerBase provisioner = null;

                provisioner.ProvisionDevice(CurrentDevice.Id, CurrentDevice.Authentication.SymmetricKey.PrimaryKey);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to provision device: " + ex.Message);
            }

            _ProvisioningDevice = false;
        }

        public void DisplayMessage(string message)
        {
            MessageBox.Show(message);
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

        private ObservableCollection<string> _Subscriptions = null;

        private IServiceProvider _ServiceProvider = null;
        private IAzureIoTHubAccountManager _IoTHubAccountManager = null;
        private Authenticator _Authenticator = null;

        private bool _ProvisioningDevice = false;
    }
}
