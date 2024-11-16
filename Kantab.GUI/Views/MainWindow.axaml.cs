using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Notification = DesktopNotifications.Notification;

namespace Kantab.GUI.Views;

public partial class MainWindow : Window {
    private bool firstClose = false;
    private readonly DesktopNotifications.INotificationManager _notificationManager;

    public MainWindow()
    {
        InitializeComponent();
        _notificationManager = App.NotifMan ??
                               throw new InvalidOperationException("Missing notification manager");
    }

    protected override void OnClosing(WindowClosingEventArgs e) {
        if (e.CloseReason == WindowCloseReason.WindowClosing) {
            if (!firstClose) {
                firstClose = true;

                DesktopNotifications.Notification notif = new Notification() {
                    Title = "Kantab Server",
                    Body =
                        "Kantab Server is still running in the background!\nRightclick the tray icon for more options."
                };
                _notificationManager?.ShowNotification(notif);
            }
            e.Cancel = true;
            Hide();
            return;
        }
        base.OnClosing(e);
    }
}
