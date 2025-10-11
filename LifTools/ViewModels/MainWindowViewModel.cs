using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using LifTools.Models;
using System.Linq;
using System.ComponentModel;
using Avalonia.Data;
using System.Collections.Specialized;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Text;
using Avalonia.Input;
using LifTools.Services;
using LifTools.Views;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;

namespace LifTools.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private string _selectedFilePath = string.Empty;
    private Race? _currentRace;
    private bool _isLoading;
    private TimeFormatMode _timeFormatMode = TimeFormatMode.Raw;
    private readonly SettingsService _settingsService;
    private readonly RaceSplitService _raceSplitService;
    private readonly IVersionService _versionService;
    private IList<object> _selectedRacers = new List<object>();

    public MainWindowViewModel()
    {
        _settingsService = new SettingsService();
        _raceSplitService = new RaceSplitService();
        _versionService = new VersionService();
        LoadSettings();
    }
    
    public string SelectedFilePath
    {
        get => _selectedFilePath;
        set => SetProperty(ref _selectedFilePath, value);
    }
    
    public Race? CurrentRace
    {
        get => _currentRace;
        set => SetProperty(ref _currentRace, value);
    }
    
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    
    public string Version => _versionService.GetVersion();
    
    public string Title
    {
        get
        {
            var version = _versionService.GetVersion();
            return version == "Unknown" ? "LifTools" : $"LifTools v{version}";
        }
    }
    
    public TimeFormatMode TimeFormatMode
    {
        get => _timeFormatMode;
        set
        {
            if (SetProperty(ref _timeFormatMode, value))
            {
                RefreshRacersDisplay();
                // Notify that the boolean properties have changed
                OnPropertyChanged(nameof(IsRawTimeFormat));
                OnPropertyChanged(nameof(IsFormattedTimeFormat));
                // Save settings when time format changes
                SaveSettings();
            }
        }
    }
    
    public bool IsRawTimeFormat 
    { 
        get 
        {
            return _timeFormatMode == TimeFormatMode.Raw;
        }
        set
        {
            if (value)
            {
                TimeFormatMode = TimeFormatMode.Raw;
            }
        }
    }
    
    public bool IsFormattedTimeFormat 
    { 
        get 
        {
            return _timeFormatMode == TimeFormatMode.Formatted;
        }
        set
        {
            if (value)
            {
                TimeFormatMode = TimeFormatMode.Formatted;
            }
        }
    }
    
    public ObservableCollection<Racer> Racers { get; } = new();
    
    public IList<object> SelectedRacers
    {
        get => _selectedRacers;
        set
        {
            if (SetProperty(ref _selectedRacers, value))
            {
                OnPropertyChanged(nameof(CanSplitRace));
            }
        }
    }
    
    public bool CanSplitRace => CurrentRace != null && SelectedRacers.Count > 0 && SelectedRacers.Count < Racers.Count;
    
    private void RefreshRacersDisplay()
    {
        // Update the DisplayFinishTime for each racer based on the current format mode
        foreach (var racer in Racers)
        {
            racer.DisplayFinishTime = _timeFormatMode == TimeFormatMode.Raw ? racer.FinishTimeRaw : racer.FinishTimeFormatted;
        }
    }
    
    public string FormatFinishTime(string finishTime)
    {
        if (string.IsNullOrEmpty(finishTime) || _timeFormatMode == TimeFormatMode.Raw)
        {
            return finishTime;
        }
        
        // Try to parse the finish time as seconds
        if (double.TryParse(finishTime, out double totalSeconds))
        {
            int minutes = (int)(totalSeconds / 60);
            double seconds = totalSeconds % 60;
            
            // Only show minutes if it's 1 or more
            if (minutes > 0)
            {
                return $"{minutes}:{seconds:F3}";
            }
            else
            {
                return $"{seconds:F3}";
            }
        }
        
        return finishTime; // Return original if parsing fails
    }
    
    // Clipboard commands
    [RelayCommand]
    private async Task CopySelectedCell()
    {
        // This will be implemented to copy the currently selected cell
        await CopyToClipboard("Selected cell functionality not yet implemented");
    }
    
    [RelayCommand]
    private async Task CopySelectedRow()
    {
        // This will be implemented to copy the currently selected row
        await CopyToClipboard("Selected row functionality not yet implemented");
    }
    
    [RelayCommand]
    private async Task CopyAllData()
    {
        if (Racers.Count == 0)
        {
            await CopyToClipboard("No data to copy");
            return;
        }
        
        var sb = new StringBuilder();
        
        // Add headers
        sb.AppendLine("Position,First Name,Last Name,Affiliation,Finish Time,Lane,ID,License,Delta,Reaction,Splits");
        
        // Add data rows
        foreach (var racer in Racers)
        {
            sb.AppendLine($"{racer.Position},{racer.FirstName},{racer.LastName},{racer.Affiliation},{racer.FinishTimeRaw},{racer.LineNumber},{racer.RacerId},{racer.License},{racer.DeltaTime},{racer.ReacTime},{racer.Splits}");
        }
        
        await CopyToClipboard(sb.ToString());
    }
    
    [RelayCommand]
    private async Task SplitRace()
    {
        if (CurrentRace == null || SelectedRacers.Count == 0)
            return;

        var selectedRacersList = SelectedRacers.Cast<Racer>().ToList();
        
        var dialog = new SplitDialog();
        var dialogViewModel = new SplitDialogViewModel(selectedRacersList, CurrentRace, SelectedFilePath);
        dialog.DataContext = dialogViewModel;
        
        var mainWindow = GetMainWindow();
        if (mainWindow == null)
        {
            System.Diagnostics.Debug.WriteLine("Could not get main window for dialog");
            return;
        }
        
        var result = await dialog.ShowDialog<bool?>(mainWindow);
        
        if (result == true && !string.IsNullOrEmpty(dialogViewModel.NewRaceNumber))
        {
            try
            {
                var (originalFilePath, newFilePath) = await _raceSplitService.SplitRaceAsync(
                    CurrentRace, 
                    selectedRacersList, 
                    dialogViewModel.NewRaceNumber, 
                    SelectedFilePath);
                
                // Show success message
                var originalFileName = Path.GetFileName(originalFilePath);
                var newFileName = Path.GetFileName(newFilePath);
                var message = $"Split completed successfully!\n\nFiles created:\n• {originalFileName}\n• {newFileName}";
                
                await ShowMessageAsync(mainWindow, "Split Complete", message);
            }
            catch (System.Exception ex)
            {
                var errorMessage = $"Error splitting race: {ex.Message}";
                
                await ShowMessageAsync(mainWindow, "Split Error", errorMessage);
            }
        }
    }
    
    private Window? GetMainWindow()
    {
        // This is a simplified way to get the MainWindow
        // In a real application, you might want to pass this from the view
        return Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
    }
    
    private async Task ShowMessageAsync(Window? parentWindow, string title, string message)
    {
        var messageBox = new Window
        {
            Title = title,
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            ShowInTaskbar = false
        };

        var panel = new StackPanel
        {
            Margin = new Thickness(20)
        };

        var textBlock = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var button = new Button
        {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Center,
            Width = 80
        };

        button.Click += (sender, e) => messageBox.Close();

        panel.Children.Add(textBlock);
        panel.Children.Add(button);
        messageBox.Content = panel;

        if (parentWindow != null)
        {
            await messageBox.ShowDialog(parentWindow);
        }
        else
        {
            messageBox.Show();
        }
    }
    
    public async Task CopyFinishTime(string finishTime, TopLevel? topLevel = null)
    {
        if (!string.IsNullOrEmpty(finishTime))
        {
            // Copy the exact value that was displayed in the DataGrid
            await CopyToClipboard(finishTime, topLevel);
        }
    }
    
    private async Task CopyToClipboard(string text, TopLevel? topLevel = null)
    {
        try
        {
            if (topLevel?.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(text);
                System.Diagnostics.Debug.WriteLine($"Successfully copied to clipboard: {text}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Clipboard not available. Would copy: {text}");
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error copying to clipboard: {ex.Message}");
        }
    }

    public async Task SelectFileAsync(TopLevel topLevel)
    {
        var options = new FilePickerOpenOptions
        {
            Title = "Select LIF File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("LIF Files")
                {
                    Patterns = new[] { "*.lif" }
                },
                new FilePickerFileType("All Files")
                {
                    Patterns = new[] { "*.*" }
                }
            }
        };

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        
        if (files.Count > 0)
        {
            var file = files[0];
            SelectedFilePath = file.Path.LocalPath;
            await LoadRaceAsync();
        }
    }

    private async Task LoadRaceAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath) || !File.Exists(SelectedFilePath))
            return;

        try
        {
            IsLoading = true;
            
            var dataProvider = new FileDataProvider(SelectedFilePath);
            var parser = new LifParserService(dataProvider);
            var race = await parser.ParseRaceAsync();
            
            CurrentRace = race;
            Racers.Clear();
            
            // Add racers to the collection (they will be sorted by IComparable)
            foreach (var racer in race.Racers)
            {
                // Initialize DisplayFinishTime based on current format mode
                racer.DisplayFinishTime = _timeFormatMode == TimeFormatMode.Raw ? racer.FinishTimeRaw : racer.FinishTimeFormatted;
                Racers.Add(racer);
            }
            
            // Notify that CanSplitRace might have changed
            OnPropertyChanged(nameof(CanSplitRace));
        }
        catch (System.Exception ex)
        {
            // In a real application, you'd want to show this error to the user
            System.Diagnostics.Debug.WriteLine($"Error loading LIF file: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadSettings()
    {
        var settings = _settingsService.LoadSettings();
        _timeFormatMode = settings.TimeFormatMode;
        // Notify that the boolean properties have changed
        OnPropertyChanged(nameof(IsRawTimeFormat));
        OnPropertyChanged(nameof(IsFormattedTimeFormat));
    }

    private void SaveSettings()
    {
        var settings = new AppSettings
        {
            TimeFormatMode = _timeFormatMode
        };
        _settingsService.SaveSettings(settings);
    }

}
