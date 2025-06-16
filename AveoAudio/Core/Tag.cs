namespace AveoAudio;

public readonly ref struct Tag(ReadOnlySpan<char> rawTag, int start)
{
    private readonly ReadOnlySpan<char> rawTag = rawTag;
    private readonly ReadOnlySpan<char> tag = rawTag.EndsWith('!') ? rawTag[..^1] : rawTag;

    public int End => this.Start + this.rawTag.Length - 1;

    public ReadOnlySpan<char> AsSpan() => this.rawTag;

    public bool HasToken => this.rawTag.EndsWith('!');

    public int Start { get; } = start;

    public static implicit operator ReadOnlySpan<char>(Tag tag) => tag.tag;

    public override string ToString() => this.tag.ToString();
}
