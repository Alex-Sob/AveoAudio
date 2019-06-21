using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Windows.UI.Xaml;

namespace AveoAudio.ViewModels
{
    public class NavigationViewModel : NotificationBase
    {
        private object currentView;
        private Stack<object> views = new Stack<object>();
        private TaskCompletionSource<object> navCompletion;

        public bool CanGoBack => views.Count > 1;

        public object CurrentView
        {
            get => this.currentView;
            set => this.SetProperty(ref this.currentView, value);
        }

        public string Title { get; set; }

        public void GoBack()
        {
            if (!this.CanGoBack) return;

            this.views.Pop();
            this.CurrentView = this.views.Peek();
            this.OnPropertyChanged(nameof(this.CanGoBack));

            this.navCompletion.SetResult(null);
            this.navCompletion = null;
        }

        public Task NavigateAsync<TView>(object viewModel, string title = null)
            where TView : FrameworkElement
        {
            this.Title = title;
            this.OnPropertyChanged(nameof(Title));

            var view = Activator.CreateInstance<TView>();
            view.DataContext = viewModel;
            this.CurrentView = view;

            this.views.Push(view);
            this.OnPropertyChanged(nameof(this.CanGoBack));

            this.navCompletion = new TaskCompletionSource<object>();
            return this.navCompletion.Task;
        }
    }
}
