namespace AveoAudio
{
    public class AppState : NotificationBase
    {
        private TimeOfDay? timeOfDay;
        private Weather? weather;

        public TimeOfDay? TimeOfDay
        {
            get => this.timeOfDay;
            set => this.SetProperty(ref this.timeOfDay, value);
        }

        public Weather? Weather
        {
            get => this.weather;
            set => this.SetProperty(ref this.weather, value);
        }
    }
}
