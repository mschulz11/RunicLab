using Microsoft.Maui.Storage;
using RunicLab.Models;

namespace RunicLab.Views;

/// <summary>Persistente Einstellungen des Polybius-Quadrats (Plattform-Preferences).</summary>
internal static class PolybiusPreferences
{
    private const string PrefKeyword = "PolybiusKeyword";
    private const string PrefSeparator = "PolybiusSeparator";
    private const string PrefUmlaute = "PolybiusUmlaute";
    private const string PrefEntschluesseln = "PolybiusEntschluesseln";

    public static PolybiusOptions Load() => new()
    {
        Keyword = Preferences.Get(PrefKeyword, string.Empty),
        Separator = (PolybiusSeparator)Preferences.Get(PrefSeparator, (int)PolybiusSeparator.Leerzeichen),
        TransliterateUmlauts = Preferences.Get(PrefUmlaute, true)
    };

    public static void Save(PolybiusOptions options)
    {
        Preferences.Set(PrefKeyword, options.Keyword ?? string.Empty);
        Preferences.Set(PrefSeparator, (int)options.Separator);
        Preferences.Set(PrefUmlaute, options.TransliterateUmlauts);
    }

    /// <summary>Richtung der Chiffre: true = entschlüsseln.</summary>
    public static bool LoadEntschluesseln() => Preferences.Get(PrefEntschluesseln, false);

    public static void SaveEntschluesseln(bool entschluesseln) => Preferences.Set(PrefEntschluesseln, entschluesseln);
}
