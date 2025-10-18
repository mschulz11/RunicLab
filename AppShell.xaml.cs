using RunicLab.Views;

namespace RunicLab
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(CodeCaeserVerfahren), typeof(CodeCaeserVerfahren));
        }
    }
}
