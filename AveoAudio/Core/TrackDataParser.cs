using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using Windows.Storage;
using Windows.Storage.FileProperties;

namespace AveoAudio
{
    public class TrackDataParser
    {
        private readonly AppSettings settings;
        private readonly IDictionary<ReadOnlyMemory<char>, TimesOfDay> timesOfDayMap;
        private readonly IDictionary<ReadOnlyMemory<char>, int> customTagsMap;

        public TrackDataParser(AppSettings appSettings)
        {
            this.settings = appSettings;
            this.timesOfDayMap = GetTimesOfDay();
            this.customTagsMap = new Dictionary<ReadOnlyMemory<char>, int>(appSettings.Tags.Count);

            for (int i = 0; i < appSettings.Tags.Count; i++)
            {
                this.customTagsMap[appSettings.Tags[i].AsMemory()] = i;
            }
        }
        
        public static (DateTime dateCreated, string rawTags) ExtractCustomProperties(StorageFile file, MusicProperties props)
        {
            if (props.Subtitle.Length < 10) return (file.DateCreated.Date, rawTags: null);

            var hasDate = TryParseDate(props.Subtitle.AsSpan(0, 10), out var dateCreated);
            dateCreated = hasDate ? dateCreated : file.DateCreated.Date;

            var rawTags = props.Subtitle[10] == ';' ? props.Subtitle.Substring(11) : null;

            return (dateCreated, rawTags);
        }

        public (TimesOfDay timesOfDay, BitVector32 customTags) ParseTags(string rawTags)
        {
            var timesOfDay = default(TimesOfDay);
            var customTags = new BitVector32();

            if (!string.IsNullOrEmpty(rawTags))
            {
                int current = 0;

                while (current <= rawTags.Length)
                {
                    var index = rawTags.IndexOf(',', current);
                    var length = index >= 0 ? index - current : rawTags.Length - current;
                    var tag = rawTags.AsSpan(current, length);

                    // TODO: Use Enum.TryParse<TEnum>(ReadOnlySpan<char>, TEnum) when migrated to .NET 6+
                    if (TryGetValue(timesOfDayMap, tag, out var timesOfDayValue))
                    {
                        timesOfDay |= timesOfDayValue;
                    }
                    else if (TryGetValue(customTagsMap, tag, out var customTagIndex))
                    {
                        customTags[1 << customTagIndex] = true;
                    }

                    current += length + 1;
                }
            }

            return (timesOfDay, customTags);
        }

        private static IDictionary<ReadOnlyMemory<char>, TimesOfDay> GetTimesOfDay()
        {
            var names = Enum.GetNames(typeof(TimesOfDay));
            var result = new Dictionary<ReadOnlyMemory<char>, TimesOfDay>(names.Length);

            foreach (var name in names)
            {
                result.Add(name.AsMemory(), Enum.Parse<TimesOfDay>(name));
            }

            return result;
        }

        private static bool TryGetValue<TValue>(IDictionary<ReadOnlyMemory<char>, TValue> dictionary, ReadOnlySpan<char> key, out TValue value)
        {
            foreach (var pair in dictionary)
            {
                if (key.Equals(pair.Key.Span, StringComparison.Ordinal))
                {
                    value = pair.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static bool TryParseDate(ReadOnlySpan<char> value, out DateTime date)
        {
            date = default;

            if (value[2] != '.' || value[5] != '.') return false;

            if (!TryParseInt(value.Slice(0, 2), out var day)) return false;
            if (!TryParseInt(value.Slice(3, 2), out var month)) return false;
            if (!TryParseInt(value.Slice(6, 4), out var year)) return false;

            date = new DateTime(year, month, day);
            return true;
        }

        private static bool TryParseInt(ReadOnlySpan<char> value, out int number)
        {
            number = 0;
            int m = 1;

            for (int i = value.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(value[i])) return false;

                number += (value[i] - '0') * m;
                m *= 10;
            }

            return true;
        }
    }
}
