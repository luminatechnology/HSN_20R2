<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LS401000.aspx.cs" Inherits="Page_LS401000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="HSNFinance.LSLedgerStlmtInq" PrimaryView="LedgerStlmt">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid AllowSearch="True" FastFilterFields="BatchNbr,AccountID,SubID,RefNbr,TranDesc,InventoryID,ProjectID,TaskID,CostCodeID" AdjustPageSize="Auto" SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="PrimaryInquire" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="LedgerStlmt">
			    <Columns>
				<px:PXGridColumn DataField="Selected" Width="40" Type="CheckBox" AllowCheckAll="True" ></px:PXGridColumn>
				<px:PXGridColumn DataField="SettlementNbr" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="BranchID" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="BatchNbr" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="LineNbr" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Module" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="LedgerID" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="AccountID" Width="120" ></px:PXGridColumn>
				<px:PXGridColumn DataField="SubID" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="OrigDebitAmt" Width="100" ></px:PXGridColumn>
				<px:PXGridColumn DataField="OrigCreditAmt" Width="100" ></px:PXGridColumn>
				<px:PXGridColumn DataField="SettledDebitAmt" Width="100" ></px:PXGridColumn>
				<px:PXGridColumn DataField="SettledCreditAmt" Width="100" ></px:PXGridColumn>
				<px:PXGridColumn DataField="TranDesc" Width="280" ></px:PXGridColumn>
				<px:PXGridColumn DataField="TranDate" Width="90" ></px:PXGridColumn>
				<px:PXGridColumn DataField="RefNbr" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="InventoryID" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ProjectID" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="TaskID" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CostCodeID" Width="70" ></px:PXGridColumn></Columns>
			
				<RowTemplate>
					<px:PXSelector runat="server" ID="CstPXSelector1" DataField="BatchNbr" AllowEdit="True" ></px:PXSelector>
								<px:PXSegmentMask AllowEdit="True" runat="server" ID="CstPXSegmentMask2" DataField="InventoryID" ></px:PXSegmentMask>
								<px:PXSelector runat="server" ID="CstPXSelector3" DataField="LedgerID" AllowEdit="True" ></px:PXSelector>
								<px:PXSegmentMask AllowEdit="True" runat="server" ID="CstPXSegmentMask4" DataField="ProjectID" ></px:PXSegmentMask>
								<px:PXSegmentMask AllowEdit="True" runat="server" ID="CstPXSegmentMask5" DataField="TaskID" ></px:PXSegmentMask></RowTemplate></px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" ></AutoSize>
		<ActionBar >
		</ActionBar>
	</px:PXGrid>
</asp:Content>