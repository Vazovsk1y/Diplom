using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Diplom.WPF.Data;
using Diplom.WPF.Infrastructure;
using Diplom.WPF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace Diplom.WPF.ViewModels;

public partial class PlaneAddViewModel : DialogViewModel
{
    public required Guid Id { get; set; }

    public ObservableCollection<EnumValue> PlaneTypes { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _registrationNumber = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _model = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _manufacturer = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private int _passengersCapacity;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private double _range; // в километрах

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private double _maxSpeed; // в километрах в час

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private double _fuelCapacity; // в литрах

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private double _fuelConsumption; // литров на 100 км

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private EnumValue _type = null!;

    public PlaneAddViewModel(IUserDialogService userDialogService) : base(userDialogService)
    {
    }

    protected override async Task Accept(object action)
    {
        if (AcceptCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();
        var plane = new Plane
        {
            FuelCapacity = FuelCapacity,
            FuelConsumption = FuelConsumption,
            Manufacturer = Manufacturer.Trim(),
            MaxSpeed = MaxSpeed,
            Model = Model.Trim(),
            PassengersCapacity = PassengersCapacity,
            Range = Range,
            RegistrationNumber = RegistrationNumber.Trim(),
            Type = Enum.Parse<PlaneType>(Type.Value.ToString()),
        };

        var validationResult = Validate(plane);
        if (!validationResult.IsValid)
        {
            MessageBoxHelper.ShowErrorBox(validationResult.ToDisplayRow());
            return;
        }

        if (await dbContext.Planes.AnyAsync(e => e.RegistrationNumber == plane.RegistrationNumber))
        {
            MessageBoxHelper.ShowErrorBox("Необходимо обеспечить уникальность регистрационного номера.");
            return;
        }

        dbContext.Planes.Add(plane);
        await dbContext.SaveChangesAsync();
        Messenger.Send(new PlaneAddedMessage(plane.ToViewModel()));
        _dialogService.CloseDialog();
    }

    protected override bool CanAccept(object parameter)
    {
        return !(
            string.IsNullOrWhiteSpace(RegistrationNumber) ||
            string.IsNullOrWhiteSpace(Model) ||
            string.IsNullOrWhiteSpace(Manufacturer) ||
            PassengersCapacity <= 0 ||
            Range <= 0.01 ||
            MaxSpeed <= 0.01 ||
            FuelCapacity <= 0.01 ||
            FuelConsumption <= 0.01 ||
            Type is null
        );
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        foreach (var item in Enum.GetValues<PlaneType>().Select(e => e.ToEnumValue()))
        {
            PlaneTypes.Add(item);
        }
    }
}

public record PlaneAddedMessage(PlaneViewModel ViewModel);
