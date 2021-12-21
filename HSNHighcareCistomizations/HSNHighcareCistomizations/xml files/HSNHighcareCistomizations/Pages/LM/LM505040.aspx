<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM505040.aspx.cs" Inherits="Page_LM505040" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="HSNHighcareCistomizations.Graph.PINCodeDeferredScheduleProc"
        PrimaryView="ProcessList">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="PrimaryInquire" AllowAutoHide="false">
        <Levels>
            <px:PXGridLevel DataMember="ProcessList">
                <Columns>
                    <px:PXGridColumn DataField="Selected" Width="40" Type="CheckBox" TextAlign="Center" CommitChanges="True" AllowCheckAll="True"></px:PXGridColumn>
                    <px:PXGridColumn DataField="Customer__AcctCD" Width="200px" />
                    <px:PXGridColumn DataField="Pin" AllowNull="False" />
                    <px:PXGridColumn DataField="LUMPINCodeMapping__SerialNbr" AllowNull="False" />
                    <px:PXGridColumn DataField="CPriceClassID" AllowNull="False" />
                    <px:PXGridColumn DataField="ScheduleNbr" AllowNull="False" />
                    <px:PXGridColumn DataField="StartDate" Width="200px" />
                    <px:PXGridColumn DataField="EndDate" Width="200px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar>
        </ActionBar>
    </px:PXGrid>

</asp:Content>
