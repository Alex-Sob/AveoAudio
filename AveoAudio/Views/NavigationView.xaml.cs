using AveoAudio.ViewModels;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace AveoAudio.Views
{
    public sealed partial class NavigationView : UserControl
    {
        public NavigationView()
        {
            this.InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        public NavigationViewModel ViewModel { get; set; }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            this.ViewModel = DataContext as NavigationViewModel;
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.X < 0 && this.ViewModel.CanGoBack)
            {
                this.ViewModel.GoBack();
                e.Handled = true;
            }
        }
    }
}
