using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Search;

namespace AveoAudio;

public class MusicLibrary
{
    private static MusicLibrary current;

    private static readonly QueryOptions QueryOptions = new() { FolderDepth = FolderDepth.Deep, FileTypeFilter = { ".mp3" } };

    private readonly Dictionary<string, IReadOnlyList<Track>> tracksByGenre = [];
    private readonly ConcurrentDictionary<string, Track> tracksByName = [];

    public static MusicLibrary Current => current ??= new();

    public static async Task<IReadOnlyList<string>> GetGenres()
    {
        var folders = await KnownFolders.MusicLibrary.GetFoldersAsync();

        var genres = from folder in folders
                     where Directory.EnumerateFiles(folder.Path, "*.mp3").Any()
                     let genre = folder.Name
                     orderby genre
                     select genre;

        var result = new List<string>(folders.Count);
        result.AddRange(genres);

        return result;
    }

    public Track GetByName(string name) => this.tracksByName.TryGetValue(name, out var track) ? track : null;

    public IEnumerable<Track> GetByGenres(IEnumerable<string> genres)
    {
        return from genre in genres
               let tracks = this.tracksByGenre.TryGetValue(genre, out var tracks) ? tracks : []
               from track in tracks
               select track;
    }

    public async Task LoadByGenresAsync(IEnumerable<string> genres)
    {
        var folders = await KnownFolders.MusicLibrary.GetFoldersAsync().AsTask().ConfigureAwait(false);

        var tasks = from genre in genres
                    where !this.tracksByGenre.ContainsKey(genre)
                    join folder in folders on genre equals folder.Name
                    select LoadTracksAsync(genre, folder);

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task LoadByNamesAsync(IEnumerable<string> names)
    {
        names = names.Where(name => !this.tracksByName.ContainsKey(name));
        var namesToLoad = new HashSet<string>(names);

        if (namesToLoad.Count == 0) return;

        var folders = await KnownFolders.MusicLibrary.GetFoldersAsync();
        var alternate = namesToLoad.GetAlternateLookup<ReadOnlySpan<char>>();

        var tasks = from folder in folders
                    from path in Directory.EnumerateFiles(folder.Path)
                    where alternate.Contains(Track.GetName(path))
                    select LoadTrack(path, folder.Name);

        await Task.WhenAll(tasks).ConfigureAwait(false);

        async Task LoadTrack(string path, string genre)
        {
            var track = await Track.Load(path, genre).ConfigureAwait(false);
            this.tracksByName[track.Name] = track;
        }
    }

    private async Task LoadTracksAsync(string genre, StorageFolder folder)
    {
        var fileQuery = folder.CreateFileQueryWithOptions(QueryOptions);
        var files = await fileQuery.GetFilesAsync().AsTask().ConfigureAwait(false);

        var tracks = new Track[files.Count];
        var alternate = this.tracksByName.GetAlternateLookup<ReadOnlySpan<char>>();

        await Task.WhenAll(files.Select(Load));
        this.tracksByGenre[genre] = tracks;

        async Task Load(StorageFile file, int index)
        {
            var track = await Track.Load(file, genre).ConfigureAwait(false);
            var name = Track.GetName(file.Name);

            if (alternate.TryGetValue(name, out var cachedTrack))
                tracks[index] = cachedTrack;
            else
                tracks[index] = alternate[name] = track;
        }
    }
}
