using System.IO;
using System.Text.Json;

namespace IndiCaps;

public sealed class Settings
{
    public bool IndicatorEnabled { get; set; } = true;
    public string Language { get; set; } = "fr";
    public bool SoundEnabled { get; set; } = true;
    public bool AnimationEnabled { get; set; } = true;
    public bool StartWithWindows { get; set; }

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    private static string ConfigDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IndiCaps");

    private static string ConfigPath => Path.Combine(ConfigDir, "config.json");

    public static Settings Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var s = JsonSerializer.Deserialize<Settings>(File.ReadAllText(ConfigPath));
                if (s != null)
                {
                    if (s.Language != "fr" && s.Language != "en") s.Language = "fr";
                    return s;
                }
            }
        }
        catch { }
        return new Settings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(ConfigDir);
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this, JsonOpts));
        }
        catch { }
    }
}