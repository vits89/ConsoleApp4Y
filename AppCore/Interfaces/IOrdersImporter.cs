using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp4Y.AppCore.Interfaces
{
    public interface IOrdersImporter
    {
        Task<bool> TryImportAsync(IDictionary<int, ICollection<string>> errors);
    }
}
