using System;
using System.Data;
using System.IO;
using System.Reflection;
using Kantab.Classes;
using Kantab.Classes.PenStateProviders;
using Kantab.GUI.Enums;
using Rectangle = Kantab.Structs.Rectangle;

namespace Kantab.GUI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private Rectangle _liveScissor;
    public MousePenStateProvider MouseStateProv = new();
    public KRelayPenStateProvider RelayStateProv;
    public Rectangle ScreenRegionScissor
    {
        get => _liveScissor;
        set
        {
            SetProperty(ref _liveScissor, value);
            OnPropertyChanged(nameof(RegionScissorString));
        }
    }

    private ScissorSetupState _scissorState;

    public ScissorSetupState ScissorSetupState {
        get => _scissorState;
        set
        {
            SetProperty(ref _scissorState, value);
            OnPropertyChanged(nameof(SettingTopLeft));
            OnPropertyChanged(nameof(SettingBotomRight));
            OnPropertyChanged(nameof(SettingRectangle));
        }
    }

    public string RegionScissorString => $"Current: ({ScreenRegionScissor.TopLeft.X}, {ScreenRegionScissor.TopLeft.Y}) — ({ScreenRegionScissor.BottomRight.X}, {ScreenRegionScissor.BottomRight.Y})";
    public bool SettingTopLeft => _scissorState == ScissorSetupState.TOPLEFT;
    public bool SettingBotomRight => _scissorState == ScissorSetupState.BOTTOMRIGHT;
    public bool SettingRectangle => _scissorState != ScissorSetupState.NONE;

    public string VersionString => $"version v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";

    public string ConfigPath => Path.Join(AppDomain.CurrentDomain.BaseDirectory, "config.json");
}
