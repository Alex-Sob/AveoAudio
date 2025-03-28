using AveoAudio.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using System.ComponentModel;
using System.Linq;

namespace AveoAudio.Views;

public sealed partial class TracklistView : UserControl
{
    public TracklistView()
    {
        this.InitializeComponent();
        this.DataContextChanged += this.OnDataContextChanged;
    }

    public TracklistViewModel ViewModel { get; set; }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (this.ViewModel != null) this.ViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
        this.ViewModel = this.DataContext as QueueViewModel;
        if (this.ViewModel != null) this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(this.ViewModel.SelectedTrack))
            this.tracklist.ScrollIntoView(this.ViewModel.SelectedTrack);
    }

    private void OnScrollToBottom(object sender, RoutedEventArgs e)
    {
        if (this.tracklist.Items.Count > 0) this.tracklist.ScrollIntoView(this.tracklist.Items.Last());
    }

    private void OnScrollToTop(object sender, RoutedEventArgs e)
    {
        if (this.tracklist.Items.Count > 0) this.tracklist.ScrollIntoView(this.tracklist.Items.First());
    }
}
