using System;
using System.Collections.Generic;
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
    /// Interaction logic for PageDeviceTwin.xaml
    /// </summary>
    public partial class PageDeviceTwin : UserControl
    {
        public PageDeviceTwin()
        {
            InitializeComponent();
        }

        private void MainTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((MainTab.SelectedItem as TabItem).Header.ToString() == "Update Properties")
            {
                ButtonRefresh.Visibility = Visibility.Collapsed;
                ButtonUpdate.Visibility = Visibility.Visible;
            }
            else
            {
                ButtonRefresh.Visibility = Visibility.Visible;
                ButtonUpdate.Visibility = Visibility.Collapsed;
            }
        }
    }
}
