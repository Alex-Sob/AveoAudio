namespace AveoAudio.ViewModels;

public class LibraryViewModel : NotificationBase
{
    public TracklistViewModel NewTracks { get; }

    public TracklistViewModel SearchResults { get; }

    public string? SearchText { get; set; }

    public LibraryViewModel(ListeningQueue queue)
    {
        this.NewTracks = new(queue);
        this.SearchResults = new(queue);

        MusicLibrary.TrackAdded += OnTrackAdded;
    }

    public string MaxNewestTracksCount { get; set; } = "10";

    public void LoadNewest() => App.Current.GetBusy(LoadNewestAsync(), "Loading");

    public void Search() => App.Current.GetBusy(SearchAsync(), "Searching");

    private async Task LoadNewestAsync()
    {
        var maxCount = Convert.ToInt32(this.MaxNewestTracksCount);
        var tracks = await MusicLibrary.Current.LoadNewest(maxCount);
        this.NewTracks.Load(tracks);
    }

    private void OnTrackAdded(object? sender, TrackEventArgs e)
    {
        App.Current.Dispatch(() => this.NewTracks.Insert(e.Track, 0));
    }

    private async Task SearchAsync()
    {
        if (string.IsNullOrEmpty(this.SearchText)) return;

        var tracks = await MusicLibrary.Current.Search(this.SearchText);
        this.SearchResults.Load(tracks);
    }
}
