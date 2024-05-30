namespace Diplom.WPF.Models;
public class User : Entity
{
    public required string NormalizedUserName { get; init; }

    public required string UserName { get; init; }

    public required string PasswordHash { get; init; }
}