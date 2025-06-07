namespace FinanceApp.Views;
using FinanceApp.Services;
using FinanceApp.ViewModels;

public partial class SettingViewPage : ContentPage
{
    public SettingViewPage()
    {
        InitializeComponent();
        this.BindingContext = new SettingViewModele();
        var savedImagePath = Preferences.Get("UserImage", "person.png");
        if (!string.IsNullOrEmpty(savedImagePath) && File.Exists(savedImagePath))
        {
            var stream = File.OpenRead(savedImagePath);
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            SettingViewModele.imageButtonSource = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var savedImagePath = Preferences.Get("UserImage", "person.png");
        if (!string.IsNullOrEmpty(savedImagePath) && File.Exists(savedImagePath))
        {
            var stream = File.OpenRead(savedImagePath);
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            SettingViewModele.imageButtonSource = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
        }
    }

    private void NotificationsControl(object sender, ToggledEventArgs e)
    {
        if(e.Value)
        {
            NotificationService.IsNotificationAllowed = true;
        }
        else
            NotificationService.IsNotificationAllowed = false;
    }
    public void ThemeHandler(object sender, ToggledEventArgs e)
    {
        if (e.Value & Application.Current != null)
        {
            Application.Current.UserAppTheme = AppTheme.Dark;
            SettingViewModele.isDarkModeEnable = true;
        }
        else
        {
            Application.Current.UserAppTheme = AppTheme.Light;
            SettingViewModele.isDarkModeEnable = false;
        }
    }

    private async void GoToProfilPage(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new ProfilSetterViewPage());
    }
    private async void LoadAppInfos(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new AppInfosViewPage());
    }
    private async void OnLogOutButtonClik(object sender, TappedEventArgs e)
    {
        string command = await DisplayActionSheet("Voulez-vous vraiment vous deconnecter ?", "Annuler",null, "Deconnexion" );
        switch (command)
        {
            case "Deconnexion":
                Preferences.Set("Email", string.Empty);
                Preferences.Set("RememberMe", false);
                Preferences.Set("CurrentUser", string.Empty);

                await Shell.Current.GoToAsync($"//{nameof(LoginViewPage)}");
                break;
            case "annuler":
            
                break;
            default:
            break;
        }
        

    }
}
