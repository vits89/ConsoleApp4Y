using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ConsoleApp4Y.AppCore.Entities;
using ConsoleApp4Y.AppCore.Interfaces;

namespace ConsoleApp4Y.AppCore.Services
{
    public class OrdersImporter : IOrdersImporter
    {
        private const int LIST_CAPACITY = 1000;

        private readonly IMapper _mapper;

        private readonly IOrdersReader _reader;
        private readonly IOrdersSaver _saver;
        private readonly IOrderValidator _validator;

        public OrdersImporter(IMapper mapper, IOrdersReader reader, IOrdersSaver saver, IOrderValidator validator)
        {
            _mapper = mapper;
            _reader = reader;
            _saver = saver;
            _validator = validator;
        }

        public async Task<bool> TryImportAsync(IDictionary<int, ICollection<string>> errors)
        {
            var isSuccessful = true;

            var orders = new List<Order> { Capacity = LIST_CAPACITY };

            var orderNumber = 1;

            foreach (var order in _reader.Read())
            {
                if (_validator.TryValidate(order, out var validationErrors))
                {
                    orders.Add(_mapper.Map<Order>(order));

                    if (orders.Count == LIST_CAPACITY)
                    {
                        await _saver.SaveAsync(orders);

                        orders.Clear();
                    }
                }
                else
                {
                    isSuccessful = false;

                    if (!errors.ContainsKey(orderNumber))
                    {
                        errors[orderNumber] = new List<string>();
                    }

                    foreach (var error in validationErrors)
                    {
                        errors[orderNumber].Add(error);
                    }
                }

                orderNumber++;
            }

            await _saver.SaveAsync(orders);

            return isSuccessful;
        }
    }
}
