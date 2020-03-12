using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConsoleApp4Y.AppCore.Models
{
    public class OrderValidatable : IValidatableObject
    {
        public int? Id { get; set; }
        public DateTime? Dt { get; set; }
        public int? ProductId { get; set; }
        public float? Amount { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            var results = new List<ValidationResult>();

            if (!Id.HasValue)
            {
                results.Add(new ValidationResult("Не задан идентификатор заказа"));
            }
            else if (Id.Value < 1)
            {
                results.Add(new ValidationResult("Неверное значение идентификатора заказа"));
            }

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
