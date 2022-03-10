<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM303000.aspx.cs" Inherits="Page_LM303000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" UDFTypeField="CustomerClassID" EnableAttributes="true" Width="100%" TypeName="HSNHighcareCistomizations.Graph.CustomerPINCodeMaint" PrimaryView="Document">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="viewDefSchedule" Visible="false" DependOnGrid="grid"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="viewSalesOrder" Visible="false" DependOnGrid="grid"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="viewInvoice" Visible="false" DependOnGrid="grid"></px:PXDSCallbackCommand>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="BAccount" runat="server" Width="100%" Caption="Customer Summary" DataMember="Document" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="false" ActivityField="NoteActivity" LinkIndicator="true" BPEventsIndicator="true" DefaultControlID="edAcctCD">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
            <px:PXSelector ID="edAcctCD" runat="server" DataField="AcctCD" FilterByAllFields="True" CommitChanges="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Style="z-index: 100;" Width="100%" Caption="Customer PIN Code" SkinID="Details" Height="300px">
        <Levels>
            <px:PXGridLevel DataMember="Transaction">
                <RowTemplate>
                    <px:PXSelector ID="edCPriceClassID" runat="server" DataField="CPriceClassID"></px:PXSelector>
                    <px:PXSelector ID="edScheduleNbr" runat="server" DataField="ScheduleNbr"></px:PXSelector>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="IsActive" Type="CheckBox" />
                    <px:PXGridColumn DataField="Pin" AllowNull="False" />
                    <px:PXGridColumn DataField="SerialNbr" AllowNull="False" />
                    <px:PXGridColumn DataField="CPriceClassID" AllowNull="False" />
                    <px:PXGridColumn DataField="ScheduleNbr" LinkCommand="viewDefSchedule" />
                    <px:PXGridColumn DataField="SOOrderNbr" LinkCommand="viewSalesOrder" />
                    <px:PXGridColumn DataField="InvoiceNbr" LinkCommand="viewInvoice" />
                    <px:PXGridColumn DataField="StartDate" Width="200px" />
                    <px:PXGridColumn DataField="EndDate" Width="200px" />
                    <px:PXGridColumn DataField="CreatedByID" Width="200px" />
                    <px:PXGridColumn DataField="CreatedDateTime" Width="200px" />
                    <px:PXGridColumn DataField="LastModifiedByID" Width="200px" />
                    <px:PXGridColumn DataField="LastModifiedDateTime" Width="200px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <CallbackCommands>
            <Save PostData="Page" />
        </CallbackCommands>
    </px:PXGrid>
</asp:Content>
