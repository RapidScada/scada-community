namespace GrafanaDataProvider.Models
{
    /// <summary>
    /// Password to connect to the server
    /// </summary>
    public class Password
    {
        public string password { get; set; }
        public string encryptedPassword { get; set; }
    }
}