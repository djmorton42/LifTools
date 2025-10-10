using Avalonia.Controls;
using LifTools.ViewModels;
using LifTools.Models;
using System.Linq;
using System;

namespace LifTools.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }

        private async void BrowseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.SelectFileAsync(this);
            }
        }

    private async void CopyFinishTime_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string finishTime && DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.CopyFinishTime(finishTime, this);
        }
    }

    private void RawTimeFormat_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Console.WriteLine("RawTimeFormat_Click called");
        if (DataContext is MainWindowViewModel viewModel)
        {
            Console.WriteLine("Setting TimeFormatMode to Raw");
            viewModel.TimeFormatMode = TimeFormatMode.Raw;
        }
    }

    private void FormattedTimeFormat_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Console.WriteLine("FormattedTimeFormat_Click called");
        if (DataContext is MainWindowViewModel viewModel)
        {
            Console.WriteLine("Setting TimeFormatMode to Formatted");
            viewModel.TimeFormatMode = TimeFormatMode.Formatted;
        }
    }

}