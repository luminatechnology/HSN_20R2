<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TW401000.aspx.cs" Inherits="Page_TW401000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="eGUICustomization4HSN.Graph.eGUIInquiry"
        PrimaryView="ViewGUITrans" >
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server"></asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid FastFilterFields="GUINbr" AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True" Width="100%" SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" SkinID="Primary" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="ViewGUITrans">
			    <Columns>
				<px:PXGridColumn DataField="GUIStatus" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Branch" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Branch_Branch_acctName" Width="220" ></px:PXGridColumn>
				<px:PXGridColumn DataField="GUIDirection" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="GUIFormatcode" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn Visible="" DataField="GUIControlCode" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Guinbr" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Guidate" Width="90" ></px:PXGridColumn>
				<px:PXGridColumn DataField="GUIDecPeriod" Width="90" ></px:PXGridColumn>
				<px:PXGridColumn DataField="TaxZoneID" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn AllowResize="True" DataField="TaxCategoryID" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Taxid" Width="120" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Vattype" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn CommitChanges="True" DataField="TaxNbr" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn CommitChanges="True" DataField="OurTaxNbr" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="NetAmount" Width="100" ></px:PXGridColumn>
				<px:PXGridColumn DataField="TaxAmount" Width="100" ></px:PXGridColumn>
				<px:PXGridColumn DataField="NetAmtRemain" Width="100" ></px:PXGridColumn>
				<px:PXGridColumn DataField="TaxAmtRemain" Width="100" ></px:PXGridColumn>
				<px:PXGridColumn DataField="SequenceNo" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CustVend" Width="80" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CustVendName" Width="280" ></px:PXGridColumn>
				<px:PXGridColumn DataField="DeductionCode" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="BatchNbr" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="OrderNbr" Width="80" ></px:PXGridColumn>
				<px:PXGridColumn DataField="TransDate" Width="90" ></px:PXGridColumn>
				<px:PXGridColumn DataField="PrintedDate" Width="90" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ExportMethods" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ExportTicketType" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ExportTicketNbr" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CustomType" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn Visible="" DataField="ClearingDate" Width="90" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Remark" Width="80" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Guititle" Width="220" ></px:PXGridColumn>
				<px:PXGridColumn Type="CheckBox" DataField="EGUIExcluded" Width="60" ></px:PXGridColumn>
				<px:PXGridColumn Type="CheckBox" DataField="EGUIExported" Width="60" ></px:PXGridColumn>
				<px:PXGridColumn DataField="EGUIExportedDateTime" Width="90" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CarrierType" Width="72" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CarrierID" Width="220" ></px:PXGridColumn>
				<px:PXGridColumn DataField="NPONbr" Width="120" ></px:PXGridColumn>
				<px:PXGridColumn Type="CheckBox" DataField="B2CPrinted" Width="60" ></px:PXGridColumn>
				<px:PXGridColumn DataField="PrintCount" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn Visible="" DataField="QREncrypter" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CreatedDateTime" Width="90" ></px:PXGridColumn></Columns>
			
				<RowTemplate>
					<px:PXSelector AllowEdit="True" runat="server" ID="CstPXSelector1" DataField="ExportMethods" ></px:PXSelector>
					<px:PXSelector AllowEdit="True" runat="server" ID="CstPXSelector2" DataField="ExportTicketType" ></px:PXSelector>
					<px:PXSelector AllowEdit="True" runat="server" ID="CstPXSelector3" DataField="NPONbr" ></px:PXSelector>
					<px:PXSelector runat="server" ID="CstPXSelector4" DataField="OrderNbr" AllowEdit="True" ></px:PXSelector>
								<px:PXSelector AllowEdit="True" runat="server" ID="CstPXSelector5" DataField="TaxCategoryID" />
								<px:PXSelector AllowEdit="True" runat="server" ID="CstPXSelector6" DataField="TaxID" />
								<px:PXSelector AllowEdit="True" runat="server" ID="CstPXSelector7" DataField="TaxZoneID" ></px:PXSelector></RowTemplate></px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True"></AutoSize>
		<ActionBar >
		</ActionBar>
	
		<Mode AllowUpload="True" AllowAddNew="False" ></Mode>
		<Mode AllowDelete="False" ></Mode></px:PXGrid>
</asp:Content>