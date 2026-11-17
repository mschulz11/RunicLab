using RunicLab.Models;

namespace RunicLab.Views;

/// <summary>
/// Enigma-Seite. Eingabe, Aktions-Button und Ausgabe kommen aus
/// <see cref="CipherPageBase"/>; die Seite liefert den Schlüsselhinweis und den
/// Walzenlauf als eigene Seite.
/// </summary>
public partial class CodeEnigma : CipherPageBase
{
    /// <summary>Letzter Lauf; <c>null</c>, solange nichts gerechnet wurde.</summary>
    private EnigmaLauf? _letzterLauf;

    public CodeEnigma()
    {
        InitializeComponent();
        EingabeVerwenden(CipherEingabeControl);
    }

    protected override void EinstellungenAnwenden()
    {
        var options = EnigmaPreferences.Load();
        bool entschluesseln = EnigmaPreferences.LoadEntschluesseln();

        Eingabe.AktionText = entschluesseln ? "Entschlüsseln" : "Verschlüsseln";

        string stecker = string.IsNullOrWhiteSpace(options.Steckerbrett)
            ? "ohne Steckerbrett"
            : $"Stecker: {SteckerAnzahl(options.Steckerbrett)} Paare";

        Eingabe.ModusText =
            $"Modus: {(entschluesseln ? "Entschlüsseln" : "Verschlüsseln")} · "
            + $"Walzen {EnigmaCipher.WalzenlageText(options.Walzenlage)} · UKW {options.Umkehrwalze} · "
            + $"Ring {EnigmaCipher.StellungenText(options.Ringstellung)} · "
            + $"Grundstellung {EnigmaCipher.Anzeige(options.Grundstellung)} · {stecker}";
    }

    protected override void AktionAusfuehren()
    {
        var options = EnigmaPreferences.Load();
        var mode = EnigmaPreferences.LoadEntschluesseln()
            ? EnigmaMode.Entschlüsseln
            : EnigmaMode.Verschlüsseln;

        _letzterLauf = EnigmaCipher.FuehreAus(Eingabe.EingabeText, options, mode);
        Eingabe.AusgabeText = _letzterLauf.Text;

        bool hatSchritte = _letzterLauf.Schritte.Count > 0;
        btnWalzenlauf.IsEnabled = hatSchritte;

        lbHinweis.IsVisible = !hatSchritte;
        lbHinweis.Text = hatSchritte
            ? string.Empty
            : "Kein Buchstabe erkannt – die Enigma kennt nur A–Z.";
    }

    protected override string EinstellungsMerkmale()
    {
        var options = EnigmaPreferences.Load();

        return $"{string.Join(",", options.Walzenlage)}|{options.Umkehrwalze}|"
               + $"{string.Join(",", options.Ringstellung)}|{string.Join(",", options.Grundstellung)}|"
               + $"{options.Steckerbrett}|{options.Ausgabe}|{options.TransliterateUmlauts}|"
               + $"{EnigmaPreferences.LoadEntschluesseln()}";
    }

    protected override Page EinstellungsSeiteErzeugen() => new SettingsEnigma();

    private async void OnWalzenlaufClicked(object? sender, EventArgs e)
    {
        if (_letzterLauf is null || _letzterLauf.Schritte.Count == 0)
        {
            return;
        }

        var options = EnigmaPreferences.Load();
        var mode = EnigmaPreferences.LoadEntschluesseln()
            ? EnigmaMode.Entschlüsseln
            : EnigmaMode.Verschlüsseln;

        await Navigation.PushAsync(new EnigmaWalzenlauf(options, mode, _letzterLauf.Schritte));
    }

    private static int SteckerAnzahl(string stecker) =>
        stecker.Split([' ', '\t', ',', ';', '-'], StringSplitOptions.RemoveEmptyEntries).Length;
}
