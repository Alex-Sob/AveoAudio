using Windows.System;

namespace AveoAudio.ViewModels;

public class TrackViewModel(TracklistViewModel tracklist, Track track) : NotificationBase
{
    private DateTime? datePlayed;
    private bool isCurrent;
    private bool isSelected;

    private TagListBuilder tagsBuilder = new(track.Tags);

    public DateTime? DatePlayed
    {
        get => this.datePlayed;
        set => this.SetProperty(ref this.datePlayed, value);
    }

    public bool HasChanges => this.Tags != this.Track.Tags;

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

    public Track Track { get; } = track;

    public TracklistViewModel Tracklist { get; } = tracklist;

    public string Tags
    {
        get => this.tagsBuilder.Tags;
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

        var tags = this.Tracklist.TagGroups.SelectMany(g => g);

        foreach (var tag in tags)
        {
            tag.IsChecked = this.tagsBuilder.HasTag(tag);
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
        if (!Enum.TryParse<TimesOfDay>(tag, out var timesOfDay)) return;

        this.tagsBuilder.ToggleBestTimeOfDay(timesOfDay);

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

        var alternate = this.Tracklist.CommonTags.GetAlternateLookup<ReadOnlySpan<char>>();

        foreach (var tag in this.tagsBuilder.Tags)
        {
            if (!alternate.Contains(tag)) others.Tags.Add(new(tag.ToString()));
        }

        this.Tracklist.TagGroups[^1] = others;
    }

    private async Task UpdateTagsAsync()
    {
        await this.Track.UpdateTags(this.Tags);
        this.OnPropertyChanged(nameof(HasChanges));
    }
}
