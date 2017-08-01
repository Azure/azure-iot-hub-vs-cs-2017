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
    /// <summary>
    /// Currently supported wizard modes.
    /// </summary>
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
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="accountManager"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="canUseTpm"></param>
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

            // should be moved to generic
            _DeviceTwinProperties.Add(new DeviceTwinProperty("SampleProperty1", "Desired", "String"));
            _DeviceTwinProperties.Add(new DeviceTwinProperty("SampleProperty2", "Reported", "String"));

            _DeviceMethods.Add(new DeviceMethodDescription("SampleMethod1"));
            _DeviceMethods.Add(new DeviceMethodDescription("SampleMethod2"));
        }

        /// <summary>
        /// Authenticator object.
        /// Comes from Visual Studio.
        /// </summary>
        private Authenticator Authenticator
        {
            get
            {
                if ((this._Authenticator == null) && (_ServiceProvider != null))
                {
                    this._Authenticator = new Authenticator(this._ServiceProvider);
                    this._Authenticator.PropertyChanged += this.OnAuthenticatorPropertyChanged;
                    this._Authenticator.AuthenticationChanged += this.OnAuthenticationChanged;
                }
                return this._Authenticator;
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (this._Authenticator != null)
                    {
                        this._Authenticator.PropertyChanged -= this.OnAuthenticatorPropertyChanged;
                        this._Authenticator.AuthenticationChanged -= this.OnAuthenticationChanged;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Callback when authentication state changes.
        /// Used to trigger IoT Hub population.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    ClearHubs();
                }

                ConfigurePages();
            }
        }

        /// <summary>
        /// Callback when authentication changes.
        /// Used to trigger IoT Hub population.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAuthenticationChanged(object sender, AuthenticationChangedEventArgs e)
        {
            ClearHubs();

            if (Authenticator.IsAuthenticated)
            {
                PopulateHubs();
            }

            ConfigurePages();
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

        /// <summary>
        /// Creates new IoT Hub.
        /// 
        /// </summary>
        /// <param name="subscriptionName"></param>
        /// <param name="resourceGroupName"></param>
        /// <param name="location"></param>
        /// <param name="iotHubName"></param>
        public async void CreateNewHub(string subscriptionName, string resourceGroupName, string location, string iotHubName)
        {
            NewHub_FieldsEnabled = false;
            IncrementBusyCounter();

            try
            {
                // check if resource group exists
                if (!ResourceGroups.Any<ResourceGroup> (rg => (rg.Name == resourceGroupName)))
                {
                    ResourceGroup group = await Authenticator.CreateResourceGroup(_IoTHubAccountManager, subscriptionName, resourceGroupName, new CancellationToken());
                }

                IAzureIoTHub hub = await Authenticator.CreateIoTHub(_IoTHubAccountManager, subscriptionName, resourceGroupName, location, iotHubName, new CancellationToken());

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
            NewHub_FieldsEnabled = true;
        }

        /// <summary>
        /// Query resource groups for given subscription.
        /// </summary>
        /// <param name="subscriptionName"></param>
        public async void QueryResourceGroups(string subscriptionName)
        {
            List<ResourceGroup> response = await Authenticator.GetResourceGroups(_IoTHubAccountManager, subscriptionName, new CancellationToken());

            ResourceGroups = (response != null) ? new ObservableCollection<ResourceGroup>(response) : null;

            List<ResourceLocation> locations = await Authenticator.GetLocations(_IoTHubAccountManager, subscriptionName, new CancellationToken());

            Locations = (locations != null) ? new ObservableCollection<ResourceLocation>(locations) : null;
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

        /// <summary>
        /// Subscriptions.
        /// </summary>
        public ObservableCollection<string> Subscriptions
        {
            get { return _Subscriptions; }
            set { _Subscriptions = value; OnPropertyChanged("Subscriptions"); }
        }

        /// <summary>
        /// This is used by Summary page, when it's visible, Finish button should be enabled.
        /// </summary>
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

        /// <summary>
        /// Handles hubs selection.
        /// </summary>
        private async void HandleHubSelected()
        {
            if (_CurrentHub != null)
            {
                PrimaryKeys keys = await _CurrentHub.GetPrimaryKeysAsync(new CancellationToken());

                _CurrentHub_ConnectionString = string.Format(CultureInfo.InvariantCulture,
                    "HostName={0};SharedAccessKeyName=iothubowner;SharedAccessKey={1}",
                    _CurrentHub.Properties["iotHubUri"], keys.IoTHubOwner);
            }
            else
            {
                _CurrentHub_ConnectionString = "";
            }

            ConfigurePages();
            PopulateDevices();
        }

        /// <summary>
        /// Handles device selection.
        /// </summary>
        private void HandleDeviceSelected()
        {
            ConfigurePages();
        }

        /// <summary>
        /// Populates lists of IoT Hubs and subcriptions.
        /// </summary>
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

        /// <summary>
        /// Clears list of IoT Hubs and Subscriptions.
        /// </summary>
        private void ClearHubs()
        {
            Hubs = null;
            Subscriptions = null;
        }   

        /// <summary>
        /// Configure wizard pages accordingly to mode.
        /// </summary>
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

        /// <summary>
        /// Provisions device.
        /// This should probably go to generic part of the class as it's VS independent.
        /// </summary>
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

        public bool ProvisioningDevice
        {
            get { return _ProvisioningDevice; }
        }

        /// <summary>
        /// Display message
        /// </summary>
        /// <param name="message"></param>
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
