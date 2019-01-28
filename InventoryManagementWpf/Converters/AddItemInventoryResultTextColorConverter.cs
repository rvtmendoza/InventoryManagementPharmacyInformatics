using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace InventoryManagementWpf
{
    public class AddItemInventoryResultTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isSuccess = (Result) value;

            switch (isSuccess)
            {
                case Result.NoResult:
                    return null;
                case Result.Success:
                    return new SolidColorBrush(Colors.LawnGreen);
                case Result.Fail:
                    return new SolidColorBrush(Colors.Red);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}