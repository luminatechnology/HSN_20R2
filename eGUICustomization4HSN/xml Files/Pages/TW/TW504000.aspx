<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TW504000.aspx.cs" Inherits="Page_TW504000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="eGUICustomization4HSN.Graph.TWNExpGUICN2BankPro"
        PrimaryView="GUITranProc">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid  AdjustPageSize="Auto" AllowPaging="True" SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Inquire" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="GUITranProc">
			    <Columns>
			        <px:PXGridColumn Type="CheckBox" AllowCheckAll="True" DataField="Selected" Width="60" ></px:PXGridColumn>
				<px:PXGridColumn DataField="GUIStatus" Width="120" />
				<px:PXGridColumn DataField="GUIFormatcode" Width="70" />
				<px:PXGridColumn DataField="GUINbr" Width="140" />
				<px:PXGridColumn DataField="GUIDate" Width="90" />
				<px:PXGridColumn DataField="OurTaxNbr" Width="96" />
				<px:PXGridColumn DataField="TaxNbr" Width="96" />
				<px:PXGridColumn DataField="GUITitle" Width="220" />
				<px:PXGridColumn DataField="NetAmount" Width="100" />
				<px:PXGridColumn DataField="TaxAmount" Width="100" />
				<px:PXGridColumn DataField="VATType" Width="70" />
				<px:PXGridColumn DataField="OrderNbr" Width="140" />
				<px:PXGridColumn DataField="CustomType" Width="70" />
				<px:PXGridColumn DataField="BatchNbr" Width="140" />
				<px:PXGridColumn Type="CheckBox" DataField="EGUIExported" Width="60" ></px:PXGridColumn>
				<px:PXGridColumn DataField="EGUIExportedDateTime" Width="90" />
			    </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" />
		<ActionBar >
		</ActionBar>
	</px:PXGrid>
</asp:Content>