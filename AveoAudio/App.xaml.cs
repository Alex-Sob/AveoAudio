// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AveoAudio;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    public new static App Current => (App)Application.Current;

    public AppSettings AppSettings { get; private set; }

    public UserSettings UserSettings { get; } = new();

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();

        // Create a Frame to act as the navigation context.
        Frame rootFrame = new Frame();

        // Place the frame in the current Window
        m_window.Content = rootFrame;

        m_window.Activate();

        this.AppSettings = await SettingsManager.GetSettingsAsync();
        rootFrame.Navigate(typeof(MainPage), this.AppSettings);
    }

    private Window m_window;
}
