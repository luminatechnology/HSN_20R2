<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="LM630540.aspx.cs" Inherits="Page_LM630540" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="HSNFinance.Graph.LumARAgedPeriodMaint"
        PrimaryView="MasterFilter"
        >
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="MasterFilter" Width="100%" Height="50px" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True"></px:PXLayoutRule>
			<px:PXSelector runat="server" ID="CstPXSelector1" DataField="FinPeriodID" CommitChanges="True" ></px:PXSelector></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Details" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="DetailsView">
			    <Columns>
				<px:PXGridColumn DataField="ConditionPeriodID" Width="100" />
				<px:PXGridColumn DataField="Current" Width="120" />
				<px:PXGridColumn DataField="OneMDays" Width="120" />
				<px:PXGridColumn DataField="TwoMDays" Width="120" />
				<px:PXGridColumn DataField="ThreeMDays" Width="120" />
				<px:PXGridColumn DataField="FourMDays" Width="120" />
				<px:PXGridColumn DataField="FiveMDays" Width="120" />
				<px:PXGridColumn DataField="SixMDays" Width="120" />
				<px:PXGridColumn DataField="SevenMDays" Width="120" />
				<px:PXGridColumn DataField="OverSevenMDays" Width="120" />
				<px:PXGridColumn DataField="Total" Width="150" /></Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar >
		</ActionBar>
	</px:PXGrid>
</asp:Content>