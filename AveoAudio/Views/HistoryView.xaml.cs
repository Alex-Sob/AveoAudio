using AveoAudio.ViewModels;

using System.Linq;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

namespace AveoAudio.Views;

public sealed partial class HistoryView : UserControl
{
    private HistoryViewModel viewModel;

    public HistoryView()
    {
        this.InitializeComponent();
        this.DataContextChanged += this.OnDataContextChanged;
    }

    public HistoryViewModel ViewModel
    {
        get => this.viewModel;
        set
        {
            if (this.viewModel != null) this.viewModel.PropertyChanged -= this.OnModelPropertyChanged;
            this.viewModel = value;
            if (this.viewModel != null) this.viewModel.PropertyChanged += this.OnModelPropertyChanged;
        }
    }

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(this.ViewModel.SelectedTrack))
            this.tracklist.ScrollIntoView(this.ViewModel.SelectedTrack);
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        this.ViewModel = this.DataContext as HistoryViewModel;
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
