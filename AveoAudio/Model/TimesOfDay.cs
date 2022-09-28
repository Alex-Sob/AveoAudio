using System;

namespace AveoAudio
{
    [Flags]
    public enum TimeOfDay
    {
        EarlyMorn = 1,
        LateMorn = 2,
        Afternoon = 4,
        EarlyEvening = 8,
        Sunset = 16,
        Twilight = 32,
        Night = 64,

        Morning = 3,
        Daytime = 31,
        Evening = 56,
        SunDown = 96,
    }
}
