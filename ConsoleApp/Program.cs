using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using ConsoleApp4Y.AppCore.Services;
using ConsoleApp4Y.Infrastructure.Data;
using ConsoleApp4Y.Infrastructure.Services;

namespace ConsoleApp4Y.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var path = args[0];

                var repository = new Repository(ConfigurationManager.ConnectionStrings["Default"].ConnectionString);

                await repository.InitAsync();

                MapperConfig.Initialize();

                var ordersImporter = new OrdersImporter(MapperConfig.Configuration.CreateMapper(),
                    new OrdersFileReader(path, propertyNames => new OrderParser(propertyNames)), repository,
                    new OrderValidator());

                var errors = new Dictionary<int, ICollection<string>>();

                if (!await ordersImporter.TryImportAsync(errors))
                {
                    DisplayErrors(errors);
                }

                Console.WriteLine($"1. Количество и сумма заказов по каждому продукту за текущий месяц ({DateTime.Now.Month}):");
                DisplayDataTable(await repository.GetTask1DataAsync(DateTime.Now));

                Console.WriteLine($"2а. Продукты, которые были заказаны в текущем месяце ({DateTime.Now.Month}), но не в прошлом ({DateTime.Now.AddMonths(-1).Month}):");
                DisplayDataTable(await repository.GetTask2aDataAsync(DateTime.Now));

                Console.WriteLine($"2б. Продукты, которые были заказаны в прошлом месяце ({DateTime.Now.AddMonths(-1).Month}), но не в текущем ({DateTime.Now.Month}):");
                DisplayDataTable(await repository.GetTask2bDataAsync(DateTime.Now));

                Console.WriteLine("3. Сумма заказов и доля от общей суммы для продукта с максимальной суммой за каждый месяц:");
                DisplayDataTable(await repository.GetTask3DataAsync());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }

        private static void DisplayErrors(IDictionary<int, ICollection<string>> errors)
        {
            foreach (var error in errors)
            {
                Console.WriteLine($"Ошибки в строке {error.Key}:");

                foreach (var message in error.Value)
                {
                    Console.WriteLine($"- {message}");
                }
            }
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
