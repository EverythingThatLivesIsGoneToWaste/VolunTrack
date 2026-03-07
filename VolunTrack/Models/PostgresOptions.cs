using Npgsql;

namespace VolunTrack.Models
{
    public class PostgresOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 5432;
        public string Password { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string SslMode { get; set; } = "Prefer";
        public bool Pooling { get; set; } = true;
        public bool IncludeErrorDetail { get; set; } = false;

        public string BuildConnectionString()
        {
            return new NpgsqlConnectionStringBuilder
            {
                Host = Host,
                Port = Port,
                Password = Password,
                Username = Username,
                Database = Database,
                SslMode = Enum.Parse<SslMode>(SslMode, true),
                Pooling = Pooling
            }.ToString();
        }
    }
}
