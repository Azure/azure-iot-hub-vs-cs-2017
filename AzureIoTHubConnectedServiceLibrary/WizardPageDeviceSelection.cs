using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using System.Windows;

using Microsoft.Azure.Devices;

namespace AzureIoTHubConnectedService
{
    public class WizardPageDeviceSelection : ConnectedServiceWizardPage
    {
        internal WizardPageDeviceSelection(WizardMain wizard)
        {
            this.Title = Resource.WizardPageTitle;
            this.Legend = Resource.WizardPageDeviceSelectionLegend;
            this.Description = Resource.WizardPageDeviceSelectionDescription;
            this.View = new WizardPageDeviceSelectionView();
            this.View.DataContext = wizard;
            this.IsEnabled = false;
        }

        public void DeviceSelected()
        {
            (this.View as WizardPageDeviceSelectionView).DeviceSelected();
        }
    }
}
