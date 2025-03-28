using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;

using Windows.Storage;
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
    
    public static (DateTime dateAdded, string rawTags) ExtractCustomProperties(StorageFile file, MusicProperties props)
    {
        if (props.Subtitle.Length < 10) return (file.DateCreated.Date, rawTags: "");

        var hasDate = DateTime.TryParseExact(props.Subtitle.AsSpan(0, 10), "dd.MM.yyyy", null, DateTimeStyles.None, out var dateAdded);
        dateAdded = hasDate ? dateAdded : file.DateCreated.Date;

        var rawTags = props.Subtitle[10] == ';' ? props.Subtitle[11..] : "";

        return (dateAdded, rawTags);
    }

    public void ParseTags(Track track, string rawTags)
    {
        var timesOfDay = default(TimesOfDay);
        var customTags = new BitVector32();
        var weather = Weather.None;

        track.Tags = new TagList(rawTags);

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
