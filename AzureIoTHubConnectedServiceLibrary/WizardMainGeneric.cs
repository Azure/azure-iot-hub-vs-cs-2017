using System;

using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Azure.Devices;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AzureIoTHubConnectedService
{
    public partial class WizardMain : ICommand
    {
        /*--------------------------------------------------------------------------------------------------------------------
         * PUBLIC PROPERTIES
         *--------------------------------------------------------------------------------------------------------------------*/

        /// <summary>
        /// Is there any operation pending?
        /// </summary>
        public bool IsBusy
        {
            get { return (_IsBusy > 0); }
        }

        //--------------------------------------------------------------------------------------------------------------------
        // IOT HUBS
        //--------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Observable collection of IoT Hubs
        /// </summary>
        public ObservableCollection<IAzureIoTHub> Hubs
        {
            get { return _Hubs; }
            set { _Hubs = value; OnPropertyChanged("Hubs"); }
        }

        //--------------------------------------------------------------------------------------------------------------------
        // RESOURCE GROUPS
        //--------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// List of available resource groups for currently selected subscription
        /// </summary>
        public ObservableCollection<ResourceGroup> ResourceGroups
        {
            get { return _ResourceGroups; }
            set { _ResourceGroups = value; OnPropertyChanged("ResourceGroups"); }
        }

        private ObservableCollection<ResourceGroup> _ResourceGroups = null;

        /// <summary>
        /// List of available locations for currently selected subscription
        /// </summary>
        public ObservableCollection<ResourceLocation> Locations
        {
            get { return _Locations; }
            set { _Locations = value; OnPropertyChanged("Locations"); }
        }

        private ObservableCollection<ResourceLocation> _Locations = null;

        //--------------------------------------------------------------------------------------------------------------------
        // PROPERTIES BELOW ARE RELATED TO CURRENTLY SELECTED HUB
        //--------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Currently selected IoT Hub
        /// </summary>
        public IAzureIoTHub CurrentHub
        {
            get
            {
                return _CurrentHub;
            }
            set
            {
                _CurrentHub = value;

                OnPropertyChanged("CurrentHub");
                OnPropertyChanged("CurrentHub_Subscription");
                OnPropertyChanged("CurrentHub_Name");
                OnPropertyChanged("CurrentHub_HostName");
                OnPropertyChanged("CurrentHub_ConnectionString");
                HandleHubSelected();
            }
        }

        /// <summary>
        /// Subscription name of selected Iot hub.
        /// Empty string if no hub selected.
        /// </summary>
        public string CurrentHub_Subscription
        {
            get { return (null != _CurrentHub) ? _CurrentHub.Properties["SubscriptionName"] : ""; }
        }

        /// <summary>
        /// Selected IoT hub name.
        /// Empty string if no hub selected.
        /// </summary>
        public string CurrentHub_Name
        {
            get { return (null != _CurrentHub) ? _CurrentHub.Properties["IoTHubName"] : ""; }
        }

        /// <summary>
        /// Selected IoT hub host.
        /// Empty string if no hub selected.
        /// </summary>
        public string CurrentHub_HostName
        {

            get { return (null != _CurrentHub) ? _CurrentHub.Properties["iotHubUri"] : ""; }
        }

        /// <summary>
        /// Selected IoT hub connection string (primary).
        /// Empty string if no hub selected.
        /// </summary>
        public string CurrentHub_ConnectionString
        {
            get { return _CurrentHub_ConnectionString; }
            set
            {
                _CurrentHub_ConnectionString = value;

                if (_CurrentHub_ConnectionString != "")
                {
                    PopulateDevices();
                }
            }
        }

        /// <summary>
        /// Selected IoT hub host name.
        /// Empty string if no hub selected.
        /// </summary>
        public string CurrentHub_Host
        {
            get
            {
                string[] separators = { "HostName=" };
                string[] temp = _CurrentHub_ConnectionString.Split(separators, StringSplitOptions.None);
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
        public Device CurrentDevice
        {
            get
            {
                return _CurrentDevice;
            }
            set
            {
                _CurrentDevice = value;

                if (_CurrentDevice != null)
                {
                    _CurrentDevice_PrimaryConnectionString = string.Format(CultureInfo.InvariantCulture,
                        "HostName={0};DeviceId={1};SharedAccessKey={2}",
                        CurrentHub_Host, value.Id, value.Authentication.SymmetricKey.PrimaryKey);

                    _CurrentDevice_SecondaryConnectionString = string.Format(CultureInfo.InvariantCulture,
                        "HostName={0};DeviceId={1};SharedAccessKey={2}",
                        CurrentHub_Host, value.Id, value.Authentication.SymmetricKey.SecondaryKey);


                    // [ZKK] no need to get it here in current version of the extension
                    // GetDeviceTwinData();
                }
                else
                {
                    _CurrentDevice_PrimaryConnectionString = "";
                    _CurrentDevice_SecondaryConnectionString = "";
                }

                OnPropertyChanged("CurrentDevice_Id");
                OnPropertyChanged("CurrentDevice_PrimaryConnectionString");
                OnPropertyChanged("CurrentDevice_SecondaryConnectionString");
                OnPropertyChanged("CurrentDevice_PrimarySharedKey");
                OnPropertyChanged("CurrentDevice");

                InvokeCanExecuteChanged();

                HandleDeviceSelected();
            }
        }

        /// <summary>
        /// Currently selected device id
        /// </summary>
        public string CurrentDevice_Id
        {
            get { return (null != _CurrentDevice) ? _CurrentDevice.Id : ""; }
        }

        /// <summary>
        /// Currently selected device's connection string (primary).
        /// </summary>
        public string CurrentDevice_PrimaryConnectionString
        {
            get { return _CurrentDevice_PrimaryConnectionString; }
        }

        /// <summary>
        /// Currently selected device's connection string (secondary).
        /// </summary>
        public string CurrentDevice_SecondaryConnectionString
        {
            get { return _CurrentDevice_SecondaryConnectionString; }
        }

        public string CurrentDevice_PrimarySharedKey
        {
            get { return (null != _CurrentDevice) ? _CurrentDevice.Authentication.SymmetricKey.PrimaryKey : ""; }
        }

        //--------------------------------------------------------------------------------------------------------------------
        // DEVICE TWIN RELATED PROPERTIES
        //--------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Is device twin functionality enabled by the user?
        /// </summary>
        public bool DeviceTwinEnabled
        {
            get
            {
                return _DeviceTwinEnabled;
            }
            set
            {
                _DeviceTwinEnabled = value;
                OnPropertyChanged("DeviceTwinEnabled");
            }
        }

        /// <summary>
        /// List of device twin properties
        /// </summary>
        public ObservableCollection<DeviceTwinProperty> DeviceTwinProperties { get { return _DeviceTwinProperties; } }

        private bool _DeviceTwinEnabled = true;
        private ObservableCollection<DeviceTwinProperty> _DeviceTwinProperties = new ObservableCollection<DeviceTwinProperty>();

        //--------------------------------------------------------------------------------------------------------------------
        // DIRECT METHOD RELATED PROPERTIES
        //--------------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Is direct method functionality enabled by the user?
        /// </summary>
        public bool DirectMethodEnabled
        {
            get
            {
                return _DirectMethodEnabled;
            }
            set
            {
                _DirectMethodEnabled = value;
                OnPropertyChanged("DirectMethodEnabled");
            }
        }

        /// <summary>
        /// List of direct methods
        /// </summary>
        public ObservableCollection<DirectMethodDescription> DirectMethods { get { return _DirectMethods; } }

        private bool _DirectMethodEnabled = true;
        private ObservableCollection<DirectMethodDescription> _DirectMethods = new ObservableCollection<DirectMethodDescription>();

        //--------------------------------------------------------------------------------------------------------------------
        // NEW IOT HUB CREATION RELATED CODE
        //--------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Can new IoT Hub be created?
        /// </summary>
        public bool NewHub_CanCreate
        {
            get
            {
                return _NewHub_CanCreate;
            }
            set
            {
                _NewHub_CanCreate = value;
                OnPropertyChanged("NewHub_CanCreate");
                InvokeCanExecuteChanged();
            }
        }

        /// <summary>
        /// Are new IoT hub fields enabled?
        /// </summary>
        public bool NewHub_FieldsEnabled
        {
            get
            {
                return _NewHub_FieldsEnabled;
            }
            set
            {
                _NewHub_FieldsEnabled = value;
                OnPropertyChanged("NewHub_FieldsEnabled");
                NewHub_Validate();
            }
        }

        /// <summary>
        /// New IoT Hub subscription name.
        /// </summary>
        public string NewHub_SubscriptionName
        {
            get { return _NewHub_SubscriptionName; }
            set { _NewHub_SubscriptionName = value; NewHub_Validate(); OnPropertyChanged("NewHub_SubscriptionName"); QueryResourceGroups(_NewHub_SubscriptionName); }
        }

        /// <summary>
        /// New IoT Hub location.
        /// </summary>
        public string NewHub_Location
        {
            get { return _NewHub_Location; }
            set { _NewHub_Location = value; OnPropertyChanged("NewHub_Location"); }
        }

        /// <summary>
        /// New IoT Hub name.
        /// </summary>
        public string NewHub_Name
        {
            get { return _NewHub_Name; }
            set { _NewHub_Name = value; NewHub_Validate(); OnPropertyChanged("NewHub_Name"); }
        }

        /// <summary>
        /// New IoT Hub resource group name.
        /// </summary>
        public string NewHub_ResourceGroupName
        {
            get { return _NewHub_ResourceGroupName; }
            set { _NewHub_ResourceGroupName = value; NewHub_Validate(); OnPropertyChanged("NewHub_ResourceGroupName"); }
        }

        /// <summary>
        /// Creates new IoT Hub.
        /// </summary>
        internal void CreateNewHub()
        {
            CreateNewHub(NewHub_SubscriptionName, NewHub_ResourceGroupName, NewHub_Location, NewHub_Name);
        }

        /// <summary>
        /// Adds new hub to the list.
        /// Selects it by default.
        /// Clears fields.
        /// </summary>
        /// <param name="hub"></param>
        private void AddHub(IAzureIoTHub hub)
        {
            Hubs.Add(hub);
            NewHub_FieldsEnabled = true;
            NewHub_Name = "";
            NewHub_ResourceGroupName = "";
            NewHub_SubscriptionName = "";

            CurrentHub = hub;
        }

        /// <summary>
        /// Validate new hub fields
        /// </summary>
        private void NewHub_Validate()
        {
            bool match = true;

            // in case of synscription name, just verify that it's not empty 
            if (_NewHub_SubscriptionName == "")
                match = false;

            // verify iot hub name - it can contain only alphanymeric characters
            if (!System.Text.RegularExpressions.Regex.IsMatch(_NewHub_Name, @"^[a-zA-Z0-9]+$"))
                match = false;

            // verify resource group name -- it can contain alphanumeric characters
            // periods, underscores, hyphens, parenthesis.
            // Period cannot be last character
            if (!System.Text.RegularExpressions.Regex.IsMatch(_NewHub_ResourceGroupName, @"^[a-zA-Z0-9\._\-\(\)]*[a-zA-Z0-9_\-\(\)]$"))
                match = false;

            NewHub_CanCreate = match && _NewHub_FieldsEnabled;
        }

        private bool _NewHub_CanCreate = false;
        private bool _NewHub_FieldsEnabled = true;
        private string _NewHub_ResourceGroupName = "";
        private string _NewHub_SubscriptionName = "";
        private string _NewHub_Location = "";
        private string _NewHub_Name = "";

        //--------------------------------------------------------------------------------------------------------------------
        // NEW DEVICE CREATION RELATED CODE
        //--------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Can new device be created?
        /// </summary>
        public bool NewDevice_CanCreate
        {
            get
            {
                return _NewDevice_CanCreate;
            }
            set
            {
                _NewDevice_CanCreate = value;
                OnPropertyChanged("NewDevice_CanCreate");
                InvokeCanExecuteChanged();
            }
        }

        /// <summary>
        /// New device fields enabled indication, fields should be disabled while device is created.
        /// </summary>
        public bool NewDevice_FieldsEnabled
        {
            get
            {
                return _NewDevice_FieldsEnabled;
            }
            set
            {
                _NewDevice_FieldsEnabled = value;
                OnPropertyChanged("NewDevice_FieldsEnabled");
                NewDevice_Validate();
            }
        }

        /// <summary>
        /// New device name.
        /// </summary>
        public string NewDevice_Name
        {
            get
            {
                return _NewDevice_Name;
            }
            set
            {
                _NewDevice_Name = value;
                NewDevice_Validate();
                OnPropertyChanged("NewDevice_Name");
            }
        }

        /// <summary>
        /// Validated device name.
        /// </summary>
        private void NewDevice_Validate()
        {
            // matching all alphanumeric characters + additional characters as defined in Azure IoT Hub documentation:
            // https://docs.microsoft.com/en-us/rest/api/iothub/deviceapi
            // A case-sensitive string (up to 128 char long) of ASCII 7-bit alphanumeric chars + {'-', ':', '.', '+', '%', '_', '#', '*', '?', '!', '(', ')', ',', '=', '@', ';', '$', '''}.
            bool match = System.Text.RegularExpressions.Regex.IsMatch(_NewDevice_Name, @"^[a-zA-Z0-9_\-\:\.\+\%\#\*\?\!\(\)\,\=\@\;\$]+$");

            NewDevice_CanCreate = (_NewDevice_Name != "" &&
                                match &&
                                _NewDevice_FieldsEnabled);
        }

        /// <summary>
        /// Create new device in currently selected IoT Hub.
        /// </summary>
        public async void CreateNewDevice()
        {
            NewDevice_FieldsEnabled = false;
            IncrementBusyCounter();

            try
            {
                var device = await _RegistryManager.AddDeviceAsync(new Device(NewDevice_Name));
                Microsoft.VisualStudio.Telemetry.TelemetryService.DefaultSession.PostEvent("vs/iothubcs/DeviceCreated");

                AddDevice(device);
                NewDevice_Name = "";
            }
            catch (Exception ex)
            {
                DisplayMessage("Failed to create new device: " + ex.Message);
                Microsoft.VisualStudio.Telemetry.TelemetryService.DefaultSession.PostEvent("vs/iothubcs/FailureDeviceCreation");
            }

            DecrementBusyCounter();
            NewDevice_FieldsEnabled = true;
        }

        /// <summary>
        /// Adds device to the list.
        /// This function should be called after successful device creation in IoT Hub.
        /// Newly added device will be automatically selected.
        /// </summary>
        /// <param name="device"></param>
        private void AddDevice(Device device)
        {
            Devices.Add(device);
            CurrentDevice = device;
        }

        private bool _NewDevice_CanCreate = false;
        private bool _NewDevice_FieldsEnabled = true;
        private string _NewDevice_Name = "";

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

        public string ErrorMessage
        {
            set { _ErrorMessage = value; OnPropertyChanged("ErrorMessage"); }
            get { return _ErrorMessage; }
        }


        /*--------------------------------------------------------------------------------------------------------------------
         * INTERNAL IMPLEMENTATION
         *--------------------------------------------------------------------------------------------------------------------*/

        /// <summary>
        /// Populate devices task for currently selected IoT Hub
        /// </summary>
        private async void PopulateDevices()
        {
            Task<IEnumerable<Device>> devicesTask = null;
            lastDevicesTask = null;
            Devices = null;

            IncrementBusyCounter();

            try
            {
                if (_CurrentHub_ConnectionString != "")
                {
                    _RegistryManager = CommonFactory.CreateRegistryManagerFromConnectionString(_CurrentHub_ConnectionString);
                    devicesTask = _RegistryManager.GetDevicesAsync(1000);

                    lastDevicesTask = devicesTask;

                    var devices = await devicesTask;

                    if (lastDevicesTask == devicesTask)
                    {
                        Devices = new ObservableCollection<Device>(devices);
                        lastDevicesTask = null;
                    }
                }
            }
            catch (Exception ex)
            {
                if (devicesTask == lastDevicesTask)
                {
                    DisplayMessage("Failed to query devices: " + ex.Message);
                }
            }

            DecrementBusyCounter();
        }

        Task<IEnumerable<Device>> lastDevicesTask = null;

        /// <summary>
        /// Increment busy counter - used to display progress bar.
        /// </summary>
        protected void IncrementBusyCounter()
        {
            if (1 == ++_IsBusy) OnPropertyChanged("IsBusy");
        }

        /// <summary>
        /// Decrement busy counter.
        /// </summary>
        protected void DecrementBusyCounter()
        {
            if (0 == --_IsBusy) OnPropertyChanged("IsBusy");
        }

        /// <summary>
        /// Check if given command can be executed.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            string p = parameter as string;

            if (p == "CreateNewDevice")
            {
                return NewDevice_CanCreate;
            }
            else if (p == "CreateNewHub")
            {
                return NewHub_CanCreate;
            }
            else if (p == "ProvisionDevice")
            {
                return true;
            }
            else
            {
                CanExecuteExtras(parameter);
                return _CanExecute;
            }
        }

        /// <summary>
        /// Execute given command
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            InvokeCanExecuteChanged();;
            string p = parameter as string;

            if (p == "CreateNewDevice")
            {
                CreateNewDevice();
            }
            else if (p == "CreateNewHub")
            {
                CreateNewHub();
            }
            else if (p == "ProvisionDevice")
            {
                ProvisionDevice();
            }
            else
            {
                ExecuteExtras(parameter);
            }
        }

        bool _CanExecute = false;

        /// <summary>
        /// Check if any extra command can be executed.
        /// </summary>
        /// <param name="parameter"></param>
        partial void CanExecuteExtras(object parameter);

        /// <summary>
        /// Execute extra command.
        /// </summary>
        /// <param name="parameter"></param>
        partial void ExecuteExtras(object parameter);

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Notify clients that commands execution availability has changed.
        /// </summary>
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

        private string _CurrentHub_ConnectionString = "";
        private string _CurrentDevice_PrimaryConnectionString = "";
        private string _CurrentDevice_SecondaryConnectionString = "";
        private int _IsBusy = 0;
        private string _ErrorMessage = "";
        private ObservableCollection<IAzureIoTHub> _Hubs = null;
        private ObservableCollection<Device> _Devices = null;
        private Device _CurrentDevice = null;
        private IAzureIoTHub _CurrentHub = null;

        private RegistryManager _RegistryManager = null;
    }
}
