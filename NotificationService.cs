using Plugin.LocalNotification;


namespace FinanceApp.Services
{
    
    public class NotificationService
    {
        static public bool IsNotificationAllowed = true;
 
        static public async Task ShowNotification(string title, string message)
        {

            if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
            {
                await LocalNotificationCenter.Current.RequestNotificationPermission();
            }
        
        var notification = new NotificationRequest
            {
                Title = title,
                Description = message,
                NotificationId = new Random().Next(1, 1000),
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Now.AddSeconds(10)
                }
            };
            try
            {
                await LocalNotificationCenter.Current.Show(notification);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'envoi de la notification: {ex.Message}");
                return;   
            }
        }

    } 
}

