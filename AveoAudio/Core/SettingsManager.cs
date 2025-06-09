using System.Text.Json;

using Windows.Storage;
using Windows.System;

namespace AveoAudio;

public static class SettingsManager
{
    private const string FileName = "AppSettings.json";

    public static Task<AppSettings?> GetSettingsAsync() => GetDataAsync<AppSettings>();

    public static async Task OpenLocalSettings()
    {
        var file = await GetFileAsync().ConfigureAwait(false);
        await Launcher.LaunchFileAsync(file);
    }

    private static async Task<StorageFile> CopyDefaultFileAsync()
    {
        var uri = new Uri($"ms-appx:///Data/{FileName}");
        var defaultFile = await StorageFile.GetFileFromApplicationUriAsync(uri);

        return await defaultFile.CopyAsync(App.Current.DocumentsFolder);
    }

    private static async Task<T?> GetDataAsync<T>()
    {
        var file = await GetFileAsync().ConfigureAwait(false);
        using var stream = await file.OpenAsync(FileAccessMode.Read);

        return JsonSerializer.Deserialize<T>(stream.AsStreamForRead());
    }

    private static async Task<StorageFile> GetFileAsync()
    {
        var file = await App.Current.DocumentsFolder.TryGetItemAsync(FileName) as StorageFile;
        return file ??= await CopyDefaultFileAsync().ConfigureAwait(false);
    }
}
