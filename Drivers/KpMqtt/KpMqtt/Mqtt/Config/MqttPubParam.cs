namespace Scada.Comm.Devices.Mqtt.Config
{
    /// <summary>
    /// Represents an item that can be published.
    /// </summary>
    internal abstract class MqttPubParam : MqttTopic
    {
        /// <summary>
        /// If a broker receives a message on a topic for which there are no current subscribers, 
        /// the broker discards the message unless the publisher of the message designated the message as a retained message.
        /// The broker stores the last retained message and the corresponding QoS for the selected topic.
        /// </summary>
        public bool Retain { get; set; }
    }
}
