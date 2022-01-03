<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LL101000.aspx.cs" Inherits="Page_LL101000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
  <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"  TypeName="HSNFinance.LUMLLSetupMaint" PrimaryView="SetupRecord">
    <CallbackCommands>
    </CallbackCommands>
  </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
  <px:PXTab DataMember="SetupRecord" ID="tab" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" AllowAutoHide="false">
    <Items>
      <px:PXTabItem LoadOnDemand="True" Text="General Settings">
        <Template>
          <px:PXLayoutRule runat="server" ID="CstPXLayoutRule1" StartGroup="True" GroupCaption="Account Settings" ></px:PXLayoutRule>
                <px:PXSegmentMask runat="server" ID="CstPXSegmentMask2" DataField="InterestExpAcctID" LabelWidth="180px" ></px:PXSegmentMask>
                <px:PXSegmentMask runat="server" ID="CstPXSegmentMask3" DataField="InterestExpSubID" LabelWidth="180px" ></px:PXSegmentMask>
                <px:PXSegmentMask runat="server" ID="CstPXSegmentMask4" DataField="LeaseLiabAcctID" LabelWidth="180px" ></px:PXSegmentMask>
                <px:PXSegmentMask runat="server" ID="CstPXSegmentMask5" DataField="LeaseLiabSubID" LabelWidth="180px"></px:PXSegmentMask></Template></px:PXTabItem>
      <px:PXTabItem Visible="False" Text="Tab item 2">
      </px:PXTabItem>
    </Items>
    <AutoSize Container="Window" Enabled="True" MinHeight="200" ></AutoSize>
  </px:PXTab>
</asp:Content>