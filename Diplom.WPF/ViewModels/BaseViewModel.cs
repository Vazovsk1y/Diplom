using CommunityToolkit.Mvvm.ComponentModel;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Diplom.WPF.ViewModels;

public abstract class BaseViewModel : ObservableRecipient
{
    protected ValidationResult Validate(object obj)
    {
        using var scope = App.Services.CreateScope();
        var itemType = obj.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(itemType);
        var validator = scope.ServiceProvider.GetRequiredService(validatorType);
        var validationResult = (ValidationResult)validator.GetType().GetMethod(nameof(IValidator.Validate), [itemType])!.Invoke(validator, new[] { obj })!;
        return validationResult;
    }
}