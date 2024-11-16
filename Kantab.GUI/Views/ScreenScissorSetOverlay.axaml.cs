using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Kantab.GUI;

public partial class ScreenScissorSetOverlay : Window
{
    public ScreenScissorSetOverlay()
    {
        InitializeComponent();
    }

    protected override void OnClosing(WindowClosingEventArgs e) {
        if (!e.IsProgrammatic) {
            e.Cancel = true;
            return;
        }
        base.OnClosing(e);
    }
}