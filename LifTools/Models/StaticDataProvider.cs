using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LifTools.Models;

public class StaticDataProvider : IDataProvider
{
    private readonly IEnumerable<string> _lines;

    public StaticDataProvider(IEnumerable<string> lines)
    {
        _lines = lines ?? throw new ArgumentNullException(nameof(lines));
    }

    public Task<IEnumerable<string>> GetLinesAsync()
    {
        return Task.FromResult(_lines.Where(line => !string.IsNullOrWhiteSpace(line)));
    }
}
