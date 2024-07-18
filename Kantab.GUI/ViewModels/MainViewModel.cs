using Kantab.Classes;
using Kantab.GUI.Enums;
using Kantab.Structs;

namespace Kantab.GUI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private Rectangle _liveScissor;
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
}
