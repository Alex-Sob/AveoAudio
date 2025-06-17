using System.ComponentModel;
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

        this.ToggleGenreCommand = new DelegateCommand<string>(g => ToggleValue(this.SelectedGenres, g ?? throw new ArgumentNullException()));

        this.FilterTagsSelector = CreateTagsSelector(this.FilterTags);
        this.ExcludeTagsSelector = CreateTagsSelector(this.ExcludeTags);

        this.Genres = MusicLibrary.Current.Genres;
        this.ApplyDefaults();

        this.selectors.PropertyChanged += this.OnAppStateChanged;
    }

    public bool ExcludeAlreadyPlayed => this.SuggestIfLastPlayed == "no";

    public ICollection<string> ExcludeTags { get; } = new HashSet<string>(16);

    public TagsSelectorViewModel ExcludeTagsSelector { get; private set; }

    public bool FilterByDateAdded { get; set; }

    public bool FilterByDatePlayed { get; set; }

    public ICollection<string> FilterTags { get; } = new HashSet<string>();

    public TagsSelectorViewModel FilterTagsSelector { get; private set; }

    public IReadOnlyCollection<string> Genres { get; private set; }

    public int OutOfRotationDaysSinceAdded => this.FilterByDateAdded ? GetOutOfRotationDays(this.SuggestIfAdded) : 0;

    public int OutOfRotationDaysSincePlayed
    {
        get => this.FilterByDatePlayed && this.SuggestIfLastPlayed != "no" ? GetOutOfRotationDays(this.SuggestIfLastPlayed) : 0;
    }

    public string SuggestIfAdded { get; set; } = "1y";

    public string SuggestIfLastPlayed { get; set; } = "0.5y";

    public ICollection<string> SelectedGenres { get; } = new HashSet<string>(16);

    public ICommand ToggleGenreCommand { get; private set; }

    public Task Configure() => SettingsManager.OpenLocalSettings();

    public bool HasGenre(string genre) => this.SelectedGenres.Contains(genre);

    public void RebuildPlaylist() => App.Current.MainViewModel.RebuildPlaylist();

    private static int GetOutOfRotationDays(string value) => (int)double.Parse(value.AsSpan()[..^1]) * 365;

    private static ICollection<TagGroup> CreateTagGroups()
    {
        var groups = new List<TagGroup>(8);

        foreach (var (name, tags) in App.Current.AppSettings.TagGroups)
        {
            groups.Add(new(name, tags));
        }

        return groups;
    }

    private static TagsSelectorViewModel CreateTagsSelector(ICollection<string> selectedTags)
    {
        var toggleTagCommand = new DelegateCommand<TagEditorItem>(t => ToggleValue(selectedTags, t ?? throw new ArgumentNullException()));
        return new(CreateTagGroups(), toggleTagCommand);
    }

    private static void ToggleValue(ICollection<string> collection, string value)
    {
        if (!collection.Remove(value)) collection.Add(value);
    }

    private void OnAppStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectorsViewModel.TimeOfDay) || e.PropertyName == nameof(SelectorsViewModel.Weather))
        {
            this.ApplyDefaults();
        }
    }

    private void ApplyDefaults()
    {
        if (this.settings.FilterDefaults == null) return;

        this.FilterTags.Clear();
        this.ExcludeTags.Clear();
        this.SelectedGenres.Clear();

        ApplyDefaults(this.selectors.TimeOfDay);
        ApplyDefaults(this.selectors.Weather);
        ExcludeSeasons();

        this.OnPropertyChanged(nameof(this.HasGenre));

        this.FilterTagsSelector.SelectTags(this.FilterTags);
        this.ExcludeTagsSelector.SelectTags(this.ExcludeTags);
    }

    private void ApplyDefaults<TEnum>(TEnum? value) where TEnum : struct
    {
        this.settings.FilterDefaults.TryGetValue(value?.ToString() ?? "", out FilterDefaults? defaults);

        this.SelectedGenres.AddRange(defaults?.Genres ?? []);
        this.FilterTags.AddRange(defaults?.Tags ?? []);
        this.ExcludeTags.AddRange(defaults?.ExcludeTags ?? []);
    }

    private void ExcludeSeasons()
    {
        Span<Season> seasons = [Season.Autumn, Season.Spring, Season.Summer, Season.Winter];

        foreach (var season in seasons)
        {
            if (season != Season.Current) this.ExcludeTags.Add(season);
        }
    }
}
