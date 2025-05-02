using System;
using System.Security.Cryptography;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Kantab.GUI.ViewModels;
using Kantab.GUI.Views;

namespace Kantab.GUI;

public partial class App : Application {
    public static DesktopNotifications.INotificationManager? NotifMan = null;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void BringBackMainWin() {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
           desktop.MainWindow.Show();
        }
    }

    public void ShutdownKantab() {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.Shutdown();
        }
    }

    private void MiOpen_OnClick(object? sender, EventArgs e) {
        BringBackMainWin();
    }


    private void MiExit_OnClick(object? sender, EventArgs e) {
        ShutdownKantab();
    }
}
