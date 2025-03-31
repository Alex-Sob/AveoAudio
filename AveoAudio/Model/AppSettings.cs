using System.Collections.Generic;
using System.Linq;

namespace AveoAudio;

public class AppSettings
{
    private string[] tags;

    public int PlaylistSize { get; set; }

    public IDictionary<string, FilterDefaults> FilterDefaults { get; set; }

    public IDictionary<string, string[]> TagGroups { get; set; }

    public IReadOnlyList<string> Tags => tags ??= this.TagGroups.SelectMany(p => p.Value).ToArray();
}
