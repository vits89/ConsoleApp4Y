using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using ConsoleApp4Y.Models;

namespace ConsoleApp4Y
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const int LIST_CAPACITY = 1000;

            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["Sqlite"].ConnectionString;

                IRepository repository = new Repository(connectionString);

                await repository.InitAsync();

                var orders = new List<Order> { Capacity = LIST_CAPACITY };

                using (var reader = new StreamReader(args[0]))
                {
                    string line;
                    string[] columnNames = null;

                    var lineNumber = 1;

                    while ((line = reader.ReadLine()) != null)
                    {
                        var values = line.Split('\t');

                        if (lineNumber == 1)
                        {
                            columnNames = (string[])values.Clone();
                        }
                        else
                        {
                            var order = new Order();

                            for (var i = 0; i < columnNames.Length; i++)
                            {
                                if (i == values.Length) break;

                                switch (columnNames[i])
                                {
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

                            var context = new ValidationContext(order);
                            var results = new List<ValidationResult>();

                            if (Validator.TryValidateObject(order, context, results, validateAllProperties: false))
                            {
                                orders.Add(order);

                                if (orders.Count == LIST_CAPACITY)
                                {
                                    await repository.AddOrdersAsync(orders);

                                    orders.Clear();
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Ошибки в строке {lineNumber}:");

                                foreach (var result in results)
                                {
                                    Console.WriteLine($"- {result.ErrorMessage}");
                                }
                            }
                        }

                        lineNumber++;
                    }
                }

                await repository.AddOrdersAsync(orders);

                //DisplayDataTable(await repository.GetOrdersAsync());

                Console.WriteLine($"1. Количество и сумма заказов по каждому продукту за текущий месяц ({DateTime.Now.Month}).");

                var query =
                    "SELECT p.name AS Product, count(o.id) AS Number, total(o.amount) AS [Total amount] " +
                    "FROM product p " +
                    "JOIN [order] o ON o.product_id = p.id " +
                    "WHERE strftime('%m', o.dt, 'unixepoch') = @cur_month " +
                    "GROUP BY p.name " +
                    "ORDER BY p.name";
                var parameters = new Dictionary<string, object> { ["@cur_month"] = DateTime.Now.ToString("MM") };

                DisplayDataTable(await repository.GetDataAsync(query, parameters));

                Console.WriteLine($"2а. Продукты, которые были заказаны в текущем месяце ({DateTime.Now.Month}), но не в прошлом ({DateTime.Now.AddMonths(-1).Month}).");

                query =
                    "SELECT p.name AS Product " +
                    "FROM product p " +
                    "JOIN [order] o ON o.product_id = p.id " +
                    "WHERE strftime('%m', o.dt, 'unixepoch') = @cur_month " +
                    "GROUP BY p.name " +
                    "EXCEPT " +
                    "SELECT p.name AS Product " +
                    "FROM product p " +
                    "JOIN [order] o ON o.product_id = p.id " +
                    "WHERE strftime('%m', o.dt, 'unixepoch') = @prev_month " +
                    "GROUP BY p.name " +
                    "ORDER BY p.name";

                parameters.Add("@prev_month", DateTime.Now.AddMonths(-1).ToString("MM"));

                DisplayDataTable(await repository.GetDataAsync(query, parameters));

                Console.WriteLine($"2б. Продукты, которые были заказаны в прошлом месяце ({DateTime.Now.AddMonths(-1).Month}), но не в текущем ({DateTime.Now.Month}).");

                query =
                    "SELECT p.name AS Product " +
                    "FROM product p " +
                    "JOIN [order] o ON o.product_id = p.id " +
                    "WHERE strftime('%m', o.dt, 'unixepoch') = @prev_month " +
                    "GROUP BY p.name " +
                    "EXCEPT " +
                    "SELECT p.name AS Product " +
                    "FROM product p " +
                    "JOIN [order] o ON o.product_id = p.id " +
                    "WHERE strftime('%m', o.dt, 'unixepoch') = @cur_month " +
                    "GROUP BY p.name " +
                    "ORDER BY p.name";

                DisplayDataTable(await repository.GetDataAsync(query, parameters));

                Console.WriteLine("3. Сумма заказов и доля от общей суммы для продукта с максимальной суммой за каждый месяц.");

                query =
                    "SELECT t1.period AS Period, t1.product AS Product, t1.amount AS Amount, round(t1.amount / t2.total_amount * 100, 2) AS Percentage " +
                    "FROM (" +
                        "SELECT strftime('%Y-%m', o.dt, 'unixepoch') AS period, p.name AS product, max(o.amount) AS amount " +
                        "FROM product p " +
                        "JOIN [order] o ON o.product_id = p.id " +
                        "GROUP BY period" +
                    ") t1 " +
                    "JOIN (" +
                        "SELECT strftime('%Y-%m', dt, 'unixepoch') AS period, total(amount) AS total_amount " +
                        "FROM [order] " +
                        "GROUP BY period" +
                    ") t2 ON t1.period = t2.period " +
                    "GROUP BY t1.period " +
                    "ORDER BY t1.period";

                DisplayDataTable(await repository.GetDataAsync(query));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }

        private static void DisplayDataTable(DataTable dataTable)
        {
            for (var i = 0; i < dataTable.Columns.Count; i++)
            {
                Console.Write($"{dataTable.Columns[i].ColumnName}\t");
            }

            Console.WriteLine();

            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                for (var j = 0; j < dataTable.Columns.Count; j++)
                {
                    Console.Write($"{dataTable.Rows[i][j]}\t");
                }

                Console.WriteLine();
            }
        }
    }
}
