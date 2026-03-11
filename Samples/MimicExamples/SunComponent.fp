<?xml version="1.0" encoding="utf-8"?>
<Faceplate editorVersion="6.0.0.0">
  <Dependencies />
  <Document>
    <CssClass>sun-component</CssClass>
    <PropertyExports isArray="true">
      <Item>
        <Name>shining</Name>
        <Path />
        <DefaultValue>0</DefaultValue>
      </Item>
    </PropertyExports>
    <Script>class extends ComponentScript {
    _getShining(args) {
      return Number.parseInt(args.component.properties.shining) &gt; 0;
    }
    
    _showShining(args) {
      args.component.dom.find("svg path")
        .attr("fill", this._getShining(args) ? "cyan" : "lightcyan");
    }

    domCreated(args) {
      console.log("SunComponent, domCreated");
      this._showShining(args);
    }

    domUpdated(args) {
      console.log("SunComponent, domUpdated");
      this._showShining(args);
    }

    dataUpdated(args) {
    }
    
    getCommandValue(args) {
      console.log("SunComponent, getCommandValue");
      return this._getShining(args) ? 0 : 1;
    }
}
</Script>
    <Size>
      <Width>100</Width>
      <Height>120</Height>
    </Size>
    <Stylesheet />
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
          <ShowDialog>False</ShowDialog>
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
      <Markup>&lt;svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"&gt;
	&lt;path fill="silver" d="M232 488c0 13.3 10.7 24 24 24s24-10.7 24-24l0-56c0-13.3-10.7-24-24-24s-24 10.7-24 24l0 56zm0-408c0 13.3 10.7 24 24 24s24-10.7 24-24l0-56c0-13.3-10.7-24-24-24s-24 10.7-24 24l0 56zM75 75c-9.4 9.4-9.4 24.6 0 33.9l39.6 39.6c9.4 9.4 24.6 9.4 33.9 0s9.4-24.6 0-33.9L108.9 75c-9.4-9.4-24.6-9.4-33.9 0zM363.5 363.5c-9.4 9.4-9.4 24.6 0 33.9L403.1 437c9.4 9.4 24.6 9.4 33.9 0s9.4-24.6 0-33.9l-39.6-39.6c-9.4-9.4-24.6-9.4-33.9 0zM0 256c0 13.3 10.7 24 24 24l56 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-56 0c-13.3 0-24 10.7-24 24zm408 0c0 13.3 10.7 24 24 24l56 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-56 0c-13.3 0-24 10.7-24 24zM75 437c9.4 9.4 24.6 9.4 33.9 0l39.6-39.6c9.4-9.4 9.4-24.6 0-33.9s-24.6-9.4-33.9 0L75 403.1c-9.4 9.4-9.4 24.6 0 33.9zM363.5 148.5c9.4 9.4 24.6 9.4 33.9 0L437 108.9c9.4-9.4 9.4-24.6 0-33.9s-24.6-9.4-33.9 0l-39.6 39.6c-9.4 9.4-9.4 24.6 0 33.9zM256 368a112 112 0 1 0 0-224 112 112 0 1 0 0 224z"/&gt;
&lt;/svg&gt;</Markup>
      <Name>svgSun</Name>
      <ObjNum>0</ObjNum>
      <OutCnlNum>0</OutCnlNum>
      <PropertyBindings isArray="true" />
      <Script />
      <Size>
        <Width>100</Width>
        <Height>100</Height>
      </Size>
      <Tooltip />
      <Visible>True</Visible>
    </ExtraMarkup>
    <Text>
      <ID>2</ID>
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
        <Y>100</Y>
      </Location>
      <Name>txtDescr</Name>
      <ObjNum>0</ObjNum>
      <OutCnlNum>0</OutCnlNum>
      <Padding>
        <Top>0</Top>
        <Right>0</Right>
        <Bottom>0</Bottom>
        <Left>0</Left>
      </Padding>
      <PropertyBindings isArray="true" />
      <Script />
      <Size>
        <Width>100</Width>
        <Height>20</Height>
      </Size>
      <Text>Faceplate</Text>
      <TextAlign>MiddleCenter</TextAlign>
      <TextDirection>Horizontal</TextDirection>
      <Tooltip />
      <Visible>True</Visible>
      <WordWrap>False</WordWrap>
    </Text>
  </Components>
  <Images />
</Faceplate>