using FluentValidation;

namespace Diplom.WPF.Models.Validators;

public class FlightValidator : AbstractValidator<Flight>
{
    public FlightValidator()
    {
        RuleFor(e => e.DepartureDate).GreaterThanOrEqualTo(DateTimeOffset.Now);
        RuleFor(e => e.ArrivalDate).GreaterThan(e => e.DepartureDate);
        RuleFor(e => e.DepartureDate);
        RuleFor(e => e.CrewMembers).NotEmpty();
    }
}