namespace AveoAudio.ViewModels;

public class SelectorsViewModel : NotificationBase
{
    private bool isOpen;

    public bool CanSelectWeather { get; set; } = true;

    public string? SelectedTimeOfDay
    {
        get => this.TimeOfDay.ToString();
        set
        {
            this.TimeOfDay = ParseEnum<TimesOfDay>(value);
            this.OnPropertyChanged(nameof(this.TimeOfDay));

            this.CanSelectWeather = this.TimeOfDay < TimesOfDay.Sunset;
            this.OnPropertyChanged(nameof(CanSelectWeather));

            if (!CanSelectWeather) this.SelectedWeather = null;
        }
    }

    public bool IsOpen
    {
        get => this.isOpen;
        set => this.SetProperty(ref this.isOpen, value);
    }

    public string? SelectedWeather
    {
        get => this.Weather.ToString();
        set
        {
            var oldValue = this.Weather;
            this.Weather = ParseEnum<Weather>(value);
            if (this.Weather != oldValue) this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.Weather));
        }
    }

    public TimesOfDay? TimeOfDay { get; private set; }

    public Weather? Weather { get; private set; }

    private static T? ParseEnum<T>(string? value) where T : struct
    {
        return !string.IsNullOrEmpty(value) ? Enum.Parse<T>(value) : null;
    }
}
