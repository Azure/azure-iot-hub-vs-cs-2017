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
    public partial class WizardPageHubSelectionView : UserControl
    {
        public WizardPageHubSelectionView()
        {
            InitializeComponent();
        }

        public void HubSelected()
        {
            Tabs.SelectedIndex = 0;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainGrid.Height = e.NewSize.Height;
        }
    }
}
