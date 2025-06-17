using System.Collections.Specialized;

namespace AveoAudio;

public struct CommonTags
{
    private readonly static Dictionary<string, int> tagToIndexMap = new(32);

    private BitVector32 tags;

    public static void AddTag(string tag) => tagToIndexMap[tag] = tagToIndexMap.Count;

    public static CommonTags CreateMask(IEnumerable<string> tags)
    {
        var mask = new CommonTags();

        foreach (var tag in tags)
        {
            if (tagToIndexMap.TryGetValue(tag, out var index))
                mask.tags[1 << index] = true;
        }

        return mask;
    }

    public bool HasAll(CommonTags tags)
    {
        var mask = tags.tags.Data;
        return (this.tags.Data & mask) == mask;
    }

    public bool HasAny(CommonTags tags)
    {
        var mask = tags.tags.Data;
        return (this.tags.Data & mask) > 0;
    }

    public bool HasTag(string tag) => this.tags[1 << tagToIndexMap[tag]];

    public void Toggle(string tag)
    {
        var bit = 1 << tagToIndexMap[tag];
        this.tags[bit] = !this.tags[bit];
    }

    public bool TrySet(ReadOnlySpan<char> tag, bool value = true)
    {
        var alternate = tagToIndexMap.GetAlternateLookup<ReadOnlySpan<char>>();

        if (!alternate.TryGetValue(tag, out var index)) return false;

        tags[1 << index] = value;
        return true;
    }
}
