using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HSNCustomizations.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common;
using PX.Objects.Common.Bql;
using PX.Objects.FS;
using PX.Objects.IN;

namespace HSNCustomizations.Graph
{
    public class PrintTransferProcess : PXGraph<PrintTransferProcess>
    {

        //public PXSave<TransferFilter> Save;
        public PXCancel<TransferFilter> Cancel;
        public PXFilteredProcessing<INRegister, TransferFilter> TransferRecords;
        
        public PXFilter<TransferFilter> MasterView;
        [PXFilterable]
        public SelectFrom<INRegister>.View DetailsView;
        public SelectFrom<LumINTran>.View LumINTranView;

        #region Transfer Report Type
        Dictionary<string, string> dicTransferReportType = new Dictionary<string, string>()
        {
            { "PickingList", "LM644005" },
            { "DeliveryOrder", "LM644010" }
        };
        #endregion

        public PrintTransferProcess()
        {
            //TransferRecords.SetProcessVisible(false);
            TransferRecords.SetProcessAllVisible(false);
            TransferRecords.SetProcessDelegate(list => PrintTransfers(list));
        }

        public void PrintTransfers(IEnumerable<INRegister> list)
        {
            PrintTransferProcess printTransferProcessGraph = PXGraph.CreateInstance<PrintTransferProcess>();
            TransferFilter transferFilter = MasterView.Current as TransferFilter;
            PXCache cache = this.Caches[typeof(LumINTran)];

            // Truncate Table
            /*Connect to Database*/
            using (new PXConnectionScope())
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    /*Execute Stored Procedure*/
                    PXDatabase.Execute("SP_TruncateLumINTran", new PXSPParameter[0]);
                    ts.Complete();
                }
            }

            foreach (var transfer in list)
            {
                var result = SelectFrom<INTran>
                                .LeftJoin<INRegister>.On<INRegister.docType.IsEqual<INTran.docType>.And<INRegister.refNbr.IsEqual<INTran.refNbr>>>
                                .Where<INTran.docType.IsEqual<@P.AsString>.And<INTran.refNbr.IsEqual<@P.AsString>>>
                                .View.Select(this, transfer.DocType, transfer.RefNbr);

                foreach (PXResult<INTran, INRegister> line in result)
                {
                    LumINTran lumINTran = new LumINTran();

                    INTran iNTranLine = line;
                    INRegister iNRegisterLine = line;

                    lumINTran.DocType = iNTranLine.DocType;
                    lumINTran.RefNbr = iNTranLine.RefNbr;
                    lumINTran.LineNbr = iNTranLine.LineNbr;
                    lumINTran.TranDate = iNTranLine.TranDate;
                    lumINTran.TranType = iNTranLine.TranType;
                    lumINTran.InventoryID = iNTranLine.InventoryID;
                    lumINTran.Siteid = iNTranLine.SiteID;
                    lumINTran.InvtMult = iNTranLine.InvtMult;
                    lumINTran.LocationID = iNTranLine.LocationID;
                    lumINTran.Qty = iNTranLine.Qty;
                    lumINTran.TranDesc = iNTranLine.TranDesc;
                    lumINTran.Uom = iNTranLine.UOM;
                    lumINTran.Tositeid = iNTranLine.ToSiteID;
                    lumINTran.UsrAppointmentNbr = iNRegisterLine.GetExtension<INRegisterExt>().UsrAppointmentNbr;
                    this.Caches[typeof(LumINTran)].Update(lumINTran);
                }

                if (transferFilter.ReportType == dicTransferReportType["PickingList"])      transfer.GetExtension<INRegisterExt>().UsrPLIsPrinted = true;
                if (transferFilter.ReportType == dicTransferReportType["DeliveryOrder"])    transfer.GetExtension<INRegisterExt>().UsrDOIsPrinted = true;
                this.Caches[typeof(INRegister)].Update(transfer);

                this.Actions.PressSave();
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            throw new PXReportRequiredException(parameters, transferFilter.ReportType, $"Report {transferFilter.ReportType}");
        }

        #region Delegate DataView
        public IEnumerable detailsView()
        {
            TransferFilter transferFilter = MasterView.Current as TransferFilter;
            var currentSearchStartDate = transferFilter?.StartDate;
            var currentSearchEndDate = transferFilter?.EndDate;
            var currentFromWarehouse = transferFilter?.SiteID;

            if (currentFromWarehouse == null)
            {
                if (currentSearchStartDate == null)
                    return SelectFrom<INRegister>
                        .Where<INRegister.tranDate.IsLessEqual<@P.AsDateTime>.And<INRegister.docType.IsEqual<@P.AsString>>>
                        .View.Select(this, currentSearchEndDate, "T");
                else if (currentSearchStartDate != null && currentSearchEndDate != null)
                    return SelectFrom<INRegister>
                        .Where<INRegister.tranDate.IsGreaterEqual<@P.AsDateTime>.And<INRegister.tranDate.IsLessEqual<@P.AsDateTime>.And<INRegister.docType.IsEqual<@P.AsString>>>>
                        .View.Select(this, currentSearchStartDate, currentSearchEndDate, "T");
                else
                    return SelectFrom<INRegister>.Where<INRegister.docType.IsEqual<@P.AsString>>
                        .View.Select(this, "T");
            }
            else
            {
                if (currentSearchStartDate == null)
                    return SelectFrom<INRegister>
                        .Where<INRegister.tranDate.IsLessEqual<@P.AsDateTime>.And<INRegister.docType.IsEqual<@P.AsString>>.And<INRegister.siteID.IsEqual<@P.AsInt>>>
                        .View.Select(this, currentSearchEndDate, "T", currentFromWarehouse);
                else if (currentSearchStartDate != null && currentSearchEndDate != null)
                    return SelectFrom<INRegister>
                        .Where<INRegister.tranDate.IsGreaterEqual<@P.AsDateTime>.And<INRegister.tranDate.IsLessEqual<@P.AsDateTime>.And<INRegister.docType.IsEqual<@P.AsString>>>.And<INRegister.siteID.IsEqual<@P.AsInt>>>
                        .View.Select(this, currentSearchStartDate, currentSearchEndDate, "T", currentFromWarehouse);
                else
                    return SelectFrom<INRegister>.Where<INRegister.docType.IsEqual<@P.AsString>.And<INRegister.siteID.IsEqual<@P.AsInt>>>
                        .View.Select(this, "T", currentFromWarehouse);
            }
        }
        #endregion

        #region Transfer Filter
        [Serializable]
        [PXCacheName("Transfer Filter")]
        public class TransferFilter : IBqlTable
        {
            #region ReportType
            [PXDBString(8)]
            [PXUIField(DisplayName = "Action")]
            [PXStringList(
                    new string[] { "LM644005", "LM644010" },
                    new string[] { "Print Picking List", "Print Delivery Order" })]
            [PXDefault("LM644005")]
            public virtual string ReportType { get; set; }
            public abstract class reportType : PX.Data.BQL.BqlString.Field<reportType> { }
            #endregion

            #region StartDate
            [PXDBDate]
            [PXDefault]
            [PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
            public virtual DateTime? StartDate { get; set; }
            public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
            #endregion

            #region EndDate
            [PXDBDate]
            [PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.SelectorVisible)]
            [PXDefault(typeof(AccessInfo.businessDate))]
            public virtual DateTime? EndDate { get; set; }
            public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
            #endregion

            #region SiteID
            [PX.Objects.IN.Site(DisplayName = "From Warehouse", DescriptionField = typeof(INSite.descr))]
            public virtual Int32? SiteID { get; set; }
            public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
            #endregion
        }
        #endregion
    }
}