using System;

namespace AveoAudio;

public struct TagListBuilder(TagList tags)
{
    public TagListBuilder(string tags) : this(new TagList(tags)) { }

    public TagList Tags { get; private set; } = tags;

    // TODO: Use enum?
    public bool HasTag(string tag) => FindTag(this.Tags, tag, out _);

    public void ToggleBestTimeOfDay(TimesOfDay timesOfDay)
    {
        var tag = timesOfDay.ToString();

        if (!FindTag(this.Tags, tag, out var result))
        {
            AddTag(tag, '!');
            return;
        }

        string value = this.Tags;

        if (result.HasToken)
            this.Tags = new TagList(value.Remove(result.End, 1));
        else
            this.Tags = new TagList(value.Insert(result.End + 1, "!"));
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

    private void AddTag(string tag, char? token = null)
    {
        if (this.Tags.Length > 0)
            this.Tags = new TagList(this.Tags + ',' + tag + token);
        else
            this.Tags = new TagList(tag);
    }

    private void RemoveTag(Tag tag)
    {
        string value = this.Tags;

        if (tag.End == value.Length - 1)
            value = tag.Start > 0 ? value[..(tag.Start - 1)] : value[..tag.Start];
        else
            value = value.Remove(tag.Start, tag.End - tag.Start + 2);

        this.Tags = new TagList(value);
    }
}