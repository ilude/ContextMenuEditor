using System;
using System.Globalization;
using System.Windows.Data;

namespace ContextMenuEditor.Utilities;

/// <summary>
/// Converter that strips leading and trailing double quotes from strings.
/// </summary>
public class StripQuotesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str && !string.IsNullOrEmpty(str))
        {
            return str.Trim('"');
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
