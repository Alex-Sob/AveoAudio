using AveoAudio.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using System.ComponentModel;

namespace AveoAudio.Views;

public sealed partial class TracklistView : UserControl
{
    public static readonly DependencyProperty TrackInfoTemplateProperty = DependencyProperty.Register(
        "TrackInfoTemplate",
        typeof(DataTemplate),
        typeof(TracklistView),
        new PropertyMetadata(null));

    public TracklistView()
    {
        this.InitializeComponent();

        this.TrackInfoTemplate = (DataTemplate)this.Resources["DefaultTrackInfoTemplate"];
        this.DataContextChanged += this.OnDataContextChanged;
    }

    public DataTemplate TrackInfoTemplate
    {
        get { return (DataTemplate)GetValue(TrackInfoTemplateProperty); }
        set { SetValue(TrackInfoTemplateProperty, (DataTemplate)value); }
    }

    public TracklistViewModel? ViewModel { get; set; }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (this.ViewModel != null) this.ViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
        this.ViewModel = this.DataContext as TracklistViewModel;
        if (this.ViewModel != null) this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(this.ViewModel.SelectedTrack))
            this.tracklist.ScrollIntoView(this.ViewModel!.SelectedTrack);
    }

    private void OnScrollToBottom(object sender, RoutedEventArgs e)
    {
        if (this.tracklist.Items.Count > 0) this.tracklist.ScrollIntoView(this.tracklist.Items.Last());
    }

    private void OnScrollToTop(object sender, RoutedEventArgs e)
    {
        if (this.tracklist.Items.Count > 0) this.tracklist.ScrollIntoView(this.tracklist.Items.First());
    }

    private void OnTagsViewRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var toggleButton = FindAncestor<ToggleButton>((DependencyObject)e.OriginalSource);

        if (toggleButton != null)
        {
            this.ViewModel?.ToggleBestTimeOfDay((TagEditorItem)toggleButton.DataContext);
            e.Handled = true;
        }
    }

    private static T? FindAncestor<T>(DependencyObject obj)
    {
        var parent = VisualTreeHelper.GetParent(obj);

        for (; parent != null; parent = VisualTreeHelper.GetParent(parent))
        {
            if (parent is T result) return result;
        }

        return default;
    }
}
