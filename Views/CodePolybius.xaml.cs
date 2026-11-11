using Microsoft.Maui.Controls.Shapes;
using RunicLab.Models;

namespace RunicLab.Views
{
    public partial class CodePolybius : ContentPage
    {
        public CodePolybius()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var options = PolybiusPreferences.Load();
            bool entschluesseln = PolybiusPreferences.LoadEntschluesseln();

            PolybiusGridBuilder.Fill(SquareGrid, options);
            RefreshModus(options, entschluesseln);
        }

        /// <summary>Beschriftet den Aktions-Button passend zur gewählten Richtung.</summary>
        private void RefreshModus(PolybiusOptions options, bool entschluesseln)
        {
            btnAktion.Text = entschluesseln ? "Entschlüsseln" : "Verschlüsseln";

            string keyword = PolybiusCipher.BuildAlphabet(options);
            string ersteZeile = keyword[..PolybiusCipher.GridSize];

            lbModusAnzeige.Text = entschluesseln
                ? $"Modus: Entschlüsseln – Gitter beginnt mit {ersteZeile} (änderbar in den Einstellungen)"
                : $"Modus: Verschlüsseln – Gitter beginnt mit {ersteZeile} (änderbar in den Einstellungen)";
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPolybius());
        }

        private void OnAktionClicked(object? sender, EventArgs e)
        {
            var options = PolybiusPreferences.Load();

            zielText.Text = PolybiusPreferences.LoadEntschluesseln()
                ? PolybiusCipher.Decrypt(quellText.Text, options)
                : PolybiusCipher.Encrypt(quellText.Text, options);
        }
    }
}
