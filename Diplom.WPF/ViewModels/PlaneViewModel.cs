using CommunityToolkit.Mvvm.ComponentModel;

namespace Diplom.WPF.ViewModels;

public partial class PlaneViewModel : ObservableObject
{
    public required Guid Id { get; set; }

    [ObservableProperty]
    private string _registrationNumber = null!;

    [ObservableProperty]
    private string _model = null!;

    [ObservableProperty]
    private string _manufacturer = null!;

    [ObservableProperty]
    private int _passengersCapacity;

    [ObservableProperty]
    private double _range; // в километрах

    [ObservableProperty]
    private double _maxSpeed; // в километрах в час

    [ObservableProperty]
    private double _fuelCapacity; // в литрах

    [ObservableProperty]
    private double _fuelConsumption; // литров на 100 км

    [ObservableProperty]
    private EnumValue _type = null!;
}
