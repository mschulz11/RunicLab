using RunicLab.Models;
using Microsoft.Maui.Storage; // für Preferences

namespace RunicLab.Views
{
    public partial class SettingsCaeser : ContentPage
    {
        private readonly AppSettings _settings;

        private int verschiebung = 0;

        public SettingsCaeser(AppSettings settings)
        {
            InitializeComponent();
            _settings = settings; 

            // Verschiebung aus Preferences laden
            verschiebung = Preferences.Get("CaeserVerschiebung", 3);
            entryVerschiebung.Text = verschiebung.ToString();
            stepVerschiebung.Value = verschiebung;
        }
        private void OnEntryVerschiebung(object sender, EventArgs e)
        {
            if (int.TryParse(entryVerschiebung.Text, out int value))
            {
                if (value < stepVerschiebung.Minimum) value = (int)stepVerschiebung.Minimum;
                if (value > stepVerschiebung.Maximum) value = (int)stepVerschiebung.Maximum;
                verschiebung = value;
                entryVerschiebung.Text = value.ToString();
                stepVerschiebung.Value = verschiebung;
            }
        }
        private void OnStepVerschiebung(object sender, EventArgs e)
        {
            int value = (int)stepVerschiebung.Value;            
            if (value < stepVerschiebung.Minimum) value = (int)stepVerschiebung.Minimum;
            if (value > stepVerschiebung.Maximum) value = (int)stepVerschiebung.Maximum;
            verschiebung = value;
            entryVerschiebung.Text = value.ToString();
            stepVerschiebung.Value = verschiebung;

        }

        private async void OnCloseClicked(object sender, EventArgs e)
        { 
            Preferences.Set("CaeserVerschiebung", verschiebung);

            await Navigation.PopAsync();
        }
    }
}