using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AveoAudio.ViewModels;

public class FilterViewModel : NotificationBase
{
    private readonly SelectorsViewModel selectors;
    private readonly AppSettings settings;

    public FilterViewModel(SelectorsViewModel selectors, AppSettings settings)
    {
        this.selectors = selectors;
        this.settings = settings;

        this.ToggleExcludeTagCommand = new DelegateCommand<string>(t => ToggleValue(this.ExcludeTags, t));
        this.ToggleFilterTagCommand = new DelegateCommand<string>(t => ToggleValue(this.SelectedTags, t));
        this.ToggleGenreCommand = new DelegateCommand<string>(g => ToggleValue(this.SelectedGenres, g));

        this.selectors.PropertyChanged += this.OnAppStateChanged;
    }

    public ICollection<string> ExcludeTags { get; } = new HashSet<string>(16);

    public ICollection<string> SelectedTags { get; } = new HashSet<string>();

    public IReadOnlyList<string> Genres { get; private set; }

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

    public ICollection<string> SelectedGenres { get; } = new HashSet<string>(16);

    public IReadOnlyList<string> Tags { get; private set; }

    public ICommand ToggleExcludeTagCommand { get; private set; }

    public ICommand ToggleFilterTagCommand { get; private set; }

    public ICommand ToggleGenreCommand { get; private set; }

    private UserSettings UserSettings => App.Current.UserSettings;

    public Task Configure() => SettingsManager.OpenLocalSettings();

    public bool ExcludesTag(string tag) => this.ExcludeTags.Contains(tag);

    public bool HasGenre(string genre) => this.SelectedGenres.Contains(genre);

    public bool HasFilterTag(string tag) => this.SelectedTags.Contains(tag);

    public async Task Initialize()
    {
        this.Tags = this.settings.Tags;

        this.Genres = await MusicLibrary.GetGenres();
        this.OnPropertyChanged(nameof(this.Genres));

        this.ApplyDefaults();
    }

    private static void ToggleValue(ICollection<string> collection, string value)
    {
        if (!collection.Remove(value)) collection.Add(value);
    }

    private void OnAppStateChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectorsViewModel.TimeOfDay) || e.PropertyName == nameof(SelectorsViewModel.Weather))
        {
            this.ApplyDefaults();
        }
    }

    private void ApplyDefaults()
    {
        if (this.settings.FilterDefaults == null) return;

        this.SelectedTags.Clear();
        this.ExcludeTags.Clear();
        this.SelectedGenres.Clear();

        ApplyDefaults(this.selectors.TimeOfDay);
        ApplyDefaults(this.selectors.Weather);

        this.OnPropertyChanged(nameof(this.HasGenre));
        this.OnPropertyChanged(nameof(this.HasFilterTag));
        this.OnPropertyChanged(nameof(this.ExcludesTag));
    }

    private void ApplyDefaults<TEnum>(TEnum? value) where TEnum : struct
    {
        this.settings.FilterDefaults.TryGetValue(value?.ToString() ?? "", out FilterDefaults defaults);

        this.SelectedGenres.AddRange(defaults?.Genres ?? []);
        this.SelectedTags.AddRange(defaults?.Tags ?? []);
        this.ExcludeTags.AddRange(defaults?.ExcludeTags ?? []);
    }
}
