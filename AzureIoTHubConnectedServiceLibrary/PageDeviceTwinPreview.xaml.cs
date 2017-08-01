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
    /// Interaction logic for WizardPageSummaryView.xaml
    /// </summary>
    public partial class PageDeviceTwinPreview : UserControl
    {
        public PageDeviceTwinPreview()
        {
            InitializeComponent();
        }

        public PageDeviceTwinPreview(object model)
        {
            InitializeComponent();

            DataContext = model;
        }

        public void SetModel(object model)
        {
            DataContext = model;
        }
    }
}
