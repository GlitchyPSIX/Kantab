using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Kantab.Classes;

namespace Kantab.GUI.Views;

public partial class MainView : UserControl {
    private KantabServer _server = new (null);
    public MainView()
    {
        InitializeComponent();
        OnServerOff();

        _server.ServerStarted += (sender, args) => {
            OnServerOn();
        };

        _server.ServerStopped += (sender, args) => {
            OnServerOff();
        };
    }

    private void OnServerOff() {
        lbServerStatus.Text = "turned off.";
    }

    private void OnServerOn()
    {
        lbServerStatus.Text = "turned on.";
    }

    private void BtnExit_OnClick(object sender, RoutedEventArgs args) {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) lifetime.Shutdown();
    }

    private void BtnStartSrv_OnClick(object? sender, RoutedEventArgs e) {
        _server.Start();
    }
}
