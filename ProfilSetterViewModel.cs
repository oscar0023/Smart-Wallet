using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace FinanceApp.ViewModels
{
    public partial class ProfilSetterViewModel : ObservableObject
    {
        [ObservableProperty]
        private string userMail;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string userPhone;

        [ObservableProperty]
        private string userNewPassWord = string.Empty;

        [ObservableProperty]
        private string userConfirmePassWord = string.Empty;

        [ObservableProperty]
        private bool isPasswordHidden = true;

        [ObservableProperty]
        private ImageSource passWordVisibility = "password_eye_off.png";

        [ObservableProperty]
        private ImageSource imageButtonSource = "";


        public ProfilSetterViewModel()
        {

            UserName = Preferences.Get("CurrentUser", "User Full Name");
            UserMail = Preferences.Get("Email", "usercurrentmail@gmail.com");
            UserPhone = Preferences.Get("UserPhoneNumber", "+33 6 00 00 00 00");
            var savedImagePath = Preferences.Get("UserImage", "person.png");
            if (!string.IsNullOrEmpty(savedImagePath) && File.Exists(savedImagePath))
            {
                var stream = File.OpenRead(savedImagePath);
                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                ImageButtonSource = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
            }
            else ImageButtonSource = "person.png";
        }


        [RelayCommand]
        private void IsPasswordVisible()
        {
            IsPasswordHidden = !isPasswordHidden;
            PassWordVisibility = IsPasswordHidden ? "password_eye_off.png" : "password_eye_on.png";
        }

        [RelayCommand]
        private async Task LoadProfilImage()
        {
            var fileName = "user_profil_file.jpg";
            var NewFileName = "user_profil_file2.jpg";
            var destinationPath = Path.Combine(FileSystem.AppDataDirectory, fileName);


            try
            {
                var imageHandler = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Choisir une autre image",
                    FileTypes = FilePickerFileType.Images
                });

                if (imageHandler != null)
                {
                    using var stream = await imageHandler.OpenReadAsync();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();

            
                    ImageButtonSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));

                    if (File.Exists(destinationPath))
                    {
                        
                        try
                        {
                            File.Delete(destinationPath);
                        }
                        
                        catch (Exception ex) 
                        {
                            var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                            if (mainPage != null)
                            {
                                await mainPage.DisplayAlert("Erreur", ex.Message, "OK");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Erreur: {ex.Message}");
                            }
                        }
                        destinationPath = Path.Combine(FileSystem.AppDataDirectory, NewFileName);
                    }
                    File.WriteAllBytes(destinationPath, imageBytes);
                    Preferences.Set("UserImage", destinationPath);
               
                }
            }
            catch (Exception ex)
            {
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlert("Erreur", ex.Message, "OK");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur: {ex.Message}");
                }
            }
        }
    }
} 
