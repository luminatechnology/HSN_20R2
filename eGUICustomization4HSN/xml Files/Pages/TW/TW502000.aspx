<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TW502000.aspx.cs" Inherits="Page_TW502000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="eGUICustomization4HSN.Graph.TWNGenZeroTaxRateMedFile"
        PrimaryView="FilterGUITran">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="FilterGUITran" Width="100%" Height="50px" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True"></px:PXLayoutRule>
			<px:PXDateTimeEdit runat="server" ID="CstPXDateTimeEdit1" DataField="FromDate" CommitChanges="True" ></px:PXDateTimeEdit>
			<px:PXLayoutRule runat="server" ID="CstPXLayoutRule3" StartColumn="True" ></px:PXLayoutRule>
			<px:PXDateTimeEdit CommitChanges="True" runat="server" ID="CstPXDateTimeEdit2" DataField="ToDate" ></px:PXDateTimeEdit></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid AdjustPageSize="Auto" AllowPaging="True" SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Inquire" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="GUITranProc">
			    <Columns>
				<px:PXGridColumn AllowCheckAll="True" Type="CheckBox" DataField="Selected" Width="60" ></px:PXGridColumn>
				<px:PXGridColumn DataField="GUIFormatcode" Width="70" />
				<px:PXGridColumn DataField="GUIDate" Width="90" />
				<px:PXGridColumn DataField="GUINbr" Width="140" />
				<px:PXGridColumn DataField="TaxNbr" Width="96" />
				<px:PXGridColumn DataField="VATType" Width="70" />
				<px:PXGridColumn DataField="CustomType" Width="140" />
				<px:PXGridColumn DataField="ExportMethods" Width="70" />
				<px:PXGridColumn DataField="ExportTicketType" Width="70" />
				<px:PXGridColumn DataField="ExportTicketNbr" Width="140" />
				<px:PXGridColumn DataField="NetAmount" Width="100" />
				<px:PXGridColumn DataField="ClearingDate" Width="90" /></Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" ></AutoSize>
		<ActionBar >
		</ActionBar>
	</px:PXGrid>
</asp:Content>