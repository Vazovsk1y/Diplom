﻿using System.ComponentModel;

namespace Diplom.WPF.Models;

public class Flight : Entity
{
    public required Guid PlaneId { get; init; }

    public required Guid RouteId { get; init; }

    /// <summary>
    /// Номер рейса.
    /// </summary>
    public required string Number { get;  set; }

    /// <summary>
    /// Дата и время вылета.
    /// </summary>
    public required DateTimeOffset DepartureDate { get; set; }

    /// <summary>
    /// Дата и время прибытия.
    /// </summary>
    public required DateTimeOffset ArrivalDate { get; set; }


    public required FlightStatus Status { get; set; }

    /// <summary>
    /// Бортовой состав рейса.
    /// </summary>
    public ICollection<CrewMemberFlight> CrewMembers { get; set; } = [];

    /// <summary>
    /// Заметки о рейсе.
    /// </summary>
    public ICollection<FlightNote> Notes { get; set;} = [];


    public Plane Plane { get; set; } = null!;

    public Route Route { get; set; } = null!;
}

public enum FlightStatus
{
    [Description("Запланирован")]
    Scheduled,

    [Description("Выполняется")]
    InProgress,

    [Description("Отменен")]
    Canceled,

    [Description("Задерживается")]
    Delayed,

    [Description("Завершен")]
    Completed,
}
