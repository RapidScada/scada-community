using System;
using System.Text;

namespace StriderMqtt
{
    /// <summary>
    /// Base class for all MQTT messages
    /// </summary>
    public abstract class PacketBase
    {       
        /// <summary>
        /// Packet type
        /// </summary>
        public byte PacketType
        {
			get;
			protected set;
        }
			

		/// <summary>
		/// Writes the packet to the stream
		/// </summary>
		/// <param name="stream">The stream to write to</param>
		/// <param name="protocolVersion">Protocol to be used while reading</param>
		internal abstract void Serialize(PacketWriter writer, MqttProtocolVersion protocolVersion);

		/// <summary>
		/// Reads a packet from the stream
		/// </summary>
		/// <param name="fixedHeaderFirstByte">Fixed header first byte previously read</param>
		/// <param name="stream">The stream to read from</param>
		/// <param name="protocolVersion">The protocol version to be used while reading</param>
		internal abstract void Deserialize(PacketReader reader, MqttProtocolVersion protocolVersion);

    }


	public abstract class IdentifiedPacket : PacketBase
	{
		/// <summary>
		/// Packet identifier
		/// </summary>
		public ushort PacketId
		{
			get;
			set;
		}
	}

}
