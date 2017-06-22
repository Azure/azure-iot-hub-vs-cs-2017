using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AzureIoTHubConnectedService
{
    /// <summary>
    /// Interaction logic for WizardPageHubSelectionView.xaml
    /// </summary>
    public partial class WizardPageDeviceSelectionView : UserControl
    {
        public WizardPageDeviceSelectionView()
        {
            InitializeComponent();
        }
        public WizardPageDeviceSelectionView(object model)
        {
            PageModel = model;

            InitializeComponent();

            DataContext = model;

#pragma warning disable CS4014
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PageModel.CreateNewDevice(this.TextBoxDeviceName.Text);
        }

        private dynamic PageModel;
    }
}
