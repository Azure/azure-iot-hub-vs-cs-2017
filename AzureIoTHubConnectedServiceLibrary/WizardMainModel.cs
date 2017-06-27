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

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common;
using Microsoft.Azure.Devices.Common.Security;
using Microsoft.ServiceBus.Messaging;


namespace AzureIoTHubConnectedService
{
    public partial class WizardMain : ICommand
    {
        /*--------------------------------------------------------------------------------------------------------------------
         * PUBLIC API
         *--------------------------------------------------------------------------------------------------------------------*/

        public async void CreateNewDevice(string deviceId)
        {
            IncrementBusyCounter();

            try
            {
                var device = await _RegistryManager.AddDeviceAsync(new Device(deviceId));
                //Microsoft.VisualStudio.Telemetry.TelemetryService.DefaultSession.PostEvent("vs/iothubcs/DeviceCreated");

                AddDevice(device);
            }
            catch (Exception /*ex*/)
            {
                //await context.Logger.WriteMessageAsync(LoggerMessageCategory.Warning, Resource.DeviceCreationFailure, deviceId);
                //Microsoft.VisualStudio.Telemetry.TelemetryService.DefaultSession.PostEvent("vs/iothubcs/FailureDeviceCreation");
            }

            DecrementBusyCounter();
        }

        /*--------------------------------------------------------------------------------------------------------------------
         * PUBLIC PROPERTIES
         *--------------------------------------------------------------------------------------------------------------------*/
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

        /// <summary>
        /// Is there any operation pending?
        /// </summary>
        public bool IsBusy
        {
            get { return (_IsBusy > 0); }
        }

        public bool UseTPM
        {
            get { return _UseTPM; }
            set
            {
                _UseTPM = value;
                // XXX
                //ConfigurePages();
            }
        }

        /// <summary>
        /// Observable collection of IoT Hubs
        /// </summary>
        public ObservableCollection<IAzureIoTHub> Hubs
        {
            get { return _Hubs; }
            set { _Hubs = value; OnPropertyChanged("Hubs"); }
        }


        //--------------------------------------------------------------------------------------------------------------------
        // PROPERTIES BELOW ARE RELATED TO CURRENTLY SELECTED HUB
        //--------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Currently selected IoT Hub
        /// </summary>
        public IAzureIoTHub SelectedHub
        {
            get
            {
                return _SelectedHub;
            }
            set
            {
                _SelectedHub = value;
                HandleHubSelected();
            }
        }

        /// <summary>
        /// Subscription name of selected Iot hub.
        /// Empty string if no hub selected.
        /// </summary>
        public string Subscription
        {
            get { return (null != _SelectedHub) ? _SelectedHub.Properties["SubscriptionName"] : ""; }
        }

        /// <summary>
        /// Selected IoT hub name.
        /// Empty string if no hub selected.
        /// </summary>
        public string IoTHubName
        {
            get { return (null != _SelectedHub) ? _SelectedHub.Properties["IoTHubName"] : ""; }
        }

        /// <summary>
        /// Selected IoT hub connection string (primary).
        /// Empty string if no hub selected.
        /// </summary>
        public string IoTHubConnectionString
        {
            get { return m_SelectedHubConnectionString; }
            set
            {
                m_SelectedHubConnectionString = value;

                if (m_SelectedHubConnectionString != "")
                {
                    PopulateDevices();
                }
            }
        }

        /// <summary>
        /// Selected IoT hub host name.
        /// Empty string if no hub selected.
        /// </summary>
        public string SelectedHubHost
        {
            get
            {
                string[] separators = { "HostName=" };
                string[] temp = m_SelectedHubConnectionString.Split(separators, StringSplitOptions.None);
                if (temp.Length == 2)
                {
                    return temp[1].Split(';')[0];
                }

                return "";
            }
        }

        /// <summary>
        /// Observable collection of devices registered on currently selected IoT Hub.
        /// Null if no hub selected.
        /// </summary>
        public ObservableCollection<Device> Devices
        {
            get { return _Devices; }
            set { _Devices = value; OnPropertyChanged("Devices"); }
        }

        //--------------------------------------------------------------------------------------------------------------------
        // PROPERTIES BELOW ARE RELATED TO CURRENTLY SELECTED DEVICE
        //--------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Currently selected device.
        /// </summary>
        public Device SelectedDevice
        {
            get
            {
                return _SelectedDevice;
            }
            set
            {
                _SelectedDevice = value;

                if (_SelectedDevice != null)
                {
                    // XXX - clean up m_SelectedDevice
                    m_SelectedDevice = new SelectedDevice { Id = value.Id, Key = value.Authentication.SymmetricKey.PrimaryKey };

                    m_SelectedDevicePrimaryConnectionString = string.Format(CultureInfo.InvariantCulture,
                        "HostName={0};DeviceId={1};SharedAccessKey={2}",
                        SelectedHubHost, value.Id, value.Authentication.SymmetricKey.PrimaryKey);

                    m_SelectedDeviceSecondaryConnectionString = string.Format(CultureInfo.InvariantCulture,
                        "HostName={0};DeviceId={1};SharedAccessKey={2}",
                        SelectedHubHost, value.Id, value.Authentication.SymmetricKey.SecondaryKey);


                    GetDeviceTwinData();
                }
                else
                {
                    m_SelectedDevice = null;
                    m_SelectedDevicePrimaryConnectionString = "";
                    m_SelectedDeviceSecondaryConnectionString = "";
                }

                OnPropertyChanged("Subscription");
                OnPropertyChanged("IoTHubName");
                OnPropertyChanged("IoTHubConnectionString");
                OnPropertyChanged("DeviceId");
                OnPropertyChanged("DevicePrimaryConnectionString");
                OnPropertyChanged("DeviceSecondaryConnectionString");

                OnPropertyChanged("SelectedDevice");

                InvokeCanExecuteChanged();;

                // XXX - fix this
                //ConfigurePages();
            }
        }

        /// <summary>
        /// Currently selected device id
        /// </summary>
        public string DeviceId
        {
            get { return (null != m_SelectedDevice) ? m_SelectedDevice.Id : ""; }
        }

        /// <summary>
        /// Currently selected device's connection string (primary).
        /// </summary>
        public string DevicePrimaryConnectionString
        {
            get { return m_SelectedDevicePrimaryConnectionString; }
        }

        /// <summary>
        /// Currently selected device's connection string (secondary).
        /// </summary>
        public string DeviceSecondaryConnectionString
        {
            get { return m_SelectedDeviceSecondaryConnectionString; }
        }

        //--------------------------------------------------------------------------------------------------------------------
        // DEVICE TWIN RELATED PROPERTIES
        //--------------------------------------------------------------------------------------------------------------------

        public string DeviceTwin
        {
            set { _DeviceTwin = value; OnPropertyChanged("DeviceTwin"); }
            get { return _DeviceTwin; }
        }

        public string DeviceTwinTags
        {
            set { _DeviceTwinTags = value; OnPropertyChanged("DeviceTwinTags"); }
            get { return _DeviceTwinTags; }
        }

        public string DeviceTwinReportedProperties
        {
            set { _DeviceTwinReportedProperties = value; OnPropertyChanged("DeviceTwinReportedProperties"); }
            get { return _DeviceTwinReportedProperties; }
        }

        public string DeviceTwinDesiredProperties
        {
            set { _DeviceTwinDesiredProperties = value; OnPropertyChanged("DeviceTwinDesiredProperties"); }
            get { return _DeviceTwinDesiredProperties; }
        }

        public string DeviceTwinUpdate
        {
            set
            {
                _DeviceTwinUpdate = value;
                OnPropertyChanged("DeviceTwinUpdate");
                InvokeCanExecuteChanged(); ;
            }
            get { return _DeviceTwinUpdate; }
        }


        //--------------------------------------------------------------------------------------------------------------------
        // DEVICE METHOD RELATED PROPERTIES
        //--------------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------

        public ICommand ExecuteCmd
        {
            get
            {
                return this;
            }
        }

        public string ReceiveMsgOutput
        {
            set { _ReceiveMsgOutput = value; OnPropertyChanged("ReceiveMsgOutput"); }
            get { return _ReceiveMsgOutput; }
        }

        public string DeviceMethodName
        {
            set
            {
                _DeviceMethodName = value;
                OnPropertyChanged("DeviceMethodName");
                InvokeCanExecuteChanged();;
            }
            get { return _DeviceMethodName; }
        }

        public string DeviceMethodPayload
        {
            set { _DeviceMethodPayload = value; OnPropertyChanged("DeviceMethodPayload"); }
            get { return _DeviceMethodPayload; }
        }

        public string DeviceMethodReturnStatus
        {
            set { _DeviceMethodReturnStatus = value; OnPropertyChanged("DeviceMethodReturnStatus"); }
            get { return _DeviceMethodReturnStatus; }
        }
        public string DeviceMethodReturnPayload
        {
            set { _DeviceMethodReturnPayload = value; OnPropertyChanged("DeviceMethodReturnPayload"); }
            get { return _DeviceMethodReturnPayload; }
        }

        public string CloudToDeviceContent
        {
            set
            {
                _CloudToDeviceContent = value;
                OnPropertyChanged("CloudToDeviceContent");
                InvokeCanExecuteChanged();;
            }
            get { return _CloudToDeviceContent; }
        }

        public string ErrorMessage
        {
            set { _ErrorMessage = value; OnPropertyChanged("ErrorMessage"); }
            get { return _ErrorMessage; }
        }


        /*--------------------------------------------------------------------------------------------------------------------
         * INTERNAL IMPLEMENTATION
         *--------------------------------------------------------------------------------------------------------------------*/

        private async void PopulateDevices()
        {
            Devices = null;

            IncrementBusyCounter();

            try
            {
                _RegistryManager = RegistryManager.CreateFromConnectionString(m_SelectedHubConnectionString);
                var devicesTask = _RegistryManager.GetDevicesAsync(1000);

                Devices = new ObservableCollection<Device>(await devicesTask);
            }
            catch (Exception) { }

            DecrementBusyCounter();
        }

        public void AddHub(IAzureIoTHub hub)
        {
            Hubs.Add(hub);
            // XXXX
            //_PageHubSelection.ClearCreate();
        }

        public void AddDevice(Device device)
        {
            Devices.Add(device);
            SelectedDevice = device;
        }

        protected void IncrementBusyCounter()
        {
            if (1 == ++_IsBusy) OnPropertyChanged("IsBusy");
        }

        protected void DecrementBusyCounter()
        {
            if (0 == --_IsBusy) OnPropertyChanged("IsBusy");
        }

        public bool CanExecute(object parameter)
        {
            string p = parameter as string;

            if (this.SelectedDevice == null)
                return false;

            if (p == "ReceiveMsgStart")
            {
                return !_IsMonitoring;
            }
            else if (p == "ReceiveMsgEnd")
            {
                return _IsMonitoring;
            }
            else if (p == "ReceiveMsgClear")
            {
                return true;
            }
            else if (p == "CloudToDeviceSend")
            {
                return (CloudToDeviceContent != "") && !_CloudToDeviceSending;
            }
            else if (p == "DeviceTwinUpdateDesired")
            {
                return (DeviceTwinUpdate != "") && !_DeviceTwinUpdating;
            }
            else if (p == "DeviceTwinRefresh")
            {
                return !_DeviceTwinUpdating;
            }
            else if (p == "DeviceMethodExecute")
            {
                return (DeviceMethodName != "") && !_DeviceMethodExecuting;
            }
            else
            {
                return false;
            }
        }

        public void Execute(object parameter)
        {
            InvokeCanExecuteChanged();;
            string p = parameter as string;

            if (p == "ReceiveMsgStart")
            {
                _MonitorCancellationTokenSource = new CancellationTokenSource();
                MonitorEventHubAsync(_MonitorCancellationTokenSource.Token, "$Default");
            }
            else if (p == "ReceiveMsgEnd")
            {
                if (_MonitorCancellationTokenSource != null)
                {
                    _MonitorCancellationTokenSource.Cancel();
                    _MonitorCancellationTokenSource.Dispose();
                    _MonitorCancellationTokenSource = null;
                }
            }
            else if (p == "ReceiveMsgClear")
            {
                this.ReceiveMsgOutput = "";
            }
            else if (p == "DeviceTwinUpdateDesired")
            {
                DeviceTwinUpdateDesired();
            }
            else if (p == "DeviceTwinRefresh")
            {
                GetDeviceTwinData();
            }
            else if (p == "CloudToDeviceSend")
            {
                CloudToDeviceSend();
            }
            else if (p == "DeviceMethodExecute")
            {
                DeviceMethodExecute();
            }

        }

        public event EventHandler CanExecuteChanged;


        private async void MonitorEventHubAsync(CancellationToken ct, string consumerGroupName)
        {
            _IsMonitoring = true;
            InvokeCanExecuteChanged();;

            EventHubClient eventHubClient = null;
            EventHubReceiver eventHubReceiver = null;

            try
            {
                string selectedDevice = SelectedDevice.Id;
                eventHubClient = EventHubClient.CreateFromConnectionString(m_SelectedHubConnectionString, "messages/events");
                ReceiveMsgOutput = "Receiving events...\r\n";
                int eventHubPartitionsCount = eventHubClient.GetRuntimeInformation().PartitionCount;
                string partition = EventHubPartitionKeyResolver.ResolveToPartition(selectedDevice, eventHubPartitionsCount);
                eventHubReceiver = eventHubClient.GetConsumerGroup(consumerGroupName).CreateReceiver(partition);

                //receive the events from startTime until current time in a single call and process them
                var events = await eventHubReceiver.ReceiveAsync(int.MaxValue, TimeSpan.FromSeconds(20));

                foreach (var eventData in events)
                {
                    var data = Encoding.UTF8.GetString(eventData.GetBytes());
                    var enqueuedTime = eventData.EnqueuedTimeUtc.ToLocalTime();
                    var connectionDeviceId = eventData.SystemProperties["iothub-connection-device-id"].ToString();

                    if (string.CompareOrdinal(selectedDevice.ToUpper(), connectionDeviceId.ToUpper()) == 0)
                    {
                        ReceiveMsgOutput += $"{enqueuedTime}> Device: [{connectionDeviceId}], Data:[{data}]";

                        if (eventData.Properties.Count > 0)
                        {
                            ReceiveMsgOutput += "Properties:\r\n";
                            foreach (var property in eventData.Properties)
                            {
                                ReceiveMsgOutput += $"'{property.Key}': '{property.Value}'\r\n";
                            }
                        }
                        ReceiveMsgOutput += "\r\n";

                    }
                }

                //having already received past events, monitor current events in a loop
                while (true)
                {
                    ct.ThrowIfCancellationRequested();

                    var eventData = await eventHubReceiver.ReceiveAsync(TimeSpan.FromSeconds(1));

                    if (eventData != null)
                    {
                        var data = Encoding.UTF8.GetString(eventData.GetBytes());
                        var enqueuedTime = eventData.EnqueuedTimeUtc.ToLocalTime();

                        // Display only data from the selected device; otherwise, skip.
                        var connectionDeviceId = eventData.SystemProperties["iothub-connection-device-id"].ToString();

                        if (string.CompareOrdinal(selectedDevice, connectionDeviceId) == 0)
                        {
                            ReceiveMsgOutput += $"{enqueuedTime}> Device: [{connectionDeviceId}], Data:[{data}]";

                            if (eventData.Properties.Count > 0)
                            {
                                ReceiveMsgOutput += "Properties:\r\n";
                                foreach (var property in eventData.Properties)
                                {
                                    ReceiveMsgOutput += $"'{property.Key}': '{property.Value}'\r\n";
                                }
                            }

                            ReceiveMsgOutput += "\r\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                {
                    ReceiveMsgOutput += $"Stopped Monitoring events. {ex.Message}\r\n";
                }
                else
                {
                    ErrorMessage = ex.Message;
                }
            }

            _IsMonitoring = false;
            InvokeCanExecuteChanged();;
        }

        public async void GetDeviceTwinData()
        {
            _DeviceTwinUpdating = true;
            InvokeCanExecuteChanged();;

            try
            {
                dynamic registryManager = RegistryManager.CreateFromConnectionString(IoTHubConnectionString);
                var deviceTwin = await registryManager.GetTwinAsync(SelectedDevice.Id);
                DeviceTwin = deviceTwin.ToJson();
                DeviceTwinTags = deviceTwin.Tags.ToJson();
                DeviceTwinReportedProperties = deviceTwin.Properties.Reported.ToJson();
                DeviceTwinDesiredProperties = deviceTwin.Properties.Desired.ToJson();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            _DeviceTwinUpdating = false;
            InvokeCanExecuteChanged();;
        }

        public async void DeviceTwinUpdateDesired()
        {
            _DeviceTwinUpdating = true;
            InvokeCanExecuteChanged();;

            try
            {
                dynamic registryManager = RegistryManager.CreateFromConnectionString(IoTHubConnectionString);
                Microsoft.Azure.Devices.Shared.Twin original = await registryManager.GetTwinAsync(SelectedDevice.Id);
                await registryManager.UpdateTwinAsync(original.DeviceId, DeviceTwinUpdate, original.ETag);
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            _DeviceTwinUpdating = false;
            InvokeCanExecuteChanged();;
        }

        public async void DeviceMethodExecute()
        {
            _DeviceMethodExecuting = true;
            InvokeCanExecuteChanged();;
            double timeout = 60;

            try
            {
                dynamic serviceClient = ServiceClient.CreateFromConnectionString(IoTHubConnectionString);

                Microsoft.Azure.Devices.CloudToDeviceMethod method = new Microsoft.Azure.Devices.CloudToDeviceMethod(DeviceMethodName, TimeSpan.FromSeconds(timeout));

                method = method.SetPayloadJson(DeviceMethodPayload);

                dynamic result = await serviceClient.InvokeDeviceMethodAsync(SelectedDevice.Id, method, new CancellationToken());

                DeviceMethodReturnStatus = result.Status;
                DeviceMethodReturnPayload = result.GetPayloadAsJson();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            _DeviceMethodExecuting = false;
            InvokeCanExecuteChanged();;
        }

        async void CloudToDeviceSend()
        {
            _CloudToDeviceSending = true;
            InvokeCanExecuteChanged();;

            try
            {
                ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(IoTHubConnectionString);

                var serviceMessage = new Microsoft.Azure.Devices.Message(Encoding.ASCII.GetBytes(CloudToDeviceContent));
                serviceMessage.Ack = DeliveryAcknowledgement.Full;
                serviceMessage.MessageId = Guid.NewGuid().ToString();

                await serviceClient.SendAsync(DeviceId, serviceMessage);

                await serviceClient.CloseAsync();

            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            _CloudToDeviceSending = false;
            InvokeCanExecuteChanged();;
        }

        private void InvokeCanExecuteChanged()
        {
            if (null != CanExecuteChanged)
            {
                CanExecuteChanged.Invoke(this, null);
            }
        }

        /*--------------------------------------------------------------------------------------------------------------------
         * INTERNAL DATA
         *--------------------------------------------------------------------------------------------------------------------*/

        private string m_SelectedHubConnectionString = "";
        private string m_SelectedDevicePrimaryConnectionString = "";
        private string m_SelectedDeviceSecondaryConnectionString = "";
        private SelectedDevice m_SelectedDevice = null;
        private bool _UseTPM = false;
        private bool _CanUseTPM = false;
        private int _IsBusy = 0;
        private string _ErrorMessage = "";
        private ObservableCollection<IAzureIoTHub> _Hubs = null;
        private ObservableCollection<Device> _Devices = null;
        private Device _SelectedDevice = null;
        private IAzureIoTHub _SelectedHub = null;

        private RegistryManager _RegistryManager = null;

        private string _ReceiveMsgOutput = "";

        private string _DeviceTwin = "";
        private string _DeviceTwinTags = "";
        private string _DeviceTwinReportedProperties = "";
        private string _DeviceTwinDesiredProperties = "";
        private string _DeviceTwinUpdate = "{ 'properties': { 'reported': { 'param': 'value' }}}";


        private string _DeviceMethodName = "SampleMethod";
        private string _DeviceMethodPayload = "";
        private string _DeviceMethodReturnStatus = "";
        private string _DeviceMethodReturnPayload = "";
        private string _CloudToDeviceContent = "";

        private bool _IsMonitoring = false;
        private CancellationTokenSource _MonitorCancellationTokenSource = null;

        private bool _DeviceMethodExecuting = false;
        private bool _CloudToDeviceSending = false;
        private bool _DeviceTwinUpdating = false;
    }
}
