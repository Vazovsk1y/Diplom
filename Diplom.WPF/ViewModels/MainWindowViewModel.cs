using CommunityToolkit.Mvvm.ComponentModel;

namespace Diplom.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ICollection<IComboBoxItem> ComboBoxItems { get; } = [];

    [ObservableProperty]
    private IComboBoxItem? _selectedItem;

    public MainWindowViewModel(
        PlanesPanelViewModel planesPanelViewModel,
        CrewMembersPanelViewModel crewMembersPanelViewModel,
        FlightsPanelViewModel flightsPanelViewModel, 
        RoutesPanelViewModel routesPanelViewModel)
    {
        ComboBoxItems.Add(planesPanelViewModel);
        ComboBoxItems.Add(flightsPanelViewModel);
        ComboBoxItems.Add(crewMembersPanelViewModel);
        ComboBoxItems.Add(routesPanelViewModel);
        ActivateComboBoxItems();
        SelectedItem = ComboBoxItems.FirstOrDefault();
    }

    private void ActivateComboBoxItems()
    {
        foreach (var item in ComboBoxItems)
        {
            if (item is ObservableRecipient recipient)
            {
                recipient.IsActive = true;
            }
        }
    }
}
