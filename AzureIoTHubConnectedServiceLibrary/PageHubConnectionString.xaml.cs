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
    /// Interaction logic for PageCloudToDeviceMsg.xaml
    /// </summary>
    public partial class PageHubConnectionString : UserControl
    {
        public PageHubConnectionString()
        {
            InitializeComponent();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionString.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            ChangeButton.Visibility = Visibility.Visible;
            UpdateButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            ConnectionString.IsEnabled = false;
        }
        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeButton.Visibility = Visibility.Collapsed;
            UpdateButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Visible;
            ConnectionString.IsEnabled = true;
            ConnectionString.Text = "";
            ConnectionString.Focus();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionString.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            ChangeButton.Visibility = Visibility.Visible;
            UpdateButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            ConnectionString.IsEnabled = false;
        }
    }
}
