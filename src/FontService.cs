namespace IndiCaps;

internal static class FontService
{
    private const uint FR_PRIVATE = 0x10;

    private static string _familyName = "Segoe UI";
    private static bool _initialized;

    public static string? LoadedFamilyName { get; private set; }

    public static void Init()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            var path = Assets.Resolve("Poppins-Regular.ttf");
            if (path != null && NativeMethods.AddFontResourceEx(path, FR_PRIVATE, IntPtr.Zero) > 0)
            {
                _familyName = "Poppins";
                LoadedFamilyName = _familyName;
            }
        }
        catch { }
    }

    public static Font FontOf(float size, FontStyle style = FontStyle.Regular)
    {
        try
        {
            return new Font(_familyName, size, style, GraphicsUnit.Point, gdiCharSet: 0, gdiVerticalFont: false);
        }
        catch
        {
            return new Font("Segoe UI", size, style, GraphicsUnit.Point);
        }
    }
}