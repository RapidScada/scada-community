using System;
using System.Text;

namespace StriderMqtt
{
    public class ConnectPacket : PacketBase
    {
		internal const byte PacketTypeCode = 0x01;

        internal const string ProtocolNameV3_1 = "MQIsdp";
        internal const string ProtocolNameV3_1_1 = "MQTT";
        
        // max length for client id (removed in 3.1.1)
        internal const int ClientIdMaxLength = 23;

        internal const ushort KeepAlivePeriodDefault = 60; // seconds

        // connect flags
        internal const byte UsernameFlagOffset = 0x07;
        internal const byte PasswordFlagOffset = 0x06;
        internal const byte WillRetainFlagOffset = 0x05;
        internal const byte WillQosFlagOffset = 0x03;
        internal const byte WillFlagOffset = 0x02;
        internal const byte CleanSessionFlagOffset = 0x01;
        

        public MqttProtocolVersion ProtocolVersion
		{
			get;
			set;
		}

        public string ClientId
		{
			get;
			set;
		}

        public bool WillRetain
		{
			get;
			set;
		}

        
        public MqttQos WillQosLevel
		{
			get;
			set;
		}

        public bool WillFlag
		{
			get;
			set;
		}
        
		public string WillTopic
		{
			get;
			set;
		}
        
		public byte[] WillMessage
		{
			get;
			set;
		}
        
		public string Username
		{
			get;
			set;
		}

        public string Password
		{
			get;
			set;
		}
        
		public bool CleanSession
		{
			get;
			set;
		}
        
		internal ushort KeepAlivePeriod
		{
			get;
			set;
		}
        

        internal ConnectPacket()
        {
            this.PacketType = PacketTypeCode;
        }


        /// <summary>
        /// Reads a Connect packet from the given stream.
		/// (This method should not be used since clients don't receive Connect packets)
        /// </summary>
        /// <param name="fixedHeaderFirstByte">Fixed header first byte previously read</param>
        /// <param name="stream">The stream to read from</param>
        /// <param name="protocolVersion">The protocol version to be used to read</param>
		internal override void Deserialize(PacketReader reader, MqttProtocolVersion protocolVersion)
        {
			throw new MqttProtocolException("Connect packet should not be received");
        }

		/// <summary>
		/// Writes the Connect packet to the given stream and using the given
		/// protocol version.
		/// </summary>
		/// <param name="stream">The stream to write to</param>
		/// <param name="protocolVersion">Protocol to be used to write</param>
		internal override void Serialize(PacketWriter writer, MqttProtocolVersion protocolVersion)
        {
            if (protocolVersion == MqttProtocolVersion.V3_1_1)
            {
                // will flag set, will topic and will message MUST be present
                if (this.WillFlag && (WillMessage == null || String.IsNullOrEmpty(WillTopic)))
				{
					throw new MqttProtocolException("Last will message is invalid");
				}
                // willflag not set, retain must be 0 and will topic and message MUST NOT be present
				else if (!this.WillFlag && (this.WillRetain || WillMessage != null || !String.IsNullOrEmpty(WillTopic)))
				{
					throw new MqttProtocolException("Last will message is invalid");
				}
            }

			if (this.WillFlag && ((this.WillTopic.Length < Packet.MinTopicLength) || (this.WillTopic.Length > Packet.MaxTopicLength)))
			{
				throw new MqttProtocolException("Invalid last will topic length");
			}

			writer.SetFixedHeader(PacketType);

			MakeVariableHeader(writer);
			MakePayload(writer);
        }

		void MakeVariableHeader(PacketWriter w)
		{
			if (this.ProtocolVersion == MqttProtocolVersion.V3_1)
			{
				w.AppendTextField(ProtocolNameV3_1);
			}
			else
			{
				w.AppendTextField(ProtocolNameV3_1_1);
			}

			w.Append((byte)this.ProtocolVersion);

			w.Append(MakeConnectFlags());
			w.AppendIntegerField(KeepAlivePeriod);
		}

		byte MakeConnectFlags()
		{
			byte connectFlags = 0x00;
			connectFlags |= (Username != null) ? (byte)(1 << UsernameFlagOffset) : (byte)0x00;
			connectFlags |= (Password != null) ? (byte)(1 << PasswordFlagOffset) : (byte)0x00;
			connectFlags |= (this.WillRetain) ? (byte)(1 << WillRetainFlagOffset) : (byte)0x00;

			if (this.WillFlag)
				connectFlags |= (byte)((byte)WillQosLevel << WillQosFlagOffset);

			connectFlags |= (this.WillFlag) ? (byte)(1 << WillFlagOffset) : (byte)0x00;
			connectFlags |= (this.CleanSession) ? (byte)(1 << CleanSessionFlagOffset) : (byte)0x00;

			return connectFlags;
		}

		void MakePayload(PacketWriter w)
		{
			w.AppendTextField(ClientId);

			if (!String.IsNullOrEmpty(WillTopic))
			{
				w.AppendTextField(WillTopic);
			}

			if (WillMessage != null)
			{
				w.AppendBytesField(WillMessage);
			}

			if (Username != null)
			{
				w.AppendTextField(Username);
			}

			if (Password != null)
			{
				w.AppendTextField(Password);
			}
		}
    }


	public class ConnackPacket : PacketBase
	{
		internal const byte PacketTypeCode = 0x02;

		private const byte SessionPresentFlag = 0x01;

		public bool SessionPresent {
			get;
			private set;
		}

		public ConnackReturnCode ReturnCode {
			get;
			set;
		}

		internal ConnackPacket()
		{
			this.PacketType = PacketTypeCode;
		}

		internal override void Serialize(PacketWriter writer, MqttProtocolVersion protocolVersion)
		{
			throw new MqttProtocolException("Clients should not send connack packets");
		}

		internal override void Deserialize(PacketReader reader, MqttProtocolVersion protocolVersion)
		{
			if (protocolVersion == MqttProtocolVersion.V3_1_1)
			{
				if ((reader.FixedHeaderFirstByte & Packet.PacketFlagsBitMask) != Packet.ZeroedHeaderFlagBits)
				{
					throw new MqttProtocolException("Connack packet received with invalid header flags");
				}
			}

			if (reader.RemainingLength != 2)
			{
				throw new MqttProtocolException("Connack packet received with invalid remaining length");
			}

			this.SessionPresent = (reader.ReadByte() & SessionPresentFlag) > 0;
			this.ReturnCode = (ConnackReturnCode)reader.ReadByte();
		}
	}


	internal class PingreqPacket : PacketBase
	{
		internal const byte PacketTypeCode = 0x0C;
		internal const byte PingreqFlagBits = 0x00;

		internal PingreqPacket()
		{
			this.PacketType = PacketTypeCode;
		}

		internal override void Serialize(PacketWriter writer, MqttProtocolVersion protocolVersion)
		{
			writer.SetFixedHeader(this.PacketType);
		}

		internal override void Deserialize(PacketReader reader, MqttProtocolVersion protocolVersion)
		{
			throw new MqttProtocolException("Pingreq packet should not be received");
		}
	}


	internal class PingrespPacket : PacketBase
	{
		internal const byte PacketTypeCode = 0x0D;

		internal PingrespPacket()
		{
			this.PacketType = PacketTypeCode;
		}

		internal override void Serialize(PacketWriter writer, MqttProtocolVersion protocolVersion)
		{
			throw new MqttProtocolException("Clients should not send pingresp packets");
		}

		internal override void Deserialize(PacketReader reader, MqttProtocolVersion protocolVersion)
		{
			if (protocolVersion == MqttProtocolVersion.V3_1_1)
			{
				if ((reader.FixedHeaderFirstByte & Packet.PacketFlagsBitMask) != Packet.ZeroedHeaderFlagBits)
				{
					throw new MqttProtocolException("Pingresp packet received with invalid header flags");
				}
			}

			if (reader.RemainingLength != 0)
			{
				throw new MqttProtocolException("Pingresp packet received with invalid remaining length");
			}
		}
	}


	internal class DisconnectPacket : PacketBase
	{
		internal const byte PacketTypeCode = 0x0E;

		internal DisconnectPacket()
		{
			this.PacketType = PacketTypeCode;
		}

		internal override void Deserialize(PacketReader reader, MqttProtocolVersion protocolVersion)
		{
			throw new MqttProtocolException("Disconnect packet should not be received");
		}

		internal override void Serialize(PacketWriter writer, MqttProtocolVersion protocolVersion)
		{
			writer.SetFixedHeader(PacketType);
		}
	}
}
