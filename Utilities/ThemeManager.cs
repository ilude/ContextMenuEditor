using System;
using System.Windows;
using System.Windows.Media;

namespace ContextMenuEditor.Utilities;

/// <summary>
/// Manages application theme switching between light and dark modes.
/// Follows singleton pattern for global theme management.
/// </summary>
public class ThemeManager
{
    private static ThemeManager? _instance;
    private bool _isDarkMode = true; // Default to dark mode
    private ResourceDictionary? _themeResources;
    private int _mahAppsThemeIndex = -1;

    public static ThemeManager Instance => _instance ??= new ThemeManager();

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (_isDarkMode != value)
            {
                _isDarkMode = value;
                ApplyTheme();
                ThemeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler? ThemeChanged;

    private ThemeManager()
    {
        // Apply dark mode on startup
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        var app = Application.Current;
        if (app?.Resources == null) return;
        // 1) Ensure MahApps theme dictionary (Dark.Blue / Light.Blue) matches current mode
        EnsureMahAppsThemeDictionary(app);

        // 2) Maintain our dedicated theme resource dictionary without touching other
        // merged dictionaries (e.g., MahApps base resources).
        if (_themeResources == null)
        {
            _themeResources = new ResourceDictionary();
            app.Resources.MergedDictionaries.Add(_themeResources);
        }

        _themeResources.Clear();

        if (_isDarkMode)
        {
            ApplyDarkTheme(_themeResources);
        }
        else
        {
            ApplyLightTheme(_themeResources);
        }
    }

    private void EnsureMahAppsThemeDictionary(Application app)
    {
        var dicts = app.Resources.MergedDictionaries;

        // Locate MahApps theme dict index once
        if (_mahAppsThemeIndex < 0)
        {
            for (int i = 0; i < dicts.Count; i++)
            {
                var src = dicts[i].Source?.OriginalString ?? string.Empty;
                if (src.Contains("/MahApps.Metro;component/Styles/Themes/", StringComparison.OrdinalIgnoreCase))
                {
                    _mahAppsThemeIndex = i;
                    break;
                }
            }
        }

        if (_mahAppsThemeIndex >= 0 && _mahAppsThemeIndex < dicts.Count)
        {
            var newUri = new Uri(_isDarkMode
                ? "pack://application:,,,/MahApps.Metro;component/Styles/Themes/Dark.Blue.xaml"
                : "pack://application:,,,/MahApps.Metro;component/Styles/Themes/Light.Blue.xaml",
                UriKind.Absolute);

            // Only replace if different
            var current = dicts[_mahAppsThemeIndex];
            if (current.Source == null || !Uri.Compare(current.Source, newUri, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase).Equals(0))
            {
                var newDict = new ResourceDictionary { Source = newUri };
                dicts[_mahAppsThemeIndex] = newDict;
            }
        }
    }

    private void ApplyDarkTheme(ResourceDictionary dict)
    {
        // Background colors
        dict["WindowBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        dict["ControlBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(45, 45, 48));
        dict["HeaderBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(37, 37, 38));
        dict["AlternateRowBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(40, 40, 43));
        
        // Foreground colors
        dict["TextForegroundBrush"] = new SolidColorBrush(Color.FromRgb(241, 241, 241));
        dict["HeaderForegroundBrush"] = new SolidColorBrush(Color.FromRgb(200, 200, 200));
        
        // Border colors
        dict["BorderBrush"] = new SolidColorBrush(Color.FromRgb(63, 63, 70));
        dict["GridLineBrush"] = new SolidColorBrush(Color.FromRgb(63, 63, 70));
        
        // Button colors
        dict["ButtonBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(62, 62, 66));
        dict["ButtonHoverBrush"] = new SolidColorBrush(Color.FromRgb(82, 82, 86));
        dict["ButtonPressedBrush"] = new SolidColorBrush(Color.FromRgb(42, 42, 46));
        
        // Selection colors
        dict["SelectionBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(51, 153, 255));
        dict["SelectionInactiveBrush"] = new SolidColorBrush(Color.FromRgb(70, 70, 74));

    // ScrollBar colors (dark)
    dict["ScrollBarTrackBrush"] = new SolidColorBrush(Color.FromRgb(35, 35, 35));
    dict["ScrollBarThumbBrush"] = new SolidColorBrush(Color.FromRgb(90, 90, 90));
    dict["ScrollBarThumbHoverBrush"] = new SolidColorBrush(Color.FromRgb(120, 120, 120));
    }

    private void ApplyLightTheme(ResourceDictionary dict)
    {
        // Background colors
        dict["WindowBackgroundBrush"] = new SolidColorBrush(Colors.White);
        dict["ControlBackgroundBrush"] = new SolidColorBrush(Colors.White);
        dict["HeaderBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(240, 240, 240));
        dict["AlternateRowBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(245, 245, 245));
        
        // Foreground colors
        dict["TextForegroundBrush"] = new SolidColorBrush(Colors.Black);
        dict["HeaderForegroundBrush"] = new SolidColorBrush(Color.FromRgb(64, 64, 64));
        
        // Border colors
        dict["BorderBrush"] = new SolidColorBrush(Color.FromRgb(208, 208, 208));
        dict["GridLineBrush"] = new SolidColorBrush(Color.FromRgb(208, 208, 208));
        
        // Button colors
        dict["ButtonBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(240, 240, 240));
        dict["ButtonHoverBrush"] = new SolidColorBrush(Color.FromRgb(229, 241, 251));
        dict["ButtonPressedBrush"] = new SolidColorBrush(Color.FromRgb(204, 228, 247));
        
        // Selection colors
        dict["SelectionBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(51, 153, 255));
        dict["SelectionInactiveBrush"] = new SolidColorBrush(Color.FromRgb(220, 220, 220));

    // ScrollBar colors (light)
    dict["ScrollBarTrackBrush"] = new SolidColorBrush(Color.FromRgb(234, 234, 234));
    dict["ScrollBarThumbBrush"] = new SolidColorBrush(Color.FromRgb(192, 192, 192));
    dict["ScrollBarThumbHoverBrush"] = new SolidColorBrush(Color.FromRgb(158, 158, 158));
    }

    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
    }
}
