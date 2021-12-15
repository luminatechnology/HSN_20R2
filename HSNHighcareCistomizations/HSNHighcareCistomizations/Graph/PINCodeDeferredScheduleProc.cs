using HSNHighcareCistomizations.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.IN;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.CR;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace HSNHighcareCistomizations.Graph
{
    public class PINCodeDeferredScheduleProc : PXGraph<PINCodeDeferredScheduleProc>
    {

        [InjectDependency]
        public IFinPeriodRepository FinPeriodRepository { get; set; }

        public PXCancel<LumCustomerPINCode> Cancel;
        public PXProcessingJoin<LumCustomerPINCode,
                                InnerJoin<Customer, On<LumCustomerPINCode.bAccountID, Equal<Customer.bAccountID>>>,
                                Where<LumCustomerPINCode.scheduleNbr.IsNull>> ProcessList;

        public PINCodeDeferredScheduleProc()
            => ProcessList.SetProcessDelegate(GoProcess);

        #region Static Method

        public static void GoProcess(List<LumCustomerPINCode> list)
            => PXGraph.CreateInstance<PINCodeDeferredScheduleProc>().CreateDeferralSchedule(list);
        #endregion

        #region Method
        public virtual void CreateDeferralSchedule(List<LumCustomerPINCode> list)
        {
            if (list.Count <= 0)
                return;
            PXLongOperation.StartOperation(this, delegate ()
            {
                foreach (var item in list)
                {
                    try
                    {
                        FinPeriod finPeriod = FinPeriodRepository.GetFinPeriodByDate(DateTime.Now, PXAccess.GetParentOrganizationID(PXAccess.GetBranchID()));
                        var scope = SelectFrom<LumServiceScopeHeader>
                                    .Where<LumServiceScopeHeader.cPriceClassID.IsEqual<P.AsString>>
                                    .View.Select(this, item.CPriceClassID).RowCast<LumServiceScopeHeader>().FirstOrDefault();
                        var acctInfo = BAccount2.PK.Find(this, item.BAccountID.Value);
                        var itemInfo = InventoryItem.PK.Find(this, scope.InventoryID);
                        if (scope == null)
                            throw new PXException("please maintain Service Scope");
                        // Create Draft Schedule Graph
                        var graph = PXGraph.CreateInstance<DraftScheduleMaint>();
                        // Create document
                        var draftDoc = graph.Schedule.Insert((DRSchedule)graph.Schedule.Cache.CreateInstance());
                        graph.Schedule.Cache.SetValue<DRSchedule.bAccountID>(draftDoc, item.BAccountID);
                        graph.Schedule.Cache.SetValueExt<DRSchedule.bAccountLocID>(draftDoc, acctInfo?.DefLocationID);
                        graph.Schedule.Cache.SetValueExt<DRSchedule.termStartDate>(draftDoc, item.StartDate);
                        graph.Schedule.Cache.SetValueExt<DRSchedule.termEndDate>(draftDoc, item.EndDate);
                        // Create Components
                        var component = graph.Components.Insert((DRScheduleDetail)graph.Components.Cache.CreateInstance());
                        graph.Components.Cache.SetValueExt<DRScheduleDetail.accountID>(component, itemInfo.SalesAcctID);
                        graph.Components.Cache.SetValueExt<DRScheduleDetail.subID>(component, itemInfo.SalesSubID);
                        graph.Components.Cache.SetValueExt<DRScheduleDetail.componentID>(component, scope.InventoryID);
                        graph.Components.Cache.SetValueExt<DRScheduleDetail.defCode>(component, scope.DefCode);
                        graph.Components.Cache.SetValueExt<DRScheduleDetail.totalAmt>(component, scope.TotalAmt);
                        graph.Components.Cache.SetValueExt<DRScheduleDetail.branchID>(component, PXAccess.GetBranchID());
                        component.FinPeriodID = finPeriod.FinPeriodID;
                        // Generate Transactions
                        graph.generateTransactions.Press();
                        graph.Save.Press();
                        graph.release.Press();
                        // setting CustomerPIN Code DeferralSchedule
                        PXUpdate<Set<LumCustomerPINCode.scheduleNbr, Required<LumCustomerPINCode.scheduleNbr>>,
                                LumCustomerPINCode,
                                Where<LumCustomerPINCode.bAccountID, Equal<P.AsInt>,
                                  And<LumCustomerPINCode.pin, Equal<P.AsString>,
                                  And<LumCustomerPINCode.cPriceClassID, Equal<P.AsString>>>>>.Update(this, graph.Schedule.Current.ScheduleNbr, item.BAccountID, item.Pin, item.CPriceClassID);
                        this.Actions.PressSave();
                    }
                    catch (Exception ex)
                    {
                        PXProcessing.SetError<LumCustomerPINCode>(ex.Message);
                    }
                }
            });
        }
        #endregion
    }
}
