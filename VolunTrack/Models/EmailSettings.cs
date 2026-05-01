namespace VolunTrack.Models
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = "VolunTrack";
        public string AppPassword { get; set; } = string.Empty;
    }
}
