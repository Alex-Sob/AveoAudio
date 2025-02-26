using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace AveoAudio
{
    // TODO: [Images] Seasons
    // TODO: [Images] Provide UI to rotate image
    public class ImageManager
    {
        private const string DefaultFolder = "Default";

        private readonly Dictionary<string, (int, IList<StorageFile>)> imagesByFolder;

        public ImageManager()
        {
            this.imagesByFolder = new();
        }

        public async Task<string> GetNextDefaultImage()
        {
            // TODO: Copy default images first time if there are none
            var folder = await KnownFolders.PicturesLibrary.TryGetItemAsync(DefaultFolder) as StorageFolder;
            if (folder == null) return null;

            return await this.GetNextImage(folder);
        }

        public async Task<string> GetNextImage(string timeOfDay, string weather)
        {
            var folder = await GetFolder(timeOfDay, weather);

            if (folder != null)
            {
                var nextImage = await this.GetNextImage(folder);
                if (nextImage != null) return nextImage;
            }

            return await this.GetNextDefaultImage();
        }

        private static async Task<StorageFolder> GetFolder(string timeOfDay, string weather)
        {
            if (string.IsNullOrEmpty(timeOfDay)) return null;

            var folder = await KnownFolders.PicturesLibrary.TryGetItemAsync(timeOfDay) as StorageFolder;
            
            if (folder == null) return null;
            if (string.IsNullOrEmpty(weather)) return folder;

            folder = await folder.TryGetItemAsync(weather) as StorageFolder;
            if (folder != null) return folder;

            return folder;
        }

        private async Task<string> GetNextImage(StorageFolder folder)
        {
            if (this.imagesByFolder.TryGetValue(folder.Path, out (int current, IList<StorageFile> images) pair))
            {
                var current = pair.current < pair.images.Count - 1 ? pair.current + 1 : 0;
                this.imagesByFolder[folder.Path] = (current, pair.images);
                return pair.images[current].Path;
            }
            else
            {
                var images = await folder.GetFilesAsync();
                var randomizedImages = images.Shuffle().ToList();

                if (randomizedImages.Any())
                {
                    this.imagesByFolder[folder.Path] = (0, randomizedImages);
                    return randomizedImages[0].Path;
                }
            }

            return null;
        }
    }
}
