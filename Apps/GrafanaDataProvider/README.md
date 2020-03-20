A simple example of integrating Grafana and Rapid SCADA
=============================
Grafana is the open source analytics and monitoring solution for every database.

Sequencing
------------------
 - **Installation Grafana**
 
  Download Grafana from the link https://grafana.com/grafana/download Here you will find detailed installation and configuration instructions.
  
  - **Install the Simple Jason plugin**
  
  Simple JSON Datasource - a generic backend datasource
  
  Download plagin from the link https://grafana.com/grafana/plugins/grafana-simple-json-datasource A detailed description, as well as installation and configuration instructions, are also available here.
  
  -  **Description of work with GrafanaDataProvider**
  
  1. Configure access to the server SCADA file Web.config
```xml
<appSettings>
    <add key="serverHost" value="" />
    <add key="serverPort" value="" />
    <add key="serverUser" value="ScadaWeb" />
    <add key="Password value="" /
     <add key="serverTimeout" value="10000" />
</appSettings>
```
  2. Password must be encrypted.
  Page index.html
  
  3. Configure DataSource SimpleJason in Grafana Server
Create a data source - select the previously installed SimpleJason plugin
HTPP URL: http: // localhost:Port(example)/api/trends

 4. Build the graph. Choosing Query - SimpleJason. Timeseries: 101 (here enter the channel number on which you want to plot the graph). It is important to enter the channel digital values. Choose the time range for which you want to get data. It is also possible to plot along multiple channels.
 
 5. It is important to set the visualization parameter during settings:
    Stacking & Null value - set Null value = null as zero. This makes it possible to see a graph with a line break, in that time period where no data were received.
  
