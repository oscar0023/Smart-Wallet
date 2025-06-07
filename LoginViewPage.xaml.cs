namespace FinanceApp.Views;
using FinanceApp.ViewModels;

public partial class LoginViewPage : ContentPage
{
    public LoginViewPage()
    {
        InitializeComponent();
        this.BindingContext = new LoginViewModel();
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
    }
}