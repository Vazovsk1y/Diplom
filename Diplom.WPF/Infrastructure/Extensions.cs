using Diplom.WPF.Models;
using Diplom.WPF.ViewModels;
using FluentValidation.Results;
using System.ComponentModel;
using System.Reflection;

namespace Diplom.WPF.Infrastructure;

public static class Extensions
{
    public static string ToDisplayRow(this ValidationResult validationResult)
    {
        return string.Join(Environment.NewLine, validationResult.Errors.Select(x => x.ErrorMessage));
    }

    public static EnumValue ToEnumValue(this Enum value)
    {
        FieldInfo field = value.GetType().GetField(value.ToString());
        DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();

        return new EnumValue(attribute == null ? value.ToString() : attribute.Description, value);
    }

    public static CrewMemberViewModel ToViewModel(this CrewMember crewMember)
    {
        var result = new CrewMemberViewModel()
        {
            Id = crewMember.Id,
            FullName = crewMember.FullName,
            Type = crewMember.Type.ToEnumValue(),
        };

        result.SaveState();
        return result;
    }

    public static PlaneViewModel ToViewModel(this Plane plane)
    {
        var result = new PlaneViewModel
        {
            Id = plane.Id,
            FuelCapacity = plane.FuelCapacity,
            FuelConsumption = plane.FuelConsumption,
            Manufacturer = plane.Manufacturer,
            MaxSpeed = plane.MaxSpeed,
            Model = plane.Model,
            PassengersCapacity = plane.PassengersCapacity,
            Range = plane.PassengersCapacity,
            RegistrationNumber = plane.RegistrationNumber,
            Type = plane.Type.ToEnumValue()
        };

        result.SaveState();
        return result;
    }
}