using System;
using System.IO;
using System.Text;

namespace StriderMqtt
{
	/// <summary>
	/// Mqtt Packet writer, with common methods to write on mqtt data formats.
	/// </summary>
	internal class PacketWriter : IDisposable
	{
		byte fixedHeader;
		MemoryStream content = new MemoryStream();


		internal PacketWriter()
		{
		}


		/// <summary>
		/// Writes the fixed header with flags set to 0 and qos to 0.
		/// </summary>
		/// <param name="packetTypeCode">Packet type code.</param>
		internal void SetFixedHeader(byte packetTypeCode)
		{
			this.fixedHeader = (byte)((packetTypeCode << Packet.PacketTypeOffset) & Packet.PacketTypeMask);
		}

		/// <summary>
		/// Writes the fixed header with the specified qos level and flags zeroed.
		/// </summary>
		/// <param name="packetType">Packet type.</param>
		/// <param name="qosLevel">Qos level.</param>
		internal void SetFixedHeader(byte packetType, MqttQos qosLevel)
		{
			fixedHeader = (byte)(packetType << Packet.PacketTypeOffset);
			fixedHeader |= (byte)((byte)qosLevel << Packet.QosLevelOffset);
		}

		/// <summary>
		/// Writes the fixed header using the provided values for qos and flags
		/// </summary>
		/// <param name="packetType">Packet type.</param>
		/// <param name="duplicate">If set to <c>true</c> duplicate.</param>
		/// <param name="qosLevel">Qos level.</param>
		/// <param name="retain">If set to <c>true</c> retain.</param>
		internal void SetFixedHeader(byte packetType, bool duplicate, MqttQos qosLevel, bool retain)
		{
			fixedHeader = (byte)(packetType << Packet.PacketTypeOffset);
			fixedHeader |= duplicate ? (byte)(1 << Packet.DupFlagOffset) : (byte)0x00;
			fixedHeader |= (byte)(((byte)qosLevel << Packet.QosLevelOffset) & Packet.QosLevelMask);
			fixedHeader |= retain ? (byte)(1 << Packet.RetainFlagOffset) : (byte)0x00;
		}

		internal void Append(byte value)
		{
			content.WriteByte(value);
		}

		internal void Append(byte[] bytes)
		{
			content.Write(bytes, 0, bytes.Length);
		}

		/// <summary>
		/// Writes an MQTT integer to the stream, with 16-bits in big-endian order.
		/// </summary>
		/// <param name="stream">Stream.</param>
		/// <param name="value">Value.</param>
		internal void AppendIntegerField(ushort value)
		{
			content.WriteByte((byte)((value >> 8) & 0xFF));  // MSB
			content.WriteByte((byte)(value & 0xFF)); // LSB
		}

		/// <summary>
		/// Writes a MQTT text field to the stream.
		/// The mqtt text field consists of the length of the text (16-bit big-endian)
		/// followed by the UTF-8 encoded char data.
		/// </summary>
		/// <param name="stream">Stream.</param>
		/// <param name="value">Value.</param>
		internal void AppendTextField(string value)
		{
			if (value == null)
			{
				throw new ArgumentException("Value shouldn't be null");
			}

			byte[] bytes = Encoding.UTF8.GetBytes(value);

			if (bytes.Length > ushort.MaxValue)
			{
				throw new ArgumentException("Value shouldn't be longer than 65535 bytes");
			}

			AppendIntegerField((ushort)bytes.Length);
			content.Write(bytes, 0, bytes.Length);
		}

		/// <summary>
		/// Writes a MQTT bytes field to the stream.
		/// It consists of an integer length of the data (16-bit big-endian)
		/// followed by the data itself.
		/// </summary>
		/// <param name="stream">Stream.</param>
		/// <param name="value">Value.</param>
		internal void AppendBytesField(byte[] value)
		{
			if (value == null)
			{
				throw new ArgumentException("Value shouldn't be null");
			}

			if (value.Length > ushort.MaxValue)
			{
				throw new ArgumentException("Value shouldn't be longer than 65535 bytes");
			}

			AppendIntegerField((ushort)value.Length);
			content.Write(value, 0, value.Length);
		}

		/// <summary>
		/// Writes the contents of the current PacketWriter
		/// to the given stream.
		/// </summary>
		/// <param name="s">S.</param>
		internal void WriteTo(Stream s)
		{
			s.WriteByte(fixedHeader);
			WriteRemainingLength(s);

			if (content.Length > 0)
			{
				content.WriteTo(s);
			}
		}

		/// <summary>
		/// Encode remaining length value and writes to the stream
		/// </summary>
		/// <param name="remainingLength">Remaining length value to encode</param>
		/// <param name="stream">The stream to write to</param>
		void WriteRemainingLength(Stream output)
		{
			int remainingLength = (int)content.Length;

			if (remainingLength > Packet.MaxRemainingLength)
			{
				throw new MqttProtocolException("Packet size limit exceeded");
			}
			else if (remainingLength < 0)
			{
				throw new InvalidOperationException("Remaining length should not be negative");
			}

			int digit = 0;

			do
			{
				digit = remainingLength % 128;
				remainingLength /= 128;
				if (remainingLength > 0)
				{
					digit = digit | 0x80;
				}

				output.WriteByte((byte)digit);
			} while (remainingLength > 0);
		}

		public void Dispose()
		{
			content.Dispose();
		}
	}
}

