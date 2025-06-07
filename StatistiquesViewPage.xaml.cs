using FinanceApp.ViewModels;
namespace FinanceApp.Views;


public partial class StatistiquesViewPage : ContentPage
{
	public StatistiquesViewPage()
	{
		InitializeComponent();
		this.BindingContext = new StatistiquesViewModel();
	}
}