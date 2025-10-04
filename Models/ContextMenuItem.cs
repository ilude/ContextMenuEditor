using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace ContextMenuEditor.Models;

/// <summary>
/// Represents a single context menu item discovered from the Windows registry.
/// Follows the Single Responsibility Principle - only contains data, no business logic.
/// </summary>
public class ContextMenuItem : INotifyPropertyChanged
{
    private bool _isEnabled;
    private VisibilityState _visibility = VisibilityState.Normal;

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
    /// Aggregated set of menu types this item applies to (e.g., File, Directory, Background).
    /// This enables deduplication across contexts while still surfacing coverage in the UI.
    /// </summary>
    public List<ContextMenuType> MenuTypes { get; set; } = new();

    /// <summary>
    /// Returns a comma-separated string of the aggregated menu types for display.
    /// Falls back to the single MenuType if MenuTypes is empty (backward compatible).
    /// </summary>
    public string TypesDisplay
    {
        get
        {
            if (MenuTypes != null && MenuTypes.Count > 0)
            {
                var ordered = MenuTypes.Distinct().OrderBy(t => t.ToString());
                return string.Join(", ", ordered);
            }
            return MenuType.ToString();
        }
    }

    /// <summary>
    /// Display-friendly target(s) where this item appears. Uses "Empty" instead of "Background".
    /// Aggregates multiple targets when present.
    /// </summary>
    public string TargetDisplay
    {
        get
        {
            IEnumerable<ContextMenuType> types = (MenuTypes != null && MenuTypes.Count > 0)
                ? MenuTypes.Distinct()
                : new[] { MenuType };

            var mapped = types.Select(t => t == ContextMenuType.Background ? "Empty" : t.ToString());
            return string.Join(", ", mapped.OrderBy(s => s));
        }
    }

    /// <summary>
    /// Gets or sets whether this is a system-level (as opposed to user-level) entry.
    /// </summary>
    public bool IsSystemLevel { get; set; }

    /// <summary>
    /// Visibility state in Explorer (Normal, Extended, Hidden). Extended shows when Shift is held.
    /// </summary>
    public VisibilityState Visibility
    {
        get => _visibility;
        set
        {
            if (_visibility != value)
            {
                _visibility = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsExtended));
            }
        }
    }

    /// <summary>
    /// Convenience flag: true if the item is only visible when pressing Shift (Extended).
    /// </summary>
    public bool IsExtended => Visibility == VisibilityState.Extended;

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

    /// <summary>
    /// True if this registry location represents a shellex ContextMenuHandler (COM CLSID based).
    /// </summary>
    public bool IsShellEx { get; set; }

    /// <summary>
    /// The CLSID associated with this shellex handler (normalized to {guid}).
    /// </summary>
    public string? HandlerClsid { get; set; }
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

/// <summary>
/// Visibility state of a context menu item in Explorer.
/// </summary>
public enum VisibilityState
{
    Normal,
    Extended,
    Hidden
}
