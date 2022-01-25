using HSNCustomizations.DAC;
using HSNHighcareCistomizations.DAC;
using HSNHighcareCistomizations.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.EP;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PX.Objects.FS.FSSODet;

namespace PX.Objects.FS
{
    public class ServiceOrderEntryExt : PXGraphExtension<ServiceOrderEntry>
    {

        public SelectFrom<v_HighcareServiceHistory>
               .Where<v_HighcareServiceHistory.soRefNbr.IsNotEqual<FSServiceOrder.refNbr.FromCurrent>
                 .And<v_HighcareServiceHistory.customerID.IsEqual<FSServiceOrder.customerID.FromCurrent>>>
               .View HighcareSrvHistory;

        public SelectFrom<LUMServiceScope>
               .InnerJoin<PX.Objects.CR.Location>.On<LUMServiceScope.cPriceClassID.IsEqual<PX.Objects.CR.Location.cPriceClassID>>
               .InnerJoin<Customer>.On<PX.Objects.CR.Location.locationID.IsEqual<Customer.defLocationID>>
               .Where<Customer.bAccountID.IsEqual<FSServiceOrder.customerID.FromCurrent>>
               .View SrvScope;

        [PXHidden]
        public SelectFrom<LUMHSNSetup>.View hsnSetup;

        public static bool IsActive()
        {
            return (SelectFrom<LUMHSNSetup>.View.Select(new PXGraph()).RowCast<LUMHSNSetup>().FirstOrDefault()?.GetExtension<LUMHSNSetupExtension>().EnableHighcareFunction ?? false);
        }

        #region Override Method
        public override void Initialize()
        {
            base.Initialize();

            var hsnSetup = SelectFrom<LUMHSNSetup>.View.Select(Base).RowCast<LUMHSNSetup>().FirstOrDefault();

            this.HighcareSrvHistory.AllowDelete = this.HighcareSrvHistory.AllowInsert = this.HighcareSrvHistory.AllowUpdate = false;
            this.HighcareSrvHistory.AllowSelect = hsnSetup.GetExtension<LUMHSNSetupExtension>()?.EnableHighcareFunction ?? false;


            this.SrvScope.AllowDelete = this.SrvScope.AllowInsert = this.SrvScope.AllowUpdate = false;
            this.SrvScope.AllowSelect = hsnSetup.GetExtension<LUMHSNSetupExtension>()?.EnableHighcareFunction ?? false;
        }
        #endregion

        #region Delegate Method

        /// <summary> Delegate Schedule Appointment </summary>
        public delegate IEnumerable ScheduleAppointmentDelegate(PXAdapter adapter);
        [PXOverride]
        public IEnumerable ScheduleAppointment(PXAdapter adapter, ScheduleAppointmentDelegate baseMethod)
        {
            List<FSServiceOrder> list = adapter.Get<FSServiceOrder>().ToList();
            try
            {
                baseMethod.Invoke(adapter);
            }
            catch (PXRedirectRequiredException ex)
            {
                var graph = (AppointmentEntry)ex.Graph;
                var appDet = graph.AppointmentDetails.Select().RowCast<FSAppointmentDet>().ToList();
                appDet.ForEach(x => { x.ManualDisc = x.DiscAmt.HasValue && x.DiscAmt != 0; });
                throw new PXRedirectRequiredException(ex.Graph, null);
            }
            return list;
        }

        #endregion

        #region Event

        public virtual void _(Events.FieldUpdated<FSSODet.SMequipmentID> e, PXFieldUpdated baseMethod)
        {
            baseMethod?.Invoke(e.Cache, e.Args);
            if (this.hsnSetup.Current.GetExtension<LUMHSNSetupExtension>()?.EnableHighcareFunction ?? false)
                GetHighcareDiscount(e);
        }

        #endregion

        #region Method

        public void GetHighcareDiscount(Events.FieldUpdated<FSSODet.SMequipmentID> e)
        {
            var doc = Base.ServiceOrderRecords.Current;

            if (e.Row is FSSODet row && row != null && row.SMEquipmentID.HasValue && doc != null)
            {
                HighcareHelper helper = new HighcareHelper();
                var itemInfo = InventoryItem.PK.Find(Base, row.InventoryID);
                var customerInfo = Customer.PK.Find(Base, doc.CustomerID);
                if (customerInfo.ClassID != "HIGHCARE")
                    return;
                var _equipment = helper.GetEquipmentInfo((int)e.NewValue);
                // 裝置有綁定 未啟用 不計算
                if(_equipment.Status != EPEquipmentStatus.Active)
                    return;
                var currentPINCode = _equipment.GetExtension<FSEquipmentExtension>().UsrPINCode;
                if (string.IsNullOrEmpty(currentPINCode))
                    return;
                var pinCodeInfo = SelectFrom<LUMCustomerPINCode>
                                  .Where<LUMCustomerPINCode.pin.IsEqual<P.AsString>
                                    .And<LUMCustomerPINCode.bAccountID.IsEqual<P.AsInt>>>
                                  .View.Select(Base, currentPINCode, customerInfo.BAccountID)
                                  .RowCast<LUMCustomerPINCode>().ToList()
                                  .Where(x => Base.Accessinfo.BusinessDate?.Date >= x.StartDate?.Date && Base.Accessinfo.BusinessDate?.Date <= x.EndDate?.Date && (x.IsActive ?? false)).FirstOrDefault();
                if (pinCodeInfo == null)
                    return;
                var servicescopeInfo = SelectFrom<LUMServiceScope>
                                       .Where<LUMServiceScope.cPriceClassID.IsEqual<P.AsString>
                                         .And<LUMServiceScope.priceClassID.IsEqual<P.AsString>.Or<LUMServiceScope.inventoryID.IsEqual<P.AsInt>>>>
                                       .View.Select(Base, pinCodeInfo.CPriceClassID, itemInfo?.PriceClassID, row.InventoryID)
                                       .RowCast<LUMServiceScope>().FirstOrDefault();
                if (servicescopeInfo == null)
                    return;
                // Service History
                var usedServiceCountHist = !string.IsNullOrEmpty(servicescopeInfo?.PriceClassID) ?
                                        this.HighcareSrvHistory.Select()
                                            .RowCast<v_HighcareServiceHistory>()
                                            .Where(x => x?.PriceClassID == servicescopeInfo?.PriceClassID && x.Pincode == currentPINCode).Count() :
                                        this.HighcareSrvHistory.Select()
                                            .RowCast<v_HighcareServiceHistory>()
                                            .Where(x => x?.InventoryID == servicescopeInfo?.InventoryID && x.Pincode == currentPINCode).Count();
                // Detail Cache
                var usedServiceCountCache = Base.ServiceOrderDetails
                                            .Select().RowCast<FSSODet>()
                                            .Where(x => x != row && x?.InventoryID == row?.InventoryID && 
                                                        helper.GetEquipmentInfo(x?.SMEquipmentID).GetExtension<FSEquipmentExtension>()?.UsrPINCode == currentPINCode &&
                                                        helper.GetEquipmentInfo(x?.SMEquipmentID).Status == EPEquipmentStatus.Active).Count();
                // 不限次數，直接給折扣
                if (servicescopeInfo.LimitedCount == 0)
                    Base.ServiceOrderDetails.Cache.SetValueExt<FSSODet.discPct>(row, (servicescopeInfo?.DiscountPrecent ?? 0));
                // 限制次數，給予折扣
                else if (servicescopeInfo.LimitedCount - usedServiceCountHist - usedServiceCountCache > 0)
                    Base.ServiceOrderDetails.Cache.SetValueExt<FSSODet.discPct>(row, (servicescopeInfo?.DiscountPrecent ?? 0));
                // 次數不夠，跳出警示
                else
                    e.Cache.RaiseExceptionHandling<FSSODet.SMequipmentID>(
                        row,
                        e.NewValue,
                        new PXSetPropertyException<FSSODet.SMequipmentID>("Limited count for this service has been reached", PXErrorLevel.Warning));
            }
            // 移除Equipment時 還原折扣
            else if (e.NewValue == null)
                Base.ServiceOrderDetails.Cache.SetValueExt<FSSODet.discPct>(e.Row, (decimal)0);
        }

        #endregion
    }
}
