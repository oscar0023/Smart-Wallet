using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceApp.Models;
using FinanceApp.Views;
using System.Text.RegularExpressions;
using FinanceApp.Services;



namespace FinanceApp.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly AppDatabase databaseService;
        private readonly EmailService emailService;
        private string _generatedCode;
        private DateTime _codeExpiration;

        public RegisterViewModel()
        {
            databaseService = new AppDatabase();
            emailService = new EmailService();
            IsCodeSent = false;
            IsVerificationCodeEntryVisible = false; // Initially hide the verification code entry
            IsBusy = false; // Initialize IsBusy to false
        }


        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string passwordConfirm;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string phoneNumber;


        [ObservableProperty]
        private string verificationCode;

        [ObservableProperty]
        private bool isCodeSent;

        [ObservableProperty]
        private bool isVerificationSuccessful;

        [ObservableProperty]
        private bool isVerificationCodeEntryVisible;

        [ObservableProperty]
        private bool _isBusy; // Added IsBusy property



        [RelayCommand]
        private async Task ValidateEmailAndPassword()
        {
            IsBusy = true; // Show loading indicator

            try
            {
                // 1. Vérification de l'email
                if (string.IsNullOrWhiteSpace(Email))
                {
                    await ShowAlert("Erreur", "Veuillez entrer un email valide.");
                    return;
                }

                if (!IsValidEmail(Email))
                {
                    await ShowAlert("Erreur", "L'adresse email n'est pas valide.");
                    return;
                }

                // Vérifier si l'utilisateur existe déjà
                var existingUser = await databaseService.GetUserByEmailAsync(Email);
                if (existingUser != null)
                {
                    await ShowAlert("Erreur", "Ce nom d'utilisateur (email) existe déjà !");
                    return;
                }

                // 2. Vérification du mot de passe
                if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(PasswordConfirm))
                {
                    await ShowAlert("Erreur", "Veuillez entrer un mot de passe.");
                    return;
                }

                if (Password?.Length < 6)
                {
                    await ShowAlert("Erreur", "Les mots de passe doivent contenir au moins 6 caractères.");
                    return;
                }

                if (Password != PasswordConfirm)
                {
                    await ShowAlert("Erreur", "Les mots de passe ne correspondent pas !");
                    return;
                }

                // 3. Envoi du code de confirmation
                try
                {
                    _generatedCode = await emailService.GenerateVerificationCode();
                    _codeExpiration = DateTime.UtcNow.AddMinutes(10);
                    await EmailService.SendVerificationEmailAsync(Email, _generatedCode);
                    IsCodeSent = true;
                    IsVerificationCodeEntryVisible = true; // Show the verification code entry
                    await ShowAlert("Info", "Un code de vérification a été envoyé à votre email. Veuillez vérifier votre boîte de réception (et votre dossier spam).");
                }
                catch (Exception ex)
                {
                    await ShowAlert("Erreur", "Échec de l'envoi de l'email. Veuillez réessayer plus tard.");
                    System.Diagnostics.Debug.WriteLine($"Erreur SendVerificationCodeAsync: {ex.Message}");
                }
            }
            finally
            {
                IsBusy = false; // Masquer l'indicateur du chargement
            }
        }


        [RelayCommand]
        private async Task VerifyCode()
        {
            IsBusy = true; // Show loading indicator

            try
            {
                // 4. Vérification du code de confirmation
                if (string.IsNullOrWhiteSpace(VerificationCode))
                {
                    await ShowAlert("Erreur", "Veuillez entrer le code de vérification.");
                    return;
                }

                if (VerificationCode == _generatedCode && DateTime.UtcNow <= _codeExpiration)
                {
                    IsVerificationSuccessful = true;
                    await CreateNewAccount(); // Proceed to create the account
                }
                else
                {
                    await ShowAlert("Erreur", "Le code de vérification est incorrect ou a expiré. Veuillez réessayer.");
                    IsVerificationSuccessful = false;
                }
            }
            finally
            {
                IsBusy = false; // Hide loading indicator (always executed)
            }
        }



        private async Task CreateNewAccount()
        {
            IsBusy = true; // Show loading indicator

            try
            {
                if (!IsVerificationSuccessful)
                {
                    // This check is redundant, but good for safety
                    await ShowAlert("Erreur", "Veuillez vérifier votre code de confirmation avant de créer un compte.");
                    return;
                }
                // 5. Création du compte et insertion de l'utilisateur dans la base de données
                var newUser = new Users
                {
                    Email = Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(Password),
                    Name = Name,
                    PhoneNumber = PhoneNumber,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                // Insérer l'utilisateur dans la base de données
                await databaseService.InsertUserAsync(newUser);
                await ShowAlert("Succès", "Votre compte a été créé avec succès !");
                await Shell.Current.GoToAsync($"//{nameof(LoginViewPage)}");



                Email = string.Empty;
                Password = string.Empty;
                PasswordConfirm = string.Empty;
                Name = string.Empty;
                PhoneNumber = string.Empty;
                VerificationCode = string.Empty;
                IsCodeSent = false;
                IsVerificationCodeEntryVisible = false; // Hide the verification code entry
                IsVerificationSuccessful = false;

            }
            catch (Exception ex)
            {
                await ShowAlert("Erreur", "Échec de la création du compte. Veuillez réessayer plus tard.");
                System.Diagnostics.Debug.WriteLine($"Erreur CreateNewAccount: {ex.Message}");
            }
            finally
            {
                IsBusy = false; // Hide loading indicator (always executed)
            }
        }

        // 1. Vérification de l'email

        private bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        [RelayCommand]
        private async Task ButtonBack()
        {
            Shell.Current.GoToAsync($"//{nameof(LoginViewPage)}");
        }

        private async Task ShowAlert(string title, string message)
        {
            await App.Current.MainPage.DisplayAlert(title, message, "OK");
        }
    }
}
