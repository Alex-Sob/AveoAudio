using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

using Windows.Storage;

namespace AveoAudio
{
    public static class SettingsManager
    {
        private const string FileName = "AppSettings.json";

        public static Task<AppSettings> GetSettingsAsync()
        {
            return GetData<AppSettings>(FileName);
        }

        private static async Task<StorageFile> EnsureFileAsync(string name)
        {
            var file = await GetFileAsync(name);
            if (file != null) return file;

            var uri = new Uri($"ms-appx:///Data/{name}");
            var defaultFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var localFolder = ApplicationData.Current.LocalFolder;
            return await defaultFile.CopyAsync(localFolder);
        }

        private static async Task<T> GetData<T>(string fileName)
        {
            var file = await EnsureFileAsync(fileName);

            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                var settings = new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true };
                var serializer = new DataContractJsonSerializer(typeof(T), settings);
                return (T)serializer.ReadObject(stream.AsStreamForRead());
            }
        }

        private static async Task<StorageFile> GetFileAsync(string name)
        {
            var folder = ApplicationData.Current.LocalFolder;
            if (await folder.TryGetItemAsync(name) == null) return null;
            return await folder.GetFileAsync(name);
        }
    }
}
