using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.IN;

namespace PX.Objects.FS
{
    public class ServiceOrderEntry_Extension : PXGraphExtension<ServiceOrderEntry>
    {

        #region Selects
        public SelectFrom<INRegister>.Where<INRegister.docType.IsIn<INDocType.transfer, INDocType.receipt>
                                            .And<INRegisterExt.usrSrvOrdType.IsEqual<FSServiceOrder.srvOrdType.FromCurrent>
                                                 .And<INRegisterExt.usrSORefNbr.IsEqual<FSServiceOrder.refNbr.FromCurrent>>>>.View INRegisterView;
        public SelectFrom<LUMSrvEventHistory>.Where<LUMSrvEventHistory.srvOrdType.IsEqual<FSServiceOrder.srvOrdType.FromCurrent>
                                                    .And<LUMSrvEventHistory.sORefNbr.IsEqual<FSServiceOrder.refNbr.FromCurrent>>>.View EventHistory;
        #endregion

        #region Delegate Method
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            var isNewData = Base.ServiceOrderRecords.Cache.Inserted.RowCast<FSServiceOrder>().Count() > 0;
            var oldStatus = SelectFrom<FSServiceOrder>
                               .Where<FSServiceOrder.srvOrdType.IsEqual<P.AsString>
                                   .And<FSServiceOrder.refNbr.IsEqual<P.AsString>>>
                                .View.Select(new PXGraph(), Base.ServiceOrderRecords.Current.SrvOrdType, Base.ServiceOrderRecords.Current.RefNbr)
                                .RowCast<FSServiceOrder>()?.FirstOrDefault()?.Status;
            var nowStatus = Base.ServiceOrderRecords.Current.Status;
            baseMethod();
            try
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    FSWorkflowStageHandler.srvEntry = Base;
                    FSWorkflowStageHandler.InitStageList();

                    if (oldStatus != nowStatus && oldStatus != null)
                        FSWorkflowStageHandler.InsertEventHistoryForStatus(nameof(ServiceOrderEntry), oldStatus, nowStatus);

                    LUMAutoWorkflowStage autoWFStage = isNewData ?
                        LUMAutoWorkflowStage.PK.Find(Base, Base.ServiceOrderRecords.Current.SrvOrdType, nameof(WFRule.OPEN01)) :
                        FSWorkflowStageHandler.AutoWFStageRule(nameof(ServiceOrderEntry));
                    if (autoWFStage != null && autoWFStage.Active == true)
                        FSWorkflowStageHandler.UpdateWFStageID(nameof(ServiceOrderEntry), autoWFStage);

                    baseMethod();
                    ts.Complete();
                }
            }
            catch (PXException)
            {
                throw;
            }
        }
        #endregion

    }
}