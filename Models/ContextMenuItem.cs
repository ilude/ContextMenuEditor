using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ContextMenuEditor.Models;

/// <summary>
/// Represents a single context menu item discovered from the Windows registry.
/// Follows the Single Responsibility Principle - only contains data, no business logic.
/// </summary>
public class ContextMenuItem : INotifyPropertyChanged
{
    private bool _isEnabled;

    /// <summary>
    /// Gets or sets whether this context menu item is currently enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the registry key path for this item.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the context menu item.
    /// </summary>
    public string ProgramName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the publisher/vendor of the software that created this menu item.
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    /// Gets or sets the file path to the executable or DLL that handles this menu item.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the type of context menu (File, Directory, Drive, Background).
    /// </summary>
    public ContextMenuType MenuType { get; set; }

    /// <summary>
    /// Gets or sets whether this is a system-level (as opposed to user-level) entry.
    /// </summary>
    public bool IsSystemLevel { get; set; }

    /// <summary>
    /// Gets or sets all registry locations where this item exists.
    /// Used to apply changes (enable/disable/delete) to all instances.
    /// </summary>
    public List<RegistryLocation> RegistryLocations { get; set; } = new();

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}

/// <summary>
/// Represents a specific registry location for a context menu item.
/// </summary>
public class RegistryLocation
{
    /// <summary>
    /// The registry root (e.g., HKEY_CLASSES_ROOT, HKEY_CURRENT_USER).
    /// </summary>
    public string RootKey { get; set; } = string.Empty;

    /// <summary>
    /// The full registry path under the root key.
    /// </summary>
    public string SubKeyPath { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a system-level entry (from HKEY_CLASSES_ROOT).
    /// </summary>
    public bool IsSystemLevel { get; set; }
}

/// <summary>
/// Defines the types of context menus available in Windows.
/// </summary>
public enum ContextMenuType
{
    File,
    Directory,
    Drive,
    Background,
    AllFiles
}
