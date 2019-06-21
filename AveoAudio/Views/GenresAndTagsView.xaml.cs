using AveoAudio.ViewModels;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AveoAudio.Views
{
    public sealed partial class GenresAndTagsView : UserControl
    {
        public GenresAndTagsView()
        {
            this.InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        public GenresAndTagsViewModel ViewModel { get; set; }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            this.ViewModel = DataContext as GenresAndTagsViewModel;
        }
    }
}
