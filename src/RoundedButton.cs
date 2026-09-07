using System.Drawing.Drawing2D;

namespace IndiCaps;

public sealed class RoundedButton : Control
{
    private bool _selected;
    private bool _hover;
    private bool _danger;

    public bool Selected
    {
        get => _selected;
        set
        {
            if (_selected != value)
            {
                _selected = value;
                Invalidate();
            }
        }
    }

    public bool Danger
    {
        get => _danger;
        set
        {
            _danger = value;
            Invalidate();
        }
    }

    public RoundedButton(string text)
    {
        Text = text;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        Size = new Size(64, 30);
        Cursor = Cursors.Hand;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hover = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        var bg = _danger
            ? (_hover ? Theme.ButtonHover : Theme.TrackOff)
            : (_selected ? Theme.Accent : (_hover ? Theme.ButtonHover : Theme.TrackOff));

        using (var b = new SolidBrush(bg))
            g.FillPath(b, Ui.RoundedRect(rect, Math.Min(10, Height / 2)));

        var textColor = _selected && !_danger ? Color.FromArgb(10, 10, 12) : Theme.TextPrimary;
        using var font = FontService.FontOf(10.5f);
        TextRenderer.DrawText(g, Text, font, rect, textColor, bg,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
    }
}