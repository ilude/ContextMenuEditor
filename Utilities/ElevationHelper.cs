using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace ContextMenuEditor.Utilities;

/// <summary>
/// Helper class for detecting and requesting elevation (admin rights).
/// Based on: https://anthonysimmon.com/programmatically-elevate-dotnet-app-on-any-platform/
/// </summary>
public static class ElevationHelper
{
    /// <summary>
    /// Checks if the current process is running with administrator privileges.
    /// </summary>
    /// <returns>True if elevated, false otherwise.</returns>
    public static bool IsElevated()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        // On Linux/macOS, check if running as root (UID 0)
        // Not applicable for this Windows-only WPF app, but included for completeness
        return geteuid() == 0;
    }

    /// <summary>
    /// Attempts to restart the current application with elevated privileges.
    /// Returns true if the restart was initiated, false if already elevated or failed.
    /// </summary>
    /// <param name="args">Command-line arguments to pass to the elevated instance.</param>
    /// <returns>True if relaunch initiated (caller should exit), false if already elevated.</returns>
    public static bool TryRelaunchElevated(string[] args)
    {
        if (IsElevated())
        {
            return false; // Already elevated, no need to relaunch
        }

        try
        {
            var currentProcessPath = Environment.ProcessPath 
                ?? throw new InvalidOperationException("Cannot determine current process path");

            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = currentProcessPath,
                Verb = "runas" // Windows UAC elevation
            };

            // Pass along original command-line arguments
            foreach (var arg in args)
            {
                startInfo.ArgumentList.Add(arg);
            }

            Process.Start(startInfo);
            return true; // Successfully started elevated process, caller should exit
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // User declined UAC prompt
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to elevate: {ex.Message}");
            return false;
        }
    }

    // P/Invoke for Unix systems (not used in this Windows-only app, but included for reference)
    [DllImport("libc", SetLastError = true)]
    private static extern uint geteuid();
}
