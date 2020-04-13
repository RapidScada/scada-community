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
    <add key="serverHost"    value="" />
    <add key="serverPort"    value="" />
    <add key="serverUser"    value="ScadaWeb" />
    <add key="Password"      value="" />
    <add key="serverTimeout" value="10000" />
</appSettings>
``` 
  
  2. Password must be encrypted.
  To encrypt the password, you can use the index.html service page GrafanaDataProvider.
  
  **3. The manual installation sequence:**
   -  Install Microsoft Internet Information Services (IIS) by selecting the appropriate Windows features.
   -  Install Microsoft .NET Framework (install IIS before the framework) of the version specified in System Requirements.
   -  Unzip files from the GrafanaDataProvider folder of the installation pachage to the SCADA installation directory. Recommended destination is C:\SCADA
   -  Open IIS management console: Control Panel > System and Security > Administrative Tools > Internet Information Services (IIS) Manager.
   -  Add a web application to the site tree. Right click the appropriate site, Default Web Site in most cases, then choose the Add Application menu item.
   -  Enter the application alias: Scada. Check that the selected application pool uses .NET 4.0 runtime version and integrated pipeline mode. Specify the physical path to the web application files: C:\SCADA\GrafanaDataProvider. Then click OK.
      
  4. Configure DataSource SimpleJason in Grafana Server.
Create a data source - select the previously installed SimpleJason plugin.
Set HTPP URL: http: http://localhost/GrafanaDataProvider/api/trends

 5. Build the graph. Select a database source (Query) SimpleJason. Timeseries: 101 (here enter the channel number on which you want to plot the graph). It is important to enter the channel digital values. Choose the time range for which you want to get data. It is also possible to plot along multiple channels.
 
 6. It is important to set the visualization parameter during settings:
    Stacking & Null value - set Null value = null. This makes it possible to see a graph with a line break, in that time period where no data were received.
  
7. In Grafana get Api Key, when setting up Configuration the  and run the script as:

```script
curl -H "Authorization: Bearer eyJrIjoiVjkyRjd2a2dSQW81ZU51QW5pbDR5WmxESUNDWUY0Z0UiLCJuIjoiTXktV2ViU2l0ZS1Nb25pdG9yaW5nIiwiaWQiOjFash//{path for grafana graph}
```

This is just an example.

7. Get a link to your graph in grafana. Panel Title -> Share->Copy.

8. In the application Administrator Application, add a link to the graph.

If during operation you find any problems in the operation of this service, please inform at https://forum.rapidscada.org/

https://www.youtube.com/watch?v=fC1lKPcui4A&t=15s
