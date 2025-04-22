namespace AveoAudio.ViewModels;

public class TagEditorItem(string tag) : NotificationBase
{
    private bool isChecked;

    public bool IsChecked
    {
        get => this.isChecked;
        set => this.SetProperty(ref this.isChecked, value);
    }

    public string Tag { get; } = tag;

    public static implicit operator string(TagEditorItem item) => item.Tag;

    public override string ToString() => this.Tag;
}
