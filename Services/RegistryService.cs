using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using ContextMenuEditor.Models;
using ContextMenuEditor.Utilities;

namespace ContextMenuEditor.Services;

/// <summary>
/// Service for discovering and managing Windows context menu items in the registry.
/// Follows Single Responsibility Principle - only handles registry operations.
/// </summary>
public class RegistryService : IRegistryService
{
    // Registry paths for different context menu types
    private static readonly string[] FileContextPaths = new[]
    {
        @"*\shell",
        @"*\shellex\ContextMenuHandlers"
    };

    private static readonly string[] DirectoryContextPaths = new[]
    {
        @"Directory\shell",
        @"Directory\shellex\ContextMenuHandlers",
        @"Directory\Background\shell",
        @"Directory\Background\shellex\ContextMenuHandlers"
    };

    private static readonly string[] DriveContextPaths = new[]
    {
        @"Drive\shell",
        @"Drive\shellex\ContextMenuHandlers"
    };

    public async Task<List<ContextMenuItem>> DiscoverContextMenuItemsAsync()
    {
        return await Task.Run(() =>
        {
            var items = new List<ContextMenuItem>();

            // Discover file context menus
            items.AddRange(DiscoverMenuItems(FileContextPaths, ContextMenuType.File));

            // Discover directory context menus
            items.AddRange(DiscoverMenuItems(DirectoryContextPaths, ContextMenuType.Directory));

            // Discover drive context menus
            items.AddRange(DiscoverMenuItems(DriveContextPaths, ContextMenuType.Drive));

            // Remove duplicates - same program registered in multiple locations
            // Group by Key and normalized FilePath to catch items registered in multiple contexts
            // For example: git_shell appears in both Directory\shell and Directory\Background\shell
            var deduplicatedItems = items
                .GroupBy(item => new 
                { 
                    item.Key, 
                    // Normalize file path for comparison (remove quotes, lowercase, trim)
                    NormalizedPath = item.FilePath?.Trim('"').ToLowerInvariant().Split(' ')[0] ?? string.Empty
                })
                .Select(group =>
                {
                    // Take the first item (prefer system-level for display properties)
                    var primary = group.OrderByDescending(item => item.IsSystemLevel).First();
                    
                    // Merge all registry locations from all duplicates
                    primary.RegistryLocations = group
                        .SelectMany(item => item.RegistryLocations)
                        .ToList();
                    
                    return primary;
                })
                .OrderBy(item => item.MenuType)
                .ThenBy(item => item.ProgramName)
                .ToList();

            return deduplicatedItems;
        });
    }

    private List<ContextMenuItem> DiscoverMenuItems(string[] basePaths, ContextMenuType menuType)
    {
        var items = new List<ContextMenuItem>();

        foreach (var basePath in basePaths)
        {
            try
            {
                // Check both HKEY_CLASSES_ROOT (system) and HKEY_CURRENT_USER
                items.AddRange(ScanRegistryKey(Registry.ClassesRoot, basePath, menuType, true));
                items.AddRange(ScanRegistryKey(Registry.CurrentUser, @"Software\Classes\" + basePath, menuType, false));
            }
            catch (Exception ex)
            {
                // Log error but continue scanning other paths
                System.Diagnostics.Debug.WriteLine($"Error scanning {basePath}: {ex.Message}");
            }
        }

        return items;
    }

    private List<ContextMenuItem> ScanRegistryKey(RegistryKey rootKey, string path, ContextMenuType menuType, bool isSystemLevel)
    {
        var items = new List<ContextMenuItem>();
        var rootKeyName = rootKey.Name; // e.g., "HKEY_CLASSES_ROOT" or "HKEY_CURRENT_USER"

        try
        {
            using var key = rootKey.OpenSubKey(path);
            if (key == null) return items;

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                try
                {
                    using var subKey = key.OpenSubKey(subKeyName);
                    if (subKey == null) continue;

                    var item = CreateContextMenuItem(subKey, subKeyName, path, menuType, isSystemLevel, rootKeyName);
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reading subkey {subKeyName}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening key {path}: {ex.Message}");
        }

        return items;
    }

    private ContextMenuItem? CreateContextMenuItem(RegistryKey key, string keyName, string basePath, 
        ContextMenuType menuType, bool isSystemLevel, string rootKeyName)
    {
        // Skip known Windows built-in items we don't want to manage
        var skipList = new[] 
        { 
            "pintostartscreen", "pintohome", "windows.modernshare", "windows.share",
            "copyaspath", "copyto", "moveto", "sendto", "opennewwindow", "opennewprocess"
        };
        
        if (skipList.Contains(keyName.ToLowerInvariant()))
            return null;

        var displayName = key.GetValue("") as string ?? keyName;
        var muiVerb = key.GetValue("MUIVerb") as string;
        if (!string.IsNullOrEmpty(muiVerb))
            displayName = muiVerb;

        // Resolve resource strings like @shell32.dll,-8506 using Windows API
        displayName = ResourceStringResolver.ResolveResourceString(displayName);

        // Remove keyboard shortcut markers (& character)
        displayName = displayName.Replace("&", "");

        // Get command path
        string? commandPath = null;
        using (var commandKey = key.OpenSubKey("command"))
        {
            commandPath = commandKey?.GetValue("") as string;
        }

        // Filter out items without a file path - they're usually just submenus or organizational keys
        if (string.IsNullOrWhiteSpace(commandPath))
            return null;

        // Filter out Windows system directory programs using environment variables
        if (IsWindowsSystemProgram(commandPath))
            return null;

        // Check if item is enabled (LegacyDisable key existence means it's disabled)
        var isEnabled = key.GetValue("LegacyDisable") == null;

        // Try to determine publisher from the command path
        string? publisher = null;
        if (!string.IsNullOrEmpty(commandPath))
        {
            publisher = TryGetPublisher(commandPath);
        }

        // Create the registry location for this item
        var registryLocation = new RegistryLocation
        {
            RootKey = rootKeyName,
            SubKeyPath = $@"{basePath}\{keyName}",
            IsSystemLevel = isSystemLevel
        };

        return new ContextMenuItem
        {
            IsEnabled = isEnabled,
            Key = keyName,
            ProgramName = displayName,
            Publisher = publisher,
            FilePath = commandPath,
            MenuType = menuType,
            IsSystemLevel = isSystemLevel,
            RegistryLocations = new List<RegistryLocation> { registryLocation }
        };
    }

    private bool IsWindowsSystemProgram(string commandPath)
    {
        if (string.IsNullOrWhiteSpace(commandPath))
            return false;

        // Clean up the command path - remove quotes and arguments
        var cleanPath = commandPath.Trim().Trim('"').Split(' ')[0];

        // Get Windows directory paths using environment variables
        // This ensures it works regardless of where Windows is installed (C:\Windows, D:\Windows, etc.)
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

        // Also check for common Windows system paths in case environment vars aren't set properly
        var normalizedPath = cleanPath.Replace('/', '\\').ToLowerInvariant();
        if (normalizedPath.Contains(@"\windows\system32\") || 
            normalizedPath.Contains(@"\windows\syswow64\") ||
            normalizedPath.Contains(@"\windows\explorer.exe") ||
            normalizedPath.Contains(@"\windows\notepad.exe"))
            return true;

        return false;
    }

    private string? TryGetPublisher(string path)
    {
        // Simple heuristic to extract publisher from common paths
        if (path.Contains("Visual Studio", StringComparison.OrdinalIgnoreCase))
            return "Microsoft Corporation";
        if (path.Contains("VS Code", StringComparison.OrdinalIgnoreCase) || path.Contains(@"\Code.exe", StringComparison.OrdinalIgnoreCase))
            return "Microsoft Corporation";
        if (path.Contains("Git", StringComparison.OrdinalIgnoreCase))
            return "Git";
        if (path.Contains("7-Zip", StringComparison.OrdinalIgnoreCase))
            return "Igor Pavlov";
        if (path.Contains("Dropbox", StringComparison.OrdinalIgnoreCase))
            return "Dropbox, Inc.";
        if (path.Contains("OneDrive", StringComparison.OrdinalIgnoreCase))
            return "Microsoft Corporation";
        if (path.Contains("WinRAR", StringComparison.OrdinalIgnoreCase))
            return "win.rar GmbH";
        if (path.Contains("Tortoise", StringComparison.OrdinalIgnoreCase))
            return "TortoiseSVN";

        return null;
    }

    public Task<bool> EnableItemAsync(ContextMenuItem item)
    {
        return Task.Run(() =>
        {
            try
            {
                // Enable the item in all registry locations
                foreach (var location in item.RegistryLocations)
                {
                    var rootKey = GetRegistryRootKey(location.RootKey);
                    if (rootKey == null) continue;

                    using var key = rootKey.OpenSubKey(location.SubKeyPath, writable: true);
                    if (key == null) continue;

                    // Remove the LegacyDisable value to enable the item
                    try
                    {
                        key.DeleteValue("LegacyDisable", throwOnMissingValue: false);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // May need admin rights for system-level keys
                        System.Diagnostics.Debug.WriteLine($"Need admin rights to enable: {location.SubKeyPath}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error enabling item: {ex.Message}");
                return false;
            }
        });
    }

    public Task<bool> DisableItemAsync(ContextMenuItem item)
    {
        return Task.Run(() =>
        {
            try
            {
                // Disable the item in all registry locations
                foreach (var location in item.RegistryLocations)
                {
                    var rootKey = GetRegistryRootKey(location.RootKey);
                    if (rootKey == null) continue;

                    using var key = rootKey.OpenSubKey(location.SubKeyPath, writable: true);
                    if (key == null) continue;

                    // Add LegacyDisable value to disable the item
                    try
                    {
                        key.SetValue("LegacyDisable", string.Empty);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // May need admin rights for system-level keys
                        System.Diagnostics.Debug.WriteLine($"Need admin rights to disable: {location.SubKeyPath}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disabling item: {ex.Message}");
                return false;
            }
        });
    }

    public Task<bool> DeleteItemAsync(ContextMenuItem item)
    {
        return Task.Run(() =>
        {
            try
            {
                // TODO: Create backup before deletion
                
                // Delete the item from all registry locations
                foreach (var location in item.RegistryLocations)
                {
                    var rootKey = GetRegistryRootKey(location.RootKey);
                    if (rootKey == null) continue;

                    // Get parent key path
                    var lastBackslash = location.SubKeyPath.LastIndexOf('\\');
                    if (lastBackslash < 0) continue;
                    
                    var parentPath = location.SubKeyPath.Substring(0, lastBackslash);
                    var keyToDelete = location.SubKeyPath.Substring(lastBackslash + 1);

                    try
                    {
                        using var parentKey = rootKey.OpenSubKey(parentPath, writable: true);
                        parentKey?.DeleteSubKeyTree(keyToDelete, throwOnMissingSubKey: false);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // May need admin rights for system-level keys
                        System.Diagnostics.Debug.WriteLine($"Need admin rights to delete: {location.SubKeyPath}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting item: {ex.Message}");
                return false;
            }
        });
    }

    private RegistryKey? GetRegistryRootKey(string rootKeyName)
    {
        // Map root key name to actual RegistryKey
        if (rootKeyName.Contains("HKEY_CLASSES_ROOT", StringComparison.OrdinalIgnoreCase))
            return Registry.ClassesRoot;
        if (rootKeyName.Contains("HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase))
            return Registry.CurrentUser;
        if (rootKeyName.Contains("HKEY_LOCAL_MACHINE", StringComparison.OrdinalIgnoreCase))
            return Registry.LocalMachine;
        
        return null;
    }

    public Task<bool> CreateBackupAsync(List<ContextMenuItem> items, string filePath)
    {
        return Task.Run(() =>
        {
            try
            {
                using var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.Unicode);
                
                // Write .reg file header
                writer.WriteLine("Windows Registry Editor Version 5.00");
                writer.WriteLine();
                writer.WriteLine("; Context Menu Editor Backup");
                writer.WriteLine($"; Created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"; Total Items: {items.Count}");
                writer.WriteLine();

                foreach (var item in items)
                {
                    writer.WriteLine($"; {item.ProgramName} ({item.MenuType})");
                    
                    foreach (var location in item.RegistryLocations)
                    {
                        try
                        {
                            var rootKey = GetRegistryRootKey(location.RootKey);
                            if (rootKey == null) continue;

                            using var key = rootKey.OpenSubKey(location.SubKeyPath);
                            if (key == null) continue;

                            // Write the registry key path
                            var fullPath = $"{ConvertRootKeyToRegFormat(location.RootKey)}\\{location.SubKeyPath}";
                            writer.WriteLine($"[{fullPath}]");

                            // Export all values in the key
                            foreach (var valueName in key.GetValueNames())
                            {
                                var value = key.GetValue(valueName);
                                var valueKind = key.GetValueKind(valueName);
                                
                                WriteRegistryValue(writer, valueName, value, valueKind);
                            }

                            // Export the command subkey if it exists
                            using var commandKey = key.OpenSubKey("command");
                            if (commandKey != null)
                            {
                                writer.WriteLine();
                                writer.WriteLine($"[{fullPath}\\command]");
                                
                                foreach (var valueName in commandKey.GetValueNames())
                                {
                                    var value = commandKey.GetValue(valueName);
                                    var valueKind = commandKey.GetValueKind(valueName);
                                    
                                    WriteRegistryValue(writer, valueName, value, valueKind);
                                }
                            }

                            writer.WriteLine();
                        }
                        catch (Exception ex)
                        {
                            writer.WriteLine($"; Error reading {location.SubKeyPath}: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"Error backing up {location.SubKeyPath}: {ex.Message}");
                        }
                    }
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

    private string ConvertRootKeyToRegFormat(string rootKeyName)
    {
        if (rootKeyName.Contains("HKEY_CLASSES_ROOT", StringComparison.OrdinalIgnoreCase))
            return "HKEY_CLASSES_ROOT";
        if (rootKeyName.Contains("HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase))
            return "HKEY_CURRENT_USER";
        if (rootKeyName.Contains("HKEY_LOCAL_MACHINE", StringComparison.OrdinalIgnoreCase))
            return "HKEY_LOCAL_MACHINE";
        
        return rootKeyName;
    }

    private void WriteRegistryValue(System.IO.StreamWriter writer, string valueName, object? value, Microsoft.Win32.RegistryValueKind valueKind)
    {
        if (value == null) return;

        var displayName = string.IsNullOrEmpty(valueName) ? "@" : $"\"{valueName}\"";

        switch (valueKind)
        {
            case Microsoft.Win32.RegistryValueKind.String:
                writer.WriteLine($"{displayName}=\"{EscapeRegString(value.ToString() ?? "")}\"");
                break;

            case Microsoft.Win32.RegistryValueKind.DWord:
                writer.WriteLine($"{displayName}=dword:{Convert.ToUInt32(value):x8}");
                break;

            case Microsoft.Win32.RegistryValueKind.QWord:
                writer.WriteLine($"{displayName}=qword:{Convert.ToUInt64(value):x16}");
                break;

            case Microsoft.Win32.RegistryValueKind.Binary:
                if (value is byte[] bytes)
                {
                    writer.Write($"{displayName}=hex:");
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (i > 0) writer.Write(",");
                        if (i > 0 && i % 25 == 0) writer.Write("\\\n  ");
                        writer.Write($"{bytes[i]:x2}");
                    }
                    writer.WriteLine();
                }
                break;

            case Microsoft.Win32.RegistryValueKind.ExpandString:
                writer.WriteLine($"{displayName}=hex(2):{StringToHex(value.ToString() ?? "")}");
                break;

            case Microsoft.Win32.RegistryValueKind.MultiString:
                if (value is string[] strings)
                {
                    var combined = string.Join("\0", strings) + "\0";
                    writer.WriteLine($"{displayName}=hex(7):{StringToHex(combined)}");
                }
                break;

            default:
                writer.WriteLine($"; Unsupported value type: {valueKind}");
                break;
        }
    }

    private string EscapeRegString(string str)
    {
        return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private string StringToHex(string str)
    {
        var bytes = System.Text.Encoding.Unicode.GetBytes(str + "\0");
        var hex = string.Join(",", bytes.Select(b => $"{b:x2}"));
        return hex;
    }
}
