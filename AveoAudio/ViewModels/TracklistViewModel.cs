using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System;

namespace AveoAudio.ViewModels
{
    public class TracklistViewModel : NotificationBase
    {
        private readonly MainViewModel mainViewModel;
        private readonly TrackDataParser trackDataParser;

        private TrackViewModel selectedTrack;
        private bool showAll = true;

        public TracklistViewModel(MainViewModel mainViewModel, TrackDataParser trackDataParser)
        {
            this.mainViewModel = mainViewModel;
            this.trackDataParser = trackDataParser;
            this.PlayTrackCommand = new DelegateCommand(this.PlayTrack);
            this.SwitchViewCommand = new DelegateCommand<string>(this.SwitchView);
        }

        public ICommand PlayTrackCommand { get; set; }

        public ICommand SwitchViewCommand { get; set; }

        public TrackViewModel SelectedTrack
        {
            get => this.selectedTrack;
            set
            {
                if (this.selectedTrack != null) this.selectedTrack.IsSelected = false;
                this.SetProperty(ref this.selectedTrack, value);
                if (this.selectedTrack != null) this.selectedTrack.IsSelected = true;
            }
        }

        public IList<TrackViewModel> Tracks { get; private set; } = new ObservableCollection<TrackViewModel>();

        public event EventHandler ScrollToTrack;

        public void AddTrack(Track track)
        {
            this.Tracks.Add(new TrackViewModel(track) { IsVisible = this.showAll });
        }

        public void GoToTrack(int index)
        {
            this.SelectedTrack = this.Tracks[index];
            this.ScrollToTrack?.Invoke(this, EventArgs.Empty);
        }

        public async void LaunchFolder()
        {
            var file = this.SelectedTrack.Track.File;
            var folder = await file.GetParentAsync();
            await Launcher.LaunchFolderAsync(folder, new FolderLauncherOptions { ItemsToSelect = { file } });
        }

        public void PlayTrack()
        {
            this.mainViewModel.Play(this.Tracks.IndexOf(this.selectedTrack));
        }

        public void SaveTags()
        {
            var tasks =
                from editor in this.Tracks
                where editor.HasChanges
                select editor.SaveChangesAsync(this.trackDataParser);

            this.mainViewModel.GetBusy(Task.WhenAll(tasks), "Saving");
        }

        private void SwitchView(string view)
        {
            this.showAll = (view == "All");

            foreach (var track in this.Tracks)
                track.IsVisible = this.showAll || track.Played;
        }
    }
}
