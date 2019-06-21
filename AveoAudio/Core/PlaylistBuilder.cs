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
        private readonly Dictionary<string, IList<Track>> tracksByGenre = new Dictionary<string, IList<Track>>();
        private readonly AppSettings settings;

        public PlaylistBuilder(AppSettings settings)
        {
            this.settings = settings;
        }

        public async Task<IList<Track>> BuildPlaylistAsync(PlaylistProfile profile)
        {
            var allTracks = await SelectTracksAsync(profile);

            var newerTracksQuota = this.settings.PlaylistSize - this.settings.OlderTracksQuota;
            var playlist = new List<Track>(this.settings.PlaylistSize);

            var tracks =
                from track in allTracks.Take(newerTracksQuota)
                select track;

            if (allTracks.Count > newerTracksQuota)
            {
                var random = new Random();

                var olderTracks =
                    from track in GetRange(allTracks, newerTracksQuota)
                    let rand = random.Next(allTracks.Count)
                    orderby rand
                    select track;

                tracks = tracks.Concat(olderTracks.Take(this.settings.OlderTracksQuota));
            }

            playlist.AddRange(tracks.Randomize());
            return playlist;
        }

        private static async Task<IEnumerable<string>> GetGenresAsync()
        {
            return from folder in await KnownFolders.MusicLibrary.GetFoldersAsync()
                   select folder.Name;
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
                select Track.CreateAsync(file, genre));

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

            tracks.AddRange(
                from track in allTracks.tracks
                where excludeTags.Count == 0 || !excludeTags.Overlaps(track.Tags)
                where filterTags.Count == 0 || filterTags.Overlaps(track.Tags)
                orderby track.DateCreated descending
                select track);

            return tracks;
        }
    }
}
