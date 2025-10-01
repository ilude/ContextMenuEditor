using System.Windows.Controls;
using ContextMenuEditor.ViewModels;

namespace ContextMenuEditor.Views;

/// <summary>
/// Interaction logic for StartupView.xaml
/// </summary>
public partial class StartupView : UserControl
{
    public StartupView()
    {
        InitializeComponent();
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is StartupViewModel viewModel)
        {
            viewModel.UpdateSelection();
        }
    }
}
