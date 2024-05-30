using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Diplom.WPF.Infrastructure;

public interface IUserDialogService
{
    void ShowDialog<T>() where T : Window;

    void CloseDialog();

    void ShowDialog<T, TViewModel>(TViewModel dataContext) where TViewModel : ObservableObject where T : Window;
}

public class UserDialogService : IUserDialogService
{
    private readonly Stack<Window> _windows = new();

    public void CloseDialog()
    {
        if (_windows.TryPop(out var window))
        {
            window.Close();
        }
    }

    public void ShowDialog<T>() where T : Window
    {
        var scope = App.Services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<T>();

        EventHandler onWindowClose = default!;
        onWindowClose = (e, _) =>
        {
            var sender = (Window)e!;
            scope.Dispose();
            if (_windows.Count != 0 && _windows.Peek() == sender)
            {
                _windows.Pop();
            }
            window.Closed -= onWindowClose;
        };
        window.Closed += onWindowClose;

        _windows.Push(window);
        window.ShowDialog();
    }

    public void ShowDialog<T, TViewModel>(TViewModel dataContext) where TViewModel : ObservableObject where T : Window
    {
        var scope = App.Services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<T>();
        window.DataContext = dataContext;

        EventHandler onWindowClose = default!;
        onWindowClose = (e, _) =>
        {
            var sender = (Window)e!;
            scope.Dispose();
            if (_windows.Count != 0 && _windows.Peek() == sender)
            {
                _windows.Pop();
            }
            window.Closed -= onWindowClose;
        };
        window.Closed += onWindowClose;

        _windows.Push(window);
        window.ShowDialog();
    }
}