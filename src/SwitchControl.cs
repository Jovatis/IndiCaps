using System.Drawing.Drawing2D;

namespace IndiCaps;

public sealed class SwitchControl : Control
{
    private bool _on;
    private float _knobPos;
    private System.Windows.Forms.Timer? _anim;

    public event EventHandler? Toggled;

    public bool On => _on;

    public SwitchControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        Size = new Size(46, 26);
        Cursor = Cursors.Hand;
    }

    public void SetOn(bool value, bool animate = true)
    {
        if (_on == value)
        {
            if (!animate)
            {
                _knobPos = value ? 1f : 0f;
                Invalidate();
            }
            return;
        }

        _on = value;
        if (animate) StartAnim();
        else
        {
            _knobPos = value ? 1f : 0f;
            Invalidate();
        }
        Toggled?.Invoke(this, EventArgs.Empty);
    }

    private void StartAnim()
    {
        _anim?.Stop();
        _anim?.Dispose();
        _anim = new System.Windows.Forms.Timer { Interval = 12 };
        _anim.Tick += (_, _) =>
        {
            float target = _on ? 1f : 0f;
            _knobPos += (target - _knobPos) * 0.35f;
            if (Math.Abs(target - _knobPos) < 0.01f)
            {
                _knobPos = target;
                _anim!.Stop();
                _anim.Dispose();
                _anim = null;
            }
            Invalidate();
        };
        _anim.Start();
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button == MouseButtons.Left) SetOn(!_on);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var track = new Rectangle(0, 1, Width - 1, Height - 2);
        using (var b = new SolidBrush(_on ? Theme.Accent : Theme.TrackOff))
            g.FillPath(b, Ui.RoundedRect(track, track.Height / 2));

        int knob = Height - 6;
        float x = 3 + _knobPos * (Width - 6 - knob);
        using var b2 = new SolidBrush(Color.White);
        g.FillEllipse(b2, x, 3, knob, knob);
    }
}