namespace AveoAudio;

public class DelegateCommand<T> : DelegateCommand
{
    public DelegateCommand(Action<T?> action) : base(p => action((T?)p))
    {
    }
}
