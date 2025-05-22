using System;

using AveoAudio.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

namespace AveoAudio;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    public MainViewModel ViewModel { get; private set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var settings = (AppSettings)e.Parameter;
        this.ViewModel = new MainViewModel(settings, this.mediaPlayer.MediaPlayer);
    }

    private void OnImageManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        var x = e.Cumulative.Translation.X;
        var y = e.Cumulative.Translation.Y;

        if (Math.Abs(y) < x)
            this.ViewModel.MoveNext();
        else if (x < -Math.Abs(y))
            this.ViewModel.MovePrevious();
        else if (y > -Math.Abs(x))
            this.ViewModel.RewindToStart();
    }

    private void OnPlayerElementTapped(object sender, TappedRoutedEventArgs e) => e.Handled = true;
}
