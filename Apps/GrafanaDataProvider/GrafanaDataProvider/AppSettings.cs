using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scada;

namespace GrafanaDataProvider
{
    public class AppSettings
    {
        /// <summary>
        /// Get or set server
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// Get or set port
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// Get or set user
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// Get or set password
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Get or set timeout
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// Set default settings
        /// </summary>
        public void SetToDefault()
        {
            Server = "localhost";
            Port = 3306;
            User = "";
            Password = "";
            TimeOut = 1000;
        }

        public AppSettings()
        {
            SetToDefault();
        }

        /// <summary>
        /// Get parameter from collection.
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
        /// Download settings from Web.config.
        /// </summary>
        public bool Load(out string errMsg)
        {
            try
            {
                SetToDefault();
                NameValueCollection settings = ConfigurationManager.AppSettings;

                Server = GetParameter(settings, "serverHost");
                Port = Convert.ToInt32(GetParameter(settings, "serverPort"));
                User = GetParameter(settings, "serverUser");
                Password = ScadaUtils.Decrypt(GetParameter(settings, "Password"));
                TimeOut = Convert.ToInt32(GetParameter(settings, "serverTimeout"));

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