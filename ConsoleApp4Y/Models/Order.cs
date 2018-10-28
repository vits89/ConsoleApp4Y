using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConsoleApp4Y.Models
{
    public class Order : IValidatableObject
    {
        public DateTime? Dt { get; set; }
        public int? ProductId { get; set; }
        public float? Amount { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            var results = new List<ValidationResult>();

            if (!Dt.HasValue)
            {
                results.Add(new ValidationResult("Не заданы дата и время"));
            }

            if (!ProductId.HasValue)
            {
                results.Add(new ValidationResult("Не задан идентификатор продукта"));
            }
            else if (ProductId.Value < 1)
            {
                results.Add(new ValidationResult("Неверное значение идентификатора продукта"));
            }

            if (!Amount.HasValue)
            {
                results.Add(new ValidationResult("Не задано количество"));
            }
            else if (Amount.Value <= 0)
            {
                results.Add(new ValidationResult("Неверное значение количества"));
            }

            return results;
        }
    }
}
