namespace AveoAudio;

public sealed class TrackEventArgs(Track track) : EventArgs
{
    public Track Track { get; } = track;
}
