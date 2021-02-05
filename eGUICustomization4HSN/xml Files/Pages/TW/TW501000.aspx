<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TW501000.aspx.cs" Inherits="Page_TW501000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
  <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="eGUICustomization4HSN.Graph.TWNGenGUIMediaFile"
        PrimaryView="Filter">
    <CallbackCommands>
    </CallbackCommands>
  </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
  <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" Height="" AllowAutoHide="false">
    <Template>
      <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True"></px:PXLayoutRule>
      <px:PXLayoutRule runat="server" ID="CstPXLayoutRule1" StartColumn="True" ></px:PXLayoutRule>
      <px:PXDateTimeEdit CommitChanges="True" runat="server" ID="CstPXDateTimeEdit2" DataField="FromDate" ></px:PXDateTimeEdit>
      <px:PXLayoutRule runat="server" ID="CstPXLayoutRule5" StartColumn="True" ></px:PXLayoutRule>
      <px:PXDateTimeEdit CommitChanges="True" runat="server" ID="CstPXDateTimeEdit4" DataField="ToDate" ></px:PXDateTimeEdit></Template>
  </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
  <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Details" AllowAutoHide="false">
    <Levels>
      <px:PXGridLevel DataMember="GUITransList">
          <Columns>
        <px:PXGridColumn AllowCheckAll="True" Type="CheckBox" DataField="Selected" Width="60" ></px:PXGridColumn>
        <px:PXGridColumn DataField="GUIStatus" Width="120" ></px:PXGridColumn>
        <px:PXGridColumn DataField="GUIDirection" Width="140" ></px:PXGridColumn>
        <px:PXGridColumn DataField="GUIFormatcode" Width="70" ></px:PXGridColumn>
        <px:PXGridColumn DataField="GUIControlCode" Width="140" ></px:PXGridColumn>
        <px:PXGridColumn DataField="GUINbr" Width="140" ></px:PXGridColumn>
        <px:PXGridColumn DataField="GUIDate" Width="90" ></px:PXGridColumn>
        <px:PXGridColumn DataField="GUIDecPeriod" Width="90" ></px:PXGridColumn>
        <px:PXGridColumn DataField="TaxZoneID" Width="120" ></px:PXGridColumn>
        <px:PXGridColumn DataField="TaxCategoryID" Width="120" ></px:PXGridColumn>
        <px:PXGridColumn DataField="TaxID" Width="140" ></px:PXGridColumn>
        <px:PXGridColumn DataField="VATType" Width="70" ></px:PXGridColumn>
        <px:PXGridColumn DataField="TaxNbr" Width="96" ></px:PXGridColumn>
        <px:PXGridColumn DataField="OurTaxNbr" Width="96" ></px:PXGridColumn>
        <px:PXGridColumn DataField="NetAmount" Width="100" ></px:PXGridColumn>
        <px:PXGridColumn DataField="TaxAmount" Width="100" ></px:PXGridColumn>
        <px:PXGridColumn DataField="NetAmtRemain" Width="100" ></px:PXGridColumn>
        <px:PXGridColumn DataField="TaxAmtRemain" Width="100" ></px:PXGridColumn>
        <px:PXGridColumn DataField="SequenceNo" Width="70" ></px:PXGridColumn>
        <px:PXGridColumn DataField="CustVend" Width="140" ></px:PXGridColumn>
        <px:PXGridColumn DataField="Branch" Width="120" ></px:PXGridColumn>
        <px:PXGridColumn DataField="DeductionCode" Width="70" ></px:PXGridColumn>
        <px:PXGridColumn DataField="BatchNbr" Width="140" ></px:PXGridColumn>
        <px:PXGridColumn DataField="OrderNbr" Width="140" ></px:PXGridColumn>
        <px:PXGridColumn DataField="TransDate" Width="90" ></px:PXGridColumn>
        <px:PXGridColumn DataField="PrintedDate" Width="90" ></px:PXGridColumn>
        <px:PXGridColumn DataField="ExportMethods" Width="70" ></px:PXGridColumn>
        <px:PXGridColumn DataField="ExportTicketType" Width="70" ></px:PXGridColumn>
        <px:PXGridColumn DataField="CustomType" Width="140" ></px:PXGridColumn>
        <px:PXGridColumn DataField="ClearingDate" Width="90" ></px:PXGridColumn>
        <px:PXGridColumn DataField="Remark" Width="140" ></px:PXGridColumn>
        <px:PXGridColumn DataField="GUITitle" Width="220" ></px:PXGridColumn>
        <px:PXGridColumn DataField="EGUIExcluded" Width="60" ></px:PXGridColumn>
        <px:PXGridColumn DataField="EGUIExported" Width="60" ></px:PXGridColumn>
        <px:PXGridColumn DataField="EGUIExportedDateTime" Width="90" ></px:PXGridColumn>
        <px:PXGridColumn DataField="CreatedDateTime" Width="90" ></px:PXGridColumn></Columns>
      </px:PXGridLevel>
    </Levels>
    <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    <ActionBar >
    </ActionBar>
  </px:PXGrid>
</asp:Content>