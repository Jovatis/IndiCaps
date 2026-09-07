namespace IndiCaps;

internal static class Assets
{
    /// <summary>
    /// Resolves a runtime asset, searching in order:
    /// 1. "../assets/" relative to the exe (project assets folder when the exe lives in dist/)
    /// 2. "assets/" next to the exe (deployed with a copied assets folder)
    /// 3. flat next to the exe (legacy layout)
    /// Returns null when the file is not found anywhere.
    /// </summary>
    public static string? Resolve(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, "..", "assets", fileName),
            Path.Combine(baseDir, "assets", fileName),
            Path.Combine(baseDir, fileName),
        };

        foreach (var candidate in candidates)
        {
            var full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(candidate));
            if (File.Exists(full)) return full;
        }

        return null;
    }
}