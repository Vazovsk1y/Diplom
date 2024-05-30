using CommunityToolkit.Mvvm.ComponentModel;

namespace Diplom.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ICollection<IComboBoxItem> ComboBoxItems { get; } = [];

    [ObservableProperty]
    private IComboBoxItem? _selectedItem;

    public MainWindowViewModel(PlanesPanelViewModel planesPanelViewModel)
    {
        ComboBoxItems.Add(planesPanelViewModel);
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
