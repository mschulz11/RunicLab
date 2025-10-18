using System.Diagnostics.Metrics;

namespace RunicLab.Views
{
    public partial class MainPage : ContentPage
    { 
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCaeserClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(CodeCaeserVerfahren));
        }
        
        
    }
}
