using Microsoft.Maui.Storage;
using RunicLab.Models;

namespace RunicLab.Views;

/// <summary>Persistente Einstellungen der Häufigkeitsanalyse (Al-Kindi).</summary>
internal static class AlKindiPreferences
{
    private const string PrefSprache = "AlKindiSprache";
    private const string PrefTopAnzahl = "AlKindiTopAnzahl";
    private const string PrefZeigeErwartung = "AlKindiZeigeErwartung";

    public const int TopMin = 3;
    public const int TopMax = 10;

    public static FrequencyLanguage LoadSprache() =>
        (FrequencyLanguage)Preferences.Get(PrefSprache, (int)FrequencyLanguage.Deutsch);

    public static void SaveSprache(FrequencyLanguage sprache) => Preferences.Set(PrefSprache, (int)sprache);

    /// <summary>Wie viele der häufigsten Buchstaben angezeigt werden.</summary>
    public static int LoadTopAnzahl() => Math.Clamp(Preferences.Get(PrefTopAnzahl, 5), TopMin, TopMax);

    public static void SaveTopAnzahl(int anzahl) => Preferences.Set(PrefTopAnzahl, Math.Clamp(anzahl, TopMin, TopMax));

    /// <summary>Erwartungswerte der Sprache neben den Messwerten anzeigen.</summary>
    public static bool LoadZeigeErwartung() => Preferences.Get(PrefZeigeErwartung, true);

    public static void SaveZeigeErwartung(bool zeigen) => Preferences.Set(PrefZeigeErwartung, zeigen);
}
