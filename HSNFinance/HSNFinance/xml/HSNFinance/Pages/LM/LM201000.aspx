<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM201000.aspx.cs" Inherits="Page_LM201000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="HSNFinance.LUMHyperionAcctMapMaint" PrimaryView="HyperionAcctMapping">
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" SkinID="Primary" AllowAutoHide="false" AllowPaging="True">
		<Levels>
			<px:PXGridLevel DataMember="HyperionAcctMapping">
			    <Columns>
				<px:PXGridColumn DataField="AccountID" Width="150" />
				<px:PXGridColumn DataField="AccountID_Account_description" Width="350" />
				<px:PXGridColumn DataField="SubID" Width="150" />
				<px:PXGridColumn DataField="SubID_Sub_description" Width="350" />
				<px:PXGridColumn DataField="HyperionAcct" Width="200" /></Columns>	
				<RowTemplate>
					<px:PXSegmentMask runat="server" ID="CstPXSegmentMask1" DataField="AccountID" AllowEdit="True"></px:PXSegmentMask>
					<px:PXSegmentMask runat="server" ID="CstPXSegmentMask2" DataField="SubID" AllowEdit="True" />
				</RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" ></AutoSize>
		<ActionBar PagerVisible="Bottom"><PagerSettings Mode="NumericCompact" /></ActionBar>
		<Mode AllowUpload="True" /></px:PXGrid>
</asp:Content>