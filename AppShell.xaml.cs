using RunicLab.Views;

namespace RunicLab
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(CodeCaeserVerfahren), typeof(CodeCaeserVerfahren));
            Routing.RegisterRoute(nameof(CodePolybius), typeof(CodePolybius));
            Routing.RegisterRoute(nameof(CodeAlKindi), typeof(CodeAlKindi));
        }
    }
}
