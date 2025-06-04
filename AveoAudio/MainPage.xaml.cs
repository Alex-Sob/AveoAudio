using AveoAudio.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AveoAudio;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    public MainViewModel? ViewModel { get; private set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var settings = (AppSettings)e.Parameter;
        this.ViewModel = new MainViewModel(settings, this.mediaPlayer.MediaPlayer);
    }
}
