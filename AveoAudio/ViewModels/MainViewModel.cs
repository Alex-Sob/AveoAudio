using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.Media.Core;
using Windows.Media.Playback;

namespace AveoAudio.ViewModels;

public class MainViewModel : NotificationBase
{
    private enum Pane
    {
        Filter,
        Player,
        Queue,
        History
    }

    private readonly AppSettings appSettings;
    private readonly DispatcherQueue dispatcherQueue;
    private readonly ImageManager imageManager;
    private readonly MediaPlayer mediaPlayer;
    private readonly MusicLibrary musicLibrary;
    private readonly ListeningQueue queue;

    private string busyText;
    private Track currentTrack;
    private ImageSource imageSource;
    private bool listened;
    private MediaPlaybackList playbackList;
    private int selectedPane;

    public MainViewModel(AppSettings appSettings, MediaPlayer mediaPlayer, DispatcherQueue dispatcherQueue)
    {
        this.mediaPlayer = mediaPlayer;
        this.appSettings = appSettings;
        this.dispatcherQueue = dispatcherQueue;

        this.musicLibrary = new MusicLibrary();
        this.queue = new ListeningQueue(appSettings.PlaylistSize);
        this.imageManager = new ImageManager();

        this.Selectors = new SelectorsViewModel();
        this.Filter = new FilterViewModel(this.Selectors, this.appSettings);
        this.Queue = new QueueViewModel(this.queue, this);
        this.History = new HistoryViewModel(this.queue, this);

        this.queue.CollectionChanged += this.OnQueueChanged;
        this.mediaPlayer.PlaybackSession.PositionChanged += this.OnPositionChanged;
        this.mediaPlayer.PlaybackSession.PlaybackStateChanged += this.OnPlaybackStateChanged;
    }

    public string BusyText
    {
        get => this.busyText;
        set
        {
            if (this.SetProperty(ref this.busyText, value))
                this.OnPropertyChanged(nameof(this.IsBusy));
        }
    }

    public bool CanBuildPlaylist => this.SelectedPane < (int)Pane.Queue;

    public bool CanSaveTags => this.SelectedPane == (int)Pane.History;

    public Track CurrentTrack
    {
        get => this.currentTrack;
        set
        {
            if (this.SetProperty(ref this.currentTrack, value))
            {
                this.OnPropertyChanged(nameof(this.HasCurrentTrack));
                this.OnPropertyChanged(nameof(this.DisplayTags));
            }
        }
    }

    public string DisplayTags
    {
        get
        {
            if (!this.HasCurrentTrack) return null;

            int current = 0;
            Span<char> span = stackalloc char[this.currentTrack.Tags.Length + 1];

            foreach (ReadOnlySpan<char> tag in this.currentTrack.Tags)
            {
                if (Enum.TryParse<TimesOfDay>(tag, out _)) continue;
                tag.CopyTo(span.Slice(current, tag.Length));
                current += tag.Length;
                span[current++] = ' ';
            }

            return span[..current].ToString();
        }
    }

    public FilterViewModel Filter { get; }

    public bool HasCurrentTrack => this.currentTrack != null;

    public HistoryViewModel History { get; }

    public ImageSource Image
    {
        get => this.imageSource;
        set => this.SetProperty(ref this.imageSource, value);
    }

    public bool IsBusy => this.BusyText != null;

    public bool IsPlaying => this.mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;

    public bool IsPaused => !this.IsPlaying;

    public MediaPlaybackList Playlist
    {
        get => this.playbackList;
        set => SetProperty(ref this.playbackList, value);
    }

    public QueueViewModel Queue { get; }

    public int SelectedPane
    {
        get => this.selectedPane;
        set
        {
            if (this.SetProperty(ref this.selectedPane, value))
            {
                this.OnPropertyChanged(nameof(this.CanBuildPlaylist));
                this.OnPropertyChanged(nameof(this.CanSaveTags));
            }
        }
    }

    public SelectorsViewModel Selectors { get; }

    private int CurrentTrackIndex => (int)this.playbackList.CurrentItemIndex;

    private static UserSettings UserSettings => App.Current.UserSettings;

    public void BuildPlaylist()
    {
        _ = this.GetBusy(this.BuildPlaylistAsync(), "Building Playlist");
    }

    public void ViewInQueue()
    {
        ShowPane(Pane.Queue);
        if (this.HasCurrentTrack)
        {
            this.Queue.GoToTrack(this.CurrentTrackIndex);
        }
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

    public Task Initialize() => Task.WhenAll(this.Filter.Initialize(), InitializeImage());

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
        ShowPane(Pane.Player);
        this.mediaPlayer.Play();
    }

    public void PlayNext()
    {
        this.playbackList.MoveNext();
        this.Play();
    }

    public void RewindToStart()
    {
        this.mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
    }

    public void TogglePlayback()
    {
        var state = this.mediaPlayer.PlaybackSession.PlaybackState;

        if (state == MediaPlaybackState.Playing)
            this.mediaPlayer.Pause();
        else
            this.mediaPlayer.Play();
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

    private async Task BuildPlaylistAsync()
    {
        this.mediaPlayer.Pause();

        if (this.Playlist != null) this.Playlist.CurrentItemChanged -= this.OnTrackChanged;
        this.Playlist = null;

        var tracks = await this.musicLibrary.LoadTracksAsync(this.Filter.SelectedGenres);

        var builder = new PlaylistBuilder(tracks, this.appSettings)
            .WithTimeOfDay(this.Selectors.TimeOfDay)
            .WithWeather(this.Selectors.Weather)
            .ExcludeTags(this.Filter.ExcludeTags)
            .FilterByTags(this.Filter.SelectedTags)
            .WithOutOfRotationTimeSinceAdded(UserSettings.OutOfRotationDaysSinceAdded)
            .WithOutOfRotationTimeSincePlayed(UserSettings.OutOfRotationDaysSincePlayed);

        var query = builder.Build().Shuffle().Take(this.appSettings.PlaylistSize);

        UpdatePlaylist(query);
        await UpdateImageAsync();

        ShowPane(Pane.Player);
    }

    private void Dispatch(Action action)
    {
        this.dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => action());
    }

    private async Task InitializeImage()
    {
        var imagePath = await this.imageManager.GetNextDefaultImage();
        this.Image = imagePath != null ? new BitmapImage(new Uri(imagePath)) : null;
    }

    private void OnQueueChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewStartingIndex == this.queue.CurrentIndex + 1)
            this.Playlist.Items[this.CurrentTrackIndex + 1] = CreateMediaSource(this.queue.Next);
    }

    private void OnPlaybackStateChanged(MediaPlaybackSession sender, object args)
    {
        this.Dispatch(() =>
        {
            this.OnPropertyChanged(nameof(this.IsPlaying));
            this.OnPropertyChanged(nameof(this.IsPaused));
        });
    }

    private void OnPositionChanged(MediaPlaybackSession sender, object args)
    {
        if (this.CurrentTrackIndex < 0) return;

        var session = this.mediaPlayer.PlaybackSession;

        if (!this.listened && session.Position.TotalSeconds > session.NaturalDuration.TotalSeconds * 0.9)
        {
            this.listened = true;
            HistoryManager.Add(this.currentTrack);
            this.Dispatch(() => this.History.Add(this.currentTrack));
        }
    }

    private void OnTrackChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
    {
        this.listened = false;
        var newIndex = this.CurrentTrackIndex;

        if (newIndex < 0 || newIndex >= this.queue.Capacity) return;

        this.Dispatch(() =>
        {
            this.CurrentTrack = this.queue[newIndex];

            if (newIndex > this.queue.CurrentIndex)
            {
                this.queue.MoveNext();

                if (this.Playlist.Items.Count == this.queue.Count && this.queue.HasNext)
                    AddToPlaylist(this.Playlist, this.queue.Next);
            }
            else
            {
                this.queue.MoveBack();
            }
        });
    }

    private void ShowPane(Pane pane) => this.SelectedPane = (int)pane;

    private async Task UpdateImageAsync()
    {
        var timeOfDay = this.Selectors.SelectedTimeOfDay;
        var weather = this.Selectors.SelectedWeather;
        var imagePath = await this.imageManager.GetNextImage(timeOfDay, weather);
        this.Image = imagePath != null ? new BitmapImage(new Uri(imagePath)) : null;
    }

    private void UpdatePlaylist(IEnumerable<Track> query)
    {
        this.queue.Reload(query);

        var playlist = new MediaPlaybackList();
        AddToPlaylist(playlist, this.queue[0]);

        this.Playlist = playlist.Items.Any() ? playlist : null;
        playlist.CurrentItemChanged += this.OnTrackChanged;
    }
}
