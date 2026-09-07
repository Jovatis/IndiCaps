using System.Drawing.Drawing2D;

namespace IndiCaps;

public sealed class PanelForm : Form
{
    private readonly Settings _settings;
    private readonly Action _apply;
    private readonly Action _quit;

    private const int W = 330;
    private const int H = 412;
    private static readonly Rectangle CloseRect = new(W - 46, 24, 28, 28);

    private readonly GLabel _subtitle;
    private readonly GLabel _langLabel;
    private readonly RoundedButton _enBtn;
    private readonly RoundedButton _frBtn;
    private readonly RoundedButton _quitBtn;
    private readonly List<(string key, GLabel label, SwitchControl sw)> _rows = new();

    public event Action? LanguageChanged;

    public PanelForm(Settings settings, Action apply, Action quit)
    {
        _settings = settings;
        _apply = apply;
        _quit = quit;

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        KeyPreview = true;
        BackColor = Color.Black;
        ClientSize = new Size(W, H);
        Region = new Region(Ui.RoundedRect(new Rectangle(0, 0, W, H), 18));
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint, true);

        _subtitle = new GLabel("", 10f, Theme.TextSecondary, new Point(24, 54), new Size(240, 20));
        _langLabel = new GLabel("", 11.5f, Theme.TextPrimary, new Point(24, 92), new Size(120, 22));

        _enBtn = new RoundedButton("EN") { Location = new Point(24, 118) };
        _frBtn = new RoundedButton("FR") { Location = new Point(94, 118) };
        _enBtn.Click += (_, _) => SetLanguage("en");
        _frBtn.Click += (_, _) => SetLanguage("fr");

        _quitBtn = new RoundedButton("") { Location = new Point(24, 358), Size = new Size(W - 48, 40), Danger = true };
        _quitBtn.Click += (_, _) => _quit();

        Controls.AddRange(new Control[] { _subtitle, _langLabel, _enBtn, _frBtn, _quitBtn });

        AddRow("opt_indicator", () => _settings.IndicatorEnabled, v => _settings.IndicatorEnabled = v);
        AddRow("opt_sound", () => _settings.SoundEnabled, v => _settings.SoundEnabled = v);
        AddRow("opt_animation", () => _settings.AnimationEnabled, v => _settings.AnimationEnabled = v);
        AddRow("opt_startup", () => _settings.StartWithWindows, v => _settings.StartWithWindows = v);

        ApplyLanguage();
        Shown += (_, _) => ApplyLanguage();
    }

    private void AddRow(string key, Func<bool> getter, Action<bool> setter)
    {
        int y = 184 + _rows.Count * 42;

        var label = new GLabel("", 11.5f, Theme.TextPrimary, new Point(24, y + 5), new Size(224, 24));
        var sw = new SwitchControl { Location = new Point(W - 70, y + 8) };
        sw.SetOn(getter(), animate: false);
        sw.Toggled += (_, _) =>
        {
            setter(sw.On);
            _apply();
        };

        Controls.Add(label);
        Controls.Add(sw);
        _rows.Add((key, label, sw));
    }

    public void PositionNearTray()
    {
        var wa = Screen.PrimaryScreen!.WorkingArea;
        Location = new Point(wa.Right - Width - 8, wa.Bottom - Height - 8);
    }

    public void ApplyLanguage()
    {
        var lang = _settings.Language;
        _subtitle.Text = Lang.Get(lang, "title");
        _langLabel.Text = Lang.Get(lang, "lang_label");
        _enBtn.Selected = lang == "en";
        _frBtn.Selected = lang == "fr";
        _quitBtn.Text = Lang.Get(lang, "quit");

        foreach (var (key, label, _) in _rows)
            label.Text = Lang.Get(lang, key);

        Refresh();
    }

    private void SetLanguage(string lang)
    {
        _settings.Language = lang;
        _apply();
        ApplyLanguage();
        LanguageChanged?.Invoke();
    }

    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);
        Hide();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Escape) Hide();
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button == MouseButtons.Left && CloseRect.Contains(e.Location))
            Hide();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        using (var pen = new Pen(Theme.Border, 1.5f))
            g.DrawPath(pen, Ui.RoundedRect(new Rectangle(1, 1, Width - 3, Height - 3), 17));
        using (var line = new Pen(Theme.Border))
            g.DrawLine(line, 24, 171, W - 24, 171);

        using var titleFont = FontService.FontOf(15.5f);
        TextRenderer.DrawText(g, "IndiCaps", titleFont, new Rectangle(24, 24, 220, 30),
            Theme.TextPrimary, TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

        // Close icon: two crossing strokes
        using (var xPen = new Pen(Theme.TextSecondary, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
        {
            int cx = CloseRect.X + 14, cy = CloseRect.Y + 14;
            g.DrawLine(xPen, cx - 5, cy - 5, cx + 5, cy + 5);
            g.DrawLine(xPen, cx + 5, cy - 5, cx - 5, cy + 5);
        }
    }
}