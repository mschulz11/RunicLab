namespace RunicLab.Views;

/// <summary>
/// Basis aller Verschlüsselungs- und Analyse-Seiten. Kümmert sich um das
/// Einstellungen-ToolbarItem, das Verbinden des gemeinsamen Eingabeblocks
/// (<see cref="CipherEingabe"/>) und das erneute Ausführen nach einer
/// Einstellungsänderung.
///
/// Abgeleitete Seiten implementieren nur noch:
/// <see cref="EinstellungenAnwenden"/>, <see cref="AktionAusfuehren"/> und
/// <see cref="EinstellungsSeiteErzeugen"/> – und, wenn das Ergebnis von
/// Einstellungen abhängt, <see cref="EinstellungsMerkmale"/>.
/// </summary>
public abstract class CipherPageBase : ContentPage
{
    private CipherEingabe? _eingabe;

    /// <summary>Eingabe des letzten Laufs; <c>null</c> = es wurde noch nichts gerechnet.</summary>
    private string? _letzteEingabe;

    /// <summary>Fingerabdruck der Einstellungen beim letzten Lauf.</summary>
    private string? _letzteMerkmale;

    protected CipherPageBase()
    {
        var einstellungen = new ToolbarItem
        {
            Text = "Einstellungen",
            IconImageSource = "playbutton1.png",
            Order = ToolbarItemOrder.Primary,
            Priority = 0
        };
        einstellungen.Clicked += OnEinstellungenClicked;
        ToolbarItems.Add(einstellungen);
    }

    /// <summary>Der gemeinsame Eingabe-/Aktions-/Ausgabe-Block der Seite.</summary>
    protected CipherEingabe Eingabe =>
        _eingabe ?? throw new InvalidOperationException(
            "CipherEingabe nicht verbunden: im Konstruktor der Seite EingabeVerwenden(...) aufrufen.");

    /// <summary><c>true</c>, sobald die Seite mindestens einmal gerechnet hat.</summary>
    protected bool WurdeBerechnet => _letzteEingabe is not null;

    /// <summary>
    /// Verbindet den Block aus dem XAML der Seite und hängt die Aktionslogik ein.
    /// Muss im Konstruktor nach <c>InitializeComponent()</c> aufgerufen werden.
    /// </summary>
    protected void EingabeVerwenden(CipherEingabe eingabe)
    {
        _eingabe = eingabe;
        _eingabe.AktionAngefordert += (_, _) => Ausfuehren();
    }

    /// <summary>Aktualisiert Beschriftungen und Statuszeile aus den Einstellungen.</summary>
    protected abstract void EinstellungenAnwenden();

    /// <summary>Verarbeitet die Eingabe und schreibt das Ergebnis in den Block.</summary>
    protected abstract void AktionAusfuehren();

    /// <summary>Erzeugt die Einstellungsseite dieses Verfahrens.</summary>
    protected abstract Page EinstellungsSeiteErzeugen();

    /// <summary>
    /// Fingerabdruck aller Einstellungen, die das Ergebnis beeinflussen – etwa
    /// <c>$"{Verschiebung}|{Modus}"</c>. Unterscheidet er sich beim Erscheinen der
    /// Seite vom letzten Lauf, rechnet die Basis automatisch neu; gleicher Wert
    /// heißt: nichts tun. Seiten ohne Einstellungen lassen die Vorgabe stehen.
    /// </summary>
    protected virtual string EinstellungsMerkmale() => string.Empty;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        EinstellungenAnwenden();

        if (!WurdeBerechnet || _letzteMerkmale == EinstellungsMerkmale())
        {
            return;
        }

        // In den Einstellungen wurde etwas geändert: mit den neuen Werten noch
        // einmal rechnen, ohne dass der Aktions-Button gedrückt werden muss.
        // Maßgeblich ist der aktuelle Feldinhalt; ein zwischenzeitlich geleertes
        // Feld bekommt die zuletzt gerechnete Eingabe zurück, damit Eingabe und
        // Ergebnis nicht auseinanderlaufen.
        if (string.IsNullOrEmpty(Eingabe.EingabeText))
        {
            Eingabe.EingabeText = _letzteEingabe!;
        }

        Ausfuehren();
    }

    private void Ausfuehren()
    {
        _letzteEingabe = Eingabe.EingabeText;
        _letzteMerkmale = EinstellungsMerkmale();
        AktionAusfuehren();
    }

    private async void OnEinstellungenClicked(object? sender, EventArgs e)
        => await Navigation.PushAsync(EinstellungsSeiteErzeugen());
}
