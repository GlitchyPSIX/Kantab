using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Kantab.Classes;
using Kantab.Structs;

namespace Kantab.GUI.Views;

public partial class MainView : UserControl {
    private KantabServer _server = new (new());
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

    private void BtnSaveConsSettings_OnClick(object? sender, RoutedEventArgs e) {
        KantabSettings setts = _server.LoadedSettings with {Scale = (float)nudScale.Value};
        _server.SetSettings(setts);
    }
}
