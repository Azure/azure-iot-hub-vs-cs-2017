using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using System.Windows;

using Microsoft.Azure.Devices;

namespace AzureIoTHubConnectedService
{
    public class WizardPageDeviceSelection : ConnectedServiceWizardPage
    {
        internal WizardPageDeviceSelection(WizardMain wizard)
        {
            this.Title = "Azure IoT Hub";
            this.Legend = "Device";
            this.Description = "Select or Create Device";
            this.View = new WizardPageDeviceSelectionView(this);
            this.IsEnabled = false;

            this.MainModel = wizard;
        }

        public void CreateNewDevice(string device)
        {
            Create_FieldsEnabled = false;
            MainModel.CreateNewDevice(device);
        }

        public void ClearCreate(bool switchTab)
        {
            Create_FieldsEnabled = true;
            Create_DeviceName = "";

            if (switchTab)
            {
                (this.View as WizardPageDeviceSelectionView).Tabs.SelectedIndex = 0;
            }
        }

        public bool Create_IsEnabled
        {
            get
            {
                return _Create_IsEnabled;
            }
            set
            {
                _Create_IsEnabled = value;
                OnPropertyChanged("Create_IsEnabled");
            }
        }

        public bool Create_FieldsEnabled
        {
            get
            {
                return _Create_FieldsEnabled;
            }
            set
            {
                _Create_FieldsEnabled = value;
                OnPropertyChanged("Create_FieldsEnabled");
                Create_Validate();
            }
        }

        public string Create_DeviceName
        {
            get
            {
                return _Create_DeviceName;
            }
            set
            {
                _Create_DeviceName = value;
                Create_Validate();
                OnPropertyChanged("Create_DeviceName");
            }
        }

        private void Create_Validate()
        {
            // matching all alphanumeric characters + additional characters as defined in Azure IoT Hub documentation:
            // https://docs.microsoft.com/en-us/rest/api/iothub/deviceapi
            // A case-sensitive string (up to 128 char long) of ASCII 7-bit alphanumeric chars + {'-', ':', '.', '+', '%', '_', '#', '*', '?', '!', '(', ')', ',', '=', '@', ';', '$', '''}.
            bool match = System.Text.RegularExpressions.Regex.IsMatch(_Create_DeviceName, @"^[a-zA-Z0-9_\-\:\.\+\%\#\*\?\!\(\)\,\=\@\;\$]+$");

            Create_IsEnabled = (_Create_DeviceName != "" &&
                                match &&
                                _Create_FieldsEnabled);
        }

        private bool _Create_IsEnabled = false;
        private bool _Create_FieldsEnabled = true;
        private string _Create_DeviceName = "";

        public WizardMain MainModel { get; set; }
    }
}
