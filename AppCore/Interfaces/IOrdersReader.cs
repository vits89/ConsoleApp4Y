using System.Collections.Generic;
using ConsoleApp4Y.AppCore.Models;

namespace ConsoleApp4Y.AppCore.Interfaces
{
    public interface IOrdersReader
    {
        IEnumerable<OrderValidatable> Read();
    }
}
