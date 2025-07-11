using Windows.System;

namespace AveoAudio.ViewModels;

public class TrackViewModel(TracklistViewModel tracklist, Track track) : NotificationBase
{
    private DateTime? datePlayed;
    private bool isCurrent;
    private bool isSelected;
    private bool justPlayed;

    private TagListBuilder tagsBuilder = new(track.Tags);

    public DateTime? DatePlayed
    {
        get => this.datePlayed;
        set => this.SetProperty(ref this.datePlayed, value);
    }

    public bool HasChanges => !this.Track.Tags.AsSpan().Equals(this.Tags, StringComparison.Ordinal);

    public bool IsInvalid => !Track.DateAdded.HasValue;

    public bool IsCurrent
    {
        get => this.isCurrent;
        set => this.SetProperty(ref this.isCurrent, value);
    }

    public bool IsSelected
    {
        get => this.isSelected;
        set => this.SetProperty(ref this.isSelected, value);
    }

    public bool JustPlayed
    {
        get => this.justPlayed;
        set => this.SetProperty(ref this.justPlayed, value);
    }

    public Track Track { get; } = track;

    public TracklistViewModel Tracklist { get; } = tracklist;

    public string Tags
    {
        get => this.tagsBuilder.Tags.ToString();
        set
        {
            this.SetProperty(ref this.tagsBuilder, new TagListBuilder(value));
            this.OnPropertyChanged(nameof(HasChanges));
        }
    }

    public void EditTags()
    {
        if (this.Tracklist.EditingTagsFor != this) UpdateOtherTags();

        this.Tracklist.EditingTagsFor = this;

        var items = this.Tracklist.TagGroups.SelectMany(g => g);

        foreach (var item in items)
        {
            // TODO: Optimize?
            item.IsChecked = this.tagsBuilder.Tags.FindTag(item.Tag, out _);
        }
    }

    public void OpenFolder()
    {
        var path = Path.GetDirectoryName(this.Track.File.Path);
        _ = Launcher.LaunchFolderPathAsync(path, new FolderLauncherOptions { ItemsToSelect = { this.Track.File } });
    }

    public void UpdateTags() => App.Current.GetBusy(UpdateTagsAsync(), "Updating tags");

    public void ToggleBestTimeOfDay(string tag)
    {
        if (!tag.IsTimeOfDay(out var timeOfDay)) return;

        this.tagsBuilder.ToggleBestTimeOfDay(timeOfDay);

        OnPropertyChanged(nameof(this.Tags));
        OnPropertyChanged(nameof(this.HasChanges));
    }

    public void ToggleTag(string tag)
    {
        this.tagsBuilder.ToggleTag(tag);

        OnPropertyChanged(nameof(this.Tags));
        OnPropertyChanged(nameof(this.HasChanges));
    }

    private void UpdateOtherTags()
    {
        var others = this.Tracklist.TagGroups[^1];
        others.Tags.Clear();

        foreach (var tag in this.tagsBuilder.Tags)
        {
            if (!CommonTags.TryGetTag(tag, out _)) others.Tags.Add(new(tag.ToString()));
        }

        this.Tracklist.TagGroups[^1] = others;
    }

    private async Task UpdateTagsAsync()
    {
        await this.Track.UpdateTags(this.Tags);

        this.OnPropertyChanged(nameof(HasChanges));
        this.OnPropertyChanged(nameof(IsInvalid));
    }
}
