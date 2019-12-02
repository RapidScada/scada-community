namespace Scada.Comm.Devices
{
    public class KpMqttView : KPView
    {
        public KpMqttView() : this(0)
        {
        }

        public KpMqttView(int number)
            : base(number)
        {
        }

        public override string KPDescr
        {
            get
            {
                return Localization.UseRussian ?
                    "Библиотека КП для протокола MQTT." :
                    "Device library for protocol MQTT.";
            }
        }

        /// <summary>
        /// Gets the default device request parameters.
        /// </summary>
        public override KPReqParams DefaultReqParams
        {
            get
            {
                return new KPReqParams(10000, 200) { CmdLine = "KpMQTT_Config.xml" };
            }
        }
    }
}
