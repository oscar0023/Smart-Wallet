using FinanceApp.ViewModels;
namespace FinanceApp.Views;


public partial class RegisterViewPage : ContentPage
{
    public RegisterViewPage(RegisterViewModel RviewModele)
    {
        InitializeComponent();
        this.BindingContext = RviewModele;
    }
}