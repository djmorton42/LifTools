using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifTools.Models;

public class FileDataProvider : IDataProvider
{
    private readonly string _filePath;

    public FileDataProvider(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    public async Task<IEnumerable<string>> GetLinesAsync()
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException($"LIF file not found: {_filePath}");
        }

        var lines = await File.ReadAllLinesAsync(_filePath, Encoding.UTF8);
        return lines.Where(line => !string.IsNullOrWhiteSpace(line));
    }
}
