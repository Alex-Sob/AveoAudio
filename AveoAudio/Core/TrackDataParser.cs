using System.Collections.Specialized;
using System.Globalization;

using Windows.Storage.FileProperties;

namespace AveoAudio;

internal class TrackDataParser
{
    private readonly Dictionary<string, int> customTagsMap;

    public TrackDataParser(AppSettings appSettings)
    {
        this.customTagsMap = new Dictionary<string, int>(appSettings.Tags.Count);

        for (int i = 0; i < appSettings.Tags.Count; i++)
        {
            this.customTagsMap[appSettings.Tags[i]] = i;
        }
    }
    
    public static (DateTime? dateAdded, ReadOnlyMemory<char> tags) ExtractCustomProperties(MusicProperties props)
    {
        var subtitle = props.Subtitle;

        if (subtitle.Length < 10) return (dateAdded: null, tags: ReadOnlyMemory<char>.Empty);

        var hasDate = DateTime.TryParseExact(subtitle.AsSpan(0, 10), "dd.MM.yyyy", null, DateTimeStyles.None, out var dateAdded);
        var rawTags = subtitle[10] == ';' ? subtitle.AsMemory(11) : ReadOnlyMemory<char>.Empty;

        return (hasDate ? dateAdded : null, rawTags);
    }

    public void ParseTags(Track track, ReadOnlyMemory<char> tags)
    {
        var timesOfDay = default(TimesOfDay);
        var customTags = new BitVector32();
        var weather = Weather.None;

        track.Tags = new TagList(tags);

        var alternate = this.customTagsMap.GetAlternateLookup<ReadOnlySpan<char>>();

        foreach (var tag in track.Tags)
        {
            if (Enum.TryParse<TimesOfDay>(tag, out var timeOfDay))
            {
                timesOfDay |= timeOfDay;
            }
            else if (alternate.TryGetValue(tag, out var index))
            {
                customTags[1 << index] = true;
            }
            else if (weather == Weather.None) Enum.TryParse(tag, out weather);
        }

        if (weather == Weather.Sun && timesOfDay == TimesOfDay.None)
            timesOfDay = TimesOfDay.Daytime & ~TimesOfDay.Sunset;

        track.TimesOfDay = timesOfDay;
        track.CustomTags = customTags;
        track.Weather = weather;
    }
}
