using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp4Y.AppCore.Entities;
using ConsoleApp4Y.AppCore.Interfaces;

namespace ConsoleApp4Y.Infrastructure.Data
{
    public class Repository : IRepository, IOrdersSaver
    {
        private readonly string _connectionString;

        public Repository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task InitAsync()
        {
            var connection = new SQLiteConnection(_connectionString);
            var command = new SQLiteCommand(connection);

            command.CommandText =
                "DROP TABLE IF EXISTS product; " +
                "CREATE TABLE product (" +
                    "id INTEGER PRIMARY KEY, " +
                    "name TEXT NOT NULL" +
                "); " +
                "INSERT INTO product (name) VALUES " +
                    "('A'), " +
                    "('B'), " +
                    "('C'), " +
                    "('D'), " +
                    "('E'), " +
                    "('F'), " +
                    "('G'); " +
                "DROP TABLE IF EXISTS [order]; " +
                "CREATE TABLE [order] (" +
                    "id INTEGER PRIMARY KEY, " +
                    "dt INTEGER NOT NULL, " +
                    "product_id INTEGER REFERENCES product (id), " +
                    "amount REAL NOT NULL" +
                ")";

            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task AddOrdersAsync(IEnumerable<Order> orders)
        {
            if ((orders?.Count() ?? 0) == 0) return;

            var connection = new SQLiteConnection(_connectionString);
            var command = new SQLiteCommand(connection)
            {
                CommandText =
                    "INSERT INTO [order] (id, dt, product_id, amount) " +
                    "VALUES (@id, strftime('%s', @dt), @product_id, @amount)"
            };

            SQLiteTransaction transaction = null;

            try
            {
                await connection.OpenAsync();

                transaction = connection.BeginTransaction();

                command.Transaction = transaction;

                foreach (var order in orders)
                {
                    command.Parameters.AddWithValue("@id", order.Id);
                    command.Parameters.AddWithValue("@dt", order.Dt.ToString("s"));
                    command.Parameters.AddWithValue("@product_id", order.ProductId);
                    command.Parameters.AddWithValue("@amount", order.Amount);

                    await command.ExecuteNonQueryAsync();

                    command.Parameters.Clear();
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction?.Rollback();

                throw e;
            }
            finally
            {
                connection.Close();
            }
        }

        public Task<DataTable> GetOrdersAsync()
        {
            var query =
                "SELECT o.id AS ID, datetime(o.dt, 'unixepoch') AS DateTime, p.name AS Product, o.amount AS Amount " +
                "FROM [order] o " +
                "JOIN product p ON p.id = o.product_id";

            return GetDataAsync(query);
        }

        public Task<DataTable> GetTask1DataAsync(DateTime dateTime)
        {
            var query =
                "SELECT p.name AS Product, count(o.id) AS Count, total(o.amount) AS [Total amount] " +
                "FROM product p " +
                "JOIN [order] o ON o.product_id = p.id " +
                "WHERE strftime('%m', o.dt, 'unixepoch') = @month " +
                "GROUP BY p.name " +
                "ORDER BY p.name";
            var parameters = new Dictionary<string, object> { ["@month"] = dateTime.ToString("MM") };

            return GetDataAsync(query, parameters);
        }

        public Task<DataTable> GetTask2aDataAsync(DateTime dateTime)
        {
            var query =
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
            var parameters = new Dictionary<string, object>
            {
                ["@cur_month"] = dateTime.ToString("MM"),
                ["@prev_month"] = dateTime.AddMonths(-1).ToString("MM")
            };

            return GetDataAsync(query, parameters);
        }

        public Task<DataTable> GetTask2bDataAsync(DateTime dateTime)
        {
            var query =
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
            var parameters = new Dictionary<string, object>
            {
                ["@cur_month"] = dateTime.ToString("MM"),
                ["@prev_month"] = dateTime.AddMonths(-1).ToString("MM")
            };

            return GetDataAsync(query, parameters);
        }

        public Task<DataTable> GetTask3DataAsync()
        {
            var query =
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

            return GetDataAsync(query);
        }

        private async Task<DataTable> GetDataAsync(string query, IDictionary<string, object> parameters = null)
        {
            var connection = new SQLiteConnection(_connectionString);
            var command = new SQLiteCommand(query, connection);

            if ((parameters?.Count ?? 0) > 0)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
            }

            var dataTable = new DataTable();

            try
            {
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    dataTable.Load(reader);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                connection.Close();
            }

            return dataTable;
        }

        Task IOrdersSaver.SaveAsync(IEnumerable<Order> orders)
        {
            return AddOrdersAsync(orders);
        }
    }
}
