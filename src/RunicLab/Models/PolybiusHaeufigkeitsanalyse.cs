namespace RunicLab.Models;

/// <summary>
/// Ein Feld des 5×5-Rasters: seine Koordinate, wie oft das Paar im Geheimtext
/// vorkam, sein Anteil daran und – falls belegt – der Buchstabe, den die
/// Häufigkeitsanalyse dort vermutet.
/// </summary>
/// <param name="Zeile">Zeile im Raster, 1-basiert.</param>
/// <param name="Spalte">Spalte im Raster, 1-basiert.</param>
/// <param name="Anzahl">Vorkommen des Paares im Geheimtext.</param>
/// <param name="Anteil">Anteil an allen gültigen Paaren, 0 bis 1.</param>
/// <param name="Buchstabe">Vermuteter Buchstabe, <c>null</c> wenn unsicher.</param>
public sealed record PaarHaeufigkeit(int Zeile, int Spalte, int Anzahl, double Anteil, char? Buchstabe)
{
    /// <summary>Das Paar als Text, z. B. „15“.</summary>
    public string Paar => $"{Zeile}{Spalte}";

    /// <summary>Wurde dem Feld ein Buchstabe zugeordnet?</summary>
    public bool Belegt => Buchstabe is not null;
}

/// <summary>
/// Ergebnis der Häufigkeitsanalyse über Polybius-Koordinaten: ein vollständiges
/// Raster mit den Häufigkeiten und – soweit belegbar – den vermuteten Buchstaben,
/// dazu der damit entschlüsselte Text.
/// </summary>
/// <param name="Felder">Alle 25 Felder in Lesereihenfolge (zeilenweise).</param>
/// <param name="Klartext">Entschlüsselter Text; offene Felder stehen als „?“.</param>
/// <param name="Sprache">Sprache, gegen deren Verteilung zugeordnet wurde.</param>
public sealed record PolybiusHypothese(
    IReadOnlyList<PaarHaeufigkeit> Felder,
    string Klartext,
    FrequencyLanguage Sprache)
{
    /// <summary>Anzahl der Felder, denen ein Buchstabe zugeordnet wurde.</summary>
    public int BelegteFelder => Felder.Count(f => f.Belegt);

    /// <summary>Wie viele Buchstaben der Klartext enthält (ohne „?“).</summary>
    public int Buchstaben => Klartext.Count(c => c != PolybiusCipher.UngueltigesPaar);

    /// <summary>Anzahl der im Geheimtext vorgekommenen Paare insgesamt.</summary>
    public int GezaehltePaare => Felder.Sum(f => f.Anzahl);

    /// <summary>Feld an einer 1-basierten Koordinate, sonst <c>null</c>.</summary>
    public PaarHaeufigkeit? Feld(int zeile, int spalte) =>
        Felder.FirstOrDefault(f => f.Zeile == zeile && f.Spalte == spalte);

    /// <summary>Die häufigsten vorgekommenen Paare, absteigend nach Anzahl.</summary>
    public IReadOnlyList<PaarHaeufigkeit> Haeufigste(int max) =>
        Felder
            .Where(f => f.Anzahl > 0)
            .OrderByDescending(f => f.Anzahl)
            .ThenBy(f => f.Zeile)
            .ThenBy(f => f.Spalte)
            .Take(max)
            .ToList();

    /// <summary>Buchstabe zu einem Paar, <c>null</c> wenn das Feld offen ist.</summary>
    public char? BuchstabeZu(int zeile, int spalte) => Feld(zeile, spalte)?.Buchstabe;
}

/// <summary>
/// Häufigkeitsanalyse über Polybius-Koordinaten: Statt das Gitter aus den
/// Einstellungen zu übernehmen, wird es aus dem Geheimtext selbst erschlossen.
/// Die Koordinatenpaare werden gezählt, nach Häufigkeit sortiert und der Reihe
/// nach den häufigsten Buchstaben der Sprache zugeordnet (E, N, I, S, R …).
/// Was zu selten vorkommt, bleibt offen und wird mit „?“ markiert.
///
/// Die Zuordnung ist eine Hypothese: häufige Buchstaben trifft sie meist,
/// seltene fast nie. Reine Logik ohne MAUI-Abhängigkeiten, damit sie testbar bleibt.
/// </summary>
public static class PolybiusHaeufigkeitsanalyse
{
    /// <summary>Unterhalb dieser Zahl gültiger Paare ist die Analyse zu unzuverlässig.</summary>
    public const int MindestPaare = FrequencyAnalysis.MindestBuchstaben;

    /// <summary>Ein Paar muss mindestens so oft vorkommen, um zugeordnet zu werden.</summary>
    public const int MindestVorkommen = 3;

    /// <summary>Ein Paar muss mindestens diesen Anteil am Text haben, um zugeordnet zu werden.</summary>
    public const double MindestAnteil = 0.01;

    /// <summary>
    /// Analysiert einen Koordinatentext und liefert die Gitter-Hypothese samt
    /// Klartext. <c>null</c>, wenn der Text keine gültigen Koordinatenpaare in
    /// ausreichender Zahl enthält.
    /// </summary>
    public static PolybiusHypothese? Analysiere(
        string? cipher,
        FrequencyLanguage language = FrequencyLanguage.Deutsch)
    {
        var zaehler = ZaehlePaare(cipher, out int gesamt);
        if (gesamt < MindestPaare)
        {
            return null;
        }

        // Paare nach beobachteter Häufigkeit, bei Gleichstand nach Koordinate –
        // damit dasselbe Ergebnis immer dieselbe Zuordnung bekommt.
        var rang = zaehler
            .OrderByDescending(e => e.Value)
            .ThenBy(e => e.Key.Zeile)
            .ThenBy(e => e.Key.Spalte)
            .Select(e => e.Key)
            .ToList();

        var buchstaben = Buchstabenrang(language);
        var zuordnung = new Dictionary<(int Zeile, int Spalte), char>();
        var felder = new List<PaarHaeufigkeit>(PolybiusCipher.GridSize * PolybiusCipher.GridSize);

        for (int zeile = 1; zeile <= PolybiusCipher.GridSize; zeile++)
        {
            for (int spalte = 1; spalte <= PolybiusCipher.GridSize; spalte++)
            {
                var paar = (zeile, spalte);
                int anzahl = zaehler.TryGetValue(paar, out int n) ? n : 0;
                double anteil = (double)anzahl / gesamt;

                char? buchstabe = null;
                if (anzahl >= MindestVorkommen && anteil >= MindestAnteil)
                {
                    int stelle = rang.IndexOf(paar);
                    if (stelle >= 0 && stelle < buchstaben.Count)
                    {
                        buchstabe = buchstaben[stelle];
                        zuordnung[paar] = buchstabe.Value;
                    }
                }

                felder.Add(new PaarHaeufigkeit(zeile, spalte, anzahl, anteil, buchstabe));
            }
        }

        return new PolybiusHypothese(felder, Entschluesseln(cipher, zuordnung), language);
    }

    /// <summary>
    /// Entschlüsselt Koordinaten mit einer festen Zuordnung Paar → Buchstabe.
    /// Unbekannte Paare werden zu <see cref="PolybiusCipher.UngueltigesPaar"/>.
    /// </summary>
    public static string Entschluesseln(
        string? cipher,
        IReadOnlyDictionary<(int Zeile, int Spalte), char> zuordnung)
    {
        if (string.IsNullOrEmpty(cipher))
        {
            return string.Empty;
        }

        var text = new System.Text.StringBuilder(cipher.Length / 2);
        int zeile = 0;
        bool ersteZiffer = true;

        foreach (char c in cipher)
        {
            if (!char.IsDigit(c))
            {
                continue;
            }

            int ziffer = c - '0';
            if (ersteZiffer)
            {
                zeile = ziffer;
                ersteZiffer = false;
                continue;
            }

            ersteZiffer = true;
            text.Append(zuordnung.TryGetValue((zeile, ziffer), out char buchstabe)
                ? buchstabe
                : PolybiusCipher.UngueltigesPaar);
        }

        return text.ToString();
    }

    /// <summary>Zählt gültige Koordinatenpaare (Zeile und Spalte je 1–5).</summary>
    public static Dictionary<(int Zeile, int Spalte), int> ZaehlePaare(string? cipher, out int gesamt)
    {
        var zaehler = new Dictionary<(int Zeile, int Spalte), int>();
        gesamt = 0;

        if (string.IsNullOrEmpty(cipher))
        {
            return zaehler;
        }

        int zeile = 0;
        bool ersteZiffer = true;

        foreach (char c in cipher)
        {
            if (!char.IsDigit(c))
            {
                continue;
            }

            int ziffer = c - '0';
            if (ersteZiffer)
            {
                zeile = ziffer;
                ersteZiffer = false;
                continue;
            }

            ersteZiffer = true;

            // Nur Paare, die im 5×5-Raster existieren; alles andere (z. B. „07“,
            // „69“) gehört zu keinem Feld und würde die Anteile nur verzerren.
            if (zeile < 1 || zeile > PolybiusCipher.GridSize || ziffer < 1 || ziffer > PolybiusCipher.GridSize)
            {
                continue;
            }

            var paar = (zeile, ziffer);
            zaehler[paar] = zaehler.TryGetValue(paar, out int bisher) ? bisher + 1 : 1;
            gesamt++;
        }

        return zaehler;
    }

    /// <summary>
    /// Die 25 Zeichen des Polybius-Gitters, sortiert nach ihrer erwarteten
    /// Häufigkeit in der Sprache – Platz 1 ist der häufigste Buchstabe.
    /// </summary>
    /// <remarks>
    /// Grundlage ist <see cref="PolybiusCipher.Alphabet"/>: dort teilen sich I und J
    /// ein Feld, das Gitter hat also 25 statt 26 Zeichen. Deshalb wird der Anteil von
    /// J dem Anteil von I zugeschlagen, bevor sortiert wird – sonst würde J einen
    /// Rang belegen, den im Gitter kein Feld hat, und alle folgenden Buchstaben
    /// rutschten eine Stelle nach hinten. Diese Umrechnung passiert nur hier.
    /// </remarks>
    public static IReadOnlyList<char> Buchstabenrang(FrequencyLanguage language)
    {
        double[] anteile = FrequencyAnalysis.ErwarteteVerteilung(language);
        int jIndex = 'J' - 'A';

        var rangliste = new List<(char Buchstabe, double Anteil)>(PolybiusCipher.GridSize * PolybiusCipher.GridSize);
        foreach (char c in PolybiusCipher.Alphabet)
        {
            double anteil = anteile[c - 'A'];
            if (c == 'I')
            {
                anteil += anteile[jIndex];
            }

            rangliste.Add((c, anteil));
        }

        return rangliste
            .OrderByDescending(e => e.Anteil)
            // Bei Gleichstand alphabetisch, damit die Reihenfolge reproduzierbar ist.
            .ThenBy(e => e.Buchstabe)
            .Select(e => e.Buchstabe)
            .ToList();
    }
}
