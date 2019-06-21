using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace AveoAudio
{
    public class ImageManager
    {
        public static readonly string DefaultPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Images");

        private const string DefaultFolder = "Default";

        private readonly string basePath;
        private readonly Dictionary<string, (int, IList<StorageFile>)> imagesByFolder;

        public ImageManager(string basePath)
        {
            this.basePath = basePath;
            this.imagesByFolder = new Dictionary<string, (int, IList<StorageFile>)>();
        }

        public Task<ImageSource> GetNextDefaultImageAsync()
        {
            return this.GetNextImageAsync(DefaultFolder);
        }

        public async Task<ImageSource> GetNextImageAsync(string timeOfDay, string weather)
        {
            string folder;

            switch (timeOfDay)
            {
                case nameof(TimeOfDay.Twilight):
                case nameof(TimeOfDay.Night):
                    folder = timeOfDay;
                    break;
                default:
                    var sunset = nameof(Weather.Sunset);
                    folder = weather == sunset ? sunset : $"{weather} {timeOfDay}";
                    break;
            }

            return await GetNextImageAsync(folder) ?? await GetNextImageAsync(DefaultFolder);
        }

        private async Task<IReadOnlyList<StorageFile>> GetImagesAsync(string path)
        {
            path = Path.Combine(this.basePath, path);
            if (!Directory.Exists(path)) return new List<StorageFile>();
            var folder = await StorageFolder.GetFolderFromPathAsync(path);
            return await folder.GetFilesAsync();
        }

        private async Task<ImageSource> GetNextImageAsync(string folder)
        {
            StorageFile nextImage = null;

            if (this.imagesByFolder.TryGetValue(folder, out (int current, IList<StorageFile> images) item))
            {
                var current = item.current < item.images.Count - 1 ? item.current + 1 : 0;
                this.imagesByFolder[folder] = (current, item.images);
                nextImage = item.images[current];
            }
            else
            {
                var images = await GetImagesAsync(folder);
                if (images.Any())
                {
                    var randomizedImages = images.Randomize().ToList();
                    this.imagesByFolder[folder] = (0, randomizedImages);
                    nextImage = randomizedImages[0];
                }
            }

            return nextImage != null ? new BitmapImage(new Uri(nextImage.Path)) : null;
        }
    }
}
