// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using AveoAudio.ViewModels;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using System.Diagnostics;

using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AveoAudio;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private const string DocumentsSubfolderName = nameof(AveoAudio);
    private const string LogFileName = "Log.txt";

    private MainViewModel? mainViewModel;
    private Window? window;

    private StorageFolder? documentsFolder;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        UnhandledException += OnUnhandledException;
    }

    public new static App Current => (App)Application.Current;

    public AppSettings AppSettings { get; private set; } = AppSettings.Default;

    public StorageFolder DocumentsFolder => this.documentsFolder ?? throw new InvalidOperationException();

    public MainViewModel MainViewModel => this.mainViewModel ?? throw new InvalidOperationException();

    public void Dispatch(Action action)
    {
        this.window!.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => action());
    }

    public Task GetBusy(Task task, string description) => this.mainViewModel!.GetBusy(task, description);

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        window = new MainWindow();

        // Create a Frame to act as the navigation context.
        var rootFrame = new Frame();

        // Place the frame in the current Window
        window.Content = rootFrame;
        window.Activate();

        await EnsureDocumentsFolder();
        await Task.WhenAll(LoadSettings(), MusicLibrary.Current.InitializeAsync());

        InitializeTracing();

        rootFrame.Navigate(typeof(MainPage), this.AppSettings);

        this.mainViewModel = ((MainPage)rootFrame.Content).ViewModel;
        await this.mainViewModel!.Initialize();
    }

    private async Task EnsureDocumentsFolder()
    {
        var library = KnownFolders.DocumentsLibrary;

        this.documentsFolder = await library.TryGetItemAsync(DocumentsSubfolderName) as StorageFolder;
        this.documentsFolder ??= await library.CreateFolderAsync(DocumentsSubfolderName);
    }

    private void InitializeTracing()
    {
        var listener = (DefaultTraceListener)Trace.Listeners[0];
        listener.LogFileName = Path.Combine(this.documentsFolder!.Path, LogFileName);
    }

    private async Task LoadSettings()
    {
        this.AppSettings = await SettingsManager.GetSettingsAsync() ?? AppSettings.Default;
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Logger.LogError(e.Exception);
    }
}
