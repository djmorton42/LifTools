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

namespace LifTools.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private string _selectedFilePath = string.Empty;
    private Race? _currentRace;
    private bool _isLoading;

    public string Greeting { get; } = "Welcome to LifTools!";
    
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
    
    public ObservableCollection<Racer> Racers { get; } = new();
    
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
            sb.AppendLine($"{racer.Position},{racer.FirstName},{racer.LastName},{racer.Affiliation},{racer.FinishTime},{racer.LineNumber},{racer.RacerId},{racer.License},{racer.DeltaTime},{racer.ReacTime},{racer.Splits}");
        }
        
        await CopyToClipboard(sb.ToString());
    }
    
    public async Task CopyFinishTime(string finishTime, TopLevel? topLevel = null)
    {
        if (!string.IsNullOrEmpty(finishTime))
        {
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
                Racers.Add(racer);
            }
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

}
