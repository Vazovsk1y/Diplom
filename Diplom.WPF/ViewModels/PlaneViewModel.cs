using CommunityToolkit.Mvvm.ComponentModel;

namespace Diplom.WPF.ViewModels;

public partial class PlaneViewModel : ModifiableViewModel<PlaneViewModel>
{
    public required Guid Id { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string _registrationNumber = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string _model = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string _manufacturer = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private int _passengersCapacity;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private double _range; // в километрах

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private double _maxSpeed; // в километрах в час

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private double _fuelCapacity; // в литрах

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private double _fuelConsumption; // литров на 100 км

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private EnumValue _type = null!;

    public override bool IsModified()
    {
        return RegistrationNumber != PreviousState.RegistrationNumber ||
               Model != PreviousState.Model ||
               Manufacturer != PreviousState.Manufacturer ||
               PassengersCapacity != PreviousState.PassengersCapacity ||
               Range != PreviousState.Range ||
               MaxSpeed != PreviousState.MaxSpeed ||
               FuelCapacity != PreviousState.FuelCapacity ||
               FuelConsumption != PreviousState.FuelConsumption ||
               Type != PreviousState.Type;
    }

    public override void RollBackChanges()
    {
        RegistrationNumber = PreviousState.RegistrationNumber;
        Model = PreviousState.Model;
        Manufacturer = PreviousState.Manufacturer;
        PassengersCapacity = PreviousState.PassengersCapacity;
        Range = PreviousState.Range;
        MaxSpeed = PreviousState.MaxSpeed;
        FuelCapacity = PreviousState.FuelCapacity;
        FuelConsumption = PreviousState.FuelConsumption;
        Type = PreviousState.Type;
        OnPropertyChanged(nameof(UpdatableSign));
    }
}
