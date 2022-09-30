using System.Threading.Tasks;

namespace AveoAudio.ViewModels
{
    public class TrackViewModel : NotificationBase
    {
        private bool isSelected;
        private bool isVisible;

        public TrackViewModel(Track track)
        {
            this.Track = track;
            this.Value = track.RawTags;
        }

        public bool HasChanges => this.Value != this.Track.RawTags;

        public bool IsSelected
        {
            get => this.isSelected;
            set => this.SetProperty(ref this.isSelected, value);
        }

        public bool IsVisible
        {
            get => this.isVisible;
            set => this.SetProperty(ref this.isVisible, value);
        }

        public bool Played { get; set; }

        public Track Track { get; }

        public string Value { get; set; }
    }
}
