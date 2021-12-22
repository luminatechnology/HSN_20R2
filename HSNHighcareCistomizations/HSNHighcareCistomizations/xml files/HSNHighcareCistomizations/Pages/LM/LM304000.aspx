<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM304000.aspx.cs" Inherits="Page_LM304000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" EnableAttributes="true" Width="100%" TypeName="HSNHighcareCistomizations.Graph.ServiceScopeMaint" PrimaryView="Document">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="formFilter" runat="server" Width="100%" Caption="Service Scope" DataMember="Document" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="false" ActivityField="NoteActivity" LinkIndicator="true" BPEventsIndicator="true" DefaultControlID="CPriceClassID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
            <px:PXSelector ID="edScopeList" runat="server" DataField="CPriceClassID" FilterByAllFields="True" CommitChanges="True" />
            <px:PXSelector ID="edInventoryID" runat="server" DataField="InventoryID" FilterByAllFields="True" />
            <px:PXSelector ID="edDefCode" runat="server" DataField="DefCode" FilterByAllFields="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Style="z-index: 100;" Width="100%" Caption="Service Scope by Price Class" SkinID="Details" Height="300px">
        <Levels>
            <px:PXGridLevel DataMember="ScopeList">
                <Columns>
                    <px:PXGridColumn DataField="CPriceClassID" />
                    <px:PXGridColumn DataField="PriceClassID" Width="200px" />
                    <px:PXGridColumn DataField="InventoryID" Width="200px" />
                    <px:PXGridColumn DataField="DiscountPrecent" Width="200px" />
                    <px:PXGridColumn DataField="LimitedCount" Width="200px" />
                    <px:PXGridColumn DataField="Description" Width="200px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <CallbackCommands>
            <Save PostData="Page" />
        </CallbackCommands>
    </px:PXGrid>
</asp:Content>
