using FluentValidation;

namespace Diplom.WPF.Models.Validators;

public class PlaneValidator : AbstractValidator<Plane>
{
    public PlaneValidator()
    {
        RuleFor(e => e.FuelCapacity).GreaterThanOrEqualTo(0.01);
        RuleFor(e => e.Manufacturer).NotEmpty();
        RuleFor(e => e.FuelConsumption).GreaterThanOrEqualTo(0.01);
        RuleFor(e => e.PassengersCapacity).GreaterThanOrEqualTo(1);
        RuleFor(e => e.MaxSpeed).GreaterThanOrEqualTo(1);
        RuleFor(e => e.Model).NotEmpty();
        RuleFor(e => e.Range).GreaterThanOrEqualTo(0.01);
        RuleFor(e => e.RegistrationNumber).NotEmpty();
    }
}