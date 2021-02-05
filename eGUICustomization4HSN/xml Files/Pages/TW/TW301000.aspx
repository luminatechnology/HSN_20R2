<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TW301000.aspx.cs" Inherits="Page_TW301000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
  <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="eGUICustomization4HSN.Graph.TWNManGUIAPEntry"
        PrimaryView="manualGUIAP_Open">
    <CallbackCommands>
    </CallbackCommands>
  </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
  <px:PXTab Width="100%" runat="server" ID="CstPXTab1">
    <Items>
      <px:PXTabItem Text="Open">
        <Template>
          <px:PXGrid SkinID="DetailsInTab" Width="100%" DataSourceID="ds" SyncPosition="True" AdjustPageSize="Auto" AllowPaging="True" runat="server" ID="CstPXGrid4">
            <Levels>
              <px:PXGridLevel DataMember="manualGUIAP_Open" >
                <Columns>
                  <px:PXGridColumn DataField="Status" Width="70" ></px:PXGridColumn>
                  <px:PXGridColumn CommitChanges="True" DataField="VendorID" Width="140" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="Vatincode" Width="70" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="GUINbr" Width="140" CommitChanges="True" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="Guidate" Width="90" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxZoneID" Width="120" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxCategoryID" Width="120" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxID" Width="140" ></px:PXGridColumn>
                  <px:PXGridColumn CommitChanges="True" DataField="TaxNbr" Width="96" ></px:PXGridColumn>
                  <px:PXGridColumn CommitChanges="True" DataField="OurTaxNbr" Width="96" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="Deduction" Width="70" ></px:PXGridColumn>
                  <px:PXGridColumn CommitChanges="True" DataField="NetAmt" Width="100" ></px:PXGridColumn>
                  <px:PXGridColumn CommitChanges="True" DataField="TaxAmt" Width="100" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="CreatedByID" Width="220" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="Remark" Width="140" ></px:PXGridColumn></Columns></px:PXGridLevel></Levels>
            <AutoSize Container="Window" Enabled="True" ></AutoSize>
            <Mode InitNewRow="True" AllowUpload="True" ></Mode></px:PXGrid></Template></px:PXTabItem>
      <px:PXTabItem Text="Released" >
        <Template>
          <px:PXGrid runat="server" ID="CstPXGrid3" AdjustPageSize="Auto" AllowPaging="True" SkinID="DetailsInTab" Width="100%" DataSourceID="ds" SyncPosition="True">
            <Levels>
              <px:PXGridLevel DataMember="manualGUIAP_Released" >
                <Columns>
                  <px:PXGridColumn DataField="Status" Width="70" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="VendorID" Width="140" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="Vatincode" Width="70" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="Guinbr" Width="140" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="Guidate" Width="90" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxZoneID" Width="120" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxCategoryID" Width="120" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxID" Width="140" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxNbr" Width="96" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="OurTaxNbr" Width="96" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="Deduction" Width="70" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="NetAmt" Width="100" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxAmt" Width="100" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="CreatedByID" Width="220" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="Remark" Width="140" ></px:PXGridColumn></Columns></px:PXGridLevel></Levels>
            <AutoSize Container="Window" Enabled="True" ></AutoSize>
            <Mode AllowUpdate="False" AllowDelete="False" AllowAddNew="False" ></Mode>
            </px:PXGrid></Template></px:PXTabItem></Items></px:PXTab></asp:Content>