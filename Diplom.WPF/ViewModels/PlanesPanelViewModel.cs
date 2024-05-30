using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Diplom.WPF.Data;
using Diplom.WPF.Infrastructure;
using Diplom.WPF.Models;
using Diplom.WPF.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace Diplom.WPF.ViewModels;

public partial class PlanesPanelViewModel : BaseViewModel, IComboBoxItem, IRecipient<PlaneAddedMessage>
{
    public ObservableCollection<PlaneViewModel> Planes { get; } = [];

    public ObservableCollection<EnumValue> PlaneTypes { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeletePlaneCommand))]
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

        foreach (var item in Enum.GetValues<PlaneType>().Select(e => e.ToEnumValue()))
        {
            PlaneTypes.Add(item);
        }

        foreach (var item in vms)
        {
            Planes.Add(item);
        }
    }

    [RelayCommand]
    private static void AddPlane()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<PlaneAddWindow>();
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeletePlane()
    {
        if (DeletePlaneCommand.IsRunning)
        {
            return;
        }

        var dialogResult = MessageBoxHelper.ShowDialogBoxYesNo("Вы уверены, что желаете удалить выбранную запись?");
        if (dialogResult == System.Windows.MessageBoxResult.No)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();
        var item = await dbContext.Planes.FirstOrDefaultAsync(e => e.Id == SelectedPlane!.Id);
        if (item is not null)
        {
            Planes.Remove(SelectedPlane!);
            dbContext.Planes.Remove(item);
            await dbContext.SaveChangesAsync();
        }
    }

    private bool CanDelete() => SelectedPlane is not null;

    public void Receive(PlaneAddedMessage message)
    {
        Planes.Add(message.ViewModel);
    }
}