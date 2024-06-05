using CommunityToolkit.Mvvm.ComponentModel;

namespace Diplom.WPF.ViewModels;

public partial class RouteViewModel : ModifiableViewModel<RouteViewModel>
{
    public required Guid Id { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string _from = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string _to = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private double _range;

    public override bool IsModified()
    {
        return From != PreviousState.From ||
               To != PreviousState.To ||
               Range != PreviousState.Range;
    }

    public override void RollBackChanges()
    {
        From = PreviousState.From;
        To = PreviousState.To;
        Range = PreviousState.Range;
        OnPropertyChanged(nameof(UpdatableSign));
    }
}
