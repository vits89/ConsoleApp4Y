using System;

namespace ConsoleApp4Y.AppCore.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime Dt { get; set; }
        public int ProductId { get; set; }
        public float Amount { get; set; }
    }
}
