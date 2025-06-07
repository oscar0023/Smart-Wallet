using FinanceApp.Models;
using FinanceApp.Services;
using FinanceApp.ViewModels;
namespace FinanceApp.Views;

public partial class EpargneViewPage : ContentPage
{
    public EpargneViewPage()
    {
        InitializeComponent();
        BindingContext = new EpargneViewModel(this.Navigation);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if(BindingContext is EpargneViewModel epargneViewModel)
        {
            epargneViewModel.AfficheAllEpargnes();
        }
    }

    private async void EpargneItemTapped(object sender, EventArgs e)
    {
        AppDatabase database = new();
        string command = await DisplayActionSheet("Action", "Annuler", null, "Supprimer", "Collecter");
        if (sender is EpargneModel EModele && EModele.BindingContext is EpargneViewModel epargneViewModel)
        {
            switch (command)
            {
                case "Supprimer":
                    if (await database.DeleteEpargne(epargneViewModel.Id) > 0)
                    {
                        await DisplayAlert("Info", "Epargne Supprimer de la liste", "OK");
                        await NotificationService.ShowNotification("Suppression d'un Epargne", epargneViewModel.Titre + ": " + epargneViewModel.Description);
                        OnAppearing();
                    }
                    else await DisplayAlert("info", "Echec "+ epargneViewModel.Id.ToString(), "ok");
                    break;
                case "Collecter":
                    break;

                default:
                break;
            }
        }
    }
}
