using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace AveoAudio
{
    public class TrackDataParser
    {
        private readonly AppSettings settings;
        private readonly IDictionary<ReadOnlyMemory<char>, int> customTagsMap;

        public TrackDataParser(AppSettings appSettings)
        {
            this.settings = appSettings;
            this.customTagsMap = new Dictionary<ReadOnlyMemory<char>, int>(appSettings.Tags.Count);

            for (int i = 0; i < appSettings.Tags.Count; i++)
            {
                this.customTagsMap[appSettings.Tags[i].AsMemory()] = i;
            }
        }
        
        public static (DateTime dateCreated, string rawTags) ExtractCustomProperties(StorageFile file, MusicProperties props)
        {
            if (props.Subtitle.Length < 10) return (file.DateCreated.Date, rawTags: null);

            var hasDate = DateTime.TryParseExact(props.Subtitle.AsSpan(0, 10), "dd.MM.yyyy", null, DateTimeStyles.None, out var dateCreated);
            dateCreated = hasDate ? dateCreated : file.DateCreated.Date;

            var rawTags = props.Subtitle[10] == ';' ? props.Subtitle.Substring(11) : null;

            return (dateCreated, rawTags);
        }

        public void ParseTags(Track track, string rawTags)
        {
            var timesOfDay = default(TimesOfDay);
            var customTags = new BitVector32();
            Weather weather = Weather.None;

            if (!string.IsNullOrEmpty(rawTags))
            {
                int current = 0;

                while (current <= rawTags.Length)
                {
                    var index = rawTags.IndexOf(',', current);
                    var length = index >= 0 ? index - current : rawTags.Length - current;
                    var tag = rawTags.AsSpan(current, length);

                    if (Enum.TryParse<TimesOfDay>(tag, out var timesOfDayValue))
                    {
                        timesOfDay |= timesOfDayValue;
                    }
                    else if (TryGetValue(customTagsMap, tag, out var customTagIndex))
                    {
                        customTags[1 << customTagIndex] = true;
                    }
                    else if (weather == Weather.None) Enum.TryParse(tag, out weather);

                    current += length + 1;
                }
            }

            track.RawTags = rawTags;
            track.TimesOfDay = timesOfDay;
            track.CustomTags = customTags;
            track.Weather = weather;
        }

        private static bool TryGetValue<TValue>(IDictionary<ReadOnlyMemory<char>, TValue> dictionary, ReadOnlySpan<char> tag, out TValue value)
        {
            foreach (var pair in dictionary)
            {
                if (tag.Equals(pair.Key.Span, StringComparison.Ordinal))
                {
                    value = pair.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
