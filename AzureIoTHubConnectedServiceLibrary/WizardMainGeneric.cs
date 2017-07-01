using System;

using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Azure.Devices;

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
        public string CurrentHub_Name
        {
            get { return (null != _SelectedHub) ? _SelectedHub.Properties["IoTHubName"] : ""; }
        }

        /// <summary>
        /// Selected IoT hub connection string (primary).
        /// Empty string if no hub selected.
        /// </summary>
        public string CurrentHub_ConnectionString
        {
            get { return _SelectedHubConnectionString; }
            set
            {
                _SelectedHubConnectionString = value;

                if (_SelectedHubConnectionString != "")
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
                string[] temp = _SelectedHubConnectionString.Split(separators, StringSplitOptions.None);
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
                    _SelectedDevicePrimaryConnectionString = string.Format(CultureInfo.InvariantCulture,
                        "HostName={0};DeviceId={1};SharedAccessKey={2}",
                        CurrentHub_Host, value.Id, value.Authentication.SymmetricKey.PrimaryKey);

                    _SelectedDeviceSecondaryConnectionString = string.Format(CultureInfo.InvariantCulture,
                        "HostName={0};DeviceId={1};SharedAccessKey={2}",
                        CurrentHub_Host, value.Id, value.Authentication.SymmetricKey.SecondaryKey);


                    // [ZKK] no need to get it here in current version of the extension
                    // GetDeviceTwinData();
                }
                else
                {
                    _SelectedDevicePrimaryConnectionString = "";
                    _SelectedDeviceSecondaryConnectionString = "";
                }

                OnPropertyChanged("Subscription");
                OnPropertyChanged("CurrentHub_Name");
                OnPropertyChanged("CurrentHub_ConnectionString");
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
            get { return (null != _SelectedDevice) ? _SelectedDevice.Id : ""; }
        }

        /// <summary>
        /// Currently selected device's connection string (primary).
        /// </summary>
        public string DevicePrimaryConnectionString
        {
            get { return _SelectedDevicePrimaryConnectionString; }
        }

        /// <summary>
        /// Currently selected device's connection string (secondary).
        /// </summary>
        public string DeviceSecondaryConnectionString
        {
            get { return _SelectedDeviceSecondaryConnectionString; }
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
            }
        }

        /// <summary>
        /// List of device twin properties
        /// </summary>
        public ObservableCollection<DeviceTwinProperty> DeviceTwinProperties { get { return _DeviceTwinProperties; } }

        private bool _DeviceTwinEnabled = false;
        private ObservableCollection<DeviceTwinProperty> _DeviceTwinProperties = new ObservableCollection<DeviceTwinProperty>();

        //--------------------------------------------------------------------------------------------------------------------
        // DIRECT METHOD RELATED PROPERTIES
        //--------------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Is direct method functionality enabled by the user?
        /// </summary>
        public bool DeviceMethodEnabled
        {
            get
            {
                return _DeviceMethodEnabled;
            }
            set
            {
                _DeviceMethodEnabled = value;
            }
        }

        /// <summary>
        /// List of device methods
        /// </summary>
        public ObservableCollection<DeviceMethodDescription> DeviceMethods { get { return _DeviceMethods; } }

        private bool _DeviceMethodEnabled = false;
        private ObservableCollection<DeviceMethodDescription> _DeviceMethods = new ObservableCollection<DeviceMethodDescription>();

        //--------------------------------------------------------------------------------------------------------------------
        // NEW IOT HUB CREATION RELATED CODE
        //--------------------------------------------------------------------------------------------------------------------

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

        public bool NewHub_FieldsEnabled
        {
            get
            {
                return _NewHub_FieldsEnabled;
            }
            set
            {
                _NewHub_FieldsEnabled = value;
                OnPropertyChanged("Create_FieldsEnabled");
                NewHub_Validate();
            }
        }

        public string NewHub_SubscriptionName
        {
            get { return _NewHub_SubscriptionName; }
            set { _NewHub_SubscriptionName = value; NewHub_Validate(); OnPropertyChanged("NewHub_SubscriptionName"); QueryResourceGroups(_NewHub_SubscriptionName); }
        }

        public string NewHub_Name
        {
            get { return _NewHub_Name; }
            set { _NewHub_Name = value; NewHub_Validate(); OnPropertyChanged("NewHub_Name"); }
        }

        public string NewHub_ResourceGroupName
        {
            get { return _NewHub_ResourceGroupName; }
            set { _NewHub_ResourceGroupName = value; NewHub_Validate(); OnPropertyChanged("NewHub_ResourceGroupName"); }
        }

        internal void CreateNewHub()
        {
            NewHub_FieldsEnabled = false;

            CreateNewHub(NewHub_SubscriptionName, NewHub_ResourceGroupName, NewHub_Name);
        }

        private void AddHub(IAzureIoTHub hub)
        {
            Hubs.Add(hub);
            NewHub_FieldsEnabled = true;
            NewHub_Name = "";
            NewHub_ResourceGroupName = "";
        }

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
        private string _NewHub_Name = "";

        //--------------------------------------------------------------------------------------------------------------------
        // NEW DEVICE CREATION RELATED CODE
        //--------------------------------------------------------------------------------------------------------------------

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

        public async void CreateNewDevice()
        {
            NewDevice_FieldsEnabled = false;
            IncrementBusyCounter();

            try
            {
                var device = await _RegistryManager.AddDeviceAsync(new Device(NewDevice_Name));
                //Microsoft.VisualStudio.Telemetry.TelemetryService.DefaultSession.PostEvent("vs/iothubcs/DeviceCreated");

                AddDevice(device);
                NewDevice_Name = "";
            }
            catch (Exception ex)
            {
                DisplayMessage("Failed to create new device: " + ex.Message);
                //Microsoft.VisualStudio.Telemetry.TelemetryService.DefaultSession.PostEvent("vs/iothubcs/FailureDeviceCreation");
            }

            DecrementBusyCounter();
            NewDevice_FieldsEnabled = true;
        }

        private void AddDevice(Device device)
        {
            Devices.Add(device);
            SelectedDevice = device;
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

        private async void PopulateDevices()
        {
            Devices = null;

            IncrementBusyCounter();

            try
            {
                _RegistryManager = CommonFactory.CreateRegistryManagerFromConnectionString(_SelectedHubConnectionString);
                var devicesTask = _RegistryManager.GetDevicesAsync(1000);

                Devices = new ObservableCollection<Device>(await devicesTask);
            }
            catch (Exception ex)
            {
                DisplayMessage("Failed to query devices: " + ex.Message);
            }

            DecrementBusyCounter();
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

            if (p == "CreateNewDevice")
            {
                return NewDevice_CanCreate;
            }
            else if (p == "CreateNewHub")
            {
                return NewHub_CanCreate;
            }
            else
            {
                CanExecuteExtras(parameter);
                return _CanExecute;
            }
        }

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
            else
            {
                ExecuteExtras(parameter);
            }
        }

        bool _CanExecute = false;

        partial void CanExecuteExtras(object parameter);
        partial void ExecuteExtras(object parameter);

        public event EventHandler CanExecuteChanged;

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

        private string _SelectedHubConnectionString = "";
        private string _SelectedDevicePrimaryConnectionString = "";
        private string _SelectedDeviceSecondaryConnectionString = "";
        private int _IsBusy = 0;
        private string _ErrorMessage = "";
        private ObservableCollection<IAzureIoTHub> _Hubs = null;
        private ObservableCollection<Device> _Devices = null;
        private Device _SelectedDevice = null;
        private IAzureIoTHub _SelectedHub = null;

        private RegistryManager _RegistryManager = null;
    }
}
