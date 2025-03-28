using System;

namespace AveoAudio;

public readonly struct TagList
{
    private readonly string rawTags;

    // TODO: Use Memory
    public TagList(string rawTags) => this.rawTags = rawTags;

    public static implicit operator string(TagList tagList) => tagList.rawTags;

    public int Length => this.rawTags.Length;

    public Enumerator GetEnumerator() => new(this.rawTags);

    public ref struct Enumerator(string rawTags)
    {
        private int current;

        public Tag Current { get; private set; }

        public bool MoveNext()
        {
            if (current >= rawTags.Length) return false;

            var index = rawTags.IndexOf(',', current);
            var length = index >= 0 ? index - current : rawTags.Length - current;
            this.Current = new Tag(rawTags.AsSpan(current, length), current);
            current += length + 1;

            return true;
        }
    }
}
