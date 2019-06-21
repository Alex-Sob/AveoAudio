using AveoAudio.ViewModels;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AveoAudio.Views
{
    public sealed partial class SelectorsView : UserControl
    {
        public SelectorsView()
        {
            this.InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        public SelectorsViewModel ViewModel { get; set; }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            ViewModel = DataContext as SelectorsViewModel;
        }
    }
}
