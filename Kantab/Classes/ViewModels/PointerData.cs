using System;
using System.Numerics;
using System.Threading;
using System.Windows.Input;

namespace Kantab.Classes.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public class PointerData : ObservableObject
{
    public Vector2 MousePosition { get; set; }
    private Timer _pollingTimer;

    public PointerData() {
        _pollingTimer = new Timer(pollingTimerTick, null, TimeSpan.Zero,
             // 30fps
             TimeSpan.FromMilliseconds(33.34f));
    }

    void pollingTimerTick(object? sender) {
        Console.WriteLine("Tick");
        MousePosition = CursorData.CursorPosition(Vector2.Zero);
        OnPropertyChanged(nameof(MousePosition));
    }
}