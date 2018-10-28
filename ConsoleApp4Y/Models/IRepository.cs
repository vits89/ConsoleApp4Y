using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ConsoleApp4Y.Models
{
    public interface IRepository
    {
        Task InitAsync();
        Task AddOrdersAsync(IEnumerable<Order> orders);
        Task<DataTable> GetOrdersAsync();
        Task<DataTable> GetDataAsync(string query, Dictionary<string, object> parameters = null);
    }
}
