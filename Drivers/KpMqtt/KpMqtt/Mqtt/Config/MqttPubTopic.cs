namespace Scada.Comm.Devices.Mqtt.Config
{
    /// <summary>
    /// Represents a topic to publish.
    /// </summary>
    internal class MqttPubTopic : MqttPubParam
    {
        public int NumCnl { get; set; }
        public PubBehavior PubBehavior { get; set; }
        public string DecimalSeparator { get; set; }
        public double Value { get; set; }
        public bool IsPub { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
    }
}
