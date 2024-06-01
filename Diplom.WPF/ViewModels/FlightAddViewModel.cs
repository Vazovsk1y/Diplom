using CommunityToolkit.Mvvm.ComponentModel;
using Diplom.WPF.Data;
using Diplom.WPF.Infrastructure;
using Diplom.WPF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Input;

namespace Diplom.WPF.ViewModels;

public partial class FlightAddViewModel : DialogViewModel
{
    public required Guid Id { get; set; }

    public ObservableCollection<PlaneInfo> Planes { get; } = [];
    public ObservableCollection<CrewMemberInfo> CrewMembers { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _number = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private DateTime _departureDate = DateTime.Today;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private TimeOnly _departureTime;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private DateTime _arrivalDate = DateTime.Today;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private TimeOnly _arrivalTime;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _from = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _to = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private PlaneInfo _plane = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private double _range;

    public ObservableCollection<CrewMemberInfo> SelectedCrewMembers { get; } = [];


    private CrewMemberInfo? _selectedCrewMember;

    public CrewMemberInfo? SelectedCrewMember
    {
        get => _selectedCrewMember;
        set {

            if (!SetProperty(ref _selectedCrewMember, value) || value is null || SelectedCrewMembers.Any(e => e.Id == value.Id))
            {
                return;
            }

            SelectedCrewMembers.Add(value);
        }
    }

    public FlightAddViewModel(IUserDialogService userDialogService) : base(userDialogService)
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
        var flight = new Flight
        {
            Range = Range,
            Number = Number.Trim(),
            DepartureDate = (DateOnly.FromDateTime(DepartureDate), DepartureTime).ToDateTimeOffset(),
            ArrivalDate = (DateOnly.FromDateTime(ArrivalDate), ArrivalTime).ToDateTimeOffset(),
            From = From.Trim(),
            To = To.Trim(),
            Status = FlightStatus.Scheduled,
            PlaneId = Plane.Id,
            Plane = dbContext.Planes.First(e => e.Id == Plane.Id),
        };
        flight.CrewMembers = [.. dbContext.CrewMembers.Where(e => SelectedCrewMembers.Select(i => i.Id).Contains(e.Id)).Select(e => new CrewMemberFlight { CrewMemberId = e.Id, FlightId = flight.Id, CrewMember = e })];

        var validationResult = Validate(flight);
        if (!validationResult.IsValid)
        {
            MessageBoxHelper.ShowErrorBox(validationResult.ToDisplayRow());
            return;
        }

        if (await dbContext.Flights.AnyAsync(e => e.Number == flight.Number))
        {
            MessageBoxHelper.ShowErrorBox("Необходимо обеспечить уникальность номера рейса.");
            return;
        }

        dbContext.Flights.Add(flight);
        await dbContext.SaveChangesAsync();
        Messenger.Send(new FlightAddedMessage(flight.ToViewModel()));
        _dialogService.CloseDialog();
    }

    [RelayCommand]
    private void RemoveCrewMember(CrewMemberInfo crewMemberInfo)
    {
        SelectedCrewMembers.Remove(crewMemberInfo);
    }

    protected override bool CanAccept(object parameter)
    {
        return !(
            string.IsNullOrWhiteSpace(Number) ||
            string.IsNullOrWhiteSpace(From) ||
            string.IsNullOrWhiteSpace(To) ||
            Plane is null ||
            DepartureDate == default ||
            DepartureTime == default ||
            ArrivalDate == default ||
            ArrivalTime == default ||
            !SelectedCrewMembers.Any()
        );
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();

        var planes = dbContext.Planes
            .Where(e => !dbContext.Flights.Where(e => e.Status != FlightStatus.Completed || e.Status != FlightStatus.Canceled).Select(e => e.PlaneId).Contains(e.Id))
            .ToList();

        foreach (var plane in planes)
        {
            Planes.Add(new PlaneInfo(plane.Id, plane.RegistrationNumber, plane.Model, plane.Manufacturer));
        }

        var crewMembers = dbContext.CrewMembers.Where(e => 
        !dbContext.CrewMemberFlights.Include(e => e.Flight).Any(c => (c.Flight.Status != FlightStatus.Completed || c.Flight.Status != FlightStatus.Canceled) && c.CrewMemberId == e.Id))
            .ToList();

        foreach (var crewMember in crewMembers)
        {
            CrewMembers.Add(new CrewMemberInfo(crewMember.Id, $"{crewMember.FullName} ({crewMember.Type.ToEnumValue().Description})"));
        }
    }
}

public record FlightAddedMessage(FlightViewModel ViewModel);
