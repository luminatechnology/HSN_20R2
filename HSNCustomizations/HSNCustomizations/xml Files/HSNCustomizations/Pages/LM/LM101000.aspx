<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM101000.aspx.cs" Inherits="Page_LM101000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="HSNCustomizations.LUMHSNSetupMaint" PrimaryView="hSNSetup">
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXTab DataMember="hSNSetup" ID="tab" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" AllowAutoHide="false">
		<Items>
			<px:PXTabItem Text="General Setting">
			
				<Template>
					<px:PXLayoutRule GroupCaption="NUMBERING SETTING" runat="server" ID="CstPXLayoutRule1" StartGroup="True" LabelsWidth="L" ControlSize="" ></px:PXLayoutRule>
								<px:PXSelector runat="server" ID="CstPXSelector3" DataField="CPrepaymentNumberingID" AllowEdit="True" ></px:PXSelector>
					<px:PXLayoutRule GroupCaption="DATA ENTRY SETTING" runat="server" ID="CstPXLayoutRule2" StartGroup="True" LabelsWidth="M" ControlSize="" ></px:PXLayoutRule>
								<px:PXCheckBox AlignLeft="True" runat="server" ID="CstPXCheckBox4" DataField="EnableUniqSerialNbrByEquipType" ></px:PXCheckBox>
								<px:PXCheckBox AlignLeft="True" runat="server" ID="CstPXCheckBox5" DataField="EnablePartReqInAppt" ></px:PXCheckBox>
								<px:PXCheckBox AlignLeft="True" runat="server" ID="CstPXCheckBox6" DataField="EnableRMAProcInAppt" ></px:PXCheckBox>
								<px:PXCheckBox AlignLeft="True" runat="server" ID="CstPXCheckBox10" DataField="EnableHeaderNoteSync" ></px:PXCheckBox>
								<px:PXCheckBox AlignLeft="True" runat="server" ID="CstPXCheckBox11" DataField="EnableChgInvTypeOnBill" ></px:PXCheckBox>
								<px:PXCheckBox AlignLeft="True" runat="server" ID="CstPXCheckBox12" DataField="DisplayTransferToHQ" ></px:PXCheckBox>
								<px:PXCheckBox AlignLeft="True" runat="server" ID="CstPXCheckBox13" DataField="DispApptActiviteInSrvOrd" ></px:PXCheckBox></Template></px:PXTabItem>
			<px:PXTabItem Text="Branch Warehouse">
			
				<Template>
					<px:PXGrid runat="server" ID="CstPXGrid7" Width="100%" SkinID="DetailsInTab" DataSourceID="ds" SyncPosition="True">
						<Levels>
							<px:PXGridLevel DataMember="BranchWarehouse" >
								<Columns>
									<px:PXGridColumn DataField="BranchID" Width="140" />
									<px:PXGridColumn DataField="SiteID" Width="140" /></Columns>
								<RowTemplate>
									<px:PXSegmentMask AllowEdit="True" runat="server" ID="CstPXSegmentMask8" DataField="BranchID" ></px:PXSegmentMask>
									<px:PXSegmentMask runat="server" ID="CstPXSegmentMask9" DataField="SiteID" AllowEdit="True" /></RowTemplate></px:PXGridLevel></Levels>
						<AutoSize Enabled="True" /></px:PXGrid></Template></px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" ></AutoSize>
	</px:PXTab>
</asp:Content>