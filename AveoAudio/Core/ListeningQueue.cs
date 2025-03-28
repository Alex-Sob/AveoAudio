using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace AveoAudio;

public class ListeningQueue(int capacity) : INotifyCollectionChanged
{
    private readonly int capacity = capacity;
    private readonly List<Track> source = [];

    public Track this[int index] => this.source[index];

    public int Capacity => this.source.Count;

    public int Count { get; private set; }

    public Track Current => this.source[this.CurrentIndex];

    public int CurrentIndex { get; private set; } = -1;

    public bool HasNext => this.CurrentIndex < this.source.Count - 1;

    public Track Next => this.HasNext ? this.source[this.CurrentIndex + 1] : null;

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public event EventHandler CurrentChanged;

    public void AddNextUp(Track track)
    {
        Count++;
        this.source.Insert(this.CurrentIndex + 1, track);

        RaiseCollectionChanged(NotifyCollectionChangedAction.Add, this.CurrentIndex + 1);
    }

    public void Enqueue(Track track)
    {
        this.source.Insert(this.Count++, track);
        RaiseCollectionChanged(NotifyCollectionChangedAction.Add, this.Count - 1);
    }

    public void MoveNext()
    {
        if (++this.CurrentIndex == this.Count)
        {
            this.Count++;
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, this.CurrentIndex);
        }

        this.CurrentChanged?.Invoke(this, EventArgs.Empty);
    }

    public void MoveBack()
    {
        this.CurrentIndex--;
        this.CurrentChanged?.Invoke(this, EventArgs.Empty);
        this.Shrink();
    }

    public void Reload(IEnumerable<Track> tracks)
    {
        this.source.Clear();
        this.source.EnsureCapacity(this.capacity);
        this.source.AddRange(tracks);

        this.CurrentIndex = -1;
        this.CurrentChanged?.Invoke(this, EventArgs.Empty);

        this.Count = 0;
        this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void RaiseCollectionChanged(NotifyCollectionChangedAction action, int index)
    {
        this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changedItem: null, index));
    }

    private void Shrink()
    {
        for (var i = this.Count - 1; i > this.CurrentIndex; i--)
        {
            this.Count--;
            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, i);
        }
    }
}
