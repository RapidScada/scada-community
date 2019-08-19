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
    }
}
