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

        private async void OnPolybiusClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(CodePolybius));
        }

        private async void OnAlKindiClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(CodeAlKindi));
        }
        
        
    }
}
