<?xml version="1.0" encoding="utf-8"?>
<Faceplate editorVersion="6.0.2.1">
  <Dependencies>
    <Faceplate typeName="Damper" path="Mimics\Damper.fp" />
  </Dependencies>
  <Document>
    <BackColor />
    <BackgroundImage />
    <BackgroundPadding>
      <Top>0</Top>
      <Right>0</Right>
      <Bottom>0</Bottom>
      <Left>0</Left>
    </BackgroundPadding>
    <BlinkingState>
      <BackColor />
      <ForeColor />
      <BorderColor />
      <Underline>False</Underline>
    </BlinkingState>
    <Border>
      <Width>1</Width>
      <Color />
    </Border>
    <CornerRadius>
      <TopLeft>10</TopLeft>
      <TopRight>10</TopRight>
      <BottomRight>10</BottomRight>
      <BottomLeft>10</BottomLeft>
    </CornerRadius>
    <CssClass />
    <DisabledState>
      <BackColor />
      <ForeColor />
      <BorderColor />
      <Underline>False</Underline>
    </DisabledState>
    <Font>
      <Inherit>False</Inherit>
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
    <PropertyExports isArray="true">
      <Item>
        <Name>enabled1</Name>
        <Path>damper1.enabled</Path>
        <DefaultValue />
        <DefaultBinding>
          <PropertyName />
          <DataSource />
          <DataMember>Value</DataMember>
          <Expression />
          <Format />
        </DefaultBinding>
      </Item>
      <Item>
        <Name>rotation1</Name>
        <Path>damper1.rotation</Path>
        <DefaultValue />
        <DefaultBinding>
          <PropertyName>rotation1</PropertyName>
          <DataSource />
          <DataMember>Value</DataMember>
          <Expression />
          <Format />
        </DefaultBinding>
      </Item>
      <Item>
        <Name>enabled2</Name>
        <Path>damper2.enabled</Path>
        <DefaultValue />
        <DefaultBinding>
          <PropertyName />
          <DataSource />
          <DataMember>Value</DataMember>
          <Expression />
          <Format />
        </DefaultBinding>
      </Item>
      <Item>
        <Name>rotation2</Name>
        <Path>damper2.rotation</Path>
        <DefaultValue />
        <DefaultBinding>
          <PropertyName>rotation2</PropertyName>
          <DataSource />
          <DataMember>Value</DataMember>
          <Expression />
          <Format />
        </DefaultBinding>
      </Item>
    </PropertyExports>
    <Script />
    <Size>
      <Width>500</Width>
      <Height>180</Height>
    </Size>
    <Stylesheet />
    <Tooltip />
  </Document>
  <Components>
    <Damper>
      <ID>1</ID>
      <Blinking>False</Blinking>
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
      <DeviceNum>0</DeviceNum>
      <Enabled>True</Enabled>
      <InCnlNum>0</InCnlNum>
      <Location>
        <X>20</X>
        <Y>20</Y>
      </Location>
      <Name>damper1</Name>
      <ObjNum>0</ObjNum>
      <OutCnlNum>0</OutCnlNum>
      <PropertyBindings isArray="true" />
      <Rotation>0</Rotation>
      <Size>
        <Width>220</Width>
        <Height>140</Height>
      </Size>
      <Visible>True</Visible>
    </Damper>
    <Damper>
      <ID>2</ID>
      <Blinking>False</Blinking>
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
      <DeviceNum>0</DeviceNum>
      <Enabled>True</Enabled>
      <InCnlNum>0</InCnlNum>
      <Location>
        <X>260</X>
        <Y>20</Y>
      </Location>
      <Name>damper2</Name>
      <ObjNum>0</ObjNum>
      <OutCnlNum>0</OutCnlNum>
      <PropertyBindings isArray="true" />
      <Rotation>0</Rotation>
      <Size>
        <Width>220</Width>
        <Height>140</Height>
      </Size>
      <Visible>True</Visible>
    </Damper>
  </Components>
  <Images />
</Faceplate>