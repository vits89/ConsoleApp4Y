using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ConsoleApp4Y.AppCore.Interfaces;
using ConsoleApp4Y.AppCore.Models;

namespace ConsoleApp4Y.AppCore.Services
{
    public class OrderValidator : IOrderValidator
    {
        public bool TryValidate(OrderValidatable order, out ICollection<string> errors)
        {
            var context = new ValidationContext(order);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(order, context, results, validateAllProperties: false);

            errors = results.Select(r => r.ErrorMessage).ToList();

            return isValid;
        }
    }
}
