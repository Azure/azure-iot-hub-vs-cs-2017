using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using System.Windows;

using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace AzureIoTHubConnectedService
{
    public class WizardPageLogin : ConnectedServiceWizardPage
    {
        public WizardPageLogin(WizardMain model, Authenticator authenticator)
        {
            this.Title = "Azure IoT Hub";
            this.Legend = "Login";
            this.Description = "Login to Azure or Select TPM";
            this.MainModel = model;

            this.View = new WizardPageLoginView(this, authenticator);
        }

        public WizardMain MainModel
        {
            get; set;
        }
    }
}
