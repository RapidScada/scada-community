using System.Web.Http;
using Scada;
using GrafanaDataProvider.Models;

namespace GrafanaDataProvider.Controllers
{
    /// <summary>
    /// Represents a controller for password encryption.
    /// </summary>
    public class PasswordsController : ApiController
    {
        [HttpGet]
        [Route("api/passwords/{password}")]
        public Password GetEncryptor(string password)
        {
            return new Password
            {
                password = password,
                encryptedPassword = ScadaUtils.Encrypt(password)
            };
        }
    }
}
