using System;
using System.Windows.Data;

namespace AzureIoTHubConnectedService
{
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(System.Convert.ToString(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
