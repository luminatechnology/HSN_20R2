<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LL501000.aspx.cs" Inherits="Page_LL501000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="HSNFinance.LUMCalcInterestExpProc" PrimaryView="Filter">
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" Height="70px" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXBranchSelector CommitChanges="True" ID="BranchSelector1" runat="server" DataField="OrgBAccountID" MarkRequired="Dynamic" />
			<px:PXSelector CommitChanges="True" runat="server" InputMask="##-####" DataField="PeriodID" ID="edPeriodID" Size="S" AutoRefresh="true"/>
			<px:PXLayoutRule runat="server" StartColumn="true" LabelsWidth="S" ControlSize="M"/>
			<px:PXSelector CommitChanges="True" runat="server" DataField="ClassID" ID="edClassID" />
			<px:PXSelector CommitChanges="True" runat="server" DataField="BookID" ID="edBookID" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" AllowAutoHide="false" SyncPosition="true" AllowPaging="true" AdjustPageSize="Auto" AutoAdjustColumns="true">
		<Levels>
			<px:PXGridLevel DataMember="InterestExp">
			    <Columns>
			        <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" CommitChanges="true" ></px:PXGridColumn>
					<px:PXGridColumn DataField="AssetID"></px:PXGridColumn>
					<px:PXGridColumn DataField="TermCount"></px:PXGridColumn>
					<px:PXGridColumn DataField="AssetID_description" Width="350" />
					<px:PXGridColumn DataField="BranchID" DisplayMode="Hint" />
					<px:PXGridColumn DataField="BookID"></px:PXGridColumn>
					<px:PXGridColumn DataField="FinPeriodID" DisplayFormat="##-####" />
					<px:PXGridColumn DataField="BegBalance"></px:PXGridColumn>
					<px:PXGridColumn DataField="MonthlyRent"></px:PXGridColumn>
					<px:PXGridColumn DataField="InterestRate"></px:PXGridColumn>
					<px:PXGridColumn DataField="EndBalance"></px:PXGridColumn></Columns>
				<RowTemplate>
					<px:PXSelector runat="server" ID="CstPXSelector1" DataField="AssetID" AllowEdit="True"  ></px:PXSelector>
					<px:PXSelector runat="server" ID="CstPXSelector2" DataField="BookID" AllowEdit="True" />
					<px:PXSegmentMask runat="server" ID="CstPXSegmentMask3" DataField="BranchID" AllowEdit="true" /></RowTemplate></px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar >
		</ActionBar>
	</px:PXGrid>
</asp:Content>