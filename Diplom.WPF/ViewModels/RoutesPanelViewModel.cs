using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Diplom.WPF.Data;
using Diplom.WPF.Infrastructure;
using Diplom.WPF.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace Diplom.WPF.ViewModels;

public partial class RoutesPanelViewModel : BaseViewModel,
    IComboBoxItem,
    IRecipient<RouteAddedMessage>
{
    public ObservableCollection<RouteViewModel> Routes { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteRouteCommand))]
    [NotifyCanExecuteChangedFor(nameof(RollbackChangesCommand))]
    [NotifyCanExecuteChangedFor(nameof(UpdateRouteCommand))]
    private RouteViewModel? _selectedRoute;

    public string Title => "Маршруты";

    private string? _filterText;

    public string? FilterText
    {
        get => _filterText;
        set
        {
            if (SetProperty(ref _filterText, value))
            {
                RoutesView?.Refresh();
            }
        }
    }

    public ICollectionView RoutesView { get; }

    public RoutesPanelViewModel()
    {
        RoutesView = CollectionViewSource.GetDefaultView(Routes);
        RoutesView.Filter = Filter;

        bool Filter(object obj)
        {
            if (string.IsNullOrWhiteSpace(FilterText))
            {
                return true;
            }

            return obj is RouteViewModel route && (route.From.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                || route.To.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();

        var vms = dbContext
            .Routes
            .OrderBy(e => e.From)
            .Select(e => e.ToViewModel())
            .ToList();

        foreach (var item in vms)
        {
            Routes.Add(item);
        }
    }

    [RelayCommand]
    private static void AddRoute()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<RouteAddWindow>();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteOrRollbackChangesOrUpdate))]
    private async Task DeleteRoute()
    {
        if (DeleteRouteCommand.IsRunning)
        {
            return;
        }

        var dialogResult = MessageBoxHelper.ShowDialogBoxYesNo("Вы уверены, что желаете удалить выбранную запись?");
        if (dialogResult == System.Windows.MessageBoxResult.No)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();
        var item = await dbContext.Routes.FirstOrDefaultAsync(e => e.Id == SelectedRoute!.Id);
        if (item is not null)
        {
            if (await dbContext.Flights.AnyAsync(e => e.RouteId == item.Id))
            {
                MessageBoxHelper.ShowErrorBox("Удаление невозможно, присутствуют связанные данные.");
                return;
            }

            Routes.Remove(SelectedRoute!);
            dbContext.Routes.Remove(item);
            await dbContext.SaveChangesAsync();
        }
    }

    private bool CanDeleteOrRollbackChangesOrUpdate() => SelectedRoute is not null;

    [RelayCommand(CanExecute = nameof(CanDeleteOrRollbackChangesOrUpdate))]
    private void RollbackChanges()
    {
        if (!SelectedRoute!.IsModified())
        {
            return;
        }
        SelectedRoute.RollBackChanges();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteOrRollbackChangesOrUpdate))]
    private async Task UpdateRoute()
    {
        if (UpdateRouteCommand.IsRunning)
        {
            return;
        }

        if (!SelectedRoute!.IsModified())
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();
        var route = dbContext.Routes.First(e => e.Id == SelectedRoute!.Id);

        route.From = SelectedRoute.From.Trim();
        route.To = SelectedRoute.To.Trim();
        route.Range = SelectedRoute.Range;

        var validationResult = Validate(route);
        if (!validationResult.IsValid)
        {
            MessageBoxHelper.ShowErrorBox(validationResult.ToDisplayRow());
            return;
        }

        await dbContext.SaveChangesAsync();
        SelectedRoute.SaveState();

        MessageBoxHelper.ShowInfoBox("Данные успешно обновлены.");
    }

    public void Receive(RouteAddedMessage message)
    {
        Routes.Add(message.ViewModel);
    }
}

public record RouteAddedMessage(RouteViewModel ViewModel);