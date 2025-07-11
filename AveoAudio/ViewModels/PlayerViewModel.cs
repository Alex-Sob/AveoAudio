using System.Diagnostics.CodeAnalysis;

using Windows.Media.Playback;

namespace AveoAudio.ViewModels;

public class PlayerViewModel : NotificationBase
{
    private readonly MediaPlayer mediaPlayer;
    private readonly ListeningQueue queue;

    private Track? currentTrack;

    public PlayerViewModel(MediaPlayer mediaPlayer, ListeningQueue queue)
    {
        this.mediaPlayer = mediaPlayer;
        this.queue = queue;

        queue.CurrentChanged += OnCurrentTrackChanged;
        this.mediaPlayer.PlaybackSession.PlaybackStateChanged += this.OnPlaybackStateChanged;

        BluetoothManager.DeviceConnected += this.OnConnectedDevicesChanged;
        BluetoothManager.DeviceDisconnected += this.OnConnectedDevicesChanged;
        BluetoothManager.WatchConnectedDevices();
    }

    public Track? CurrentTrack
    {
        get => this.currentTrack;
        set
        {
            if (this.SetProperty(ref this.currentTrack, value))
            {
                this.OnPropertyChanged(nameof(this.HasCurrentTrack));
                this.OnPropertyChanged(nameof(this.HasDateLastPlayed));
            }
        }
    }

    [MemberNotNullWhen(true, nameof(currentTrack))]
    public bool HasCurrentTrack => this.currentTrack != null;

    public bool HasDateLastPlayed => this.currentTrack?.LastPlayedOn.HasValue ?? false;

    public bool IsConnectedToDevice { get; set; }

    public bool IsNotConnectedToDevice => !this.IsConnectedToDevice;

    public bool IsPlaying => this.mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;

    public bool IsPaused => !this.IsPlaying;

    public void OpenBluetoothSettings() => _ = BluetoothManager.OpenSettingsAsync();

    public void RefreshImage() => _ = App.Current.MainViewModel.UpdateImageAsync();

    public void TogglePlayback()
    {
        var state = this.mediaPlayer.PlaybackSession.PlaybackState;

        if (state == MediaPlaybackState.Playing)
            this.mediaPlayer.Pause();
        else
            this.mediaPlayer.Play();
    }

    public void ViewTrackInQueue() => App.Current.MainViewModel.ViewTrackInQueue();

    private void OnConnectedDevicesChanged(object? sender, EventArgs args)
    {
        this.IsConnectedToDevice = BluetoothManager.HasConnectedDevices;

        App.Current.Dispatch(() =>
        {
            OnPropertyChanged(nameof(this.IsConnectedToDevice));
            OnPropertyChanged(nameof(this.IsNotConnectedToDevice));
        });
    }

    private void OnCurrentTrackChanged(object? sender, EventArgs e) => this.CurrentTrack = this.queue.Current;

    private void OnPlaybackStateChanged(MediaPlaybackSession sender, object args)
    {
        App.Current.Dispatch(() =>
        {
            this.OnPropertyChanged(nameof(this.IsPlaying));
            this.OnPropertyChanged(nameof(this.IsPaused));
        });
    }
}
