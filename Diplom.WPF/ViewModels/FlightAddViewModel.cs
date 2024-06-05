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

    public ObservableCollection<RouteInfo> Routes { get; } = [];

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
    private PlaneInfo _plane = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private RouteInfo _selectedRoute = null!;

    public ObservableCollection<CrewMemberInfo> SelectedCrewMembers { get; } = [];


    private CrewMemberInfo? _selectedCrewMember;

    public CrewMemberInfo? SelectedCrewMember
    {
        get => _selectedCrewMember;
        set
        {

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
            RouteId = SelectedRoute.Id,
            Number = Number.Trim(),
            DepartureDate = (DateOnly.FromDateTime(DepartureDate), DepartureTime).ToDateTimeOffset(),
            ArrivalDate = (DateOnly.FromDateTime(ArrivalDate), ArrivalTime).ToDateTimeOffset(),
            Status = FlightStatus.Scheduled,
            PlaneId = Plane.Id,
            Plane = dbContext.Planes.First(e => e.Id == Plane.Id),
            Route = dbContext.Routes.First(e => e.Id == SelectedRoute.Id),
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
            Plane is null ||
            SelectedRoute is null ||
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
        .Where(p => !dbContext.Flights
        .Where(f => f.Status != FlightStatus.Completed && f.Status != FlightStatus.Canceled)
        .Select(f => f.PlaneId)
        .Contains(p.Id))
        .ToList();

        foreach (var plane in planes)
        {
            Planes.Add(new PlaneInfo(plane.Id, plane.RegistrationNumber, plane.Model, plane.Manufacturer));
        }

        var crewMembers = dbContext.CrewMembers
        .Where(cm => !dbContext.CrewMemberFlights
        .Include(cmf => cmf.Flight)
        .Any(cmf => cmf.CrewMemberId == cm.Id && cmf.Flight.Status != FlightStatus.Completed && cmf.Flight.Status != FlightStatus.Canceled))
        .ToList();

        foreach (var crewMember in crewMembers)
        {
            CrewMembers.Add(new CrewMemberInfo(crewMember.Id, $"{crewMember.FullName} ({crewMember.Type.ToEnumValue().Description})"));
        }

        var routes = dbContext.Routes.Select(e => new RouteInfo(e.Id, e.From, e.To, e.Range)).ToList();
        foreach (var route in routes)
        {
            Routes.Add(route);
        }
    }
}

public record RouteInfo(Guid Id, string From, string To, double Range)
{
    public string DisplayRow => $"{From} {To} - {Range}км";
}

public record FlightAddedMessage(FlightViewModel ViewModel);