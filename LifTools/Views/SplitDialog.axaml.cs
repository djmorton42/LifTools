using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LifTools.Views;

public partial class SplitDialog : Window
{
    public SplitDialog()
    {
        InitializeComponent();
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
    
    private void SplitButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }
}
