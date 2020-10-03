using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace StriderMqtt
{
	public class SubscribePacket : IdentifiedPacket
    {
		internal const byte PacketTypeCode = 0x08;

		private const byte QosPartMask = 0x03;

        /// <summary>
        /// List of topics to subscribe
        /// </summary>
        public string[] Topics
        {
			get;
			set;
        }

        /// <summary>
        /// List of QOS Levels related to topics
        /// </summary>
        public MqttQos[] QosLevels
        {
			get;
			set;
        }


        public SubscribePacket()
        {
            this.PacketType = PacketTypeCode;
        }

		internal override void Serialize(PacketWriter writer, MqttProtocolVersion protocolVersion)
		{
			if (Topics.Length != QosLevels.Length) {
				throw new InvalidOperationException("The length of Topics should match the length of QosLevels");
			}

			if (protocolVersion == MqttProtocolVersion.V3_1_1)
			{
				writer.SetFixedHeader(PacketType, MqttQos.AtLeastOnce);
			}
			else
			{
				writer.SetFixedHeader(PacketType);
			}

			writer.AppendIntegerField(PacketId);

			for (int i = 0; i < Topics.Length; i++)
			{
				if (String.IsNullOrEmpty(this.Topics[i]) || this.Topics[i].Length > Packet.MaxTopicLength)
				{
					throw new InvalidOperationException("Invalid topic length");
				}

				writer.AppendTextField(this.Topics[i]);
				writer.Append((byte)(((byte)this.QosLevels[i]) & QosPartMask));
			}
		}

		internal override void Deserialize(PacketReader reader, MqttProtocolVersion protocolVersion)
		{
			throw new MqttProtocolException("Clients should not receive subscribe packets");
		}
    }


	public class SubackPacket : IdentifiedPacket
	{
		internal const byte PacketTypeCode = 0x09;

		public SubackReturnCode[] GrantedQosLevels
		{
			get;
			internal set;
		}

		internal SubackPacket()
		{
			this.PacketType = PacketTypeCode;
		}


		internal override void Serialize(PacketWriter writer, MqttProtocolVersion protocolVersion)
		{
			throw new MqttProtocolException("Clients should not send unsuback packets");
		}

		internal override void Deserialize(PacketReader reader, MqttProtocolVersion protocolVersion)
		{
			if (protocolVersion == MqttProtocolVersion.V3_1_1)
			{
				if ((reader.FixedHeaderFirstByte & Packet.PacketFlagsBitMask) != Packet.ZeroedHeaderFlagBits)
				{
					throw new MqttProtocolException("Unsuback packet received with invalid header flags");
				}
			}

			this.PacketId = reader.ReadIntegerField();

			var bytes = reader.ReadToEnd();
			this.GrantedQosLevels = new SubackReturnCode[bytes.Length];

			for (int i = 0; i < bytes.Length; i++)
			{
				if (bytes[i] > (byte)SubackReturnCode.ExactlyOnceGranted && bytes[i] != (byte)SubackReturnCode.SubscriptionFailed)
				{
					throw new MqttProtocolException(String.Format("Invalid qos level '{0}' received from broker", bytes[i]));
				}

				this.GrantedQosLevels[i] = (SubackReturnCode)bytes[i];
			}
		}
	}


	public class UnsubscribePacket : IdentifiedPacket
	{
		internal const byte PacketTypeCode = 0x0A;

		public string[] Topics
		{
			get;
			set;
		}

		public UnsubscribePacket()
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

			foreach (string topic in this.Topics)
			{
				writer.AppendTextField(topic);
			}
		}

		internal override void Deserialize(PacketReader reader, MqttProtocolVersion protocolVersion)
		{
			throw new MqttProtocolException("Clients should not send unsubscribe packets");
		}
	}


	public class UnsubackPacket : IdentifiedPacket
	{
		internal const byte PacketTypeCode = 0x0B;

		public UnsubackPacket()
		{
			this.PacketType = PacketTypeCode;
		}

		internal override void Serialize(PacketWriter writer, MqttProtocolVersion protocolVersion)
		{
			throw new MqttProtocolException("Clients should not send unsuback packets");
		}

		internal override void Deserialize(PacketReader reader, MqttProtocolVersion protocolVersion)
		{
			if (protocolVersion == MqttProtocolVersion.V3_1_1)
			{
				if ((reader.FixedHeaderFirstByte & Packet.PacketFlagsBitMask) != Packet.ZeroedHeaderFlagBits)
				{
					throw new MqttProtocolException("Unsuback packet received with invalid header flags");
				}
			}

			if (reader.RemainingLength != 2)
			{
				throw new MqttProtocolException("Unsuback packet received with invalid remaining length");
			}

			this.PacketId = reader.ReadIntegerField();
		}
	}
}
