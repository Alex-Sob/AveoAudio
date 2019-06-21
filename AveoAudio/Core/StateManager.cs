using System;

using Windows.Storage;

namespace AveoAudio
{
    public static class StateManager
    {
        private const string LastPlayedDatesContainer = "LastPlayedDates";

        static StateManager()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.CreateContainer(LastPlayedDatesContainer, ApplicationDataCreateDisposition.Always);
        }

        public static DateTime? GetLastPlayed(Track track)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var value = (DateTimeOffset?)localSettings.Containers[LastPlayedDatesContainer].Values[track.FileName];
            return value?.Date;
        }

        public static void SetLastPlayed(Track track)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Containers[LastPlayedDatesContainer].Values[track.FileName] = DateTimeOffset.Now;
        }
    }
}
