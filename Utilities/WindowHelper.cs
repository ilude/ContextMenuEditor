using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ContextMenuEditor.Utilities;

/// <summary>
/// Helper class for Windows-specific window operations like setting title bar theme.
/// Uses P/Invoke to enable dark mode title bar on Windows 10/11.
/// </summary>
public static class WindowHelper
{
    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    /// <summary>
    /// Sets the title bar theme for a WPF Window.
    /// Works on Windows 10 (build 17763+) and Windows 11.
    /// </summary>
    /// <param name="window">The Window to apply the theme to</param>
    /// <param name="isDarkMode">True for dark mode, false for light mode</param>
    public static void SetTitleBarTheme(Window window, bool isDarkMode)
    {
        if (window == null) return;

        try
        {
            var helper = new WindowInteropHelper(window);
            IntPtr hwnd = helper.Handle;

            if (hwnd == IntPtr.Zero)
            {
                // Window not yet shown, hook into SourceInitialized
                window.SourceInitialized += (s, e) =>
                {
                    var h = new WindowInteropHelper(window);
                    ApplyTitleBarTheme(h.Handle, isDarkMode);
                };
            }
            else
            {
                ApplyTitleBarTheme(hwnd, isDarkMode);
            }
        }
        catch
        {
            // Silently fail on older Windows versions that don't support this
        }
    }

    private static void ApplyTitleBarTheme(IntPtr hwnd, bool isDarkMode)
    {
        if (hwnd == IntPtr.Zero) return;

        int useImmersiveDarkMode = isDarkMode ? 1 : 0;

        // Try Windows 11 / Windows 10 20H1+ attribute first
        int result = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE,
            ref useImmersiveDarkMode, sizeof(int));

        // If that fails, try the older attribute for Windows 10 versions before 20H1
        if (result != 0)
        {
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1,
                ref useImmersiveDarkMode, sizeof(int));
        }
    }

    /// <summary>
    /// Updates the title bar theme for an already-shown window.
    /// Call this when theme changes at runtime.
    /// </summary>
    public static void UpdateTitleBarTheme(Window window, bool isDarkMode)
    {
        if (window == null) return;

        try
        {
            var helper = new WindowInteropHelper(window);
            if (helper.Handle != IntPtr.Zero)
            {
                ApplyTitleBarTheme(helper.Handle, isDarkMode);
            }
        }
        catch
        {
            // Silently fail
        }
    }
}
