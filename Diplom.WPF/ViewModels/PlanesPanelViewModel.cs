using CommunityToolkit.Mvvm.ComponentModel;
using Diplom.WPF.Data;
using Diplom.WPF.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace Diplom.WPF.ViewModels;

public partial class PlanesPanelViewModel : BaseViewModel, IComboBoxItem
{
    public ObservableCollection<PlaneViewModel> Planes { get; } = [];

    [ObservableProperty]
    private PlaneViewModel? _selectedPlane;

    public string Title => "Самолеты";

    protected override void OnActivated()
    {
        base.OnActivated();

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();

        var vms = dbContext
            .Planes
            .OrderBy(e => e.Manufacturer)
            .ThenBy(e => e.Model)
            .Select(e => new PlaneViewModel
            {
                Id = e.Id,
                FuelCapacity = e.FuelCapacity,
                FuelConsumption = e.FuelConsumption,
                Manufacturer= e.Manufacturer,
                MaxSpeed = e.MaxSpeed,
                Model = e.Model,
                PassengersCapacity = e.PassengersCapacity,
                Range = e.PassengersCapacity,
                RegistrationNumber = e.RegistrationNumber,
                Type = e.Type.ToEnumValue()
            })
            .ToList();


        foreach (var item in vms)
        {
            Planes.Add(item);
        }
    }
}