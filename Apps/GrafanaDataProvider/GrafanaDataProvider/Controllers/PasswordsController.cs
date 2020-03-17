using System.Web.Http;
using Scada;
using GrafanaDataProvider.Models;

namespace GrafanaDataProvider.Controllers
{
    public class PasswordsController : ApiController
    {
        [HttpGet]
        [Route("api/passwords/{password}")]
        public Encryptor GetEncryptor(string password)
        {
            Encryptor encryptor = new Encryptor
            {
                encryptorString = ScadaUtils.Encrypt(password)
            };
            return encryptor;
        }
    }
}
