using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace AveoAudio.Tagging
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var track in GetTracks())
            {
                var file = TagLib.File.Create(track);
                var tag = file.Tag;
                var comment = tag.Comment ?? string.Empty;
                bool changed = false;

                if (tag.Subtitle != null && !tag.Subtitle.Contains(";")) Console.WriteLine($"Invalid subtitle: {track}");

                if (string.IsNullOrEmpty(tag.Subtitle))
                {
                    tag.Subtitle = $"{new FileInfo(track).CreationTime.ToString("dd.MM.yyyy")};{comment.Replace(" ", string.Empty)}";
                    tag.Comment = string.Empty;
                    changed = true;
                }

                if (changed) file.Save();
            }

            Properties.Settings.Default.LastRunTime = DateTime.Now;
            Properties.Settings.Default.Save();

            Console.WriteLine("Finished");
            Console.ReadKey(true);
        }

        private static IEnumerable<string> GetTracks()
        {
            var lastRunTime = Properties.Settings.Default.LastRunTime;
            var tracks = Directory.EnumerateFiles(ConfigurationManager.AppSettings["Path"], "*.mp3", SearchOption.AllDirectories);

            if (lastRunTime == default(DateTime)) return tracks;

            return from track in tracks
                   //where File.GetCreationTime(track) >= lastRunTime
                   select track;
        }
    }
}
