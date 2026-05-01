namespace VolunTrack.Services
{
    public interface IEmailService
    {
        Task SendReminderAsync(string toEmail, string eventName, DateTime eventDate);
    }
}
