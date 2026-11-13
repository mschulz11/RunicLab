namespace RunicLab.Views;

/// <summary>
/// Basis aller Verschlüsselungs- und Analyse-Seiten. Kümmert sich um das
/// Einstellungen-ToolbarItem, das Verbinden des gemeinsamen Eingabeblocks
/// (<see cref="CipherEingabe"/>) und das Aktualisieren nach Rückkehr von der
/// Einstellungsseite.
///
/// Abgeleitete Seiten implementieren nur noch:
/// <see cref="EinstellungenAnwenden"/>, <see cref="AktionAusfuehren"/> und
/// <see cref="EinstellungsSeiteErzeugen"/>.
/// </summary>
public abstract class CipherPageBase : ContentPage
{
    private CipherEingabe? _eingabe;

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

    /// <summary>
    /// Verbindet den Block aus dem XAML der Seite und hängt die Aktionslogik ein.
    /// Muss im Konstruktor nach <c>InitializeComponent()</c> aufgerufen werden.
    /// </summary>
    protected void EingabeVerwenden(CipherEingabe eingabe)
    {
        _eingabe = eingabe;
        _eingabe.AktionAngefordert += (_, _) => AktionAusfuehren();
    }

    /// <summary>Aktualisiert Beschriftungen und Statuszeile aus den Einstellungen.</summary>
    protected abstract void EinstellungenAnwenden();

    /// <summary>Verarbeitet die Eingabe und schreibt das Ergebnis in den Block.</summary>
    protected abstract void AktionAusfuehren();

    /// <summary>Erzeugt die Einstellungsseite dieses Verfahrens.</summary>
    protected abstract Page EinstellungsSeiteErzeugen();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        EinstellungenAnwenden();
    }

    private async void OnEinstellungenClicked(object? sender, EventArgs e)
        => await Navigation.PushAsync(EinstellungsSeiteErzeugen());
}
