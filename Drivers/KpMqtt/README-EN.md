KpMqtt
=============================
The KpMqtt.dll driver is an application library for the Scada Communicator of the Rapid SCADA project. Using this driver, can be receive and transmit messages using the MQTT protocol.

Brief Introduction to MQTT Protocol
=============================

MQTT stands for MQ Telemetry Transport. It is a publish/subscribe, extremely simple and lightweight messaging protocol, designed for constrained devices and low-bandwidth, high-latency or unreliable networks. The design principles are to minimise network bandwidth and device resource requirements whilst also attempting to ensure reliability and some degree of assurance of delivery. These principles also turn out to make the protocol ideal of the emerging “machine-to-machine” (M2M) or “Internet of Things” world of connected devices, and for mobile applications where bandwidth and battery power are at a premium.
The specification and other documentation are available via the  http://mqtt.org/documentation

Examples of Building Topics
=============================

Typically, a description of most hierarchical objects can be represented as a tree-like structure. For example, a description of access to Rapid SCADA channels could be presented in the form of the following template for a tree branch:

/rapidobject/rapidkp/rapidcnl , where

-	**Rapidobject** indicates a unique object number from the Rapid SCADA object directory;
-	**Rapidkp** indicates a unique device number from the devices directory;
-	**Rapidcnl** indicates the channel number from the directory of input channels;

As a result, the topic name can be as follows: /1/1/1 (object number / device number / channel number) or /1/1/2. That as a whole will be described by object No. 1, which includes device 1, which in turn has channel No. 1 and No. 2.
Naming topics can be more specific when using custom encoding rules adopted for the automation object. But keep in mind that the total length of the topic is limited by the standard.

MQTT Broker
=============================

The MQTT protocol description [page](http://mqtt.org/) contains information on various implementations of client and server applications. Most often, [Mosquitto server](http://mosquitto.org/)  is used as a MQTT broker for small IoT projects. The project site has a lot of documentation that will allow you to deploy a server for various operating systems, including the Windows operating system. [The Mosquitto project](http://mosquitto.org/)   is actively supported and complies with the latest versions of the MQTT protocol specification. The software packages of this broker are also available for routers based on the OpenWRT OS. There are also open-source projects for installing enterprise-wide brokers that can withstand high loads and provide fault tolerance mechanisms. If it is not possible to install your own MQTT broker, then can be use the services of cloud services deployed by third-party companies. Free access to such brokers usually has some limitations. Examples of such brokers:
-	[IoT Eclipse](http//iot.eclipse.org/)
-	[CloudMQTT](https//www.cloudmqtt.com/)
-	[HiveMQ](https://www.hivemq.com/)

For testing performance and other tasks, it is convenient to use the graphical client application for working with the MQTT broker. An overview of such applications can be found [here.](https://hivemq.com/blog/seven-best-mqtt-client-tools/)

Configuring the KpMqtt Driver for Scada Communicator
=============================

KpMqtt driver setup can be divided into 2 stages:
- Creating a configuration file for the **KpMqtt** driver
- Creating a communication line, adding device and linking to it a configuration file
When creating a configuration file for the KpMqtt driver, the following block sections and their attributes are used:
-	**MqttParams** section - this section contains attributes designed to configure the connection with the MQTT broker.
	-	**Hostname** attribute - this attribute must contain the DNS (IP) address of the MQTT broker. (Example: iot.eclipse.org)
	-	**ClientID**  attribute - this attribute must contain a unique identifier (name) of the client to connect to the MQTT broker.
	-	**Port** attribute - this attribute must contain the port number on which the MQTT broker accepts connections from client applications.
	-	**Username** and **Password** attributes - these attributes contain the username and password for accessing the MQTT broker. They can be empty if authorization is not used and the broker allows connection in this mode.

-	**RapSrvCnf** section - this section contains attributes designed to connect to the Rapid SCADA server.
	-	**ServerHost** attribute - this attribute must contain the IP address of the Rapid SCADA server.
	-	**ServerPort**  attribute - this attribute must contain the port number on which the Rapid SCADA server accepts connections from client applications..
	-	**ServerUser** and **ServerPwd** attributes - these attributes contain the username and password for accessing the Rapid SCADA server.

-	**MqttSubTopics** section - this section is the parent of the Topic section and is used to configure topic subscriptions.

-	**MqttPubTopics** section - this section is the parent of the Topic section and is used to configure topic publishing.

-	**MqttPubCmds** section - this section is the parent of the Topic section and is used to configure the publication of Rapid SCADA commands.

-	**MqttSubCmds** section - this section is the parent of the Topic section and is intended to configure the subscription to commands coming from the MQTT broker as a string.

-	**MqttSubJSs** section - this section is the parent of the Topic section and is used to configure the subscription to messages coming from the MQTT broker in JSON format.

-	**Topic** section - this section is a child of the MqttPubTopics, MqttSubTopics, MqttPubCmds, MqttSubCmds, MqttSubJSs sections and is intended for topic configuration.

	-	**TopicName** attribute - this attribute must contain the identifier of the topic in the form of a URI address. This address determines the placement of the topic in the tree that describes the structure of the real object.

	-	**QosLevel** attribute - this attribute defines the value that affects the guarantee of message delivery between the client and the broker. This attribute can have the following values: 0 - message delivery is guaranteed at the TCP / IP protocol level. This level provides maximum performance. 1 - delivery of a message to the broker from one or more attempts is guaranteed. At this level, a slight decrease in performance occurs. 2 - delivery of the message to the broker and the client is guaranteed. At this level, maximum reliability is ensured with minimal performance.

	-	**NumCnl** attribute - this attribute defines the channel number from the Rapid SCADA configuration table.

	-	**PubBehavior** attribute - this attribute defines the mode of sending messages to the broker and applies only to Topic, for which the parent section is MqttPubTopics.

"OnChange" - a message is sent to the broker if the current value differs from the previous one. (It is mainly used when connecting to external MQTT services, which limit the number of processed messages)
"OnAlways" - sending a message to the broker occurs in each polling cycle.

-	**Retain** attribute - this attribute defines the server behavior when subscribing to this topic. Used for the MqttPubTopics section.

"true" - when subscribing to a topic, the server will immediately send the last value that was current at the time of subscription to the client.
"false" - when subscribing to a topic, the server will send the value only at the time of the next publication of the value for this topic.
-	**NDS** attribute - this attribute defines the decimal separator for floating point numbers. Used for the MqttPubTopics section.

"." - the symbol "." will be used as the decimal place. 
"," - the symbol "," will be used as a decimal place.
-	**NumCmd** attribute -  this attribute must contain the command number from the control channel. Used in the MqttPubCmds and MqttSubCmds sections.
-	**CmdType** attribute - this attribute must contain the type of command. Used in the MqttSubCmds section. Must contain one of the following values:

"St" - a standard control command is formed. The format of the input value: "20.02", "20".
"BinHex" - a binary control command is formed. The input value format must be a string of HEX characters.
"BinTxt" - a binary control command is formed. The input value format should be in the form of a normal UTF-8 string. "Req" - the command of an extraordinary poll of the CP is formed. The input value is ignored. The value of the KpNum attribute is used.
-	**KpNum** attribute - this attribute must contain the number of the device. Used in the MqttSubCmds section.
-	**IDUser** attribute - this attribute must contain the user ID from which the command is sent. Used in the MqttSubCmds section.
-	**NumCnlCtrl** attribute - this attribute must contain the number of the control channel to which the command will be sent. Used in the MqttSubCmds section.
-	**CnlCnt** attribute - this attribute must contain the value of the number of channels in which the values will be written. Used in the MqttSubJSs section.
-	**JSHandlerPath** attribute - this attribute must contain the path to the JS file that will handle the JSON value. Used in the MqttSubJSs section.

An example of the contents of the configuration file for the **KpMqtt** driver is shown below:

```xml
<?xml version="1.0" encoding="utf-8"?>
<DevTemplate>

    <MqttParams Hostname="iot.eclipse.org" ClientID="KpMQTTrs111" Port="1883" UserName="" Password=""/>
    <RapSrvCnf ServerHost="xxx.xxx.xxx.xxx" ServerPort="10000" ServerUser="ScadaComm" ServerPwd="12345"/>
    <MqttSubTopics>
        <Topic TopicName="/rsparam1" QosLevel="0" NumCnl="600"/>
    </MqttSubTopics>
    <MqttPubTopics>
        <Topic TopicName="/rsparam10" QosLevel="0" NumCnl="600" PubBehavior="OnChange" Retain="true" NDS="."/>
    </MqttPubTopics>
    <MqttPubCmds>
        <Topic TopicName="/rsparam100" QosLevel="0" NumCmd="1"/>
        <Topic TopicName="/rsparam1000" QosLevel="0" NumCmd="2"/>
    </MqttPubCmds>
    <MqttSubCmds>
        <Topic TopicName="/rsparam10000" QosLevel="0" NumCmd="10" CmdType="St" KPNum="60" IDUser="0" NumCnlCtrl="500"/>
        <Topic TopicName="/rsparam100000" QosLevel="0" NumCmd="10" CmdType="BinHex" KpNum="60" IDUser="0" NumCnlCtrl="501"/>
    </MqttSubCmds>
    <MqttSubJSs>
        <Topic TopicName="/mesparam11" QosLevel="0" CnlCnt="1" JSHandlerPath="/home/RSSCADA/553/RSComm553Run/Config/job.js"/>
    </MqttSubJSs>
</DevTemplate>
```
An example of a JSON string for processing by a JavaScript script:

```javascript
{"Num":600,"Val":200.3,"Stat":0}
```
An example of the contents of a JavaScript script for processing a message in JSON format:

```javascript
    var obj2 = JSON.parse(InMsg);
    jsvals[0].CnlNum=obj2.Num;
    jsvals[0].Stat=obj2.Stat;
    jsvals[0].Val=obj2.Val;
    mylog("Script return OK");
 ```
When developing a JavaScript script, the following conditions must be considered:
- [The Jint](https://github.com/sebastienros/jint)  library is used to process scripts
- The script has global functions and variables:
Variable **mqttJS** - makes it possible to access information about the topic name (TopicName), the path along which the handler is located (JSHandlerPath), the number of channels available for sending to RapidScada.
InMsg variable - allows to get the JSON string received from the MQTT server.
jsvals variable - makes it possible to access for reading and writing information on the channel number (CnlNum), value (Val) and status (Stat).
mylog function - allows to write string messages to the RapidScada system log.
Next, you need to create a communication line, add a device and attach a configuration file to it. These operations can be performed by editing the ScadaCommSvcConfig.xml file in the Config folder of the ScadaCommunicator application, or using the graphic application of the same name by filling in the appropriate fields.
The following is an example of a section of a ScadaCommunicator configuration file regarding a communications link:

```xml
<!--Линия 17-->
    <CommLine active="true" bind="true" number="17" name="MQTT">
      <CommChannel type="" />
      <LineParams>
        <Param name="ReqTriesCnt" value="3" descr="Количество попыток перезапроса КП при ошибке" />
        <Param name="CycleDelay" value="0" descr="Задержка после цикла опроса, мс" />
        <Param name="CmdEnabled" value="false" descr="Команды ТУ разрешены" />
        <Param name="DetailedLog" value="true" descr="Записывать в журнал подробную информацию" />
      </LineParams>
      <CustomParams />
      <ReqSequence>
        <KP active="true" bind="false" number="42" name="Test MQTT" dll="KpMqtt.dll" address="0" callNum="" timeout="0" delay="60" time="00:00:00" period="00:00:00" cmdLine="KpMQTT_Config.xml" />
      </ReqSequence>
    </CommLine>
```

This example shows the configuration of the communication line with number 17. In your case, it may be different, for example, the next number in order. This number must be brought into line with the table of communication lines on the Scada server.
Description of sections and attributes regarding the operation of the KpMqtt driver:
- **ReqSequence** section - this section is the parent for the device section and is intended for the configuration of all devices on this communication line.
- **Device** section - this section contains settings for a specific type of device:
	-	**bind** attribute (For GUI: Device polling / Selected device / Binding) - this attribute defines the condition of channel binding to a specific device number from the server configuration table. It should be set to false, because in the current driver implementation such a binding mechanism is not used.

	-	**Dll** attribute (For GUI: Device polling / Selected device / DLL) - this attribute defines the name of the file, which is used as a driver that implements the logic of the device. The file name must be set to KpMqtt.dll. The application file must be placed in the device folder of the ScadaCommunicator application.

	-	**Delay** attribute (For GUI: Device polling / Selected device / Delay) - this attribute defines the value of the delay between the polling cycles of the MQTT broker. This value is selected individually depending on the requirements for the data collection mechanism. In this case, a value of 60 is selected.

	-	**cmdLine** attribute (Device polling / Selected device / Command line) - this attribute defines the name of the configuration file for the current device. In this case, the file name is KpMQTT_Config.xml. The file is located in the Config folder of the ScadaCommunicator application.

There are no restrictions on the number of created devices within the same communication line for this driver. But at the same time, other conditions should be taken into account, which can have a significant impact on the entire mechanism of the communication line as a whole. If we take into account external conditions, then you should pay attention to the configuration parameter of the **MQTT** broker timeout. The total polling cycle of all devices on one communication line should not exceed the broker's timeout. When creating more than one **KpMqtt** device on the same communication line, you must specify different **ClientIDs**.

If during operation you find any problems in the operation of this driver, please inform at https://forum.rapidscada.org/
