namespace RunicLab.Models;

/// <summary>
/// Ein Kandidat der Vergleichsanalyse: welches Verfahren, mit welcher Einstellung,
/// welcher Klartext und wie gut er zur erwarteten Buchstabenverteilung passt.
/// </summary>
/// <param name="Verfahren">Name des Verfahrens, z. B. „Cäsar“.</param>
/// <param name="Details">Einstellung des Verfahrens, z. B. „Verschiebung 3“.</param>
/// <param name="Klartext">So liest sich der Geheimtext mit diesem Verfahren.</param>
/// <param name="Guetewert">Chi-Quadrat-Abstand zur erwarteten Verteilung – kleiner ist besser.</param>
public sealed record Verfahrensbefund(string Verfahren, string Details, string Klartext, double Guetewert);

/// <summary>
/// Vergleichsanalyse: probiert alle Verfahren der App blind durch und ordnet sie
/// danach, wie gut der jeweils entstehende Text zur Buchstabenverteilung der
/// gewählten Sprache passt (Chi-Quadrat, kleiner ist besser).
///
/// Verglichen werden: unveränderter Text, alle 26 Cäsar-Verschiebungen und – wenn
/// die Eingabe wie Koordinatenpaare aussieht – das Polybius-Quadrat (mit dem
/// Standardgitter und, falls gesetzt, mit dem konfigurierten Schlüsselwort).
/// Reine Logik ohne MAUI-Abhängigkeiten, damit sie testbar bleibt.
/// </summary>
public static class CipherVergleich
{
    public const string Unverschluesselt = "Unverschlüsselt";
    public const string Caeser = "Cäsar";
    public const string Polybius = "Polybius-Quadrat";

    /// <summary>
    /// Alle Verfahren durchprobieren, bestes Ergebnis zuerst.
    /// Leere Liste, wenn der Text zu kurz für eine Aussage ist.
    /// </summary>
    public static IReadOnlyList<Verfahrensbefund> Analysiere(
        string? text,
        FrequencyLanguage language = FrequencyLanguage.Deutsch,
        PolybiusOptions? polybius = null)
    {
        var befunde = new List<Verfahrensbefund>();

        if (string.IsNullOrWhiteSpace(text))
        {
            return befunde;
        }

        // 1. Vielleicht ist gar nichts verschlüsselt.
        Add(befunde, Unverschluesselt, "Text unverändert gelesen", text, language);

        // 2. Cäsar: die Chi-Quadrat-Schätzung liefert die beste Verschiebung.
        //    Verschiebung 0 bedeutet „nichts verändert“ – dafür steht schon der
        //    Kandidat oben, ein zweiter mit identischem Text wäre nur Rauschen.
        var guess = FrequencyAnalysis.GuessCaesarShift(text, language);
        if (guess is not null && guess.Shift != 0)
        {
            Add(befunde, Caeser, $"Verschiebung {guess.Shift}", CaeserCipher.Decrypt(text, guess.Shift), language);
        }

        // 3. Polybius: nur wenn die Eingabe aus Koordinatenpaaren besteht.
        if (SiehtNachKoordinaten(text))
        {
            var standard = new PolybiusOptions();
            Add(befunde, Polybius, "Standardgitter",
                PolybiusCipher.Decrypt(text, standard), language);

            string? keyword = polybius?.Keyword;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var gesetzt = new PolybiusOptions
                {
                    Keyword = keyword,
                    Separator = polybius!.Separator,
                    TransliterateUmlauts = polybius.TransliterateUmlauts
                };

                Add(befunde, Polybius, $"Gitter mit Schlüsselwort „{keyword}“",
                    PolybiusCipher.Decrypt(text, gesetzt), language);
            }
        }

        return befunde
            .OrderBy(b => b.Guetewert)
            .ThenBy(b => b.Verfahren)
            .ToList();
    }

    /// <summary>Bestes Ergebnis oder <c>null</c>, wenn nichts bewertbar war.</summary>
    public static Verfahrensbefund? Bestes(
        string? text,
        FrequencyLanguage language = FrequencyLanguage.Deutsch,
        PolybiusOptions? polybius = null) =>
        Analysiere(text, language, polybius).FirstOrDefault();

    /// <summary>
    /// Sieht der Text nach Polybius-Koordinaten aus? Mindestens so viele Ziffern
    /// wie für <see cref="FrequencyAnalysis.MindestBuchstaben"/> Buchstaben nötig,
    /// gerade Anzahl und jedes Paar zwischen 1 und 5.
    /// </summary>
    public static bool SiehtNachKoordinaten(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var ziffern = new List<char>();
        foreach (char c in text)
        {
            if (char.IsDigit(c))
            {
                ziffern.Add(c);
            }
            else if (char.IsLetter(c))
            {
                // Buchstaben im Geheimtext sprechen gegen Koordinatenpaare.
                return false;
            }
        }

        if (ziffern.Count < FrequencyAnalysis.MindestBuchstaben * 2 || ziffern.Count % 2 != 0)
        {
            return false;
        }

        for (int i = 0; i < ziffern.Count; i += 2)
        {
            if (ziffern[i] < '1' || ziffern[i] > '5' || ziffern[i + 1] < '1' || ziffern[i + 1] > '5')
            {
                return false;
            }
        }

        return true;
    }

    private static void Add(
        List<Verfahrensbefund> befunde,
        string verfahren,
        string details,
        string? klartext,
        FrequencyLanguage language)
    {
        if (string.IsNullOrEmpty(klartext))
        {
            return;
        }

        // Zu kurze Texte liefert ChiQuadrat als null – die taugen für keinen Vergleich.
        if (FrequencyAnalysis.ChiQuadrat(klartext, language) is not { } guete)
        {
            return;
        }

        befunde.Add(new Verfahrensbefund(verfahren, details, klartext, guete));
    }
}
