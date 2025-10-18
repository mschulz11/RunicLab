using RunicLab.Models; 

namespace RunicLab.Views
{
	public partial class CodeCaeserVerfahren : ContentPage
    {
        private AppSettings _settings = new AppSettings();
        public CodeCaeserVerfahren()
		{
			InitializeComponent();
		}

        private async void OnSettingsClicked(object sender, EventArgs e)
        { 
            await Navigation.PushAsync(new SettingsCaeser(_settings));
        } 
        private void OnQuellTextChanged(object sender, TextChangedEventArgs e)
        {
            var editor = sender as Editor;
            if (editor == null) return;

            // nur Buchstaben A-Z und a-z erlauben
            string filtered = new string(editor.Text
                .Where(c => char.IsLetter(c) || c == ' ')
                .ToArray());

            // Wenn sich Text verändert hat, aktualisieren
            if (filtered != editor.Text)
            {
                editor.Text = filtered;
            }
        }

        private void OnVerschluesseln(object? sender, EventArgs e)
        {
            if (quellText.Text == null) return;

            int verschiebung = Preferences.Get("CaeserVerschiebung", 3);

            char[] verschluesselt = new char[quellText.Text.Length];
            
            for (int i = 0; i < quellText.Text.Length; i++)
            {
                char c = quellText.Text[i];
                if (char.IsLetter(c))
                {
                    char offset = char.IsUpper(c) ? 'A' : 'a';
                    verschluesselt[i] = (char)(((c + verschiebung - offset) % 26) + offset);
                } 
            } 

            zielText.Text = new string(verschluesselt).ToUpper();
        }

    }
}