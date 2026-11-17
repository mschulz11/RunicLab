using Microsoft.Maui.Storage;
using RunicLab.Models;

namespace RunicLab.Views;

/// <summary>Persistente Einstellungen der Enigma (Plattform-Preferences).</summary>
internal static class EnigmaPreferences
{
    private const string PrefWalzenlage = "EnigmaWalzenlage";
    private const string PrefUmkehrwalze = "EnigmaUmkehrwalze";
    private const string PrefRingstellung = "EnigmaRingstellung";
    private const string PrefGrundstellung = "EnigmaGrundstellung";
    private const string PrefSteckerbrett = "EnigmaSteckerbrett";
    private const string PrefAusgabe = "EnigmaAusgabe";
    private const string PrefUmlaute = "EnigmaUmlaute";
    private const string PrefEntschluesseln = "EnigmaEntschluesseln";

    public static EnigmaOptions Load()
    {
        var standard = new EnigmaOptions();

        return new EnigmaOptions
        {
            Walzenlage = LiesStellungen(Preferences.Get(PrefWalzenlage, "1,2,3"), standard.Walzenlage, 1),
            Umkehrwalze = LiesUmkehrwalze(Preferences.Get(PrefUmkehrwalze, standard.Umkehrwalze.ToString())),
            Ringstellung = LiesStellungen(Preferences.Get(PrefRingstellung, "0,0,0"), standard.Ringstellung, 0),
            Grundstellung = LiesStellungen(Preferences.Get(PrefGrundstellung, "0,0,0"), standard.Grundstellung, 0),
            Steckerbrett = Preferences.Get(PrefSteckerbrett, string.Empty),
            Ausgabe = (EnigmaAusgabe)Preferences.Get(PrefAusgabe, (int)EnigmaAusgabe.Fünfergruppen),
            TransliterateUmlauts = Preferences.Get(PrefUmlaute, true)
        };
    }

    public static void Save(EnigmaOptions options)
    {
        Preferences.Set(PrefWalzenlage, SchreibeStellungen(options.Walzenlage));
        Preferences.Set(PrefUmkehrwalze, options.Umkehrwalze.ToString());
        Preferences.Set(PrefRingstellung, SchreibeStellungen(options.Ringstellung));
        Preferences.Set(PrefGrundstellung, SchreibeStellungen(options.Grundstellung));
        Preferences.Set(PrefSteckerbrett, options.Steckerbrett ?? string.Empty);
        Preferences.Set(PrefAusgabe, (int)options.Ausgabe);
        Preferences.Set(PrefUmlaute, options.TransliterateUmlauts);
    }

    /// <summary>Richtung der Chiffre: true = entschlüsseln.</summary>
    public static bool LoadEntschluesseln() => Preferences.Get(PrefEntschluesseln, false);

    public static void SaveEntschluesseln(bool entschluesseln) => Preferences.Set(PrefEntschluesseln, entschluesseln);

    private static string SchreibeStellungen(int[]? werte) =>
        string.Join(",", werte ?? []);

    /// <summary>
    /// „1,2,3“ wieder in Zahlen übersetzen. Kaputte oder unvollständige Werte
    /// fallen auf den Standard zurück, damit die Seite immer startet.
    /// </summary>
    private static int[] LiesStellungen(string? text, int[] standard, int minimum)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return (int[])standard.Clone();
        }

        var teile = text.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (teile.Length != EnigmaOptions.AnzahlWalzen)
        {
            return (int[])standard.Clone();
        }

        var werte = new int[EnigmaOptions.AnzahlWalzen];
        for (int i = 0; i < teile.Length; i++)
        {
            if (!int.TryParse(teile[i], out int wert) || wert < minimum || wert > minimum + 25)
            {
                return (int[])standard.Clone();
            }

            werte[i] = wert;
        }

        return werte;
    }

    private static char LiesUmkehrwalze(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 'B';
        }

        char c = char.ToUpperInvariant(text.Trim()[0]);
        return EnigmaCipher.UmkehrwalzenNamen.Contains(c) ? c : 'B';
    }
}
