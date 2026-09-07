using System.Media;
using Microsoft.Win32;

namespace IndiCaps;

internal sealed class CapsApp : IDisposable
{
    private readonly Settings _settings;
    private StreamWriter? _log;

    private NotifyIcon? _tray;
    private ContextMenuStrip? _menu;
    private ToolStripMenuItem? _openItem;
    private ToolStripMenuItem? _quitItem;
    private readonly CapsNotifierForm _notifier;
    private PanelForm? _panel;
    private Icon? _trayIcon;
    private SoundPlayer? _onSound;
    private SoundPlayer? _offSound;

    private IntPtr _hookId;
    private NativeMethods.LowLevelKeyboardProc _hookProc = null!;
    private bool _keyDown;
    private bool _capsOn;

    public CapsApp(Settings settings, bool debug)
    {
        _settings = settings;

        if (debug)
        {
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IndiCaps");
                Directory.CreateDirectory(dir);
                _log = new StreamWriter(Path.Combine(dir, "debug.log"), true);
            }
            catch { }
        }

        ApplyStartupRegistry(_settings.StartWithWindows);
        LoadSounds();
        _notifier = new CapsNotifierForm();
        BuildTray();

        _capsOn = ReadCapsLock();
        Log($"start capsOn={_capsOn} keystate={GetKeyStateBit()} font={FontService.LoadedFamilyName}");
        InstallHook();
    }

    private int GetKeyStateBit() => NativeMethods.GetKeyState((int)NativeMethods.VK_CAPITAL) & 0x0001;

    private static bool ReadCapsLock()
        => (NativeMethods.GetKeyState((int)NativeMethods.VK_CAPITAL) & 0x0001) != 0;

    private void InstallHook()
    {
        _hookProc = HookCallback;
        using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        _hookId = NativeMethods.SetWindowsHookEx(
            NativeMethods.WH_KEYBOARD_LL,
            _hookProc,
            NativeMethods.GetModuleHandle(curModule.ModuleName),
            0);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var data = System.Runtime.InteropServices.Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
            int msg = wParam.ToInt32();

            if (data.vkCode == NativeMethods.VK_CAPITAL)
            {
                bool down = msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN;
                bool up = msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP;

                if (down && !_keyDown)
                {
                    _keyDown = true;
                    _capsOn = !_capsOn;
                    Log($"press capsOn={_capsOn} keystate={GetKeyStateBit()}");
                    OnCapsChange();
                }
                else if (up)
                {
                    _keyDown = false;
                }
            }
        }

        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private void OnCapsChange()
    {
        if (_settings.SoundEnabled)
        {
            try { (_capsOn ? _onSound : _offSound)?.Play(); } catch { }
        }
        if (!_settings.IndicatorEnabled) return;

        string text = Lang.Get(_settings.Language, _capsOn ? "caps_on" : "caps_off");
        _notifier.Display(text, _capsOn, _settings.AnimationEnabled);
    }

    private void Log(string message)
    {
        if (_log == null) return;
        _log.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {message}");
        _log.Flush();
    }

    private void LoadSounds()
    {
        try
        {
            var on = Assets.Resolve("capson.wav");
            if (on != null) { _onSound = new SoundPlayer(on); _onSound.Load(); }
        }
        catch { }
        try
        {
            var off = Assets.Resolve("capsoff.wav");
            if (off != null) { _offSound = new SoundPlayer(off); _offSound.Load(); }
        }
        catch { }
    }

    private void BuildTray()
    {
        _trayIcon = TrayIcon.Create();
        _menu = new ContextMenuStrip();
        _menu.Font = FontService.FontOf(10f);

        _openItem = new ToolStripMenuItem { };
        _openItem.Click += (_, _) => TogglePanel();
        _menu.Items.Add(_openItem);
        _menu.Items.Add(new ToolStripSeparator());
        _quitItem = new ToolStripMenuItem { };
        _quitItem.Click += (_, _) => Quit();
        _menu.Items.Add(_quitItem);

        _menu.Opening += (_, _) => UpdateMenuTexts();

        _tray = new NotifyIcon
        {
            Icon = _trayIcon,
            Text = "IndiCaps 1.0.0 · by Jovatis",
            Visible = true,
            ContextMenuStrip = _menu
        };
        _tray.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left) TogglePanel();
        };

        UpdateMenuTexts();
    }

    private void UpdateMenuTexts()
    {
        var lang = _settings.Language;
        if (_openItem != null) _openItem.Text = Lang.Get(lang, "menu_open");
        if (_quitItem != null) _quitItem.Text = Lang.Get(lang, "menu_quit");
    }

    private void TogglePanel()
    {
        if (_panel == null || _panel.IsDisposed)
        {
            _panel = new PanelForm(_settings, ApplySettings, Quit);
            _panel.LanguageChanged += RebuildPanel;
        }

        if (_panel.Visible)
        {
            _panel.Hide();
        }
        else
        {
            _panel.ApplyLanguage();
            _panel.PositionNearTray();
            _panel.Show();
            _panel.Activate();
        }
    }

    private void RebuildPanel()
    {
        var fresh = new PanelForm(_settings, ApplySettings, Quit);
        fresh.LanguageChanged += RebuildPanel;
        fresh.ApplyLanguage();
        fresh.PositionNearTray();

        var old = _panel;
        _panel = fresh;
        old?.Hide();
        old?.Dispose();

        fresh.Show();
        fresh.Activate();
    }

    private void ApplySettings()
    {
        ApplyStartupRegistry(_settings.StartWithWindows);
        _settings.Save();
        UpdateMenuTexts();
    }

    private static void ApplyStartupRegistry(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null) return;
            if (enable)
                key.SetValue("IndiCaps", $"\"{Application.ExecutablePath}\"");
            else
                if (key.GetValue("IndiCaps") != null)
                    key.DeleteValue("IndiCaps", false);
        }
        catch { }
    }

    private static void Quit() => Application.Exit();

    public void Dispose()
    {
        if (_hookId != IntPtr.Zero)
        {
            try { NativeMethods.UnhookWindowsHookEx(_hookId); } catch { }
            _hookId = IntPtr.Zero;
        }
        if (_tray != null) { _tray.Visible = false; _tray.Dispose(); }
        _menu?.Dispose();
        _notifier?.Dispose();
        _panel?.Dispose();
        _trayIcon?.Dispose();
        _onSound?.Dispose();
        _offSound?.Dispose();
        _log?.Dispose();
    }
}