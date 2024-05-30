using CommunityToolkit.Mvvm.ComponentModel;
using Diplom.WPF.Data;
using Diplom.WPF.ViewModels;
using Diplom.WPF.Views;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Diplom.WPF.Infrastructure;

public static class Registrator
{
    internal static void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {
        collection.AddSingleton<IUserDialogService, UserDialogService>();
        collection.AddWindowWithViewModelTransient<LoginWindow, LoginViewModel>();
        collection.AddWindowWithViewModelSingleton<MainWindow, MainWindowViewModel>();
        collection.AddWindowWithViewModelTransient<PlaneAddWindow, PlaneAddViewModel>();

        collection.AddSingleton<PlanesPanelViewModel>();

        collection.AddDbContext<DiplomDbContext>(e =>
        {
            string connectionString = context.Configuration.GetConnectionString("Default") ?? throw new ApplicationException("Необходимо определить строку подключения.");
            e.UseNpgsql(connectionString);
        });

        collection.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public static IHostBuilder CreateApplicationAssociatedFolder(this IHostBuilder hostBuilder)
    {
        if (!Directory.Exists(App.AssociatedFolder))
        {
            Directory.CreateDirectory(App.AssociatedFolder);
        }

        return hostBuilder;
    }

    private static IServiceCollection AddWindowWithViewModelTransient<TWindow, TViewModel>(this IServiceCollection services)
            where TViewModel : ObservableObject
            where TWindow : Window
            => services
        .AddTransient<TViewModel>()
        .AddTransient(s =>
        {
            var viewModel = s.GetRequiredService<TViewModel>();

            if (viewModel is ObservableRecipient observableRecipient)
            {
                observableRecipient.IsActive = true;
            }

            var window = Activator.CreateInstance<TWindow>();
            window.DataContext = viewModel;
            return window;
        });

    private static IServiceCollection AddWindowWithViewModelSingleton<TWindow, TViewModel>(this IServiceCollection services)
            where TViewModel : ObservableObject
            where TWindow : Window
            => services
        .AddSingleton<TViewModel>()
        .AddSingleton(s =>
        {
            var viewModel = s.GetRequiredService<TViewModel>();
            var window = Activator.CreateInstance<TWindow>();

            if (viewModel is ObservableRecipient observableRecipient)
            {
                observableRecipient.IsActive = true;
            }

            window.DataContext = viewModel;
            return window;
        });
}