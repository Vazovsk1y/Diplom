using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Diplom.WPF.Data;
using Diplom.WPF.Infrastructure;
using Diplom.WPF.Models;
using Diplom.WPF.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace Diplom.WPF.ViewModels;

public partial class CrewMembersPanelViewModel : BaseViewModel, 
    IComboBoxItem,
    IRecipient<CrewMemberAddedMessage>
{
    public ObservableCollection<CrewMemberViewModel> CrewMembers { get; } = [];

    public ObservableCollection<EnumValue> Types { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCrewMemberCommand))]
    private CrewMemberViewModel? _selectedCrewMember;

    public string Title => "Члены экипажа";

    protected override void OnActivated()
    {
        base.OnActivated();

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();

        var vms = dbContext
            .CrewMembers
            .OrderBy(e => e.FullName)
            .Select(e => e.ToViewModel())
            .ToList();

        foreach (var item in Enum.GetValues<CrewMemberType>().Select(e => e.ToEnumValue()))
        {
            Types.Add(item);
        }

        foreach (var item in vms)
        {
            CrewMembers.Add(item);
        }
    }

    [RelayCommand]
    private static void AddCrewMember()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<CrewMemberAddWindow>();
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteCrewMember()
    {
        if (DeleteCrewMemberCommand.IsRunning)
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
        var item = await dbContext.CrewMembers.FirstOrDefaultAsync(e => e.Id == SelectedCrewMember!.Id);
        if (item is not null)
        {
            if (await dbContext.CrewMemberFlights.AnyAsync(e => e.CrewMemberId == SelectedCrewMember!.Id))
            {
                MessageBoxHelper.ShowErrorBox("Удаление невозможно, присутствуют связанные данные.");
                return;
            }

            CrewMembers.Remove(SelectedCrewMember!);
            dbContext.CrewMembers.Remove(item);
            await dbContext.SaveChangesAsync();
        }
    }

    private bool CanDelete() => SelectedCrewMember is not null;

    [RelayCommand]
    private static void RollbackChanges(CrewMemberViewModel crewMemberViewModel)
    {
        if (!crewMemberViewModel.IsModified())
        {
            return;
        }

        crewMemberViewModel.RollBackChanges();
    }

    [RelayCommand]
    private async Task UpdateCrewMember(CrewMemberViewModel crewMemberViewModel)
    {
        if (UpdateCrewMemberCommand.IsRunning || !crewMemberViewModel!.IsModified())
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();
        var crewMember = dbContext.CrewMembers.First(e => e.Id == crewMemberViewModel.Id);

        crewMember.FullName = crewMemberViewModel.FullName.Trim();
        crewMember.Type = Enum.Parse<CrewMemberType>(crewMemberViewModel.Type.Value.ToString());

        var validationResult = Validate(crewMember);
        if (!validationResult.IsValid)
        {
            MessageBoxHelper.ShowErrorBox(validationResult.ToDisplayRow());
            return;
        }

        await dbContext.SaveChangesAsync();
        crewMemberViewModel.SaveState();

        MessageBoxHelper.ShowInfoBox("Данные успешно обновлены.");
    }

    public void Receive(CrewMemberAddedMessage message)
    {
        CrewMembers.Add(message.ViewModel);
    }
}