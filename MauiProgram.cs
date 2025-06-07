using FinanceApp.Models;
using FinanceApp.Services;
using FinanceApp.ViewModels;
using FinanceApp.Views;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace FinanceApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons.ttf", "MaterialIcons");
            });

        builder.Services.AddSingleton<AppDatabase>();
        builder.Services.AddSingleton<EmailService>();
        builder.Services.AddSingleton<RegisterViewModel>();
        builder.Services.AddSingleton<RegisterViewPage>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<Users>();
        builder.Services.AddSingleton<ForgotPasswordViewModel>();
        builder.Services.AddSingleton<ForgotPasswordViewPage>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<LoginViewPage>();
        builder.Services.AddSingleton<DashboardViewModel>();
        builder.Services.AddSingleton<DashboardViewPage>();
        builder.Services.AddSingleton<StatistiquesViewModel>();
        builder.Services.AddSingleton<StatistiquesViewPage>();
        builder.Services.AddSingleton<AddPage>();


#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
