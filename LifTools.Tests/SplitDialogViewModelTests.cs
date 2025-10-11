using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LifTools.Models;
using LifTools.ViewModels;
using Xunit;

namespace LifTools.Tests;

public class SplitDialogViewModelTests
{
    [Fact]
    public void SplitDialogViewModel_ShouldShowOnlyFileNamesNotPaths()
    {
        // Arrange
        var selectedRacers = new List<Racer>
        {
            new Racer { Position = new Position("1"), FirstName = "John", LastName = "Doe" },
            new Racer { Position = new Position("2"), FirstName = "Jane", LastName = "Smith" }
        };

        var originalRace = new Race
        {
            RaceInfo = new RaceInfo { RaceNumber = "21B" },
            Racers = selectedRacers.Concat(new[]
            {
                new Racer { Position = new Position("3"), FirstName = "Bob", LastName = "Johnson" }
            }).ToList()
        };

        var originalFilePath = Path.Combine("C:", "Users", "Test", "Documents", "21B-1-01.lif");
        var viewModel = new SplitDialogViewModel(selectedRacers, originalRace, originalFilePath);

        // Act
        viewModel.NewRaceNumber = "21C";

        // Assert
        Assert.Equal("21B-1-01-split.lif", viewModel.OriginalFileName);
        Assert.Equal("21C-1-01-split.lif", viewModel.NewFileName);
        
        // Verify no path separators are included
        Assert.DoesNotContain(Path.DirectorySeparatorChar.ToString(), viewModel.OriginalFileName);
        Assert.DoesNotContain(Path.DirectorySeparatorChar.ToString(), viewModel.NewFileName);
        Assert.DoesNotContain(Path.AltDirectorySeparatorChar.ToString(), viewModel.OriginalFileName);
        Assert.DoesNotContain(Path.AltDirectorySeparatorChar.ToString(), viewModel.NewFileName);
    }

    [Fact]
    public void SplitDialogViewModel_WithEmptyNewRaceNumber_ShouldShowEmptyFileNames()
    {
        // Arrange
        var selectedRacers = new List<Racer>
        {
            new Racer { Position = new Position("1"), FirstName = "John", LastName = "Doe" }
        };

        var originalRace = new Race
        {
            RaceInfo = new RaceInfo { RaceNumber = "21B" },
            Racers = selectedRacers
        };

        var originalFilePath = "21B-1-01.lif";
        var viewModel = new SplitDialogViewModel(selectedRacers, originalRace, originalFilePath);

        // Act
        viewModel.NewRaceNumber = "";

        // Assert
        Assert.Equal(string.Empty, viewModel.OriginalFileName);
        Assert.Equal(string.Empty, viewModel.NewFileName);
    }
}
