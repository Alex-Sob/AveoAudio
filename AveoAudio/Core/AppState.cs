namespace AveoAudio
{
    public class AppState : NotificationBase
    {
        private TimesOfDay? timeOfDay;
        private Weather? weather;

        public TimesOfDay? TimeOfDay
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
