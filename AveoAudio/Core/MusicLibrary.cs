using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Search;

namespace AveoAudio;

public class MusicLibrary
{
    private static readonly QueryOptions QueryOptions = new() { FolderDepth = FolderDepth.Deep, FileTypeFilter = { ".mp3" } };

    private readonly Dictionary<string, IReadOnlyList<Track>> tracksByGenre = [];

    public static async Task<IReadOnlyList<string>> GetGenres()
    {
        var folders = await KnownFolders.MusicLibrary.GetFoldersAsync();

        var genres =
            from folder in folders
            where Directory.EnumerateFiles(folder.Path, "*.mp3").Any()
            let genre = folder.Name
            orderby genre
            select genre;

        var result = new List<string>(folders.Count);
        result.AddRange(genres);

        return result;
    }

    public async Task<IEnumerable<Track>> LoadTracksAsync(IEnumerable<string> genres)
    {
        var folders = await KnownFolders.MusicLibrary.GetFoldersAsync().AsTask().ConfigureAwait(false);

        var tasks =
            from genre in genres
            where !this.tracksByGenre.ContainsKey(genre)
            join folder in folders on genre equals folder.Name
            select LoadTracksAsync(genre, folder);

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return from pair in this.tracksByGenre
               where genres.Contains(pair.Key)
               from track in pair.Value
               select track;
    }

    private async Task LoadTracksAsync(string genre, StorageFolder folder)
    {
        var fileQuery = folder.CreateFileQueryWithOptions(QueryOptions);
        var files = await fileQuery.GetFilesAsync().AsTask().ConfigureAwait(false);

        var tracks = new Track[files.Count];

        await Task.WhenAll(files.Select(Load));
        this.tracksByGenre[genre] = tracks;

        async Task Load(StorageFile file, int index)
        {
            tracks[index] = await Track.Load(file, genre).ConfigureAwait(false);
        }
    }
}
