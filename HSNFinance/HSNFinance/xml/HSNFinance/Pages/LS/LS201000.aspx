<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LS201000.aspx.cs" Inherits="Page_LS201000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="HSNFinance.LSStlmtAccountMaint" PrimaryView="StlmtAccount">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Primary" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="StlmtAccount">
			    <Columns>
				<px:PXGridColumn DataField="AccountID" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="AccountCD" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Description" Width="220" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Type" Width="70" ></px:PXGridColumn></Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" ></AutoSize>
		<ActionBar >
		</ActionBar>
	</px:PXGrid>
</asp:Content>