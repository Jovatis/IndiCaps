namespace IndiCaps;

internal static class Lang
{
    private static readonly Dictionary<string, (string en, string fr)> T = new()
    {
        ["app_name"] = ("IndiCaps", "IndiCaps"),
        ["caps_on"] = ("Caps ON", "Maj ACTIVÉ"),
        ["caps_off"] = ("Caps OFF", "Maj DÉSACTIVÉ"),
        ["title"] = ("Settings", "Réglages"),
        ["subtitle"] = ("Caps Lock indicator", "Indicateur de verrouillage Maj"),
        ["lang_label"] = ("Language", "Langue"),
        ["opt_indicator"] = ("Caps indicator", "Indicateur Caps"),
        ["opt_sound"] = ("Sound", "Son"),
        ["opt_animation"] = ("Fade animation", "Animation"),
        ["opt_startup"] = ("Launch at Windows startup", "Lancer au démarrage"),
        ["quit"] = ("Quit IndiCaps", "Quitter IndiCaps"),
        ["menu_open"] = ("Open settings", "Ouvrir les réglages"),
        ["menu_quit"] = ("Quit", "Quitter"),
    };

    public static string Get(string language, string key)
    {
        if (T.TryGetValue(key, out var pair))
            return language == "fr" ? pair.fr : pair.en;
        return key;
    }
}