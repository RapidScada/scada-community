namespace GrafanaDataProvider.Models
{
    /// <summary>
    /// Represents password to connect to the server.
    /// </summary>
    public class Password
    {
        public string password { get; set; }
        public string encryptedPassword { get; set; }
    }
}