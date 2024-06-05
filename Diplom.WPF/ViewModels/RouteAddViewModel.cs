using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Diplom.WPF.Data;
using Diplom.WPF.Infrastructure;
using Diplom.WPF.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Diplom.WPF.ViewModels;

public partial class RouteAddViewModel : DialogViewModel
{
    public required Guid Id { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _from = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _to = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private double _range; // в километрах

    public RouteAddViewModel(IUserDialogService userDialogService) : base(userDialogService)
    {
    }

    protected override async Task Accept(object action)
    {
        if (AcceptCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();
        var route = new Route
        {
            From = From.Trim(),
            To = To.Trim(),
            Range = Range
        };

        var validationResult = Validate(route);
        if (!validationResult.IsValid)
        {
            MessageBoxHelper.ShowErrorBox(validationResult.ToDisplayRow());
            return;
        }

        dbContext.Routes.Add(route);
        await dbContext.SaveChangesAsync();
        Messenger.Send(new RouteAddedMessage(route.ToViewModel()));
        _dialogService.CloseDialog();
    }

    protected override bool CanAccept(object parameter)
    {
        return !(
            string.IsNullOrWhiteSpace(From) ||
            string.IsNullOrWhiteSpace(To) ||
            Range <= 0.01
        );
    }

    protected override void OnActivated()
    {
        base.OnActivated();
    }
}
