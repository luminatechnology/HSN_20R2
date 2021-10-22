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
using PX.Objects.CS;
using PX.Objects.FS;
using PX.Objects.IN;

namespace HSNCustomizations.Graph
{
    public class PrintTransferDeliveryOrder : PXGraph<PrintTransferDeliveryOrder>
    {

        //public PXSave<TransferFilter> Save;
        public PXCancel<TransferFilter> Cancel;
        public PXFilteredProcessing<INRegister, TransferFilter> TransferRecords;
        
        public PXFilter<TransferFilter> MasterView;
        [PXFilterable]
        public SelectFrom<INRegister>.OrderBy<Desc<INRegister.refNbr>>.View DetailsView;
        public SelectFrom<LumINTran>.View LumINTranView;

        #region Transfer Report Type
        Dictionary<string, string> dicTransferReportType = new Dictionary<string, string>()
        {
            { "DeliveryOrder", "LM644010" },
            { "ReDeliveryOrder", "RE644010" }
        };
        #endregion

        public PrintTransferDeliveryOrder()
        {
            PXUIFieldAttribute.SetEnabled<INRegisterExt.usrTrackingNbr>(DetailsView.Cache, null, true);
            //TransferRecords.SetProcessVisible(false);
            TransferRecords.SetProcessAllVisible(false);
            TransferRecords.SetProcessDelegate(list => PrintTransfers(list));
        }

        public void PrintTransfers(IEnumerable<INRegister> list)
        {
            PrintTransferDeliveryOrder printTransferDeliveryOrder = PXGraph.CreateInstance<PrintTransferDeliveryOrder>();
            TransferFilter transferFilter = MasterView.Current as TransferFilter;
            PXCache cache = this.Caches[typeof(LumINTran)];

            if (transferFilter.ReportType == dicTransferReportType["DeliveryOrder"])
            {
                //check - cannot be checked items included unprinted and printed
                int _countPrinted, _countUnprinted, _countPickingNbr;
                //if (transferFilter.ReportType == dicTransferReportType["PickingList"])
                //{
                //    _countPrinted = list.Where(x => x.GetExtension<INRegisterExt>().UsrPickingListNumber != null).Count();
                //    _countUnprinted = list.Where(x => x.GetExtension<INRegisterExt>().UsrPickingListNumber == null).Count();
                //}
                //else
                //{
                    var TrackingNbrTop = list.Where(x => x.GetExtension<INRegisterExt>().UsrPickingListNumber != null).FirstOrDefault();
                    if (TrackingNbrTop != null) _countPickingNbr = list.Where(x => x.GetExtension<INRegisterExt>().UsrPickingListNumber != TrackingNbrTop.GetExtension<INRegisterExt>().UsrPickingListNumber).Count();
                    else _countPickingNbr = 0;
                    if (_countPickingNbr > 0) throw new PXException("Cannot print the Delivery Order which including two or more Picking Number.");

                    _countPrinted = list.Where(x => x.GetExtension<INRegisterExt>().UsrDeliveryOrderNumber != null).Count();
                    _countUnprinted = list.Where(x => x.GetExtension<INRegisterExt>().UsrDeliveryOrderNumber == null).Count();
                //}
                if (_countPrinted > 0 && _countUnprinted > 0) throw new PXException("Cannot print the report which including both printed and unprinted transactions.");

                this.Actions.PressSave();

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

                Dictionary<string, string> dicNumberingSequence = new Dictionary<string, string>();

                foreach (var transfer in list)
                {
                    var result = SelectFrom<INTran>
                                    .LeftJoin<INRegister>.On<INRegister.docType.IsEqual<INTran.docType>.And<INRegister.refNbr.IsEqual<INTran.refNbr>>>
                                    .Where<INTran.docType.IsEqual<@P.AsString>.And<INTran.refNbr.IsEqual<@P.AsString>>>
                                    .View.Select(this, transfer.DocType, transfer.RefNbr);

                    string _trackingNbr;
                    //if (transferFilter.ReportType == dicTransferReportType["PickingList"]) _trackingNbr = null;
                    //else
                    //{
                    var currentTrackingNbr = list.Where(x => x.GetExtension<INRegisterExt>().UsrTrackingNbr != null).FirstOrDefault();
                        if (currentTrackingNbr != null) _trackingNbr = list.Where(x => x.GetExtension<INRegisterExt>().UsrTrackingNbr != null).FirstOrDefault().GetExtension<INRegisterExt>().UsrTrackingNbr;
                        else throw new PXException("Please enter a Tracking Number.");
                    //}

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
                        lumINTran.ToLocationID = iNTranLine.ToLocationID;
                        lumINTran.Qty = iNTranLine.Qty;
                        lumINTran.TranDesc = iNTranLine.TranDesc;
                        lumINTran.Uom = iNTranLine.UOM;
                        lumINTran.Tositeid = iNTranLine.ToSiteID;
                        lumINTran.UsrAppointmentNbr = iNRegisterLine.GetExtension<INRegisterExt>().UsrAppointmentNbr;
                        lumINTran.UsrTrackingNbr = _trackingNbr;
                        this.Caches[typeof(LumINTran)].Update(lumINTran);
                    }

                    //string _numberingID = transferFilter.ReportType == dicTransferReportType["PickingList"] ? SelectFrom<LUMHSNSetup>.View.Select(this).TopFirst.PickingListNumberingID : SelectFrom<LUMHSNSetup>.View.Select(this).TopFirst.DeliveryOrderNumberingID;
                    string _numberingID = SelectFrom<LUMHSNSetup>.View.Select(this).TopFirst.DeliveryOrderNumberingID;
                    if (_numberingID == null) throw new PXException("Please select a Numbering Sequence in HSN Preferences page.");

                    string _numberingSequence = "";
                    if (dicNumberingSequence.ContainsKey($"{transfer.SiteID}-{transfer.ToSiteID}"))
                    {
                        _numberingSequence = dicNumberingSequence[$"{transfer.SiteID}-{transfer.ToSiteID}"];
                    }
                    else
                    {
                        _numberingSequence = AutoNumberAttribute.GetNextNumber(cache, transfer, _numberingID, DateTime.Now);
                        dicNumberingSequence.Add($"{transfer.SiteID}-{transfer.ToSiteID}", _numberingSequence);
                    }

                    //insert numbering sequence
                    /*
                    if (transferFilter.ReportType == dicTransferReportType["PickingList"])
                    {
                        transfer.GetExtension<INRegisterExt>().UsrPLIsPrinted = true;
                        if (transfer.GetExtension<INRegisterExt>().UsrPickingListNumber == null) transfer.GetExtension<INRegisterExt>().UsrPickingListNumber = _numberingSequence;
                    }
                    */
                    if (transferFilter.ReportType == dicTransferReportType["DeliveryOrder"])
                    {
                        transfer.GetExtension<INRegisterExt>().UsrDOIsPrinted = true;
                        if (transfer.GetExtension<INRegisterExt>().UsrDeliveryOrderNumber == null) transfer.GetExtension<INRegisterExt>().UsrDeliveryOrderNumber = _numberingSequence;
                        if (transfer.GetExtension<INRegisterExt>().UsrTrackingNbr == null) transfer.GetExtension<INRegisterExt>().UsrTrackingNbr = _trackingNbr;
                    }

                    this.Caches[typeof(INRegister)].Update(transfer);

                    this.Actions.PressSave();
                }
            }
            else
            {
                //Checking
                bool _coutinue;
                string _errorMsg;
                //if (transferFilter.ReportType == dicTransferReportType["RePickingList"])
                //{
                //    _coutinue = list.Where(x => x.GetExtension<INRegisterExt>().UsrPickingListNumber == null).Count() > 0 ? false : true;
                //    _errorMsg = "Picking Number cannot be empty.";
                //}
                //else
                //{
                    _coutinue = list.Where(x => x.GetExtension<INRegisterExt>().UsrDeliveryOrderNumber == null).Count() > 0 ? false : true;
                    _errorMsg = "Delivery Order Number cannot be empty.";
                //}
                if (!_coutinue) throw new PXException(_errorMsg);

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

                PrintTransferPickingList graphPrintTransferPickingList = PXGraph.CreateInstance<PrintTransferPickingList>();
                Dictionary<string, bool> dicInsertedDeliveryOrder = new Dictionary<string, bool>();

                foreach (var transfer in list)
                {
                    PXResultset<INTran> rows = new PXResultset<INTran>();
                    //if (transferFilter.ReportType == dicTransferReportType["RePickingList"])
                    //{
                    //    rows = SelectFrom<INTran>
                    //           .LeftJoin<INRegister>.On<INRegister.docType.IsEqual<INTran.docType>.And<INRegister.refNbr.IsEqual<INTran.refNbr>>>
                    //           .Where<INRegisterExt.usrPickingListNumber.IsEqual<@P.AsString>>
                    //           .View.Select(this, transfer.GetExtension<INRegisterExt>().UsrPickingListNumber);
                    //}
                    //else
                    //{
                        rows = SelectFrom<INTran>
                               .LeftJoin<INRegister>.On<INRegister.docType.IsEqual<INTran.docType>.And<INRegister.refNbr.IsEqual<INTran.refNbr>>>
                               .Where<INRegisterExt.usrDeliveryOrderNumber.IsEqual<@P.AsString>>
                               .View.Select(this, transfer.GetExtension<INRegisterExt>().UsrDeliveryOrderNumber);
                    //}
                       

                    foreach (PXResult<INTran, INRegister> row in rows)
                    {
                        LumINTran lumINTran = new LumINTran();

                        INTran iNTranLine = row;
                        INRegister iNRegisterLine = row;

                        if (!dicInsertedDeliveryOrder.ContainsKey($"{iNTranLine.RefNbr}-{iNTranLine.LineNbr}"))
                        {

                            //avoid updating LastModifiedDateTime
                            graphPrintTransferPickingList.ProviderInsert<LumINTran>(
                            new PXDataFieldAssign("DocType", iNTranLine.DocType),
                            new PXDataFieldAssign("RefNbr", iNTranLine.RefNbr),
                            new PXDataFieldAssign("LineNbr", iNTranLine.LineNbr),
                            new PXDataFieldAssign("TranDate", iNTranLine.TranDate),
                            new PXDataFieldAssign("TranType", iNTranLine.TranType),
                            new PXDataFieldAssign("InventoryID", iNTranLine.InventoryID),
                            new PXDataFieldAssign("Siteid", iNTranLine.SiteID),
                            new PXDataFieldAssign("InvtMult", iNTranLine.InvtMult),
                            new PXDataFieldAssign("LocationID", iNTranLine.LocationID),
                            new PXDataFieldAssign("ToLocationID", iNTranLine.ToLocationID),
                            new PXDataFieldAssign("Qty", iNTranLine.Qty),
                            new PXDataFieldAssign("TranDesc", iNTranLine.TranDesc),
                            new PXDataFieldAssign("Uom", iNTranLine.UOM),
                            new PXDataFieldAssign("Tositeid", iNTranLine.ToSiteID),
                            new PXDataFieldAssign("UsrAppointmentNbr", iNRegisterLine.GetExtension<INRegisterExt>().UsrPickingListNumber),
                            new PXDataFieldAssign("UsrTrackingNbr", iNRegisterLine.GetExtension<INRegisterExt>().UsrTrackingNbr),
                            new PXDataFieldAssign("LastModifiedByID", PXAccess.GetUserID()),
                            new PXDataFieldAssign("LastModifiedByScreenID", "LM502010"),
                            new PXDataFieldAssign("LastModifiedDateTime", DateTime.Now)
                            );
                            dicInsertedDeliveryOrder.Add($"{iNTranLine.RefNbr}-{iNTranLine.LineNbr}", true);
                        }
                    }
                }
            }
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            throw new PXReportRequiredException(parameters, transferFilter.ReportType.Replace("RE", "LM"), $"Report {transferFilter.ReportType.Replace("RE", "LM")}");
        }

        #region Delegate DataView
        public IEnumerable detailsView()
        {
            TransferFilter transferFilter = MasterView.Current as TransferFilter;
            var currentSearchStartDate = transferFilter?.StartDate;
            var currentSearchEndDate = transferFilter?.EndDate;
            var currentFromWarehouse = transferFilter?.SiteID;

            // Default: DocType = T, Released = 1
            if (currentFromWarehouse == null)
            {
                if (currentSearchStartDate == null)
                    return SelectFrom<INRegister>
                            .Where<INRegister.tranDate.IsLessEqual<@P.AsDateTime>.And<INRegister.docType.IsEqual<@P.AsString>.And<INRegister.released.IsEqual<True>>>>
                            .View.Select(this, currentSearchEndDate, "T");
                else if (currentSearchStartDate != null && currentSearchEndDate != null)
                    return SelectFrom<INRegister>
                            .Where<INRegister.tranDate.IsGreaterEqual<@P.AsDateTime>.And<INRegister.tranDate.IsLessEqual<@P.AsDateTime>.And<INRegister.docType.IsEqual<@P.AsString>.And<INRegister.released.IsEqual<True>>>>>
                            .View.Select(this, currentSearchStartDate, currentSearchEndDate, "T");
                else
                    return SelectFrom<INRegister>.Where<INRegister.docType.IsEqual<@P.AsString>.And<INRegister.released.IsEqual<True>>>
                            .View.Select(this, "T");
            }
            else
            {
                if (currentSearchStartDate == null)
                    return SelectFrom<INRegister>
                            .Where<INRegister.tranDate.IsLessEqual<@P.AsDateTime>.And<INRegister.docType.IsEqual<@P.AsString>>.And<INRegister.siteID.IsEqual<@P.AsInt>.And<INRegister.released.IsEqual<True>>>>
                            .View.Select(this, currentSearchEndDate, "T", currentFromWarehouse);
                else if (currentSearchStartDate != null && currentSearchEndDate != null)
                    return SelectFrom<INRegister>
                            .Where<INRegister.tranDate.IsGreaterEqual<@P.AsDateTime>.And<INRegister.tranDate.IsLessEqual<@P.AsDateTime>.And<INRegister.docType.IsEqual<@P.AsString>>>.And<INRegister.siteID.IsEqual<@P.AsInt>.And<INRegister.released.IsEqual<True>>>>
                            .View.Select(this, currentSearchStartDate, currentSearchEndDate, "T", currentFromWarehouse);
                else
                    return SelectFrom<INRegister>.Where<INRegister.docType.IsEqual<@P.AsString>.And<INRegister.siteID.IsEqual<@P.AsInt>.And<INRegister.released.IsEqual<True>>>>
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
                    new string[] { "LM644010", "RE644010" },
                    new string[] { "Print Delivery Order", "Reprint Delivery Order" })]
            [PXDefault("LM644010")]
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