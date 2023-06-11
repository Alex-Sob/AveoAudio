using System.Collections.Generic;
using System.IO;
using System.Linq;

using Windows.Storage;

namespace AveoAudio
{
    public class ImageManager
    {
        public static readonly string DefaultPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Images");

        private const string DefaultFolder = "Default";

        private readonly string basePath;
        private readonly Dictionary<string, (int, IList<string>)> imagesByFolder;

        public ImageManager(string basePath)
        {
            this.basePath = basePath;
            this.imagesByFolder = new Dictionary<string, (int, IList<string>)>();
        }

        public string GetNextDefaultImage()
        {
            // TODO: Copy default images first time if there are none
            return this.GetNextImage(Path.Combine(this.basePath, DefaultFolder));
        }

        public string GetNextImage(string timeOfDay, string weather)
        {
            var folder = Path.Combine(this.basePath, timeOfDay, weather);
            var nextImage = this.GetNextImage(folder);
            if (nextImage != null) return nextImage;

            folder = Path.Combine(this.basePath, timeOfDay);
            nextImage = this.GetNextImage(folder);
            if (nextImage != null) return nextImage;

            return this.GetNextDefaultImage();
        }

        private string GetNextImage(string path)
        {
            if (this.imagesByFolder.TryGetValue(path, out (int current, IList<string> images) item))
            {
                var current = item.current < item.images.Count - 1 ? item.current + 1 : 0;
                this.imagesByFolder[path] = (current, item.images);
                return item.images[current];
            }
            else
            {
                var images = Directory.Exists(path) ? Directory.EnumerateFiles(path) : Enumerable.Empty<string>();
                var randomizedImages = images.Randomize().ToList();

                if (randomizedImages.Any())
                {
                    this.imagesByFolder[path] = (0, randomizedImages);
                    return randomizedImages[0];
                }
            }

            return null;
        }
    }
}
