using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceApp.Views;

namespace FinanceApp.ViewModels
{
    public partial class SettingViewModele : ObservableObject
    {
        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string userMail;

        [ObservableProperty]
        static public ImageSource imageButtonSource;

        [ObservableProperty]
        static public bool isDarkModeEnable;

        public SettingViewModele()
        {
            UserName = Preferences.Get("CurrentUser", "USER FULL NAME");
            UserMail = Preferences.Get("Email", "exempledemail@gmail.com");
            IsDarkModeEnable = false;
            var savedImagePath = Preferences.Get("UserImage", string.Empty);
            if (!string.IsNullOrEmpty(savedImagePath) && File.Exists(savedImagePath))
            {
                ImageButtonSource = ImageSource.FromFile(savedImagePath);
            }
        }

        [RelayCommand]
        private async Task OnLogOutButtonClik()
        {
            Preferences.Set("Email", string.Empty);
            Preferences.Set("RememberMe", false);
            Preferences.Set("CurrentUser", string.Empty);

            await Shell.Current.GoToAsync($"//{nameof(LoginViewPage)}");
        }
    }
}