using System;
using System.Buffers.Text;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Streams;

namespace AveoAudio
{
    public static class HistoryManager
    {
        private const string FileName = "History.json";
        private const string LastPlayedDatesContainer = "LastPlayedDates";

        static HistoryManager()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.CreateContainer(LastPlayedDatesContainer, ApplicationDataCreateDisposition.Always);
        }

        private static ApplicationDataContainer Container => ApplicationData.Current.LocalSettings.Containers[LastPlayedDatesContainer];

        public static DateTime? GetLastPlayedOn(Track track)
        {
            var dates = GetLocalDates(track);
            return dates.Length > 0 ? dates[^1].DateTime : null;
        }

        public static void Add(Track track)
        {
            var dates = GetLocalDates(track);
            DateTimeOffset[] newDates = [.. dates, DateTimeOffset.Now.TruncateTime()];
            Container.Values[track.FileName] = newDates;
        }

        public static async Task Sync()
        {
            using var fileStream = await CreateOrOpenFile().ConfigureAwait(false);
            using var stream = fileStream.AsStreamForWrite();

            var settings = Container.Values;
            var history = fileStream.Size > 0 ? GetHistory(stream) : new(settings.Count);

            if (history.Count == 0)
            {
                history = settings.ToDictionary(s => s.Key, s => GetLocalDates(s.Value));
            }
            else
            {
                var hashSet = new HashSet<DateTimeOffset>(10);

                foreach (var setting in settings)
                {
                    var localDates = GetLocalDates(setting.Value);
                    SyncTrackHistory(setting.Key, localDates, history, hashSet);
                }

                foreach (var entry in history)
                {
                    if (settings.ContainsKey(entry.Key)) continue;
                    Container.Values[entry.Key] = entry.Value;
                }
            }

            var options = new JsonSerializerOptions();
            options.Converters.Add(new DateTimeOffsetConverter());

            fileStream.Seek(0);
            JsonSerializer.Serialize(stream, history, options);
        }

        private static DateTimeOffset[] GetLocalDates(Track track) => GetLocalDates(Container.Values[track.FileName]);

        private static DateTimeOffset[] GetLocalDates(object value)
        {
            if (value is null) return [];
            return value is DateTimeOffset offset ? [offset.TruncateTime()] : (DateTimeOffset[])value;
        }

        private static void SyncTrackHistory(string fileName, DateTimeOffset[] localDates, Dictionary<string, DateTimeOffset[]> history, HashSet<DateTimeOffset> hashSet)
        {
            hashSet.Clear();
            hashSet.AddRange(localDates);

            if (history.TryGetValue(fileName, out var dates))
            {
                var addedAny = false;
                foreach (var date in dates)
                {
                    addedAny |= hashSet.Add(date);
                }

                if (addedAny)
                {
                    var mergedValues = hashSet.OrderBy(d => d).ToArray();
                    Container.Values[fileName] = mergedValues;
                    history[fileName] = mergedValues;
                    return;
                }
            }

            history[fileName] = localDates;
        }

        private static async Task<IRandomAccessStream> CreateOrOpenFile()
        {
            var folder = await KnownFolders.DocumentsLibrary.TryGetItemAsync(nameof(AveoAudio)) as StorageFolder;
            folder ??= await KnownFolders.DocumentsLibrary.CreateFolderAsync(nameof(AveoAudio));

            var file = await folder.CreateFileAsync(FileName, CreationCollisionOption.OpenIfExists);
            return await file.OpenAsync(FileAccessMode.ReadWrite);
        }

        private static Dictionary<string, DateTimeOffset[]> GetHistory(Stream stream)
        {
            return JsonSerializer.Deserialize<Dictionary<string, DateTimeOffset[]>>(stream);
        }

        private static DateTimeOffset TruncateTime(this DateTimeOffset value)
        {
            return value.AddTicks(-(value.Ticks % TimeSpan.TicksPerSecond));
        }

        private class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
        {
            public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (Utf8Parser.TryParse(reader.ValueSpan, out DateTime value, out _)) return value;
                throw new FormatException();
            }

            public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
            {
                Span<char> span = stackalloc char[32];
                value.TryFormat(span, out var count, "s");
                writer.WriteStringValue(span[..count]);
            }
        }
    }
}
