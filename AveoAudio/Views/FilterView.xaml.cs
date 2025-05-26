using AveoAudio.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AveoAudio.Views;

public sealed partial class FilterView : UserControl
{
    public FilterView()
    {
        this.InitializeComponent();
        this.DataContextChanged += OnDataContextChanged;
    }

    public FilterViewModel? ViewModel { get; set; }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        this.ViewModel = DataContext as FilterViewModel;
    }
}
