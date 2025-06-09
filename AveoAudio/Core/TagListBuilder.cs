namespace AveoAudio;

public struct TagListBuilder(TagList tags)
{
    public TagListBuilder(string tags) : this(new TagList(tags)) { }

    public TagList Tags { get; private set; } = tags;

    // TODO: Use enum?
    public bool HasTag(string tag) => FindTag(this.Tags, tag, out _);

    public void ToggleBestTimeOfDay(TimesOfDay timesOfDay)
    {
        var timeOfDay = timesOfDay.ToString();

        if (!FindTag(this.Tags, timeOfDay, out var tag))
        {
            AddTag(timeOfDay, "!");
            return;
        }

        var span = this.Tags.AsSpan();

        if (tag.HasToken)
            this.Tags = new TagList(string.Concat(span[..tag.End], span[(tag.End + 1)..]));
        else
            this.Tags = new TagList(string.Concat(span[..(tag.End + 1)], "!", span[(tag.End + 1)..]));
    }

    public void ToggleTag(string tag)
    {
        if (FindTag(this.Tags, tag, out var result))
            RemoveTag(result);
        else
            AddTag(tag);
    }

    private static bool FindTag(TagList tags, string tag, out Tag result)
    {
        ReadOnlySpan<char> span = tag;

        foreach (var t in tags)
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

    private void AddTag(string tag, string? token = null)
    {
        if (this.Tags.Length > 0)
            this.Tags = new TagList(string.Concat(this.Tags.AsSpan(), ",", tag, token));
        else
            this.Tags = new TagList(tag);
    }

    private void RemoveTag(Tag tag)
    {
        var value = this.Tags.AsSpan();

        if (tag.End == value.Length - 1)
            this.Tags = new TagList(tag.Start > 0 ? value[..(tag.Start - 1)].ToString() : "");
        else
            this.Tags = new TagList(string.Concat(value[..tag.Start], value[(tag.End + 2)..]));
    }
}