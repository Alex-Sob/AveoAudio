using System.Collections.Generic;

namespace AveoAudio
{
    public class AppSettings
    {
        public string ImagesPath { get; set; }

        public int PlaylistSize { get; set; }

        public int OlderTracksQuota { get; set; }

        public IDictionary<string, Selector> Selectors { get; set; }

        public IList<string> Tags { get; set; }
    }
}
