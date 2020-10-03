using System;
using System.IO;
using System.Text;

namespace StriderMqtt
{
	/// <summary>
	/// Mqtt packet reader with convenience methods to decode incoming data
	/// </summary>
	internal class PacketReader
	{
		Stream input;

		internal byte FixedHeaderFirstByte
		{
			get;
			private set;
		}

		internal byte PacketTypeCode
		{
			get
			{
				return (byte)(FixedHeaderFirstByte >> Packet.PacketTypeOffset);
			}
		}

		internal bool Dup
		{
			get
			{
				return (FixedHeaderFirstByte & Packet.DupFlagMask) == Packet.DupFlagMask;
			}
		}

		internal MqttQos QosLevel
		{
			get
			{
				return (MqttQos)((FixedHeaderFirstByte & Packet.QosLevelMask) >> Packet.QosLevelOffset);
			}
		}

		internal bool Retain
		{
			get
			{
				return (FixedHeaderFirstByte & Packet.RetainFlagMask) == Packet.RetainFlagMask;
			}
		}

		internal int RemainingLength
		{
			get;
			private set;
		}

		/// <summary>
		/// An index to follow remaining length reading
		/// </summary>
		/// <value>The index.</value>
		internal int Index
		{
			get;
			private set;
		}

		internal PacketReader(byte fixedHeaderFirstByte, Stream stream)
		{
			this.FixedHeaderFirstByte = fixedHeaderFirstByte;
			this.input = stream;

			this.RemainingLength = ReadRemainingLength(input);
		}

		internal PacketReader(Stream stream)
		{
			this.input = stream;

			this.FixedHeaderFirstByte = (byte)input.ReadByte();
			this.RemainingLength = ReadRemainingLength(input);
		}

		/// <summary>
		/// Decode remaining length reading bytes from socket
		/// </summary>
		/// <param name="channel">Channel from reading bytes</param>
		/// <returns>Decoded remaining length</returns>
		int ReadRemainingLength(Stream stream)
		{
			int multiplier = 1;
			int value = 0;
			int digit = 0;

			byte[] nextByte = new byte[1];

			do
			{
				// next digit from stream
				if (stream.Read(nextByte, 0, 1) == 1)
				{
					digit = nextByte[0];
					value += ((digit & 127) * multiplier);
					multiplier *= 128;
				}
				else
				{
					throw new MqttProtocolException("Could not read remaining length");
				}
			} while ((digit & 128) != 0);

			return value;
		}

		internal byte ReadByte()
		{
			Index++;
			return (byte)input.ReadByte();
		}

		internal byte[] ReadBytes(ushort n)
		{
			Index += n;
			byte[] buffer = new byte[n];

			this.input.Read(buffer, 0, n);

			return buffer;
		}

		internal ushort ReadIntegerField()
		{
			Index += 2;

			byte[] buffer = new byte[2];

			this.input.Read(buffer, 0, 2);

			ushort value = (ushort)((buffer[0] << 8) & 0xFF00);
			value |= buffer[1];

			return value;
		}

		internal string ReadTextField()
		{
			ushort length = this.ReadIntegerField();
			byte[] bytes = this.ReadBytes(length);

			return Encoding.UTF8.GetString(bytes);
		}

		internal byte[] ReadToEnd()
		{
			int remaining = RemainingLength - Index;

			if (remaining < 0)
			{
				throw new MqttProtocolException("More than the remaining length was read");
			}
			else if (remaining == 0)
			{
				return new byte[0];
			}
			else
			{
				byte[] buffer = new byte[remaining];
				this.input.Read(buffer, 0, remaining);
				return buffer;
			}
		}

	}
}

