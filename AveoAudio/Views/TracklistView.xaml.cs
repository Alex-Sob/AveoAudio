using AveoAudio.ViewModels;

using System;
using System.Linq;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AveoAudio.Views
{
    public sealed partial class TracklistView : UserControl
    {
        private TracklistViewModel viewModel;

        public TracklistView()
        {
            this.InitializeComponent();
            this.DataContextChanged += this.OnDataContextChanged;
        }

        public TracklistViewModel ViewModel
        {
            get => this.viewModel;
            set
            {
                if (this.viewModel != null) this.viewModel.ScrollToTrack -= this.OnScrollToTrack;
                this.viewModel = value;
                if (this.viewModel != null) this.viewModel.ScrollToTrack += this.OnScrollToTrack;
            }
        }

        private void OnScrollToTrack(object sender, EventArgs e)
        {
            this.tracks.ScrollIntoView(this.ViewModel.SelectedTrack);
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            this.ViewModel = this.DataContext as TracklistViewModel;
        }

        private void OnScrollToBottom(object sender, RoutedEventArgs e)
        {
            if (this.tracks.Items.Count > 0) this.tracks.ScrollIntoView(this.tracks.Items.Last());
        }

        private void OnScrollToTop(object sender, RoutedEventArgs e)
        {
            if (this.tracks.Items.Count > 0) this.tracks.ScrollIntoView(this.tracks.Items.First());
        }
    }
}
