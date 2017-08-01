using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Globalization;

namespace AzureIoTHubConnectedService
{
    public partial class WizardMain : INotifyPropertyChanged
    {
        public WizardMain()
        {
            _Hubs = new ObservableCollection<IAzureIoTHub>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void HandleHubSelected()
        {
            if (_CurrentHub != null)
            {
                _CurrentHub_ConnectionString = string.Format(CultureInfo.InvariantCulture,
                    "HostName={0};SharedAccessKeyName=iothubowner;SharedAccessKey={1}",
                    _CurrentHub.Properties["iotHubUri"], "fakekey-fakekey-fakekey");
            }
            else
            {
                _CurrentHub_ConnectionString = "";
            }

            PopulateDevices();
        }

        private void HandleDeviceSelected()
        {
        }

        public async void CreateNewHub(string subscriptionName, string resourceGroupName, string location, string iotHubName)
        {
            NewHub_FieldsEnabled = false;
            IncrementBusyCounter();

            AzureIoTHubFake hub = new AzureIoTHubFake();

            if (!simulateOperationFailure)
            {
                hub.WritableProperties.Add("IoTHubName", iotHubName);
                hub.WritableProperties.Add("Region", "westus");
                hub.WritableProperties.Add("SubscriptionName", subscriptionName);
                hub.WritableProperties.Add("ResourceGroup", resourceGroupName);
                hub.WritableProperties.Add("iotHubUri", iotHubName + ".azuredevices.net");

                AddHub(hub);
            }
            else
            {
                DisplayMessage("Failed to create new IoT Hub");
            }

            DecrementBusyCounter();
            NewHub_FieldsEnabled = true;
        }

        public async void QueryResourceGroups(string subscriptionName) { }

        // this is a hack to handle some inconsistencies that still exist
        public WizardMain MainModel { get { return this; } }

        public void ProvisionDevice()
        {

        }

        public void DisplayMessage(string message)
        {
            //MessageBox(message);
        }

        public bool simulateOperationFailure = false;
    }
}
