using RunicLab.Models;
using Microsoft.Maui.Storage;

namespace RunicLab.Views
{
    public partial class SettingsCaeser : ContentPage
    {
        private const string PrefVerschiebung = "CaeserVerschiebung";
        private const string PrefModus = "CaeserModus";

        // Breite (dp), ab der die beiden Karten nebeneinander stehen.
        private const double WideThreshold = 700;

        private readonly AppSettings _settings;
        private bool _syncing;
        private bool _isWide;
        private bool _layoutApplied;

        public double VerschiebungMin { get; set; } = CaeserCipher.MinShift;
        public double VerschiebungMax { get; set; } = CaeserCipher.MaxShift;
        public int Verschiebung { get; set; }
        public CaeserMode Modus { get; private set; } = CaeserMode.Verschlüsseln;

        public SettingsCaeser(AppSettings settings)
        {
            InitializeComponent();

            _settings = settings;

            stepVerschiebung.Minimum = VerschiebungMin;
            stepVerschiebung.Maximum = VerschiebungMax;
            sliderVerschiebung.Minimum = VerschiebungMin;
            sliderVerschiebung.Maximum = VerschiebungMax;

            // Modus laden (Richtung der Chiffre).
            Modus = (CaeserMode)Preferences.Get(PrefModus, (int)CaeserMode.Verschlüsseln);

            _syncing = true;
            if (Modus == CaeserMode.Entschlüsseln)
            {
                rbEntschluesseln.IsChecked = true;
            }
            else
            {
                rbVerschluesseln.IsChecked = true;
            }
            _syncing = false;

            // Verschiebung aus Preferences laden.
            SetVerschiebung(Preferences.Get(PrefVerschiebung, 3));

            ApplyLayout(Width);
            _layoutApplied = true;
        }

        // ---------------------------------------------------------------- Layout

        private void OnPageSizeChanged(object sender, EventArgs e) => ApplyLayout(Width);

        /// <summary>
        /// Schaltet zwischen gestapeltem (Handy/Hochformat) und zweispaltigem
        /// (Tablet/Querformat) Layout um und passt Paddings an.
        /// </summary>
        private void ApplyLayout(double width)
        {
            bool wide = width >= WideThreshold;
            if (_layoutApplied && wide == _isWide)
            {
                return;
            }

            _isWide = wide;

            RootGrid.Padding = wide ? new Thickness(40, 24) : new Thickness(12, 8);
            var cardPadding = wide ? new Thickness(24) : new Thickness(16, 12);
            CardShift.Padding = cardPadding;
            CardExample.Padding = cardPadding;

            double fontSize = wide ? 18 : 16;
            lbVerschiebung.FontSize = fontSize;
            lbVerschiebBeispiel.FontSize = fontSize;
            lbModus.FontSize = fontSize;

            if (wide)
            {
                Grid.SetRow(CardShift, 0);
                Grid.SetColumn(CardShift, 0);
                Grid.SetColumnSpan(CardShift, 1);

                Grid.SetRow(CardExample, 0);
                Grid.SetColumn(CardExample, 1);
                Grid.SetColumnSpan(CardExample, 1);
            }
            else
            {
                Grid.SetRow(CardShift, 0);
                Grid.SetColumn(CardShift, 0);
                Grid.SetColumnSpan(CardShift, 2);

                Grid.SetRow(CardExample, 1);
                Grid.SetColumn(CardExample, 0);
                Grid.SetColumnSpan(CardExample, 2);
            }

            BuildAlphabetGrid(Verschiebung);
        }

        // ------------------------------------------------------------ Eingaben

        private void OnEntryVerschiebung(object sender, EventArgs e)
        {
            if (int.TryParse(entryVerschiebung.Text, out int value))
            {
                SetVerschiebung(value);
            }
        }

        private void OnStepVerschiebung(object sender, EventArgs e)
        {
            SetVerschiebung((int)stepVerschiebung.Value);
        }

        private void OnSliederVerschiebung(object sender, EventArgs e)
        {
            SetVerschiebung((int)sliderVerschiebung.Value);
        }

        private void OnModusChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_syncing || sender is not RadioButton radio || !radio.IsChecked)
            {
                return;
            }

            Modus = ReferenceEquals(radio, rbEntschluesseln)
                ? CaeserMode.Entschlüsseln
                : CaeserMode.Verschlüsseln;

            // Auswahl sofort sichern, damit die Cäsar-Seite sie übernimmt.
            Preferences.Set(PrefModus, (int)Modus);

            BuildAlphabetGrid(Verschiebung);
        }

        private void SetVerschiebung(int value)
        {
            if (_syncing)
            {
                return;
            }

            if (value < VerschiebungMin)
            {
                value = (int)VerschiebungMin;
            }

            if (value > VerschiebungMax)
            {
                value = (int)VerschiebungMax;
            }

            _syncing = true;
            Verschiebung = value;
            entryVerschiebung.Text = Verschiebung.ToString();
            stepVerschiebung.Value = Verschiebung;
            sliderVerschiebung.Value = Verschiebung;
            _syncing = false;

            Preferences.Set(PrefVerschiebung, Verschiebung);

            BuildAlphabetGrid(Verschiebung);
        }

        // -------------------------------------------------------------- Beispiel

        private void BuildAlphabetGrid(int verschiebung)
        {
            AlphabetGrid.Children.Clear();

            double fontSize = _isWide ? 18 : 16;

            // Kopfzeile: Quelle, Richtung, Ergebnis.
            AlphabetGrid.Add(new Label
            {
                Text = "Original",
                FontAttributes = FontAttributes.Bold,
                FontSize = fontSize,
                HorizontalOptions = LayoutOptions.Center
            }, 0, 0);

            AlphabetGrid.Add(new Label
            {
                Text = Modus == CaeserMode.Verschlüsseln ? "→" : "←",
                FontAttributes = FontAttributes.Bold,
                FontSize = fontSize,
                HorizontalOptions = LayoutOptions.Center
            }, 1, 0);

            AlphabetGrid.Add(new Label
            {
                Text = "Ergebnis",
                FontAttributes = FontAttributes.Bold,
                FontSize = fontSize,
                HorizontalOptions = LayoutOptions.Center
            }, 2, 0);

            int row = 1;

            for (char c = 'A'; c <= 'Z'; c++)
            {
                AlphabetGrid.Add(new Label
                {
                    Text = c.ToString(),
                    FontSize = fontSize,
                    HorizontalOptions = LayoutOptions.Center
                }, 0, row);

                AlphabetGrid.Add(new Label
                {
                    Text = Modus == CaeserMode.Verschlüsseln ? "→" : "←",
                    FontSize = fontSize,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center
                }, 1, row);

                AlphabetGrid.Add(new Label
                {
                    Text = CaeserCipher.Apply(c, verschiebung, Modus).ToString(),
                    FontSize = fontSize,
                    HorizontalOptions = LayoutOptions.Center
                }, 2, row);

                row++;
            }
        }

        // ------------------------------------------------------------- Schließen

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            Preferences.Set(PrefVerschiebung, Verschiebung);
            Preferences.Set(PrefModus, (int)Modus);

            await Navigation.PopAsync();
        }
    }
}
