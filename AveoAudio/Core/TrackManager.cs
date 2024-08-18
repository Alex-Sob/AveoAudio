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
        private static readonly QueryOptions QueryOptions = new() { FolderDepth = FolderDepth.Deep, FileTypeFilter = { ".mp3" } };

        private readonly TrackDataParser trackDataParser;
        private readonly Dictionary<string, IList<Track>> tracksByGenre = new();

        public TrackManager(AppSettings settings)
        {
            this.trackDataParser = new TrackDataParser(settings);
        }

        public async Task<IEnumerable<Track>> GetTracksAsync(IEnumerable<string> genres)
        {
            var folders = await KnownFolders.MusicLibrary.GetFoldersAsync().AsTask().ConfigureAwait(false);

            var tasks =
                from folder in folders
                join genre in genres on folder.Name equals genre
                select this.tracksByGenre.TryGetValue(genre, out var tracks) ? Task.FromResult(tracks) : LoadTracksAsync(genre, folder);

            var trackLists = await Task.WhenAll(tasks).ConfigureAwait(false);

            var tracks =
                from trackList in trackLists
                from track in trackList
                select track;

            return tracks;
        }

        public async Task UpdateTags(Track track, string rawTags)
        {
            track.Properties.Subtitle = $"{track.DateCreated:dd.MM.yyyy};{rawTags}";
            await track.Properties.SavePropertiesAsync().AsTask().ConfigureAwait(false);
            this.trackDataParser.ParseTags(track, rawTags);
        }

        private async Task LoadTrackAsync(Track track)
        {
            var props = await track.File.Properties.GetMusicPropertiesAsync().AsTask().ConfigureAwait(false);
            var (dateCreated, rawTags) = TrackDataParser.ExtractCustomProperties(track.File, props);

            track.Properties = props;
            track.DateCreated = dateCreated;

            this.trackDataParser.ParseTags(track, rawTags);
        }

        private async Task<IList<Track>> LoadTracksAsync(string genre, StorageFolder folder)
        {
            var files = await folder.CreateFileQueryWithOptions(QueryOptions).GetFilesAsync().AsTask().ConfigureAwait(false);

            var tracks = new List<Track>(files.Count);

            foreach (var file in files)
            {
                tracks.Add(new Track { Genre = genre, File = file });
            }

            await Task.WhenAll(tracks.Select(LoadTrackAsync)).ConfigureAwait(false);

            this.tracksByGenre[genre] = tracks;
            return tracks;
        }
    }
}
