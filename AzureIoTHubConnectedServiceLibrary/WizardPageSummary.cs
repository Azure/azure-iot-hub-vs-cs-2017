using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;


namespace AzureIoTHubConnectedService
{
    public class WizardPageSummary : ConnectedServiceWizardPage
    {
        internal WizardPageSummary(WizardMain wizard)
        {
            this.Title = Resource.WizardPageTitle;
            this.Legend = Resource.WizardPageSummaryLegend;
            this.Description = Resource.WizardPageSummaryDescription;
            this.IsEnabled = false;

            this.View = new WizardPageSummaryView();
            this.View.DataContext = wizard;
        }
    }
}
