using Scada.Client;
using Scada.Data.Tables;
using StriderMqtt;
using System.Collections.Generic;

namespace Scada.Comm.Devices
{
    public abstract class MQTTPubParam
    {
        public string TopicName { get; set; }
        public MqttQos QosLevels { get; set; }
        public bool Retain { get; set; }
    }

    public class MQTTPubTopic : MQTTPubParam
    {
        public int NumCnl { get; set; }
        public string PubBehavior { get; set; }
        public string NumberDecimalSeparator { get; set; }
        public double Value { get; set; }
        public bool IsPub { get; set; }
    }

    public class MQTTPubCmd : MQTTPubParam
    {
        public int NumCmd { get; set; }
    }

    public class  RapSrvEx : ServerComm
    {
        public bool cn { get; set; }
  
        public bool IsCurr { get; set; }

        private RapSrvEx rsrv;

        public RapSrvEx(CommSettings cs)
        {
            rsrv = this;
            rsrv.commSettings = cs;

        }

        public List<MQTTPubTopic> GetValues(List<MQTTPubTopic> MqttPTs)
        {
            if (cn)
            {
                SrezTableLight stl = new SrezTableLight();
                IsCurr = rsrv.ReceiveSrezTable("current.dat", stl);
                SrezTableLight.Srez srez = stl.SrezList.Values[0];
                bool found;
                SrezTableLight.CnlData cnlData;
                foreach (MQTTPubTopic MqttPT in MqttPTs)
                {
                    found = srez.GetCnlData(MqttPT.NumCnl, out cnlData);
                    if (found)
                    {
                        if (MqttPT.PubBehavior == "OnChange")
                        {
                            if (MqttPT.Value != cnlData.Val)
                                MqttPT.IsPub = true;
                        }
                        if (MqttPT.PubBehavior == "OnAlways")
                            MqttPT.IsPub = true;
                        MqttPT.Value = cnlData.Val;
                    }
                }
            }
            return MqttPTs;
        }

        public void Conn()
        {
            cn = rsrv.Connect();
        }

        public void Disconn()
        {
            rsrv.Disconnect();
        }

    }

}

