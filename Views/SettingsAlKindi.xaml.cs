using RunicLab.Models;

namespace RunicLab.Views
{
    public partial class SettingsAlKindi : ContentPage
    {
        private bool _syncing;

        public SettingsAlKindi()
        {
            InitializeComponent();

            _syncing = true;

            stepTopAnzahl.Minimum = AlKindiPreferences.TopMin;
            stepTopAnzahl.Maximum = AlKindiPreferences.TopMax;
            stepTopAnzahl.Value = AlKindiPreferences.LoadTopAnzahl();
            switchErwartung.IsToggled = AlKindiPreferences.LoadZeigeErwartung();

            if (AlKindiPreferences.LoadSprache() == FrequencyLanguage.Englisch)
            {
                rbEnglisch.IsChecked = true;
            }
            else
            {
                rbDeutsch.IsChecked = true;
            }

            _syncing = false;
        }

        private void OnSpracheChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_syncing || sender is not RadioButton radio || !radio.IsChecked)
            {
                return;
            }

            AlKindiPreferences.SaveSprache(ReferenceEquals(radio, rbEnglisch)
                ? FrequencyLanguage.Englisch
                : FrequencyLanguage.Deutsch);
        }

        private void OnTopAnzahlChanged(object sender, ValueChangedEventArgs e)
        {
            if (_syncing)
            {
                return;
            }

            AlKindiPreferences.SaveTopAnzahl((int)e.NewValue);
        }

        private void OnErwartungToggled(object sender, ToggledEventArgs e)
        {
            if (_syncing)
            {
                return;
            }

            AlKindiPreferences.SaveZeigeErwartung(e.Value);
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
