using System;
using ConsoleApp4Y.AppCore.Interfaces;
using ConsoleApp4Y.AppCore.Models;

namespace ConsoleApp4Y.AppCore.Services
{
    public class OrderParser : IOrderParser
    {
        private readonly string[] _propertyNames;

        public OrderParser(string[] propertyNames)
        {
            _propertyNames = propertyNames;
        }

        public OrderValidatable Parse(string line)
        {
            var order = new OrderValidatable();

            var values = line.Split('\t');

            for (var i = 0; i < _propertyNames.Length; i++)
            {
                if (i == values.Length) break;

                switch (_propertyNames[i])
                {
                    case "id":
                        if (int.TryParse(values[i], out var id))
                        {
                            order.Id = id;
                        }

                        break;
                    case "dt":
                        if (DateTime.TryParse(values[i], out var dateTime))
                        {
                            order.Dt = dateTime;
                        }

                        break;
                    case "product_id":
                        if (int.TryParse(values[i], out var productId))
                        {
                            order.ProductId = productId;
                        }

                        break;
                    case "amount":
                        if (float.TryParse(values[i], out var amount))
                        {
                            order.Amount = amount;
                        }

                        break;
                }
            }

            return order;
        }
    }
}
