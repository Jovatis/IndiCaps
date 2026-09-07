using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace IndiCaps;

public sealed class CapsNotifierForm : Form
{
    private const float SourceW = 683f;
    private const float SourceH = 226f;

    private Bitmap? _onImage;
    private Bitmap? _offImage;
    private Bitmap? _surface;

    private string _text = "";
    private bool _capsOn;
    private bool _animate = true;
    private int _phase;
    private int _count;
    private int _dispW;
    private int _dispH;
    private System.Windows.Forms.Timer? _anim;

    public CapsNotifierForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        BackColor = Color.Black;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);

        LoadImages();
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= 0x00000080 | 0x08000000 | 0x00080000 | 0x00000020; // TOOLWINDOW | NOACTIVATE | LAYERED | TRANSPARENT
            return cp;
        }
    }

    private void LoadImages()
    {
        try
        {
            var on = Assets.Resolve("bgon.png");
            var off = Assets.Resolve("bgoff.png");
            if (on != null) _onImage = new Bitmap(on);
            if (off != null) _offImage = new Bitmap(off);
        }
        catch { }
    }

    public void Display(string text, bool capsOn, bool animate)
    {
        _text = text;
        _capsOn = capsOn;
        _animate = animate;

        float dpi;
        using (var g = Graphics.FromHwnd(IntPtr.Zero))
            dpi = g.DpiX;

        _dispW = (int)Math.Min(SourceW, Math.Round(210 * dpi / 96f));
        _dispH = (int)Math.Round(_dispW * SourceH / SourceW);
        Size = new Size(_dispW, _dispH);

        var wa = Screen.PrimaryScreen!.WorkingArea;
        Location = new Point(wa.Right - _dispW - 18, wa.Bottom - _dispH - 14);

        RenderSurface();

        _phase = 0;
        _count = 0;

        _anim?.Stop();
        _anim?.Dispose();
        _anim = new System.Windows.Forms.Timer { Interval = 12 };
        _anim.Tick += (_, _) => Tick();
        Show();

        // Force topmost, no activation, no size/move change (pure overlay, click-through).
        NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_TOPMOST,
            Location.X, Location.Y, _dispW, _dispH,
            NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE |
            NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);

        UpdateSurfaceAlpha(_animate ? 0 : 255);
        _anim.Start();
    }

    private void RenderSurface()
    {
        _surface?.Dispose();
        _surface = new Bitmap(_dispW, _dispH, PixelFormat.Format32bppArgb);

        using var g = Graphics.FromImage(_surface);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.Clear(Color.Transparent);

        var bg = _capsOn ? _onImage : _offImage;
        if (bg != null)
            g.DrawImage(bg, 0, 0, _dispW, _dispH);
        else
            g.FillRectangle(new SolidBrush(Color.FromArgb(20, 20, 20)), 0, 0, _dispW, _dispH);

        DrawCenteredText(g, _dispW, _dispH);

        Premultiply(_surface);
    }

    private void DrawCenteredText(Graphics g, int w, int h)
    {
        float fontSize = (float)Math.Round(w * 0.075);
        using var font = FontService.FontOf(fontSize);
        using var sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        var rect = new RectangleF(0, 0, w, h);

        // Soft shadow for legibility on any background.
        using (var shadow = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
        {
            rect.Offset(0f, Math.Max(1f, fontSize * 0.06f));
            g.DrawString(_text, font, shadow, rect, sf);
            rect.Offset(0f, -Math.Max(1f, fontSize * 0.06f));
        }

        using var white = new SolidBrush(Color.White);
        g.DrawString(_text, font, white, rect, sf);
    }

    private static void Premultiply(Bitmap bmp)
    {
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        int bytes = Math.Abs(data.Stride) * bmp.Height;
        var buffer = new byte[bytes];
        Marshal.Copy(data.Scan0, buffer, 0, bytes);
        int stride = Math.Abs(data.Stride);

        for (int y = 0; y < bmp.Height; y++)
        {
            int row = y * stride;
            for (int x = 0; x < bmp.Width; x++)
            {
                int i = row + x * 4;
                byte a = buffer[i + 3];
                if (a == 0)
                {
                    buffer[i] = 0;
                    buffer[i + 1] = 0;
                    buffer[i + 2] = 0;
                }
                else if (a < 255)
                {
                    buffer[i] = (byte)(buffer[i] * a / 255);
                    buffer[i + 1] = (byte)(buffer[i + 1] * a / 255);
                    buffer[i + 2] = (byte)(buffer[i + 2] * a / 255);
                }
            }
        }

        Marshal.Copy(buffer, 0, data.Scan0, bytes);
        bmp.UnlockBits(data);
    }

    private void UpdateSurfaceAlpha(int alpha)
    {
        if (_surface == null || !IsHandleCreated) return;

        var hdcScreen = NativeMethods.GetDC(IntPtr.Zero);
        var hdcMem = NativeMethods.CreateCompatibleDC(hdcScreen);
        var hbmp = _surface.GetHbitmap(Color.FromArgb(0));
        var old = NativeMethods.SelectObject(hdcMem, hbmp);

        var pos = new NativeMethods.POINT { X = Location.X, Y = Location.Y };
        var size = new NativeMethods.SIZE { CX = _dispW, CY = _dispH };
        var src = new NativeMethods.POINT();
        var blend = new NativeMethods.BLENDFUNCTION
        {
            BlendOp = 0x00,
            BlendFlags = 0x00,
            SourceConstantAlpha = (byte)alpha,
            AlphaFormat = 0x01 // AC_SRC_ALPHA
        };

        NativeMethods.UpdateLayeredWindow(Handle, hdcScreen, ref pos, ref size, hdcMem, ref src, 0, ref blend, 0x00000002);

        NativeMethods.SelectObject(hdcMem, old);
        NativeMethods.DeleteObject(hbmp);
        NativeMethods.DeleteDC(hdcMem);
        NativeMethods.ReleaseDC(IntPtr.Zero, hdcScreen);
    }

    private void Tick()
    {
        switch (_phase)
        {
            case 0: // fade in
                if (_animate)
                {
                    UpdateSurfaceAlpha((int)Math.Round(255 * (float)(_count + 1) / 12.0));
                    if (++_count >= 12) { _count = 0; _phase = 1; }
                }
                else { UpdateSurfaceAlpha(255); _count = 0; _phase = 1; }
                break;

            case 1: // hold
                if (++_count >= 95) { _count = 0; _phase = 2; }
                break;

            case 2: // fade out
                if (_animate)
                {
                    UpdateSurfaceAlpha((int)Math.Round(255 * (1.0 - (float)(_count + 1) / 18.0)));
                    if (++_count >= 18) { _count = 0; _phase = 3; }
                }
                else { _count = 0; _phase = 3; }
                break;

            default:
                Finish();
                return;
        }
    }

    private void Finish()
    {
        _anim?.Stop();
        _anim?.Dispose();
        _anim = null;
        Hide();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _anim?.Dispose();
            _surface?.Dispose();
            _onImage?.Dispose();
            _offImage?.Dispose();
        }
        base.Dispose(disposing);
    }
}