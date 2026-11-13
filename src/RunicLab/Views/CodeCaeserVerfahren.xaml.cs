using Microsoft.Maui.Storage;
using RunicLab.Models;

namespace RunicLab.Views;

/// <summary>
/// Cäser-Verfahren. Die Seite liefert nur die Texte und die Chiffre-Anbindung;
/// Toolbar, Eingabe, Aktions-Button und Ausgabe kommen aus <see cref="CipherPageBase"/>.
/// </summary>
public partial class CodeCaeserVerfahren : CipherPageBase
{
    private const string PrefVerschiebung = "CaeserVerschiebung";
    private const string PrefModus = "CaeserModus";

    private readonly AppSettings _settings = new();

    public CodeCaeserVerfahren()
    {
        InitializeComponent();
        EingabeVerwenden(CipherEingabeControl);
    }

    private static CaeserMode Modus =>
        (CaeserMode)Preferences.Get(PrefModus, (int)CaeserMode.Verschlüsseln);

    private static int Verschiebung => Preferences.Get(PrefVerschiebung, 3);

    protected override void EinstellungenAnwenden()
    {
        bool entschluesseln = Modus == CaeserMode.Entschlüsseln;

        Eingabe.AktionText = entschluesseln ? "Entschlüsseln" : "Verschlüsseln";
        Eingabe.ModusText = entschluesseln
            ? $"Modus: Entschlüsseln (Verschiebung {Verschiebung}) – änderbar in den Einstellungen"
            : $"Modus: Verschlüsseln (Verschiebung {Verschiebung}) – änderbar in den Einstellungen";
    }

    protected override void AktionAusfuehren()
        => Eingabe.AusgabeText = CaeserCipher.Shift(Eingabe.EingabeText, Verschiebung, Modus);

    protected override Page EinstellungsSeiteErzeugen() => new SettingsCaeser(_settings);
}
