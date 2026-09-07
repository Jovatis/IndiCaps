namespace IndiCaps;

public sealed class GLabel : Control
{
    private readonly Color _textColor;
    private readonly float _fontSize;
    private readonly FontStyle _fontStyle;

    public GLabel(string text, float fontSize, Color textColor, Point location, Size size,
                  FontStyle style = FontStyle.Regular)
        : this(text, fontSize, textColor, style)
    {
        Location = location;
        Size = size;
    }

    public GLabel(string text, float fontSize, Color textColor, FontStyle style = FontStyle.Regular)
    {
        Text = text;
        _fontSize = fontSize;
        _textColor = textColor;
        _fontStyle = style;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Black;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var font = FontService.FontOf(_fontSize, _fontStyle);
        TextRenderer.DrawText(e.Graphics, Text, font, new Rectangle(0, 0, Width, Height),
            _textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
    }
}