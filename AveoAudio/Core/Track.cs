using System.Globalization;

using Windows.Storage;
using Windows.Storage.FileProperties;

namespace AveoAudio;

public class Track
{
    public const string Extension = ".mp3";

    private string? name;

    static Track()
    {
        var settings = App.Current.AppSettings;

        for (int i = 0; i < settings.Tags.Count; i++)
        {
            CommonTags.AddTag(settings.Tags[i]);
        }

        foreach (var timeOfDay in Enum.GetNames<TimesOfDay>().AsSpan(1))
        {
            CommonTags.AddTag(timeOfDay);
        }

        CommonTags.AddTag(nameof(Weather.Sun));
        CommonTags.AddTag(nameof(Weather.Cloudy));
    }

    public CommonTags CommonTags { get; private set; }

    public DateTime? DateAdded { get; private set; }

    public required StorageFile File { get; init; }

    public string Name => this.name ?? (name = GetName(this.File.Name).ToString());

    public required string Genre { get; init; }

    public DateTime? LastPlayedOn => HistoryManager.GetLastPlayedOn(this);

    public required MusicProperties Properties { get; init; }

    public TagList Tags { get; private set; }

    public TimesOfDay TimesOfDay { get; private set; }

    public Weather Weather { get; private set; }

    public uint? Year => this.Properties.Year > 0 ? this.Properties.Year : null;

    public static event EventHandler<TrackEventArgs>? TagsUpdated;

    public static ReadOnlySpan<char> GetName(ReadOnlySpan<char> pathOrFileName) => Path.GetFileNameWithoutExtension(pathOrFileName);

    public static ReadOnlyMemory<char> GetName(ReadOnlyMemory<char> pathOrFileName)
    {
        var index = MemoryExtensions.LastIndexOf(pathOrFileName.Span, Path.DirectorySeparatorChar);
        var start = index >= 0 ? index + 1 : 0;

        return pathOrFileName.Slice(start, pathOrFileName.Length - start - Extension.Length);
    }

    public static async Task<Track> Load(StorageFile file, string genre)
    {
        var props = await file.Properties.GetMusicPropertiesAsync().AsTask().ConfigureAwait(false);

        var (dateAdded, tags) = ParseProperties(props);
        var (tagList, commonTags, timesOfDay, weather) = ParseTags(tags);

        return new Track
        {
            CommonTags = commonTags,
            DateAdded = dateAdded,
            Genre = genre,
            File = file,
            Properties = props,
            Tags = tagList,
            TimesOfDay = timesOfDay,
            Weather = weather
        };
    }

    public override string ToString() => this.Name;

    public async Task UpdateTags(string tags)
    {
        this.DateAdded ??= this.File.DateCreated.Date;
        this.Properties.Subtitle = $"{this.DateAdded:dd.MM.yyyy};{tags}";

        await this.Properties.SavePropertiesAsync();

        var (tagList, commonTags, timesOfDay, weather) = ParseTags(tags.AsMemory());

        this.CommonTags = commonTags;
        this.Tags = tagList;
        this.TimesOfDay = timesOfDay;
        this.Weather = weather;

        TagsUpdated?.Invoke(null, new TrackEventArgs(this));
    }

    private static (DateTime? dateAdded, ReadOnlyMemory<char> tags) ParseProperties(MusicProperties props)
    {
        var subtitle = props.Subtitle;

        if (subtitle.Length < 10) return (dateAdded: null, tags: ReadOnlyMemory<char>.Empty);

        var hasDate = DateTime.TryParseExact(subtitle.AsSpan(0, 10), "dd.MM.yyyy", null, DateTimeStyles.None, out var dateAdded);
        var rawTags = subtitle[10] == ';' ? subtitle.AsMemory(11) : ReadOnlyMemory<char>.Empty;

        return (hasDate ? dateAdded : null, rawTags);
    }

    private static (TagList, CommonTags, TimesOfDay, Weather) ParseTags(ReadOnlyMemory<char> tags)
    {
        var tagList = new TagList(tags);
        var commonTags = new CommonTags();
        var timesOfDay = default(TimesOfDay);
        var weather = Weather.None;

        foreach (var tag in tagList)
        {
            commonTags.TrySet(tag);

            if (Enum.TryParse<TimesOfDay>(tag, out var timeOfDay))
            {
                timesOfDay |= timeOfDay;
            }
            else if (tag.AsSpan().Equals(nameof(Weather.Sun), StringComparison.Ordinal)) weather = Weather.Sun;
        }

        if (weather == Weather.Sun && timesOfDay == TimesOfDay.None)
            timesOfDay = TimesOfDay.Daytime & ~TimesOfDay.Sunset;

        return (tagList, commonTags, timesOfDay, weather);
    }
}
