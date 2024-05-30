namespace Diplom.WPF.Models;

public class CrewMemberFlight
{
    public required Guid CrewMemberId { get; init; }

    public required Guid FlightId { get; init; }

    public Flight Flight { get; set; } = null!;

    public CrewMember CrewMember { get; set; } = null!;
}