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
        private readonly ListeningQueue queue;

        private TrackViewModel selectedTrack;

        public TracklistViewModel(ListeningQueue queue, MainViewModel mainViewModel)
        {
            this.queue = queue;
            this.mainViewModel = mainViewModel;

            this.PlayCommand = new DelegateCommand(this.Play);
            this.EnqueueCommand = new DelegateCommand(this.Enqueue);
        }

        public ICommand EnqueueCommand { get; }

        public ICommand PlayCommand { get; }

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

        public void Enqueue() => this.queue.Enqueue(this.selectedTrack.Track);

        public async void LaunchFolder()
        {
            var file = this.SelectedTrack.Track.File;
            var folder = await file.GetParentAsync();

            await Launcher.LaunchFolderAsync(folder, new FolderLauncherOptions { ItemsToSelect = { file } });
        }

        public void Play()
        {
            var index = this.Tracks.IndexOf(this.SelectedTrack);

            if (index == this.queue.CurrentIndex)
                this.mainViewModel.Play();
            else if (index == this.queue.CurrentIndex + 1)
                this.mainViewModel.PlayNext();
            else
            {
                this.queue.AddNextUp(this.SelectedTrack.Track);
                this.mainViewModel.PlayNext();
            }
        }

        public void SaveTags()
        {
            var tasks =
                from editor in this.Tracks
                where editor.HasChanges
                select editor.Track.UpdateTags(editor.Value);

            this.mainViewModel.GetBusy(Task.WhenAll(tasks), "Saving");
        }
    }
}
