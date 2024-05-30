using CommunityToolkit.Mvvm.ComponentModel;
using Diplom.WPF.Data;
using Diplom.WPF.Infrastructure;
using Diplom.WPF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Diplom.WPF.ViewModels;

internal partial class LoginViewModel : DialogViewModel
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _userName = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _password = null!;

    [ObservableProperty]
    private bool _IsRegistration;

    public LoginViewModel(IUserDialogService userDialogService) : base(userDialogService)
    {
    }

    protected override async Task Accept(object action)
    {
        if (AcceptCommand.IsRunning)
        {
            return;
        }

        bool result;
        if (IsRegistration)
        {
            result = await Register(UserName, Password);
        }
        else
        {
            result = await Login(UserName.Trim().ToUpper(), Password.Trim());
        }

        if (result)
        {
            App.Services.GetRequiredService<MainWindow>().Show();
            _dialogService.CloseDialog();
        }
    }

    protected override bool CanAccept(object p) => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);

    private static async Task<bool> Login(string userName, string password)
    {
        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();

        var user = await dbContext
            .Users
            .FirstOrDefaultAsync(e => e.NormalizedUserName == userName);

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            MessageBoxHelper.ShowErrorBox("Неверный логин или пароль.");
            return false;
        }

        return true;
    }

    private async Task<bool> Register(string userName, string password)
    {
        const int passwordMinLenght = 6;
        if (password.Trim() is { Length: < passwordMinLenght})
        {
            MessageBoxHelper.ShowErrorBox($"Минимальная длина пароля {passwordMinLenght} символов.");
            return false;
        }

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();
        var user = new User
        {
            UserName = userName.Trim(),
            NormalizedUserName = userName.Trim().ToUpper(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password.Trim()),
        };

        var validationResult = Validate(user);
        if (!validationResult.IsValid)
        {
            MessageBoxHelper.ShowErrorBox(validationResult.ToDisplayRow());
            return false;
        }

        if (await dbContext.Users.AnyAsync(e => e.NormalizedUserName == user.NormalizedUserName))
        {
            MessageBoxHelper.ShowErrorBox("Данное имя пользователя уже занято.");
            return false;
        }

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return true;
    }
}