namespace Diplom.WPF.ViewModels;

public record FlightReportRow(
    string Number,
    string DepartureDateTime,
    string ArrivalDateTime,
    double TravelTime,
    string From,
    string To,
    double Range,
    string CrewMembersForExcel,
    string PlaneNumber,
    string PlaneManufactureAndModel,
    IEnumerable<string> CrewMembers,
    int AccidentCount,
    int TotalNotesCount,
    decimal FuelUsed
    );
