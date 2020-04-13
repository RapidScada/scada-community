A Simple Example of Integrating Grafana and Rapid SCADA
=============================
Grafana is the open source analytics and monitoring solution for every database.

Sequencing
------------------
 - **Installation Grafana**
 
  Download Grafana from the link https://grafana.com/grafana/download Here you will find detailed installation and configuration instructions.
  
  - **Install the Simple Jason plugin**
  
  Simple JSON Datasource - a generic backend datasource
    
  You can download the plugin at https://grafana.com/grafana/plugins/grafana-simple-json-datasource. A detailed description, as well as installation and configuration instructions, are also available here.
  
  -  **Description of work with GrafanaDataProvider**
  
  1. Configure access to the server SCADA file Web.config in GrafanaDataProvider project.
```xml
<appSettings>
    <add key="serverHost" value="" />
    <add key="serverPort" value="" />
    <add key="serverUser" value="ScadaWeb" />
    <add key="Password value="" /
     <add key="serverTimeout" value="10000" />
</appSettings>
```xml  
  
  2. Password must be encrypted.
  To encrypt the password, you can use the index.html service page GrafanaDataProvider.
  
  3. The manual installation sequence:
      
  4. Configure DataSource SimpleJason in Grafana Server.
Create a data source - select the previously installed SimpleJason plugin.
Set HTPP URL: http: http://localhost/GrafanaDataProvider/api/trends

 5. Build the graph. Choosing Query - SimpleJason. Timeseries: 101 (here enter the channel number on which you want to plot the graph). It is important to enter the channel digital values. Choose the time range for which you want to get data. It is also possible to plot along multiple channels.
 
 6. It is important to set the visualization parameter during settings:
    Stacking & Null value - set Null value = null. This makes it possible to see a graph with a line break, in that time period where no data were received.
  
7. In Grafana get Api Key, when setting up Configuration the  and run the script as:

```script
curl -H "Authorization: Bearer eyJrIjoiVjkyRjd2a2dSQW81ZU51QW5pbDR5WmxESUNDWUY0Z0UiLCJuIjoiTXktV2ViU2l0ZS1Nb25pdG9yaW5nIiwiaWQiOjFash//{path for grafana graph}
```

This is just an example.

7. Get a link to your graph in graphan. Panel Title -> Share->Copy.

8. Add the link to Scada Admin -> Interface
We transfer the project to the server, marking Web Stat.

If during operation you find any problems in the operation of this driver, please inform at https://forum.rapidscada.org/

https://www.youtube.com/watch?v=fC1lKPcui4A&t=15s
