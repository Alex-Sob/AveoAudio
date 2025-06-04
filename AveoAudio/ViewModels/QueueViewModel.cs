using System.Collections.Specialized;

namespace AveoAudio.ViewModels;

public class QueueViewModel : TracklistViewModel
{
    private readonly ListeningQueue queue;
    private int currentIndex = -1;

    public QueueViewModel(ListeningQueue queue) : base(queue)
    {
        this.queue = queue;

        this.queue.CollectionChanged += OnQueueChanged;
        this.queue.CurrentChanged += OnCurrentChanged;
    }

    public IList<TrackViewModel> Queue => this.Tracks;

    public void GoToCurrent() => this.SelectedTrack = this.Queue[this.currentIndex];

    private void Insert(int index) => this.Insert(this.queue[index], index);

    private void OnCurrentChanged(object? sender, EventArgs e)
    {
        if (this.currentIndex != -1) this.Queue[this.currentIndex].IsCurrent = false;

        this.currentIndex = this.queue.CurrentIndex;
        if (this.currentIndex != -1) this.Queue[this.currentIndex].IsCurrent = true;
    }

    private void OnQueueChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
            this.Insert(e.NewStartingIndex);
        else if (e.Action == NotifyCollectionChangedAction.Remove)
            this.Queue.RemoveAt(e.OldStartingIndex);
        else if (e.Action == NotifyCollectionChangedAction.Reset)
            this.Queue.Clear();
    }
}
