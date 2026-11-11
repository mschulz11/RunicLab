namespace RunicLab.Models;

/// <summary>Sprache, gegen deren Buchstabenverteilung analysiert wird.</summary>
public enum FrequencyLanguage
{
    Deutsch = 0,
    Englisch = 1
}

/// <summary>Buchstabe mit absoluter und relativer Häufigkeit im untersuchten Text.</summary>
public sealed record LetterFrequency(char Letter, int Count, double Percent);

/// <summary>Ergebnis der Cäsar-Schätzung: vermutete Verschiebung und Chi-Quadrat-Abstand.</summary>
public sealed record ShiftGuess(int Shift, double Score);

/// <summary>
/// Häufigkeitsanalyse im Sinne Al-Kindis (9. Jahrhundert): Buchstaben in einem
/// Text werden gezählt und mit der typischen Verteilung einer Sprache verglichen.
/// Daraus lässt sich eine Cäsar-Verschiebung bestimmen (Chi-Quadrat-Anpassung).
/// Reine Logik ohne MAUI-Abhängigkeiten, damit sie testbar bleibt.
/// </summary>
public static class FrequencyAnalysis
{
    /// <summary>Unterhalb dieser Buchstabenzahl ist die Analyse zu unzuverlässig.</summary>
    public const int MindestBuchstaben = 20;

    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // Typische Verteilung in Prozent (gerundet). Wird beim Rechnen auf 100 % normiert.
    private static readonly double[] Deutsch =
    {
        6.47, 1.74, 2.84, 4.73, 17.48, 1.66, 3.01, 4.13, 7.74, 0.26, 1.21, 3.49, 2.54,
        9.73, 2.72, 0.85, 0.02, 7.14, 7.21, 6.12, 4.18, 0.69, 1.48, 0.03, 0.03, 1.13
    };

    private static readonly double[] Englisch =
    {
        8.17, 1.49, 2.78, 4.25, 12.70, 2.23, 2.02, 6.09, 6.97, 0.15, 0.77, 4.03, 2.41,
        6.75, 7.51, 1.93, 0.10, 5.99, 6.33, 9.06, 2.76, 0.98, 2.36, 0.15, 1.97, 0.07
    };

    /// <summary>Erwartete Verteilung als Anteile, die zusammen 1 ergeben (Index 0 = A).</summary>
    public static double[] ErwarteteVerteilung(FrequencyLanguage language)
    {
        double[] source = language == FrequencyLanguage.Englisch ? Englisch : Deutsch;
        double sum = source.Sum();
        var result = new double[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            result[i] = source[i] / sum;
        }

        return result;
    }

    /// <summary>Zählt A–Z (Groß-/Kleinschreibung egal) und liefert die Häufigkeiten in Alphabet-Reihenfolge.</summary>
    public static IReadOnlyList<LetterFrequency> Analyze(string? text)
    {
        int[] counts = CountLetters(text, out int total);
        var result = new List<LetterFrequency>(counts.Length);

        for (int i = 0; i < counts.Length; i++)
        {
            double percent = total == 0 ? 0 : counts[i] * 100.0 / total;
            result.Add(new LetterFrequency((char)('A' + i), counts[i], percent));
        }

        return result;
    }

    /// <summary>
    /// Schätzt die Cäsar-Verschiebung durch Chi-Quadrat-Vergleich mit der
    /// erwarteten Buchstabenverteilung. <c>null</c>, wenn der Text zu kurz ist.
    /// </summary>
    public static ShiftGuess? GuessCaesarShift(string? text, FrequencyLanguage language = FrequencyLanguage.Deutsch)
    {
        int[] counts = CountLetters(text, out int total);
        if (total < MindestBuchstaben)
        {
            return null;
        }

        double[] expected = ErwarteteVerteilung(language);
        ShiftGuess? best = null;

        for (int shift = 0; shift < 26; shift++)
        {
            double chi2 = 0;
            for (int plain = 0; plain < 26; plain++)
            {
                // Buchstabe plain im Klartext war im Geheimtext um `shift` verschoben.
                int observedIndex = (plain + shift) % 26;
                double expectedCount = expected[plain] * total;
                if (expectedCount <= 0)
                {
                    continue;
                }

                double diff = counts[observedIndex] - expectedCount;
                chi2 += diff * diff / expectedCount;
            }

            if (best is null || chi2 < best.Score)
            {
                best = new ShiftGuess(shift, chi2);
            }
        }

        return best;
    }

    /// <summary>
    /// Entschlüsselt einen Cäsar-Text mit der geschätzten Verschiebung.
    /// <c>null</c>, wenn der Text zu kurz für eine Schätzung ist.
    /// </summary>
    public static string? BreakCaesar(string? text, FrequencyLanguage language = FrequencyLanguage.Deutsch)
    {
        var guess = GuessCaesarShift(text, language);
        return guess is null ? null : CaeserCipher.Decrypt(text, guess.Shift);
    }

    /// <summary>Buchstabenzähler (Index 0 = A) plus Gesamtzahl der Buchstaben.</summary>
    public static int[] CountLetters(string? text, out int total)
    {
        var counts = new int[26];
        total = 0;

        if (string.IsNullOrEmpty(text))
        {
            return counts;
        }

        foreach (char c in text)
        {
            char upper = char.ToUpperInvariant(c);
            if (upper >= 'A' && upper <= 'Z')
            {
                counts[upper - 'A']++;
                total++;
            }
        }

        return counts;
    }
}
