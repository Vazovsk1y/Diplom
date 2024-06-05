using Diplom.WPF.Models;
using Diplom.WPF.ViewModels;
using FluentValidation.Results;
using System.ComponentModel;
using System.Reflection;

namespace Diplom.WPF.Infrastructure;

public static class Extensions
{
    public static string ToDisplayRow(this ValidationResult validationResult)
    {
        return string.Join(Environment.NewLine, validationResult.Errors.Select(x => x.ErrorMessage));
    }

    public static RouteViewModel ToViewModel(this Route route)
    {
        var result = new RouteViewModel
        {
            Id = route.Id,
            From = route.From,
            Range = route.Range,
            To = route.To,
        };

        result.SaveState();
        return result;
    }
    public static DateTimeOffset ToDateTimeOffset(this (DateOnly date, TimeOnly time) tuple)
    {
        var dateTime = tuple.date.ToDateTime(tuple.time);

        var dateTimeOffset = new DateTimeOffset(dateTime, TimeSpan.Zero);

        return dateTimeOffset;
    }

    public static EnumValue ToEnumValue(this Enum value)
    {
        FieldInfo field = value.GetType().GetField(value.ToString());
        DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();

        return new EnumValue(attribute == null ? value.ToString() : attribute.Description, value);
    }

    public static CrewMemberViewModel ToViewModel(this CrewMember crewMember)
    {
        var result = new CrewMemberViewModel()
        {
            Id = crewMember.Id,
            FullName = crewMember.FullName,
            Type = crewMember.Type.ToEnumValue(),
        };

        result.SaveState();
        return result;
    }

    public static FlightViewModel ToViewModel(this Flight flight)
    {
        var result = new FlightViewModel
        {
            Id = flight.Id,
            FlightNotes = new (flight.Notes.Select(e => new FlightNoteInfo(e.Id, e.Type.ToEnumValue().Description, e.Title, e.Description))),
            ArrivalDate = flight.ArrivalDate.ToDateOnly(),
            ArrivalTime = flight.ArrivalDate.ToTimeOnly(),
            CrewMembers = flight.CrewMembers.Select(e => e.CrewMember).Select(e => new CrewMemberInfo(e.Id, $"{e.FullName} ({e.Type.ToEnumValue().Description})")),
            DepartureDate = flight.DepartureDate.ToDateOnly(),
            DepartureTime = flight.DepartureDate.ToTimeOnly(),
            Number = flight.Number,
            Route = new RouteInfo(flight.Route.Id, flight.Route.From, flight.Route.To, flight.Route.Range),
            Plane = new PlaneInfo(flight.Plane.Id, flight.Plane.RegistrationNumber, flight.Plane.Model, flight.Plane.Manufacturer),
            Status = flight.Status.ToEnumValue(),
        };

        result.SaveState();
        return result;
    }

    public static PlaneViewModel ToViewModel(this Plane plane)
    {
        var result = new PlaneViewModel
        {
            Id = plane.Id,
            FuelCapacity = plane.FuelCapacity,
            FuelConsumption = plane.FuelConsumption,
            Manufacturer = plane.Manufacturer,
            MaxSpeed = plane.MaxSpeed,
            Model = plane.Model,
            PassengersCapacity = plane.PassengersCapacity,
            Range = plane.Range,
            RegistrationNumber = plane.RegistrationNumber,
            Type = plane.Type.ToEnumValue()
        };

        result.SaveState();
        return result;
    }

    public static TimeOnly ToTimeOnly(this DateTimeOffset dateTimeOffset)
    {
        return TimeOnly.FromDateTime(dateTimeOffset.DateTime);
    }

    public static DateOnly ToDateOnly(this DateTimeOffset dateTimeOffset)
    {
        return DateOnly.FromDateTime(dateTimeOffset.DateTime);
    }
}