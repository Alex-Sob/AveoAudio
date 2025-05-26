namespace AveoAudio;

public class AppSettings
{
    private string[]? tags;

    public static AppSettings Default => new()
    {
        FilterDefaults = new Dictionary<string, FilterDefaults>(),
        TagGroups = new Dictionary<string, string[]>()
    };

    public int PlaylistSize { get; set; }

    public required IDictionary<string, FilterDefaults> FilterDefaults { get; set; }

    public required IDictionary<string, string[]> TagGroups { get; set; }

    public IReadOnlyList<string> Tags => tags ??= this.TagGroups.SelectMany(p => p.Value).ToArray();
}
