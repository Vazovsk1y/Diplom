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
}