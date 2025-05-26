using AveoAudio.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AveoAudio.Views;

public sealed partial class QueueView : UserControl
{
    public QueueView()
    {
        this.InitializeComponent();
        this.DataContextChanged += this.OnDataContextChanged;
    }

    public QueueViewModel? ViewModel { get; set; }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        this.ViewModel = this.DataContext as QueueViewModel;
    }
}
