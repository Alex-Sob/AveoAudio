using System;

namespace AveoAudio.ViewModels
{
    public class SelectorsViewModel : NotificationBase
    {
        private readonly AppState appState;

        private bool isOpen;

        public SelectorsViewModel(AppState appState)
        {
            this.appState = appState;
        }

        public bool CanSelectWeather { get; set; } = true;

        public string SelectedTimeOfDay
        {
            get => this.appState.TimeOfDay.ToString();
            set
            {
                this.appState.TimeOfDay = ParseEnum<TimeOfDay>(value);

                this.CanSelectWeather = this.appState.TimeOfDay < TimeOfDay.Twilight;
                this.OnPropertyChanged(nameof(CanSelectWeather));

                if (!CanSelectWeather) this.SelectedWeather = null;
            }
        }

        public bool IsOpen
        {
            get => this.isOpen;
            set => this.SetProperty(ref this.isOpen, value);
        }

        public string SelectedWeather
        {
            get => this.appState.Weather?.ToString();
            set
            {
                var oldValue = this.appState.Weather;
                this.appState.Weather = ParseEnum<Weather>(value);
                if (this.appState.Weather != oldValue) this.OnPropertyChanged();
            }
        }

        private static T? ParseEnum<T>(string value) where T : struct
        {
            return !string.IsNullOrEmpty(value) ? (T?)Enum.Parse(typeof(T), value) : null;
        }
    }
}
