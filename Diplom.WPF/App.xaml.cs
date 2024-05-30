using Diplom.WPF.Data;
using Diplom.WPF.Infrastructure;
using Diplom.WPF.ViewModels;
using Diplom.WPF.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Diplom.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? throw new InvalidOperationException("Версия не определена.");

    public const string Title = "Diplom";

    private static readonly IHost Host;

    public static readonly string AssociatedFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Title);

    public static ILogger Logger { get; }

    public static IServiceProvider Services { get; }

    static App()
    {
        Host = Program.CreateHostBuilder(Environment.GetCommandLineArgs()).Build();
        Services = Host.Services;
        Logger = Services.GetRequiredService<ILogger<App>>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Host.Start();

        var dialogService = Services.GetRequiredService<IUserDialogService>();
        Logger.LogInformation("Запуск окна входа в аккаунт.");
        dialogService.ShowDialog<LoginWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        Logger.LogInformation("Приложение было остановлено.");
    }

    public void StartGlobalExceptionsHandling()
    {
        const string MessageTemplate = "Что-то пошло не так в {exceptionSource}.";

        DispatcherUnhandledException += (sender, e) =>
        {
            Logger.LogError(e.Exception, MessageTemplate, nameof(DispatcherUnhandledException));
            e.Handled = true;

            Current?.Shutdown();
        };

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Logger.LogError(e.ExceptionObject as Exception, MessageTemplate, $"{nameof(AppDomain.CurrentDomain)}.{nameof(AppDomain.CurrentDomain.UnhandledException)}");

            Current?.Shutdown();
        };

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            Logger.LogError(e.Exception, MessageTemplate, nameof(TaskScheduler.UnobservedTaskException));

            Current?.Shutdown();
        };
    }

    public void MigrateDatabase()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();
        dbContext.Database.Migrate();
    }
}