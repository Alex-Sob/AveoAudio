using AveoAudio.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AveoAudio.Views;

public sealed partial class PlayerView : UserControl
{
    private Dictionary<string, ContentControl> tagControls = new(32);

    public PlayerView()
    {
        this.InitializeComponent();
        this.DataContextChanged += this.OnDataContextChanged;
    }

    public PlayerViewModel? ViewModel { get; set; }

    private static string GetResourceKey(Tag tag) => tag.AsSpan() switch
    {
        "public" => "TagTemplate_public",
        "private" => "TagTemplate_private",
        nameof(Weather.Sun) => "TagTemplate_Sun",
        _ when tag.HasToken => "BestTimeOfDayTagTemplate",
        "Winter" => "TagTemplate_Winter",
        _ => "DefaultTagTemplate"
    };

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (this.ViewModel != null) this.ViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
        this.ViewModel = this.DataContext as PlayerViewModel;
        if (this.ViewModel != null) this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
    }

    private void OnImageManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        var x = e.Cumulative.Translation.X;
        var y = e.Cumulative.Translation.Y;

        if (Math.Abs(y) < x)
            App.Current.MainViewModel.MoveNext();
        else if (x < -Math.Abs(y))
            App.Current.MainViewModel.MovePrevious();
        else if (y > -Math.Abs(x))
            App.Current.MainViewModel.RewindToStart();
    }

    private void OnPlayerElementTapped(object sender, TappedRoutedEventArgs e) => e.Handled = true;

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var track = this.ViewModel!.CurrentTrack;
        var currentTimeOfDay = App.Current.MainViewModel.Selectors.TimeOfDay ?? TimesOfDay.None;

        if (track == null || e.PropertyName != nameof(PlayerViewModel.CurrentTrack)) return;

        this.tags.Children.Clear();

        foreach (var tag in track.Tags)
        {
            if (!tag.IsTimeOfDay(out var timeOfDay) || track.IsBestTimeOfDay(timeOfDay) && timeOfDay.HasFlag(currentTimeOfDay))
            {
                var resourceKey = GetResourceKey(tag);
                var t = CommonTags.TryGetTag(tag, out var value) ? value : tag.ToString();

                ref var control = ref CollectionsMarshal.GetValueRefOrAddDefault(this.tagControls, t, out var exists);

                if (!exists)
                {
                    control = new ContentControl { Content = t, ContentTemplate = (DataTemplate)this.Resources[resourceKey] };
                }

                this.tags.Children.Add(control);
            }
        }
    }
}
