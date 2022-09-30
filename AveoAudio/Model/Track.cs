using System;
using System.Collections.Specialized;
using System.IO;

using Windows.Storage;
using Windows.Storage.FileProperties;

namespace AveoAudio
{
    public class Track
    {
        public BitVector32 CustomTags { get; internal set; }

        public DateTime DateCreated { get; internal set; }

        public StorageFile File { get; internal set; }

        public string FileName => Path.GetFileNameWithoutExtension(this.File.Name);

        public string Genre { get; internal set; }

        public MusicProperties Properties { get; internal set; }

        public string RawTags { get; internal set; }

        public TimesOfDay TimesOfDay { get; internal set; }

        public Weather Weather { get; internal set; }

        public override string ToString()
        {
            var artist = this.Properties.Artist ?? "";
            var title = this.Properties.Title ?? "";

            return artist != "" && title != "" ? $"{this.Properties.Artist} - {this.Properties.Title}" : this.FileName;
        }
    }
}
