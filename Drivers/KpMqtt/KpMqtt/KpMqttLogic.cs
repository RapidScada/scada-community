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
            public string TagName { get; set; }
            public int Stat { get; set; }
            public double Val { get; set; }
        }

        private const int PollTimeout = 1000; // timeout to check status, microseconds
        private static readonly NumberFormatInfo Nfi = new NumberFormatInfo();
        
        private DeviceConfig deviceConfig;
        private IMqttTransport mqttTransport;
        private IMqttPersistence mqttPersistence;
        private SubscribePacket subscribePacket;

        private Dictionary<string, MqttSubTopic> subTopics;
        private Dictionary<string, MqttSubJS> subJSs;
        private Dictionary<string, MqttSubCmd> subCmds;


        public KpMqttLogic(int number) : base(number)
        {
            ConnRequired = false;
            CanSendCmd = true;
            WorkState = WorkStates.Normal;
        }

        private void InitDeviceTags()
        {
            List<TagGroup> tagGroups = new List<TagGroup>();
            int signal = 1;

            if (deviceConfig.SubTopics.Count > 0)
            {
                TagGroup tagGroup = new TagGroup("Subscriptions");
                tagGroups.Add(tagGroup);

                foreach (MqttSubTopic subTopic in deviceConfig.SubTopics)
                {
                    subTopic.TagIndex = signal - 1;

                    tagGroup.KPTags.Add(new KPTag
                    {
                        Signal = signal++,
                        Name = subTopic.TopicName
                    });
                }
            }

            foreach (MqttSubJS subJS in deviceConfig.SubJSs)
            {
                TagGroup tagGroup = new TagGroup(subJS.TopicName);
                tagGroups.Add(tagGroup);
                subJS.TagIndex = signal - 1;

                for (int i = 0, cnt = subJS.CnlCnt; i < cnt; i++)
                {
                    tagGroup.KPTags.Add(new KPTag
                    {
                        Signal = signal++,
                        Name = subJS.TopicName + " [" + i + "]"
                    }); ;
                }
            }

            InitKPTags(tagGroups);
        }

        private void InitSubscribePacket()
        {
            subscribePacket = new SubscribePacket();
            subTopics = new Dictionary<string, MqttSubTopic>();
            subJSs = new Dictionary<string, MqttSubJS>();
            subCmds = new Dictionary<string, MqttSubCmd>();

            int subscrIdx = 0;
            int subscrCnt = deviceConfig.SubTopics.Count + deviceConfig.SubJSs.Count + deviceConfig.SubCmds.Count;

            subscribePacket.Topics = new string[subscrCnt];
            subscribePacket.QosLevels = new MqttQos[subscrCnt];

            foreach (MqttSubTopic subTopic in deviceConfig.SubTopics)
            {
                subscribePacket.Topics[subscrIdx] = subTopic.TopicName;
                subscribePacket.QosLevels[subscrIdx] = subTopic.QosLevel;
                subTopics[subTopic.TopicName] = subTopic;
                subscrIdx++;
            }

            foreach (MqttSubJS subJS in deviceConfig.SubJSs)
            {
                subscribePacket.Topics[subscrIdx] = subJS.TopicName;
                subscribePacket.QosLevels[subscrIdx] = subJS.QosLevel;
                subJSs[subJS.TopicName] = subJS;
                subscrIdx++;
            }

            foreach (MqttSubCmd subCmd in deviceConfig.SubCmds)
            {
                subscribePacket.Topics[subscrIdx] = subCmd.TopicName;
                subscribePacket.QosLevels[subscrIdx] = subCmd.QosLevel;
                subCmds[subCmd.TopicName] = subCmd;
                subscrIdx++;
            }
        }

        private void ResumeOutgoingFlows()
        {
            foreach (OutgoingFlow flow in mqttPersistence.GetPendingOutgoingFlows())
            {
                Resume(flow);
            }
        }

        private void Resume(OutgoingFlow flow)
        {
            if (flow.Qos == MqttQos.AtLeastOnce || (flow.Qos == MqttQos.ExactlyOnce && !flow.Received))
            {
                PublishPacket packet = new PublishPacket()
                {
                    PacketId = flow.PacketId,
                    QosLevel = flow.Qos,
                    Topic = flow.Topic,
                    Message = flow.Payload,
                    DupFlag = true
                };

                Publish(packet);
            }
            else if (flow.Qos == MqttQos.ExactlyOnce && flow.Received)
            {
                Pubrel(flow.PacketId);
            }

            mqttPersistence.LastOutgoingPacketId = flow.PacketId;
        }

        private ushort Publish(PublishPacket packet)
        {
            if (packet.QosLevel != MqttQos.AtMostOnce)
            {
                if (packet.PacketId == 0)
                    packet.PacketId = GetNextPacketId();

                mqttPersistence.RegisterOutgoingFlow(new OutgoingFlow
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
            PacketBase packet = mqttTransport.Read();

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
            if (mqttTransport.IsClosed)
            {
                lastCommSucc = false;
                WriteToLog(Localization.UseRussian ?
                    "Ошибка: попытка отправить пакет в закрытом состоянии транспорта" :
                    "Error: attempt to send packet while the transport is closed");
            }
            else
            {
                WriteToLog("Send packet");
                mqttTransport.Write(packet);
            }
        }

        private ushort GetNextPacketId()
        {
            ushort x = mqttPersistence.LastOutgoingPacketId;
            if (x == Packet.MaxPacketId)
            {
                mqttPersistence.LastOutgoingPacketId = 1;
                return 1;
            }
            else
            {
                x += 1;
                mqttPersistence.LastOutgoingPacketId = x;
                return x;
            }
        }

        private void ReceivePacket()
        {
            try
            {
                WriteToLog("Receive packet");
                PacketBase packet = mqttTransport.Read();
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
            string packetMsg = Encoding.UTF8.GetString(packet.Message);
            WriteToLog(packet.Topic + " = " + packetMsg);

            // get subscription data
            if (subTopics.TryGetValue(packet.Topic, out MqttSubTopic subTopic))
            {
                double tagVal = ScadaUtils.StrToDouble(packetMsg);

                if (double.IsNaN(tagVal))
                    InvalidateCurData(subTopic.TagIndex, 1);
                else
                    SetCurData(subTopic.TagIndex, tagVal, 1);
            }

            // execute JavaScript
            if (subJSs.TryGetValue(packet.Topic, out MqttSubJS subJS))
            {
                Engine jsEngine = new Engine();
                JsVal[] jsvals = new JsVal[subJS.CnlCnt];

                for (int i = 0; i < subJS.CnlCnt; i++)
                {
                    jsvals[i] = new JsVal();
                }

                jsEngine.SetValue("mqttJS", subJS);
                jsEngine.SetValue("InMsg", packetMsg);
                jsEngine.SetValue("jsvals", jsvals);
                jsEngine.SetValue("mylog", new Action<string>(WriteToLog));

                try
                {
                    jsEngine.Execute(subJS.JSHandler);

                    for (int i = 0; i < subJS.CnlCnt; i++)
                    {
                        JsVal jsval = jsvals[i];
                        int tagIndex = subJS.TagIndex + i;
                        SetCurData(tagIndex, jsval.Val, jsval.Stat);

                        if (!string.IsNullOrEmpty(jsval.TagName))
                            KPTags[tagIndex].Name = jsval.TagName;
                    }
                }
                catch (Exception ex)
                {
                    WriteToLog((Localization.UseRussian ? 
                        "Ошибка выполнения JavaScript: " :
                        "Error executing JavaScript: ") + ex.Message);

                    InvalidateCurData(subJS.TagIndex, subJS.CnlCnt);
                }
            }

            // send commands to Server
            if (subCmds.TryGetValue(packet.Topic, out MqttSubCmd subCmd))
            {
                switch (subCmd.CmdType)
                {
                    case CmdType.St:
                        double cmdVal = ScadaUtils.StrToDouble(packetMsg);

                        if (double.IsNaN(cmdVal))
                        {
                            WriteToLog(CommonPhrases.RealRequired);
                        }
                        else
                        {
                            CommLineSvc.ServerComm?.SendStandardCommand(
                                subCmd.IDUser, subCmd.NumCnlCtrl, cmdVal, out bool result1);
                        }
                        break;

                    case CmdType.BinTxt:
                        CommLineSvc.ServerComm?.SendBinaryCommand(
                            subCmd.IDUser, subCmd.NumCnlCtrl, packet.Message, out bool result2);
                        break;

                    case CmdType.BinHex:
                        if (ScadaUtils.HexToBytes(packetMsg.Trim(), out byte[] cmdData))
                        {
                            CommLineSvc.ServerComm?.SendBinaryCommand(
                                subCmd.IDUser, subCmd.NumCnlCtrl, cmdData, out bool result3);
                        }
                        else
                        {
                            WriteToLog(CommonPhrases.NotHexadecimal);
                        }
                        break;

                    case CmdType.Req:
                        if (int.TryParse(packetMsg, out int kpNum))
                        {
                            CommLineSvc.ServerComm?.SendRequestCommand(
                                subCmd.IDUser, subCmd.NumCnlCtrl, kpNum, out bool result4);
                        }
                        else
                        {
                            WriteToLog(CommonPhrases.IntegerRequired);
                        }
                        break;
                }
            }

            // process QOS
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
            if (!mqttPersistence.IsIncomingFlowRegistered(packet.PacketId))
            {
                // Register the incoming packetId, so duplicate messages can be filtered.
                // This is done after "ProcessIncomingPublish" because we can't assume the
                // mesage was received in the case that method throws an exception.
                mqttPersistence.RegisterIncomingFlow(packet.PacketId);

                // the ideal would be to run `PubishReceived` and `Persistence.RegisterIncomingFlow`
                // in a single transaction (either both or neither succeeds).
            }

            Send(new PubrecPacket { PacketId = packet.PacketId });
        }

        private void OnPubrelReceived(PubrelPacket packet)
        {
            mqttPersistence.ReleaseIncomingFlow(packet.PacketId);
            Send(new PubcompPacket { PacketId = packet.PacketId });
        }

        // -- outgoing publish events --

        private void OnPubackReceived(PubackPacket packet)
        {
            mqttPersistence.SetOutgoingFlowCompleted(packet.PacketId);
        }

        private void OnPubrecReceived(PubrecPacket packet)
        {
            mqttPersistence.SetOutgoingFlowReceived(packet.PacketId);
            Send(new PubrelPacket { PacketId = packet.PacketId });
        }

        private void OnPubcompReceived(PubcompPacket packet)
        {
            mqttPersistence.SetOutgoingFlowCompleted(packet.PacketId);
        }

        // -- subscription events --
        private void OnSubackReceived(SubackPacket packet)
        {

        }

        private void OnUnsubackReceived(UnsubackPacket packet)
        {

        }

        private void Connect()
        {
            MqttConnectionArgs connArgs = deviceConfig.ConnectionArgs;
            connArgs.ReadTimeout = TimeSpan.FromMilliseconds(ReqParams.Timeout);
            connArgs.WriteTimeout = TimeSpan.FromMilliseconds(ReqParams.Timeout);

            mqttPersistence = new InMemoryPersistence();
            mqttTransport = new TcpTransport(connArgs.Hostname, connArgs.Port) { Version = connArgs.Version };
            mqttTransport.SetTimeouts(connArgs.ReadTimeout, connArgs.WriteTimeout);

            Send(MakeConnectMessage(connArgs));

            if (ReceiveConnack())
            {
                ResumeOutgoingFlows();

                if (subscribePacket.Topics.Length > 0)
                    Subscribe(subscribePacket);

                WriteToLog(Localization.UseRussian ?
                    "Соединение установлено" :
                    "Connection established");
            }
        }

        private void Disconnect()
        {
            Send(new DisconnectPacket());
            mqttTransport.Close();
            WriteToLog(Localization.UseRussian ? 
                "Отключение от MQTT брокера" : 
                "Disconnect from MQTT broker");
        }

        private bool Reconnect(out string errMsg)
        {
            try
            {
                if (mqttTransport.IsClosed)
                    Connect();

                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }

        private List<MqttPubTopic> GetValues(List<MqttPubTopic> pubTopics)
        {
            if (CommLineSvc.ServerComm != null && pubTopics.Count > 0)
            {
                SrezTableLight srezTable = new SrezTableLight();
                CommLineSvc.ServerComm.ReceiveSrezTable("current.dat", srezTable);

                if (srezTable.SrezList.Count > 0)
                {
                    SrezTableLight.Srez srez = srezTable.SrezList.Values[0];

                    foreach (MqttPubTopic pubTopic in pubTopics)
                    {
                        if (srez.GetCnlData(pubTopic.NumCnl, out SrezTableLight.CnlData cnlData))
                        {
                            if (pubTopic.PubBehavior == PubBehavior.OnChange)
                            {
                                if (pubTopic.Value != cnlData.Val)
                                    pubTopic.IsPub = true;
                            }
                            else if (pubTopic.PubBehavior == PubBehavior.OnAlways)
                            {
                                pubTopic.IsPub = true;
                            }

                            pubTopic.Value = cnlData.Val;
                        }
                    }
                }
            }

            return pubTopics;
        }

        public override void Session()
        {
            base.Session();

            if (deviceConfig == null)
            {
                lastCommSucc = false;
                WriteToLog(CommPhrases.NormalKpExecImpossible);
            }
            else if (!Reconnect(out string errMsg))
            {
                lastCommSucc = false;
                WriteToLog(string.Format(Localization.UseRussian ? 
                    "Ошибка при повторном подключении: {0}" : 
                    "Error reconnecting: {0}", errMsg));
            }
            else
            {
                // receive data
                if (mqttTransport.Poll(PollTimeout))
                    ReceivePacket();
                else
                    Send(new PingreqPacket()); // send ping request

                // publish data
                foreach (MqttPubTopic pubTopic in GetValues(deviceConfig.PubTopics))
                {
                    if (!pubTopic.IsPub)
                        continue;

                    Nfi.NumberDecimalSeparator = pubTopic.DecimalSeparator;

                    Publish(new PublishPacket
                    {
                        Topic = pubTopic.TopicName,
                        QosLevel = pubTopic.QosLevel,
                        Retain = pubTopic.Retain,
                        Message = Encoding.UTF8.GetBytes(pubTopic.Prefix + pubTopic.Value.ToString(Nfi) + pubTopic.Suffix)
                    });

                    pubTopic.IsPub = false;
                }  
            }

            Thread.Sleep(ReqParams.Delay);
            CalcSessStats();
        }

        public override void OnAddedToCommLine()
        {
            deviceConfig = new DeviceConfig();
            string configFileName = DeviceConfig.GetFileName(AppDirs.ConfigDir, Number, ReqParams.CmdLine);

            if (deviceConfig.Load(configFileName, out string errMsg))
            {
                InitDeviceTags();
                InitSubscribePacket();
            }
            else
            {
                deviceConfig = null;
                WriteToLog(errMsg);
            }
        }

        public override void OnCommLineStart()
        {
            if (deviceConfig != null)
                Connect();
        }

        public override void SendCmd(Command cmd)
        {
            base.SendCmd(cmd);

            if (deviceConfig == null)
            {
                lastCommSucc = false;
                WriteToLog(CommPhrases.NormalKpExecImpossible);
            }
            else
            {
                foreach (MqttPubCmd pubCmd in deviceConfig.PubCmds)
                {
                    if (pubCmd.NumCmd == cmd.CmdNum)
                    {
                        if (cmd.CmdTypeID == BaseValues.CmdTypes.Standard)
                        {
                            Publish(new PublishPacket()
                            {
                                Topic = pubCmd.TopicName,
                                QosLevel = pubCmd.QosLevel,
                                Retain = pubCmd.Retain,
                                Message = Encoding.UTF8.GetBytes(cmd.CmdVal.ToString(NumberFormatInfo.InvariantInfo))
                            });
                        }
                        else if (cmd.CmdTypeID == BaseValues.CmdTypes.Binary)
                        {
                            Publish(new PublishPacket()
                            {
                                Topic = pubCmd.TopicName,
                                QosLevel = pubCmd.QosLevel,
                                Retain = pubCmd.Retain,
                                Message = cmd.CmdData
                            });
                        }
                    }
                }
            }

            CalcCmdStats();
        }

        public override void OnCommLineTerminate()
        {
            if (deviceConfig != null)
                Disconnect();
        }
    }
}
