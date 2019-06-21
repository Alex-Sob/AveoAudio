using System.Collections.Generic;

namespace AveoAudio
{
    public class PlaylistProfile
    {
        public ISet<string> ExcludeTags { get; private set; } = new HashSet<string>();

        public ISet<string> FilterTags { get; private set; } = new HashSet<string>();

        public ISet<string> Genres { get; private set; } = new HashSet<string>();
    }
}
