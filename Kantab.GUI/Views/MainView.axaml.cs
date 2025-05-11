using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Kantab.Classes;
using Kantab.Structs;
using Kantab.GUI.Enums;
using Kantab.GUI.ViewModels;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using Kantab.Classes.PenStateProviders;

namespace Kantab.GUI.Views;

public partial class MainView : UserControl
{
    private KantabServer _server = new(new());
    private MainViewModel vm = new();
    private Rectangle _previousScissor;
    private ScreenScissorSetOverlay? _scrOverlay;
    private Window? _myWin;
    public MainView() {
        
        DataContext = vm;
        vm.RelayStateProv = new KRelayPenStateProvider(_server);
        InitializeComponent();
        OnServerOff();

        _server.ServerStarted += (sender, args) =>
        {
            OnServerOn();
        };

        _server.ServerStopped += (sender, args) =>
        {
            OnServerOff();
        };

        _server.SetupModePositionReceived += UpdateWorkingScissor;

        tbPort.Text = _server.LoadedSettings.Port.ToString();
       
    }

    protected override void OnInitialized() {
        base.OnInitialized();

        EnsureMainWindow();
        try {
            KantabSettings? setts = null;
            setts = JsonConvert.DeserializeObject<KantabSettings>(File.ReadAllText(vm.ConfigPath));
            _server.SetSettings(setts ?? new());
        }
        catch {
            var msgbox = MessageBoxManager.GetMessageBoxStandard("Kantab Settings",
                "Note: Settings could not be read. Using default settings.\nPlease confirm your current settings.", icon: Icon.Error, windowStartupLocation: WindowStartupLocation.CenterOwner);
            msgbox.ShowAsync();
        }

        vm.ScreenRegionScissor = _server.LoadedSettings.ScreenRegion;
        RefillConstructCombobox();
        cbProvider.SelectedIndex = _server.LoadedSettings.PenProvider;
        nudScale.Value = (decimal)_server.LoadedSettings.Scale;
        nudPort.Value =  _server.LoadedSettings.Port;
    }

    private void OnServerOff()
    {
        lbServerStatus.Text = "turned off.";
    }

    private void OnServerOn()
    {
        lbServerStatus.Text = "turned on.";
    }

    private void BtnExit_OnClick(object sender, RoutedEventArgs args)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) lifetime.Shutdown();
    }

    private void BtnStartSrv_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!_server.Running)
        {
            _server.Start();
        }
        else
        {
            _server.Stop();
        }
    }

    private bool _previouslyPressed;
    private bool _updatingWorkingScissor;


    private void EnsureMainWindow()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && _myWin == null)
        {
            _myWin = desktop.MainWindow;
        }
    }
    public void UpdateWorkingScissor(object? sender, PenState received)
    {
        if (!_updatingWorkingScissor) return;

        Rectangle currentScissor = vm.ScreenRegionScissor;
        if (vm.ScissorSetupState == ScissorSetupState.NONE)
        {

            EnsureMainWindow();

            vm.ScissorSetupState = ScissorSetupState.TOPLEFT;
            Dispatcher.UIThread.Post(() =>
            {
                _myWin.Topmost = true;
                _scrOverlay = new();
                _scrOverlay.Show(_myWin);
            });


        }

        bool pressed = received.Pressure > 0;
        if (vm.ScissorSetupState == ScissorSetupState.TOPLEFT)
        {
            currentScissor.TopLeft = received.Position;
        }
        else if (vm.ScissorSetupState == ScissorSetupState.BOTTOMRIGHT)
        {
            currentScissor.BottomRight = received.Position;
        }

        vm.ScreenRegionScissor = currentScissor;

        if (pressed && !_previouslyPressed)
        {
            _previouslyPressed = true;
        }

        if (!pressed && _previouslyPressed)
        {
            _previouslyPressed = false;

            if (vm.ScissorSetupState == ScissorSetupState.TOPLEFT)
            {
                vm.ScissorSetupState = ScissorSetupState.BOTTOMRIGHT;
                return;
            }

            if (vm.ScissorSetupState == ScissorSetupState.BOTTOMRIGHT)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _myWin.Topmost = false;
                    _scrOverlay?.Close();
                    _scrOverlay = null;
                });
                vm.ScreenRegionScissor = vm.ScreenRegionScissor.Normalize();
                _updatingWorkingScissor = false;
                _server.RegionSetupMode = false;
                _server.LoadedSettings.ScreenRegion = vm.ScreenRegionScissor;
                vm.ScissorSetupState = ScissorSetupState.NONE;

                return;
            }
        }
    }

    private void RefillConstructCombobox()
    {
        cbConstruct.Items.Clear();

        // walking around mvvm
        int index = 0;
        bool foundSelected = false;

        foreach (var constructPair in _server.AvailableConstructs)
        {
            ComboBoxItem cbItem = new();
            cbItem.Tag = constructPair.Key;
            cbItem.Content = constructPair.Value.Name;
            cbConstruct.Items.Add(cbItem);
            if (!foundSelected)
            {
                if (_server.CurrentConstruct.GetValueOrDefault(default).Id == constructPair.Value.Id)
                {
                    foundSelected = true;
                }
                else
                {
                    index++;
                }

            }
        }

        cbConstruct.SelectedIndex = index;
    }

    private void ReloadConstructList()
    {
        _server.LoadConstructs();
        RefillConstructCombobox();
    }

    private void UpdateSelectedConstructInfo()
    {
        if (cbConstruct.SelectedItem != null)
        {
            ConstructMetadata construct =
                _server.AvailableConstructs[((string?)((ComboBoxItem)cbConstruct.SelectedItem).Tag)];
            tbConstructMeta.Text = $"Name: {construct.Name} — Made by: {construct.Author}\n\n{construct.Description}";
        }
        else
        {
            tbConstructMeta.Text = "";
        }
    }

    private void BtnSaveConsSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        string selectedConstructId = ((string?)((ComboBoxItem)cbConstruct.SelectedItem).Tag);
        _server.LoadedSettings.Scale = (float)nudScale.Value;
        _server.LoadedSettings.ConstructFolder = selectedConstructId;
        _server.SetSettings(_server.LoadedSettings);
        _server.SelectConstruct(selectedConstructId);
        SaveSettings();
    }

    private void BtnReloadCons_OnClick(object? sender, RoutedEventArgs e)
    {
        ReloadConstructList();
    }

    private void CbConstruct_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateSelectedConstructInfo();
    }

    private void BtnScreenScissor_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!_server.Running)
        {
            EnsureMainWindow();
            var msgbox = MessageBoxManager.GetMessageBoxStandard("Server not running!",
                "Kantab Server must be running to change screen region.", icon: Icon.Warning, windowStartupLocation: WindowStartupLocation.CenterOwner);
            msgbox.ShowWindowDialogAsync(_myWin);
            return;
        }

        _previousScissor = _server.LoadedSettings.ScreenRegion;
        _updatingWorkingScissor = true;
        _server.RegionSetupMode = true;
    }

    private void BtnServerSettsUndo_OnClick(object? sender, RoutedEventArgs e)
    {
        vm.ScreenRegionScissor = _previousScissor;
        nudPort.Value = _server.LoadedSettings.Port;
    }

    private void BtnServerSettsSave_OnClick(object? sender, RoutedEventArgs e)
    {
        _server.LoadedSettings.PenProvider = (byte)cbProvider.SelectedIndex;
        _previousScissor = vm.ScreenRegionScissor;
        _server.LoadedSettings.ScreenRegion = _previousScissor;
        _server.LoadedSettings.Port = (short)Math.Truncate(nudPort.Value ?? 7329);
        tbPort.Text = _server.LoadedSettings.Port.ToString();
        SaveSettings();
    }

    private void SaveSettings() {
        EnsureMainWindow();
        try {
            File.WriteAllText(vm.ConfigPath, JsonConvert.SerializeObject(_server.LoadedSettings));
            var msgbox = MessageBoxManager.GetMessageBoxStandard("Kantab Settings",
                "Settings saved.", icon: Icon.Success, windowStartupLocation: WindowStartupLocation.CenterOwner);
            msgbox.ShowWindowDialogAsync(_myWin);
        }
        catch {
            var msgbox = MessageBoxManager.GetMessageBoxStandard("Kantab Settings",
                "Settings could not be saved.\nCan Kantab write to the directory it resides in?", icon: Icon.Error, windowStartupLocation: WindowStartupLocation.CenterOwner);
            msgbox.ShowWindowDialogAsync(_myWin);
        }
    }

    private void cbProvider_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e) {

        _server.PenStateProvider = (cbProvider?.SelectedIndex ?? 0 ) == 0 ? vm.MouseStateProv : vm.RelayStateProv; // lol
    }

    private void BtnCopy_OnClick(object? sender, RoutedEventArgs e) {
        EnsureMainWindow();
        Process.Start(new ProcessStartInfo($"http://localhost:{_server.LoadedSettings.Port}/views/") {UseShellExecute = true});
    }

    private void BtnGPSI_OnClick(object? sender, RoutedEventArgs e) {
        EnsureMainWindow();
        Process.Start(new ProcessStartInfo("https://glitchypsi.xyz") { UseShellExecute = true });
    }

    private void BtnGithub_OnClick(object? sender, RoutedEventArgs e) {
        EnsureMainWindow();
        Process.Start(new ProcessStartInfo("https://github.com/GlitchyPSIX/Kantab") { UseShellExecute = true });
    }

    private void BtnDonate_OnClick(object? sender, RoutedEventArgs e) {
        EnsureMainWindow();
        Process.Start(new ProcessStartInfo("https://ko-fi.com/glitchypsi") { UseShellExecute = true });
    }
    private void BtnItch_OnClick(object? sender, RoutedEventArgs e) {
        EnsureMainWindow();
        Process.Start(new ProcessStartInfo("https://glitchypsi.itch.io") { UseShellExecute = true });
    }
}
