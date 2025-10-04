using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContextMenuEditor.Services;

/// <summary>
/// Interface for registry operations following the Dependency Inversion Principle.
/// Allows for easier testing and mocking.
/// </summary>
public interface IRegistryService
{
    /// <summary>
    /// Discovers all context menu items from the Windows registry.
    /// </summary>
    /// <returns>List of discovered context menu items.</returns>
    Task<List<Models.ContextMenuItem>> DiscoverContextMenuItemsAsync();

    /// <summary>
    /// Discovers all context menu items with control over including Windows system items.
    /// </summary>
    /// <param name="includeWindowsSystemItems">If true, includes items under Windows directories (System32, etc.).</param>
    Task<List<Models.ContextMenuItem>> DiscoverContextMenuItemsAsync(bool includeWindowsSystemItems);

    /// <summary>
    /// Discovers all context menu items with control over including Windows system items and COM handlers.
    /// </summary>
    /// <param name="includeWindowsSystemItems">If true, includes items under Windows directories (System32, etc.).</param>
    /// <param name="includeComHandlers">If true, includes COM-based context menu handlers (shellex).</param>
    Task<List<Models.ContextMenuItem>> DiscoverContextMenuItemsAsync(bool includeWindowsSystemItems, bool includeComHandlers);

    /// <summary>
    /// Enables a context menu item in the registry.
    /// </summary>
    /// <param name="item">The item to enable.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> EnableItemAsync(Models.ContextMenuItem item);

    /// <summary>
    /// Disables a context menu item in the registry.
    /// </summary>
    /// <param name="item">The item to disable.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> DisableItemAsync(Models.ContextMenuItem item);

    /// <summary>
    /// Deletes a context menu item from the registry.
    /// </summary>
    /// <param name="item">The item to delete.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> DeleteItemAsync(Models.ContextMenuItem item);

    /// <summary>
    /// Creates a backup of context menu registry entries to a .reg file.
    /// </summary>
    /// <param name="items">List of context menu items to backup.</param>
    /// <param name="filePath">Path where the .reg file should be saved.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> CreateBackupAsync(List<Models.ContextMenuItem> items, string filePath);
}
