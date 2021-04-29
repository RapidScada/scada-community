using Jint;
using Scada.Client;
using Scada.Comm.Devices.Mqtt.Config;
using Scada.Data.Configuration;
using Scada.Data.Models;
using Scada.Data.Tables;
using StriderMqtt;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace Scada.Comm.Devices
{
    public class KpMqttLogic : KPLogic
    {
        private class JsVal
        {
            public int CnlNum { get; set; }
            public int Stat { get; set; }
            public double Val { get; set; }
        }

        private const int PollTimeout = 1000; // timeout to check status, microseconds
        private static readonly NumberFormatInfo Nfi = new NumberFormatInfo();

        private XmlNode MQTTSettings;

        private IMqttTransport Transport;
        private IMqttPersistence Persistence;

        private SubscribePacket sp;
        private MqttConnectionArgs connArgs;
        private List<MqttPubTopic> PubTopics;
        private List<MqttPubCmd> PubCmds;
        private List<MqttSubCmd> SubCmds;
        private List<MqttSubJS> SubJSs;


        public KpMqttLogic(int number) : base(number)
        {
            ConnRequired = false;
            CanSendCmd = true;
            WorkState = WorkStates.Normal;
        }

        private void ResumeOutgoingFlows()
        {
            foreach (OutgoingFlow flow in Persistence.GetPendingOutgoingFlows())
            {
                Resume(flow);
            }
        }

        private void Resume(OutgoingFlow flow)
        {
            if (flow.Qos == MqttQos.AtLeastOnce || (flow.Qos == MqttQos.ExactlyOnce && !flow.Received))
            {
                PublishPacket publish = new PublishPacket()
                {
                    PacketId = flow.PacketId,
                    QosLevel = flow.Qos,
                    Topic = flow.Topic,
                    Message = flow.Payload,
                    DupFlag = true
                };

                Publish(publish);
            }
            else if (flow.Qos == MqttQos.ExactlyOnce && flow.Received)
            {
                Pubrel(flow.PacketId);
            }

            Persistence.LastOutgoingPacketId = flow.PacketId;
        }

        private ushort Publish(PublishPacket packet)
        {
            if (packet.QosLevel != MqttQos.AtMostOnce)
            {
                if (packet.PacketId == 0)
                    packet.PacketId = GetNextPacketId();

                Persistence.RegisterOutgoingFlow(new OutgoingFlow
                {
                    PacketId = packet.PacketId,
                    Topic = packet.Topic,
                    Qos = packet.QosLevel,
                    Payload = packet.Message
                });
            }

            Send(packet);
            return packet.PacketId;
        }

        private void Pubrel(ushort packetId)
        {
            Send(new PubrelPacket() { PacketId = packetId });
        }

        private void Subscribe(SubscribePacket packet)
        {
            if (packet.PacketId == 0)
                packet.PacketId = GetNextPacketId();

            Send(packet);
        }

        private void Unsubscribe(UnsubscribePacket packet)
        {
            if (packet.PacketId == 0)
                packet.PacketId = GetNextPacketId();

            Send(packet);
        }

        private ConnectPacket MakeConnectMessage(MqttConnectionArgs args)
        {
            ConnectPacket conn = new ConnectPacket
            {
                ProtocolVersion = args.Version,
                ClientId = args.ClientId,
                Username = args.Username,
                Password = args.Password
            };

            if (args.WillMessage != null)
            {
                conn.WillFlag = true;
                conn.WillTopic = args.WillMessage.Topic;
                conn.WillMessage = args.WillMessage.Message;
                conn.WillQosLevel = args.WillMessage.Qos;
                conn.WillRetain = args.WillMessage.Retain;
            }

            conn.CleanSession = args.CleanSession;
            conn.KeepAlivePeriod = (ushort)args.Keepalive.TotalSeconds;

            return conn;
        }

        private bool ReceiveConnack()
        {
            PacketBase packet = Transport.Read();

            if (packet == null)
            {
                WriteToLog(Localization.UseRussian ?
                    "Ошибка: первый пакет не получен" :
                    "Error: first packet is not received");
            }
            else if (!(packet is ConnackPacket connack))
            {
                WriteToLog(string.Format(Localization.UseRussian ? 
                    "Ошибка: первый принимаемый пакет должен быть Connack, но получили {0}" :
                    "Error: first packet must be Connack, but {0} received", 
                    packet.GetType().Name));
            }
            else if (connack.ReturnCode != ConnackReturnCode.Accepted)
            {
                WriteToLog(string.Format(Localization.UseRussian ? 
                    "Соединение не разрешено брокером. Код: {0}" : 
                    "The connection was not accepted by a broker. Return code: {0}",
                    connack.ReturnCode));
            }
            else
            {
                return true;
            }

            return false;
        }

        private void Send(PacketBase packet)
        {
            if (Transport.IsClosed)
            {
                lastCommSucc = false;
                WriteToLog(Localization.UseRussian ?
                    "Ошибка: попытка отправить пакет в закрытом состоянии транспорта" :
                    "Error: attempt to send packet while the transport is closed");
            }
            else
            {
                WriteToLog("Send packet");
                Transport.Write(packet);
            }
        }

        private ushort GetNextPacketId()
        {
            ushort x = Persistence.LastOutgoingPacketId;
            if (x == Packet.MaxPacketId)
            {
                Persistence.LastOutgoingPacketId = 1;
                return 1;
            }
            else
            {
                x += 1;
                Persistence.LastOutgoingPacketId = x;
                return x;
            }
        }

        private void ReceivePacket()
        {
            try
            {
                WriteToLog("Receive packet");
                PacketBase packet = Transport.Read();
                HandleReceivedPacket(packet);
            }
            catch (MqttProtocolException ex)
            {
                WriteToLog(string.Format(Localization.UseRussian ?
                    "Ошибка: {0}" :
                    "Error: {0}", ex.ToString()));
                Disconnect();
                lastCommSucc = false;
            }
            finally
            {
                FinishRequest();
            }
        }

        private void HandleReceivedPacket(PacketBase packet)
        {
            switch (packet.PacketType)
            {
                case PublishPacket.PacketTypeCode:
                    OnPublishReceived(packet as PublishPacket);
                    break;
                case PubackPacket.PacketTypeCode:
                    OnPubackReceived(packet as PubackPacket);
                    break;
                case PubrecPacket.PacketTypeCode:
                    OnPubrecReceived(packet as PubrecPacket);
                    break;
                case PubrelPacket.PacketTypeCode:
                    OnPubrelReceived(packet as PubrelPacket);
                    break;
                case PubcompPacket.PacketTypeCode:
                    OnPubcompReceived(packet as PubcompPacket);
                    break;
                case SubackPacket.PacketTypeCode:
                    OnSubackReceived(packet as SubackPacket);
                    break;
                case UnsubackPacket.PacketTypeCode:
                    OnUnsubackReceived(packet as UnsubackPacket);
                    break;
                case PingrespPacket.PacketTypeCode:
                    break;
                default:
                    lastCommSucc = false;
                    WriteToLog(string.Format(Localization.UseRussian ?
                        "Ошибка: невозможно принять пакет типа {0}" :
                        "Error: can not receive message of type {0}", packet.GetType().Name));
                    break;
            }
        }

        // -- incoming publish events --

        private void OnPublishReceived(PublishPacket packet)
        {
            //WriteToLog (Encoding.UTF8.GetString(packet.Message));
            //WriteToLog (packet.Topic);
            string pv = Encoding.UTF8.GetString(packet.Message);
            Regex reg = new Regex(@"^[-]?\d+[,.]?\d+$");
            Regex reg2 = new Regex(@"^[-\d]+$");

            if (SubJSs.Count > 0)
            {
                Engine jsEng = new Engine();

                foreach (MqttSubJS mqttjs in SubJSs)
                {
                    if (mqttjs.TopicName == packet.Topic)
                    {

                        SrezTableLight.Srez srez = new SrezTableLight.Srez(DateTime.Now, mqttjs.CnlCnt);
                        List<JsVal> jsvals = new List<JsVal>();

                        for (int i = 0; i < srez.CnlNums.Length; i++)
                        {
                            JsVal jsval = new JsVal();

                            jsvals.Add(jsval);
                        }

                        jsEng.SetValue("mqttJS", mqttjs);
                        jsEng.SetValue("InMsg", Encoding.UTF8.GetString(packet.Message));
                        jsEng.SetValue("jsvals", jsvals);
                        jsEng.SetValue("mylog", new Action<string>(WriteToLog));

                        try
                        {
                            jsEng.Execute(mqttjs.JSHandler);
                            int i = 0;

                            foreach (JsVal jsvl in jsvals)
                            {

                                SrezTableLight.CnlData cnlData = new SrezTableLight.CnlData
                                {
                                    Stat = jsvl.Stat,
                                    Val = jsvl.Val
                                };

                                srez.CnlNums[i] = jsvl.CnlNum;
                                srez.SetCnlData(jsvl.CnlNum, cnlData);
                                i++;
                            }

                            CommLineSvc.ServerComm?.SendSrez(srez, out bool sndres);
                        }
                        catch
                        {
                            WriteToLog(Localization.UseRussian ? 
                                "Ошибка обработки JS" : 
                                "Error executing JS");
                        }
                        break;
                    }
                }
            }

            if (SubCmds.Count > 0)
            {
                foreach (MqttSubCmd mqttcmd in SubCmds)
                {
                    if (mqttcmd.TopicName == packet.Topic)
                    {
                        if (mqttcmd.CmdType == "St")
                        {
                            if (reg.IsMatch(pv))
                                pv = pv.Replace('.', ',');

                            mqttcmd.CmdVal = ScadaUtils.StrToDouble(pv);
                            if (!double.IsNaN(mqttcmd.CmdVal))
                            {
                                CommLineSvc.ServerComm?.SendStandardCommand(
                                    mqttcmd.IDUser, mqttcmd.NumCnlCtrl, mqttcmd.CmdVal, out bool result);
                            }
                            break;
                        }
                        else if (mqttcmd.CmdType == "BinTxt")
                        {
                            CommLineSvc.ServerComm?.SendBinaryCommand(
                                mqttcmd.IDUser, mqttcmd.NumCnlCtrl, packet.Message, out bool result);
                            break;
                        }
                        else if (mqttcmd.CmdType == "BinHex")
                        {
                            if (ScadaUtils.HexToBytes(pv.Trim(), out byte[] cmdData))
                            {
                                CommLineSvc.ServerComm?.SendBinaryCommand(
                                    mqttcmd.IDUser, mqttcmd.NumCnlCtrl, cmdData, out bool result);
                            }

                            break;
                        }
                        else if (mqttcmd.CmdType == "Req")
                        {
                            CommLineSvc.ServerComm?.SendRequestCommand(
                                mqttcmd.IDUser, mqttcmd.NumCnlCtrl, mqttcmd.KPNum, out bool result);
                            break;
                        }
                    }
                }
            }

            if (KPTags.Length > 0)
            {
                int tagInd = 0;
                foreach (KPTag kpt in KPTags)
                {
                    if (kpt.Name == packet.Topic)
                    {
                        if (reg.IsMatch(pv))
                        {
                            pv = pv.Replace('.', ',');
                            SetCurData(tagInd, ScadaUtils.StrToDouble(pv), 1);
                            WriteToLog(pv);
                            break;
                        }
                        if (reg2.IsMatch(pv))
                        {
                            SetCurData(tagInd, ScadaUtils.StrToDouble(pv), 1);
                            WriteToLog(pv);
                            break;
                        }
                    }
                    tagInd++;
                }
            }

            if (packet.QosLevel == MqttQos.ExactlyOnce)
            {
                OnQos2PublishReceived(packet);
            }
            else if (packet.QosLevel == MqttQos.AtLeastOnce)
            {
                Send(new PubackPacket { PacketId = packet.PacketId });
            }
        }

        private void OnQos2PublishReceived(PublishPacket packet)
        {
            if (!Persistence.IsIncomingFlowRegistered(packet.PacketId))
            {
                // Register the incoming packetId, so duplicate messages can be filtered.
                // This is done after "ProcessIncomingPublish" because we can't assume the
                // mesage was received in the case that method throws an exception.
                Persistence.RegisterIncomingFlow(packet.PacketId);

                // the ideal would be to run `PubishReceived` and `Persistence.RegisterIncomingFlow`
                // in a single transaction (either both or neither succeeds).
            }

            Send(new PubrecPacket { PacketId = packet.PacketId });
        }

        private void OnPubrelReceived(PubrelPacket packet)
        {
            Persistence.ReleaseIncomingFlow(packet.PacketId);
            Send(new PubcompPacket { PacketId = packet.PacketId });
        }

        // -- outgoing publish events --

        private void OnPubackReceived(PubackPacket packet)
        {
            Persistence.SetOutgoingFlowCompleted(packet.PacketId);
        }

        private void OnPubrecReceived(PubrecPacket packet)
        {
            Persistence.SetOutgoingFlowReceived(packet.PacketId);
            Send(new PubrelPacket { PacketId = packet.PacketId });
        }

        private void OnPubcompReceived(PubcompPacket packet)
        {
            Persistence.SetOutgoingFlowCompleted(packet.PacketId);
        }

        // -- subscription events --
        private void OnSubackReceived(SubackPacket packet)
        {

        }

        private void OnUnsubackReceived(UnsubackPacket packet)
        {

        }

        private bool RestoreConnect(out string errMsg)
        {
            try
            {
                if (Transport.IsClosed)
                    Connect(MQTTSettings);

                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }

        private void Connect(XmlNode MQTTSettings)
        {
            connArgs = new MqttConnectionArgs
            {
                ClientId = MQTTSettings.Attributes.GetNamedItem("ClientID").Value,
                Hostname = MQTTSettings.Attributes.GetNamedItem("Hostname").Value,
                Port = Convert.ToInt32(MQTTSettings.Attributes.GetNamedItem("Port").Value),
                Username = MQTTSettings.Attributes.GetNamedItem("UserName").Value,
                Password = MQTTSettings.Attributes.GetNamedItem("Password").Value,
                //Keepalive = TimeSpan.FromSeconds(60),
            };

            connArgs.ReadTimeout = TimeSpan.FromMilliseconds(ReqParams.Timeout);
            connArgs.WriteTimeout = TimeSpan.FromMilliseconds(ReqParams.Timeout);

            Persistence = new InMemoryPersistence();
            Transport = new TcpTransport(connArgs.Hostname, connArgs.Port) { Version = connArgs.Version };
            Transport.SetTimeouts(connArgs.ReadTimeout, connArgs.WriteTimeout);

            Send(MakeConnectMessage(connArgs));

            if (ReceiveConnack())
            {
                ResumeOutgoingFlows();

                if (sp.Topics.Length > 0)
                    Subscribe(sp);

                WriteToLog(Localization.UseRussian ?
                    "Соединение установлено" :
                    "Connection established");
            }
        }

        private void Disconnect()
        {
            Send(new DisconnectPacket());
            Transport.Close();
            WriteToLog(Localization.UseRussian ? 
                "Отключение от MQTT брокера" : 
                "Disconnect from MQTT broker");
        }

        private List<MqttPubTopic> GetValues(List<MqttPubTopic> MqttPTs)
        {
            if (CommLineSvc.ServerComm != null)
            {
                SrezTableLight stl = new SrezTableLight();
                CommLineSvc.ServerComm.ReceiveSrezTable("current.dat", stl);
                SrezTableLight.Srez srez = stl.SrezList.Values[0];

                foreach (MqttPubTopic MqttPT in MqttPTs)
                {
                    if (srez.GetCnlData(MqttPT.NumCnl, out SrezTableLight.CnlData cnlData))
                    {
                        if (MqttPT.PubBehavior == PubBehavior.OnChange)
                        {
                            if (MqttPT.Value != cnlData.Val)
                                MqttPT.IsPub = true;
                        }
                        else if (MqttPT.PubBehavior == PubBehavior.OnAlways)
                        {
                            MqttPT.IsPub = true;
                        }

                        MqttPT.Value = cnlData.Val;
                    }
                }
            }
            return MqttPTs;
        }

        public override void Session()
        {
            base.Session();

            if (!RestoreConnect(out string errMsg))
            {
                lastCommSucc = false;
                WriteToLog(string.Format(Localization.UseRussian ? 
                    "Ошибка при повторном подключении: {0}" : 
                    "Error reconnecting: {0}", errMsg));
            }
            else
            {
                if (Transport.Poll(PollTimeout))
                    ReceivePacket();
                else
                    Send(new PingreqPacket()); // send ping request

                foreach (MqttPubTopic mqtttp in GetValues(PubTopics))
                {
                    if (!mqtttp.IsPub)
                        continue;

                    Nfi.NumberDecimalSeparator = mqtttp.DecimalSeparator;

                    Publish(new PublishPacket
                    {
                        Topic = mqtttp.TopicName,
                        QosLevel = mqtttp.QosLevel,
                        Retain = mqtttp.Retain,
                        Message = Encoding.UTF8.GetBytes(mqtttp.Prefix + mqtttp.Value.ToString(Nfi) + mqtttp.Suffix)
                    });

                    mqtttp.IsPub = false;
                }  
            }

            Thread.Sleep(ReqParams.Delay);
            CalcSessStats();
        }

        public override void OnAddedToCommLine()
        {
            List<TagGroup> tagGroups = new List<TagGroup>();
            TagGroup tagGroup = new TagGroup("GroupMQTT");
            //TagGroup tagGroupJS = new TagGroup("GoupJS");

            XmlDocument xmlDoc = new XmlDocument();
            string filename = ReqParams.CmdLine.Trim();
            xmlDoc.Load(AppDirs.ConfigDir + filename);

            XmlNode MQTTSubTopics = xmlDoc.DocumentElement.SelectSingleNode("MqttSubTopics");
            XmlNode MQTTPubTopics = xmlDoc.DocumentElement.SelectSingleNode("MqttPubTopics");
            XmlNode MQTTPubCmds = xmlDoc.DocumentElement.SelectSingleNode("MqttPubCmds");
            XmlNode MQTTSubCmds = xmlDoc.DocumentElement.SelectSingleNode("MqttSubCmds");
            XmlNode MQTTSubJSs = xmlDoc.DocumentElement.SelectSingleNode("MqttSubJSs");
            MQTTSettings = xmlDoc.DocumentElement.SelectSingleNode("MqttParams");

            PubTopics = new List<MqttPubTopic>();
            PubCmds = new List<MqttPubCmd>();
            bool checkRetain;

            foreach (XmlElement MqttPTCnf in MQTTPubTopics)
            {
                if (bool.TryParse(MqttPTCnf.GetAttribute("Retain"), out bool retain))
                    checkRetain = retain;
                else
                    checkRetain = false;

                PubTopics.Add(new MqttPubTopic()
                {
                    TopicName = MqttPTCnf.GetAttribute("TopicName"),
                    QosLevel = (MqttQos)Convert.ToByte(MqttPTCnf.GetAttribute("QosLevel")),
                    Retain = checkRetain,
                    NumCnl = Convert.ToInt32(MqttPTCnf.GetAttribute("NumCnl")),
                    PubBehavior = MqttPTCnf.GetAttrAsEnum<PubBehavior>("PubBehavior"),
                    DecimalSeparator = MqttPTCnf.GetAttribute("NDS"),
                    Value = 0,                    
                    Prefix = MqttPTCnf.GetAttribute("Prefix"),
                    Suffix = MqttPTCnf.GetAttribute("Suffix")
                });
            }

            foreach (XmlElement MqttPTCnf in MQTTPubCmds)
            {
                PubCmds.Add(new MqttPubCmd()
                {
                    TopicName = MqttPTCnf.GetAttribute("TopicName"),
                    QosLevel = (MqttQos)Convert.ToByte(MqttPTCnf.GetAttribute("QosLevel")),
                    Retain = false,
                    NumCmd = MqttPTCnf.GetAttrAsInt("NumCmd")
                });
            }

            sp = new SubscribePacket();
            int i = 0;
            int spCnt = MQTTSubTopics.ChildNodes.Count;
            spCnt += MQTTSubCmds.ChildNodes.Count;
            spCnt += MQTTSubJSs.ChildNodes.Count;

            sp.Topics = new string[spCnt];
            sp.QosLevels = new MqttQos[spCnt];

            foreach (XmlElement elemGroupElem in MQTTSubTopics.ChildNodes)
            {
                sp.Topics[i] = elemGroupElem.GetAttribute("TopicName");
                sp.QosLevels[i] = (MqttQos)Convert.ToByte(elemGroupElem.GetAttribute("QosLevel"));

                tagGroup.KPTags.Add(new KPTag
                {
                    Signal = i + 1,
                    Name = sp.Topics[i],
                    CnlNum = Convert.ToInt32(elemGroupElem.GetAttribute("NumCnl"))
                });

                i++;
            }

            tagGroups.Add(tagGroup);
            InitKPTags(tagGroups);

            SubCmds = new List<MqttSubCmd>();

            foreach (XmlElement elemGroupElem in MQTTSubCmds.ChildNodes)
            {
                sp.Topics[i] = elemGroupElem.GetAttribute("TopicName");
                sp.QosLevels[i] = (MqttQos)Convert.ToByte(elemGroupElem.GetAttribute("QosLevel"));

                MqttSubCmd cmd = new MqttSubCmd()
                {
                    TopicName = sp.Topics[i],
                    CmdNum = elemGroupElem.GetAttrAsInt("NumCmd", 0),
                    CmdType = elemGroupElem.GetAttribute("CmdType"),
                    KPNum = elemGroupElem.GetAttrAsInt("KPNum", 0),
                    IDUser = elemGroupElem.GetAttrAsInt("IDUser", 0),
                    NumCnlCtrl = elemGroupElem.GetAttrAsInt("NumCnlCtrl", 0)
                };

                SubCmds.Add(cmd);
                i++;
            }

            SubJSs = new List<MqttSubJS>();

            foreach (XmlElement elemGroupElem in MQTTSubJSs.ChildNodes)
            {
                sp.Topics[i] = elemGroupElem.GetAttribute("TopicName");
                sp.QosLevels[i] = (MqttQos)Convert.ToByte(elemGroupElem.GetAttribute("QosLevel"));

                MqttSubJS msjs = new MqttSubJS()
                {
                    TopicName = sp.Topics[i],
                    CnlCnt = elemGroupElem.GetAttrAsInt("CnlCnt", 1),
                    JSHandlerPath = elemGroupElem.GetAttrAsString("JSHandlerPath", "")
                };

                if (msjs.LoadJSHandler())
                {
                    SubJSs.Add(msjs);
                    i++;
                }
            }

            Connect(MQTTSettings);
        }

        public override void SendCmd(Command cmd)
        {
            base.SendCmd(cmd); 

            foreach (MqttPubCmd mpc in PubCmds)
            {
                if (mpc.NumCmd == cmd.CmdNum)
                {
                    if (cmd.CmdTypeID == BaseValues.CmdTypes.Standard)
                    {
                        Nfi.NumberDecimalSeparator = ".";
                        Publish(new PublishPacket()
                        {
                            Topic = mpc.TopicName,
                            QosLevel = mpc.QosLevel,
                            Retain = mpc.Retain,
                            Message = Encoding.UTF8.GetBytes(cmd.CmdVal.ToString(Nfi))
                        });
                    }
                    else if (cmd.CmdTypeID == BaseValues.CmdTypes.Binary)
                    {
                        Publish(new PublishPacket()
                        {
                            Topic = mpc.TopicName,
                            QosLevel = mpc.QosLevel,
                            Retain = mpc.Retain,
                            Message = cmd.CmdData
                        });
                    }
                }
            }

            CalcCmdStats();
        }

        public override void OnCommLineTerminate()
        {
            Disconnect();
        }
    }
}
