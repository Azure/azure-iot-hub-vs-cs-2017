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
    public partial class PageConfiguration : UserControl
    {
        public PageConfiguration()
        {
            InitializeComponent();

            this.DataContextChanged += PageConfiguration_DataContextChanged;
        }

        private void PageConfiguration_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Authenticator authenticator = (DataContext as WizardMain).Authenticator;

            if (authenticator.View.Parent == null)
            {
                this.TopPanel.Children.Add(authenticator.View);
            }
        }
    }
}
