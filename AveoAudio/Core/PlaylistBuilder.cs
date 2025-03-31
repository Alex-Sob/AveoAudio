using System;
using System.Collections.Generic;
using System.Linq;

namespace AveoAudio
{
    public class PlaylistBuilder(IEnumerable<Track> source, AppSettings settings)
    {
        private readonly AppSettings settings = settings;
        private IEnumerable<Track> query = source;

        public IEnumerable<Track> Build() => this.query;

        public PlaylistBuilder ExcludeTags(ICollection<string> tags)
        {
            if (tags.Count == 0) return this;

            var mask = CreateMask(tags);
            this.query = this.query.Where(track => (mask & track.CustomTags.Data) == 0);
            return this;
        }

        public PlaylistBuilder FilterByTags(ICollection<string> tags)
        {
            if (tags.Count == 0) return this;

            var mask = CreateMask(tags);
            this.query = this.query.Where(track => (mask & track.CustomTags.Data) == mask);
            return this;
        }

        public PlaylistBuilder WithOutOfRotationTimeSinceAdded(int days)
        {
            if (days > 0)
            {
                var startingDate = DateTime.Today.AddDays(-days);
                this.query = this.query.Where(track => track.DateAdded < startingDate);
            }

            return this;
        }

        public PlaylistBuilder WithOutOfRotationTimeSincePlayed(int days)
        {
            if (days >= 0)
            {
                var startingDate = DateTime.Today.AddDays(-days);
                this.query = this.query.Where(track => !track.LastPlayedOn.HasValue || track.LastPlayedOn < startingDate);
            }
            else
            {
                this.query = this.query.Where(track => !track.LastPlayedOn.HasValue);
            }

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

        private int CreateMask(ICollection<string> tags)
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
