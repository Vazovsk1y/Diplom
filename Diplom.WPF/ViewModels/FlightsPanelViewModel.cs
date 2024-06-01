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

public partial class FlightsPanelViewModel : BaseViewModel, 
    IComboBoxItem, 
    IRecipient<FlightAddedMessage>,
    IRecipient<FlightNoteAddedMessage>
{
    public ObservableCollection<FlightViewModel> Flights { get; } = [];

    public ObservableCollection<EnumValue> Statuses { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteFlightCommand))]
    [NotifyCanExecuteChangedFor(nameof(RollbackChangesCommand))]
    [NotifyCanExecuteChangedFor(nameof(UpdateFlightCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddNoteCommand))]
    private FlightViewModel? _selectedFlight;

    public string Title => "Рейсы";

    protected override void OnActivated()
    {
        base.OnActivated();

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();

        var vms = dbContext
            .Flights
            .Include(e => e.CrewMembers)
            .ThenInclude(e => e.CrewMember)
            .Include(e => e.Notes)
            .Include(e => e.Plane)
            .OrderBy(e => e.DepartureDate)
            .Select(e => e.ToViewModel())
            .ToList();

        foreach (var item in Enum.GetValues<FlightStatus>().Select(e => e.ToEnumValue()))
        {
            Statuses.Add(item);
        }

        foreach (var item in vms)
        {
            Flights.Add(item);
        }
    }

    [RelayCommand]
    private static void AddFlight()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<FlightAddWindow>();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteOrRollbackChangesOrUpdateOrAddNote))]
    private async Task DeleteFlight()
    {
        if (DeleteFlightCommand.IsRunning)
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
        var item = await dbContext.Flights.FirstOrDefaultAsync(e => e.Id == SelectedFlight!.Id);
        if (item is not null)
        {
            Flights.Remove(SelectedFlight!);
            dbContext.Flights.Remove(item);
            await dbContext.SaveChangesAsync();
        }
    }

    private bool CanDeleteOrRollbackChangesOrUpdateOrAddNote() => SelectedFlight is not null;


    [RelayCommand(CanExecute = nameof(CanDeleteOrRollbackChangesOrUpdateOrAddNote))]
    private void RollbackChanges()
    {
        if (!SelectedFlight!.IsModified())
        {
            return;
        }
        SelectedFlight.RollBackChanges();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteOrRollbackChangesOrUpdateOrAddNote))]
    private async Task UpdateFlight()
    {
        if (UpdateFlightCommand.IsRunning)
        {
            return;
        }

        if (!SelectedFlight!.IsModified())
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();
        var flight = dbContext.Flights.Include(e => e.CrewMembers).First(e => e.Id == SelectedFlight!.Id);

        flight.From = SelectedFlight.From.Trim();
        flight.To = SelectedFlight.To.Trim();
        flight.Status = Enum.Parse<FlightStatus>(SelectedFlight.Status.Value.ToString());
        flight.ArrivalDate = (SelectedFlight.ArrivalDate, SelectedFlight.ArrivalTime).ToDateTimeOffset();
        flight.DepartureDate = (SelectedFlight.DepartureDate, SelectedFlight.DepartureTime).ToDateTimeOffset();
        flight.Range = SelectedFlight.Range;
        flight.Number = SelectedFlight.Number.Trim();

        if (await dbContext.Flights.AnyAsync(e => e.Id != flight.Id && e.Number == flight.Number))
        {
            MessageBoxHelper.ShowErrorBox("Необходимо обеспечить уникальность номера.");
            return;
        }

        var validationResult = Validate(flight);
        if (!validationResult.IsValid)
        {
            MessageBoxHelper.ShowErrorBox(validationResult.ToDisplayRow());
            return;
        }

        await dbContext.SaveChangesAsync();
        SelectedFlight.SaveState();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteOrRollbackChangesOrUpdateOrAddNote))]
    private void AddNote()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        var vm = scope.ServiceProvider.GetRequiredService<FlightNoteAddViewModel>();
        vm.IsActive = true;
        vm.FlightId = SelectedFlight!.Id;
        dialogService.ShowDialog<FlightNoteAddWindow, FlightNoteAddViewModel>(vm);
    }

    public void Receive(FlightAddedMessage message)
    {
        Flights.Add(message.ViewModel);
    }

    public void Receive(FlightNoteAddedMessage message)
    {
        if (message.FlightId != SelectedFlight?.Id)
        {
            throw new ApplicationException("Выбранный рейс не соответствует рейсу из добавленной заметки.");
        }

        SelectedFlight!.FlightNotes.Add(message.ViewModel);
    }
}