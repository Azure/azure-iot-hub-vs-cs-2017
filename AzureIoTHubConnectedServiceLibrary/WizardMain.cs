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
            _PageSummary = new WizardPageSummary(this);

            this.Pages.Add(_PageLogin);
            this.Pages.Add(_PageHubSelection);
            this.Pages.Add(_PageDeviceSelection);
            this.Pages.Add(_PageDeviceTwin);
            this.Pages.Add(_PageDeviceMethod);
            this.Pages.Add(_PageSummary);
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
                  instance.Metadata.Add("TwinReportedProps", DeviceTwinUpdate);
                  instance.Metadata.Add("DeviceMethod", DeviceMethodName);
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
            catch (Exception /*ex*/)
            {
                // XXX - error message
            }

            DecrementBusyCounter();
        }

        /*--------------------------------------------------------------------------------------------------------------------
         * PUBLIC PROPERTIES
         *--------------------------------------------------------------------------------------------------------------------*/

        public ObservableCollection<IAzureRMSubscription> Subscriptions
        {
            get { return _Subscriptions; }
            set { _Subscriptions = value; OnPropertyChanged("Subscriptions"); }
        }

        /*--------------------------------------------------------------------------------------------------------------------
         * INTERNAL IMPLEMENTATION
         *--------------------------------------------------------------------------------------------------------------------*/

        private async void HandleHubSelected()
        {
            // XXX - how to handle cancellation token exactly??
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
            catch (Exception)
            {
            }

            DecrementBusyCounter();
        }

        private void ConfigurePages()
        {
            if (UseTPM)
            {
                Authenticator.View.IsEnabled = false;
                _PageHubSelection.IsEnabled = false;
                _PageDeviceSelection.IsEnabled = false;
                _PageSummary.IsEnabled = false;
                IsFinishEnabled = true;
            }
            else
            {
                Authenticator.View.IsEnabled = true;
                _PageHubSelection.IsEnabled = Authenticator.IsAuthenticated;
                _PageDeviceSelection.IsEnabled = (IoTHubName != "");
                _PageDeviceTwin.IsEnabled = (DeviceId != "");
                _PageDeviceMethod.IsEnabled = (DeviceId != "");
                _PageSummary.IsEnabled = (DeviceId != "");
                IsFinishEnabled = _PageSummary.IsEnabled;
            }
        }


        /*--------------------------------------------------------------------------------------------------------------------
         * INTERNAL DATA
         *--------------------------------------------------------------------------------------------------------------------*/
        private WizardPageLogin _PageLogin = null;
        private WizardPageHubSelection _PageHubSelection = null;
        private WizardPageDeviceSelection _PageDeviceSelection = null;
        private WizardPageDeviceTwin _PageDeviceTwin = null;
        private WizardPageDeviceMethod _PageDeviceMethod = null;
        private WizardPageSummary _PageSummary = null;

        private ObservableCollection<IAzureRMSubscription> _Subscriptions = null;

        private IServiceProvider _ServiceProvider = null;
        private IAzureIoTHubAccountManager _IoTHubAccountManager = null;
        private Authenticator _Authenticator = null;
    }
}
