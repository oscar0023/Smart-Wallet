using FinanceApp.Models;
using FinanceApp.Services;
using FinanceApp.ViewModels;

namespace FinanceApp;

public partial class AddPage : ContentPage
{
    private AppDatabase database;
    private int MinimumYear = -5;
    private int MaximumYear = 5;
    private int IdTransaction = 0;
    private string typeTransaction = "Revenu";
    private static readonly string[] AllTypeRevenu = { "Salaire", "Vente", "Cadeaux", "Autre" };
    private static readonly string[] AllTypeDepense = { "Famille", "Achat", "Transport", "Nourriture", "Traitement", "Autre" };

    public AddPage()
    {
        InitializeComponent();
        database = new AppDatabase();

        DateSetter.Date = DateTime.Now;
        DateSetter.MinimumDate = DateTime.Now.AddYears(MinimumYear);
        DateSetter.MaximumDate = DateTime.Now.AddYears(MaximumYear);

        DepenseBorder.BackgroundColor = Colors.White;
        RevenuBorder.BackgroundColor = Colors.DarkCyan;
    }
    public AddPage(TransactionViewModel transactionViewModel)
    {
        InitializeComponent();
        database = new AppDatabase();
        IdTransaction = transactionViewModel.Id;
        ButtonAdd.Text = "Modifier";
        SoldeLabel.Text = transactionViewModel.Budget.Substring(1);
        DateSetter.Date = transactionViewModel.SaveDate;
        DescLabel.Text = transactionViewModel.Description?.Split('\n')[0];
        CategoryLabel.Text = transactionViewModel.Category;

        if (transactionViewModel.Type == "Revenu")
        {
            RBrevenu.IsChecked = true;
            typeTransaction = "Revenu";
            RevenuBorder.BackgroundColor = Colors.DarkCyan;
            DepenseBorder.BackgroundColor = Colors.White;
        }
        else
        {
            RBdepense.IsChecked = true;
            typeTransaction = "Depense";
            DepenseBorder.BackgroundColor = Colors.Coral;
            RevenuBorder.BackgroundColor = Colors.White;
        }
    }

    public async void OnClickAddButton(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CategoryLabel.Text) ||
            string.IsNullOrWhiteSpace(SoldeLabel.Text) ||
            string.IsNullOrWhiteSpace(DescLabel.Text)
            )
        {
            await DisplayAlert("Alert", "Champs Vide", "OK");
            return;
        }

        if (!double.TryParse(SoldeLabel.Text, out double budget))
        {
            await DisplayAlert("Alert", "Montant invalide !", "OK");
            return;
        }

        double solde = (double)Preferences.Get("solde", 0f);
        if (RBdepense.IsChecked && budget > solde)
        {
            await DisplayAlert("Alert", "votre Solde actuel est insuffissant", "OK");
            SoldeLabel.Text = "";
            CategoryLabel.Text = "";
            DescLabel.Text = "";
            return;
        }

        DataTransaction Data = new(CategoryLabel.Text, DescLabel.Text, DateSetter.Date, budget);
        Data.Type = typeTransaction;
        if (IdTransaction != 0)
        {
            Data.Id = IdTransaction;
        }
        if ( Data.Type == "Revenu" && double.TryParse(TauxEpargne.Text, out double tauxEpargne))
        {
            await database.UpdateAllEpargnes(Data.Budget * (tauxEpargne/100.0) );
            Data.Budget -= Data.Budget * (tauxEpargne / 100.0);
        }
        await SaveTransaction(Data);
        
    }

    private async void OnClickedCategoryAddButton(object sender, EventArgs e)
    {
        string CategoryChosed = "";
        if (RBrevenu.IsChecked)
        {
            CategoryChosed = await DisplayActionSheet("Category", "Annuler", null, AllTypeRevenu);
        }
        else
        {
            CategoryChosed = await DisplayActionSheet("Category", "Annuler", null, AllTypeDepense);
            typeTransaction = "Depense";
        }

        if (!string.IsNullOrEmpty(CategoryChosed) && CategoryChosed != "Annuler")
        {
            CategoryLabel.Text = CategoryChosed;
        }
    }

    public async Task SaveTransaction(DataTransaction Data)
    {
        if (ButtonAdd.Text == "Modifier")
        {
            if (await database.UpdateTransaction(Data) <= 0)
            {
                await DisplayAlert("Erreur", "Erreur d'enregistrement, MODIFICATION", "OK");
                return;
            }
        }
        else
        {
            if (await database.AddTransaction(Data) <= 0)
            {
                await DisplayAlert("Erreur", "Erreur d'enregistrement", "OK");
                return;
            }
        }

        await DisplayAlert("Info", "Transaction Enreigistrer !", "OK");
        await this.Navigation.PushAsync(new Transaction());
        Navigation.RemovePage(this);

        //// GENARATION D'UNE NOTIFICATION INFORMANT L'AJOUT D'UNE TRANSACTION
        await NotificationService.ShowNotification("Nouvelle Transaction", "ajout d'une nouvelle tranction: " + Data.Type);
    }

    private void OnChangedTransactionType(object sndr, CheckedChangedEventArgs e)
    {
        if (e.Value && sndr == RBdepense)
        {
            DepenseBorder.BackgroundColor = Colors.Coral;
            RevenuBorder.BackgroundColor = Colors.White;

            TauxEpargne.IsVisible = false;
            LabelTauxEpargne.IsVisible = false;

            FormeBorder.Padding = 15;
        }
        else if (e.Value && sndr == RBrevenu)
        {
            DepenseBorder.BackgroundColor = Colors.White;
            RevenuBorder.BackgroundColor = Colors.DarkCyan;

            TauxEpargne.IsVisible = true;
            LabelTauxEpargne.IsVisible = true;

            FormeBorder.Padding = 10;
        }
    }
}
