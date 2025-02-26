using System.Collections.Generic;

namespace AveoAudio.ViewModels;

public class HistoryViewModel(ListeningQueue queue, MainViewModel mainViewModel) : TracklistViewModel(queue, mainViewModel)
{
    public IList<TrackViewModel> History => this.Tracks;

    public void Add(Track track) => this.History.Add(new TrackViewModel(this, track));
}
