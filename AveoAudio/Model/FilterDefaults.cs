using System.Collections.Generic;

namespace AveoAudio;

public class FilterDefaults
{
    public ICollection<string> ExcludeTags { get; set; } = [];

    public ICollection<string> Genres { get; set; } = [];

    public ICollection<string> Tags { get; set; } = [];
}
