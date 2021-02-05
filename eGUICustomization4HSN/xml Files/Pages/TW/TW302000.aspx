<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TW302000.aspx.cs" Inherits="Page_TW302000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
  <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="eGUICustomization4HSN.Graph.TWNManGUIAREntry"
        PrimaryView="manualGUIAR_Open">
    <CallbackCommands>
    </CallbackCommands>
  </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
  <px:PXTab Width="100%" runat="server" ID="CstPXTab1">
    <Items>
      <px:PXTabItem Text="Open" >
        <Template>
          <px:PXGrid AdjustPageSize="Auto" AllowPaging="True" Width="100%" SkinID="DetailsInTab" DataSourceID="ds" SyncPosition="True" runat="server" ID="CstPXGrid2">
            <Levels>
              <px:PXGridLevel DataMember="manualGUIAR_Open" >
                <Columns>
                  <px:PXGridColumn DataField="Status" Width="70" ></px:PXGridColumn>
                  <px:PXGridColumn CommitChanges="True" DataField="CustomerID" Width="140" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="VatOutCode" Width="70" ></px:PXGridColumn>
                  <px:PXGridColumn CommitChanges="" DataField="GUINbr" Width="140" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="GUIDate" Width="90" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxZoneID" Width="120" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxCategoryID" Width="120" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxID" Width="140" ></px:PXGridColumn>
                  <px:PXGridColumn CommitChanges="True" DataField="TaxNbr" Width="96" ></px:PXGridColumn>
                  <px:PXGridColumn CommitChanges="True" DataField="OurTaxNbr" Width="96" ></px:PXGridColumn>
                  <px:PXGridColumn CommitChanges="True" DataField="NetAmt" Width="100" ></px:PXGridColumn>
                  <px:PXGridColumn CommitChanges="True" DataField="TaxAmt" Width="100" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="CustomType" Width="140" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="ExportMethod" Width="70" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="ExportTicketType" Width="70" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="ExportTicketNbr" Width="140" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="ClearingDate" Width="90" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="CreatedByID" Width="220" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="Remark" Width="140" ></px:PXGridColumn></Columns></px:PXGridLevel></Levels>
            <AutoSize Container="Window" ></AutoSize>
            <AutoSize Enabled="True" ></AutoSize>
            <Mode AllowUpload="True" InitNewRow="True" ></Mode></px:PXGrid></Template></px:PXTabItem>
      <px:PXTabItem Text="Released" >
        <Template>
          <px:PXGrid AdjustPageSize="Auto" AllowPaging="True" SkinID="Inquire" Width="100%" DataSourceID="ds" SyncPosition="True" runat="server" ID="CstPXGrid3">
            <Levels>
              <px:PXGridLevel DataMember="manualGUIAR_Released" >
                <Columns>
                  <px:PXGridColumn DataField="Status" Width="70" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="CustomerID" Width="140" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="VatOutCode" Width="70" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="GUINbr" Width="140" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="GUIDate" Width="90" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxZoneID" Width="120" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxCategoryID" Width="120" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxID" Width="140" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxNbr" Width="96" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="OurTaxNbr" Width="96" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="NetAmt" Width="100" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="TaxAmt" Width="100" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="CustomType" Width="140" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="ExportMethod" Width="70" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="ExportTicketType" Width="70" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="ExportTicketNbr" Width="140" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="ClearingDate" Width="90" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="CreatedByID" Width="220" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="Remark" Width="140" ></px:PXGridColumn></Columns></px:PXGridLevel></Levels>
            <AutoSize Container="Window" ></AutoSize>
            <AutoSize Enabled="True" ></AutoSize>
            <Mode AllowDelete="False" ></Mode>
            <Mode AllowAddNew="False" ></Mode>
            <Mode AllowUpdate="False" ></Mode></px:PXGrid></Template></px:PXTabItem></Items></px:PXTab></asp:Content>