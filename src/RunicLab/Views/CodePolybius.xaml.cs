using RunicLab.Models;

namespace RunicLab.Views;

/// <summary>
/// Polybius-Quadrat. Die Seite liefert Gitter und Texte; Eingabe, Button und
/// Ausgabe kommen aus <see cref="CipherPageBase"/>.
/// </summary>
public partial class CodePolybius : CipherPageBase
{
    public CodePolybius()
    {
        InitializeComponent();
        EingabeVerwenden(CipherEingabeControl);
    }

    protected override void EinstellungenAnwenden()
    {
        var options = PolybiusPreferences.Load();
        bool entschluesseln = PolybiusPreferences.LoadEntschluesseln();

        PolybiusGridBuilder.Fill(SquareGrid, options);

        Eingabe.AktionText = entschluesseln ? "Entschlüsseln" : "Verschlüsseln";

        string ersteZeile = PolybiusCipher.BuildAlphabet(options)[..PolybiusCipher.GridSize];
        Eingabe.ModusText = entschluesseln
            ? $"Modus: Entschlüsseln – Gitter beginnt mit {ersteZeile} (änderbar in den Einstellungen)"
            : $"Modus: Verschlüsseln – Gitter beginnt mit {ersteZeile} (änderbar in den Einstellungen)";
    }

    protected override void AktionAusfuehren()
    {
        var options = PolybiusPreferences.Load();

        Eingabe.AusgabeText = PolybiusPreferences.LoadEntschluesseln()
            ? PolybiusCipher.Decrypt(Eingabe.EingabeText, options)
            : PolybiusCipher.Encrypt(Eingabe.EingabeText, options);
    }

    protected override Page EinstellungsSeiteErzeugen() => new SettingsPolybius();
}
