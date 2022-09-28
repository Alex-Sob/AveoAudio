using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Search;

namespace AveoAudio
{
    public class PlaylistBuilder
    {
        private readonly AppSettings settings;
        private readonly AppState appState;
        private readonly TrackDataParser trackDataParser;
        private readonly Dictionary<string, IList<Track>> tracksByGenre = new Dictionary<string, IList<Track>>();

        public PlaylistBuilder(AppSettings settings, AppState appState, TrackDataParser trackDataParser)
        {
            this.settings = settings;
            this.appState = appState;
            this.trackDataParser = trackDataParser;
        }

        public async Task<IList<Track>> BuildPlaylistAsync(PlaylistProfile profile)
        {
            var tracks = await SelectTracksAsync(profile);
            var playlist = new List<Track>(this.settings.PlaylistSize);
            playlist.AddRange(tracks.Randomize());
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

        private static IEnumerable<T> GetRange<T>(IList<T> list, int index)
        {
            for (int i = index; i < list.Count; i++)
            {
                yield return list[i];
            }
        }

        private async Task<IList<Track>> GetTracksAsync(string genre)
        {
            var queryOptions = new QueryOptions { FolderDepth = FolderDepth.Deep, FileTypeFilter = { ".mp3" } };

            var folder = await KnownFolders.MusicLibrary.GetFolderAsync(genre);
            var files = await folder.CreateFileQueryWithOptions(queryOptions).GetFilesAsync();

            var tracks = await Task.WhenAll(
                from file in files
                select Track.CreateAsync(file, genre, this.trackDataParser));

            this.tracksByGenre[genre] = tracks;
            return tracks;
        }

        private async Task<(IEnumerable<Track> tracks, int count)> GetTracksAsync(IEnumerable<string> genres)
        {
            var tasks =
                from genre in genres
                join pair in this.tracksByGenre on genre equals pair.Key into g
                from pair in g.DefaultIfEmpty()
                select pair.Value != null ? Task.FromResult(pair.Value) : GetTracksAsync(genre);

            var trackLists = await Task.WhenAll(tasks);
            var count = trackLists.Sum(list => list.Count);

            var tracks =
                from trackList in trackLists
                from track in trackList
                select track;

            return (tracks, count);
        }

        private async Task<List<Track>> SelectTracksAsync(PlaylistProfile profile)
        {
            var allTracks = await this.GetTracksAsync(profile.Genres);
            var tracks = new List<Track>(allTracks.count);

            var filterTags = profile.FilterTags;
            var excludeTags = profile.ExcludeTags;

            var filterMask = CreateMask(filterTags);
            var excludeMask = CreateMask(excludeTags);

            var timesOfDay = this.appState.TimesOfDay ?? TimesOfDay.None;

            tracks.AddRange(
                from track in allTracks.tracks
                where track.TimesOfDay == TimesOfDay.None || track.TimesOfDay.HasFlag(timesOfDay)
                where excludeTags.Count == 0 || (excludeMask & track.CustomTags.Data) == 0
                where filterTags.Count == 0 || (filterMask & track.CustomTags.Data) == filterMask
                select track);

            return tracks;
        }
    }
}
