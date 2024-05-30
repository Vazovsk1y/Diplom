using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Diplom.WPF.Data;
using Diplom.WPF.Infrastructure;
using Diplom.WPF.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace Diplom.WPF.ViewModels;

public partial class CrewMemberAddViewModel : DialogViewModel
{
    public required Guid Id { get; init; }

    public ObservableCollection<EnumValue> Types { get; } = [];


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _fullName = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private EnumValue _type = null!;

    public CrewMemberAddViewModel(IUserDialogService userDialogService) : base(userDialogService)
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
        var crewMember = new CrewMember
        {
           FullName = FullName.Trim(),
           Type = Enum.Parse<CrewMemberType>(Type.Value.ToString()),
        };

        var validationResult = Validate(crewMember);
        if (!validationResult.IsValid)
        {
            MessageBoxHelper.ShowErrorBox(validationResult.ToDisplayRow());
            return;
        }

        dbContext.CrewMembers.Add(crewMember);
        await dbContext.SaveChangesAsync();
        Messenger.Send(new CrewMemberAddedMessage(crewMember.ToViewModel()));
        _dialogService.CloseDialog();
    }

    protected override bool CanAccept(object p)
    {
        return !string.IsNullOrWhiteSpace(FullName) && Type is not null;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        foreach (var item in Enum.GetValues<CrewMemberType>().Select(e => e.ToEnumValue()))
        {
            Types.Add(item);
        }
    }
}

public record CrewMemberAddedMessage(CrewMemberViewModel ViewModel);