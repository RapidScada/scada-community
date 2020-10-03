using System;
using System.IO;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Collections.Generic;

namespace StriderMqtt
{
	public interface IMqttTransport
	{
		Stream Stream { get; }
		bool IsClosed { get; }

		void Close();

		bool Poll(int pollLimit);
		void SetTimeouts(TimeSpan readTimeout, TimeSpan writeTimeout);

		MqttProtocolVersion Version { get; set; }

		PacketBase Read();
		void Write(PacketBase packet);
	}

	internal abstract class BaseTransport : IMqttTransport
	{
		abstract public Stream Stream { get; }
		abstract public bool IsClosed { get; }

		abstract public void Close();

		abstract public bool Poll(int pollLimit);
		abstract public void SetTimeouts(TimeSpan readTimeout, TimeSpan writeTimeout);

		public MqttProtocolVersion Version { get; set; }

		public PacketBase Read()
		{
			var reader = new PacketReader(this.Stream);

			PacketBase packet = PacketFactory.GetInstance(reader.PacketTypeCode);
			packet.Deserialize(reader, this.Version);

			return packet;
		}

		public void Write(PacketBase packet)
		{
			using (var writer = new PacketWriter())
			{
				packet.Serialize(writer, this.Version);
				writer.WriteTo(this.Stream);
			}
		}
	}


	internal class TcpTransport : BaseTransport
	{
		private TcpClient tcpClient;
		private NetworkStream netstream;

		override public Stream Stream
		{
			get
			{
				return this.netstream;
			}
		}

		override public bool IsClosed
		{
			get
			{
				return tcpClient == null || !tcpClient.Connected;
			}
		}

		internal TcpTransport(string hostname, int port)
		{
			this.tcpClient = new TcpClient();
			this.tcpClient.Connect(hostname, port);
			this.netstream = this.tcpClient.GetStream();

		}

		override public void SetTimeouts(TimeSpan readTimeout, TimeSpan writeTimeout)
		{
			this.netstream.ReadTimeout = (int)readTimeout.TotalMilliseconds;
			this.netstream.WriteTimeout = (int)writeTimeout.TotalMilliseconds;
		}

		override public bool Poll(int pollLimit)
		{
			return this.tcpClient.Client.Poll(pollLimit, SelectMode.SelectRead);
		}

		override public void Close()
		{
			this.netstream.Close();
			this.tcpClient.Close();
		}
	}


	internal class TlsTransport : BaseTransport
	{
		private TcpClient tcpClient;
		private NetworkStream netstream;
		private SslStream sslStream;

		override public Stream Stream
		{
			get
			{
				return this.sslStream;
			}
		}

		override public bool IsClosed
		{
			get
			{
				return tcpClient == null || !tcpClient.Connected;
			}
		}

		internal TlsTransport(string hostname, int port)
		{
			this.tcpClient = new TcpClient();
			this.tcpClient.Connect(hostname, port);

			this.netstream = this.tcpClient.GetStream();
			this.sslStream = new SslStream(netstream, false);

			this.sslStream.AuthenticateAsClient(hostname);
		}

		override public void SetTimeouts(TimeSpan readTimeout, TimeSpan writeTimeout)
		{
			this.sslStream.ReadTimeout = (int)readTimeout.TotalMilliseconds;
			this.sslStream.WriteTimeout = (int)writeTimeout.TotalMilliseconds;
		}

		override public bool Poll(int pollLimit)
		{
			return this.tcpClient.Client.Poll(pollLimit, SelectMode.SelectRead);
		}

		override public void Close()
		{
			this.sslStream.Close();
			this.netstream.Close();
			this.tcpClient.Close();
		}
	}

}

