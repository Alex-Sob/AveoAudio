namespace AveoAudio.ViewModels;

public class TagEditorItem(string tag, TracklistViewModel tracklist) : NotificationBase
{
    private readonly TracklistViewModel tracklist = tracklist;
    private bool isChecked;

    public bool IsChecked
    {
        get => this.isChecked;
        set => this.SetProperty(ref this.isChecked, value);
    }

    public string Tag { get; } = tag;

    public static implicit operator string(TagEditorItem item) => item.Tag;

    public void ToggleBestTimeOfDay()
    {
        this.tracklist.EditingTagsFor.ToggleBestTimeOfDay(this.Tag);
        this.IsChecked = true;
    }

    public void ToggleTag() => this.tracklist.EditingTagsFor.ToggleTag(this.Tag);

    public override string ToString() => this.Tag;
}
