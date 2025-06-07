using FinanceApp.ViewModels;
namespace FinanceApp.Views;


public partial class DashboardViewPage : ContentPage
{
	public DashboardViewPage()
	{
        InitializeComponent();
        BindingContext = new DashboardViewModel();
    }
    
    private void GoToSettings(object sender, EventArgs e)
    {
        // Navigate to the Settings page
        Navigation.PushAsync(new SettingViewPage());
    }
    private void GoToAllTransactions(object sender, EventArgs e)
    {
        Navigation.PushAsync(new StatistiquesViewPage());
    }
}