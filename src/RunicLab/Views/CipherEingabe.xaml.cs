namespace RunicLab.Views;

/// <summary>
/// Eingabe, Aktions-Button und Ergebnisbereich für alle Verschlüsselungsseiten.
/// Die Seite liefert nur die Texte und – über <see cref="AktionAngefordert"/> –
/// das Verhalten; Ausgabe und Layout sind hier einmal definiert.
/// </summary>
public partial class CipherEingabe : ContentView
{
    private string _aktionText = "Verschlüsseln";
    private string _modusText = string.Empty;

    public CipherEingabe()
    {
        InitializeComponent();
    }

    /// <summary>Wird ausgelöst, wenn der Aktions-Button gedrückt wird.</summary>
    public event EventHandler? AktionAngefordert;

    /// <summary>Beschriftung des Aktions-Buttons, z. B. „Verschlüsseln“ oder „Analysieren“.</summary>
    public string AktionText
    {
        get => _aktionText;
        set
        {
            _aktionText = value;
            AktionButton.Text = value;
        }
    }

    /// <summary>Statuszeile über dem Button (Modus, Schlüssel, Einstellungen).</summary>
    public string ModusText
    {
        get => _modusText;
        set
        {
            _modusText = value;
            ModusLabel.Text = value;
            ModusLabel.IsVisible = !string.IsNullOrWhiteSpace(value);
        }
    }

    public string Platzhalter
    {
        get => EingabeEditor.Placeholder ?? string.Empty;
        set => EingabeEditor.Placeholder = value;
    }

    /// <summary>Text im Eingabefeld.</summary>
    public string EingabeText
    {
        get => EingabeEditor.Text ?? string.Empty;
        set => EingabeEditor.Text = value;
    }

    /// <summary>Text im Standard-Ergebnisbereich. Nur sinnvoll, solange kein eigener
    /// Ergebnisinhalt gesetzt ist (siehe <see cref="ErgebnisInhalt"/>).</summary>
    public string AusgabeText
    {
        get => AusgabeLabel.Text ?? string.Empty;
        set => AusgabeLabel.Text = value;
    }

    /// <summary>Ersetzt den Standard-Ausgabetext durch eigenen Inhalt (z. B. ein Gitter).</summary>
    public View? ErgebnisInhalt
    {
        get => ErgebnisSlot.Content;
        set => ErgebnisSlot.Content = value;
    }

    /// <summary>Rahmen um den Ergebnisbereich ein-/ausblenden.</summary>
    public bool ErgebnisRahmenSichtbar
    {
        get => ErgebnisRahmen.IsVisible;
        set => ErgebnisRahmen.IsVisible = value;
    }

    private void OnAktionClicked(object? sender, EventArgs e)
        => AktionAngefordert?.Invoke(this, EventArgs.Empty);
}
