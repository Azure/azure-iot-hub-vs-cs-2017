﻿using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common;
using Microsoft.ServiceBus.Messaging;


namespace AzureIoTHubConnectedService
{
    public partial class WizardMain
    {
        /// <summary>
        /// Check if command can be executed
        /// </summary>
        /// <param name="parameter"></param>
        partial void CanExecuteExtras(object parameter)
        {
            string p = parameter as string;

            if (this.SelectedDevice == null)
                _CanExecute = false;

            if (p == "ReceiveMsgStart")
            {
                _CanExecute = !_IsMonitoring;
            }
            else if (p == "ReceiveMsgEnd")
            {
                _CanExecute = _IsMonitoring;
            }
            else if (p == "ReceiveMsgClear")
            {
                _CanExecute = true;
            }
            else if (p == "CloudToDeviceSend")
            {
                _CanExecute = (CloudToDeviceContent != "") && !_CloudToDeviceSending;
            }
            else if (p == "DeviceTwinUpdateDesired")
            {
                _CanExecute = (DeviceTwinUpdate != "") && !_DeviceTwinUpdating;
            }
            else if (p == "DeviceTwinRefresh")
            {
                _CanExecute = !_DeviceTwinUpdating;
            }
            else if (p == "DeviceMethodExecute")
            {
                _CanExecute = (DeviceMethodName != "") && !_DeviceMethodExecuting;
            }
            else
            {
                _CanExecute = false;
            }
        }

        /// <summary>
        /// Execute command
        /// </summary>
        /// <param name="parameter"></param>
        partial void ExecuteExtras(object parameter)
        {
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

        //--------------------------------------------------------------------------------------------------------------------
        // DEVICE METHOD EXECUTION RELATED CODE
        //--------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Device method name to be executed by DeviceMethodExecute
        /// </summary>
        public string DeviceMethodName
        {
            set
            {
                _DeviceMethodName = value;
                OnPropertyChanged("DeviceMethodName");
                InvokeCanExecuteChanged(); ;
            }
            get { return _DeviceMethodName; }
        }

        /// <summary>
        /// Device method payload to be used by DeviceMethodExecute
        /// </summary>
        public string DeviceMethodPayload
        {
            set { _DeviceMethodPayload = value; OnPropertyChanged("DeviceMethodPayload"); }
            get { return _DeviceMethodPayload; }
        }

        /// <summary>
        /// Starus received by DeviceMethodExecute command.
        /// </summary>
        public string DeviceMethodReturnStatus
        {
            set { _DeviceMethodReturnStatus = value; OnPropertyChanged("DeviceMethodReturnStatus"); }
            get { return _DeviceMethodReturnStatus; }
        }

        /// <summary>
        /// Payload received by DeviceMethodExecuteCommand.
        /// </summary>
        public string DeviceMethodReturnPayload
        {
            set { _DeviceMethodReturnPayload = value; OnPropertyChanged("DeviceMethodReturnPayload"); }
            get { return _DeviceMethodReturnPayload; }
        }

        /// <summary>
        /// Execute device method on currently selected device.
        /// </summary>
        public async void DeviceMethodExecute()
        {
            _DeviceMethodExecuting = true;
            InvokeCanExecuteChanged(); ;
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
            InvokeCanExecuteChanged(); ;
        }

        private string _DeviceMethodName = "SampleMethod";
        private string _DeviceMethodPayload = "";
        private string _DeviceMethodReturnStatus = "";
        private string _DeviceMethodReturnPayload = "";
        private bool _DeviceMethodExecuting = false;

        //--------------------------------------------------------------------------------------------------------------------
        // CLOUD TO DEVICE MESSAGE RELATED CODE
        //--------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// C2D message content to be used by CloudToDeviceSend command.
        /// </summary>
        public string CloudToDeviceContent
        {
            set
            {
                _CloudToDeviceContent = value;
                OnPropertyChanged("CloudToDeviceContent");
                InvokeCanExecuteChanged(); ;
            }
            get { return _CloudToDeviceContent; }
        }

        /// <summary>
        /// Sends cloud to device message to currently selected device.
        /// </summary>
        async void CloudToDeviceSend()
        {
            _CloudToDeviceSending = true;
            InvokeCanExecuteChanged(); ;

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
            InvokeCanExecuteChanged(); ;
        }

        private string _CloudToDeviceContent = "";
        private bool _CloudToDeviceSending = false;

        //--------------------------------------------------------------------------------------------------------------------
        // DEVICE TO CLOUD MESSAGE MONITORING RELATED CODE
        //--------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Buffer for received D2C messages. 
        /// </summary>
        public string ReceiveMsgOutput
        {
            set { _ReceiveMsgOutput = value; OnPropertyChanged("ReceiveMsgOutput"); }
            get { return _ReceiveMsgOutput; }
        }

        /// <summary>
        /// Monitor D2C messages for selected device
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="consumerGroupName"></param>
        private async void MonitorEventHubAsync(CancellationToken ct, string consumerGroupName)
        {
            _IsMonitoring = true;
            InvokeCanExecuteChanged(); ;

            EventHubClient eventHubClient = null;
            EventHubReceiver eventHubReceiver = null;

            try
            {
                string selectedDevice = SelectedDevice.Id;
                eventHubClient = EventHubClient.CreateFromConnectionString(_SelectedHubConnectionString, "messages/events");
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
            InvokeCanExecuteChanged(); ;
        }

        private string _ReceiveMsgOutput = "";
        private bool _IsMonitoring = false;
        private CancellationTokenSource _MonitorCancellationTokenSource = null;

        //--------------------------------------------------------------------------------------------------------------------
        // DEVICE TWIN RELATED CODE
        //--------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Value to be used by DeviceTwinUpdateDesired
        /// </summary>
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

        /// <summary>
        /// Entire device twin in JSON format
        /// </summary>
        public string DeviceTwin
        {
            set { _DeviceTwin = value; OnPropertyChanged("DeviceTwin"); }
            get { return _DeviceTwin; }
        }

        /// <summary>
        /// Tags subtree from device twin in JSON format.
        /// </summary>
        public string DeviceTwinTags
        {
            set { _DeviceTwinTags = value; OnPropertyChanged("DeviceTwinTags"); }
            get { return _DeviceTwinTags; }
        }

        /// <summary>
        /// Reported properties section of device twin in JSON format.
        /// </summary>
        public string DeviceTwinReportedProperties
        {
            set { _DeviceTwinReportedProperties = value; OnPropertyChanged("DeviceTwinReportedProperties"); }
            get { return _DeviceTwinReportedProperties; }
        }

        /// <summary>
        /// Desired properties section of device twin in JSON format.
        /// </summary>
        public string DeviceTwinDesiredProperties
        {
            set { _DeviceTwinDesiredProperties = value; OnPropertyChanged("DeviceTwinDesiredProperties"); }
            get { return _DeviceTwinDesiredProperties; }
        }

        /// <summary>
        /// Retrieve device twin information for selected device.
        /// </summary>
        public async void GetDeviceTwinData()
        {
            _DeviceTwinUpdating = true;
            InvokeCanExecuteChanged(); ;

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
            InvokeCanExecuteChanged(); ;
        }

        /// <summary>
        /// Update device twin desired properties for selected device
        /// </summary>
        public async void DeviceTwinUpdateDesired()
        {
            _DeviceTwinUpdating = true;
            InvokeCanExecuteChanged(); ;

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
            InvokeCanExecuteChanged(); ;
        }

        private bool _DeviceTwinUpdating = false;
        private string _DeviceTwin = "";
        private string _DeviceTwinTags = "";
        private string _DeviceTwinReportedProperties = "";
        private string _DeviceTwinDesiredProperties = "";
        private string _DeviceTwinUpdate = "{ 'properties': { 'reported': { 'param': 'value' }}}";
    }
}
