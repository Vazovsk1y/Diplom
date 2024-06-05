namespace Diplom.WPF.Models;

public class Route : Entity
{
    public required string From { get; set; }

    public required string To { get; set; }

    public required double Range { get; set; }
}
