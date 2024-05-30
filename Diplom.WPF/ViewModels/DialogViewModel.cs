using CommunityToolkit.Mvvm.Input;
using Diplom.WPF.Infrastructure;

namespace Diplom.WPF.ViewModels;

public abstract partial class DialogViewModel : BaseViewModel
{
    #region --Fields--

    protected readonly IUserDialogService _dialogService;

    #endregion

    #region --Properties--



    #endregion

    #region --Constructors--

    public DialogViewModel() { }

    public DialogViewModel(IUserDialogService userDialogService)
    {
        _dialogService = userDialogService;
    }

    #endregion

    #region --Commands--

    [RelayCommand(CanExecute = nameof(CanCancel))]
    protected virtual Task Cancel(object p)
    {
        _dialogService.CloseDialog();
        return Task.CompletedTask;
    }
    protected virtual bool CanCancel(object p) => true;

    [RelayCommand(CanExecute = nameof(CanAccept))]
    protected abstract Task Accept(object action);
    protected abstract bool CanAccept(object p);

    #endregion

    #region --Methods--


    #endregion
}