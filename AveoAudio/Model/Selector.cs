using System.Collections.Generic;

namespace AveoAudio
{
    public class Selector
    {
        public IList<string> DefaultExcludeTags { get; set; } = new List<string>();

        public IList<string> DefaultFilterTags { get; set; } = new List<string>();
    }
}
