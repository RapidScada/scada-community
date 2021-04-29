using Scada.Data.Models;

namespace Scada.Comm.Devices.Mqtt.Config
{
    /// <summary>
    /// Represents a command that is sent to Server when new topic data is received.
    /// </summary>
    internal class MqttSubCmd : Command
    {
        public string TopicName { get; set; }
        public int IDUser { get; set; }
        public string CmdType { get; set; }
        public int NumCnlCtrl { get; set; }
    }
}
