using RunicLab.Models;

namespace RunicLab.Views;

/// <summary>
/// Einstellungen der Enigma: Walzenlage, Umkehrwalze, Ring- und Grundstellung,
/// Steckerbrett, Ausgabeformat und Richtung. Ungültige Eingaben werden gemeldet,
/// aber nicht gespeichert – die zuletzt gültige Einstellung bleibt aktiv.
/// </summary>
public partial class SettingsEnigma : ContentPage
{
    private static readonly string[] AusgabeNamen =
    [
        "Fünfergruppen (historisch)",
        "Wortgrenzen erhalten"
    ];

    private readonly EnigmaOptions _options;
    private bool _syncing;

    public SettingsEnigma()
    {
        InitializeComponent();

        pickerWalzeLinks.ItemsSource = EnigmaCipher.WalzenNamen.ToList();
        pickerWalzeMitte.ItemsSource = EnigmaCipher.WalzenNamen.ToList();
        pickerWalzeRechts.ItemsSource = EnigmaCipher.WalzenNamen.ToList();
        pickerUmkehrwalze.ItemsSource = EnigmaCipher.UmkehrwalzenNamen
            .Select(c => $"UKW {c}")
            .ToList();
        pickerAusgabe.ItemsSource = AusgabeNamen;

        var ring = Enumerable.Range(1, 26).Select(n => n.ToString("00")).ToList();
        pickerRingLinks.ItemsSource = ring;
        pickerRingMitte.ItemsSource = ring.ToList();
        pickerRingRechts.ItemsSource = ring.ToList();

        var grund = Enumerable.Range(0, 26).Select(n => ((char)('A' + n)).ToString()).ToList();
        pickerGrundLinks.ItemsSource = grund;
        pickerGrundMitte.ItemsSource = grund.ToList();
        pickerGrundRechts.ItemsSource = grund.ToList();

        _options = EnigmaPreferences.Load();

        _syncing = true;
        SetzePicker();
        entryStecker.Text = _options.Steckerbrett;
        pickerAusgabe.SelectedIndex = (int)_options.Ausgabe;
        switchUmlaute.IsToggled = _options.TransliterateUmlauts;

        if (EnigmaPreferences.LoadEntschluesseln())
        {
            rbEntschluesseln.IsChecked = true;
        }
        else
        {
            rbVerschluesseln.IsChecked = true;
        }

        _syncing = false;

        AktualisiereVorschau();
    }

    private void SetzePicker()
    {
        pickerWalzeLinks.SelectedIndex = _options.Walzenlage[0] - 1;
        pickerWalzeMitte.SelectedIndex = _options.Walzenlage[1] - 1;
        pickerWalzeRechts.SelectedIndex = _options.Walzenlage[2] - 1;

        int ukw = EnigmaCipher.UmkehrwalzenNamen.ToList().IndexOf(_options.Umkehrwalze);
        pickerUmkehrwalze.SelectedIndex = ukw < 0 ? 1 : ukw;

        pickerRingLinks.SelectedIndex = _options.Ringstellung[0];
        pickerRingMitte.SelectedIndex = _options.Ringstellung[1];
        pickerRingRechts.SelectedIndex = _options.Ringstellung[2];

        pickerGrundLinks.SelectedIndex = _options.Grundstellung[0];
        pickerGrundMitte.SelectedIndex = _options.Grundstellung[1];
        pickerGrundRechts.SelectedIndex = _options.Grundstellung[2];
    }

    private void OnWalzeChanged(object sender, EventArgs e)
    {
        if (_syncing || !WalzenGewaehlt())
        {
            return;
        }

        var kandidat = _options.Kopie();
        kandidat.Walzenlage =
        [
            pickerWalzeLinks.SelectedIndex + 1,
            pickerWalzeMitte.SelectedIndex + 1,
            pickerWalzeRechts.SelectedIndex + 1
        ];

        if (EnigmaCipher.Pruefe(kandidat) is { } fehler)
        {
            ZeigeHinweis(fehler);
            _syncing = true;
            SetzePicker();
            _syncing = false;
            return;
        }

        _options.Walzenlage = kandidat.Walzenlage;
        ZeigeHinweis(null);
        Speichern();
    }

    private void OnUmkehrwalzeChanged(object sender, EventArgs e)
    {
        if (_syncing || pickerUmkehrwalze.SelectedIndex < 0)
        {
            return;
        }

        _options.Umkehrwalze = EnigmaCipher.UmkehrwalzenNamen[pickerUmkehrwalze.SelectedIndex];
        Speichern();
    }

    private void OnRingChanged(object sender, EventArgs e)
    {
        if (_syncing || !StellungenGewaehlt())
        {
            return;
        }

        _options.Ringstellung =
        [
            pickerRingLinks.SelectedIndex,
            pickerRingMitte.SelectedIndex,
            pickerRingRechts.SelectedIndex
        ];

        Speichern();
    }

    private void OnGrundChanged(object sender, EventArgs e)
    {
        if (_syncing || !StellungenGewaehlt())
        {
            return;
        }

        _options.Grundstellung =
        [
            pickerGrundLinks.SelectedIndex,
            pickerGrundMitte.SelectedIndex,
            pickerGrundRechts.SelectedIndex
        ];

        Speichern();
    }

    private void OnSteckerChanged(object sender, TextChangedEventArgs e)
    {
        if (_syncing)
        {
            return;
        }

        string text = e.NewTextValue ?? string.Empty;

        if (EnigmaCipher.PruefeSteckerbrett(text) is { } fehler)
        {
            lbSteckerHinweis.Text = fehler;
            lbSteckerHinweis.IsVisible = true;
            return;
        }

        // Gültige Eingabe nur im Speicher übernehmen: Beim Tippen entstehen
        // Zwischenstände wie „A“ oder „AB “, die noch nicht gespeichert werden
        // dürfen. Gespeichert wird beim Verlassen des Feldes oder beim Schließen.
        lbSteckerHinweis.IsVisible = false;
        _options.Steckerbrett = text;
        AktualisiereVorschau();
    }

    private void OnSteckerUnfocused(object sender, FocusEventArgs e)
    {
        if (!_syncing && EnigmaCipher.Pruefe(_options) is null)
        {
            EnigmaPreferences.Save(_options);
        }
    }

    private void OnAusgabeChanged(object sender, EventArgs e)
    {
        if (_syncing || pickerAusgabe.SelectedIndex < 0)
        {
            return;
        }

        _options.Ausgabe = (EnigmaAusgabe)pickerAusgabe.SelectedIndex;
        Speichern();
    }

    private void OnUmlauteToggled(object sender, ToggledEventArgs e)
    {
        if (_syncing)
        {
            return;
        }

        _options.TransliterateUmlauts = e.Value;
        Speichern();
    }

    private void OnModusChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_syncing || sender is not RadioButton radio || !radio.IsChecked)
        {
            return;
        }

        EnigmaPreferences.SaveEntschluesseln(ReferenceEquals(radio, rbEntschluesseln));
        AktualisiereVorschau();
    }

    private bool WalzenGewaehlt() =>
        pickerWalzeLinks.SelectedIndex >= 0
        && pickerWalzeMitte.SelectedIndex >= 0
        && pickerWalzeRechts.SelectedIndex >= 0;

    private bool StellungenGewaehlt() =>
        pickerRingLinks.SelectedIndex >= 0
        && pickerRingMitte.SelectedIndex >= 0
        && pickerRingRechts.SelectedIndex >= 0
        && pickerGrundLinks.SelectedIndex >= 0
        && pickerGrundMitte.SelectedIndex >= 0
        && pickerGrundRechts.SelectedIndex >= 0;

    private void ZeigeHinweis(string? text)
    {
        lbHinweis.Text = text ?? string.Empty;
        lbHinweis.IsVisible = !string.IsNullOrEmpty(text);
    }

    private void Speichern()
    {
        EnigmaPreferences.Save(_options);
        AktualisiereVorschau();
    }

    /// <summary>
    /// Zeigt den eingestellten Schlüssel und – als kleine Probe – das
    /// Walzenfenster nach dem ersten Buchstaben.
    /// </summary>
    private void AktualisiereVorschau()
    {
        var lauf = EnigmaCipher.FuehreAus("A", _options);
        string ersterSchritt = lauf.Schritte.Count > 0
            ? lauf.Schritte[0].Walzenlage
            : "–";

        string stecker = string.IsNullOrWhiteSpace(_options.Steckerbrett)
            ? "kein Steckerbrett"
            : $"{SteckerAnzahl(_options.Steckerbrett)} Paare";

        lbVorschau.Text =
            $"Walzen {EnigmaCipher.WalzenlageText(_options.Walzenlage)} · UKW {_options.Umkehrwalze}\n"
            + $"Ring {EnigmaCipher.StellungenText(_options.Ringstellung)} · "
            + $"Grundstellung {EnigmaCipher.Anzeige(_options.Grundstellung)}\n"
            + $"{stecker} · Ausgabe {AusgabeNamen[(int)_options.Ausgabe]}\n"
            + $"Walzenfenster nach dem ersten Buchstaben: {ersterSchritt}";
    }

    private static int SteckerAnzahl(string stecker) =>
        stecker.Split([' ', '\t', ',', ';', '-'], StringSplitOptions.RemoveEmptyEntries).Length;

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        if (EnigmaCipher.Pruefe(_options) is null)
        {
            EnigmaPreferences.Save(_options);
        }

        await Navigation.PopAsync();
    }

    /// <summary>Auch die Zurück-Geste verlässt die Seite ohne „Schließen“.</summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (EnigmaCipher.Pruefe(_options) is null)
        {
            EnigmaPreferences.Save(_options);
        }
    }
}
