using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceApp.Models;
using FinanceApp.Services;
using Microcharts;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.ViewModels
{
    public partial class StatistiquesViewModel : BaseViewModel
    {
        private readonly AppDatabase _databaseService;

        [ObservableProperty]
        private decimal _totalRevenu = 0;

        [ObservableProperty]
        private decimal _totalDepense = 0;

        [ObservableProperty]
        private decimal _solde = 0;

        [ObservableProperty]
        private string _userName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<CategoryStats> _depensesParCategorie = new();

        [ObservableProperty]
        private ObservableCollection<CategoryStats> _revenusParCategorie = new();

        [ObservableProperty]
        private DateTime _selectedMonth = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<DataTransaction> _allTransactions = new();

        [ObservableProperty]
        private ObservableCollection<TransactionViewModel> _recentTransactions = new();

        [ObservableProperty]
        private Chart _evolutionChart;

        public IAsyncRelayCommand LoadStatisticsCommand { get; }

        public StatistiquesViewModel()
        {
            Title = "Statistiques";
            _databaseService = new AppDatabase();
            UserName = Preferences.Get("UserName", string.Empty);
            LoadStatisticsCommand = new AsyncRelayCommand(LoadStatisticsAsync);
            Task.Run(LoadStatisticsAsync);
        }

        partial void OnSelectedMonthChanged(DateTime value)
        {
            Task.Run(LoadStatisticsAsync);
        }

        private async Task LoadStatisticsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var allTransactionsData = await _databaseService.GetAllTransactions();
                AllTransactions.Clear();
                foreach (var transaction in allTransactionsData)
                {
                    AllTransactions.Add(transaction);
                }

                var transactionsThisMonth = allTransactionsData
                    .Where(t => t.SaveDate.Year == SelectedMonth.Year && t.SaveDate.Month == SelectedMonth.Month)
                    .ToList();

                if (transactionsThisMonth.Count == 0)
                {
                    ResetStatistics();
                    await Shell.Current.DisplayAlert("Alerte", "Aucune transaction trouvée pour le mois sélectionné.", "OK");
                    return;
                }

                CalculateGeneralStatistics(transactionsThisMonth);
                CalculateExpensesByCategory(transactionsThisMonth);
                CalculateRevenuesByCategory(transactionsThisMonth);
                LoadRecentTransactions(allTransactionsData);
               // GenerateBalanceEvolutionChart(allTransactionsData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des statistiques: {ex.Message}");
                await Shell.Current.DisplayAlert("Erreur", "Impossible de charger les statistiques.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ResetStatistics()
        {
            Solde = 0;
            TotalRevenu = 0;
            TotalDepense = 0;
            DepensesParCategorie.Clear();
            RevenusParCategorie.Clear();
            RecentTransactions.Clear();
            EvolutionChart = null;
        }

        private void CalculateGeneralStatistics(System.Collections.Generic.List<DataTransaction> transactions)
        {
            TotalRevenu = (decimal)transactions.Where(t => t.Type == "Revenu").Sum(t => t.Budget);
            TotalDepense = (decimal)transactions.Where(t => t.Type == "Depense").Sum(t => t.Budget);
            Solde = TotalRevenu - TotalDepense;
        }

        private void CalculateExpensesByCategory(System.Collections.Generic.List<DataTransaction> transactions)
        {
            var depensesParCategorieGrouped = transactions
                .Where(t => t.Type == "Depense")
                .GroupBy(t => t.Category ?? "Autre")
                .Select(g => new CategoryStats { Category = g.Key, Total = (decimal)(g.Sum(t => t.Budget)) })
                .OrderByDescending(s => s.Total);

            DepensesParCategorie.Clear();
            foreach (var stat in depensesParCategorieGrouped)
            {
                DepensesParCategorie.Add(stat);
            }
        }

        private void CalculateRevenuesByCategory(System.Collections.Generic.List<DataTransaction> transactions)
        {
            var revenusParCategorieGrouped = transactions
                .Where(t => t.Type == "Revenu")
                .GroupBy(t => t.Category ?? "Autre")
                .Select(g => new CategoryStats { Category = g.Key, Total = (decimal)(g.Sum(t => t.Budget)) })
                .OrderByDescending(s => s.Total);

            RevenusParCategorie.Clear();
            foreach (var stat in revenusParCategorieGrouped)
            {
                RevenusParCategorie.Add(stat);
            }
        }

        private void LoadRecentTransactions(System.Collections.Generic.List<DataTransaction> allTransactionsData)
        {
            RecentTransactions.Clear();
            var recentTransactions = allTransactionsData
                .OrderByDescending(t => t.SaveDate)
                .Take(5)
                .ToList();
            foreach (var transaction in recentTransactions)
            {
                RecentTransactions.Add(new TransactionViewModel(transaction));
            }
        }

        private void GenerateBalanceEvolutionChart(System.Collections.Generic.List<DataTransaction> allTransactions)
        {
            var balanceHistory = new ObservableCollection<BalancePoint>();
            decimal currentBalance = 0;

            var transactionsThisMonth = allTransactions
                .Where(t => t.SaveDate.Year == SelectedMonth.Year && t.SaveDate.Month == SelectedMonth.Month)
                .OrderBy(t => t.SaveDate);

            foreach (var transaction in transactionsThisMonth)
            {
                if (transaction.Type == "Revenu")
                {
                    currentBalance += (decimal)transaction.Budget;
                }
                else if (transaction.Type == "Depense")
                {
                    currentBalance -= (decimal)transaction.Budget;
                }
                balanceHistory.Add(new BalancePoint { Date = transaction.SaveDate, Balance = currentBalance });
            }

            var entries = balanceHistory.Select(bp => new ChartEntry((float)bp.Balance)
            {
                Label = bp.Date.ToString("dd"),
                ValueLabel = $"{bp.Balance:N2}",
                Color = bp.Balance >= 0 ? SKColors.Green : SKColors.Red
            }).ToList();

            EvolutionChart = new LineChart { Entries = entries };
        }
    }

    public class CategoryStats
    {
        public string? Category { get; set; }
        public decimal Total { get; set; }
    }

    public class BalancePoint
    {
        public DateTime Date { get; set; }
        public decimal Balance { get; set; }
    }
}