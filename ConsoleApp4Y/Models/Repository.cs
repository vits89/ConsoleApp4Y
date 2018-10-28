using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp4Y.Models
{
    public class Repository : IRepository
    {
        private readonly string _connectionString;

        public Repository(string connectionString) => _connectionString = connectionString;

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
                CommandText = "INSERT INTO [order] (dt, product_id, amount) VALUES (strftime('%s', @dt), @product_id, @amount)"
            };

            SQLiteTransaction transaction = null;

            try
            {
                await connection.OpenAsync();

                transaction = connection.BeginTransaction();

                command.Transaction = transaction;

                foreach (var order in orders)
                {
                    command.Parameters.AddWithValue("@dt", order.Dt.Value.ToString("s"));
                    command.Parameters.AddWithValue("@product_id", order.ProductId.Value);
                    command.Parameters.AddWithValue("@amount", order.Amount.Value);

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

        public async Task<DataTable> GetOrdersAsync()
        {
            var query =
                "SELECT datetime(o.dt, 'unixepoch') AS DateTime, p.name AS Product, o.amount AS Amount " +
                "FROM [order] o " +
                "JOIN product p ON p.id = o.product_id " +
                "ORDER BY DateTime";

            return await GetDataAsync(query);
        }

        public async Task<DataTable> GetDataAsync(string query, Dictionary<string, object> parameters = null)
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
    }
}
