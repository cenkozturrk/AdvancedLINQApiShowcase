using FluentValidation;
using AdvancedLINQApiShowcase.Models;

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Customer name is required.")
            .Length(2, 100).WithMessage("Customer name must be between 2 and 100 characters.");

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");

        //RuleFor(c => c.Phone)
        //    .NotEmpty().WithMessage("Phone number is required.")
        //    .Matches(@"^\d{10}$").WithMessage("Phone number must be 10 digits.");
        //RuleFor(c => c.PhoneNumber)
        // .NotEmpty().WithMessage("Phone number is required.")
        // .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format.");
        //RuleFor(c => c.Address)
        //    .NotEmpty().WithMessage("Address is required.")
        //    .Length(10, 100).WithMessage("Address must be between 10 and 100 characters.");

    }
}
