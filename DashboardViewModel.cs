using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceApp.Models;
using FinanceApp.Services;
using FinanceApp.Views;
using Microcharts;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;

namespace FinanceApp.ViewModels
{
    [SupportedOSPlatform("android")]
    [SupportedOSPlatform("ios")]
    [SupportedOSPlatform("windows")]
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly AppDatabase _database;

        [ObservableProperty]
        private string _userName = "User";

        [ObservableProperty]
        private string _userImage = "dash.jpg";

        [ObservableProperty]
        private string _solde = "0.00 MAD";

        [ObservableProperty]
        private string _totalRevenuMonth = "0.00 MAD";

        [ObservableProperty]
        private string _totalDepenseMonth = "0.00 MAD";

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<TransactionViewModel> _recentTransactions = new();

        [ObservableProperty]
        private ObservableCollection<EpargneViewModel> _savingsGoalsSummary = new();

        [ObservableProperty]
        private Chart _chartDataIncomeExpense;

        [ObservableProperty]
        private Chart _chartDataIncomeRevenu;

        [ObservableProperty]
        private Chart _chartDataGoals;

        public IAsyncRelayCommand LoadDataCommand { get; }

        public DashboardViewModel()
        {
            Title = "Dashboard";
            _database = new AppDatabase();
            LoadDataCommand = new AsyncRelayCommand(LoadDashboardDataAsync);
            Task.Run(LoadDashboardDataAsync);

            var savedImagePath = Preferences.Get("UserImage", string.Empty);
            if (!string.IsNullOrEmpty(savedImagePath))
            {
                _userImage = savedImagePath; // Assign the string path directly to _userImage
            }
        }

        partial void OnSelectedDateChanged(DateTime value)
        {
            Task.Run(LoadDashboardDataAsync);
        }

        private async Task LoadDashboardDataAsync()
        {
            IsBusy = true;
            try
            {
                UserName = Preferences.Get("CurrentUser", "User");

                var allTransactions = await _database.GetAllTransactions();
                if (allTransactions is not null)
                {
                    var transactionsThisMonth = allTransactions
                        .Where(t => t.SaveDate.Year == SelectedDate.Year && t.SaveDate.Month == SelectedDate.Month || t.SaveDate.Day == SelectedDate.Day)
                        .OrderByDescending(t => t.SaveDate)
                        .ToList();

                    double income = transactionsThisMonth.Where(t => t.Type == "Revenu").Sum(t => t.Budget);
                    double expense = transactionsThisMonth.Where(t => t.Type == "Depense").Sum(t => t.Budget);
                    TotalRevenuMonth = $"{income:N0} MAD";
                    TotalDepenseMonth = $"{expense:N2} MAD";

                    double totalIncomeAll = allTransactions.Where(t => t.Type == "Revenu").Sum(t => t.Budget);
                    double totalExpenseAll = allTransactions.Where(t => t.Type == "Depense").Sum(t => t.Budget);
                    Solde = $"{totalIncomeAll - totalExpenseAll:N0} MAD";

                    RecentTransactions.Clear();
                    foreach (var transaction in transactionsThisMonth.Take(5))
                    {
                        RecentTransactions.Add(new TransactionViewModel(transaction));
                    }

                    PrepareExpenseChart(transactionsThisMonth);
                    PrepareRevenueChart(transactionsThisMonth);
                }
                else
                {
                    ResetDashboardData();
                    await Shell.Current.DisplayAlert("Information", "Aucune transaction pour la période sélectionnée.", "OK");
                }

                var allEpargnesList = await _database.GetAllEpargnes();
                PrepareGoalsChart(allEpargnesList);
                PrepareSavingsGoalsSummary(allEpargnesList);
            }
            catch (NullReferenceException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard data (NullReference): {ex.Message}");
                await Shell.Current.DisplayAlert("Erreur", "Impossible de charger les données du tableau de bord.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
                await Shell.Current.DisplayAlert("Erreur", "Impossible de charger les données du tableau de bord.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ResetDashboardData()
        {
            TotalRevenuMonth = "0.00 MAD";
            TotalDepenseMonth = "0.00 MAD";
            Solde = "0.00 MAD";
            RecentTransactions.Clear();
            ChartDataIncomeExpense = new Microcharts.PieChart { Entries = new List<ChartEntry>() };
            ChartDataIncomeRevenu = new Microcharts.PieChart { Entries = new List<ChartEntry>() };
            ChartDataGoals = new BarChart { Entries = new List<ChartEntry>() };
            SavingsGoalsSummary.Clear();
        }

        private void PrepareExpenseChart(List<DataTransaction> transactions)
        {
            var expenseCategories = transactions
                .Where(t => t.Type == "Depense")
                .GroupBy(t => t.Category ?? "Autre")
                .Select(g => new ChartEntry((float)g.Sum(t => t.Budget))
                {
                    Label = g.Key,
                    ValueLabel = $"{g.Sum(t => t.Budget):N0}",
                    Color = SKColor.Parse(GetRandomColor())
                })
                .ToList();

            ChartDataIncomeExpense = new Microcharts.PieChart
            {
                Entries = expenseCategories.Count == 0
                    ? new[] { new ChartEntry(0) { Label = "Aucune dépense", ValueLabel = "0 MAD", Color = SKColors.Gray } }
                    : expenseCategories,
                LabelTextSize = 25f,
                LabelColor = expenseCategories.Count == 0 ? SKColors.Gray : SKColor.Empty,
                LabelMode = expenseCategories.Count == 0 ? LabelMode.RightOnly : LabelMode.LeftAndRight,
                BackgroundColor = SKColors.Cornsilk
            };
        }

        private void PrepareRevenueChart(List<DataTransaction> transactions)
        {
            var revenuCategories = transactions
                .Where(t => t.Type == "Revenu")
                .GroupBy(t => t.Category ?? "Autre")
                .Select(g => new ChartEntry((float)g.Sum(t => t.Budget))
                {
                    Label = g.Key,
                    ValueLabel = $"{g.Sum(t => t.Budget):N0}",
                    Color = SKColor.Parse(GetRandomColor())
                })
                .ToList();

            ChartDataIncomeRevenu = new Microcharts.PieChart
            {
                Entries = revenuCategories.Count == 0
                    ? new[] { new ChartEntry(0) { Label = "Aucun revenu", ValueLabel = "0 MAD", Color = SKColors.Gray } }
                    : revenuCategories,
                LabelTextSize = 15f,
                LabelColor = revenuCategories.Count == 0 ? SKColors.Gray : SKColor.Empty,
                LabelMode = revenuCategories.Count == 0 ? LabelMode.RightOnly : LabelMode.RightOnly,
                BackgroundColor = SKColors.DarkSeaGreen
            };
        }

        private void PrepareGoalsChart(List<Epargne> allEpargnesList)
        {
            if (allEpargnesList is null || !allEpargnesList.Any())
            {
                ChartDataGoals = new BarChart
                {
                    Entries = new[] { new ChartEntry(0) { Label = "Aucun objectif", ValueLabel = "0%", Color = SKColors.Gray } },
                    LabelTextSize = 25f,
                    BackgroundColor = SKColors.Transparent,
                    ValueLabelOrientation = Microcharts.Orientation.Horizontal,
                    LabelOrientation = Microcharts.Orientation.Horizontal
                };
                return;
            }

            var goalEntries = allEpargnesList.Select(e => new ChartEntry((float)e.MonatantCourant)
            {
                Label = e.Titre,
                ValueLabel = $"{(e.MontantFinal > 0 ? (e.MonatantCourant / e.MontantFinal) * 100 : 0):N0}%",
                Color = SKColor.Parse(GetRandomColor()),
                ValueLabelColor = SKColors.DarkRed,
            }).ToList();

            ChartDataGoals = new BarChart
            {
                Entries = goalEntries.Any() ? goalEntries : new[] { new ChartEntry(0) { Label = "Aucun objectif", ValueLabel = "0%", Color = SKColors.Gray } },
                LabelTextSize = 25f,
                BackgroundColor = SKColors.Transparent,
                ValueLabelOrientation = Microcharts.Orientation.Horizontal,
                LabelOrientation = Microcharts.Orientation.Horizontal
            };
        }

        private void PrepareSavingsGoalsSummary(System.Collections.Generic.List<Epargne> allEpargnes)
        {
            SavingsGoalsSummary.Clear();
            if (allEpargnes is null || !allEpargnes.Any())
            {
                SavingsGoalsSummary.Add(new EpargneViewModel(0, "Aucun objectif", "Pas d'objectifs d'épargne", 0, "0 MAD", "Aucun", "0%"));
                return;
            }

            foreach (var epargne in allEpargnes.Take(5))
            {
                SavingsGoalsSummary.Add(new EpargneViewModel(
                    epargne.Id,
                    epargne.Titre,
                    epargne.Description,
                    (double)(epargne.MontantFinal > 0 ? epargne.MonatantCourant / epargne.MontantFinal : 0),
                    $"{epargne.MontantFinal:N2} MAD",
                    epargne.MonatantCourant >= epargne.MontantFinal ? "Terminé" : $"{epargne.MonatantCourant:N2} MAD",
                    $"{epargne.Pourcentage}%"
                ));
            }
        }

        private string GetRandomColor()
        {
            var random = new System.Random();
            return $"#{random.Next(0x1000000):X6}";
        }

        [RelayCommand]
        private async Task ClickAddNewEpargne()
        {
            await Shell.Current.GoToAsync($"//{nameof(NewEpargneViewPage)}");
        }

        [RelayCommand]
        private async Task GoToAllStats()
        {
            await Shell.Current.GoToAsync($"//{nameof(StatistiquesViewPage)}");
        }

        [RelayCommand]
        private async Task GoToSavings()
        {
            await Shell.Current.GoToAsync($"//{nameof(EpargneViewPage)}");
        }

        [RelayCommand]
        private async Task BackButton()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task GoToAddNewTransaction()
        {
            await Shell.Current.GoToAsync($"//{nameof(AddPage)}");
        }

        [RelayCommand]
        private async Task GoToAllTransactions()
        {
            await Shell.Current.GoToAsync($"//{nameof(StatistiquesViewPage)}");
        }
        /*
                 [RelayCommand]
        private async Task GoToSettings()
        {
            await Shell.Current.GoToAsync($"//{nameof(SettingViewPage)}");
        }
         */
    }
}