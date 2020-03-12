using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApp4Y.AppCore.Entities;

namespace ConsoleApp4Y.AppCore.Interfaces
{
    public interface IOrdersSaver
    {
        Task SaveAsync(IEnumerable<Order> orders);
    }
}
