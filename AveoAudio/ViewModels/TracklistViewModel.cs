using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Windows.System;

namespace AveoAudio.ViewModels;

public class TracklistViewModel : NotificationBase
{
    private readonly MainViewModel mainViewModel;
    private readonly ListeningQueue queue;

    private TrackViewModel selectedTrack;

    public TracklistViewModel(ListeningQueue queue, MainViewModel mainViewModel)
    {
        this.queue = queue;
        this.mainViewModel = mainViewModel;

        var toggleTagCommand = new DelegateCommand<TagEditorItem>(t => EditingTagsFor.ToggleTag(t));

        this.TagGroups = CreateTagGroups();
        this.TagsSelector = new TagsSelectorViewModel(this.TagGroups, toggleTagCommand);

        this.CommonTags.AddRange(this.TagGroups.SelectMany(g => g).Select(t => t.Tag));
    }

    public HashSet<string> CommonTags { get; } = new(32);

    public TrackViewModel EditingTagsFor { get; set; }

    public TrackViewModel SelectedTrack
    {
        get => this.selectedTrack;
        set
        {
            if (this.selectedTrack != null) this.selectedTrack.IsSelected = false;
            this.SetProperty(ref this.selectedTrack, value);
            if (this.selectedTrack != null) this.selectedTrack.IsSelected = true;
        }
    }

    public ObservableCollection<TagGroup> TagGroups { get; }

    public TagsSelectorViewModel TagsSelector { get; }

    public IList<TrackViewModel> Tracks { get; private set; } = new ObservableCollection<TrackViewModel>();

    public void Enqueue() => this.queue.Enqueue(this.selectedTrack.Track);

    public async void LaunchFolder()
    {
        var file = this.SelectedTrack.Track.File;
        var folder = await file.GetParentAsync();

        await Launcher.LaunchFolderAsync(folder, new FolderLauncherOptions { ItemsToSelect = { file } });
    }

    public void Play()
    {
        var index = this.Tracks.IndexOf(this.SelectedTrack);

        if (index == this.queue.CurrentIndex)
            this.mainViewModel.Play();
        else if (index == this.queue.CurrentIndex + 1)
            this.mainViewModel.PlayNext();
        else
        {
            this.queue.AddNextUp(this.SelectedTrack.Track);
            this.mainViewModel.PlayNext();
        }
    }

    public void ToggleBestTimeOfDay(TagEditorItem item)
    {
        this.EditingTagsFor.ToggleBestTimeOfDay(item);
        item.IsChecked = true;
    }

    private ObservableCollection<TagGroup> CreateTagGroups()
    {
        var groups = new List<TagGroup>(8);

        foreach (var (name, tags) in App.Current.AppSettings.TagGroups)
        {
            groups.Add(new(name, tags));
        }

        groups.Add(new("Time of day", Enum.GetNames<TimesOfDay>().AsSpan(1)));
        groups.Add(new("Weather", Enum.GetNames<Weather>().AsSpan(1)));
        groups.Add(new("Others"));

        return [.. groups];
    }
}
