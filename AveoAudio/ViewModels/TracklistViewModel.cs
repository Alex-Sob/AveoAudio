using System.Collections.ObjectModel;

using Windows.System;

namespace AveoAudio.ViewModels;

public class TracklistViewModel : NotificationBase
{
    private readonly ListeningQueue queue;
    private static ObservableCollection<TagGroup>? tagGroups;

    private TrackViewModel? selectedTrack;

    public TracklistViewModel(ListeningQueue queue)
    {
        this.queue = queue;

        this.TagsSelector = CreateTagsSelector();
        this.CommonTags.AddRange(TagGroups.SelectMany(g => g).Select(t => t.Tag));

        Track.TagsUpdated += OnTagsUpdated;
    }

    public ObservableCollection<TagGroup> TagGroups => tagGroups ??= CreateTagGroups();

    public HashSet<string> CommonTags { get; } = new(32);

    public TrackViewModel? EditingTagsFor { get; set; }

    public TrackViewModel? SelectedTrack
    {
        get => this.selectedTrack;
        set
        {
            if (this.selectedTrack != null) this.selectedTrack.IsSelected = false;
            this.SetProperty(ref this.selectedTrack, value);
            if (this.selectedTrack != null) this.selectedTrack.IsSelected = true;
        }
    }

    public TagsSelectorViewModel TagsSelector { get; }

    public IList<TrackViewModel> Tracks { get; private set; } = new ObservableCollection<TrackViewModel>();

    public void Enqueue() => this.queue.Enqueue(this.selectedTrack!.Track);

    public void Insert(Track track, int index) => this.Tracks.Insert(index, new TrackViewModel(this, track));

    public void Load(IEnumerable<Track> tracks) => this.Load(tracks.Select(t => new TrackViewModel(this, t)));

    public void Load(IEnumerable<TrackViewModel> tracks)
    {
        this.Tracks.Clear();
        this.Tracks.AddRange(tracks);
    }

    public void MarkCurrentAsPlayed() => this.Tracks[this.queue.CurrentIndex].DatePlayed = DateTime.Today;

    public void Play()
    {
        if (this.SelectedTrack!.Track == this.queue.Current)
            App.Current.MainViewModel.Play();
        else
        {
            if (this.SelectedTrack.Track != this.queue.Next)
                this.queue.AddNextUp(this.SelectedTrack.Track);

            App.Current.MainViewModel.PlayNext();
        }
    }

    public void ToggleBestTimeOfDay(TagEditorItem item)
    {
        this.EditingTagsFor!.ToggleBestTimeOfDay(item);
        item.IsChecked = true;
    }

    private static ObservableCollection<TagGroup> CreateTagGroups()
    {
        var groups = new List<TagGroup>(8);

        foreach (var (name, tags) in App.Current.AppSettings.TagGroups)
        {
            groups.Add(new(name, tags));
        }

        groups.Add(new("Time of day", Enum.GetNames<TimesOfDay>().AsSpan(1)));
        groups.Add(new("Weather", [nameof(Weather.Sun), nameof(Weather.Cloudy)]));
        groups.Add(new("Others"));

        return [.. groups];
    }

    private TagsSelectorViewModel CreateTagsSelector()
    {
        var toggleTagCommand = new DelegateCommand<TagEditorItem>(t => EditingTagsFor!.ToggleTag(t ?? throw new ArgumentNullException()));
        return new TagsSelectorViewModel(TagGroups, toggleTagCommand);
    }

    private void OnTagsUpdated(object? sender, TrackEventArgs e)
    {
        foreach (var trackViewModel in this.Tracks)
        {
            if (trackViewModel.Track == e.Track) trackViewModel.Tags = e.Track.Tags.ToString();
        }
    }
}
