using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.FileProperties;

namespace AveoAudio
{
    public class Track
    {
        public BitVector32 CustomTags { get; private set; }

        public DateTime DateCreated { get; private set; }

        public StorageFile File { get; private set; }

        public string FileName => Path.GetFileNameWithoutExtension(this.File.Name);

        public string Genre { get; private set; }

        public MusicProperties Properties { get; private set; }

        public string RawTags { get; private set; }

        public TimesOfDay TimesOfDay { get; private set; }

        public static async Task<Track> CreateAsync(StorageFile file, string genre, TrackDataParser dataParser)
        {
            var props = await file.Properties.GetMusicPropertiesAsync();
            var (dateCreated, rawTags) = TrackDataParser.ExtractCustomProperties(file, props);
            var (timesOfDay, customTags) = dataParser.ParseTags(rawTags);

            return new Track
            {
                File = file,
                Genre = genre,
                Properties = props,
                DateCreated = dateCreated,
                RawTags = rawTags,
                TimesOfDay = timesOfDay,
                CustomTags = customTags,
            };
        }

        public async Task ApplyRawTags(string value, TrackDataParser dataParser)
        {
            this.Properties.Subtitle = $"{this.DateCreated:dd.MM.yyyy};{value}";
            await this.Properties.SavePropertiesAsync();

            this.RawTags = value;
            var (timesOfDay, customTags) = dataParser.ParseTags(value);

            this.TimesOfDay = timesOfDay;
            this.CustomTags = customTags;
        }

        public override string ToString()
        {
            var artist = this.Properties.Artist ?? "";
            var title = this.Properties.Title ?? "";

            if (artist != "" && title != "")
                return $"{this.Properties.Artist} - {this.Properties.Title}";

            return this.FileName;
        }
    }
}
