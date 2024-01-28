using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Windows.ApplicationModel.Core;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Dispatching;

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
        private readonly PlaylistBuilder playlistBuilder;
        private readonly AppSettings settings;

        private string busyText;
        private Track currentTrack;
        private ImageSource imageSource;
        private MediaPlaybackList playlist;
        private int selectedPane;
        private IList<Track> tracks;

        public MainViewModel(AppSettings settings, MediaPlayer mediaPlayer, DispatcherQueue dispatcherQueue)
        {
            this.mediaPlayer = mediaPlayer;
            this.settings = settings;

            this.imageManager = new ImageManager();

            var trackManager = new TrackManager(this.settings);
            this.playlistBuilder = new PlaylistBuilder(this.settings, this.appState, trackManager);
            this.dispatcherQueue = dispatcherQueue;

            this.Selectors = new SelectorsViewModel(this.appState);
            this.GenresAndTags = new GenresAndTagsViewModel(this.appState, this.settings);
            this.Tracklist = new TracklistViewModel(this, trackManager);

            this.mediaPlayer.PlaybackSession.PositionChanged += this.OnPositionChanged;

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
                    this.OnPropertyChanged(nameof(this.HasCurrentTrack));
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

        public MediaPlaybackList Playlist
        {
            get => this.playlist;
            set => SetProperty(ref this.playlist, value);
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

        private int CurrentTrackIndex => (int)this.playlist.CurrentItemIndex;

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
            if (this.playlist != null && this.CurrentTrackIndex < this.tracks.Count - 1)
                this.playlist.MoveNext();
        }

        public void MovePrevious()
        {
            if (this.playlist != null && this.CurrentTrackIndex > 0)
                this.playlist.MovePrevious();
        }

        public void OpenTilesPane()
        {
            this.Selectors.IsOpen = !this.Selectors.IsOpen;
        }

        public void Play(int index)
        {
            this.playlist.MoveTo((uint)index);
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

        private static void AddTracks(MediaPlaybackList playlist, IEnumerable<Track> tracks)
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

            if (this.playlist != null) this.playlist.CurrentItemChanged -= this.OnTrackChanged;
            this.Playlist = null;

            var playlistProfile = this.GenresAndTags.PlaylistProfile;
            this.tracks = await this.playlistBuilder.BuildPlaylistAsync(playlistProfile);

            var playlist = new MediaPlaybackList();
            AddTracks(playlist, this.tracks);
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

        private void OnPositionChanged(MediaPlaybackSession sender, object args)
        {
            if (this.CurrentTrackIndex < 0) return;

            var session = this.mediaPlayer.PlaybackSession;
            var trackViewModel = this.Tracklist.Tracks[this.CurrentTrackIndex];

            if (!trackViewModel.Played && session.Position.TotalSeconds > session.NaturalDuration.TotalSeconds / 2)
            {
                trackViewModel.Played = true;
                StateManager.SetLastPlayed(this.currentTrack);
            }
        }

        private void OnTrackChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            var index = this.CurrentTrackIndex;
            if (index < 0 || index >= this.tracks.Count) return;

            var track = this.tracks[index];
            this.dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => this.CurrentTrack = track);

            if (index == this.Tracklist.Tracks.Count)
            {
                this.dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => this.Tracklist.AddTrack(this.currentTrack));
            }
        }
    }
}
