using Avalonia.Data.Converters;
using Avalonia.Data;
using System;
using System.Globalization;
using Avalonia;
using System.Windows;

namespace EquationResolver
{
    public class BoolToVisibilityInverseConverter : IValueConverter

    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {

            if (value != null)
            {
                return ((bool)value) ? Visibility.Hidden : Visibility.Visible; 
                
            }
            else return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}





