<?xml version="1.0" encoding="utf-8"?>
<Faceplate editorVersion="6.0.2.0">
  <Dependencies />
  <Document>
    <Border>
      <Width>1</Width>
      <Color>Silver</Color>
    </Border>
    <CssClass>fp-command-box</CssClass>
    <Script>class extends ComponentScript {
  domCreated(args) {
    console.log("CommandBox, domCreated");
    args.component.dom.on("keydown", "input", (event) =&gt; {
      if (event.key === "Enter") {
        args.component.dom.find("button:first").trigger("click");
      }
    });
  }

  domUpdated(args) {
    console.log("CommandBox, domUpdated");
  }

  dataUpdated(args) {
  }
    
  getCommandValue(args) {
  }
}
</Script>
    <Size>
      <Width>250</Width>
      <Height>80</Height>
    </Size>
    <Stylesheet>.fp-command-box-input {
  width: unset !important;
  right: 80px;
}

.fp-command-box-button {
  left: unset !important;
  right: 10px;
}
</Stylesheet>
  </Document>
  <Components>
    <Text>
      <ID>1</ID>
      <AutoSize>True</AutoSize>
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
      <Conditions isArray="true" />
      <CornerRadius>
        <TopLeft>0</TopLeft>
        <TopRight>0</TopRight>
        <BottomRight>0</BottomRight>
        <BottomLeft>0</BottomLeft>
      </CornerRadius>
      <CssClass />
      <DefaultText />
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
        <X>9</X>
        <Y>5</Y>
      </Location>
      <Name />
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
        <Height>100</Height>
      </Size>
      <Text>Command value:</Text>
      <TextAlign>TopLeft</TextAlign>
      <TextDirection>Horizontal</TextDirection>
      <Tooltip />
      <Visible>True</Visible>
      <WordWrap>False</WordWrap>
    </Text>
    <ExtraMarkup>
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
      <CssClass>fp-command-box-input</CssClass>
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
        <X>9</X>
        <Y>30</Y>
      </Location>
      <Markup>&lt;input type="text" value="0" style="width: 100%; height: 100%"&gt;</Markup>
      <Name />
      <ObjNum>0</ObjNum>
      <OutCnlNum>0</OutCnlNum>
      <PropertyBindings isArray="true" />
      <Script />
      <Size>
        <Width>160</Width>
        <Height>30</Height>
      </Size>
      <Tooltip />
      <UniqueIDs>False</UniqueIDs>
      <Visible>True</Visible>
    </ExtraMarkup>
    <BasicButton>
      <ID>3</ID>
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
        <ActionType>ExecuteScript</ActionType>
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
        <Script>function (args) {
  const props = args.component.parent.properties; // faceplate instance properties
  const inputVal = args.component.dom.closest(".fp-command-box").find("input").val();
  const cmdVal = Number.parseFloat(inputVal);
  
  if (Number.isFinite(cmdVal)) {
    console.log(`Command ${cmdVal} to channel ${props.outCnlNum}`);
    args.renderContext.mainApi.sendCommand(props.outCnlNum, cmdVal, false, null);
  } else {
    alert("A number is required.");
  }
}
</Script>
      </ClickAction>
      <CornerRadius>
        <TopLeft>0</TopLeft>
        <TopRight>0</TopRight>
        <BottomRight>0</BottomRight>
        <BottomLeft>0</BottomLeft>
      </CornerRadius>
      <CssClass>fp-command-box-button</CssClass>
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
      <ImageName />
      <ImageSize>
        <Width>16</Width>
        <Height>16</Height>
      </ImageSize>
      <InCnlNum>0</InCnlNum>
      <Location>
        <X>178</X>
        <Y>30</Y>
      </Location>
      <Name />
      <ObjNum>0</ObjNum>
      <OutCnlNum>0</OutCnlNum>
      <PropertyBindings isArray="true" />
      <Script />
      <Size>
        <Width>60</Width>
        <Height>30</Height>
      </Size>
      <Text>Send</Text>
      <Tooltip />
      <Visible>True</Visible>
    </BasicButton>
  </Components>
  <Images />
</Faceplate>