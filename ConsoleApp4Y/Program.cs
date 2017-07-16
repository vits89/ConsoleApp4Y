using System;
using System.Data.SQLite;
using System.IO;

namespace ConsoleApp4Y
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (StreamReader file = new StreamReader(args[0]))
                {
                    using (SQLiteConnection conn = new SQLiteConnection("Data Source=database.db;Version=3;"))
                    {
                        conn.Open();

                        using (SQLiteCommand cmd = new SQLiteCommand(conn))
                        {
                            bool isParsed;
                            float valFloat;
                            int i = 0, valInt;
                            string line;

                            string[] colNames = null, values;

                            DateTime dateTime;

                            cmd.CommandText =
                                "DROP TABLE IF EXISTS `product`; " +
                                "CREATE TABLE `product` (" +
                                    "`id` INTEGER, " +
                                    "`name` TEXT" +
                                "); " +
                                "INSERT INTO `product` VALUES " +
                                    "(1, 'A'), " +
                                    "(2, 'B'), " +
                                    "(3, 'C'), " +
                                    "(4, 'D'), " +
                                    "(5, 'E'), " +
                                    "(6, 'F'), " +
                                    "(7, 'G');";

                            cmd.ExecuteNonQuery();

                            cmd.CommandText =
                                "DROP TABLE IF EXISTS `оrder`; " +
                                "CREATE TABLE `оrder` (" +
                                    "`id` INTEGER, " +
                                    "`dt` INTEGER, " +
                                    "`product_id` INTEGER, " +
                                    "`amount` REAL" +
                                ");";

                            cmd.ExecuteNonQuery();

                            while ((line = file.ReadLine()) != null)
                            {
                                values = line.Split(new char[] { '\t' });

                                if (i == 0)
                                {
                                    colNames = (string[])values.Clone();

                                    for (int j = 0; j < colNames.Length; j++)
                                        if (colNames[j] == "dt")
                                            values[j] = "strftime('%s', ?)";
                                        else
                                            values[j] = "?";

                                    cmd.CommandText = string.Format("INSERT INTO `оrder` ({0}) VALUES ({1});", string.Join(", ", colNames), string.Join(", ", values));

                                    cmd.Prepare();
                                }
                                else
                                {
                                    if (values.Length == colNames.Length)
                                    {
                                        isParsed = false;

                                        for (int j = 0; j < colNames.Length; j++)
                                        {
                                            switch (colNames[j])
                                            {
                                                case "id":
                                                case "product_id":
                                                    isParsed = int.TryParse(values[j], out valInt) && (valInt > 0);

                                                    if (isParsed)
                                                        cmd.Parameters.AddWithValue(null, valInt);

                                                    break;
                                                case "dt":
                                                    isParsed = DateTime.TryParse(values[j], out dateTime);

                                                    if (isParsed)
                                                        cmd.Parameters.AddWithValue(null, dateTime.ToString("s"));

                                                    break;
                                                case "amount":
                                                    isParsed = float.TryParse(values[j], out valFloat);

                                                    if (isParsed)
                                                        cmd.Parameters.AddWithValue(null, valFloat);

                                                    break;
                                            }

                                            if (!isParsed)
                                            {
                                                Console.WriteLine(string.Format("Ошибка в строке {0}: тип значения '{1}' не соответствует типу значений для столбца '{2}'.", i.ToString(), values[j], colNames[j]));

                                                break;
                                            }
                                        }

                                        if (isParsed)
                                            cmd.ExecuteNonQuery();

                                        cmd.Parameters.Clear();
                                    }
                                    else
                                        Console.WriteLine(string.Format("Ошибка в строке {0}: количество значений не совпадает с количеством столбцов таблицы.", i.ToString()));
                                }

                                i++;
                            }

                            /* cmd.CommandText =
                                "SELECT " +
                                    "datetime(`оrder`.`dt`, 'unixepoch') AS `dt`, " +
                                    "`оrder`.`amount` AS `amount`, " +
                                    "`product`.`name` AS `product` " +
                                "FROM " +
                                    "`оrder`, " +
                                    "`product` " +
                                "WHERE " +
                                    "`оrder`.`product_id` = `product`.`id` " +
                                "ORDER BY " +
                                    "`dt`;";

                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    Console.WriteLine(string.Format("{0}\t{1}\t{2}", reader["dt"], reader["amount"], reader["product"]));
                            }

                            Console.WriteLine(); */

                            // ----

                            Console.WriteLine(string.Format("1. Количество и сумма заказов по каждому продукту за текущий месяц ({0}).", DateTime.Now.Month));

                            cmd.CommandText =
                                "SELECT " +
                                    "`product`.`name` AS `product`, " +
                                    "count(`оrder`.`id`) AS `amount`, " +
                                    "total(`оrder`.`amount`) AS `sum` " +
                                "FROM " +
                                    "`оrder`, " +
                                    "`product` " +
                                "WHERE " +
                                    "`оrder`.`product_id` = `product`.`id` AND " +
                                    "strftime('%m', `оrder`.`dt`, 'unixepoch') = strftime('%m', date('now')) " +
                                "GROUP BY " +
                                    "`product` " +
                                "ORDER BY " +
                                    "`product`;";

                            Console.WriteLine("Продукт\tКоличество заказов\tСумма заказов");

                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    Console.WriteLine(string.Format("{0}\t{1}\t{2}", reader["product"], reader["amount"], reader["sum"]));
                            }

                            Console.WriteLine();

                            // ----

                            Console.WriteLine(string.Format("2а. Продукты, которые были заказаны в текущем месяце ({0}), но не в прошлом ({1}).", DateTime.Now.Month, DateTime.Now.AddMonths(-1).Month));

                            cmd.CommandText =
                                "SELECT " +
                                    "`product`.`name` AS `product` " +
                                "FROM " +
                                    "`оrder`, " +
                                    "`product` " +
                                "WHERE " +
                                    "`оrder`.`product_id` = `product`.`id` AND " +
                                    "strftime('%m', `оrder`.`dt`, 'unixepoch') = strftime('%m', date('now')) " +
                                "GROUP BY " +
                                    "`product` " +
                                "EXCEPT " +
                                "SELECT " +
                                    "`product`.`name` AS `product` " +
                                "FROM " +
                                    "`оrder`, " +
                                    "`product` " +
                                "WHERE " +
                                    "`оrder`.`product_id` = `product`.`id` AND " +
                                    "strftime('%m', `оrder`.`dt`, 'unixepoch') = strftime('%m', date('now'), '-1 month') " +
                                "GROUP BY " +
                                    "`product` " +
                                "ORDER BY " +
                                    "`product`;";

                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    Console.WriteLine(reader["product"]);
                            }

                            Console.WriteLine();

                            // ----

                            Console.WriteLine(string.Format("2б. Продукты, которые были заказаны в прошлом месяце ({0}), но не в текущем ({1}).", DateTime.Now.AddMonths(-1).Month, DateTime.Now.Month));

                            cmd.CommandText =
                                "SELECT " +
                                    "`product`.`name` AS `product` " +
                                "FROM " +
                                    "`оrder`, " +
                                    "`product` " +
                                "WHERE " +
                                    "`оrder`.`product_id` = `product`.`id` AND " +
                                    "strftime('%m', `оrder`.`dt`, 'unixepoch') = strftime('%m', date('now'), '-1 month') " +
                                "GROUP BY " +
                                    "`product` " +
                                "EXCEPT " +
                                "SELECT " +
                                    "`product`.`name` AS `product` " +
                                "FROM " +
                                    "`оrder`, " +
                                    "`product` " +
                                "WHERE " +
                                    "`оrder`.`product_id` = `product`.`id` AND " +
                                    "strftime('%m', `оrder`.`dt`, 'unixepoch') = strftime('%m', date('now')) " +
                                "GROUP BY " +
                                    "`product` " +
                                "ORDER BY " +
                                    "`product`;";

                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    Console.WriteLine(reader["product"]);
                            }

                            Console.WriteLine();

                            // ----

                            Console.WriteLine("3. Сумма заказов и доля от общей суммы для продукта с максимальной суммой за каждый месяц.");

                            cmd.CommandText =
                                "SELECT " +
                                    "`tableA`.`period` AS `period`, " +
                                    "`tableA`.`product` AS `product`, " +
                                    "`tableA`.`amount` AS `amount`, " +
                                    "round(`amount` / `tableB`.`sum` * 100, 2) AS `percentage` " +
                                "FROM " +
                                    "(" +
                                        "SELECT " +
                                            "strftime('%m-%Y', `оrder`.`dt`, 'unixepoch') AS `period`, " +
                                            "`product`.`name` AS `product`, " +
                                            "max(`оrder`.`amount`) AS `amount` " +
                                        "FROM " +
                                            "`оrder`, " +
                                            "`product` " +
                                        "WHERE " +
                                            "`оrder`.`product_id` = `product`.`id` " +
                                        "GROUP BY " +
                                            "`period`" +
                                    ") AS `tableA` " +
                                "INNER JOIN " +
                                    "(" +
                                        "SELECT " +
                                            "strftime('%m-%Y', `dt`, 'unixepoch') AS `period`, " +
                                            "total(`amount`) AS `sum` " +
                                        "FROM " +
                                            "`оrder` " +
                                        "GROUP BY " +
                                            "`period`" +
                                    ") AS `tableB` " +
                                "ON " +
                                    "`tableA`.`period` = `tableB`.`period` " +
                                "GROUP BY " +
                                    "`tableA`.`period` " +
                                "ORDER BY " +
                                    "`tableA`.`period`;";

                            Console.WriteLine("Период\tПродукт\tСумма заказов\tДоля");

                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    Console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}", reader["period"], reader["product"], reader["amount"], reader["percentage"]));
                            }
                        }
                    }
                }
            }
            catch (SQLiteException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
