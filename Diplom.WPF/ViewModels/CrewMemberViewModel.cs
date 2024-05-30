using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Diplom.WPF.ViewModels;

public partial class CrewMemberViewModel : ModifiableViewModel<CrewMemberViewModel>
{
    public required Guid Id { get; init; }

    public ObservableCollection<EnumValue> Types { get; } = [];


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string _fullName = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private EnumValue _type = null!;

    public override bool IsModified()
    {
        return PreviousState.FullName != FullName || PreviousState.Type != Type;
    }

    public override void RollBackChanges()
    {
        FullName = PreviousState.FullName;
        Type = PreviousState.Type;
        OnPropertyChanged(nameof(UpdatableSign));
    }
}