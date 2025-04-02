using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Windows.Storage;

namespace AveoAudio;

public class ImageManager
{
    private const string DefaultFolder = "Default";
    private const string LibrarySubfolderName = nameof(AveoAudio);

    private readonly Dictionary<string, ICollection<string>> imagesByFolder = [];

    public async Task<string> GetNextDefaultImage()
    {
        var appFolder = await GetLibrarySubfolder();
        if (appFolder == null) return null;

        var defaultFolder = await appFolder.TryGetItemAsync(DefaultFolder) as StorageFolder;
        return defaultFolder != null ? await GetNextImage(defaultFolder) : null;
    }

    public async Task<string> GetNextImage(string season, string timeOfDay, string weather)
    {
        var folder = await GetFolderByProbing(season, timeOfDay, weather);
        return folder != null ? await this.GetNextImage(folder) : null;
    }

    private static async Task<StorageFolder> GetLibrarySubfolder()
    {
        return await KnownFolders.PicturesLibrary.GetFolderAsync(LibrarySubfolderName);
    }

    private static async Task<StorageFolder> GetFolderByProbing(string season, string timeOfDay, string weather)
    {
        var appFolder = await GetLibrarySubfolder();
        if (appFolder == null) return null;

        var folder = GetProbingFolders(appFolder.Path, season, timeOfDay, weather).FirstOrDefault(Directory.Exists);
        return folder != null ? await StorageFolder.GetFolderFromPathAsync(folder) : null;
    }

    private static string GetNextImage(IReadOnlyList<StorageFile> files, ICollection<string> images)
    {
        var nextFile = files.Where(f => !images.Contains(f.Name)).Random();

        if (nextFile != null)
        {
            images.Add(nextFile.Name);
            return nextFile.Path;
        }

        return null;
    }

    private static IEnumerable<string> GetProbingFolders(string path, string season, string timeOfDay, string weather)
    {
        yield return Path.Join(path, season, timeOfDay, weather);
        yield return Path.Join(path, season, timeOfDay);
        yield return Path.Join(path, DefaultFolder, timeOfDay, weather);
        yield return Path.Join(path, DefaultFolder, timeOfDay);
        yield return Path.Join(path, DefaultFolder);
    }

    private async Task<string> GetNextImage(StorageFolder folder)
    {
        var files = await folder.GetFilesAsync();

        if (!this.imagesByFolder.TryGetValue(folder.Path, out var images))
            this.imagesByFolder[folder.Path] = images = new HashSet<string>();

        var next = GetNextImage(files, images);
        if (next != null) return next;

        if (images.Count > 0)
        {
            images.Clear();
            return GetNextImage(files, images);
        }

        return null;
    }
}
