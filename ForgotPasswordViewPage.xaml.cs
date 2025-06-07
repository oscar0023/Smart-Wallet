using FinanceApp.ViewModels;
namespace FinanceApp.Views;

public partial class ForgotPasswordViewPage : ContentPage
{
    public ForgotPasswordViewPage(ForgotPasswordViewModel fgPasswordViewModel)
    {
        InitializeComponent();
        this.BindingContext = fgPasswordViewModel;
    }
}
