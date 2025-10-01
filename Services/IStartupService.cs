using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContextMenuEditor.Services;

/// <summary>
/// Interface for managing Windows startup entries.
/// Follows the Dependency Inversion Principle.
/// </summary>
public interface IStartupService
{
    /// <summary>
    /// Discovers all startup entries from the Windows registry.
    /// </summary>
    /// <returns>List of discovered startup items.</returns>
    Task<List<Models.StartupItem>> DiscoverStartupItemsAsync();

    /// <summary>
    /// Enables a startup item in the registry.
    /// </summary>
    /// <param name="item">The item to enable.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> EnableItemAsync(Models.StartupItem item);

    /// <summary>
    /// Disables a startup item in the registry.
    /// </summary>
    /// <param name="item">The item to disable.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> DisableItemAsync(Models.StartupItem item);

    /// <summary>
    /// Deletes a startup item from the registry.
    /// </summary>
    /// <param name="item">The item to delete.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> DeleteItemAsync(Models.StartupItem item);

    /// <summary>
    /// Creates a backup of startup registry entries to a .reg file.
    /// </summary>
    /// <param name="items">List of startup items to backup.</param>
    /// <param name="filePath">Path where the .reg file should be saved.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> CreateBackupAsync(List<Models.StartupItem> items, string filePath);
}
