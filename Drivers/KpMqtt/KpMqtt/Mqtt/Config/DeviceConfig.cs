using StriderMqtt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Scada.Comm.Devices.Mqtt.Config
{
    /// <summary>
    /// Represents a driver configuration for a device.
    /// <para>Представляет конфигурацию драйвера для устройства.</para>
    /// </summary>
    internal class DeviceConfig
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DeviceConfig()
        {
            SetToDefault();
        }


        /// <summary>
        /// Gets the MQTT connection options.
        /// </summary>
        public MqttConnectionArgs ConnectionArgs { get; private set; }

        /// <summary>
        /// Gets the subscriptions.
        /// </summary>
        public List<MqttSubTopic> SubTopics { get; private set; }

        /// <summary>
        /// Gets the subscriptions processed by JavaScript.
        /// </summary>
        public List<MqttSubJS> SubJSs { get; private set; }

        /// <summary>
        /// Gets the topics to publish.
        /// </summary>
        public List<MqttPubTopic> PubTopics { get; private set; }

        /// <summary>
        /// Gets the commands to publish data when a telecommand is sent.
        /// </summary>
        public List<MqttPubCmd> PubCmds { get; private set; }

        /// <summary>
        /// Gets the commands sent to Server when new data is received.
        /// </summary>
        public List<MqttSubCmd> SubCmds { get; private set; }


        /// <summary>
        /// Sets the default values.
        /// </summary>
        private void SetToDefault()
        {
            ConnectionArgs = new MqttConnectionArgs();
            SubTopics = new List<MqttSubTopic>();
            PubTopics = new List<MqttPubTopic>();
            PubCmds = new List<MqttPubCmd>();
            SubCmds = new List<MqttSubCmd>();
            SubJSs = new List<MqttSubJS>();
        }

        /// <summary>
        /// Loads the configuration from the specified file.
        /// </summary>
        public bool Load(string fileName, out string errMsg)
        {
            try
            {
                SetToDefault();

                if (!File.Exists(fileName))
                    throw new FileNotFoundException(string.Format(CommonPhrases.NamedFileNotFound, fileName));

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(fileName);
                XmlElement rootElem = xmlDoc.DocumentElement;

                if (rootElem.SelectSingleNode("MqttParams") is XmlElement mqttParamsElem)
                {
                    ConnectionArgs.ClientId = mqttParamsElem.GetAttribute("ClientID");
                    ConnectionArgs.Hostname = mqttParamsElem.GetAttribute("Hostname");
                    ConnectionArgs.Port = mqttParamsElem.GetAttrAsInt("Port");
                    ConnectionArgs.Username = mqttParamsElem.GetAttribute("UserName");
                    ConnectionArgs.Password = mqttParamsElem.GetAttribute("Password");
                }

                if (xmlDoc.DocumentElement.SelectSingleNode("MqttSubTopics") is XmlNode mqttSubTopicsNode)
                {
                    foreach (XmlElement topicElem in mqttSubTopicsNode.SelectNodes("Topic"))
                    {
                        SubTopics.Add(new MqttSubTopic
                        {
                            TopicName = topicElem.GetAttribute("TopicName"),
                            QosLevel = (MqttQos)topicElem.GetAttrAsInt("QosLevel")
                        });
                    }
                }

                if (xmlDoc.DocumentElement.SelectSingleNode("MqttSubJSs") is XmlNode mqttSubJSsNode)
                {
                    foreach (XmlElement topicElem in mqttSubJSsNode.SelectNodes("Topic"))
                    {
                        MqttSubJS subJS = new MqttSubJS
                        {
                            TopicName = topicElem.GetAttribute("TopicName"),
                            QosLevel = (MqttQos)topicElem.GetAttrAsInt("QosLevel"),
                            CnlCnt = topicElem.GetAttrAsInt("CnlCnt", 1),
                            JSHandlerPath = topicElem.GetAttribute("JSHandlerPath")
                        };

                        if (subJS.LoadJSHandler())
                            SubJSs.Add(subJS);
                    }
                }

                if (xmlDoc.DocumentElement.SelectSingleNode("MqttPubTopics") is XmlNode mqttPubTopicsNode)
                {
                    foreach (XmlElement topicElem in mqttPubTopicsNode.SelectNodes("Topic"))
                    {
                        PubTopics.Add(new MqttPubTopic
                        {
                            TopicName = topicElem.GetAttribute("TopicName"),
                            QosLevel = (MqttQos)topicElem.GetAttrAsInt("QosLevel"),
                            Retain = topicElem.GetAttrAsBool("Retain"),
                            NumCnl = topicElem.GetAttrAsInt("NumCnl"),
                            PubBehavior = topicElem.GetAttrAsEnum<PubBehavior>("PubBehavior"),
                            DecimalSeparator = topicElem.GetAttribute("NDS"),
                            Prefix = topicElem.GetAttribute("Prefix"),
                            Suffix = topicElem.GetAttribute("Suffix")
                        });
                    }
                }

                if (xmlDoc.DocumentElement.SelectSingleNode("MqttPubCmds") is XmlNode mqttPubCmdsNode)
                {
                    foreach (XmlElement topicElem in mqttPubCmdsNode.SelectNodes("Topic"))
                    {
                        PubCmds.Add(new MqttPubCmd
                        {
                            TopicName = topicElem.GetAttribute("TopicName"),
                            QosLevel = (MqttQos)topicElem.GetAttrAsInt("QosLevel"),
                            Retain = false,
                            NumCmd = topicElem.GetAttrAsInt("NumCmd")
                        });
                    }
                }

                if (xmlDoc.DocumentElement.SelectSingleNode("MqttSubCmds") is XmlNode mqttSubCmdsNode)
                {
                    foreach (XmlElement topicElem in mqttSubCmdsNode.SelectNodes("Topic"))
                    {
                        SubCmds.Add(new MqttSubCmd
                        {
                            TopicName = topicElem.GetAttribute("TopicName"),
                            QosLevel = (MqttQos)topicElem.GetAttrAsInt("QosLevel"),
                            CmdType = topicElem.GetAttrAsEnum<CmdType>("CmdType"),
                            IDUser = topicElem.GetAttrAsInt("IDUser"),
                            NumCnlCtrl = topicElem.GetAttrAsInt("NumCnlCtrl"),
                        });
                    }
                }

                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = CommPhrases.LoadKpSettingsError + ": " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Gets the configuration file name.
        /// </summary>
        public static string GetFileName(string configDir, int kpNum, string defaultFileName)
        {
            return string.IsNullOrWhiteSpace(defaultFileName)
                ? Path.Combine(configDir, "KpMqtt_" + CommUtils.AddZeros(kpNum, 3) + ".xml")
                : Path.Combine(configDir, defaultFileName.Trim());
        }
    }
}
