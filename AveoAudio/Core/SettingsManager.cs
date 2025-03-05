using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.System;

namespace AveoAudio
{
    public static class SettingsManager
    {
        private const string FileName = "AppSettings.json";
        private const string FolderName = nameof(AveoAudio);

        public static Task<AppSettings> GetSettingsAsync() => GetDataAsync<AppSettings>();

        public static async Task OpenLocalSettings()
        {
            var file = await GetFileAsync().ConfigureAwait(false);
            await Launcher.LaunchFileAsync(file);
        }

        private static async Task<StorageFile> CopyDefaultFileAsync()
        {
            var uri = new Uri($"ms-appx:///Data/{FileName}");
            var defaultFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var folder = await KnownFolders.DocumentsLibrary.GetFolderAsync(FolderName);
            return await defaultFile.CopyAsync(folder);
        }

        private static async Task<T> GetDataAsync<T>()
        {
            var file = await GetFileAsync().ConfigureAwait(false);
            using var stream = await file.OpenAsync(FileAccessMode.Read);

            var settings = new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true };
            var serializer = new DataContractJsonSerializer(typeof(T), settings);
            return (T)serializer.ReadObject(stream.AsStreamForRead());
        }

        private static async Task<StorageFile> GetFileAsync()
        {
            var folder = await KnownFolders.DocumentsLibrary.TryGetItemAsync(FolderName) as StorageFolder;
            folder ??= await KnownFolders.DocumentsLibrary.CreateFolderAsync(FolderName);

            var file = await folder.TryGetItemAsync(FileName) as StorageFile;
            return file ??= await CopyDefaultFileAsync().ConfigureAwait(false);
        }
    }
}
