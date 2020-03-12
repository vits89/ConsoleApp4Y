using ConsoleApp4Y.AppCore.Models;

namespace ConsoleApp4Y.AppCore.Interfaces
{
    public interface IOrderParser
    {
        OrderValidatable Parse(string line);
    }
}
