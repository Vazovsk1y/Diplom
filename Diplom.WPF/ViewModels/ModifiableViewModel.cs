using CommunityToolkit.Mvvm.ComponentModel;

namespace Diplom.WPF.ViewModels;

public abstract class ModifiableViewModel<T> :
    ObservableObject
    where T : ModifiableViewModel<T>
{
    protected T PreviousState { get; private set; } = default!;

    public string? UpdatableSign => IsModified() ? "*" : null;

    public abstract bool IsModified();

    public abstract void RollBackChanges();

    public virtual void SaveState()
    {
        PreviousState = (T)MemberwiseClone();
        OnPropertyChanged(nameof(UpdatableSign));
    }
}