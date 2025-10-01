using System;
using System.Windows;
using System.Windows.Controls;
using ContextMenuEditor.Utilities;

namespace ContextMenuEditor.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// Keep code-behind minimal - business logic belongs in the ViewModel.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Set initial title bar theme
        WindowHelper.SetTitleBarTheme(this, ThemeManager.Instance.IsDarkMode);
        
        // Subscribe to theme changes to update title bar
        ThemeManager.Instance.ThemeChanged += OnThemeChanged;
        
        // Unsubscribe when window closes
        Closed += (s, e) => ThemeManager.Instance.ThemeChanged -= OnThemeChanged;
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        // Update title bar when theme changes
        WindowHelper.UpdateTitleBarTheme(this, ThemeManager.Instance.IsDarkMode);
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Helper to notify ViewModel of selection changes
        // This is acceptable code-behind as it's purely UI interaction logic
        if (DataContext is ViewModels.MainViewModel viewModel)
        {
            viewModel.UpdateSelection();
        }
    }
}
