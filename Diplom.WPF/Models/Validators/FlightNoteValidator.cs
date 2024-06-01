using FluentValidation;

namespace Diplom.WPF.Models.Validators;

public class FlightNoteValidator : AbstractValidator<FlightNote>
{
    public FlightNoteValidator()
    {
        RuleFor(e => e.Title).NotEmpty();
        RuleFor(e => e.Description).NotEmpty();
    }
}