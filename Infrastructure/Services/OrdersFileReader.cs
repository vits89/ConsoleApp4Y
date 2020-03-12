using System;
using System.Collections.Generic;
using System.IO;
using ConsoleApp4Y.AppCore.Interfaces;
using ConsoleApp4Y.AppCore.Models;

namespace ConsoleApp4Y.Infrastructure.Services
{
    public class OrdersFileReader : IOrdersReader
    {
        private readonly string _path;

        private readonly Func<string[], IOrderParser> _parserFactory;

        public OrdersFileReader(string path, Func<string[], IOrderParser> parserFactory)
        {
            _path = path;
            _parserFactory = parserFactory;
        }

        public IEnumerable<OrderValidatable> Read()
        {
            using (var stream = GetFileStream(FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                IOrderParser parser = null;

                string line;

                var isFirstLine = true;

                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;

                        parser = _parserFactory.Invoke(line.Split('\t'));
                    }
                    else
                    {
                        yield return parser.Parse(line);
                    }
                }
            }
        }

        protected virtual Stream GetFileStream(FileMode mode)
        {
            return new FileStream(_path, mode);
        }
    }
}
