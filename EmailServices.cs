using System.Net;
using System.Net.Mail;

namespace FinanceApp.Services
{
    public class EmailService
    {
        private static readonly Random RandomGenerator = new();
        private string _verificationCode;
        private DateTime _expirationTime;
        private const string SmtpServer = "smtp.gmail.com"; // Exemple pour Gmail
        private const int SmtpPort = 587; // Port standard pour TLS
        public async Task<string> GenerateVerificationCode()
        {
            _verificationCode = RandomGenerator.Next(100000, 999999).ToString();
            _expirationTime = DateTime.UtcNow.AddMinutes(10); // Code expire en 10 minutes
            return _verificationCode;
        }
        public bool IsCodeValid(string inputCode)
        {
            return inputCode == _verificationCode && DateTime.UtcNow <= _expirationTime;
        }
        //Méthode pour envoyer l'email de vérification

        public async static Task SendVerificationEmailAsync(string recipientEmail, string code)
        {
            try
            {
                // Configuration du serveur SMTP
                string smtpServer = SmtpServer; // Exemple pour Gmail
                int smtpPort = SmtpPort; // Port standard pour TLS
                string smtpUsername = "mamadysylla011@gmail.com";
                string smtpPassword = "oxyt alhh eqch izaq"; // mon mot de passe d'application de Gmail generer automatiquement)
                // Créer le client SMTP
                using var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true // Utiliser SSL/TLS
                };
                // Créer le message email
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUsername),
                    Subject = "Code de Confirmation",
                    Body = $"Votre code de confirmation est : {code}. Il expire dans 10 minutes.",
                    IsBodyHtml = false
                };
                mailMessage.To.Add(recipientEmail);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur d'envoi de l'email (SMTP): {ex.Message}");
                throw;
            }

        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public async Task SendEmailAsync(string email, string v1, string v2)
        {
            // Implémentez la logique d'envoi d'email ici
            string verificationCode = await GenerateVerificationCode();
            await SendVerificationEmailAsync(email, verificationCode);

        }
    }
}
