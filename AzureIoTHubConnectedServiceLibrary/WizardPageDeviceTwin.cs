using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;


namespace AzureIoTHubConnectedService
{
    public class WizardPageDeviceTwin : ConnectedServiceWizardPage
    {
        internal WizardPageDeviceTwin(WizardMain wizard)
        {
            this.Title = Resource.WizardPageTitle;
            this.Legend = "Device Twin";
            this.Description = "Device Twin";
            this.IsEnabled = false;

            this.View = new WizardPageDeviceTwinView(wizard);
        }
    }
}
