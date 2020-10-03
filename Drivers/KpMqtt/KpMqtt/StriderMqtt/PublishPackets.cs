using System;
using System.Text;

namespace StriderMqtt
{
    public class PublishPacket : IdentifiedPacket
    {
		internal const byte PacketTypeCode = 0x03;

		/// <summary>
		/// Duplicate message flag
		/// </summary>
		public bool DupFlag
		{
			get;
			set;
		}

		/// <summary>
		/// Quality of Service, see `MqttQualityOfService`
		/// </summary>
		public MqttQos QosLevel
		{
			get;
			set;
		}

		/// <summary>
		/// Retain message flag
		/// </summary>
		public bool Retain
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the topic to send the application message.
        /// </summary>
        /// <value>The topic.</value>
        public string Topic
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the Application Message to be sent to the broker.
        /// </summary>
        /// <value>The application message.</value>
        public byte[] Message
		{
			get;
			set;
		}
        
        public PublishPacket()
        {
            this.PacketType = PacketTypeCode;
        }

		internal override void Serialize(PacketWriter writer, MqttProtocolVersion protocolVersion)
        {
			ValidateTopic();

			if (protocolVersion == MqttProtocolVersion.V3_1_1)
			{
				if (this.QosLevel == MqttQos.AtMostOnce && this.PacketId > 0)
				{
					throw new InvalidOperationException("When using QoS 0 (at most once) the PacketId must not be set");
				}
			}

			writer.SetFixedHeader(PacketType, DupFlag, QosLevel, Retain);

			// variable header
			writer.AppendTextField(this.Topic);

			if (this.QosLevel > MqttQos.AtMostOnce)
			{
				writer.AppendIntegerField(this.PacketId);
			}

			writer.Append(this.Message);
        }

		private void ValidateTopic()
		{
			// topic can't contain wildcards
			if ((this.Topic.IndexOf('#') != -1) || (this.Topic.IndexOf('+') != -1))
			{
				throw new MqttProtocolException("Cannot use wildcards when publishing");
			}

			// check topic length
			if ((this.Topic.Length < Packet.MinTopicLength) || (this.Topic.Length > Packet.MaxTopicLength))
			{
				throw new MqttProtocolException("Invalid topic length");
			}
		}

        internal override void Deserialize(PacketReader reader, MqttProtocolVersion protocolVersion)
		{
			this.DupFlag = reader.Dup;
			this.QosLevel = reader.QosLevel;
			this.Retain = reader.Retain;

			this.Topic = reader.ReadTextField();

			if (QosLevel > MqttQos.AtMostOnce)
			{
				this.PacketId = reader.ReadIntegerField();
			}

			this.Message = reader.ReadToEnd();
		}
    }


	public class PubackPacket : IdentifiedPacket
	{
		internal const byte PacketTypeCode = 0x04;

		internal PubackPacket()
		{
			this.PacketType = PacketTypeCode;
		}

		internal override void Serialize(PacketWriter writer, MqttProtocolVersion protocolVersion)
		{
			writer.SetFixedHeader(PacketType);
			writer.AppendIntegerField(PacketId);
		}

		internal override void Deserialize(PacketReader reader, MqttProtocolVersion protocolVersion)
		{
			if (protocolVersion == MqttProtocolVersion.V3_1_1)
			{
				if ((reader.FixedHeaderFirstByte & Packet.PacketFlagsBitMask) != Packet.ZeroedHeaderFlagBits)
				{
					throw new MqttProtocolException("Puback packet received with invalid header flags");
				}
			}

			if (reader.RemainingLength != 2)
			{
				throw new MqttProtocolException("Remaining length of the incoming puback packet is invalid");
			}

			this.PacketId = reader.ReadIntegerField();
		}
	}


	public class PubrecPacket : IdentifiedPacket
	{
		internal const byte PacketTypeCode = 0x05;

		public PubrecPacket()
		{
			this.PacketType = PacketTypeCode;
		}

		internal override void Serialize(PacketWriter writer, MqttProtocolVersion protocolVersion)
		{
			writer.SetFixedHeader(PacketType);
			writer.AppendIntegerField(PacketId);
		}

		internal override void Deserialize(PacketReader reader, MqttProtocolVersion protocolVersion)
		{
			if (protocolVersion == MqttProtocolVersion.V3_1_1)
			{
				if ((reader.FixedHeaderFirstByte & Packet.PacketFlagsBitMask) != Packet.ZeroedHeaderFlagBits)
				{
					throw new MqttProtocolException("Pubrec packet received with invalid header flags");
				}
			}

			if (reader.RemainingLength != 2)
			{
				throw new MqttProtocolException("Remaining length of the incoming pubrec packet is invalid");
			}

			this.PacketId = reader.ReadIntegerField();
		}
	}


	public class PubrelPacket : IdentifiedPacket
	{
		internal const byte PacketTypeCode = 0x06;

		public PubrelPacket()
		{
			this.PacketType = PacketTypeCode;
		}

		internal override void Serialize(PacketWriter writer, MqttProtocolVersion protocolVersion)
		{
			if (protocolVersion == MqttProtocolVersion.V3_1_1)
			{
				writer.SetFixedHeader(PacketType, MqttQos.AtLeastOnce);
			}
			else
			{
				writer.SetFixedHeader(PacketType);
			}

			writer.AppendIntegerField(PacketId);
		}

		internal override void Deserialize(PacketReader reader, MqttProtocolVersion protocolVersion)
		{
			if (protocolVersion == MqttProtocolVersion.V3_1_1)
			{
				if ((reader.FixedHeaderFirstByte & Packet.PacketFlagsBitMask) != Packet.Qos1HeaderFlagBits)
				{
					throw new MqttProtocolException("Pubrel packet received with invalid header flags");
				}
			}

			if (reader.RemainingLength != 2)
			{
				throw new MqttProtocolException("Remaining length of the incoming pubrel packet is invalid");
			}

			this.PacketId = reader.ReadIntegerField();
		}
	}


	public class PubcompPacket : IdentifiedPacket
	{
		internal const byte PacketTypeCode = 0x07;

		public PubcompPacket()
		{
			this.PacketType = PacketTypeCode;
		}

		internal override void Serialize(PacketWriter writer, MqttProtocolVersion protocolVersion)
		{
			writer.SetFixedHeader(PacketType);
			writer.AppendIntegerField(PacketId);
		}

		internal override void Deserialize(PacketReader reader, MqttProtocolVersion protocolVersion)
		{
			if (protocolVersion == MqttProtocolVersion.V3_1_1)
			{
				if ((reader.FixedHeaderFirstByte & Packet.PacketFlagsBitMask) != Packet.ZeroedHeaderFlagBits)
				{
					throw new MqttProtocolException("Pubcomp packet received with invalid header flags");
				}
			}

			if (reader.RemainingLength != 2)
			{
				throw new MqttProtocolException("Remaining length of the incoming pubcomp packet is invalid");
			}

			this.PacketId = reader.ReadIntegerField();
		}
	}

}
