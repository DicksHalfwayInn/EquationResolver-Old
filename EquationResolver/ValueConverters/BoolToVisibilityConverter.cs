using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EquationResolver
{
    public class BoolToVisibilityConverter : IValueConverter
    {
            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                if (value is bool visible)
                    return visible ? Visibility.Visible : Visibility.Collapsed;

                return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
            }

            public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
