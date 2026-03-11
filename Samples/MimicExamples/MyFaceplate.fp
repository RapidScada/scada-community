<?xml version="1.0" encoding="utf-8"?>
<Faceplate editorVersion="6.0.1.0">
  <Dependencies />
  <Document>
    <BackColor>White</BackColor>
    <BackgroundImage />
    <BackgroundPadding>
      <Top>0</Top>
      <Right>10</Right>
      <Bottom>0</Bottom>
      <Left>10</Left>
    </BackgroundPadding>
    <BlinkingState>
      <BackColor>Orange</BackColor>
      <ForeColor />
      <BorderColor />
      <Underline>False</Underline>
    </BlinkingState>
    <Border>
      <Width>5</Width>
      <Color>Gray</Color>
    </Border>
    <CornerRadius>
      <TopLeft>10</TopLeft>
      <TopRight>10</TopRight>
      <BottomRight>10</BottomRight>
      <BottomLeft>10</BottomLeft>
    </CornerRadius>
    <DisabledState>
      <BackColor>Silver</BackColor>
      <ForeColor />
      <BorderColor />
      <Underline>False</Underline>
    </DisabledState>
    <HoverState>
      <BackColor>Azure</BackColor>
      <ForeColor />
      <BorderColor />
      <Underline>False</Underline>
    </HoverState>
    <PropertyExports isArray="true">
      <Item>
        <Name>faceValue</Name>
        <Path>txtValue.text</Path>
        <DefaultValue />
      </Item>
      <Item>
        <Name>faceColor</Name>
        <Path>txtValue.foreColor</Path>
        <DefaultValue />
      </Item>
    </PropertyExports>
    <Script>class extends ComponentScript {
    domCreated(args) {
      console.log("MyFaceplate, domCreated");
    }

    domUpdated(args) {
      console.log("MyFaceplate, domUpdated");
    }

    dataUpdated(args) {
      //console.log("MyFaceplate, dataUpdated");
    }
    
    getCommandValue(args) {
      console.log("MyFaceplate, getCommandValue");
    }
}
</Script>
    <Size>
      <Width>200</Width>
      <Height>100</Height>
    </Size>
    <Stylesheet>.my-faceplate {
  color: red;
}
</Stylesheet>
  </Document>
  <Components>
    <Text>
      <ID>1</ID>
      <DisabledState>
        <BackColor />
        <ForeColor>Gray</ForeColor>
        <BorderColor />
        <Underline>False</Underline>
      </DisabledState>
      <Enabled>True</Enabled>
      <Location>
        <X>10</X>
        <Y>10</Y>
      </Location>
      <Name>txtTitle</Name>
      <Size>
        <Width>170</Width>
        <Height>30</Height>
      </Size>
      <Text>I'm a faceplate</Text>
      <TextAlign>MiddleLeft</TextAlign>
      <Visible>True</Visible>
    </Text>
    <Text>
      <ID>3</ID>
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
        <X>10</X>
        <Y>50</Y>
      </Location>
      <Name>txtLabel</Name>
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
        <Width>50</Width>
        <Height>30</Height>
      </Size>
      <Text>Value:</Text>
      <TextAlign>MiddleLeft</TextAlign>
      <TextDirection>Horizontal</TextDirection>
      <Tooltip />
      <Visible>True</Visible>
      <WordWrap>False</WordWrap>
    </Text>
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
          <Target>Self</Target>
          <ViewID>0</ViewID>
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
        <X>70</X>
        <Y>50</Y>
      </Location>
      <Name>txtValue</Name>
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
        <Width>80</Width>
        <Height>30</Height>
      </Size>
      <Text>0</Text>
      <TextAlign>MiddleLeft</TextAlign>
      <TextDirection>Horizontal</TextDirection>
      <Tooltip />
      <Visible>True</Visible>
      <WordWrap>False</WordWrap>
    </Text>
  </Components>
  <Images />
</Faceplate>