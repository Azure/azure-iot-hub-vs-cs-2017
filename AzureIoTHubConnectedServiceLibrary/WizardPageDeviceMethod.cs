using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;


namespace AzureIoTHubConnectedService
{
    public class WizardPageDeviceMethod : ConnectedServiceWizardPage
    {
        internal WizardPageDeviceMethod(WizardMain wizard)
        {
            this.Title = "Azure IoT Hub";
            this.Legend = "Device Method";
            this.Description = "Device Method";
            this.IsEnabled = false;

            this.View = new WizardPageDeviceMethodView();
            this.View.DataContext = wizard;
        }
    }
}
