
namespace FinanceApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }


        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell();
            bool test = Preferences.Get("RemeberMe", false);
            Application.Current.Dispatcher.Dispatch(async () =>
            {
                if (!test)
                {
                    _ = shell.Navigation.PushAsync(new Views.LoginViewPage());
                }
            });
            return new Window(shell);
        }

        protected override void OnResume()
        {
            base.OnResume();
            Preferences.Set("RememberMe", false);
        }
    }
}