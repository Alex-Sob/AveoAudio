using AveoAudio.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AveoAudio.Views;

public sealed partial class TagsSelectorView : UserControl
{
    public TagsSelectorView()
    {
        this.InitializeComponent();
        this.DataContextChanged += OnDataContextChanged;
    }

    public TagsSelectorViewModel ViewModel { get; set; }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        this.ViewModel = DataContext as TagsSelectorViewModel;
    }
}
