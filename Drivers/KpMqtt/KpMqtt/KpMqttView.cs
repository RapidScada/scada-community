using Scada.Comm.Devices.Mqtt.Config;
using Scada.Data.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Scada.Comm.Devices
{
    public class KpMqttView : KPView
    {
        internal const string KpVersion = "5.0.4.0";


        public KpMqttView() : 
            this(0)
        {
        }

        public KpMqttView(int number)
            : base(number)
        {
            CanShowProps = number > 0;
        }


        public override string KPDescr
        {
            get
            {
                return Localization.UseRussian ?
                    "Подписка и публикация данных по протоколу MQTT." :
                    "Subscribes and publishes data using the MQTT protocol.";
            }
        }

        public override string Version
        {
            get
            {
                return KpVersion;
            }
        }

        public override KPReqParams DefaultReqParams
        {
            get
            {
                return new KPReqParams(10000, 500);
            }
        }

        public override KPCnlPrototypes DefaultCnls
        {
            get
            {
                // load configuration
                DeviceConfig deviceConfig = new DeviceConfig();
                string configFileName = DeviceConfig.GetFileName(AppDirs.ConfigDir, Number, KPProps.CmdLine);

                if (!File.Exists(configFileName))
                    return null;
                else if (!deviceConfig.Load(configFileName, out string errMsg))
                    throw new ScadaException(errMsg);

                // create channel prototypes
                KPCnlPrototypes prototypes = new KPCnlPrototypes();
                int signal = 1;

                void AddCnl(string name)
                {
                    prototypes.InCnls.Add(new InCnlPrototype(name, BaseValues.CnlTypes.TI) 
                    { 
                        Signal = signal++ 
                    });
                }

                deviceConfig.SubTopics.ForEach(t => AddCnl(t.TopicName));
                deviceConfig.SubJSs.ForEach(t =>
                {
                    for (int i = 0; i < t.CnlCnt; i++)
                    {
                        AddCnl(t.TopicName + " [" + i + "]");
                    }
                });

                return prototypes;
            }
        }


        public override void ShowProps()
        {
            // create configuration file if it doesn't exist
            string configFileName = DeviceConfig.GetFileName(AppDirs.ConfigDir, Number, KPProps.CmdLine);

            if (!File.Exists(configFileName))
            {
                string resourceName = "Scada.Comm.Devices.Config.KpMqtt_001.xml";
                string fileContents;

                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        fileContents = reader.ReadToEnd();
                    }
                }

                File.WriteAllText(configFileName, fileContents, Encoding.UTF8);
            }

            // open configuration directory
            Process.Start(AppDirs.ConfigDir);
        }
    }
}
