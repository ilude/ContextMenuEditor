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
        
        // Global exception handling
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            MessageBox.Show($"An unexpected error occurred: {ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnDispatcherUnhandledException(object sender, 
        System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"An unexpected error occurred: {e.Exception.Message}", 
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
