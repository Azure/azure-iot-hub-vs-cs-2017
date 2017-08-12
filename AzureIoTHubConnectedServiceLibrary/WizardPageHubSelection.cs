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
            this.View.DataContext = wizard;
        }

        public void HubSelected()
        {
            (this.View as WizardPageHubSelectionView).HubSelected();
        }
    }
}
