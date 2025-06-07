using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceApp.Services;
using FinanceApp.Views;

namespace FinanceApp.ViewModels
{
    public partial class ForgotPasswordViewModel : BaseViewModel
    {
        private readonly AppDatabase _databaseService;
        private readonly EmailService _emailService;
        private string? _generatedCode;
        private DateTime? _codeExpiration;

        public ForgotPasswordViewModel()
        {
            _databaseService = new();
            _emailService = new();
            IsVerificationCodeEntryVisible = false;
            IsPasswordFieldsVisible = false;
            IsBusy = false; // Initialize IsBusy to false
        }

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string confirmPassword;

        [ObservableProperty]
        private bool isPasswordFieldsVisible = false; // Caché par défaut

        [ObservableProperty]
        private bool isVerificationCodeEntryVisible = false;

        [ObservableProperty]
        private string verificationCode;

        [ObservableProperty]
        private bool isCodeValid;

        [ObservableProperty]
        private bool _isBusy;

        [RelayCommand]
        private async Task VerifyEmail()
        {
            IsBusy = true; // Show loading indicator

            try
            {
                if (string.IsNullOrWhiteSpace(Email))
                {
                    await Shell.Current.DisplayAlert("Erreur", "Veuillez entrer votre email.", "OK");
                    return;
                }

                var user = await _databaseService.GetUserByEmailAsync(Email);
                if (user == null)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Aucun compte associé à cet email.", "OK");
                    return;
                }

                _generatedCode = await _emailService.GenerateVerificationCode();
                _codeExpiration = DateTime.UtcNow.AddMinutes(10);
                await EmailService.SendVerificationEmailAsync(Email, _generatedCode);
                IsVerificationCodeEntryVisible = true; // Show the verification code entry
                await Shell.Current.DisplayAlert("Info", "Un code de vérification a été envoyé à votre email. Veuillez vérifier votre boîte de réception (et votre dossier spam).", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'envoi du code : {ex.Message}");
                await Shell.Current.DisplayAlert("Erreur", "Erreur lors de l'envoi du code de vérification.", "OK");
            }
            finally
            {
                IsBusy = false; // Hide loading indicator (always executed)
            }
        }

        [RelayCommand]
        private async Task VerifyCode()
        {
            IsBusy = true; // Show loading indicator

            try
            {
                if (string.IsNullOrWhiteSpace(VerificationCode))
                {
                    await ShowAlert("Erreur", "Veuillez entrer le code de vérification.");
                    return;
                }

                if (VerificationCode == _generatedCode && DateTime.UtcNow <= _codeExpiration)
                {
                    IsCodeValid = true;
                    IsPasswordFieldsVisible = true; // Show the password fields
                }
                else
                {
                    await ShowAlert("Erreur", "Le code de vérification est incorrect ou a expiré. Veuillez réessayer.");
                    IsCodeValid = false;
                }
            }
            finally
            {
                IsBusy = false; // Hide loading indicator (always executed)
            }
        }

        [RelayCommand]
        private async Task ResetPassword()
        {
            IsBusy = true; // Show loading indicator

            try
            {
                if (!IsCodeValid)
                {
                    await ShowAlert("Erreur", "Veuillez vérifier votre code de confirmation avant de réinitialiser votre mot de passe.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    await Shell.Current.DisplayAlert("Erreur", "Veuillez entrer votre nouveau mot de passe.", "OK");
                    return;
                }
                else if (Password.Length < 6 || ConfirmPassword.Length < 6)
                {
                    await Shell.Current.DisplayAlert("Erreur", "les mots de passe sont inferieurs à 6", "Réessayer");
                    return;
                }

                if (Password != ConfirmPassword)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Les mots de passe ne correspondent pas.", "OK");
                    return;
                }

                var user = await _databaseService.GetUserByEmailAsync(Email);
                if (user == null)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Erreur inattendue, veuillez recommencer.", "OK");
                    return;
                }

                // 🔥 Hash du nouveau mot de passe avant de le mettre à jour
                user.Password = BCrypt.Net.BCrypt.HashPassword(Password);
                await _databaseService.UpdateUserAsync(user);

                //await _emailService.SendEmailAsync(Email, "Réinitialisation du mot de passe", "Votre mot de passe a été modifié avec succès.");

                await Shell.Current.DisplayAlert("Succès", "Votre mot de passe a été réinitialisé.", "OK");

                // Réinitialiser les champs après la réinitialisation
                Email = string.Empty;
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                IsPasswordFieldsVisible = false;
                IsVerificationCodeEntryVisible = false;
                await Shell.Current.GoToAsync($"//{nameof(LoginViewPage)}");
            }
            finally
            {
                IsBusy = false; // Hide loading indicator (always executed)
            }
        }

        [RelayCommand]
        private async Task ClickedButtonBack()
        {
            await Shell.Current.GoToAsync($"//{nameof(LoginViewPage)}");
        }

        private async Task ShowAlert(string title, string message)
        {
            await App.Current.MainPage.DisplayAlert(title, message, "OK");
        }

    }
}
