using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using FinanceApp.Views;
using System.Collections.ObjectModel;
using FinanceApp.Services;


namespace FinanceApp.ViewModels
{
    public partial class EpargneViewModel
    {
        public int Id { get; set; }
        public string Titre { get; set; } = "Titre";
        public string CurrentAmount { get; set; } = "1200";
        public string Description { get; set; } = "Description";
        public string Montant { get; set; } = "50000";
        public double Progression { get; set; } = 0.3;
        public string Pourcentage { get; set; } = "34%";
        public DateTime DateSetter { get; set; } = DateTime.Now;
        public INavigation? navigation;

        private AppDatabase? database;
        public ObservableCollection<EpargneViewModel> AllEpargne { get; set; }

        public EpargneViewModel()
        {
            AllEpargne = [];
        }
        public EpargneViewModel(INavigation navigation)
        {
            AllEpargne = [];
            this.navigation = navigation;
        }
        public EpargneViewModel(int id, string ti, string desc, double prog, string mtn, string currmtn, string pourcenta)
        {
            this.Id = id;
            Titre = ti;
            Description = desc;
            Progression = prog;
            Montant = mtn;
            CurrentAmount = currmtn;
            Pourcentage = pourcenta;
   
            AllEpargne = [];
        }

        public async void AfficheAllEpargnes()
        {
            database = new();
            AllEpargne.Clear();
            if (database != null)
            {
                var epargnes = await database.GetAllEpargnes();

                foreach (var item in epargnes)
                {
                    Id = item.Id;
                    Titre = item.Titre;
                    Description = item.Description;
                    Montant = item.MontantFinal.ToString() + " MAD";
                    Progression = (item.MonatantCourant / item.MontantFinal);
                    Pourcentage = item.Pourcentage.ToString() + " %";
                    
                    if (item.MonatantCourant >= item.MontantFinal)
                        CurrentAmount = "Terminé";
                    else
                        CurrentAmount = item.MonatantCourant.ToString() + " MAD";

                    AllEpargne.Add(new EpargneViewModel(Id, Titre, Description, Progression, Montant, CurrentAmount, Pourcentage));
 
                }
            }
         
        } 

            
        [RelayCommand]
        private async Task ClickAddNewEpargneButton()
        {
            if (navigation != null)
                await navigation.PushAsync(new NewEpargneViewPage());
            else return;
        }
    }
}
