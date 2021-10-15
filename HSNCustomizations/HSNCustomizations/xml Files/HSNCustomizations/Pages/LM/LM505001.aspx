<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM505001.aspx.cs" Inherits="Page_LM505001" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="HSNCustomizations.Graph.ClossPrepaymentProcess" PrimaryView="PrepaymentList">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="PrimaryInquire" AllowAutoHide="false">
        <Levels>
            <px:PXGridLevel DataMember="PrepaymentList">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" Width="40" Type="CheckBox" TextAlign="Center"></px:PXGridColumn>
                    <px:PXGridColumn DataField="Doctype" Width="70"></px:PXGridColumn>
                    <px:PXGridColumn DataField="RefNbr" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="Status" Width="70"></px:PXGridColumn>
                    <px:PXGridColumn DataField="CustomerID" Width="130"></px:PXGridColumn>
                    <px:PXGridColumn DataField="customerID_Customer_acctName" Width="130"></px:PXGridColumn>
                    <px:PXGridColumn DataField="AdjDate" Width="140"></px:PXGridColumn>
                    <px:PXGridColumn DataField="PaymentMethodID" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="CashAccountID" Width="100"></px:PXGridColumn>
                    <px:PXGridColumn DataField="CuryID" Width="90"></px:PXGridColumn>
                    <px:PXGridColumn DataField="CuryUnappliedBal" Width="220" ></px:PXGridColumn>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150"></AutoSize>
        <ActionBar PagerVisible="Bottom">
            <PagerSettings Mode="NumericCompact" />
        </ActionBar>
    </px:PXGrid>
</asp:Content>
