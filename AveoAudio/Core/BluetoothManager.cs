using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.System;

namespace AveoAudio;

public static class BluetoothManager
{
    private static int connectedDeviceCount;
    private static DeviceWatcher? watcher;

    public static void WatchConnectedDevices()
    {
        var aqs = BluetoothDevice.GetDeviceSelectorFromConnectionStatus(BluetoothConnectionStatus.Connected);

        watcher = DeviceInformation.CreateWatcher(aqs);
        watcher.Added += OnConnectedDeviceAdded;
        watcher.Removed += OnConnectedDeviceRemoved;
        watcher.Start();
    }

    public static event EventHandler? DeviceConnected;

    public static event EventHandler? DeviceDisconnected;

    public static bool HasConnectedDevices => connectedDeviceCount > 0;

    public static async Task OpenSettingsAsync() => await Launcher.LaunchUriAsync(new Uri("ms-settings:devices"));

    private static void OnConnectedDeviceAdded(DeviceWatcher sender, DeviceInformation args)
    {
        connectedDeviceCount++;
        DeviceConnected?.Invoke(null, EventArgs.Empty);
    }

    private static void OnConnectedDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
    {
        connectedDeviceCount--;
        DeviceDisconnected?.Invoke(null, EventArgs.Empty);
    }
}
