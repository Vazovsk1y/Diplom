using FluentValidation;

namespace Diplom.WPF.Models.Validators;

public class CrewMemberValidator : AbstractValidator<CrewMember>
{
    public CrewMemberValidator()
    {
        RuleFor(e => e.FullName).NotEmpty();
    }
}