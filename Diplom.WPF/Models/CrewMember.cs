using System.ComponentModel;

namespace Diplom.WPF.Models;

public class CrewMember : Entity
{
    /// <summary>
    /// ФИО.
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// Должность.
    /// </summary>
    public required CrewMemberType Type { get; set; }
}

public enum CrewMemberType
{
    [Description("Пилот")]
    Pilot,

    [Description("Второй пилот")]
    CoPilot,

    [Description("Бортпроводник")]
    FlightAttendant,

    [Description("Инженер")]
    Engineer,

    [Description("Штурман")]
    Navigator,

    [Description("Бортмеханик")]
    FlightEngineer,

    [Description("Радист")]
    RadioOperator
}