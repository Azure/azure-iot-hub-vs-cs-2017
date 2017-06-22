using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Globalization;

namespace AzureIoTHubConnectedService
{
    public partial class WizardMain: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void HandleHubSelected()
        {
        }

        // this is a hack to handle some inconsistencies that still exist
        public WizardMain MainModel { get { return this; } }
    }
}
