using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace LightningRod.Frontend.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return AvaloniaProperty.UnsetValue;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return AvaloniaProperty.UnsetValue;
        }
    }
}
