using System;

namespace StriderMqtt
{
	public enum MqttProtocolVersion : byte
	{
		V3_1 = 0x03,
		V3_1_1 = 0x04
	}

	public enum MqttQos : byte
	{
		AtMostOnce = 0x00,
		AtLeastOnce = 0x01,
		ExactlyOnce = 0x02
	}

	public class Packet
	{
		internal const byte PacketTypeOffset = 0x04;
		internal const byte PacketTypeMask = 0xF0;

		internal const byte DupFlagOffset = 0x03;
		internal const byte DupFlagMask = 0x08;

		internal const byte RetainFlagOffset = 0x00;
		internal const byte RetainFlagMask = 0x01;

		internal const byte QosLevelOffset = 0x01;
		internal const byte QosLevelMask = 0x06;

		internal const byte PacketFlagsBitMask = 0x0F;
		internal const byte ZeroedHeaderFlagBits = 0x00;
		internal const byte Qos1HeaderFlagBits = 0x02;

		public const uint MaxRemainingLength = 268435455;

		public const uint MaxPacketId = 65535;

		public const ushort MinTopicLength = 1;
		public const ushort MaxTopicLength = 65535;
	}

	public enum ConnackReturnCode : byte
	{
		Accepted = 0x00,
		UnacceptableProtocol = 0x01,
		IdentifierRejected = 0x02,
		BrokerUnavailable = 0x03,
		BadUsernameOrPassword = 0x04,
		NotAuthorized = 0x05
	}

	public enum SubackReturnCode : byte
	{
		AtMostOnceGranted = 0x00,
		AtLeastOnceGranted = 0x01,
		ExactlyOnceGranted = 0x02,
		SubscriptionFailed = 0x80
	}
}
