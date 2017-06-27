using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using System.Windows;

using Microsoft.VisualStudio.WindowsAzure.Authentication;


namespace AzureIoTHubConnectedService
{
    public class WizardPageHubSelection : ConnectedServiceWizardPage
    {
        internal WizardPageHubSelection(WizardMain wizard)
        {
            this.Title = Resource.WizardPageTitle;
            this.Legend = Resource.WizardPageHubSelectionLegend;
            this.Description = Resource.WizardPageHubSelectionDescription;
            this.View = new WizardPageHubSelectionView();
            this.View.DataContext = this;
            this.MainModel = wizard;
        }

        public void SelectHub(IAzureIoTHub hub)
        {
            (this.View as WizardPageHubSelectionView).SelectHub(hub);
        }

        public void ClearCreate()
        {
            Create_FieldsEnabled = true;
            Create_IoTHubName = "";
            Create_ResourceGroupName = "";
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

                OnPropertyChanged("Create_InProgress");
            }
        }

        public Visibility Create_InProgress
        {
            get
            {
                return (_Create_FieldsEnabled && (this.MainModel.Hubs != null)) ? Visibility.Hidden : Visibility.Visible;
            }
        }

        public IAzureRMSubscription Create_SubscriptionItem
        {
            get { return _Create_SubscriptionItem; }
            set { _Create_SubscriptionItem = value; Create_Validate(); OnPropertyChanged("Create_SubscriptionName"); MainModel.QueryResourceGroups(_Create_SubscriptionItem.SubscriptionName); }
        }

        public string Create_IoTHubName
        {
            get { return _Create_IoTHubName; }
            set { _Create_IoTHubName = value; Create_Validate(); OnPropertyChanged("Create_IoTHubName"); }
        }

        public string Create_ResourceGroupName
        {
            get { return _Create_ResourceGroupName; }
            set { _Create_ResourceGroupName = value; Create_Validate(); OnPropertyChanged("Create_ResourceGroupName");  }
        }

        internal void CreateNewHub()
        {
            Create_FieldsEnabled = false;

            OnPropertyChanged("Create_InProgress");

            MainModel.CreateNewHub(Create_SubscriptionItem.SubscriptionName, Create_ResourceGroupName, Create_IoTHubName);
        }

        private void Create_Validate()
        {
            Create_IsEnabled = (_Create_IoTHubName != "" && _Create_SubscriptionItem != null && _Create_ResourceGroupName != "" && _Create_FieldsEnabled);
        }

        private bool _Create_IsEnabled = false;
        private bool _Create_FieldsEnabled = true;
        private IAzureRMSubscription _Create_SubscriptionItem = null;
        private string _Create_IoTHubName = "";
        private string _Create_ResourceGroupName = "";

        public WizardMain MainModel { get; set; }
    }
}
