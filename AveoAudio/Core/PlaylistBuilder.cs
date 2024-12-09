using System.Collections.Generic;
using System.Linq;

namespace AveoAudio
{
    public class PlaylistBuilder(IEnumerable<Track> source, AppSettings settings)
    {
        private readonly AppSettings settings = settings;
        private IEnumerable<Track> query = source;

        public void BuildPlaylist(IList<Track> playlist)
        {
            var tracks = this.query.Randomize().Take(this.settings.PlaylistSize);

            playlist.Clear();
            playlist.AddRange(tracks);
        }

        public PlaylistBuilder ExcludeTags(ISet<string> tags)
        {
            if (tags.Count == 0) return this;

            var mask = CreateMask(tags);
            this.query = this.query.Where(track => (mask & track.CustomTags.Data) == 0);
            return this;
        }

        public PlaylistBuilder FilterByTags(ISet<string> tags)
        {
            if (tags.Count == 0) return this;

            var mask = CreateMask(tags);
            this.query = this.query.Where(track => (mask & track.CustomTags.Data) == mask);
            return this;
        }

        public PlaylistBuilder WithTimeOfDay(TimesOfDay? timeOfDay)
        {
            timeOfDay ??= TimesOfDay.None;
            this.query = this.query.Where(track => track.TimesOfDay == TimesOfDay.None || track.TimesOfDay.HasFlag(timeOfDay));
            return this;
        }

        public PlaylistBuilder WithWeather(Weather? weather)
        {
            this.query = this.query.Where(track => track.Weather == Weather.None || track.Weather == weather);
            return this;
        }

        private int CreateMask(ISet<string> tags)
        {
            int bitMask = 1, result = 0;

            for (int i = 0; i < this.settings.Tags.Count; i++)
            {
                result |= tags.Contains(this.settings.Tags[i]) ? bitMask : 0;
                bitMask <<= 1;
            }

            return result;
        }
    }
}
