using System;
using System.Globalization;
using System.Windows.Data;

namespace Rewriter.ValueConverters
{
    public class TextBooleanValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolean = (bool) value;

            if (boolean)
            {
                return parameter;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}