using FluentValidation;

namespace Diplom.WPF.Models.Validators;

public class RouteValidator : AbstractValidator<Route>
{
    public RouteValidator()
    {
        RuleFor(e => e.To).NotEmpty().NotEqual(e => e.From);
        RuleFor(e => e.From).NotEmpty();
        RuleFor(e => e.Range).GreaterThanOrEqualTo(50);
    }
}