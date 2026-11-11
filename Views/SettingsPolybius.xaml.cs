using RunicLab.Models;

namespace RunicLab.Views
{
    public partial class SettingsPolybius : ContentPage
    {
        private static readonly string[] SeparatorNamen =
        {
            "Keine (durchgehende Ziffern)",
            "Leerzeichen",
            "Bindestrich",
            "Schrägstrich"
        };

        private readonly PolybiusOptions _options;
        private bool _syncing;

        public SettingsPolybius()
        {
            InitializeComponent();

            pickerSeparator.ItemsSource = SeparatorNamen;
            _options = PolybiusPreferences.Load();

            _syncing = true;
            entryKeyword.Text = _options.Keyword;
            pickerSeparator.SelectedIndex = (int)_options.Separator;
            switchUmlaute.IsToggled = _options.TransliterateUmlauts;

            if (PolybiusPreferences.LoadEntschluesseln())
            {
                rbEntschluesseln.IsChecked = true;
            }
            else
            {
                rbVerschluesseln.IsChecked = true;
            }

            _syncing = false;

            BuildPreview();
        }

        private void OnKeywordChanged(object sender, TextChangedEventArgs e)
        {
            if (_syncing)
            {
                return;
            }

            _options.Keyword = entryKeyword.Text ?? string.Empty;
            Persist();
        }

        private void OnSeparatorChanged(object sender, EventArgs e)
        {
            if (_syncing || pickerSeparator.SelectedIndex < 0)
            {
                return;
            }

            _options.Separator = (PolybiusSeparator)pickerSeparator.SelectedIndex;
            Persist();
        }

        private void OnUmlauteToggled(object sender, ToggledEventArgs e)
        {
            if (_syncing)
            {
                return;
            }

            _options.TransliterateUmlauts = e.Value;
            Persist();
        }

        private void OnModusChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_syncing || sender is not RadioButton radio || !radio.IsChecked)
            {
                return;
            }

            PolybiusPreferences.SaveEntschluesseln(ReferenceEquals(radio, rbEntschluesseln));
        }

        private void Persist()
        {
            PolybiusPreferences.Save(_options);
            BuildPreview();
        }

        private void BuildPreview() => PolybiusGridBuilder.Fill(PreviewGrid, _options, 16);

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            PolybiusPreferences.Save(_options);
            await Navigation.PopAsync();
        }
    }
}
