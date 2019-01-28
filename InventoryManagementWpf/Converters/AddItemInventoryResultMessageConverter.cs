using System;
using System.Globalization;
using System.Windows.Data;

namespace InventoryManagementWpf
{
    public class AddItemInventoryResultMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isSuccess = (Result) value;

            switch (isSuccess)
            {
                case Result.NoResult:
                    return "";
                case Result.Success:
                    return "Item added";
                case Result.Fail:
                    return "Update failed";
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