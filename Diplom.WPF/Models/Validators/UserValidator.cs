using FluentValidation;

namespace Diplom.WPF.Models.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(e => e.UserName).NotEmpty();
        RuleFor(e => e.PasswordHash).NotEmpty();
        RuleFor(e => e.NormalizedUserName).NotEmpty();
    }
}