<?xml version="1.0" encoding="utf-8"?>
<Faceplate editorVersion="6.0.2.1">
  <Dependencies />
  <Document>
    <Border>
      <Width>1</Width>
      <Color />
    </Border>
    <PropertyExports isArray="true">
      <Item>
        <Name>imageIndex</Name>
        <Path />
        <DefaultValue>0</DefaultValue>
        <DefaultBinding>
          <PropertyName>imageIndex</PropertyName>
          <DataSource>105</DataSource>
          <DataMember>Data</DataMember>
          <Expression>x.stat &gt; 0 ? x.val : 0</Expression>
          <Format />
        </DefaultBinding>
      </Item>
    </PropertyExports>
    <Script>class extends ComponentScript {
    domCreated(args) {
      console.log("ImageChanger2, domCreated");
    }

    domUpdated(args) {
      console.log("ImageChanger2, domUpdated");
    }

    dataUpdated(args) {
      let picMain = args.component.componentByName.get("picMain");
      let oldImgName = picMain.properties.imageName;
      let newImgName = "img" + args.component.properties.imageIndex;
      
      if (newImgName !== oldImgName) {
        console.log("ImageChanger2, set image to " + newImgName);
        picMain.properties.imageName = newImgName;
      }
    }
    
    getCommandValue(args) {
      console.log("ImageChanger2, getCommandValue");
    }
}
</Script>
    <Size>
      <Width>120</Width>
      <Height>120</Height>
    </Size>
  </Document>
  <Components>
    <Picture>
      <ID>1</ID>
      <BackColor />
      <Blinking>False</Blinking>
      <BlinkingState>
        <BackColor />
        <ForeColor />
        <BorderColor />
        <Underline>False</Underline>
      </BlinkingState>
      <Border>
        <Width>1</Width>
        <Color>Gray</Color>
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
      <DefaultImage />
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
      <ImageName>img</ImageName>
      <ImageStretch>None</ImageStretch>
      <InCnlNum>0</InCnlNum>
      <Location>
        <X>10</X>
        <Y>10</Y>
      </Location>
      <Name>picMain</Name>
      <ObjNum>0</ObjNum>
      <OutCnlNum>0</OutCnlNum>
      <Padding>
        <Top>0</Top>
        <Right>0</Right>
        <Bottom>0</Bottom>
        <Left>0</Left>
      </Padding>
      <PropertyBindings isArray="true" />
      <Rotation>0</Rotation>
      <Script>class extends ComponentScript {
    domCreated(args) {
      console.log("ImageChanger2, picMain, domCreated");
    }

    domUpdated(args) {
      console.log("ImageChanger2, picMain, domUpdated");
    }

    dataUpdated(args) {
      // do nothing
    }
    
    getCommandValue(args) {
      console.log("ImageChanger2, picMain, getCommandValue");
    }
}
</Script>
      <Size>
        <Width>100</Width>
        <Height>100</Height>
      </Size>
      <Tooltip />
      <Visible>True</Visible>
    </Picture>
  </Components>
  <Images>
    <Image>
      <Name>img</Name>
      <MediaType>image/svg+xml</MediaType>
      <Data>PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMDAiIGhlaWdodD0iMTAwIj4NCiAgPHJlY3Qgd2lkdGg9IjEwMCIgaGVpZ2h0PSIxMDAiIGZpbGw9IiNlZWVlZWUiLz4NCiAgPHRleHQgeD0iNTAiIHk9IjUwIiBmb250LXNpemU9IjE2IiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBkb21pbmFudC1iYXNlbGluZT0ibWlkZGxlIj5EZWZhdWx0PC90ZXh0Pg0KPC9zdmc+DQo=</Data>
    </Image>
    <Image>
      <Name>img0</Name>
      <MediaType>image/svg+xml</MediaType>
      <Data>PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMDAiIGhlaWdodD0iMTAwIj4NCiAgPHJlY3Qgd2lkdGg9IjEwMCIgaGVpZ2h0PSIxMDAiIGZpbGw9IiNlZWVlZWUiLz4NCiAgPHRleHQgeD0iNTAiIHk9IjUwIiBmb250LXNpemU9IjE2IiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBkb21pbmFudC1iYXNlbGluZT0ibWlkZGxlIj5JbWFnZSAwPC90ZXh0Pg0KPC9zdmc+DQo=</Data>
    </Image>
    <Image>
      <Name>img1</Name>
      <MediaType>image/svg+xml</MediaType>
      <Data>PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMDAiIGhlaWdodD0iMTAwIj4NCiAgPHJlY3Qgd2lkdGg9IjEwMCIgaGVpZ2h0PSIxMDAiIGZpbGw9IiNlZWVlZWUiLz4NCiAgPHRleHQgeD0iNTAiIHk9IjUwIiBmb250LXNpemU9IjE2IiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBkb21pbmFudC1iYXNlbGluZT0ibWlkZGxlIj5JbWFnZSAxPC90ZXh0Pg0KPC9zdmc+DQo=</Data>
    </Image>
    <Image>
      <Name>img2</Name>
      <MediaType>image/svg+xml</MediaType>
      <Data>PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMDAiIGhlaWdodD0iMTAwIj4NCiAgPHJlY3Qgd2lkdGg9IjEwMCIgaGVpZ2h0PSIxMDAiIGZpbGw9IiNlZWVlZWUiLz4NCiAgPHRleHQgeD0iNTAiIHk9IjUwIiBmb250LXNpemU9IjE2IiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBkb21pbmFudC1iYXNlbGluZT0ibWlkZGxlIj5JbWFnZSAyPC90ZXh0Pg0KPC9zdmc+DQo=</Data>
    </Image>
  </Images>
</Faceplate>