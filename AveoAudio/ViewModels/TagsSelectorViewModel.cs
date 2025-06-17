using System.Windows.Input;

namespace AveoAudio.ViewModels;

public class TagsSelectorViewModel(ICollection<TagGroup> tagGroups, ICommand toggleTagCommand) : NotificationBase
{
    public ICollection<TagGroup> TagGroups { get; } = tagGroups;

    public ICommand ToggleTagCommand { get; set; } = toggleTagCommand;

    public void SelectTags(ICollection<string> tags)
    {
        foreach (var group in this.TagGroups)
        foreach (var item in group)
        {
            item.IsChecked = tags.Contains(item);
        }
    }
}
