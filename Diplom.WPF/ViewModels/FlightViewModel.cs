using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Diplom.WPF.ViewModels;
public partial class FlightViewModel : ModifiableViewModel<FlightViewModel>
{
    public required Guid Id { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string _number = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private DateOnly _departureDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private TimeOnly _departureTime;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private DateOnly _arrivalDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private TimeOnly _arrivalTime;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private EnumValue _status = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private PlaneInfo _plane = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private RouteInfo _route = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private IEnumerable<CrewMemberInfo> _crewMembers = [];

    [ObservableProperty]
    private ObservableCollection<FlightNoteInfo> _flightNotes = [];

    public override bool IsModified()
    {
        return Number != PreviousState.Number ||
               DepartureDate != PreviousState.DepartureDate ||
               DepartureTime != PreviousState.DepartureTime ||
               ArrivalDate != PreviousState.ArrivalDate ||
               ArrivalTime != PreviousState.ArrivalTime ||
               Status != PreviousState.Status ||
               Plane != PreviousState.Plane ||
               Route != PreviousState.Route;
    }

    public override void RollBackChanges()
    {
        Number = PreviousState.Number;
        DepartureDate = PreviousState.DepartureDate;
        DepartureTime = PreviousState.DepartureTime;
        ArrivalDate = PreviousState.ArrivalDate;
        ArrivalTime = PreviousState.ArrivalTime;
        Status = PreviousState.Status;
        Plane = PreviousState.Plane;
        Route = PreviousState.Route;
        OnPropertyChanged(nameof(UpdatableSign));
    }
}

public record PlaneInfo(Guid Id, string RegistrationNumber, string Model, string Manufacturer);

public record CrewMemberInfo(Guid Id, string FullNameAndPost);

public record FlightNoteInfo(Guid Id, string Type, string Title, string Description);