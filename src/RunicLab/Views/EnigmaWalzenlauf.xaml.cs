using RunicLab.Models;

namespace RunicLab.Views;

/// <summary>
/// Zeigt den Walzenlauf eines Laufs: pro Buchstabe das Zeichen nach dem
/// Steckerbrett, das Walzenfenster nach dem Schritt und das Ergebnis.
/// Die Schritte kommen fertig aus <see cref="EnigmaCipher"/> – hier wird nichts
/// nachgerechnet.
/// </summary>
public partial class EnigmaWalzenlauf : ContentPage
{
    public EnigmaWalzenlauf(
        EnigmaOptions options,
        EnigmaMode mode,
        IReadOnlyList<EnigmaSchritt> schritte)
    {
        InitializeComponent();

        string stecker = string.IsNullOrWhiteSpace(options.Steckerbrett)
            ? "ohne Steckerbrett"
            : $"Stecker: {options.Steckerbrett}";

        lbKopf.Text =
            $"{(mode == EnigmaMode.Entschlüsseln ? "Entschlüsseln" : "Verschlüsseln")} · "
            + $"Walzen {EnigmaCipher.WalzenlageText(options.Walzenlage)} · UKW {options.Umkehrwalze} · "
            + $"Ring {EnigmaCipher.StellungenText(options.Ringstellung)} · "
            + $"Grundstellung {EnigmaCipher.Anzeige(options.Grundstellung)} · {stecker}";

        lbFuss.Text =
            $"{schritte.Count} Schritte · Walzenfenster nach dem Schritt. "
            + "Die mittlere Walze rückt bei ihrer Kerbe doppelt vor – daran erkennt man den Doppelschritt.";

        Liste.ItemsSource = schritte;
    }

    private async void OnCloseClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}
