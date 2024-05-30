namespace Diplom.WPF.Models;

public abstract class Entity
{
    public Guid Id { get; } = Guid.NewGuid();
}
