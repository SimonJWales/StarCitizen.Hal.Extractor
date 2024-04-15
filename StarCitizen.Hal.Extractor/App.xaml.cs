namespace StarCitizen.Hal.Extractor
{
    public partial class App : Application
    {
        const int WindowWidth = 700;

        const int WindowHeight = 600;

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

            window.Width = WindowWidth;

            window.Height = WindowHeight;

            return window;
        }
    }
}
