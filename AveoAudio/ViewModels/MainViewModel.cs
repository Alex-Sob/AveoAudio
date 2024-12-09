using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Dispatching;

using Windows.Media.Core;
using Windows.Media.Playback;

namespace AveoAudio.ViewModels
{
    public class MainViewModel : NotificationBase
    {
        private enum Pane
        {
            GenresAndTags,
            NowPlaying,
            Tracklist
        }

        private readonly AppState appState = new AppState();
        private readonly DispatcherQueue dispatcherQueue;
        private readonly ImageManager imageManager;
        private readonly MediaPlayer mediaPlayer;
        private readonly MusicLibrary musicLibrary;
        private readonly AppSettings settings;
        private readonly IList<Track> playlist = [];

        private string busyText;
        private Track currentTrack;
        private ImageSource imageSource;
        private MediaPlaybackList playbackList;
        private int selectedPane;

        public MainViewModel(AppSettings settings, MediaPlayer mediaPlayer, DispatcherQueue dispatcherQueue)
        {
            this.mediaPlayer = mediaPlayer;
            this.settings = settings;

            this.imageManager = new ImageManager();

            this.musicLibrary = new MusicLibrary();
            this.dispatcherQueue = dispatcherQueue;

            this.Selectors = new SelectorsViewModel(this.appState);
            this.GenresAndTags = new GenresAndTagsViewModel(this.appState, this.settings);
            this.Tracklist = new TracklistViewModel(this);

            this.mediaPlayer.PlaybackSession.PositionChanged += this.OnPositionChanged;
            this.mediaPlayer.PlaybackSession.PlaybackStateChanged += this.OnPlaybackStateChanged;

            this.Initialize();
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

        public bool CanBuildPlaylist => this.SelectedPane < (int)Pane.Tracklist;

        public bool CanFindInTracklist => this.SelectedPane == (int)Pane.NowPlaying;

        public bool CanSaveTags => this.SelectedPane == (int)Pane.Tracklist;

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

                foreach (var tag in this.currentTrack.Tags)
                {
                    if (Enum.TryParse<TimesOfDay>(tag, out var timesOfDay)) continue;
                    tag.CopyTo(span.Slice(current, tag.Length));
                    current += tag.Length;
                    span[current++] = ' ';
                }

                return span[..current].ToString();
            }
        }

        public GenresAndTagsViewModel GenresAndTags { get; private set; }

        public bool HasCurrentTrack => this.currentTrack != null;

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

        public int SelectedPane
        {
            get => this.selectedPane;
            set
            {
                if (this.SetProperty(ref this.selectedPane, value))
                {
                    this.OnPropertyChanged(nameof(this.CanBuildPlaylist));
                    this.OnPropertyChanged(nameof(this.CanSaveTags));
                    this.OnPropertyChanged(nameof(this.CanFindInTracklist));
                }
            }
        }

        public SelectorsViewModel Selectors { get; private set; }

        public TracklistViewModel Tracklist { get; private set; }

        private int CurrentTrackIndex => (int)this.playbackList.CurrentItemIndex;

        public void BuildPlaylist()
        {
            this.GetBusy(this.BuildPlaylistAsync(), "Building Playlist");
        }

        public void FindInTracklist()
        {
            this.SelectedPane = (int)Pane.Tracklist;
            if (this.HasCurrentTrack)
            {
                this.dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => this.Tracklist.GoToTrack(this.CurrentTrackIndex));
            }
        }

        public async void GetBusy(Task task, string description)
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

        public void MoveNext()
        {
            if (this.playbackList != null && this.CurrentTrackIndex < this.playlist.Count - 1)
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

        public void Play(int index)
        {
            this.playbackList.MoveTo((uint)index);
            this.SelectedPane = (int)Pane.NowPlaying;
            this.mediaPlayer.Play();
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

        private static void LoadPlaylist(MediaPlaybackList playlist, IEnumerable<Track> tracks)
        {
            foreach (var track in tracks)
            {
                var item = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(track.File));

                var props = item.GetDisplayProperties();
                props.Type = Windows.Media.MediaPlaybackType.Music;
                props.MusicProperties.Artist = track.Properties.Artist;
                props.MusicProperties.Title = track.Properties.Title;
                item.ApplyDisplayProperties(props);

                playlist.Items.Add(item);
            }
        }

        private async Task BuildPlaylistAsync()
        {
            this.mediaPlayer.Pause();
            this.Tracklist.Tracks.Clear();

            if (this.playbackList != null) this.playbackList.CurrentItemChanged -= this.OnTrackChanged;
            this.Playlist = null;
            
            var tracks = await this.musicLibrary.LoadTracksAsync(this.GenresAndTags.SelectedGenres);

            new PlaylistBuilder(tracks, this.settings)
                .WithTimeOfDay(this.appState.TimeOfDay)
                .WithWeather(this.appState.Weather)
                .ExcludeTags(this.GenresAndTags.ExcludingTags)
                .FilterByTags(this.GenresAndTags.FilterTags)
                .BuildPlaylist(this.playlist);

            var playlist = new MediaPlaybackList();
            LoadPlaylist(playlist, this.playlist);
            this.Playlist = playlist.Items.Any() ? playlist : null;
            playlist.CurrentItemChanged += this.OnTrackChanged;

            var timeOfDay = this.Selectors.SelectedTimeOfDay ?? "";
            var weather = this.Selectors.SelectedWeather ?? "";
            var imagePath = await this.imageManager.GetNextImage(timeOfDay, weather);
            this.Image = imagePath != null ? new BitmapImage(new Uri(imagePath)) : null;

            this.SelectedPane = (int)Pane.NowPlaying;
        }

        private async void Initialize()
        {
            var imagePath = await this.imageManager.GetNextDefaultImage();
            this.Image = imagePath != null ? new BitmapImage(new Uri(imagePath)) : null;
        }

        private void OnPlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            this.dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                this.OnPropertyChanged(nameof(this.IsPlaying));
                this.OnPropertyChanged(nameof(this.IsPaused));
            });
        }

        private void OnPositionChanged(MediaPlaybackSession sender, object args)
        {
            if (this.CurrentTrackIndex < 0) return;

            var session = this.mediaPlayer.PlaybackSession;
            var trackViewModel = this.Tracklist.Tracks[this.CurrentTrackIndex];

            if (!trackViewModel.Played && session.Position.TotalSeconds > session.NaturalDuration.TotalSeconds / 2)
            {
                trackViewModel.Played = true;
                StateManager.SetLastPlayedOn(this.currentTrack);
            }
        }

        private void OnTrackChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            var index = this.CurrentTrackIndex;
            if (index < 0 || index >= this.playlist.Count) return;

            var track = this.playlist[index];
            this.dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => this.CurrentTrack = track);

            if (index == this.Tracklist.Tracks.Count)
            {
                this.dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => this.Tracklist.AddTrack(this.currentTrack));
            }
        }
    }
}
