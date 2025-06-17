namespace AveoAudio;

public readonly struct Season
{
    private readonly string season;

    private Season(string season) => this.season = season;

    public static Season Autumn = new(nameof(Autumn));

    public static Season Current => (DateTime.Today.Month / 3 % 4) switch
    {
        0 => Winter,
        1 => Spring,
        2 => Summer,
        3 => Autumn,
        _ => default
    };

    public static Season Spring = new(nameof(Spring));

    public static Season Summer = new(nameof(Summer));

    public static Season Winter = new(nameof(Winter));

    public static implicit operator string(Season season) => season.season;
}
