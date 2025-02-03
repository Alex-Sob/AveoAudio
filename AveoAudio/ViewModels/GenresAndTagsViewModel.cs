using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.UI.Xaml;

using Windows.Storage;

namespace AveoAudio.ViewModels
{
    public class GenresAndTagsViewModel : NotificationBase
    {
        private readonly AppState appState;
        private readonly AppSettings settings;

        public GenresAndTagsViewModel(AppState appState, AppSettings settings)
        {
            this.appState = appState;
            this.settings = settings;

            this.ToggleExcludeTagCommand = new DelegateCommand<string>(g => this.ToggleValue(this.ExcludingTags, g));
            this.ToggleFilterTagCommand = new DelegateCommand<string>(t => this.ToggleValue(this.FilterTags, t));
            this.ToggleGenreCommand = new DelegateCommand<string>(t => this.ToggleValue(this.SelectedGenres, t));

            this.appState.PropertyChanged += this.OnAppStateChanged;

            this.InitializeGenres();
            this.InitializeTags();
        }

        public ISet<string> ExcludingTags { get; } = new HashSet<string>();

        public ISet<string> FilterTags { get; } = new HashSet<string>();

        public IList<string> Genres { get; private set; }

        public string OutOfRotationDaysSinceAdded
        {
            get => this.UserSettings.OutOfRotationDaysSinceAdded.ToString();
            set => this.UserSettings.OutOfRotationDaysSinceAdded = Convert.ToInt32(value);
        }

        public string OutOfRotationDaysSincePlayed
        {
            get => this.UserSettings.OutOfRotationDaysSincePlayed.ToString();
            set => this.UserSettings.OutOfRotationDaysSincePlayed = Convert.ToInt32(value);
        }

        public ISet<string> SelectedGenres { get; } = new HashSet<string>();

        public IList<TagListItem> Tags { get; private set; }

        public ICommand ToggleExcludeTagCommand { get; private set; }

        public ICommand ToggleFilterTagCommand { get; private set; }

        public ICommand ToggleGenreCommand { get; private set; }

        private UserSettings UserSettings => ((App)Application.Current).UserSettings;

        public Task Configure() => SettingsManager.OpenLocalSettings();

        private static void Refresh<T>(ICollection<T> collection, IEnumerable<T> values)
        {
            collection.Clear();
            collection.AddRange(values);
        }

        private async void InitializeGenres()
        {
            var folders = await KnownFolders.MusicLibrary.GetFoldersAsync();

            var genres =
                from folder in folders
                where Directory.EnumerateFiles(folder.Path, "*.mp3").Any()
                let genre = folder.Name
                orderby genre
                select genre;

            this.Genres = new List<string>(folders.Count);
            this.Genres.AddRange(genres);
            this.SelectedGenres.AddRange(this.Genres);

            this.OnPropertyChanged(nameof(this.Genres));
        }

        private void InitializeTags()
        {
            this.RefreshDefaultTags();

            var tags =
                from tag in this.settings.Tags ?? []
                orderby tag
                select new TagListItem
                {
                    Tag = tag,
                    Filter = this.FilterTags.Contains(tag),
                    Exclude = this.ExcludingTags.Contains(tag)
                };

            this.Tags = tags.ToList();
        }

        private void OnAppStateChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppState.TimeOfDay) || e.PropertyName == nameof(AppState.Weather))
            {
                this.RefreshDefaultTags();

                foreach (var item in this.Tags)
                {
                    item.Filter = this.FilterTags.Contains(item.Tag);
                    item.Exclude = this.ExcludingTags.Contains(item.Tag);
                }
            }
        }

        private void RefreshDefaultTags()
        {
            if (this.settings.Selectors == null) return;

            this.FilterTags.Clear();
            this.ExcludingTags.Clear();

            RefreshDefaultTags(this.appState.TimeOfDay);
            RefreshDefaultTags(this.appState.Weather);
        }

        private void RefreshDefaultTags<T>(T? selectorEnum) where T : struct
        {
            this.settings.Selectors.TryGetValue(selectorEnum?.ToString() ?? "", out Selector selector);

            var filterTags = selector?.DefaultFilterTags ?? Enumerable.Empty<string>();
            var excludeTags = selector?.DefaultExcludeTags ?? Enumerable.Empty<string>();

            this.FilterTags.AddRange(filterTags);
            this.ExcludingTags.AddRange(excludeTags);
        }

        private void ToggleValue(ISet<string> set, string value)
        {
            if (set.Contains(value)) set.Remove(value);
            else set.Add(value);
        }
    }
}
