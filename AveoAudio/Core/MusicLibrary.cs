using AveoAudio.Core;

using System.Collections.Concurrent;
using System.Diagnostics;

using Windows.Storage;
using Windows.Storage.Search;

namespace AveoAudio;

public class MusicLibrary
{
    private static readonly QueryOptions QueryOptions = new() { FolderDepth = FolderDepth.Deep, FileTypeFilter = { Track.Extension } };

    private static FileSystemWatcher watcher = new();

    private readonly Dictionary<string, IReadOnlyList<Track>> tracksByGenre = [];
    private readonly ConcurrentDictionary<string, Track> tracksByName = [];

    private HashSet<string> genres = [];

    public static MusicLibrary Current { get; } = new();

    public static event EventHandler<TrackEventArgs>? TrackAdded;

    public IReadOnlyCollection<string> Genres => this.genres;

    public Track? GetByName(string name) => this.tracksByName.TryGetValue(name, out var track) ? track : null;

    public IEnumerable<Track> GetByGenres(IEnumerable<string> genres)
    {
        return from genre in genres
               let tracks = this.tracksByGenre.TryGetValue(genre, out var tracks) ? tracks : []
               from track in tracks
               select track;
    }

    public async Task InitializeAsync()
    {
        var folders = await KnownFolders.MusicLibrary.GetFoldersAsync();

        this.genres.EnsureCapacity(folders.Count);
        this.genres.AddRange(GetGenres(folders));

        StartWatching(folders);
    }

    public async Task LoadByGenresAsync(IEnumerable<string> genres)
    {
        var stopwatch = Stopwatch.StartNew();

        var folders = await KnownFolders.MusicLibrary.GetFoldersAsync().AsTask().ConfigureAwait(false);

        var tasks = from genre in genres
                    where !this.tracksByGenre.ContainsKey(genre)
                    join folder in folders on genre equals folder.Name
                    select LoadTracksAsync(genre, folder);

        await Task.WhenAll(tasks).ConfigureAwait(false);

        stopwatch.Stop();
        Logger.LogInfo($"Loading tracks took {stopwatch.Elapsed} ({string.Join(',', genres)})");
    }

    public async Task LoadByNamesAsync(IEnumerable<string> names)
    {
        var set = new HashSet<string>(names);

        var folders = await KnownFolders.MusicLibrary.GetFoldersAsync();
        var alternate = set.GetAlternateLookup<ReadOnlySpan<char>>();

        var tasks = from folder in folders
                    from path in Directory.EnumerateFiles(folder.Path)
                    where alternate.Contains(Track.GetName(path))
                    select LoadFromPath(path, folder.Name);

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Track>> LoadNewest(int maxCount)
    {
        var root = await StorageFolder.GetFolderFromPathAsync(watcher.Path);
        var query = root.CreateFileQueryWithOptions(new QueryOptions(CommonFileQuery.OrderByDate, [Track.Extension]));

        var files = await query.GetFilesAsync(0, (uint)maxCount);
        var tasks = files.Select(file => LoadFromFile(file));

        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Track>> Search(string searchText)
    {
        var files = Directory.EnumerateFiles(watcher.Path, $"*{searchText}*", SearchOption.AllDirectories);
        var tasks = files.Select(path => LoadFromPath(path));

        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static IEnumerable<string> GetGenres(IReadOnlyCollection<StorageFolder> folders)
    {
        return from folder in folders
               where Directory.EnumerateFiles(folder.Path, $"*{Track.Extension}").Any()
               select folder.Name;
    }

    private string GetGenre(string path)
    {
        var relativePath = path.AsSpan(watcher.Path.Length + 1);
        var genre = relativePath[..relativePath.IndexOf(Path.DirectorySeparatorChar)];

        var alternate = this.genres.GetAlternateLookup<ReadOnlySpan<char>>();
        return alternate.TryGetValue(genre, out var result) ? result : genre.ToString();
    }

    private async Task<Track> LoadFromPath(string path, string? genre = null)
    {
        await Task.Delay(1000);
        var file = await StorageFile.GetFileFromPathAsync(path);
        return await LoadFromFile(file, genre);
    }

    private async Task<Track> LoadFromFile(StorageFile file, string? genre = null)
    {
        var name = Track.GetName(file.Name.AsMemory());
        var alternate = this.tracksByName.GetAlternateLookup<ReadOnlySpan<char>>();

        if (alternate.TryGetValue(name.Span, out var track)) return track;

        genre ??= GetGenre(file.Path);
        track = await Track.Load(file, genre).ConfigureAwait(false);
        alternate[name.Span] = track;

        return track;
    }

    private async Task LoadTracksAsync(string genre, StorageFolder folder)
    {
        var fileQuery = folder.CreateFileQueryWithOptions(QueryOptions);
        var files = await fileQuery.GetFilesAsync().AsTask().ConfigureAwait(false);

        var tracks = new Track[files.Count];

        await Task.WhenAll(files.Select(async (file, index) =>
        {
            var track = await LoadFromFile(file, genre).ConfigureAwait(false);
            tracks[index] = track;
        }));

        this.tracksByGenre[genre] = tracks;
    }

    private async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        var track = await LoadFromPath(e.FullPath).ConfigureAwait(false);
        TrackAdded?.Invoke(null, new TrackEventArgs(track));
    }

    private static void StartWatching(IReadOnlyCollection<StorageFolder> folders)
    {
        if (folders.Count == 0) return;

        var path = Path.GetDirectoryName(folders.First().Path);

        watcher = new FileSystemWatcher(path!, $"*{Track.Extension}") { IncludeSubdirectories = true, EnableRaisingEvents = true };
        watcher.Created += Current.OnFileCreated;
    }
}
