namespace AveoAudio;

public readonly struct TagList(ReadOnlyMemory<char> tags)
{
    private readonly ReadOnlyMemory<char> tags = tags;

    public TagList(string tags) : this(tags.AsMemory()) { }

    public static implicit operator ReadOnlyMemory<char>(TagList tagList) => tagList.tags;

    public bool IsEmpty => this.tags.Length == 0;

    public int Length => this.tags.Length;

    public ReadOnlySpan<char> AsSpan() => this.tags.Span;

    public bool FindTag(string tag, out Tag result)
    {
        ReadOnlySpan<char> span = tag;

        foreach (var t in this)
        {
            if (span.Equals(t, StringComparison.Ordinal))
            {
                result = t;
                return true;
            }
        }

        result = default;
        return false;
    }

    public Enumerator GetEnumerator() => new(this.tags);

    public override string ToString() => this.tags.ToString();

    public ref struct Enumerator(ReadOnlyMemory<char> rawTags)
    {
        private int current;

        public Tag Current { get; private set; }

        public bool MoveNext()
        {
            if (current >= rawTags.Length) return false;

            var index = rawTags.Span[current..].IndexOf(',');
            var length = index >= 0 ? index : rawTags.Length - current;
            this.Current = new Tag(rawTags.Span.Slice(current, length), current);
            current += length + 1;

            return true;
        }
    }
}
