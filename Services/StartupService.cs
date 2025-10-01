using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using ContextMenuEditor.Models;

namespace ContextMenuEditor.Services;

/// <summary>
/// Service for discovering and managing Windows startup items in the registry.
/// Follows Single Responsibility Principle - only handles startup registry operations.
/// </summary>
public class StartupService : IStartupService
{
    // Registry paths for startup locations
    private const string UserRunPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string SystemRunPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string UserRunOncePath = @"Software\Microsoft\Windows\CurrentVersion\RunOnce";
    private const string SystemRunOncePath = @"Software\Microsoft\Windows\CurrentVersion\RunOnce";
    
    // Backup registry path for disabled items
    private const string DisabledUserRunPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
    private const string DisabledSystemRunPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run32";

    public async Task<List<StartupItem>> DiscoverStartupItemsAsync()
    {
        return await Task.Run(() =>
        {
            var items = new List<StartupItem>();

            try
            {
                // Discover User Run items
                items.AddRange(DiscoverStartupItems(
                    Registry.CurrentUser, 
                    UserRunPath, 
                    StartupLocation.UserRun,
                    isSystemLevel: false));

                // Discover System Run items
                items.AddRange(DiscoverStartupItems(
                    Registry.LocalMachine, 
                    SystemRunPath, 
                    StartupLocation.SystemRun,
                    isSystemLevel: true));

                // Discover User RunOnce items
                items.AddRange(DiscoverStartupItems(
                    Registry.CurrentUser, 
                    UserRunOncePath, 
                    StartupLocation.UserRunOnce,
                    isSystemLevel: false));

                // Discover System RunOnce items
                items.AddRange(DiscoverStartupItems(
                    Registry.LocalMachine, 
                    SystemRunOncePath, 
                    StartupLocation.SystemRunOnce,
                    isSystemLevel: true));

                // Sort by name for better UI display
                return items.OrderBy(item => item.Name).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error discovering startup items: {ex.Message}");
                return items;
            }
        });
    }

    private List<StartupItem> DiscoverStartupItems(
        RegistryKey rootKey, 
        string path, 
        StartupLocation location,
        bool isSystemLevel)
    {
        var items = new List<StartupItem>();
        var rootKeyName = rootKey.Name; // e.g., "HKEY_CURRENT_USER" or "HKEY_LOCAL_MACHINE"

        try
        {
            using var key = rootKey.OpenSubKey(path);
            if (key == null) return items;

            foreach (var valueName in key.GetValueNames())
            {
                try
                {
                    // Skip empty value names
                    if (string.IsNullOrWhiteSpace(valueName))
                        continue;

                    var command = key.GetValue(valueName) as string;
                    if (string.IsNullOrWhiteSpace(command))
                        continue;

                    // Filter out Windows system programs
                    if (IsWindowsSystemProgram(command))
                        continue;

                    // Try to determine publisher from the command path
                    var publisher = TryGetPublisher(command);

                    // Check if item is disabled using StartupApproved mechanism
                    var isEnabled = CheckIfEnabled(rootKey, valueName, location);

                    var item = new StartupItem
                    {
                        Name = valueName,
                        Command = command,
                        Publisher = publisher,
                        Location = location,
                        IsSystemLevel = isSystemLevel,
                        RegistryPath = $@"{rootKeyName}\{path}",
                        IsEnabled = isEnabled
                    };

                    items.Add(item);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reading startup value {valueName}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening startup key {path}: {ex.Message}");
        }

        return items;
    }

    private bool CheckIfEnabled(RegistryKey rootKey, string valueName, StartupLocation location)
    {
        try
        {
            // Windows 10+ uses StartupApproved to track disabled startup items
            var approvedPath = location == StartupLocation.UserRun || location == StartupLocation.UserRunOnce
                ? DisabledUserRunPath
                : DisabledSystemRunPath;

            using var approvedKey = rootKey.OpenSubKey(approvedPath);
            if (approvedKey == null) return true; // No disabled tracking = enabled

            var approvedData = approvedKey.GetValue(valueName) as byte[];
            if (approvedData == null || approvedData.Length < 1) return true;

            // The first byte indicates enabled status
            // 0x02 = disabled, 0x00 = enabled (or other values for enabled)
            return approvedData[0] != 0x02;
        }
        catch
        {
            // If we can't read the approval status, assume enabled
            return true;
        }
    }

    private bool IsWindowsSystemProgram(string commandPath)
    {
        if (string.IsNullOrWhiteSpace(commandPath))
            return false;

        // Clean up the command path - remove quotes and arguments
        var cleanPath = commandPath.Trim().Trim('"').Split(' ')[0];

        // Get Windows directory paths
        var windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var system32Dir = Environment.GetFolderPath(Environment.SpecialFolder.System);
        var systemX86Dir = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);

        // Check if the path starts with any Windows system directory
        if (!string.IsNullOrEmpty(windowsDir) && 
            cleanPath.StartsWith(windowsDir, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!string.IsNullOrEmpty(system32Dir) && 
            cleanPath.StartsWith(system32Dir, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!string.IsNullOrEmpty(systemX86Dir) && 
            cleanPath.StartsWith(systemX86Dir, StringComparison.OrdinalIgnoreCase))
            return true;

        // Filter common Windows system programs
        var normalizedPath = cleanPath.ToLowerInvariant();
        if (normalizedPath.Contains(@"\windows\system32\") || 
            normalizedPath.Contains(@"\windows\syswow64\") ||
            normalizedPath.Contains("securityhealthsystray.exe") ||
            normalizedPath.Contains("windowsdefender"))
            return true;

        return false;
    }

    private string? TryGetPublisher(string path)
    {
        // Simple heuristic to extract publisher from common paths
        if (path.Contains("Microsoft", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("OneDrive", StringComparison.OrdinalIgnoreCase))
            return "Microsoft Corporation";
        if (path.Contains("Dropbox", StringComparison.OrdinalIgnoreCase))
            return "Dropbox, Inc.";
        if (path.Contains("Google", StringComparison.OrdinalIgnoreCase))
            return "Google LLC";
        if (path.Contains("Adobe", StringComparison.OrdinalIgnoreCase))
            return "Adobe Inc.";
        if (path.Contains("Steam", StringComparison.OrdinalIgnoreCase))
            return "Valve Corporation";
        if (path.Contains("Discord", StringComparison.OrdinalIgnoreCase))
            return "Discord Inc.";
        if (path.Contains("Slack", StringComparison.OrdinalIgnoreCase))
            return "Slack Technologies";
        if (path.Contains("Spotify", StringComparison.OrdinalIgnoreCase))
            return "Spotify AB";

        return null;
    }

    public Task<bool> EnableItemAsync(StartupItem item)
    {
        return Task.Run(() =>
        {
            try
            {
                var rootKey = item.IsSystemLevel ? Registry.LocalMachine : Registry.CurrentUser;
                var approvedPath = item.Location == StartupLocation.UserRun || item.Location == StartupLocation.UserRunOnce
                    ? DisabledUserRunPath
                    : DisabledSystemRunPath;

                // Remove from StartupApproved (disabled list)
                try
                {
                    using var approvedKey = rootKey.OpenSubKey(approvedPath, writable: true);
                    approvedKey?.DeleteValue(item.Name, throwOnMissingValue: false);
                }
                catch (UnauthorizedAccessException)
                {
                    System.Diagnostics.Debug.WriteLine($"Need admin rights to enable: {item.Name}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error enabling startup item: {ex.Message}");
                return false;
            }
        });
    }

    public Task<bool> DisableItemAsync(StartupItem item)
    {
        return Task.Run(() =>
        {
            try
            {
                var rootKey = item.IsSystemLevel ? Registry.LocalMachine : Registry.CurrentUser;
                var approvedPath = item.Location == StartupLocation.UserRun || item.Location == StartupLocation.UserRunOnce
                    ? DisabledUserRunPath
                    : DisabledSystemRunPath;

                // Add to StartupApproved with disabled flag
                try
                {
                    using var approvedKey = rootKey.OpenSubKey(approvedPath, writable: true) 
                        ?? rootKey.CreateSubKey(approvedPath);
                    
                    if (approvedKey != null)
                    {
                        // Create a disabled entry (0x02 in first byte indicates disabled)
                        var disabledData = new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                        approvedKey.SetValue(item.Name, disabledData, RegistryValueKind.Binary);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    System.Diagnostics.Debug.WriteLine($"Need admin rights to disable: {item.Name}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disabling startup item: {ex.Message}");
                return false;
            }
        });
    }

    public Task<bool> DeleteItemAsync(StartupItem item)
    {
        return Task.Run(() =>
        {
            try
            {
                var rootKey = item.IsSystemLevel ? Registry.LocalMachine : Registry.CurrentUser;
                var runPath = item.Location switch
                {
                    StartupLocation.UserRun => UserRunPath,
                    StartupLocation.SystemRun => SystemRunPath,
                    StartupLocation.UserRunOnce => UserRunOncePath,
                    StartupLocation.SystemRunOnce => SystemRunOncePath,
                    _ => UserRunPath
                };

                try
                {
                    // Delete from Run/RunOnce
                    using var runKey = rootKey.OpenSubKey(runPath, writable: true);
                    runKey?.DeleteValue(item.Name, throwOnMissingValue: false);

                    // Also remove from StartupApproved if present
                    var approvedPath = item.Location == StartupLocation.UserRun || item.Location == StartupLocation.UserRunOnce
                        ? DisabledUserRunPath
                        : DisabledSystemRunPath;
                    
                    using var approvedKey = rootKey.OpenSubKey(approvedPath, writable: true);
                    approvedKey?.DeleteValue(item.Name, throwOnMissingValue: false);
                }
                catch (UnauthorizedAccessException)
                {
                    System.Diagnostics.Debug.WriteLine($"Need admin rights to delete: {item.Name}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting startup item: {ex.Message}");
                return false;
            }
        });
    }

    public Task<bool> CreateBackupAsync(List<StartupItem> items, string filePath)
    {
        return Task.Run(() =>
        {
            try
            {
                using var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.Unicode);
                
                // Write .reg file header
                writer.WriteLine("Windows Registry Editor Version 5.00");
                writer.WriteLine();
                writer.WriteLine("; Startup Items Backup");
                writer.WriteLine($"; Created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"; Total Items: {items.Count}");
                writer.WriteLine();

                // Group items by registry location
                var groupedItems = items.GroupBy(item => item.RegistryPath);

                foreach (var group in groupedItems)
                {
                    var registryPath = group.Key;
                    writer.WriteLine($"[{registryPath}]");

                    foreach (var item in group)
                    {
                        // Write the registry value
                        var escapedCommand = item.Command.Replace("\\", "\\\\").Replace("\"", "\\\"");
                        writer.WriteLine($"\"{item.Name}\"=\"{escapedCommand}\"");
                    }

                    writer.WriteLine();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating backup: {ex.Message}");
                return false;
            }
        });
    }
}
