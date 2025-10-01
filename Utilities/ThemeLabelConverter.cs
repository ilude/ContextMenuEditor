using System;
using System.Globalization;
using System.Windows.Data;

namespace ContextMenuEditor.Utilities;

/// <summary>
/// Converts boolean dark mode state to theme label text.
/// Shows the mode you can switch TO (not the current mode).
/// </summary>
public class ThemeLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isDarkMode)
        {
            // Show "Light" when in dark mode (button switches to light)
            // Show "Dark" when in light mode (button switches to dark)
            return isDarkMode ? "Light" : "Dark";
        }
        return "Light";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
