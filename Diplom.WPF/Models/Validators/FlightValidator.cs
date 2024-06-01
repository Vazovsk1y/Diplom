using FluentValidation;

namespace Diplom.WPF.Models.Validators;

public class FlightValidator : AbstractValidator<Flight>
{
    public FlightValidator()
    {
        RuleFor(e => e.To).NotEmpty();
        RuleFor(e => e.From).NotEmpty().NotEqual(e => e.To, StringComparer.OrdinalIgnoreCase);
        RuleFor(e => e.ArrivalDate).GreaterThan(e => e.DepartureDate);
        RuleFor(e => e.DepartureDate);
        RuleFor(e => e.Range).GreaterThanOrEqualTo(1);
        RuleFor(e => e.CrewMembers).NotEmpty();
    }
}