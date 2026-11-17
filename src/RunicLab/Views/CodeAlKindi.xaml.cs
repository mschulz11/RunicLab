using RunicLab.Models;

namespace RunicLab.Views;

/// <summary>
/// Häufigkeitsanalyse nach Al-Kindi. Der Aktions-Button kommt aus
/// <see cref="CipherPageBase"/>; die Auswertung ist seitenspezifisch.
/// </summary>
public partial class CodeAlKindi : CipherPageBase
{
    public CodeAlKindi()
    {
        InitializeComponent();
        EingabeVerwenden(CipherEingabeControl);

        Eingabe.AktionText = "Analysieren";
        // Die Auswertung besteht aus zwei eigenen Karten statt eines Ausgabetexts.
        Eingabe.ErgebnisRahmenSichtbar = false;
    }

    protected override void EinstellungenAnwenden()
    {
        // Nach einer Einstellungsänderung rechnet die Basis die Auswertung gleich
        // neu; vorher nur der Hinweis für den leeren Zustand.
        if (WurdeBerechnet)
        {
            return;
        }

        var sprache = AlKindiPreferences.LoadSprache();
        lbZusammenfassung.Text = $"Noch keine Analyse · Sprache: {Sprachname(sprache)}";
    }

    protected override void AktionAusfuehren() => Analysiere(Eingabe.EingabeText);

    protected override string EinstellungsMerkmale() =>
        $"{AlKindiPreferences.LoadSprache()}|{AlKindiPreferences.LoadZeigeErwartung()}|{AlKindiPreferences.LoadTopAnzahl()}";

    protected override Page EinstellungsSeiteErzeugen() => new SettingsAlKindi();

    private void Analysiere(string? text)
    {
        var language = AlKindiPreferences.LoadSprache();
        var befunde = CipherVergleich.Analysiere(text, language, PolybiusPreferences.Load(), EnigmaPreferences.Load());

        var frequencies = FrequencyAnalysis.Analyze(text);
        int total = frequencies.Sum(f => f.Count);

        // Zahlenpaare (Polybius) enthalten keine Buchstaben. Dann zählt die
        // Häufigkeit des entschlüsselten Textes – Grundlage ist der beste
        // Kandidat der Vergleichsanalyse (z. B. „15“ → E).
        bool ausKoordinaten = total == 0 && befunde.Count > 0;
        if (ausKoordinaten)
        {
            frequencies = FrequencyAnalysis.Analyze(befunde[0].Klartext);
            total = frequencies.Sum(f => f.Count);
        }

        lbZusammenfassung.Text = total == 0
            ? "Keine Buchstaben gefunden – bitte einen Text eingeben."
            : ausKoordinaten
                ? $"{total} Buchstaben gezählt · entschlüsselt als {befunde[0].Verfahren} ({befunde[0].Details}) · Sprache: {Sprachname(language)}"
                : $"{total} Buchstaben gezählt · Sprache: {Sprachname(language)}";

        ZeigeGitter(text, ausKoordinaten ? befunde[0] : null);
        BuildResultGrid(frequencies, total, language);
        UpdateVergleich(befunde);
    }

    /// <summary>
    /// Gitter zeigen, mit dem ein Koordinatentext entschlüsselt wurde – samt der
    /// häufigsten Paare („15 = E“), damit die Häufigkeitsanalyse nachvollziehbar ist.
    /// </summary>
    private void ZeigeGitter(string? text, Verfahrensbefund? befund)
    {
        if (befund?.Gitter is not { } gitter)
        {
            GitterKarte.IsVisible = false;
            return;
        }

        GitterKarte.IsVisible = true;
        PolybiusGridBuilder.Fill(GitterRaster, gitter);

        var paare = PolybiusCipher.HaeufigstePaare(text, gitter, 3);
        lbGitterHinweis.Text = paare.Count == 0
            ? "Zeile/Spalte im Gitter."
            : "So wurde entschlüsselt (Zeile/Spalte): "
              + string.Join(", ", paare.Select(p => $"{p.Paar} = {p.Buchstabe} ({p.Anzahl}×)"));
    }

    /// <summary>
    /// Vergleichsanalyse (#21): alle Verfahren der App durchprobieren und die
    /// besten Kandidaten mit ihrer Bewertung anzeigen.
    /// </summary>
    private void UpdateVergleich(IReadOnlyList<Verfahrensbefund> befunde)
    {
        VergleichListe.Children.Clear();

        if (befunde.Count == 0)
        {
            lbVergleichHinweis.Text = $"Für einen Vergleich braucht es mindestens {FrequencyAnalysis.MindestBuchstaben} Buchstaben.";
            lbVergleichHinweis.IsVisible = true;
            return;
        }

        lbVergleichHinweis.IsVisible = false;

        var top = befunde.Take(3).ToList();
        for (int i = 0; i < top.Count; i++)
        {
            var befund = top[i];

            VergleichListe.Add(new Label
            {
                Text = $"Platz {i + 1}: {befund.Verfahren} – {befund.Details} (Chi² {befund.Guetewert:F1})",
                FontSize = 14,
                FontAttributes = i == 0 ? FontAttributes.Bold : FontAttributes.None
            });

            VergleichListe.Add(new Label
            {
                Text = befund.Klartext,
                FontSize = i == 0 ? 16 : 14
            });
        }
    }

    private void BuildResultGrid(IReadOnlyList<LetterFrequency> frequencies, int total, FrequencyLanguage language)
    {
        ResultGrid.Children.Clear();
        ResultGrid.RowDefinitions.Clear();
        ResultGrid.ColumnDefinitions.Clear();

        if (total == 0)
        {
            return;
        }

        bool zeigeErwartung = AlKindiPreferences.LoadZeigeErwartung();
        int top = AlKindiPreferences.LoadTopAnzahl();
        double[] expected = FrequencyAnalysis.ErwarteteVerteilung(language);

        ResultGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));  // Buchstabe
        ResultGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));  // Balken
        ResultGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));  // Prozent
        if (zeigeErwartung)
        {
            ResultGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // Erwartung
        }

        var topList = frequencies
            .Where(f => f.Count > 0)
            .OrderByDescending(f => f.Count)
            .ThenBy(f => f.Letter)
            .Take(top)
            .ToList();

        double maxPercent = topList.Count == 0 ? 1 : topList.Max(f => f.Percent);

        int row = 0;
        foreach (var entry in topList)
        {
            ResultGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            ResultGrid.Add(new Label
            {
                Text = entry.Letter.ToString(),
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center
            }, 0, row);

            ResultGrid.Add(new ProgressBar
            {
                Progress = Math.Clamp(entry.Percent / maxPercent, 0, 1),
                VerticalOptions = LayoutOptions.Center
            }, 1, row);

            ResultGrid.Add(new Label
            {
                Text = $"{entry.Percent:F1} % ({entry.Count})",
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.End
            }, 2, row);

            if (zeigeErwartung)
            {
                ResultGrid.Add(new Label
                {
                    Text = $"erwartet {expected[entry.Letter - 'A'] * 100:F1} %",
                    FontSize = 12,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                }, 3, row);
            }

            row++;
        }
    }

    private static string Sprachname(FrequencyLanguage language) =>
        language == FrequencyLanguage.Englisch ? "Englisch" : "Deutsch";
}
