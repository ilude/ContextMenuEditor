using System;
using System.Windows;
using ContextMenuEditor.Utilities;

namespace ContextMenuEditor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Register global exception handlers as early as possible
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        
        // Check if running with administrator privileges, relaunch if needed
        if (ElevationHelper.TryRelaunchElevated(e.Args))
        {
            // Elevated process was launched, shut down this instance
            Shutdown();
            return;
        }
        
        // If we get here, we're either elevated or user declined UAC
        if (!ElevationHelper.IsElevated())
        {
            // User declined elevation - show warning and continue (limited functionality)
            MessageBox.Show(
                "Context Menu Editor requires administrator privileges to modify registry entries.\n\n" +
                "The application will start, but registry operations may fail.",
                "Administrator Rights Required",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        
        // Initialize theme manager (dark mode by default)
        _ = ThemeManager.Instance;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            MessageBox.Show($"An unexpected error occurred and the application will now exit:\n\n{ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            // Ensure process exits; this handler may be on a background thread
            try { Current?.Shutdown(); } catch { /* ignore */ }
            Environment.Exit(1);
        }
    }

    private void OnDispatcherUnhandledException(object sender, 
        System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"An unexpected error occurred and the application will now exit:\n\n{e.Exception.Message}", 
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        // Mark as handled to avoid default crash dialog, then shut down cleanly
        e.Handled = true;
        try { Shutdown(); } catch { /* ignore */ }
    }
}
