using ClosedXML.Report;
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
using System.ComponentModel;
using System.Reflection;
using System.Windows.Data;

namespace Diplom.WPF.ViewModels;

public partial class FlightsPanelViewModel : BaseViewModel, 
    IComboBoxItem, 
    IRecipient<FlightAddedMessage>,
    IRecipient<FlightNoteAddedMessage>
{
    public ObservableCollection<FlightViewModel> Flights { get; } = [];

    public ObservableCollection<EnumValue> Statuses { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteFlightCommand))]
    [NotifyCanExecuteChangedFor(nameof(RollbackChangesCommand))]
    [NotifyCanExecuteChangedFor(nameof(UpdateFlightCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddNoteCommand))]
    private FlightViewModel? _selectedFlight;

    [ObservableProperty]
    private IEnumerable<FlightReportRow>? _report;

    public string Title => "Рейсы";

    private string? _filterText;

    public string? FilterText
    {
        get => _filterText;
        set
        {
            if (SetProperty(ref _filterText, value))
            {
                FlightsView?.Refresh();
            }
        }
    }

    public ICollectionView FlightsView { get; }

    public FlightsPanelViewModel()
    {
        FlightsView = CollectionViewSource.GetDefaultView(Flights);
        FlightsView.Filter = Filter;

        bool Filter(object obj)
        {
            if (string.IsNullOrWhiteSpace(FilterText))
            {
                return true;
            }

            return obj is FlightViewModel flight && (flight.Number.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                || flight.Plane.RegistrationNumber.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                || flight.Plane.Manufacturer.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                || flight.Plane.Model.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                || flight.Route.From.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                || flight.Route.To.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                || flight.Status.Description.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();

        var vms = dbContext
            .Flights
            .Include(e => e.Route)
            .Include(e => e.CrewMembers)
            .ThenInclude(e => e.CrewMember)
            .Include(e => e.Notes)
            .Include(e => e.Plane)
            .OrderBy(e => e.DepartureDate)
            .Select(e => e.ToViewModel())
            .ToList();

        foreach (var item in Enum.GetValues<FlightStatus>().Select(e => e.ToEnumValue()))
        {
            Statuses.Add(item);
        }

        foreach (var item in vms)
        {
            Flights.Add(item);
        }
    }

    [RelayCommand]
    private static void AddFlight()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<FlightAddWindow>();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteOrRollbackChangesOrUpdateOrAddNote))]
    private async Task DeleteFlight()
    {
        if (DeleteFlightCommand.IsRunning)
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
        var item = await dbContext.Flights.FirstOrDefaultAsync(e => e.Id == SelectedFlight!.Id);
        if (item is not null)
        {
            Flights.Remove(SelectedFlight!);
            dbContext.Flights.Remove(item);
            await dbContext.SaveChangesAsync();
        }
    }

    private bool CanDeleteOrRollbackChangesOrUpdateOrAddNote() => SelectedFlight is not null;


    [RelayCommand(CanExecute = nameof(CanDeleteOrRollbackChangesOrUpdateOrAddNote))]
    private void RollbackChanges()
    {
        if (!SelectedFlight!.IsModified())
        {
            return;
        }
        SelectedFlight.RollBackChanges();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteOrRollbackChangesOrUpdateOrAddNote))]
    private async Task UpdateFlight()
    {
        if (UpdateFlightCommand.IsRunning)
        {
            return;
        }

        if (!SelectedFlight!.IsModified())
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();
        var flight = dbContext.Flights.Include(e => e.CrewMembers).First(e => e.Id == SelectedFlight!.Id);

        flight.Status = Enum.Parse<FlightStatus>(SelectedFlight.Status.Value.ToString());
        flight.ArrivalDate = (SelectedFlight.ArrivalDate, SelectedFlight.ArrivalTime).ToDateTimeOffset();
        flight.DepartureDate = (SelectedFlight.DepartureDate, SelectedFlight.DepartureTime).ToDateTimeOffset();
        flight.Number = SelectedFlight.Number.Trim();

        if (await dbContext.Flights.AnyAsync(e => e.Id != flight.Id && e.Number == flight.Number))
        {
            MessageBoxHelper.ShowErrorBox("Необходимо обеспечить уникальность номера.");
            return;
        }

        var validationResult = Validate(flight);
        if (!validationResult.IsValid)
        {
            MessageBoxHelper.ShowErrorBox(validationResult.ToDisplayRow());
            return;
        }

        await dbContext.SaveChangesAsync();
        SelectedFlight.SaveState();

        MessageBoxHelper.ShowInfoBox("Данные успешно обновлены.");
    }

    [RelayCommand(CanExecute = nameof(CanDeleteOrRollbackChangesOrUpdateOrAddNote))]
    private void AddNote()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        var vm = scope.ServiceProvider.GetRequiredService<FlightNoteAddViewModel>();
        vm.IsActive = true;
        vm.FlightId = SelectedFlight!.Id;
        dialogService.ShowDialog<FlightNoteAddWindow, FlightNoteAddViewModel>(vm);
    }

    [RelayCommand]
    private async Task GenerateReport()
    {
        if (GenerateReportCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();

        Report = await dbContext
            .Flights
            .Include(e => e.Plane)
            .Include(e => e.Notes)
            .Include(e => e.Route)
            .Include(e => e.CrewMembers)
            .ThenInclude(e => e.CrewMember)
            .Where(e => e.Status == FlightStatus.Completed)
            .OrderBy(e => e.DepartureDate)
            .Select(e => new FlightReportRow(
                e.Number,
                e.DepartureDate.ToString("dd.MM.yyyy HH:mm"),
                e.ArrivalDate.ToString("dd.MM.yyyy HH:mm"),
                (e.ArrivalDate - e.DepartureDate).TotalMinutes,
                e.Route.From,
                e.Route.To,
                e.Route.Range,
                string.Join(",", e.CrewMembers.Select(e => $"{e.CrewMember.FullName} ({e.CrewMember.Type.ToEnumValue().Description})")),
                e.Plane.RegistrationNumber,
                $"{e.Plane.Manufacturer} {e.Plane.Model}",
                e.CrewMembers.Select(e => $"{e.CrewMember.FullName} ({e.CrewMember.Type.ToEnumValue().Description})"),
                e.Notes.Where(e => e.Type == FlightNoteTypes.Accident).Count(),
                e.Notes.Count(),
                (decimal)(e.Route.Range / 100 * e.Plane.FuelConsumption)
                ))
            .ToListAsync(); 
    }

    [RelayCommand]
    private async Task ExportToExcel()
    {
        const string Filter = "Excel Files (*.xlsx)|*.xlsx";
        const string Title = "Выберите файл:";
        var fileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = Filter,
            Title = Title,
            RestoreDirectory = true,
        };

        fileDialog.ShowDialog();
        string? filePath = fileDialog.FileName;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        if (ExportToExcelCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiplomDbContext>();
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Diplom.WPF.ReportTemplate.xlsx");

        Report = await dbContext
            .Flights
            .Include(e => e.Plane)
            .Include(e => e.Notes)
            .Include(e => e.Route)
            .Include(e => e.CrewMembers)
            .ThenInclude(e => e.CrewMember)
            .Where(e => e.Status == FlightStatus.Completed)
            .OrderBy(e => e.DepartureDate)
            .Select(e => new FlightReportRow(
                e.Number,
                e.DepartureDate.ToString("dd.MM.yyyy HH:mm"),
                e.ArrivalDate.ToString("dd.MM.yyyy HH:mm"),
                (e.ArrivalDate - e.DepartureDate).TotalMinutes,
                e.Route.From,
                e.Route.To,
                e.Route.Range,
                string.Join(",", e.CrewMembers.Select(e => $"{e.CrewMember.FullName} ({e.CrewMember.Type.ToEnumValue().Description})")),
                e.Plane.RegistrationNumber,
                $"{e.Plane.Manufacturer} {e.Plane.Model}",
                e.CrewMembers.Select(e => $"{e.CrewMember.FullName} ({e.CrewMember.Type.ToEnumValue().Description})"),
                e.Notes.Where(e => e.Type == FlightNoteTypes.Accident).Count(),
                e.Notes.Count(),
                (decimal)(e.Route.Range / 100 * e.Plane.FuelConsumption)
                ))
            .ToListAsync();

        using var template = new XLTemplate(stream);
        template.AddVariable(new { Items = Report });
        template.Generate();

        template.Workbook.Worksheets.First().ColumnsUsed().AdjustToContents();

        template.SaveAs(filePath);

        MessageBoxHelper.ShowInfoBox("Отчет успешно экспортирован.");
    }

    public void Receive(FlightAddedMessage message)
    {
        Flights.Add(message.ViewModel);
    }

    public void Receive(FlightNoteAddedMessage message)
    {
        if (message.FlightId != SelectedFlight?.Id)
        {
            throw new ApplicationException("Выбранный рейс не соответствует рейсу из добавленной заметки.");
        }

        SelectedFlight!.FlightNotes.Add(message.ViewModel);
    }
}