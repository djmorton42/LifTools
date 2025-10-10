using System.Collections.Generic;
using System.Threading.Tasks;

namespace LifTools.Models;

public interface IDataProvider
{
    Task<IEnumerable<string>> GetLinesAsync();
}
