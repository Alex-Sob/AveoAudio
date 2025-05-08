using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.FileProperties;

namespace AveoAudio;

public class Track
{
    private static TrackDataParser parser;
    private string name;

    static Track()
    {
        parser = new(App.Current.AppSettings);
    }

    public BitVector32 CustomTags { get; internal set; }

    public DateTime DateAdded { get; private set; }

    public StorageFile File { get; private set; }

    public string Name => this.name ?? (name = GetName(this.File.Name).ToString());

    public string Genre { get; private set; }

    public DateTime? LastPlayedOn => HistoryManager.GetLastPlayedOn(this);

    public MusicProperties Properties { get; private set; }

    public TagList Tags { get; internal set; }

    public TimesOfDay TimesOfDay { get; internal set; }

    public Weather Weather { get; internal set; }

    public static event EventHandler<TrackEventArgs> TagsUpdated;

    public static ReadOnlySpan<char> GetName(string pathOrFileName) => Path.GetFileNameWithoutExtension(pathOrFileName.AsSpan());

    public static async Task<Track> Load(StorageFile file, string genre)
    {
        var props = await file.Properties.GetMusicPropertiesAsync().AsTask().ConfigureAwait(false);
        var (dateAdded, rawTags) = TrackDataParser.ExtractCustomProperties(file, props);

        var track = new Track
        {
            Genre = genre,
            File = file,
            Properties = props,
            DateAdded = dateAdded
        };

        parser.ParseTags(track, rawTags);

        return track;
    }

    public static async Task<Track> Load(string path, string genre)
    {
        return await Load(await StorageFile.GetFileFromPathAsync(path), genre);
    }

    public override string ToString() => this.Name;

    public async Task UpdateTags(string rawTags)
    {
        this.Properties.Subtitle = $"{this.DateAdded:dd.MM.yyyy};{rawTags}";
        await this.Properties.SavePropertiesAsync();
        parser.ParseTags(this, rawTags);
        TagsUpdated?.Invoke(null, new TrackEventArgs(this));
    }
}
