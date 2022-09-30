using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Search;

namespace AveoAudio
{
    public class TrackManager
    {
        private readonly TrackDataParser trackDataParser;
        private readonly Dictionary<string, IList<Track>> tracksByGenre = new Dictionary<string, IList<Track>>();

        public TrackManager(AppSettings settings)
        {
            this.trackDataParser = new TrackDataParser(settings);
        }

        public async Task<IEnumerable<Track>> GetTracksAsync(IEnumerable<string> genres)
        {
            var tasks =
                from genre in genres
                join pair in this.tracksByGenre on genre equals pair.Key into g
                from pair in g.DefaultIfEmpty()
                select pair.Value != null ? Task.FromResult(pair.Value) : LoadTracksAsync(genre);

            var trackLists = await Task.WhenAll(tasks);

            var tracks =
                from trackList in trackLists
                from track in trackList
                select track;

            return tracks;
        }

        public async Task UpdateTags(Track track, string rawTags)
        {
            track.Properties.Subtitle = $"{track.DateCreated:dd.MM.yyyy};{rawTags}";
            await track.Properties.SavePropertiesAsync();
            this.trackDataParser.ParseTags(track, rawTags);
        }

        private async Task<Track> LoadTrackAsync(string genre, StorageFile file)
        {
            var props = await file.Properties.GetMusicPropertiesAsync();
            var (dateCreated, rawTags) = TrackDataParser.ExtractCustomProperties(file, props);

            var track = new Track
            {
                File = file,
                Genre = genre,
                Properties = props,
                DateCreated = dateCreated,
            };

            this.trackDataParser.ParseTags(track, rawTags);
            return track;
        }

        private async Task<IList<Track>> LoadTracksAsync(string genre)
        {
            var queryOptions = new QueryOptions { FolderDepth = FolderDepth.Deep, FileTypeFilter = { ".mp3" } };

            var folder = await KnownFolders.MusicLibrary.GetFolderAsync(genre);
            var files = await folder.CreateFileQueryWithOptions(queryOptions).GetFilesAsync();

            var tracks = await Task.WhenAll(
                from file in files
                select this.LoadTrackAsync(genre, file));

            this.tracksByGenre[genre] = tracks;
            return tracks;
        }
    }
}
