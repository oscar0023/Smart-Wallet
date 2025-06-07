using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceApp.Views;
using FinanceApp.Services;

namespace FinanceApp.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private AppDatabase _databaseService;

        public LoginViewModel()
        {
            _databaseService = new AppDatabase();
            Email = Preferences.Get("Email", string.Empty);
            Password = string.Empty;
            Message = string.Empty;
            RememberMe = Preferences.Get("RememberMe", false);
            IsBusy = false;


            if (RememberMe)
                Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            Email = string.Empty;
            Password = string.Empty;
        }

        // Updated to use partial properties for AOT compatibility
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string passwordIcon = "password_eye_off.png";

        [ObservableProperty]
        public bool isPasswordHidden = true;

        [ObservableProperty]
        private string message;

        [ObservableProperty]
        private bool rememberMe;

        [ObservableProperty]
        private bool isBusy; // Added IsBusy property

        [RelayCommand]
        private Task IsPasswordVisible()
        { 
            IsPasswordHidden = !isPasswordHidden;
            PasswordIcon = IsPasswordHidden ? "password_eye_off.png" : "password_eye_on.png";

            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ResetPassword()
        {
            await Shell.Current.GoToAsync($"//{nameof(ForgotPasswordViewPage)}");

            Email = string.Empty;
            Password = string.Empty;
        }

        [RelayCommand]
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlert("Erreur", "Veuillez remplir tous les champs.", "OK");
                }
                return;
            }

            IsBusy = true; // Show loading overlay

            try
            {
                var user = await _databaseService.GetUserByEmailAndPasswordAsync(Email, Password);

                if (user != null)
                {
                    Preferences.Set("Email", Email);
                    Preferences.Set("Password", Password);
                    Preferences.Set("RememberMe", RememberMe);
                    Preferences.Set("CurrentUser", user.Name);
                    Preferences.Set("UserPhoneNumber", user.PhoneNumber);

                    var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                    if (mainPage != null)
                    {
                        await mainPage.DisplayAlert("Succès", $"Connexion réussie bienvenue sur votre Smart wallet {user.Name} !", "OK");
                    }
                    await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                    Email = string.Empty;
                    Password = string.Empty;
                }
                else
                {
                    var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                    if (mainPage != null)
                    {
                        await mainPage.DisplayAlert("Erreur", "Nom d'utilisateur ou mot de passe incorrect.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlert("Erreur", "Une erreur est survenue lors de la connexion.", "OK");
                }
                System.Diagnostics.Debug.WriteLine($"Erreur Login: {ex.Message}");
            }
            finally
            {
                IsBusy = false; // Hide loading overlay (always executed)
            }
        }

        [RelayCommand]
        private async Task CreateNewAccount()
        {
            await Shell.Current.GoToAsync($"///{nameof(RegisterViewPage)}");
        }
    }
}
