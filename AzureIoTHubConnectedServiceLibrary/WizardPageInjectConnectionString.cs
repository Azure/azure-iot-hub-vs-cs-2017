using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;


namespace AzureIoTHubConnectedService
{
    public class WizardPageInjectConnectionString : ConnectedServiceWizardPage
    {
        internal WizardPageInjectConnectionString(WizardMain wizard)
        {
            this.Title = Resource.WizardPageTitle;
            this.Legend = Resource.WizardPageInjectConnectionStringLegend;
            this.Description = Resource.WizardPageInjectConnectionStringDescription;
            this.IsEnabled = false;

            this.View = new WizardPageInjectConnectionStringView();
            this.View.DataContext = wizard;
        }
    }
}
