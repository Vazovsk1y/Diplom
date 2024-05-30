using System.ComponentModel;

namespace Diplom.WPF.Models;

public class FlightNote : Entity
{
    public required Guid FlightId { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required FlightNoteTypes Type { get; init; }
}

public enum FlightNoteTypes
{
    [Description("Заметка")]
    Note,
    [Description("Индцидент")]
    Accident
}
