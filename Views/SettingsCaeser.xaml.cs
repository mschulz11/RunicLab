using RunicLab.Models;
using Microsoft.Maui.Storage; // für Preferences

namespace RunicLab.Views {
  public partial class SettingsCaeser : ContentPage {
    private readonly AppSettings _settings;

    public double VerschiebungMin { get; set; } = 0;
    public double VerschiebungMax { get; set; } = 26;
    public int Verschiebung { get; set; } = 0;


    public SettingsCaeser(AppSettings settings) {
      InitializeComponent();
      stepVerschiebung.Minimum = VerschiebungMin;
      sliderVerschiebung.Maximum = VerschiebungMax;

      stepVerschiebung.Minimum = VerschiebungMin;
      stepVerschiebung.Maximum= VerschiebungMax;

      _settings = settings;

      // Verschiebung aus Preferences laden
      SetVerschiebung(Preferences.Get("CaeserVerschiebung", 3));
    }
    private void OnEntryVerschiebung(object sender, EventArgs e) {
      if (int.TryParse(entryVerschiebung.Text, out int value)) {
        SetVerschiebung(value);
      }
    }

    private void OnStepVerschiebung(object sender, EventArgs e) {
      SetVerschiebung((int)stepVerschiebung.Value);
    }

    private void OnSliederVerschiebung(object sender, EventArgs e) {
      SetVerschiebung((int)sliderVerschiebung.Value);
    }

    bool verAktiv = false;
    private void SetVerschiebung(int value) {
      if (verAktiv) { return; }
      verAktiv = true;
      if (value < VerschiebungMin) { value = (int)VerschiebungMin; }
      if (value > VerschiebungMax) { value = (int)VerschiebungMax; }
      Verschiebung = value;
      entryVerschiebung.Text = Verschiebung.ToString();
      stepVerschiebung.Value = Verschiebung;
      sliderVerschiebung.Value = Verschiebung;

      BuildAlphabetGrid(Verschiebung); // Verschiebungsbeispiel-Tabelle bauen 
      verAktiv = false;
    } 
    private void BuildAlphabetGrid(int verschiebung) {
      AlphabetGrid.Children.Clear();

      // Kopfzeile
      AlphabetGrid.Add(new Label {
        Text = "Original",
        FontAttributes = FontAttributes.Bold,
        HorizontalOptions = LayoutOptions.Center
      }, 0, 0);

      AlphabetGrid.Add(new Label {
        Text = "?",
        FontAttributes = FontAttributes.Bold,
        HorizontalOptions = LayoutOptions.Center
      }, 1, 0);

      AlphabetGrid.Add(new Label {
        Text = "Verschoben",
        FontAttributes = FontAttributes.Bold,
        HorizontalOptions = LayoutOptions.Center
      }, 2, 0);

      int row = 1; // Startzeile nach Überschrift

      for (char c = 'A'; c <= 'Z'; c++) {
        char verschoben = (char)('A' + (c - 'A' + verschiebung) % 26);

        AlphabetGrid.Add(new Label {
          Text = c.ToString(),
          FontSize = 18,
          HorizontalOptions = LayoutOptions.Center
        }, 0, row);

        AlphabetGrid.Add(new Label {
          Text = "?",
          FontSize = 18,
          HorizontalOptions = LayoutOptions.Center
        }, 1, row);

        AlphabetGrid.Add(new Label {
          Text = verschoben.ToString(),
          FontSize = 18,
          HorizontalOptions = LayoutOptions.Center
        }, 2, row);

        row++;
      }
    }




    private async void OnCloseClicked(object sender, EventArgs e) {
      Preferences.Set("CaeserVerschiebung", Verschiebung);

      await Navigation.PopAsync();
    }
  }
}