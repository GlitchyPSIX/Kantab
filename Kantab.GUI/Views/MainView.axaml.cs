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
    }

    private void BtnExit_OnClick(object sender, RoutedEventArgs args) {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) lifetime.Shutdown();
    }
}
