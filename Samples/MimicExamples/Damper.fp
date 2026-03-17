<?xml version="1.0" encoding="utf-8"?>
<Faceplate editorVersion="6.0.1.0">
  <Dependencies />
  <Document>
    <PropertyExports isArray="true">
      <Item>
        <Name>rotation</Name>
        <Path />
        <DefaultValue>0</DefaultValue>
      </Item>
    </PropertyExports>
    <Script>class extends ComponentScript {
    _showRotation(args) {
      const idPrefix = args.component.dom.attr("id") + "-1-";
      const rotation = Number.parseInt(args.component.properties.rotation) || 0;
      const enabled = args.component.properties.enabled;
      const angle = enabled ? rotation : 0;
      const fillColor = enabled ? "#4a90e2" : "#9e9e9e";
      const strokeColor = enabled ? "#1c3f73" : "#444";
      args.component.dom
        .find(`#${idPrefix}damper`).attr("transform", `rotate(${angle} 110 70)`)
        .find("rect").attr("fill", fillColor).attr("stroke", strokeColor);
    }

    domCreated(args) {
      console.log("Damper, Faceplate, domCreated");
      this._showRotation(args);
    }

    domUpdated(args) {
      console.log("Damper, Faceplate, domUpdated");
      this._showRotation(args);
    }
}
</Script>
    <Size>
      <Width>220</Width>
      <Height>140</Height>
    </Size>
  </Document>
  <Components>
    <ExtraMarkup>
      <ID>1</ID>
      <AutoSize>False</AutoSize>
      <BackColor />
      <Blinking>False</Blinking>
      <BlinkingState>
        <BackColor />
        <ForeColor />
        <BorderColor />
        <Underline>False</Underline>
      </BlinkingState>
      <Border>
        <Width>0</Width>
        <Color />
      </Border>
      <CheckRights>False</CheckRights>
      <ClickAction>
        <ActionType>None</ActionType>
        <ChartArgs />
        <CommandArgs>
          <ShowDialog>True</ShowDialog>
          <CmdVal>0</CmdVal>
        </CommandArgs>
        <LinkArgs>
          <Url />
          <UrlParams>
            <Enabled>False</Enabled>
            <Param0 />
            <Param1 />
            <Param2 />
            <Param3 />
            <Param4 />
            <Param5 />
            <Param6 />
            <Param7 />
            <Param8 />
            <Param9 />
          </UrlParams>
          <ViewID>0</ViewID>
          <Target>Self</Target>
          <ModalWidth>Normal</ModalWidth>
          <ModalHeight>0</ModalHeight>
        </LinkArgs>
        <Script />
      </ClickAction>
      <CornerRadius>
        <TopLeft>0</TopLeft>
        <TopRight>0</TopRight>
        <BottomRight>0</BottomRight>
        <BottomLeft>0</BottomLeft>
      </CornerRadius>
      <CssClass />
      <DeviceNum>0</DeviceNum>
      <DisabledState>
        <BackColor />
        <ForeColor />
        <BorderColor />
        <Underline>False</Underline>
      </DisabledState>
      <Enabled>True</Enabled>
      <Font>
        <Inherit>True</Inherit>
        <Name />
        <Size>16</Size>
        <Bold>False</Bold>
        <Italic>False</Italic>
        <Underline>False</Underline>
      </Font>
      <ForeColor />
      <HoverState>
        <BackColor />
        <ForeColor />
        <BorderColor />
        <Underline>False</Underline>
      </HoverState>
      <InCnlNum>0</InCnlNum>
      <Location>
        <X>0</X>
        <Y>0</Y>
      </Location>
      <Markup>&lt;svg width="220" height="140"
     viewBox="0 0 220 140"
     xmlns="http://www.w3.org/2000/svg"&gt;

  &lt;!-- воздуховод --&gt;
  &lt;rect x="20" y="50" width="180" height="40"
        fill="#e0e0e0" stroke="#333" stroke-width="2"/&gt;

  &lt;!-- ось --&gt;
  &lt;circle cx="110" cy="70" r="4" fill="#333"/&gt;

  &lt;!-- заслонка --&gt;
  &lt;g id="{0}damper" transform="rotate(0 110 70)"&gt;
      &lt;rect x="70" y="66"
            width="80"
            height="8"
            fill="#9e9e9e" 
            stroke="#444"
            stroke-width="2"/&gt;
  &lt;/g&gt;

&lt;/svg&gt;</Markup>
      <Name>svgDamper</Name>
      <ObjNum>0</ObjNum>
      <OutCnlNum>0</OutCnlNum>
      <PropertyBindings isArray="true" />
      <Script>class extends ComponentScript {
    domCreated(args) {
      console.log("Damper, Markup, domCreated");
    }

    domUpdated(args) {
      console.log("Damper, Markup, domUpdated");
    }
}
</Script>
      <Size>
        <Width>220</Width>
        <Height>140</Height>
      </Size>
      <Tooltip />
      <UniqueIDs>True</UniqueIDs>
      <Visible>True</Visible>
    </ExtraMarkup>
  </Components>
  <Images />
</Faceplate>