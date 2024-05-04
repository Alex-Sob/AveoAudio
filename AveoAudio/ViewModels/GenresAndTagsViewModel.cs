using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

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

            this.ToggleExcludeTagCommand = new DelegateCommand<string>(g => this.ToggleValue(this.PlaylistProfile.ExcludeTags, g));
            this.ToggleFilterTagCommand = new DelegateCommand<string>(t => this.ToggleValue(this.PlaylistProfile.FilterTags, t));
            this.ToggleGenreCommand = new DelegateCommand<string>(t => this.ToggleValue(this.PlaylistProfile.Genres, t));

            this.appState.PropertyChanged += this.OnAppStateChanged;

            this.InitializeGenres();
            this.InitializeTags();
        }

        public IList<string> Genres { get; private set; }

        public PlaylistProfile PlaylistProfile { get; private set; } = new PlaylistProfile();

        public IList<TagListItem> Tags { get; private set; }

        public ICommand ToggleExcludeTagCommand { get; private set; }

        public ICommand ToggleFilterTagCommand { get; private set; }

        public ICommand ToggleGenreCommand { get; private set; }

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
                let genre = folder.Name
                orderby genre
                select genre;

            this.Genres = new List<string>(folders.Count);
            this.Genres.AddRange(genres);
            this.PlaylistProfile.Genres.AddRange(this.Genres);

            this.OnPropertyChanged(nameof(this.Genres));
        }

        private void InitializeTags()
        {
            this.RefreshDefaultTags();

            var tags =
                from tag in this.settings.Tags ?? new string[0]
                orderby tag
                select new TagListItem
                {
                    Tag = tag,
                    Filter = this.PlaylistProfile.FilterTags.Contains(tag),
                    Exclude = this.PlaylistProfile.ExcludeTags.Contains(tag)
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
                    item.Filter = this.PlaylistProfile.FilterTags.Contains(item.Tag);
                    item.Exclude = this.PlaylistProfile.ExcludeTags.Contains(item.Tag);
                }
            }
        }

        private void RefreshDefaultTags()
        {
            if (this.settings.Selectors == null) return;

            this.PlaylistProfile.FilterTags.Clear();
            this.PlaylistProfile.ExcludeTags.Clear();

            RefreshDefaultTags(this.appState.TimeOfDay);
            RefreshDefaultTags(this.appState.Weather);
        }

        private void RefreshDefaultTags<T>(T? selectorEnum) where T : struct
        {
            this.settings.Selectors.TryGetValue(selectorEnum?.ToString() ?? "", out Selector selector);

            var filterTags = selector?.DefaultFilterTags ?? Enumerable.Empty<string>();
            var excludeTags = selector?.DefaultExcludeTags ?? Enumerable.Empty<string>();

            this.PlaylistProfile.FilterTags.AddRange(filterTags);
            this.PlaylistProfile.ExcludeTags.AddRange(excludeTags);
        }

        private void ToggleValue(ISet<string> set, string value)
        {
            if (set.Contains(value)) set.Remove(value);
            else set.Add(value);
        }
    }
}
