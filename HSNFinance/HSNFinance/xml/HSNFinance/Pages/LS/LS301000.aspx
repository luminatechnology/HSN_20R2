<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LS301000.aspx.cs" Inherits="Page_LS301000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
  <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="HSNFinance.LSLedgerStlmtEntry" PrimaryView="Filter">
    <CallbackCommands>
    </CallbackCommands>
  </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
  <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" Height="80px" AllowAutoHide="false">
    <Template>
	<px:PXLayoutRule runat="server" StartRow="True" ID="PXLayoutRule2" ControlSize="XL" ></px:PXLayoutRule>
	<%--<px:PXLayoutRule runat="server" ID="CstPXLayoutRule1" StartColumn="True" ></px:PXLayoutRule>--%>
	<%--<px:PXSegmentMask runat="server" ID="CstPXSegmentMask4" DataField="BranchID" ></px:PXSegmentMask>--%>
	<px:PXLayoutRule runat="server" ID="CstPXLayoutRule2" StartColumn="True" ></px:PXLayoutRule>
	<%--<px:PXLayoutRule runat="server" ID="CstPXLayoutRule7" Merge="True" ></px:PXLayoutRule>--%>
	<px:PXSelector CommitChanges="True" runat="server" ID="CstPXSelector5" DataField="StlmtAcctID" ></px:PXSelector>
	<px:PXLayoutRule runat="server" ID="CstLayoutRule6" ></px:PXLayoutRule>
	<px:PXTextEdit runat="server" ID="CstPXTextEdit9" DataField="StlmtAcctID_description" ></px:PXTextEdit>
	<px:PXLayoutRule runat="server" ID="CstPXLayoutRule1" StartColumn="True" ></px:PXLayoutRule>
	<px:PXNumberEdit runat="server" ID="CstPXNumberEdit8" DataField="BalanceAmt" ></px:PXNumberEdit>
    </Template>
  </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXSplitContainer runat="server" Orientation="Horizontal" SplitterPosition="400" ID="splitConditions">
		<AutoSize Container="Window" Enabled="true" ></AutoSize>
		<Template1>
			<px:PXGrid ScrollBars="Always" AllowPaging="True" Caption="Ledger Source" CaptionVisible="True" runat="server" SyncPosition="True" Height="300px" SkinID="Primary" TabIndex="700" Width="100%" ID="grdScanMaster" DataSourceID="ds" AdjustPageSize="Auto" NoteIndicator="false" FilesIndicator="false">
				<AutoSize Enabled="True" ></AutoSize>
				<Levels>
					<px:PXGridLevel DataMember="GLTranDebit">
						<Columns >
							<px:PXGridColumn DataField="Selected" TextAlign="Center" CommitChanges="True" Type="CheckBox" AllowCheckAll="True" Width="40" ></px:PXGridColumn>
							<px:PXGridColumn DataField="BatchNbr" Width="140" ></px:PXGridColumn>
							<px:PXGridColumn DataField="LineNbr" Width="70" ></px:PXGridColumn>
							<px:PXGridColumn DataField="BranchID" Width="140" ></px:PXGridColumn>
							<px:PXGridColumn DataField="TranDate" Width="90" ></px:PXGridColumn>
							<px:PXGridColumn DataField="SubID" Width="140" ></px:PXGridColumn>
							<px:PXGridColumn DataField="RefNbr" Width="140" ></px:PXGridColumn>
							<px:PXGridColumn DataField="CuryDebitAmt" Width="100" ></px:PXGridColumn>
							<px:PXGridColumn DataField="CuryCreditAmt" Width="100" ></px:PXGridColumn>
							<px:PXGridColumn DataField="UsrRmngDebitAmt" Width="100" ></px:PXGridColumn>
							<px:PXGridColumn DataField="UsrRmngCreditAmt" Width="100" ></px:PXGridColumn>
							<px:PXGridColumn DataField="UsrSetldDebitAmt" Width="100" CommitChanges="True" ></px:PXGridColumn>
							<px:PXGridColumn DataField="UsrSetldCreditAmt" Width="100" CommitChanges="True" ></px:PXGridColumn>
							<px:PXGridColumn DataField="TranDesc" Width="280" ></px:PXGridColumn>
							<px:PXGridColumn DataField="InventoryID" Width="70" ></px:PXGridColumn>
							<px:PXGridColumn DataField="ReferenceID" Width="140" ></px:PXGridColumn>
							<px:PXGridColumn DataField="ProjectID" Width="70" ></px:PXGridColumn>
							<px:PXGridColumn DataField="TaskID" Width="70" ></px:PXGridColumn>
							<px:PXGridColumn DataField="CostCodeID" Width="70" ></px:PXGridColumn></Columns></px:PXGridLevel></Levels>
				<%--<Mode AllowAddNew="False" InitNewRow="True" ></Mode>--%>
				<ActionBar PagerVisible="Bottom"><PagerSettings Mode="NumericCompact" /></ActionBar></px:PXGrid></Template1>
		<Template2>
			<px:PXGrid ScrollBars="Always" AllowPaging="True" SyncPosition="True" CaptionVisible="True" Caption="Ledge Settlement" runat="server" SkinID="Primary" Width="100%" ID="grdScanDetail" DataSourceID="ds" AdjustPageSize="Auto" NoteIndicator="false" FilesIndicator="false">
				<AutoSize Enabled="True" ></AutoSize>
				<Levels>
					<px:PXGridLevel DataMember="GLTranCredit">
						<Columns >
							<px:PXGridColumn DataField="Selected" TextAlign="Center" CommitChanges="True" Type="CheckBox" AllowCheckAll="True" Width="40" ></px:PXGridColumn>
							<px:PXGridColumn DataField="BatchNbr" Width="140" ></px:PXGridColumn>
							<px:PXGridColumn DataField="LineNbr" Width="70" ></px:PXGridColumn>
							<px:PXGridColumn DataField="BranchID" Width="140" ></px:PXGridColumn>
							<px:PXGridColumn DataField="TranDate" Width="90" ></px:PXGridColumn>
							<px:PXGridColumn DataField="SubID" Width="140" ></px:PXGridColumn>
							<px:PXGridColumn DataField="RefNbr" Width="140" ></px:PXGridColumn>
							<px:PXGridColumn DataField="CuryDebitAmt" Width="100" ></px:PXGridColumn>
							<px:PXGridColumn DataField="CuryCreditAmt" Width="100" ></px:PXGridColumn>
							<px:PXGridColumn DataField="UsrRmngDebitAmt" Width="100" ></px:PXGridColumn>
							<px:PXGridColumn DataField="UsrRmngCreditAmt" Width="100" ></px:PXGridColumn>
							<px:PXGridColumn DataField="UsrSetldDebitAmt" Width="100" CommitChanges="True" ></px:PXGridColumn>
							<px:PXGridColumn DataField="UsrSetldCreditAmt" Width="100" CommitChanges="True" ></px:PXGridColumn>
							<px:PXGridColumn DataField="TranDesc" Width="280" ></px:PXGridColumn>
							<px:PXGridColumn DataField="InventoryID" Width="70" ></px:PXGridColumn>
							<px:PXGridColumn DataField="ReferenceID" Width="140" ></px:PXGridColumn>
							<px:PXGridColumn DataField="ProjectID" Width="70" ></px:PXGridColumn>
							<px:PXGridColumn DataField="TaskID" Width="70" ></px:PXGridColumn>
							<px:PXGridColumn DataField="CostCodeID" Width="70" ></px:PXGridColumn></Columns></px:PXGridLevel></Levels>
				<%--<Mode InitNewRow="True" ></Mode>--%>
				<ActionBar PagerVisible="Bottom"><PagerSettings Mode="NumericCompact" /></ActionBar></px:PXGrid></Template2></px:PXSplitContainer></asp:Content>