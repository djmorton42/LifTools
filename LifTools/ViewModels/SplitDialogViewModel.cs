using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LifTools.Models;

namespace LifTools.ViewModels;

public partial class SplitDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _currentRaceNumber = string.Empty;

    [ObservableProperty]
    private int _selectedRacersCount;

    [ObservableProperty]
    private string _newRaceNumber = string.Empty;

    [ObservableProperty]
    private string _originalFileName = string.Empty;

    [ObservableProperty]
    private string _newFileName = string.Empty;

    [ObservableProperty]
    private bool _canSplit;

    private readonly List<Racer> _selectedRacers;
    private readonly Race _originalRace;
    private readonly string _originalFilePath;

    public SplitDialogViewModel(List<Racer> selectedRacers, Race originalRace, string originalFilePath)
    {
        _selectedRacers = selectedRacers ?? throw new ArgumentNullException(nameof(selectedRacers));
        _originalRace = originalRace ?? throw new ArgumentNullException(nameof(originalRace));
        _originalFilePath = originalFilePath ?? throw new ArgumentNullException(nameof(originalFilePath));

        CurrentRaceNumber = originalRace.RaceInfo.RaceNumber;
        SelectedRacersCount = selectedRacers.Count;
        
        UpdateFileNames();
        
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NewRaceNumber))
        {
            UpdateFileNames();
            UpdateCanSplit();
        }
    }

    private void UpdateFileNames()
    {
        if (string.IsNullOrEmpty(NewRaceNumber))
        {
            OriginalFileName = string.Empty;
            NewFileName = string.Empty;
            return;
        }

        var directory = Path.GetDirectoryName(_originalFilePath) ?? string.Empty;
        var originalFileName = Path.GetFileName(_originalFilePath);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var extension = Path.GetExtension(originalFileName);

        // Extract race number from filename (e.g., "21B-1-01" -> "21B")
        var parts = fileNameWithoutExtension.Split('-');
        if (parts.Length >= 1)
        {
            var currentRaceNumber = parts[0];
            var newRaceNumber = NewRaceNumber;

            OriginalFileName = $"{currentRaceNumber}-1-01-split{extension}";
            NewFileName = $"{newRaceNumber}-1-01-split{extension}";
        }
    }

    private void UpdateCanSplit()
    {
        CanSplit = !string.IsNullOrWhiteSpace(NewRaceNumber) && 
                   NewRaceNumber != CurrentRaceNumber &&
                   SelectedRacersCount > 0 &&
                   SelectedRacersCount < _originalRace.Racers.Count;
    }

    [RelayCommand]
    private void Split()
    {
        // This will be handled by the dialog's result
        // The dialog should close with a positive result
    }
}
