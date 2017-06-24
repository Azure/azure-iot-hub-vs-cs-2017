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
            this.Title = "Azure IoT Hub";
            this.Legend = "Provision Device";
            this.Description = "Provision Device with Connection String";
            this.IsEnabled = false;

            this.View = new WizardPageInjectConnectionStringView();
            this.View.DataContext = wizard;
        }
    }
}
