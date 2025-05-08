using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AveoAudio.ViewModels;

public class HistoryViewModel(ListeningQueue queue, MainViewModel mainViewModel)
    : TracklistViewModel(queue, mainViewModel)
{
    public IList<TrackViewModel> History => this.Tracks;

    public string Period { get; set; } = "1w";

    public void Add(Track track) => this.History.Insert(0, new TrackViewModel(this, track) { DatePlayed = DateTime.Today });

    public void Load() => App.Current.GetBusy(LoadAsync(), "Loading");

    public void Sync() => App.Current.GetBusy(HistoryManager.Sync(), "Syncing");

    private async Task LoadAsync()
    {
        var entries = await HistoryManager.Load(GetStartDate(), DateTime.Today.AddDays(1));
        var tracks = entries.Select(e => new TrackViewModel(this, e.Track) { DatePlayed = e.Date });

        this.Tracks.Clear();
        this.Tracks.AddRange(tracks);
    }

    private DateTime GetStartDate()
    {
        var num = int.Parse(this.Period.AsSpan()[..1]);

        return this.Period.AsSpan()[1..] switch
        {
            "w" => DateTime.Today.AddDays(-num * 7),
            "m" => DateTime.Today.AddMonths(-num),
            _ => DateTime.Today
        };
    }
}
