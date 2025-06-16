using System.Collections.Specialized;

using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.Media.Core;
using Windows.Media.Playback;

namespace AveoAudio.ViewModels;

public class MainViewModel : NotificationBase
{
    private enum View
    {
        Filter,
        Player,
        Queue,
        History,
        Library
    }

    private readonly AppSettings appSettings;
    private readonly ImageManager imageManager;
    private readonly MediaPlayer mediaPlayer;
    private readonly ListeningQueue queue;

    private string? busyText;
    private ImageSource? imageSource;
    private bool listened;
    private MediaPlaybackList? playbackList;
    private int selectedPane;

    public MainViewModel(AppSettings appSettings, MediaPlayer mediaPlayer)
    {
        this.mediaPlayer = mediaPlayer;
        this.appSettings = appSettings;

        this.queue = new ListeningQueue(appSettings.PlaylistSize);
        this.imageManager = new ImageManager();

        this.Selectors = new SelectorsViewModel();
        this.Filter = new FilterViewModel(this.Selectors, this.appSettings);
        this.Player = new PlayerViewModel(mediaPlayer, this.queue);
        this.Queue = new QueueViewModel(this.queue);
        this.History = new HistoryViewModel(this.queue);
        this.Library = new LibraryViewModel(this.queue);

        Track.TagsUpdated += OnTagsUpdated;
        this.queue.CollectionChanged += this.OnQueueChanged;
        this.mediaPlayer.PlaybackSession.PositionChanged += this.OnPositionChanged;
    }

    public string? BusyText
    {
        get => this.busyText;
        set
        {
            if (this.SetProperty(ref this.busyText, value))
                this.OnPropertyChanged(nameof(this.IsBusy));
        }
    }

    public FilterViewModel Filter { get; }

    public HistoryViewModel History { get; }

    public ImageSource? Image
    {
        get => this.imageSource;
        set => this.SetProperty(ref this.imageSource, value);
    }

    public bool IsBusy => this.BusyText != null;

    public LibraryViewModel Library { get; }

    public PlayerViewModel Player { get; }

    public MediaPlaybackList? Playlist
    {
        get => this.playbackList;
        set => SetProperty(ref this.playbackList, value);
    }

    public QueueViewModel Queue { get; }

    public int SelectedPane
    {
        get => this.selectedPane;
        set => this.SetProperty(ref this.selectedPane, value);
    }

    public SelectorsViewModel Selectors { get; }

    private static Season Season => (Season)(DateTime.Today.Month / 3 % 4);

    private int CurrentTrackIndex => (int?)this.playbackList?.CurrentItemIndex ?? -1;

    public void RebuildPlaylist()
    {
        _ = this.GetBusy(this.RebuildPlaylistAsync(), "Building Playlist");
    }

    public async Task GetBusy(Task task, string description)
    {
        try
        {
            this.BusyText = description;
            await task;
        }
        finally
        {
            this.BusyText = null;
        }
    }

    public Task Initialize() => InitializeImage();

    public void MoveNext()
    {
        if (this.playbackList != null && this.CurrentTrackIndex < this.queue.Capacity - 1)
            this.playbackList.MoveNext();
    }

    public void MovePrevious()
    {
        if (this.playbackList != null && this.CurrentTrackIndex > 0)
            this.playbackList.MovePrevious();
    }

    public void OpenTilesPane()
    {
        this.Selectors.IsOpen = !this.Selectors.IsOpen;
    }

    public void Play()
    {
        ShowView(nameof(View.Player));
        this.mediaPlayer.Play();
    }

    public void PlayNext()
    {
        this.playbackList?.MoveNext();
        this.Play();
    }

    public void RewindToStart()
    {
        this.mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
    }

    public void ShowView(string name) => this.SelectedPane = (int)Enum.Parse<View>(name);

    public async Task UpdateImageAsync()
    {
        var timeOfDay = this.Selectors.SelectedTimeOfDay;
        var weather = this.Selectors.SelectedWeather;

        var imagePath = await this.imageManager.GetNextImage(Season.ToString(), timeOfDay, weather);
        this.Image = imagePath != null ? new BitmapImage(new Uri(imagePath)) : null;
    }

    public void ViewTrackInQueue()
    {
        ShowView(nameof(View.Queue));
        this.Queue.GoToCurrent();
    }

    private static void AddToPlaylist(MediaPlaybackList playlist, Track track)
    {
        var item = CreateMediaSource(track);
        playlist.Items.Add(item);
    }

    private static MediaPlaybackItem CreateMediaSource(Track track)
    {
        var item = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(track.File));

        var props = item.GetDisplayProperties();
        props.Type = Windows.Media.MediaPlaybackType.Music;
        props.MusicProperties.Artist = track.Properties.Artist;
        props.MusicProperties.Title = track.Properties.Title;
        item.ApplyDisplayProperties(props);

        return item;
    }

    private async Task RebuildPlaylistAsync()
    {
        this.mediaPlayer.Pause();

        if (this.Playlist != null) this.Playlist.CurrentItemChanged -= this.OnTrackChanged;
        this.Playlist = null;

        await MusicLibrary.Current.LoadByGenresAsync(this.Filter.SelectedGenres);
        var tracks = MusicLibrary.Current.GetByGenres(this.Filter.SelectedGenres);

        var builder = new PlaylistBuilder(tracks)
            .WithTimeOfDay(this.Selectors.TimeOfDay)
            .WithWeather(this.Selectors.Weather)
            .ExcludeTags(this.Filter.ExcludeTags)
            .FilterByTags(this.Filter.FilterTags)
            .ExcludeAlreadyPlayed(this.Filter.ExcludeAlreadyPlayed)
            .WithOutOfRotationTimeSinceAdded(this.Filter.OutOfRotationDaysSinceAdded)
            .WithOutOfRotationTimeSincePlayed(this.Filter.OutOfRotationDaysSincePlayed);

        var query = builder.Build().Shuffle().Take(this.appSettings.PlaylistSize);
        this.queue.Reload(query);

        RecreatePlaylist();
        await UpdateImageAsync();

        ShowView(nameof(View.Player));
    }

    private async Task InitializeImage()
    {
        var imagePath = await this.imageManager.GetNextDefaultImage();
        this.Image = imagePath != null ? new BitmapImage(new Uri(imagePath)) : null;
    }

    private void OnQueueChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            if (this.Playlist == null)
                RecreatePlaylist();
            else if (e.NewStartingIndex == this.Playlist.Items.Count)
                AddToPlaylist(this.Playlist, this.queue[e.NewStartingIndex]);
            else if (e.NewStartingIndex == this.queue.CurrentIndex + 1)
                this.Playlist.Items[e.NewStartingIndex] = CreateMediaSource(this.queue.Next!);
        }
    }

    private void OnPositionChanged(MediaPlaybackSession session, object args)
    {
        if (this.CurrentTrackIndex < 0) return;

        if (!this.listened && session.Position.TotalSeconds > session.NaturalDuration.TotalSeconds * 0.9)
        {
            this.listened = true;
            HistoryManager.Add(this.queue.Current!);

            App.Current.Dispatch(() =>
            {
                this.Queue.MarkCurrentAsPlayed();
                this.History.Add(this.queue.Current!);
            });
        }
    }

    private void OnTagsUpdated(object? sender, TrackEventArgs e)
    {
        for (int i = 0; i < this.queue.Count; i++)
        {
            if (this.queue[i] == e.Track) this.Playlist!.Items[i] = CreateMediaSource(e.Track);
        }
    }

    private void OnTrackChanged(MediaPlaybackList playlist, CurrentMediaPlaybackItemChangedEventArgs args)
    {
        this.listened = false;
        var newIndex = this.CurrentTrackIndex;

        if (newIndex < 0 || newIndex >= this.queue.Capacity) return;

        App.Current.Dispatch(() =>
        {
            if (newIndex > this.queue.CurrentIndex)
            {
                this.queue.MoveNext();

                if (this.queue.HasNext)
                    AddToPlaylist(playlist, this.queue.Next);
            }
            else
            {
                this.queue.MoveBack();

                if (playlist.Items.Count > this.queue.Count + 1)
                    playlist.Items.RemoveAt(playlist.Items.Count - 1);
            }
        });
    }

    private void RecreatePlaylist()
    {
        if (!this.queue.HasNext)
        {
            this.Playlist = null;
            return;
        }

        var playlist = new MediaPlaybackList();
        AddToPlaylist(playlist, this.queue[0]);

        this.Playlist = playlist;
        playlist.CurrentItemChanged += this.OnTrackChanged;
    }
}
