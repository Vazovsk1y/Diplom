using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Diplom.WPF.Data;
using Diplom.WPF.Infrastructure;
using Diplom.WPF.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace Diplom.WPF.ViewModels;

public partial class FlightNoteAddViewModel : DialogViewModel
{
    public Guid FlightId { get; set; }

    public ObservableCollection<EnumValue> NoteTypes { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _title = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _description = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private EnumValue _type = null!;

    public FlightNoteAddViewModel(IUserDialogService userDialogService) : base(userDialogService)
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
        var flightNote = new FlightNote
        {
            FlightId = FlightId,
            Title = Title.Trim(),
            Description = Description.Trim(),
            Type = Enum.Parse<FlightNoteTypes>(Type.Value.ToString())
        };

        var validationResult = Validate(flightNote);
        if (!validationResult.IsValid)
        {
            MessageBoxHelper.ShowErrorBox(validationResult.ToDisplayRow());
            return;
        }

        dbContext.FlightNotes.Add(flightNote);
        await dbContext.SaveChangesAsync();
        Messenger.Send(new FlightNoteAddedMessage(new FlightNoteInfo(flightNote.Id, flightNote.Type.ToEnumValue().Description, flightNote.Title, flightNote.Description), FlightId));
        _dialogService.CloseDialog();
    }

    protected override bool CanAccept(object parameter)
    {
        return !(
            string.IsNullOrWhiteSpace(Title) ||
            string.IsNullOrWhiteSpace(Description) ||
            Type is null
        );
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        foreach (var item in Enum.GetValues<FlightNoteTypes>().Select(e => e.ToEnumValue()))
        {
            NoteTypes.Add(item);
        }
    }
}
public record FlightNoteAddedMessage(FlightNoteInfo ViewModel, Guid FlightId);
