using System;
using System.Collections.Generic;
using System.Linq;

namespace StriderMqtt
{
	public interface IMqttPersistence
	{
		// methods related to incoming messages

		/// <summary>
		/// Stores an incoming message in persistence.
		/// In case of QoS level 2, the packet id is registered in
		/// an incoming inflight set, so duplicates can be avoided.
		/// </summary>
		void RegisterIncomingFlow(ushort packetId);

		/// <summary>
		/// Releases an entry in the incoming inflight set by packet identifier.
		/// Expected to be called when a Pubrel is received.
		/// </summary>
		void ReleaseIncomingFlow(ushort packetId);

		/// <summary>
		/// Determines whether the packet id is registered in the incoming inflight set.
		/// In the case of QoS 2 flow, this method determines wether a duplicate message
		/// should be received (if not in incoming set) or ignored (if present in incoming set).
		/// </summary>
		bool IsIncomingFlowRegistered(ushort packetId);


		// methods related to outgoing messages

		ushort LastOutgoingPacketId { get; set; }

		/// <summary>
		/// In the case of qos 0, registers the message in the published numbers list.
		/// Otherwise the message is stored in the outgoing inflight messages queue.
		/// </summary>
		void RegisterOutgoingFlow(OutgoingFlow outgoingMessage);

		/// <summary>
		/// Gets a message in the outgoing inflight messages queue.
		/// Returns null if there isn't any message in the queue.
		/// </summary>
		/// <returns>The pending outgoing message.</returns>
		IEnumerable<OutgoingFlow> GetPendingOutgoingFlows();

		/// <summary>
		/// Marks the outgoing message (in the outgoing inflight queue) as "received" by the broker.
		/// </summary>
		/// <param name="packetId">Packet identifier.</param>
		void SetOutgoingFlowReceived(ushort packetId);

		/// <summary>
		/// Removes the message from the outgoing inflight queue,
		/// and stores the related number in the published numbers list.
		/// </summary>
		/// <param name="packetId">Packet identifier.</param>
		void SetOutgoingFlowCompleted(ushort packetId);
	}

	public class OutgoingFlow
	{
		/// <summary>
		/// The packetId used for publishing.
		/// </summary>
		public ushort PacketId { get; set; }

		public string Topic { get; set; }

		public MqttQos Qos { get; set; }

		/// <summary>
		/// The payload that will be published.
		/// </summary>
		public byte[] Payload { get; set; }

		/// <summary>
		/// Received Flag, to be used with QoS2.
		/// This flag determines if the `Pubrec` packet was received from broker.
		/// </summary>
		public bool Received { get; set; }
	}


	/// <summary>
	/// In memory persistence.
	/// This persistence support multiple incoming and outgoing messages,
	/// aldough ordering is only guaranteed when only one message is inflight per direction.
	/// </summary>
	public class InMemoryPersistence : IMqttPersistence, IDisposable
	{
		List<ushort> incomingPacketIds = new List<ushort>();
		List<OutgoingFlow> outgoingFlows = new List<OutgoingFlow>();

		public ushort LastOutgoingPacketId { get; set; }

		public InMemoryPersistence()
		{
		}

		public void RegisterIncomingFlow(ushort packetId)
		{
			incomingPacketIds.Add(packetId);
		}

		public void ReleaseIncomingFlow(ushort packetId)
		{
			if (incomingPacketIds.Contains(packetId))
			{
				incomingPacketIds.Remove(packetId);
			}
		}

		public bool IsIncomingFlowRegistered(ushort packetId)
		{
			return incomingPacketIds.Contains(packetId);
		}



		public void RegisterOutgoingFlow(OutgoingFlow outgoingMessage)
		{
			outgoingFlows.Add(outgoingMessage);
		}

		public IEnumerable<OutgoingFlow> GetPendingOutgoingFlows()
		{
			return new List<OutgoingFlow>(outgoingFlows);
		}

		public void SetOutgoingFlowReceived(ushort packetId)
		{
			OutgoingFlow msg = outgoingFlows.FirstOrDefault(m => m.PacketId == packetId);
			if (msg != null)
			{
				msg.Received = true;
			}
		}

		public void SetOutgoingFlowCompleted(ushort packetId)
		{
			outgoingFlows.RemoveAll(m => m.PacketId == packetId);
		}


		public void Dispose()
		{
			incomingPacketIds.Clear();
			outgoingFlows.Clear();
		}
	}
}
