using System;
using System.Collections;
using System.Collections.Generic;

namespace AveoAudio.ViewModels;

public class TagGroup(string name) : IEnumerable<TagEditorItem>
{
    public string Name { get; } = name;

    public IList<TagEditorItem> Tags { get; } = [];

    public TagGroup(string name, ReadOnlySpan<string> tags) : this(name)
    {
        this.Tags = new TagEditorItem[tags.Length];
        
        for (int i = 0; i < tags.Length; i++)
            this.Tags[i] = new(tags[i]);
    }

    public IEnumerator<TagEditorItem> GetEnumerator() => Tags.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Tags.GetEnumerator();
}
