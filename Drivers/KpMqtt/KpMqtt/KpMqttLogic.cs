using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Globalization;
using Scada.Data;
using Scada.Data.Tables;
using Scada.Data.Models;
using Scada.Data.Configuration;
using StriderMqtt;
using Scada.Client;
using Jint;

namespace Scada.Comm.Devices
{

	public class KpMqttLogic : KPLogic
	{
		static readonly TimeSpan MaxPollTime = TimeSpan.FromSeconds(0.1);

		// all in ms
		private int Keepalive;
		private int LastRead;
		private int LastWrite;


		private IMqttTransport Transport;
		private IMqttPersistence Persistence;

		private bool IsSessionPresent { get; set; }

		private bool IsPublishing { get; set; }

		private bool InterruptLoop { get; set; }

		private MqttConnectionArgs connArgs;

		private List<MQTTPubTopic> MQTTPTs;

		private List<MQTTPubCmd> MQTTCmds;

		private RapSrvEx RSrv;

		private SubscribePacket sp;

		private List<MQTTSubCmd> SubCmds;

		private List<MQTTSubJS> SubJSs;

		bool ReadWaitExpired
		{
			get
			{
				if (Keepalive > 0)
				{
					return Environment.TickCount - LastRead > (Keepalive * 1.5);
				}
				else
				{
					return false;
				}
			}
		}

		bool WriteWaitExpired
		{
			get
			{
				if (Keepalive > 0)
				{
					return Environment.TickCount - LastWrite > Keepalive;
				}
				else
				{
					return false;
				}
			}
		}


		public KpMqttLogic (int number) : base (number)
		{
			ConnRequired = false;
			CanSendCmd = true;
			WorkState = WorkStates.Normal;
			Keepalive = 60;
		}

		private void ResumeOutgoingFlows ()
		{

			foreach (OutgoingFlow flow in Persistence.GetPendingOutgoingFlows()) {
				Resume (flow);
			}
		}

		private void Resume (OutgoingFlow flow)
		{
			if (flow.Qos == MqttQos.AtLeastOnce || (flow.Qos == MqttQos.ExactlyOnce && !flow.Received)) {
				PublishPacket publish = new PublishPacket () {
					PacketId = flow.PacketId,
					QosLevel = flow.Qos,
					Topic = flow.Topic,
					Message = flow.Payload,
					DupFlag = true
				};
				Publish (publish);
			} else if (flow.Qos == MqttQos.ExactlyOnce && flow.Received) {
				Pubrel (flow.PacketId);
			}
			Persistence.LastOutgoingPacketId = flow.PacketId;
		}

		private ushort Publish (PublishPacket packet)
		{

			if (packet.QosLevel != MqttQos.AtMostOnce) {
				if (packet.PacketId == 0) {
					packet.PacketId = this.GetNextPacketId ();
				}
				Persistence.RegisterOutgoingFlow (new OutgoingFlow () {
					PacketId = packet.PacketId,
					Topic = packet.Topic,
					Qos = packet.QosLevel,
					Payload = packet.Message
				});
			}

			try {
				IsPublishing = true;
				Send (packet);
				return packet.PacketId;
			} catch {
				IsPublishing = false;
				throw;
			}
		}

		private void Pubrel (ushort packetId)
		{
			try {
				IsPublishing = true;
				Send (new PubrelPacket () { PacketId = packetId });
			} catch {
				IsPublishing = false;
				throw;
			}
		}

		private void Subscribe (SubscribePacket packet)
		{
			if (packet.PacketId == 0) {
				packet.PacketId = this.GetNextPacketId ();
			}

			Send (packet);
		}

		private void Unsubscribe (UnsubscribePacket packet)
		{
			if (packet.PacketId == 0) {
				packet.PacketId = this.GetNextPacketId ();
			}
			Send (packet);
		}


		private ConnectPacket MakeConnectMessage (MqttConnectionArgs args)
		{
			ConnectPacket conn = new ConnectPacket ();
			conn.ProtocolVersion = args.Version;

			conn.ClientId = args.ClientId;
			conn.Username = args.Username;
			conn.Password = args.Password;

			if (args.WillMessage != null) {
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

		private void ReceiveConnack ()
		{
			PacketBase packet = Transport.Read ();
			ConnackPacket connack = packet as ConnackPacket;

			if (packet == null) {
				WriteToLog (Localization.UseRussian ? String.Format ("Первый принимаемый пакет должен быть Connack, но получили {0}", packet.GetType ().Name) : 
													String.Format ("First received message should be Connack, but {0} received instead", packet.GetType ().Name));
				throw new MqttProtocolException (String.Format ("First received message should be Connack, but {0} received instead", packet.GetType ().Name));
			}

			if (connack.ReturnCode != ConnackReturnCode.Accepted) {
				WriteToLog (Localization.UseRussian ? "Соединение не разрешено брокером" : "The connection was not accepted");
				throw new MqttConnectException ("The connection was not accepted", connack.ReturnCode);
			}

			this.IsSessionPresent = connack.SessionPresent;

		}

		private void Send (PacketBase packet)
		{
			if (Transport.IsClosed) {
				WriteToLog (Localization.UseRussian ? "Попытка отправить пакет в закрытом состоянии Transport" : "Tried to send packet while closed");
				throw new MqttClientException ("Tried to send packet while closed");
			}
			Transport.Write (packet);
			LastWrite = Environment.TickCount;
		}

		private ushort GetNextPacketId ()
		{
			ushort x = Persistence.LastOutgoingPacketId;
			if (x == Packet.MaxPacketId) {
				Persistence.LastOutgoingPacketId = 1;
				return 1;
			} else {
				x += 1;
				Persistence.LastOutgoingPacketId = x;
				return x;
			}
		}


		private void ReceivePacket ()
		{
			PacketBase packet = Transport.Read ();
			LastRead = Environment.TickCount;
			HandleReceivedPacket (packet);
		}

		void HandleReceivedPacket (PacketBase packet)
		{
			switch (packet.PacketType) {
			case PublishPacket.PacketTypeCode:
				OnPublishReceived (packet as PublishPacket);
				break;
			case PubackPacket.PacketTypeCode:
				OnPubackReceived (packet as PubackPacket);
				break;
			case PubrecPacket.PacketTypeCode:
				OnPubrecReceived (packet as PubrecPacket);
				break;
			case PubrelPacket.PacketTypeCode:
				OnPubrelReceived (packet as PubrelPacket);
				break;
			case PubcompPacket.PacketTypeCode:
				OnPubcompReceived (packet as PubcompPacket);
				break;
			case SubackPacket.PacketTypeCode:
				OnSubackReceived (packet as SubackPacket);
				break;
			case UnsubackPacket.PacketTypeCode:
				OnUnsubackReceived (packet as UnsubackPacket);
				break;
			case PingrespPacket.PacketTypeCode:
				break;
			default:
				WriteToLog (Localization.UseRussian ? String.Format ("Не возможно принять пакет типа {0}", packet.GetType ().Name) : String.Format ("Cannot receive message of type {0}", packet.GetType ().Name));
				throw new MqttProtocolException (String.Format ("Cannot receive message of type {0}", packet.GetType ().Name));
			}
		}


		// -- incoming publish events --

		void OnPublishReceived (PublishPacket packet)
		{
			//WriteToLog (Encoding.UTF8.GetString(packet.Message));
			//WriteToLog (packet.Topic);

			string pv = Encoding.UTF8.GetString (packet.Message);
			Regex reg = new Regex (@"^[-]?\d+[,.]?\d+$");
			Regex reg2 = new Regex (@"^[-\d]+$");
            
            
			if (SubJSs.Count >0){
				Engine jsEng = new Engine();

				foreach(MQTTSubJS mqttjs in SubJSs){
					if (mqttjs.TopicName == packet.Topic){
						
						SrezTableLight.Srez srez = new SrezTableLight.Srez(DateTime.Now, mqttjs.CnlCnt);
						List<JSVal> jsvals = new List<JSVal>();
						for (int i = 0; i < srez.CnlNums.Length;i++)
						{
							JSVal jsval = new JSVal();
                            
							jsvals.Add(jsval);
						}
                  
						jsEng.SetValue("mqttJS", mqttjs);
						jsEng.SetValue("InMsg", Encoding.UTF8.GetString(packet.Message));
						jsEng.SetValue("jsvals", jsvals);
						jsEng.SetValue("mylog", new Action<string>(WriteToLog));
                  
						bool sndres;
                        try
						{
							jsEng.Execute(mqttjs.JSHandler);
							int i = 0;
	                     
                            foreach(JSVal jsvl in jsvals)
							{
								
								SrezTableLight.CnlData cnlData = new SrezTableLight.CnlData();
								cnlData.Stat = jsvl.Stat;
								cnlData.Val = jsvl.Val;
								srez.CnlNums[i] = jsvl.CnlNum;
								srez.SetCnlData(jsvl.CnlNum,cnlData);
								i++;

							}

							bool snd = RSrv.SendSrez(srez, out sndres);

						}
                        catch
						{
							WriteToLog(Localization.UseRussian ? "Ошибка обработки JS" : "Error execute JS");
						}
						break;
					}
				}

			}


			if (SubCmds.Count > 0) {
				bool IsResSendCmd;
				bool IsSendCmd;
				foreach (MQTTSubCmd mqttcmd in SubCmds) {
					if (mqttcmd.TopicName == packet.Topic) {
						if (mqttcmd.CmdType == "St") {
							if (reg.IsMatch (pv)) {
								pv = pv.Replace ('.', ',');
							}
                            mqttcmd.CmdVal = ScadaUtils.StrToDouble(pv);
							IsSendCmd = RSrv.SendStandardCommand (mqttcmd.IDUser, mqttcmd.NumCnlCtrl, mqttcmd.CmdVal,out IsResSendCmd);
							break;
						}
						if (mqttcmd.CmdType == "BinTxt") {
							IsSendCmd = RSrv.SendBinaryCommand (mqttcmd.IDUser, mqttcmd.NumCnlCtrl, packet.Message,out IsResSendCmd);
							break;
						}
						if(mqttcmd.CmdType == "BinHex"){
							byte[] cmdData;
							bool IsHexToByte = ScadaUtils.HexToBytes (pv.Trim (),out cmdData);
							if(IsHexToByte){
								IsSendCmd = RSrv.SendBinaryCommand (mqttcmd.IDUser, mqttcmd.NumCnlCtrl, cmdData,out IsResSendCmd);
							}
							break;
						}
						if(mqttcmd.CmdType == "Req"){
							IsSendCmd = RSrv.SendRequestCommand (mqttcmd.IDUser, mqttcmd.NumCnlCtrl, mqttcmd.KPNum,out IsResSendCmd);
							break;
						}
					}
				}
			}


			if(KPTags.Length >0){
				int tagInd = 0;
				foreach(KPTag kpt in KPTags){
					if(kpt.Name == packet.Topic){
						if(reg.IsMatch(pv)){
							pv = pv.Replace ('.', ',');
                            SetCurData (tagInd, ScadaUtils.StrToDouble(pv), 1);
                            WriteToLog(pv);
							break;
						}
						if(reg2.IsMatch(pv)){
                            SetCurData (tagInd, ScadaUtils.StrToDouble(pv), 1);
                            WriteToLog(pv);
                            break;
						}
					}
					tagInd++;
				}
			}
				

			if (packet.QosLevel == MqttQos.ExactlyOnce) {
				OnQos2PublishReceived (packet);
			} else {

				if (packet.QosLevel == MqttQos.AtLeastOnce) {
					Send (new PubackPacket () { PacketId = packet.PacketId });
				}
			}
			
		}


		void OnQos2PublishReceived (PublishPacket packet)
		{
			if (!Persistence.IsIncomingFlowRegistered (packet.PacketId)) {

				// Register the incoming packetId, so duplicate messages can be filtered.
				// This is done after "ProcessIncomingPublish" because we can't assume the
				// mesage was received in the case that method throws an exception.
				Persistence.RegisterIncomingFlow (packet.PacketId);

				// the ideal would be to run `PubishReceived` and `Persistence.RegisterIncomingFlow`
				// in a single transaction (either both or neither succeeds).
			}

			Send (new PubrecPacket () { PacketId = packet.PacketId });
		}

		void OnPubrelReceived (PubrelPacket packet)
		{

			Persistence.ReleaseIncomingFlow (packet.PacketId);
			Send (new PubcompPacket () { PacketId = packet.PacketId });
		}


		// -- outgoing publish events --

		void OnPubackReceived (PubackPacket packet)
		{
			Persistence.SetOutgoingFlowCompleted (packet.PacketId);
			this.IsPublishing = false;
		}

		void OnPubrecReceived (PubrecPacket packet)
		{
			Persistence.SetOutgoingFlowReceived (packet.PacketId);
			Send (new PubrelPacket () { PacketId = packet.PacketId });
		}

		void OnPubcompReceived (PubcompPacket packet)
		{
			Persistence.SetOutgoingFlowCompleted (packet.PacketId);
			this.IsPublishing = false;
		}


		// -- subscription events --

		void OnSubackReceived (SubackPacket packet)
		{

		}

		void OnUnsubackReceived (UnsubackPacket packet)
		{
			
		}

		private bool Poll(int pollTime)
		{
			return Transport.Poll (Math.Min (pollTime, (int)MaxPollTime.TotalMilliseconds));
		}

		public override void Session ()
		{
			base.Session ();

			WorkState = WorkStates.Normal;

			int readThreshold = Environment.TickCount + Keepalive;
			int pollTime = readThreshold - Environment.TickCount;
			if(pollTime>0)
			{
				if(Poll(pollTime))
				{
					ReceivePacket ();
				}
				else if (WriteWaitExpired)
				{
					Send (new PingreqPacket ());
				}
				else if (ReadWaitExpired)
				{
					WriteToLog ("MQTT timeout exception");
				}
			}
				


			MQTTPTs = RSrv.GetValues (MQTTPTs);
			NumberFormatInfo nfi = new NumberFormatInfo ();

			foreach (MQTTPubTopic mqtttp in MQTTPTs) {
				if (!mqtttp.IsPub)
					continue; 
				nfi.NumberDecimalSeparator = mqtttp.NumberDecimalSeparator;
				Publish (new PublishPacket () {
					Topic = mqtttp.TopicName,
					QosLevel = mqtttp.QosLevels,
					Retain = mqtttp.Retain,
                    Message = Encoding.UTF8.GetBytes (mqtttp.Value.ToString(nfi))
				});
				mqtttp.IsPub = false;
			}

			//Thread.Sleep (ReqParams.Delay);
		}




		public override void OnAddedToCommLine ()
		{
	

			
			List<TagGroup> tagGroups = new List<TagGroup> ();
			TagGroup tagGroup = new TagGroup ("GroupMQTT");
			TagGroup tagGroupJS = new TagGroup("GoupJS");

			XmlDocument xmlDoc = new XmlDocument ();
			string filename = ReqParams.CmdLine.Trim ();
			xmlDoc.Load (AppDirs.ConfigDir + filename);
			
			XmlNode MQTTSubTopics = xmlDoc.DocumentElement.SelectSingleNode ("MqttSubTopics");
			XmlNode MQTTPubTopics = xmlDoc.DocumentElement.SelectSingleNode ("MqttPubTopics");
			XmlNode MQTTPubCmds = xmlDoc.DocumentElement.SelectSingleNode ("MqttPubCmds");
			XmlNode MQTTSubCmds = xmlDoc.DocumentElement.SelectSingleNode ("MqttSubCmds");
			XmlNode MQTTSubJSs = xmlDoc.DocumentElement.SelectSingleNode("MqttSubJSs");
			XmlNode RapSrvCnf = xmlDoc.DocumentElement.SelectSingleNode ("RapSrvCnf");
			XmlNode MQTTSettings = xmlDoc.DocumentElement.SelectSingleNode ("MqttParams");

			CommSettings cs = new CommSettings () {
				ServerHost = RapSrvCnf.Attributes.GetNamedItem ("ServerHost").Value,
				ServerPort = Convert.ToInt32 (RapSrvCnf.Attributes.GetNamedItem ("ServerPort").Value),
				ServerUser = RapSrvCnf.Attributes.GetNamedItem ("ServerUser").Value,
				ServerPwd = RapSrvCnf.Attributes.GetNamedItem ("ServerPwd").Value
			};

			RSrv = new RapSrvEx (cs);
			RSrv.Conn ();
			MQTTPTs = new List<MQTTPubTopic> ();
			MQTTCmds = new List<MQTTPubCmd> ();

			foreach (XmlElement MqttPTCnf in MQTTPubTopics) {
				MQTTPubTopic MqttPT = new MQTTPubTopic () {
					NumCnl = Convert.ToInt32 (MqttPTCnf.GetAttribute ("NumCnl")),
					QosLevels = (MqttQos)Convert.ToByte (MqttPTCnf.GetAttribute ("QosLevel")),
					TopicName = MqttPTCnf.GetAttribute ("TopicName"),
					PubBehavior = MqttPTCnf.GetAttribute ("PubBehavior"),
					NumberDecimalSeparator = MqttPTCnf.GetAttribute ("NDS"),
					Value = 0
				};
				MQTTPTs.Add (MqttPT);
			}

			foreach (XmlElement MqttPTCnf in MQTTPubCmds){
				MQTTPubCmd MqttPTCmd = new MQTTPubCmd () {
					NumCmd = MqttPTCnf.GetAttrAsInt("NumCmd"),
					QosLevels = (MqttQos)Convert.ToByte (MqttPTCnf.GetAttribute ("QosLevel")),
					Retain = false,
					TopicName = MqttPTCnf.GetAttribute ("TopicName")
				};
				MQTTCmds.Add (MqttPTCmd);
			}
				
			sp = new SubscribePacket ();
			int i = 0;
			int spCnt = MQTTSubTopics.ChildNodes.Count;
			spCnt += MQTTSubCmds.ChildNodes.Count;
			spCnt += MQTTSubJSs.ChildNodes.Count;
            
			sp.Topics = new string[spCnt];
			sp.QosLevels = new MqttQos[spCnt];
	
			foreach (XmlElement elemGroupElem in MQTTSubTopics.ChildNodes) {
				sp.Topics [i] = elemGroupElem.GetAttribute ("TopicName");
				sp.QosLevels [i] = (MqttQos)Convert.ToByte (elemGroupElem.GetAttribute ("QosLevel"));
				KPTag KPt = new KPTag () {
					Signal = i + 1,
					Name = sp.Topics [i],
					CnlNum = Convert.ToInt32 (elemGroupElem.GetAttribute ("NumCnl"))
				};
				tagGroup.KPTags.Add (KPt);
				i++;
			}
			tagGroups.Add (tagGroup);
			InitKPTags(tagGroups);
            

            

			SubCmds = new List<MQTTSubCmd> ();

			foreach (XmlElement elemGroupElem in MQTTSubCmds.ChildNodes) {
				sp.Topics [i] = elemGroupElem.GetAttribute ("TopicName");
				sp.QosLevels [i] = (MqttQos)Convert.ToByte (elemGroupElem.GetAttribute ("QosLevel"));
				MQTTSubCmd cmd = new MQTTSubCmd () {
					TopicName = sp.Topics[i],
					CmdNum = elemGroupElem.GetAttrAsInt ("NumCmd", 0),
					CmdType = elemGroupElem.GetAttribute ("CmdType"),
					KPNum = elemGroupElem.GetAttrAsInt("KPNum",0),
					IDUser = elemGroupElem.GetAttrAsInt("IDUser",0),
					NumCnlCtrl = elemGroupElem.GetAttrAsInt("NumCnlCtrl",0)
				};
				SubCmds.Add (cmd);
				i++;
			}

			SubJSs = new List<MQTTSubJS>();

			foreach(XmlElement elemGroupElem in MQTTSubJSs.ChildNodes){
				sp.Topics[i] = elemGroupElem.GetAttribute("TopicName");
				sp.QosLevels[i] = (MqttQos)Convert.ToByte(elemGroupElem.GetAttribute("QosLevel"));
				MQTTSubJS msjs = new MQTTSubJS()
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




            
			connArgs = new MqttConnectionArgs ();
			connArgs.ClientId = MQTTSettings.Attributes.GetNamedItem ("ClientID").Value;
			connArgs.Hostname = MQTTSettings.Attributes.GetNamedItem("Hostname").Value;
			connArgs.Port = Convert.ToInt32 (MQTTSettings.Attributes.GetNamedItem ("Port").Value);
			connArgs.Username = MQTTSettings.Attributes.GetNamedItem ("UserName").Value;
			connArgs.Password = MQTTSettings.Attributes.GetNamedItem ("Password").Value;
			connArgs.Keepalive = TimeSpan.FromSeconds (60);
			connArgs.ReadTimeout = TimeSpan.FromSeconds (10);
			connArgs.WriteTimeout = TimeSpan.FromSeconds (10);

			this.Persistence = new InMemoryPersistence ();
			Transport = new TcpTransport (connArgs.Hostname, connArgs.Port);
			Transport.Version = connArgs.Version;
			Transport.SetTimeouts (connArgs.ReadTimeout, connArgs.WriteTimeout);

			Send (MakeConnectMessage (connArgs));
			ReceiveConnack ();
			ResumeOutgoingFlows ();

			if(sp.Topics.Length > 0)
				Subscribe (sp);
         
			WriteToLog (Localization.UseRussian ? "Инициализация линии связи выполнена успешно." : "Communication line initialized successfully");



		}

		public override void SendCmd(Command cmd)
		{
			base.SendCmd (cmd);

			foreach(MQTTPubCmd mpc in MQTTCmds)
			{
				if (mpc.NumCmd != cmd.CmdNum)
					continue;
				if (cmd.CmdTypeID == BaseValues.CmdTypes.Standard)
				{
					NumberFormatInfo nfi = new NumberFormatInfo ();
					nfi.NumberDecimalSeparator = ".";
					Publish (new PublishPacket () {
						Topic = mpc.TopicName,
						QosLevel = mpc.QosLevels,
						Retain = mpc.Retain,
						Message = Encoding.UTF8.GetBytes (cmd.CmdVal.ToString (nfi))
					});
				}
				if(cmd.CmdTypeID == BaseValues.CmdTypes.Binary)
				{
					Publish (new PublishPacket () {
						Topic = mpc.TopicName,
						QosLevel = mpc.QosLevels,
						Retain = mpc.Retain,
						Message = cmd.CmdData
					});
				}

			}
		}


		public override void OnCommLineTerminate ()
		{
			RSrv.Disconn ();
			Send (new DisconnectPacket ());
			Transport.Close ();
			WriteToLog (Localization.UseRussian ? "Отключение от MQTT брокера" : "Disconnect from MQTT broker");
			WorkState = WorkStates.Undefined;
		}
	}
	

	

	public class MQTTSubCmd : Command
	{
		public string TopicName { get; set;}
		public int IDUser { get; set;}
		public string CmdType { get; set;}
		public int NumCnlCtrl { get; set;} 
	}

	public class MQTTSubJS
	{
		public string TopicName { get; set; }
		public string JSHandlerPath { get; set; }
		public string JSHandler { get; private set; }
		public int CnlCnt { get; set; }

        public bool LoadJSHandler()
		{
			bool IsJSLoad = false;
			if(JSHandlerPath != "")
			{
				try
				{
					
					StreamReader sr = new StreamReader(JSHandlerPath);
					JSHandler = sr.ReadToEnd();
					IsJSLoad = true;
				}
                catch
				{
					IsJSLoad = false;
				}
			}
			return IsJSLoad;
		}
	}

    public class JSVal
	{
		public int CnlNum { get; set; }
		public int Stat { get; set; }
		public double Val { get; set; }
	}
}


