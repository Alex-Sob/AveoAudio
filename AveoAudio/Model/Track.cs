using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.FileProperties;

namespace AveoAudio
{
    public class Track
    {
        public DateTime DateCreated { get; private set; }

        public StorageFile File { get; private set; }

        public string FileName => Path.GetFileNameWithoutExtension(this.File.Name);

        public string Genre { get; private set; }

        public MusicProperties Properties { get; private set; }

        public string RawTags { get; private set; }

        public IList<string> Tags { get; private set; }

        public static async Task<Track> CreateAsync(StorageFile file, string genre)
        {
            var props = await file.Properties.GetMusicPropertiesAsync();
            var (dateCreated, rawTags) = ExtractCustomProperties(file, props);
            var tags = ParseTags(rawTags);

            return new Track
            {
                File = file,
                Genre = genre,
                Properties = props,
                DateCreated = dateCreated,
                RawTags = rawTags,
                Tags = tags,
            };
        }

        public async Task ApplyRawTags(string value)
        {
            this.Properties.Subtitle = $"{this.DateCreated.ToString("dd.MM.yyyy")};{value}";
            await this.Properties.SavePropertiesAsync();

            this.RawTags = value;
            this.Tags = ParseTags(value);
        }

        public override string ToString()
        {
            var artist = this.Properties.Artist ?? "";
            var title = this.Properties.Title ?? "";

            if (artist != "" && title != "")
                return $"{this.Properties.Artist} - {this.Properties.Title}";

            return this.FileName;
        }

        private static (DateTime dateCreated, string rawTags) ExtractCustomProperties(StorageFile file, MusicProperties props)
        {
            var parts = props.Subtitle.Split(';');

            var provider = CultureInfo.InvariantCulture;
            var style = DateTimeStyles.None;

            var hasDate = DateTime.TryParseExact(parts[0], "dd.MM.yyyy", provider, style, out DateTime dateCreated);
            dateCreated = hasDate ? dateCreated : file.DateCreated.Date;

            var rawTags = parts.Length > 1 ? parts[1] : null;

            return (dateCreated, rawTags);
        }

        private static IList<string> ParseTags(string rawTags)
        {
            return !string.IsNullOrEmpty(rawTags) ? rawTags.Split(',') : new string[0];
        }
    }
}
