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
    }

    public Track? CurrentTrack
    {
        get => this.currentTrack;
        set
        {
            if (this.SetProperty(ref this.currentTrack, value))
            {
                this.OnPropertyChanged(nameof(this.HasCurrentTrack));
                this.OnPropertyChanged(nameof(this.DisplayTags));
                this.OnPropertyChanged(nameof(this.HasDateLastPlayed));
                this.OnPropertyChanged(nameof(this.HasTag));
            }
        }
    }

    public string? DisplayTags
    {
        get
        {
            if (!this.HasCurrentTrack || this.currentTrack.Tags.IsEmpty) return null;

            int current = 0;
            Span<char> span = stackalloc char[this.currentTrack!.Tags.Length + 1];

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

    [MemberNotNullWhen(true, nameof(currentTrack))]
    public bool HasCurrentTrack => this.currentTrack != null;

    public bool HasDateLastPlayed => this.currentTrack?.LastPlayedOn.HasValue ?? false;

    public bool HasTag(string tag) => HasCurrentTrack && this.currentTrack.CommonTags.HasTag(tag);

    public bool IsPlaying => this.mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;

    public bool IsPaused => !this.IsPlaying;

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
