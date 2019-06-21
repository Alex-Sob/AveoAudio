namespace AveoAudio.ViewModels
{
    public class TagListItem : NotificationBase
    {
        private bool exclude;
        private bool filter;

        public bool Exclude
        {
            get => this.exclude;
            set => this.SetProperty(ref this.exclude, value);
        }

        public bool Filter
        {
            get => this.filter;
            set => this.SetProperty(ref this.filter, value);
        }

        public string Tag { get; set; }
    }
}
