using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;


namespace AzureIoTHubConnectedService
{
    public class WizardPageDirectMethod : ConnectedServiceWizardPage
    {
        internal WizardPageDirectMethod(WizardMain wizard)
        {
            this.Title = Resource.WizardPageTitle;
            this.Legend = "Direct Method";
            this.Description = "Direct Method";
            this.IsEnabled = false;

            this.View = new WizardPageDirectMethodView();
            this.View.DataContext = wizard;
        }
    }
}
