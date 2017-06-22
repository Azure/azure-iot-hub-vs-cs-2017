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
    /// Interaction logic for WizardPageLoginView.xaml
    /// </summary>
    public partial class WizardPageLoginView : UserControl
    {
        public WizardPageLoginView(object model, object authenticator)
        {
            InitializeComponent();

            m_Authenticator = authenticator;

            DataContext = model;
        }

        private dynamic m_Authenticator;

        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (m_Authenticator.View.Parent == null)
            {
                this.TopPanel.Children.Add(m_Authenticator.View);
            }
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start("http://aka.ms/tpmiothubcs");
        }
    }

    public class RadioButtonCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}
