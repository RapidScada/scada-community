using System;

namespace StriderMqtt
{
	public class PublishReceivedEventArgs : EventArgs
	{
		public PublishPacket Packet {
			get;
			private set;
		}

		internal PublishReceivedEventArgs(PublishPacket packet) : base()
		{
			this.Packet = packet;
		}
	}

	public class IdentifiedPacketEventArgs : EventArgs
	{
		public ushort PacketId
		{
			get;
			private set;
		}

		internal IdentifiedPacketEventArgs(IdentifiedPacket packet)
		{
			this.PacketId = packet.PacketId;
		}
	}

	public class SubackReceivedEventArgs : EventArgs
	{
		public SubackReturnCode[] GrantedQosLevels
		{
			get;
			private set;
		}

		internal SubackReceivedEventArgs(SubackPacket packet)
		{
			this.GrantedQosLevels = packet.GrantedQosLevels;
		}
	}
}

