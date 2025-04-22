using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace AveoAudio.ViewModels;

public class TagsSelectorViewModel(ICollection<TagGroup> tagGroups, ICommand toggleTagCommand) : NotificationBase
{
    public ICollection<TagGroup> TagGroups { get; } = tagGroups;

    public ICommand ToggleTagCommand { get; set; } = toggleTagCommand;

    public void SelectTags(ICollection<string> tags)
    {
        foreach (var item in this.TagGroups.SelectMany(g => g))
            item.IsChecked = tags.Contains(item);
    }
}
