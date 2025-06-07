using FinanceApp.ViewModels;
using FinanceApp.Services;
namespace FinanceApp.Views;

public partial class ProfilSetterViewPage : ContentPage
{
	public ProfilSetterViewPage()
	{
		InitializeComponent();
        this.BindingContext = new ProfilSetterViewModel();

        UserName.Text = Preferences.Get("CurrentUser", "User Full Name");
        UserMail.Text = Preferences.Get("Email", "usercurrentmail@gmail.com");
        UserPhone.Text = Preferences.Get("UserPhoneNumber", "+33 6 00 00 00 00");
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        var savedImagePath = Preferences.Get("UserImage", string.Empty);
        if (!string.IsNullOrEmpty(savedImagePath) && File.Exists(savedImagePath))
        {
            ProfilSetter.Source = ImageSource.FromFile(savedImagePath);
        }
    }
    public async void SaveProfil(object sender, EventArgs e)
	{
        if (string.IsNullOrEmpty(UserName.Text) || string.IsNullOrEmpty(UserMail.Text) || string.IsNullOrEmpty(UserPhone.Text))
        {
            await DisplayAlert("Erreur", "Champs vide", "OK");
            return;
        }

        AppDatabase Database = new();
        var user = await Database.GetUserByEmailAsync(UserMail.Text);
        if (user != null)
        {
            if (!string.IsNullOrEmpty(PassWordText.Text))
            {
                if(PassWordText.Text.Length < 6)
                {
                    PassWordText.Text = string.Empty;
                    PassWordText.Placeholder = "le mot de pass comporte au moins 6 caractères";
                    PassWordText.PlaceholderColor = Colors.Red;
                    PassWordText.TextColor = Color.FromArgb("#333333");
                    return;
                }
                if (PassWordText.Text == ConfirmePassWordText.Text)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(PassWordText.Text);
        
                }
                else
                {
                    await Shell.Current.DisplayAlert("Erreur", "Les mots de passe ne correspondent pas", "OK");
                    return;
                }
            }

            user.Name = UserName.Text;
            user.PhoneNumber = UserPhone.Text;
            user.Email = UserMail.Text;
            user.UpdatedAt = DateTime.Now;

            if (await Database.UpdateUser(user) > 0)
            {
                Preferences.Set("CurrentUser", UserName.Text);
                Preferences.Set("UserPhoneNumber", UserPhone.Text);
                Preferences.Set("Email", UserMail.Text);
                await Shell.Current.DisplayAlert("Succès", "Profil mis à jour avec succès", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Erreur", "Échec de la mise à jour du profil", "OK");
                return;
            }
        }
        else await DisplayAlert("info", "user non non recuperer", "ok");
    }
}