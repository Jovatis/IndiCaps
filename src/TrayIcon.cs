using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace IndiCaps;

internal static class TrayIcon
{
    private const int Size = 32;

    public static Icon Create()
    {
        var path = Assets.Resolve("indicaps logo.png");
        if (path != null)
        {
            try
            {
                using var source = new Bitmap(path);
                using var scaled = new Bitmap(Size, Size, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(scaled))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.Clear(Color.Transparent);
                    g.DrawImage(source, 0, 0, Size, Size);
                }

                IntPtr hIcon = scaled.GetHicon();
                try
                {
                    using var temp = Icon.FromHandle(hIcon);
                    return (Icon)temp.Clone();
                }
                finally
                {
                    NativeMethods.DestroyIcon(hIcon);
                }
            }
            catch
            {
            }
        }

        return CreateFallback();
    }

    /// <summary>
    /// Simple fallback icon used only when the logo file is missing.
    /// </summary>
    private static Icon CreateFallback()
    {
        using var bmp = new Bitmap(Size, Size);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            var r = new Rectangle(2, 2, Size - 4, Size - 4);
            using (var bg = new SolidBrush(Color.FromArgb(22, 22, 28)))
                g.FillPath(bg, Ui.RoundedRect(r, 8));
            using (var pen = new Pen(Theme.Accent, 2f))
                g.DrawPath(pen, Ui.RoundedRect(r, 8));

            using var f = new Font("Segoe UI", 14f, FontStyle.Bold, GraphicsUnit.Pixel);
            using var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString("I", f, Brushes.White, r, sf);
        }

        IntPtr hIcon = bmp.GetHicon();
        try
        {
            using var temp = Icon.FromHandle(hIcon);
            return (Icon)temp.Clone();
        }
        finally
        {
            NativeMethods.DestroyIcon(hIcon);
        }
    }
}