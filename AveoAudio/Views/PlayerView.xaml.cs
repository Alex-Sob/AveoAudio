using AveoAudio.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace AveoAudio.Views;

public sealed partial class PlayerView : UserControl
{
    public PlayerView()
    {
        this.InitializeComponent();
        this.DataContextChanged += this.OnDataContextChanged;
    }

    public PlayerViewModel? ViewModel { get; set; }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        this.ViewModel = this.DataContext as PlayerViewModel;
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
}
