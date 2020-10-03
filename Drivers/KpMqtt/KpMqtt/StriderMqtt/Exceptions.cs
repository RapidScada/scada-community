using System;

namespace StriderMqtt
{
	public class MqttClientException : Exception
	{
		public MqttClientException() : base()
		{
		}

		public MqttClientException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// Mqtt protocol exception, occurs when protocol specs are violated.
	/// </summary>
	public class MqttProtocolException : MqttClientException
	{
		public MqttProtocolException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// Mqtt connect exception, occurs when it is not possible to establish a connection.
	/// </summary>
	public class MqttConnectException : MqttClientException
	{
		public ConnackReturnCode ReturnCode
		{
			get;
			private set;
		}

		public MqttConnectException(string message, ConnackReturnCode code) : base(message)
		{
			this.ReturnCode = code;
		}
	}

	/// <summary>
	/// Mqtt timeout exception, occurs when nothing was read in 1.5*keepalive period.
	/// </summary>
	public class MqttTimeoutException : MqttClientException
	{
		public MqttTimeoutException() : base()
		{
		}
	}
}

