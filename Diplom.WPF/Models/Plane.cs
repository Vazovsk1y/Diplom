using System.ComponentModel;

namespace Diplom.WPF.Models;

public class Plane : Entity
{
    /// <summary>
    /// Регистрационный номер самолета.
    /// </summary>
    public required string RegistrationNumber { get; set; }

    /// <summary>
    /// Модель самолета.
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// Производитель самолета.
    /// </summary>
    public required string Manufacturer { get; set; }

    /// <summary>
    /// Вместимость самолета (количество пассажиров).
    /// </summary>
    public required int PassengersCapacity { get; set; }

    /// <summary>
    /// Дальность полета самолета (в километрах).
    /// </summary>
    public required double Range { get; set; } // в километрах

    /// <summary>
    /// Максимальная скорость самолета (в километрах в час).
    /// </summary>
    public required double MaxSpeed { get; set; } // в километрах в час

    /// <summary>
    /// Емкость топливных баков самолета (в литрах).
    /// </summary>
    public required double FuelCapacity { get; set; } // в литрах

    /// <summary>
    /// Расход топлива самолета (литров на 100 км).
    /// </summary>
    public required double FuelConsumption { get; set; } // литров на 100 км

    /// <summary>
    /// Тип самолета.
    /// </summary>
    public required PlaneType Type { get; set; }
}

public enum PlaneType
{
    [Description("Пассажирский")]
    Passenger,

    [Description("Грузовой")]
    Cargo,

    [Description("Частный самолет")]
    PrivateJet,

    [Description("Военный")]
    Military,

    [Description("Планер")]
    Glider
}