using AdvancedLINQApiShowcase.Models;
using FluentValidation;

namespace AdvancedLINQApiShowcase.Validation
{
    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            RuleFor(o => o.Name)
           .NotEmpty().WithMessage("Order name is required.")
           .Length(2, 50).WithMessage("Order name must be between 2 and 50 characters.");

            RuleFor(o => o.OrderDate)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Order date cannot be in the future.");

            RuleFor(o => o.CustomerId)
                .GreaterThan(0).WithMessage("A valid CustomerId is required.");
        }
    }
}
