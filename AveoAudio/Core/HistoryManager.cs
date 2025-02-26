using System;

using Windows.Storage;

namespace AveoAudio
{
    // TODO: Consider loading history on startup
    public static class HistoryManager
    {
        private const string LastPlayedDatesContainer = "LastPlayedDates";

        static HistoryManager()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.CreateContainer(LastPlayedDatesContainer, ApplicationDataCreateDisposition.Always);
        }

        public static DateTime? GetLastPlayedOn(Track track)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var value = (DateTimeOffset?)localSettings.Containers[LastPlayedDatesContainer].Values[track.FileName];
            return value?.Date;
        }

        public static void Add(Track track)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Containers[LastPlayedDatesContainer].Values[track.FileName] = DateTimeOffset.Now;
        }
    }
}
