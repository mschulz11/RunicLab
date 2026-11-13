using RunicLab.Models;

namespace RunicLab.Views;

/// <summary>
/// Häufigkeitsanalyse nach Al-Kindi. Der Aktions-Button kommt aus
/// <see cref="CipherPageBase"/>; die Auswertung ist seitenspezifisch.
/// </summary>
public partial class CodeAlKindi : CipherPageBase
{
    private string? _letzteEingabe;

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
        // Nach einem Besuch der Einstellungen die Auswertung mit den neuen
        // Werten erneut aufbauen, statt sie zu verwerfen.
        if (_letzteEingabe is not null)
        {
            Analysiere(_letzteEingabe);
            return;
        }

        var sprache = AlKindiPreferences.LoadSprache();
        lbZusammenfassung.Text = $"Noch keine Analyse · Sprache: {Sprachname(sprache)}";
    }

    protected override void AktionAusfuehren()
    {
        _letzteEingabe = Eingabe.EingabeText;
        Analysiere(_letzteEingabe);
    }

    protected override Page EinstellungsSeiteErzeugen() => new SettingsAlKindi();

    private void Analysiere(string? text)
    {
        var language = AlKindiPreferences.LoadSprache();
        var frequencies = FrequencyAnalysis.Analyze(text);
        int total = frequencies.Sum(f => f.Count);

        lbZusammenfassung.Text = total == 0
            ? "Keine Buchstaben gefunden – bitte einen Text eingeben."
            : $"{total} Buchstaben gezählt · Sprache: {Sprachname(language)}";

        BuildResultGrid(frequencies, total, language);
        UpdateSchaetzung(text, language);
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

    private void UpdateSchaetzung(string? text, FrequencyLanguage language)
    {
        var guess = FrequencyAnalysis.GuessCaesarShift(text, language);

        if (guess is null)
        {
            lbSchaetzung.Text = $"Zu wenig Buchstaben für eine Schätzung (mindestens {FrequencyAnalysis.MindestBuchstaben} nötig).";
            lbEntschluesselt.Text = string.Empty;
            return;
        }

        lbSchaetzung.Text = guess.Shift == 0
            ? $"Vermutete Verschiebung: 0 – der Text ist wohl schon im Klartext (Chi-Quadrat {guess.Score:F1})."
            : $"Vermutete Verschiebung: {guess.Shift} (Chi-Quadrat {guess.Score:F1} – kleiner ist besser).";

        lbEntschluesselt.Text = CaeserCipher.Decrypt(text, guess.Shift);
    }

    private static string Sprachname(FrequencyLanguage language) =>
        language == FrequencyLanguage.Englisch ? "Englisch" : "Deutsch";
}
