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
    [NotifyCanExecuteChangedFor(nameof(RollbackChangesCommand))]
    [NotifyCanExecuteChangedFor(nameof(UpdatePlaneCommand))]
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
            .Select(e => e.ToViewModel())
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

    [RelayCommand(CanExecute = nameof(CanDeleteOrRollbackChangesOrUpdate))]
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
            if (await dbContext.Flights.AnyAsync(e => e.PlaneId == item.Id))
            {
                MessageBoxHelper.ShowErrorBox("Удаление невозможно, присутствуют связанные данные.");
                return;
            }

            Planes.Remove(SelectedPlane!);
            dbContext.Planes.Remove(item);
            await dbContext.SaveChangesAsync();
        }
    }

    private bool CanDeleteOrRollbackChangesOrUpdate() => SelectedPlane is not null;


    [RelayCommand(CanExecute = nameof(CanDeleteOrRollbackChangesOrUpdate))]
    private void RollbackChanges()
    {
        if (!SelectedPlane!.IsModified())
        {
            return;
        }
        SelectedPlane.RollBackChanges();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteOrRollbackChangesOrUpdate))]
    private async Task UpdatePlane()
    {
        if (UpdatePlaneCommand.IsRunning)
        {
            return;
        }

        if (!SelectedPlane!.IsModified())
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();
        var plane = dbContext.Planes.First(e => e.Id == SelectedPlane!.Id);

        plane.FuelCapacity = SelectedPlane!.FuelCapacity;
        plane.FuelConsumption = SelectedPlane.FuelConsumption;
        plane.Manufacturer = SelectedPlane.Manufacturer.Trim();
        plane.MaxSpeed = SelectedPlane.MaxSpeed;
        plane.Model = SelectedPlane.Model.Trim();
        plane.PassengersCapacity = SelectedPlane.PassengersCapacity;
        plane.Range = SelectedPlane.Range;
        plane.RegistrationNumber = SelectedPlane.RegistrationNumber.Trim();
        plane.Type = Enum.Parse<PlaneType>(SelectedPlane.Type.Value.ToString());

        if (await dbContext.Planes.AnyAsync(e => e.Id != plane.Id && e.RegistrationNumber == plane.RegistrationNumber))
        {
            MessageBoxHelper.ShowErrorBox("Необходимо обеспечить уникальность регистрационного номера.");
            return;
        }

        var validationResult = Validate(plane);
        if (!validationResult.IsValid)
        {
            MessageBoxHelper.ShowErrorBox(validationResult.ToDisplayRow());
            return;
        }

        await dbContext.SaveChangesAsync();
        SelectedPlane.SaveState();
    }

    public void Receive(PlaneAddedMessage message)
    {
        Planes.Add(message.ViewModel);
    }
}