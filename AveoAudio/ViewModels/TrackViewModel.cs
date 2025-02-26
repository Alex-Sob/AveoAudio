using System.Windows.Input;

namespace AveoAudio.ViewModels
{
    public class TrackViewModel(TracklistViewModel tracklist, Track track) : NotificationBase
    {
        private readonly TracklistViewModel tracklist = tracklist;

        private bool isCurrent;
        private bool isSelected;

        public ICommand EnqueueCommand => this.tracklist.EnqueueCommand;

        public bool HasChanges => this.Value != this.Track.Tags;

        public bool IsCurrent
        {
            get => this.isCurrent;
            set => this.SetProperty(ref this.isCurrent, value);
        }

        public bool IsSelected
        {
            get => this.isSelected;
            set => this.SetProperty(ref this.isSelected, value);
        }

        public ICommand PlayCommand => this.tracklist.PlayCommand;

        public Track Track { get; } = track;

        public string Value { get; set; } = track.Tags;
    }
}
