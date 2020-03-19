using System;
using System.Collections.Specialized;
using System.Configuration;
using Scada;
using Scada.Client;

namespace GrafanaDataProvider
{
    /// <summary>
    /// Represents application settings.
    /// </summary>
    public class AppSettings : CommSettings
    {

        /// <summary>
        /// Initializes connection parameters. 
        /// </summary>
        public AppSettings()
            : base()
        {

        }

        /// <summary>
        /// Gets the specified parameter from the collection.
        /// </summary>
        private string GetParameter(NameValueCollection settings, string paramName)
        {
            try
            {
                return settings[paramName];
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error retrieving parameter \"{0}\": {1}",
                    paramName, ex.Message));
            }
        }

        /// <summary>
        /// Loads settings from Web.config.
        /// </summary>
        public bool Load(out string errMsg)
        {
            try
            {
                SetToDefault();
                NameValueCollection settings = ConfigurationManager.AppSettings;                
                ServerHost = GetParameter(settings, "serverHost");
                ServerPort = Convert.ToInt32(GetParameter(settings, "serverPort"));
                ServerUser = GetParameter(settings, "serverUser");
                ServerPwd = ScadaUtils.Decrypt(GetParameter(settings, "Password"));
                ServerTimeout = Convert.ToInt32(GetParameter(settings, "serverTimeout"));
                                
                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = "Error loading application settings: " + ex.Message;
                return false;
            }
        }
    }
}