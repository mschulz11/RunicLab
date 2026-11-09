using RunicLab.Models;
using Microsoft.Maui.Storage;

namespace RunicLab.Views
{
    public partial class CodeCaeserVerfahren : ContentPage
    {
        private const string PrefVerschiebung = "CaeserVerschiebung";
        private const string PrefModus = "CaeserModus";

        private AppSettings _settings = new AppSettings();

        public CodeCaeserVerfahren()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshModus();
        }

        /// <summary>
        /// Übernimmt Richtung + Verschiebung aus den Einstellungen und beschriftet
        /// den Aktions-Button passend.
        /// </summary>
        private void RefreshModus()
        {
            var modus = (CaeserMode)Preferences.Get(PrefModus, (int)CaeserMode.Verschlüsseln);
            int verschiebung = Preferences.Get(PrefVerschiebung, 3);

            bool entschluesseln = modus == CaeserMode.Entschlüsseln;

            btnAktion.Text = entschluesseln ? "Entschlüsseln" : "Verschlüsseln";
            lbModusAnzeige.Text = entschluesseln
                ? $"Modus: Entschlüsseln (Verschiebung {verschiebung}) – änderbar in den Einstellungen"
                : $"Modus: Verschlüsseln (Verschiebung {verschiebung}) – änderbar in den Einstellungen";
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsCaeser(_settings));
        }

        private void OnAktionClicked(object? sender, EventArgs e)
        {
            var modus = (CaeserMode)Preferences.Get(PrefModus, (int)CaeserMode.Verschlüsseln);
            int verschiebung = Preferences.Get(PrefVerschiebung, 3);

            zielText.Text = CaeserCipher.Shift(quellText.Text, verschiebung, modus);
        }
    }
}
