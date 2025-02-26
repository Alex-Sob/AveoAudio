using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;

using Windows.Storage;
using Windows.Storage.FileProperties;

namespace AveoAudio
{
    [DebuggerDisplay("{FileName}")]
    public class Track
    {
        private static TrackDataParser parser;

        static Track()
        {
            parser = new(((App)Application.Current).AppSettings);
        }

        public BitVector32 CustomTags { get; internal set; }

        public DateTime DateAdded { get; private set; }

        public StorageFile File { get; private set; }

        public string FileName => Path.GetFileNameWithoutExtension(this.File.Name);

        public string Genre { get; private set; }

        public DateTime? LastPlayedOn => HistoryManager.GetLastPlayedOn(this);

        public MusicProperties Properties { get; private set; }

        public TagList Tags { get; internal set; }

        public TimesOfDay TimesOfDay { get; internal set; }

        public Weather Weather { get; internal set; }

        public static async Task<Track> Load(StorageFile file, string genre)
        {
            var props = await file.Properties.GetMusicPropertiesAsync().AsTask().ConfigureAwait(false);
            var (dateAdded, rawTags) = TrackDataParser.ExtractCustomProperties(file, props);

            var track = new Track
            {
                Genre = genre,
                File = file,
                Properties = props,
                DateAdded = dateAdded
            };

            parser.ParseTags(track, rawTags);

            return track;
        }

        public override string ToString()
        {
            var artist = this.Properties.Artist ?? "";
            var title = this.Properties.Title ?? "";

            return artist != "" && title != "" ? $"{this.Properties.Artist} - {this.Properties.Title}" : this.FileName;
        }

        public async Task UpdateTags(string rawTags)
        {
            this.Properties.Subtitle = $"{this.DateAdded:dd.MM.yyyy};{rawTags}";
            await this.Properties.SavePropertiesAsync().AsTask().ConfigureAwait(false);
            parser.ParseTags(this, rawTags);
        }
    }
}
