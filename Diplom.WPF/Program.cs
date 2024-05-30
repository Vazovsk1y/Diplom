using Diplom.WPF.Infrastructure;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;

namespace Diplom.WPF;

internal class Program
{
    public static bool IsInDebug { get; private set; }

    private static Mutex? _mutex;

    [STAThread]
    public static void Main(string[] args)
    {

#if DEBUG
        IsInDebug = true;
#endif

        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", IsInDebug ? "Development" : "Production");

        _mutex = new Mutex(true, App.Title, out bool createdNew);
        if (!createdNew)
        {
            MessageBoxHelper.ShowErrorBox("Приложение уже запущено.");
            return;
        }

        App app = new();
        app.StartGlobalExceptionsHandling();
        //app.InitializeComponent();
        app.MigrateDatabase();
        app.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host
            .CreateDefaultBuilder(args)
            .CreateApplicationAssociatedFolder()
            .ConfigureAppConfiguration((appConfig, _) =>
            {
                appConfig.HostingEnvironment.ApplicationName = App.Title;
                appConfig.HostingEnvironment.ContentRootPath = Environment.CurrentDirectory;
            })
            .UseSerilog((host, loggingConfiguration) =>
            {
                if (host.HostingEnvironment.IsDevelopment())
                {
                    loggingConfiguration.MinimumLevel.Debug();
                    loggingConfiguration.WriteTo.Debug();
                    return;
                }

                const string logFileName = "log.txt";
                const string logsFolderName = "logs";

                string logDirectory = Path.Combine(App.AssociatedFolder, logsFolderName, App.Version);
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string logFileFullPath = Path.Combine(logDirectory, logFileName);

                loggingConfiguration.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning);
                loggingConfiguration.MinimumLevel.Information();
                loggingConfiguration.WriteTo.File(logFileFullPath, rollingInterval: RollingInterval.Day);
            })
            .ConfigureServices(Registrator.ConfigureServices);
    }
}