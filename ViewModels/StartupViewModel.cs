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
/// ViewModel for managing Windows startup items.
/// Follows MVVM pattern and SOLID principles.
/// </summary>
public class StartupViewModel : ViewModelBase
{
    private readonly IStartupService _startupService;
    private StartupItem? _selectedItem;
    private bool _hasSelection;
    private string _statusMessage = "Loading...";
    private bool _isLoading;

    public StartupViewModel() : this(new StartupService())
    {
    }

    public StartupViewModel(IStartupService startupService)
    {
        _startupService = startupService;

        // Initialize collection
        StartupItems = new ObservableCollection<StartupItem>();

        // Initialize commands
        EnableCommand = new RelayCommand(ExecuteEnable, () => HasSelection && !IsLoading);
        DisableCommand = new RelayCommand(ExecuteDisable, () => HasSelection && !IsLoading);
        DeleteCommand = new RelayCommand(ExecuteDelete, () => HasSelection && !IsLoading);
        BackupCommand = new RelayCommand(ExecuteBackup, () => !IsLoading);
        RefreshCommand = new RelayCommand(LoadStartupItems, () => !IsLoading);

        // Load initial data
        LoadStartupItems();
    }

    #region Properties

    public ObservableCollection<StartupItem> StartupItems { get; }

    public StartupItem? SelectedItem
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

    #endregion

    #region Commands

    public ICommand EnableCommand { get; }
    public ICommand DisableCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand BackupCommand { get; }
    public ICommand RefreshCommand { get; }

    #endregion

    #region Command Handlers

    private async void ExecuteEnable()
    {
        if (SelectedItem == null) return;

        try
        {
            StatusMessage = $"Enabling: {SelectedItem.Name}...";
            var success = await _startupService.EnableItemAsync(SelectedItem);
            
            if (success)
            {
                SelectedItem.IsEnabled = true;
                StatusMessage = $"Enabled: {SelectedItem.Name}";
                OnPropertyChanged(nameof(StartupItems)); // Refresh UI
            }
            else
            {
                StatusMessage = $"Failed to enable: {SelectedItem.Name}";
                MessageBox.Show("Failed to enable startup item. You may need administrator rights.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Error enabling startup item: {ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ExecuteDisable()
    {
        if (SelectedItem == null) return;

        try
        {
            StatusMessage = $"Disabling: {SelectedItem.Name}...";
            var success = await _startupService.DisableItemAsync(SelectedItem);
            
            if (success)
            {
                SelectedItem.IsEnabled = false;
                StatusMessage = $"Disabled: {SelectedItem.Name}";
                OnPropertyChanged(nameof(StartupItems)); // Refresh UI
            }
            else
            {
                StatusMessage = $"Failed to disable: {SelectedItem.Name}";
                MessageBox.Show("Failed to disable startup item. You may need administrator rights.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Error disabling startup item: {ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ExecuteDelete()
    {
        if (SelectedItem == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to permanently delete '{SelectedItem.Name}'?\n\nThis will remove it from Windows startup.\n\nThis action cannot be undone.",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                StatusMessage = $"Deleting: {SelectedItem.Name}...";
                var success = await _startupService.DeleteItemAsync(SelectedItem);
                
                if (success)
                {
                    // Remove from UI collection
                    StartupItems.Remove(SelectedItem);
                    StatusMessage = $"Deleted: {SelectedItem.Name}";
                }
                else
                {
                    StatusMessage = $"Failed to delete: {SelectedItem.Name}";
                    MessageBox.Show("Failed to delete startup item. You may need administrator rights.", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show($"Error deleting startup item: {ex.Message}", 
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
                Title = "Backup Startup Registry Entries",
                Filter = "Registry Files (*.reg)|*.reg|All Files (*.*)|*.*",
                DefaultExt = ".reg",
                FileName = $"StartupBackup_{DateTime.Now:yyyyMMdd_HHmmss}.reg"
            };

            if (saveDialog.ShowDialog() == true)
            {
                StatusMessage = "Creating backup...";
                var success = await _startupService.CreateBackupAsync(StartupItems.ToList(), saveDialog.FileName);

                if (success)
                {
                    StatusMessage = $"Backup created: {System.IO.Path.GetFileName(saveDialog.FileName)}";
                    MessageBox.Show($"Startup registry backup saved successfully to:\n{saveDialog.FileName}\n\nDouble-click this file to restore the entries if needed.",
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

    #endregion

    #region Helper Methods

    public void UpdateSelection()
    {
        HasSelection = SelectedItem != null;
    }

    private async void LoadStartupItems()
    {
        IsLoading = true;
        StatusMessage = "Discovering startup items...";
        StartupItems.Clear();

        try
        {
            var items = await _startupService.DiscoverStartupItemsAsync();
            
            foreach (var item in items)
            {
                StartupItems.Add(item);
            }

            StatusMessage = $"Loaded {StartupItems.Count} startup items";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading items: {ex.Message}";
            MessageBox.Show($"Failed to load startup items:\n{ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion
}
