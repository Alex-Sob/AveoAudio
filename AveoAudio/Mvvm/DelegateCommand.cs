using System.Windows.Input;

namespace AveoAudio;

public class DelegateCommand : ICommand
{
    private readonly Action<object?> action;

    public DelegateCommand(Action action) : this(p => action())
    {
    }

    public DelegateCommand(Action<object?> action) => this.action = action;

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => true;

    public virtual void Execute(object? parameter) => this.action(parameter);

    public void RaiseCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
