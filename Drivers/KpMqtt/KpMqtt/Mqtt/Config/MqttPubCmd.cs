namespace Scada.Comm.Devices.Mqtt.Config
{
    /// <summary>
    /// Represents a command that publishes a topic when a telecommand is sent.
    /// </summary>
    internal class MqttPubCmd : MqttPubParam
    {
        public int NumCmd { get; set; }
    }
}
