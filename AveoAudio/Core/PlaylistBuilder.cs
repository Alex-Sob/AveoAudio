namespace AveoAudio;

public class PlaylistBuilder(IEnumerable<Track> source)
{
    private IEnumerable<Track> query = source;

    public IEnumerable<Track> Build() => this.query;

    public PlaylistBuilder ExcludeAlreadyPlayed(bool exclude)
    {
        if (exclude) this.query = this.query.Where(track => !track.LastPlayedOn.HasValue);
        return this;
    }

    public PlaylistBuilder ExcludeTags(ICollection<string> tags)
    {
        if (tags.Count == 0) return this;

        var mask = CommonTags.CreateMask(tags);
        this.query = this.query.Where(track => !track.CommonTags.HasAny(mask));

        return this;
    }

    public PlaylistBuilder FilterByTags(ICollection<string> tags)
    {
        if (tags.Count == 0) return this;

        var mask = CommonTags.CreateMask(tags);
        this.query = this.query.Where(track => track.CommonTags.HasAll(mask));

        return this;
    }

    public PlaylistBuilder WithOutOfRotationTimeSinceAdded(int days)
    {
        if (days > 0)
        {
            var startingDate = DateTime.Today.AddDays(-days);
            this.query = this.query.Where(track => track.DateAdded < startingDate);
        }

        return this;
    }

    public PlaylistBuilder WithOutOfRotationTimeSincePlayed(int days)
    {
        if (days > 0)
        {
            var startingDate = DateTime.Today.AddDays(-days);
            this.query = this.query.Where(track => !track.LastPlayedOn.HasValue || track.LastPlayedOn < startingDate);
        }

        return this;
    }

    public PlaylistBuilder WithTimeOfDay(TimesOfDay? timeOfDay, bool ifBestTimeOnly = false)
    {
        timeOfDay ??= TimesOfDay.None;

        if (ifBestTimeOnly)
            this.query = this.query.Where(track => track.IsBestTimeOfDay(timeOfDay.Value));
        else
            this.query = this.query.Where(track => track.TimesOfDay.HasFlag(timeOfDay) || track.TimesOfDay == TimesOfDay.None);

        return this;
    }

    public PlaylistBuilder WithWeather(Weather? weather)
    {
        this.query = this.query.Where(track => track.Weather == Weather.None || track.Weather == weather);
        return this;
    }
}
