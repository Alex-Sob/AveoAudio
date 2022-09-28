using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AveoAudio
{
    public class PlaylistBuilder
    {
        private readonly AppSettings settings;
        private readonly AppState appState;
        private readonly TrackManager trackManager;

        private List<Track> playlist;

        public PlaylistBuilder(AppSettings settings, AppState appState, TrackManager trackManager)
        {
            this.settings = settings;
            this.appState = appState;
            this.trackManager = trackManager;
        }

        public async Task<IList<Track>> BuildPlaylistAsync(PlaylistProfile profile)
        {
            var tracks = await SelectTracksAsync(profile);

            this.playlist = this.playlist ?? new List<Track>(this.settings.PlaylistSize);
            this.playlist.Clear();
            playlist.AddRange(tracks.Randomize().Take(this.playlist.Count));

            return playlist;
        }

        private int CreateMask(ISet<string> filterTags)
        {
            int bitMask = 1, result = 0;

            for (int i = 0; i < this.settings.Tags.Count; i++)
            {
                result |= filterTags.Contains(this.settings.Tags[i]) ? bitMask : 0;
                bitMask <<= 1;
            }

            return result;
        }

        private async Task<IEnumerable<Track>> SelectTracksAsync(PlaylistProfile profile)
        {
            var tracks = await this.trackManager.GetTracksAsync(profile.Genres);

            var filterTags = profile.FilterTags;
            var excludeTags = profile.ExcludeTags;

            var filterMask = CreateMask(filterTags);
            var excludeMask = CreateMask(excludeTags);

            var timesOfDay = this.appState.TimesOfDay ?? TimesOfDay.None;

            return from track in tracks
                   where track.TimesOfDay == TimesOfDay.None || track.TimesOfDay.HasFlag(timesOfDay)
                   where excludeTags.Count == 0 || (excludeMask & track.CustomTags.Data) == 0
                   where filterTags.Count == 0 || (filterMask & track.CustomTags.Data) == filterMask
                   select track;
        }
    }
}
