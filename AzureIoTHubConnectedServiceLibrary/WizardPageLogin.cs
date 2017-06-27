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
            this.Title = Resource.WizardPageTitle;
            this.Legend = Resource.WizardPageLoginLegend;
            this.Description = Resource.WizardPageLoginDescription;
            this.MainModel = model;

            this.View = new WizardPageLoginView(authenticator);
            this.View.DataContext = this;
        }

        public WizardMain MainModel
        {
            get; set;
        }
    }
}
