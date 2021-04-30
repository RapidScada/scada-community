using System.IO;

namespace Scada.Comm.Devices.Mqtt.Config
{
    /// <summary>
    /// Represents a subscription to a topic that is processed by Java Script.
    /// </summary>
    internal class MqttSubJS : MqttSubTopic
    {
        public string JSHandlerPath { get; set; }
        public string JSHandler { get; private set; }
        public int CnlCnt { get; set; }

        public bool LoadJSHandler()
        {
            if (string.IsNullOrEmpty(JSHandlerPath))
            {
                return false;
            }
            else
            {
                using (StreamReader reader = new StreamReader(JSHandlerPath))
                {
                    JSHandler = reader.ReadToEnd();
                }

                return true;
            }
        }
    }
}
