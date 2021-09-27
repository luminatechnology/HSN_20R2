<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM501000.aspx.cs" Inherits="Page_LM501000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="HSNCustomizations.Graph.PrintTransferProcess"
        PrimaryView="MasterView"
        >
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="MasterView" Width="100%" Height="80px" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule runat="server" ID="CstPXLayoutRule11" StartRow="True" ></px:PXLayoutRule>
			<px:PXLayoutRule runat="server" ID="CstPXLayoutRule13" StartColumn="True" ></px:PXLayoutRule>
			<px:PXDropDown Size="M" runat="server" ID="CstPXDropDown12" DataField="ReportType" ></px:PXDropDown>
			<px:PXLayoutRule runat="server" ID="CstPXLayoutRule16" StartColumn="True" ></px:PXLayoutRule>
			<px:PXDateTimeEdit CommitChanges="True" runat="server" ID="CstPXDateTimeEdit7" DataField="StartDate" ></px:PXDateTimeEdit>
			<px:PXLayoutRule runat="server" ID="CstPXLayoutRule10" StartRow="True" ></px:PXLayoutRule>
			<px:PXLayoutRule runat="server" ID="CstPXLayoutRule8" StartColumn="True" ></px:PXLayoutRule>
			<px:PXSegmentMask Size="M" CommitChanges="True" runat="server" ID="CstPXSegmentMask15" DataField="SiteID" ></px:PXSegmentMask>
			<px:PXLayoutRule runat="server" ID="CstPXLayoutRule9" StartColumn="True" ></px:PXLayoutRule>
			<px:PXDateTimeEdit CommitChanges="True" runat="server" ID="CstPXDateTimeEdit6" DataField="EndDate" ></px:PXDateTimeEdit></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" ActionsPosition="Top" SkinID="PrimaryInquire" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="DetailsView">
			    <Columns>
				<px:PXGridColumn TextAlign="Center" AllowCheckAll="True" Type="CheckBox" DataField="Selected" Width="60" ></px:PXGridColumn>
				<px:PXGridColumn DataField="RefNbr" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="TranDesc" Width="280" ></px:PXGridColumn>
				<px:PXGridColumn Type="CheckBox" DataField="UsrPLIsPrinted" Width="60" ></px:PXGridColumn>
				<px:PXGridColumn DataField="UsrPickingListNumber" Width="120" ></px:PXGridColumn>
				<px:PXGridColumn DataField="UsrTrackingNbr" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn Type="CheckBox" DataField="UsrDOIsPrinted" Width="60" ></px:PXGridColumn>
				<px:PXGridColumn DataField="UsrDeliveryOrderNumber" Width="120" ></px:PXGridColumn>
				<px:PXGridColumn DataField="LastModifiedDateTime" Width="90" />
				<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Width="70" />
				<px:PXGridColumn DataField="SiteID" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ToSiteID" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="UsrAppointmentNbr" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="TranDate" Width="90" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ExtRefNbr" Width="180" ></px:PXGridColumn>
				<px:PXGridColumn DataField="UsrSrvOrdType" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="DocType" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="TransferType" Width="70" ></px:PXGridColumn></Columns>
			
				<RowTemplate>
					<px:PXTextEdit Enabled="True" runat="server" ID="CstPXTextEdit14" DataField="UsrCheckingNbr" ></px:PXTextEdit></RowTemplate></px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" ></AutoSize>
		<ActionBar >
		</ActionBar>
	</px:PXGrid>
</asp:Content>