using System.Collections.Specialized;

using Windows.Storage;
using Windows.Storage.FileProperties;

namespace AveoAudio;

public class Track
{
    public const string Extension = ".mp3";

    private static readonly TrackDataParser parser = new(App.Current.AppSettings);
    private string? name;

    public BitVector32 CustomTags { get; internal set; }

    public DateTime? DateAdded { get; private set; }

    public required StorageFile File { get; init; }

    public string Name => this.name ?? (name = GetName(this.File.Name).ToString());

    public required string Genre { get; init; }

    public DateTime? LastPlayedOn => HistoryManager.GetLastPlayedOn(this);

    public required MusicProperties Properties { get; init; }

    public TagList Tags { get; internal set; }

    public TimesOfDay TimesOfDay { get; internal set; }

    public Weather Weather { get; internal set; }

    public uint? Year => this.Properties.Year > 0 ? this.Properties.Year : null;

    public static event EventHandler<TrackEventArgs>? TagsUpdated;

    public static ReadOnlySpan<char> GetName(ReadOnlySpan<char> pathOrFileName) => Path.GetFileNameWithoutExtension(pathOrFileName);

    public static ReadOnlyMemory<char> GetName(ReadOnlyMemory<char> pathOrFileName)
    {
        var index = MemoryExtensions.LastIndexOf(pathOrFileName.Span, Path.DirectorySeparatorChar);
        var start = index >= 0 ? index + 1 : 0;

        return pathOrFileName.Slice(start, pathOrFileName.Length - start - Extension.Length);
    }

    public static async Task<Track> Load(StorageFile file, string genre)
    {
        var props = await file.Properties.GetMusicPropertiesAsync().AsTask().ConfigureAwait(false);
        var (dateAdded, tags) = TrackDataParser.ExtractCustomProperties(props);

        var track = new Track
        {
            Genre = genre,
            File = file,
            Properties = props,
            DateAdded = dateAdded
        };

        parser.ParseTags(track, tags);

        return track;
    }

    public override string ToString() => this.Name;

    public async Task UpdateTags(string tags)
    {
        this.DateAdded ??= this.File.DateCreated.Date;
        this.Properties.Subtitle = $"{this.DateAdded:dd.MM.yyyy};{tags}";

        await this.Properties.SavePropertiesAsync();

        parser.ParseTags(this, tags.AsMemory());
        TagsUpdated?.Invoke(null, new TrackEventArgs(this));
    }
}
