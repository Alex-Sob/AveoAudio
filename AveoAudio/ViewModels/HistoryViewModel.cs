using System.Collections.Generic;

namespace AveoAudio.ViewModels;

public class HistoryViewModel(ListeningQueue queue, MainViewModel mainViewModel) : TracklistViewModel(queue, mainViewModel)
{
    private readonly MainViewModel mainViewModel = mainViewModel;

    public IList<TrackViewModel> History => this.Tracks;

    public void Add(Track track) => this.History.Add(new TrackViewModel(this, track));

    public void Sync() => this.mainViewModel.GetBusy(HistoryManager.Sync(), "Syncing");
}
