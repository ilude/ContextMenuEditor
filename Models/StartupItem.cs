using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ContextMenuEditor.Models;

/// <summary>
/// Represents a Windows startup entry that runs when Windows starts.
/// Follows the Single Responsibility Principle - only contains data, no business logic.
/// </summary>
public class StartupItem : INotifyPropertyChanged
{
    private bool _isEnabled;

    /// <summary>
    /// Gets or sets whether this startup item is currently enabled.
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
    /// Gets or sets the registry value name for this startup item.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command/path that executes when Windows starts.
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the publisher/vendor of the software.
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    /// Gets or sets the location type (User Run, System Run, User RunOnce, System RunOnce).
    /// </summary>
    public StartupLocation Location { get; set; }

    /// <summary>
    /// Gets or sets the full registry path for this startup item.
    /// </summary>
    public string RegistryPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is a system-level (as opposed to user-level) entry.
    /// </summary>
    public bool IsSystemLevel { get; set; }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}

/// <summary>
/// Defines the registry locations where startup items can be found.
/// </summary>
public enum StartupLocation
{
    /// <summary>
    /// HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
    /// </summary>
    UserRun,

    /// <summary>
    /// HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run
    /// </summary>
    SystemRun,

    /// <summary>
    /// HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunOnce
    /// </summary>
    UserRunOnce,

    /// <summary>
    /// HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunOnce
    /// </summary>
    SystemRunOnce
}
