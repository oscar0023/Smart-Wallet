using FinanceApp.Models;
using FinanceApp.ViewModels;
using FinanceApp.Services;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Collections.ObjectModel;

namespace FinanceApp;

public partial class Transaction : ContentPage
{
    private readonly AppDatabase? Database;
    public ObservableCollection<TransactionViewModel> Alltransactions { get; set; }

    public Transaction()
    {
        InitializeComponent();
        Database = new AppDatabase();
        Alltransactions = new ObservableCollection<TransactionViewModel>();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await AfficheNewTransaction();
        skCanvasViewHandler.InvalidateSurface();
    }

    private async void OnClickedAddButton(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddPage());
        Navigation.RemovePage(this);
    }
    private async Task AfficheNewTransaction()
    {
        Alltransactions.Clear();
        if (Database != null)
        {
            var transactions = await Database.GetAllTransactions();

            for (int i = (transactions.Count - 1); i >= 0; --i)
            {
                Alltransactions.Add(new TransactionViewModel(transactions[i]));
            }
        }
    }
    private async void OnTappedTransaction(object sender, TappedEventArgs e)
    {
        string command = await DisplayActionSheet("commande", "fermer", "Annuler", "Supprimer", "Modifier", "Details");
        if (sender is ActionModele actModele && actModele.BindingContext is TransactionViewModel transaction)
        {
            switch (command)
            {
                case "Supprimer":
                    var result = await Database.DeleteTransaction(transaction.Id);
                    if (result > 0)
                    {
                        await DisplayAlert("Info", "Transaction supprimée !", "OK");
                        await AfficheNewTransaction();
                        OnAppearing();
                    }
                    else await DisplayAlert("Info", "Echec !", "OK");
                    break;
                case "Modifier":
                    await Navigation.PushAsync(new AddPage(transaction));
                    Navigation.RemovePage(this);
                    break;
                case "Details":
                    //await Navigation.PushAsync(new DetailsPage(transaction));
                    break;
                default: return;
            }
        }
        else await DisplayAlert("Info", "Erreur !", "OK");
    }

    [Obsolete]
    private void OnSurfacePaint(object sender, SKPaintSurfaceEventArgs e)
    {
        var surface = e.Surface;
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        float centerX = e.Info.Width / 2f;
        float centerY = e.Info.Height / 2f;
        float rayon = Math.Min(centerX, centerY) * 0.8f;
        float strokeWidth = 40f;
        float rayonInterieur = rayon - (strokeWidth / 2);

        float totalRevenu = (float)Preferences.Get("totalRevenu", 0f);
        float totalDepense = (float)Preferences.Get("totalDepense", 0f);
        float solde = Preferences.Get("solde", 0f);

        float[] values = { totalRevenu, totalDepense };
        float total = values.Sum();
        SKColor[] colors = { SKColors.DarkCyan, SKColors.Coral };

        if (totalRevenu > 0 && totalDepense > 0)
        {
            float startAngle = 0f;

            for (int i = 0; i < values.Length; i++)
            {
                float AngleDessine = 360 * (values[i] / total);
                using var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = strokeWidth,
                    Color = colors[i],
                    StrokeCap = SKStrokeCap.Butt,
                    IsAntialias = true
                };

                using var path = new SKPath();
                path.MoveTo(centerX, centerY);
                path.AddArc(new SKRect(centerX - rayon, centerY - rayon, centerX + rayon, centerY + rayon),
                           startAngle, AngleDessine);

                canvas.DrawPath(path, paint);
                startAngle += AngleDessine;
            }
            using var paintInterieur = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.White,
                IsAntialias = true
            };

            canvas.DrawCircle(centerX, centerY, rayonInterieur, paintInterieur);
            using var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 26,
                FakeBoldText = true,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true
            };
            canvas.DrawText(solde.ToString() + " MAD", centerX, centerY, textPaint);

            RevenuLabel.Text = "Revenu - " + (totalRevenu / total * 100).ToString("0.00") + "%";
            DepenseLabel.Text = "Depense - " + (totalDepense / total * 100).ToString("0.00") + "%";
        }
        else if (totalRevenu > 0)
        {
            RevenuLabel.Text = "Revenu - 100%";
            DepenseLabel.Text = "Depense - 0%";
            using var paint = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = SKColors.DarkCyan,
                IsAntialias = true
            };
            using var path = new SKPath();
            canvas.DrawCircle(centerX, centerY, rayon, paint);
        }
        else if (totalDepense > 0)
        {
            DepenseLabel.Text = "Depense - 100%";
            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Coral,
                IsAntialias = true
            };
            using var path = new SKPath();
            canvas.DrawCircle(centerX, centerY, rayon, paint);

        }
        else
        {
            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3,
                Color = SKColors.Gray,
                IsAntialias = true
            };
            using var path = new SKPath();
            canvas.DrawCircle(centerX, centerY, rayon, paint);
            using var textPaint = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = 20,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true
            };
            canvas.DrawText("Aucun Solde", centerX, centerY + 10, textPaint);
        }
    }
}