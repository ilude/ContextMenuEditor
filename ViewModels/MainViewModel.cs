using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ContextMenuEditor.Models;
using ContextMenuEditor.Services;
using ContextMenuEditor.Utilities;

namespace ContextMenuEditor.ViewModels;

/// <summary>
/// Main ViewModel for the application.
/// Follows MVVM pattern and SOLID principles.
/// Uses Dependency Injection for services.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IRegistryService _registryService;
    private ContextMenuItem? _selectedItem;
    private bool _hasSelection;
    private string _statusMessage = "Loading...";
    private bool _isLoading;
    private bool _isDarkMode;
    private bool _includeWindowsSystemItems;
    private bool _showComHandlers;

    public MainViewModel() : this(new RegistryService())
    {
    }

    public MainViewModel(IRegistryService registryService)
    {
        _registryService = registryService;

        // Initialize single collection for all items
        AllMenuItems = new ObservableCollection<ContextMenuItem>();

        // Initialize theme
        _isDarkMode = ThemeManager.Instance.IsDarkMode;
        ThemeManager.Instance.ThemeChanged += OnThemeChanged;

        // Initialize commands
        EnableCommand = new RelayCommand(ExecuteEnable, () => HasSelection && !IsLoading);
        DisableCommand = new RelayCommand(ExecuteDisable, () => HasSelection && !IsLoading);
        DeleteCommand = new RelayCommand(ExecuteDelete, () => HasSelection && !IsLoading);
        BackupCommand = new RelayCommand(ExecuteBackup, () => !IsLoading);
        RefreshCommand = new RelayCommand(LoadContextMenuItems, () => !IsLoading);
        ToggleThemeCommand = new RelayCommand(ExecuteToggleTheme);

        // Load initial data
        LoadContextMenuItems();
    }

    #region Properties

    public ObservableCollection<ContextMenuItem> AllMenuItems { get; }

    public ContextMenuItem? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public bool HasSelection
    {
        get => _hasSelection;
        set => SetProperty(ref _hasSelection, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (SetProperty(ref _isDarkMode, value))
            {
                // Apply the theme immediately when the toggle changes
                ThemeManager.Instance.IsDarkMode = value;
            }
        }
    }

    public bool IncludeWindowsSystemItems
    {
        get => _includeWindowsSystemItems;
        set
        {
            if (SetProperty(ref _includeWindowsSystemItems, value))
            {
                LoadContextMenuItems();
            }
        }
    }

    public bool ShowComHandlers
    {
        get => _showComHandlers;
        set
        {
            if (SetProperty(ref _showComHandlers, value))
            {
                LoadContextMenuItems();
            }
        }
    }

    #endregion

    #region Commands

    public ICommand EnableCommand { get; }
    public ICommand DisableCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand BackupCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ToggleThemeCommand { get; }

    #endregion

    #region Command Handlers

    private async void ExecuteEnable()
    {
        if (SelectedItem == null) return;

        try
        {
            StatusMessage = $"Enabling: {SelectedItem.ProgramName}...";
            var success = await _registryService.EnableItemAsync(SelectedItem);
            
            if (success)
            {
                SelectedItem.IsEnabled = true;
                StatusMessage = $"Enabled: {SelectedItem.ProgramName}";
                OnPropertyChanged(nameof(AllMenuItems)); // Refresh UI
            }
            else
            {
                StatusMessage = $"Failed to enable: {SelectedItem.ProgramName}";
                MessageBox.Show("Failed to enable item. You may need administrator rights.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Error enabling item: {ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ExecuteDisable()
    {
        if (SelectedItem == null) return;

        try
        {
            StatusMessage = $"Disabling: {SelectedItem.ProgramName}...";
            var success = await _registryService.DisableItemAsync(SelectedItem);
            
            if (success)
            {
                SelectedItem.IsEnabled = false;
                StatusMessage = $"Disabled: {SelectedItem.ProgramName}";
                OnPropertyChanged(nameof(AllMenuItems)); // Refresh UI
            }
            else
            {
                StatusMessage = $"Failed to disable: {SelectedItem.ProgramName}";
                MessageBox.Show("Failed to disable item. You may need administrator rights.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Error disabling item: {ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ExecuteDelete()
    {
        if (SelectedItem == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to permanently delete '{SelectedItem.ProgramName}'?\n\nThis will remove it from the registry and context menus.\n\nThis action cannot be undone.",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                StatusMessage = $"Deleting: {SelectedItem.ProgramName}...";
                var success = await _registryService.DeleteItemAsync(SelectedItem);
                
                if (success)
                {
                    // Remove from UI collection
                    RemoveItemFromCollection(SelectedItem);
                    StatusMessage = $"Deleted: {SelectedItem.ProgramName}";
                }
                else
                {
                    StatusMessage = $"Failed to delete: {SelectedItem.ProgramName}";
                    MessageBox.Show("Failed to delete item. You may need administrator rights.", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show($"Error deleting item: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void ExecuteBackup()
    {
        try
        {
            // Show SaveFileDialog
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Backup Context Menu Registry Entries",
                Filter = "Registry Files (*.reg)|*.reg|All Files (*.*)|*.*",
                DefaultExt = ".reg",
                FileName = $"ContextMenuBackup_{DateTime.Now:yyyyMMdd_HHmmss}.reg"
            };

            if (saveDialog.ShowDialog() == true)
            {
                StatusMessage = "Creating backup...";
                var success = await _registryService.CreateBackupAsync(AllMenuItems.ToList(), saveDialog.FileName);

                if (success)
                {
                    StatusMessage = $"Backup created: {System.IO.Path.GetFileName(saveDialog.FileName)}";
                    MessageBox.Show($"Registry backup saved successfully to:\n{saveDialog.FileName}\n\nDouble-click this file to restore the entries if needed.",
                        "Backup Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = "Failed to create backup";
                    MessageBox.Show("Failed to create registry backup.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Error creating backup: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExecuteToggleTheme()
    {
        ThemeManager.Instance.ToggleTheme();
    }


    #endregion

    #region Helper Methods

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        IsDarkMode = ThemeManager.Instance.IsDarkMode;
    }

    public void UpdateSelection()
    {
        HasSelection = SelectedItem != null;
    }

    private async void LoadContextMenuItems()
    {
        IsLoading = true;
        StatusMessage = "Discovering context menu items...";
        AllMenuItems.Clear();

        try
        {
            var items = await _registryService.DiscoverContextMenuItemsAsync(IncludeWindowsSystemItems, ShowComHandlers);
            
            foreach (var item in items)
            {
                AllMenuItems.Add(item);
            }

            StatusMessage = $"Loaded {AllMenuItems.Count} context menu items";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading items: {ex.Message}";
            MessageBox.Show($"Failed to load context menu items:\n{ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void RemoveItemFromCollection(ContextMenuItem item)
    {
        AllMenuItems.Remove(item);
    }

    #endregion
}
