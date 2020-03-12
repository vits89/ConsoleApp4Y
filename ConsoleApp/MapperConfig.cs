using AutoMapper;
using ConsoleApp4Y.AppCore.Entities;
using ConsoleApp4Y.AppCore.Models;

namespace ConsoleApp4Y.ConsoleApp
{
    public static class MapperConfig
    {
        public static MapperConfiguration Configuration { get; private set; }

        public static void Initialize()
        {
            Configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OrderValidatable, Order>();
            });
        }
    }
}
