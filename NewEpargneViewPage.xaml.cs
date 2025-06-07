using FinanceApp.ViewModels;

namespace FinanceApp.Views;

public partial class NewEpargneViewPage : ContentPage
{
	public NewEpargneViewPage()
	{
		InitializeComponent();
        BindingContext = new NewEpargneModel(this.Navigation);

        Shell.SetTabBarIsVisible(this, true);

        DateHandler.Date = DateTime.Now;
        DateHandler.MaximumDate = DateTime.Now.AddYears(5);
        DateHandler.MinimumDate = DateTime.Now.AddYears(-5);
    }
}