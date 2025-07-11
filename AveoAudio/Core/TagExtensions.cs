namespace AveoAudio;

public static class TagExtensions
{
    public static bool Is(this Tag tag, string value) => tag.AsSpan().Equals(value, StringComparison.Ordinal);

    public static bool IsTimeOfDay(this Tag tag, out TimesOfDay timesOfDay) => Enum.TryParse(tag, out timesOfDay);

    public static bool IsTimeOfDay(this string tag, out TimesOfDay timesOfDay) => Enum.TryParse(tag, out timesOfDay);
}
