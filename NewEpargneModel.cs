using CommunityToolkit.Mvvm.Input;
using FinanceApp.Models;
using FinanceApp.Views;
using FinanceApp.Services;

namespace FinanceApp.ViewModels
{
    public partial class NewEpargneModel
    {
        public string TitreText { get; set; } = string.Empty;
        public string DescText { get; set; } = string.Empty;
        public string AmountText { get; set; } = string.Empty;
        public string Taux { get; set; } = string.Empty;
        public DateTime DateSetter { get; set; }
        private INavigation navigation;
        private AppDatabase? Database;
        public NewEpargneModel(INavigation navigation)
        {
            Database = new AppDatabase();
            this.navigation = navigation;
        }
        public NewEpargneModel()
        {
        }

        [RelayCommand]
        public async Task OnclickCancelButton()
        {
            TitreText = string.Empty;
            DescText = string.Empty;
            AmountText = string.Empty;
            Taux = string.Empty;
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }

        [RelayCommand]
        public async Task ClickSaveButton()
        {
            if (string.IsNullOrWhiteSpace(TitreText) || string.IsNullOrWhiteSpace(AmountText)
                || string.IsNullOrWhiteSpace(DescText))
            {
                await Shell.Current.DisplayAlert("Alert", "Veuillez remplir les champs vide", "Ok");
                return;
            }
            if (!double.TryParse(AmountText, out double montant))
            {
                await Shell.Current.DisplayAlert("Alert", "Montant invalide !", "OK");
            }
            if (!double.TryParse(Taux, out double pourcentage) || pourcentage > 100)
            {
                await Shell.Current.DisplayAlert("Alert", "pourcentage invalide !", "OK");
            }
            var epargne = new Epargne(TitreText, DescText, montant, pourcentage, DateSetter.Date);
            if(Database != null)
            {
                var epargnes = await Database.GetAllEpargnes();
                double totalTaux = 0;
                foreach(var item in epargnes)
                {
                    if(item.MonatantCourant < item.MontantFinal)
                        totalTaux += item.Pourcentage;
                }

                if ((totalTaux + epargne.Pourcentage) > 100.0)
                {
                    epargne.Pourcentage = 100.0 - totalTaux;
                }
 
                if (await Database.AddEpargne(epargne) > 0)
                {
                    await Shell.Current.DisplayAlert("Alert", "Epargne ajoutée avec succès !", "Ok");
                    await navigation.PushAsync(new EpargneViewPage());  //Shell.Current.GoToAsync($"//{nameof(EpargneViewPage)}");
                    await NotificationService.ShowNotification("Nouvel Epargne", epargne.Titre + ": " + epargne.Description);
                    
                    DescText = string.Empty;
                    TitreText = string.Empty;
                    AmountText = string.Empty;
                   
                }
                else await Shell.Current.DisplayAlert("Alert", "Echec, reseillez plus tard !!!", "OK");
            }
                
        }
    }
}
