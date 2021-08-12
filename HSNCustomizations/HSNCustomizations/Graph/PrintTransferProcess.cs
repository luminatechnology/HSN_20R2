using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
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

        public PrintTransferProcess()
        {
            this.TransferRecords.Cache.AllowInsert = false;
            this.TransferRecords.Cache.AllowDelete = false;
            this.TransferRecords.Cache.AllowUpdate = false;
            TransferRecords.SetProcessDelegate(list => PrintTransfers(list));
        }

        public void PrintTransfers(IEnumerable<INRegister> list)
        {
            //LM644005 Picking List
            //LM644010 Delivery Order
            TransferFilter filter = PXCache<TransferFilter>.CreateCopy(MasterView.Current);
            PXReportRequiredException ex = null;
            foreach (var transfer in list)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                string docType = SharedFunctions.GetFieldName<INRegister.docType>(true);
                string refNbr = SharedFunctions.GetFieldName<INRegister.refNbr>(true);
                parameters["DocType"] = transfer.DocType;
                parameters["RefNbr"] = transfer.RefNbr;

                if (ex == null)
                {
                    ex = new PXReportRequiredException(parameters, filter.ReportType, filter.ReportType);
                }
                else
                {
                    ex.AddSibling(filter.ReportType, parameters, false);
                }
            }
            if (ex != null) throw ex;
        }

        #region Delegate DataView
        public IEnumerable detailsView()
        {
            TransferFilter transferFilter = MasterView.Current as TransferFilter;
            var currentSearchStartDate = transferFilter?.StartDate;
            var currentSearchEndDate = transferFilter?.EndDate;

            if (currentSearchStartDate == null)
                return SelectFrom<INRegister>.Where<INRegister.tranDate.IsLessEqual<@P.AsDateTime>.And<INRegister.docType.IsEqual<@P.AsString>>>.View.Select(this, ((DateTime)currentSearchEndDate).ToString("yyyy-MM-dd"), "T");
            else
                return SelectFrom<INRegister>.Where<INRegister.tranDate.IsGreaterEqual<@P.AsDateTime>.And<INRegister.tranDate.IsLessEqual<@P.AsDateTime>.And<INRegister.docType.IsEqual<@P.AsString>>>>.View.Select(this, ((DateTime)currentSearchStartDate).ToString("yyyy-MM-dd"), ((DateTime)currentSearchEndDate).ToString("yyyy-MM-dd"), "T");
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
        }
        #endregion
    }
}