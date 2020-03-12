using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ConsoleApp4Y.AppCore.Entities;

namespace ConsoleApp4Y.AppCore.Interfaces
{
    public interface IRepository
    {
        Task InitAsync();

        Task AddOrdersAsync(IEnumerable<Order> orders);
        Task<DataTable> GetOrdersAsync();

        Task<DataTable> GetTask1DataAsync(DateTime dateTime);
        Task<DataTable> GetTask2aDataAsync(DateTime dateTime);
        Task<DataTable> GetTask2bDataAsync(DateTime dateTime);
        Task<DataTable> GetTask3DataAsync();
    }
}
