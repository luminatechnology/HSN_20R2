using HSNCustomizations.DAC;
using HSNHighcareCistomizations.DAC;
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
               .Where<v_HighcareServiceHistory.soRefNbr.IsNotEqual<FSServiceOrder.refNbr.FromCurrent>>
               .View HighcareSrvHistory;

        public SelectFrom<LUMServiceScope>
               .InnerJoin<PX.Objects.CR.Location>.On<LUMServiceScope.cPriceClassID.IsEqual<PX.Objects.CR.Location.cPriceClassID>>
               .InnerJoin<Customer>.On<PX.Objects.CR.Location.locationID.IsEqual<Customer.defLocationID>>
               .Where<Customer.bAccountID.IsEqual<FSServiceOrder.customerID.FromCurrent>>
               .View SrvScope;

        [PXHidden]
        public SelectFrom<LUMHSNSetup>.View hsnSetup;

        #region Override Method
        public override void Initialize()
        {
            base.Initialize();

            var hsnSetup = SelectFrom<LUMHSNSetup>.View.Select(Base).RowCast<LUMHSNSetup>().FirstOrDefault();

            this.HighcareSrvHistory.AllowDelete = this.HighcareSrvHistory.AllowInsert = this.HighcareSrvHistory.AllowUpdate = false;
            this.HighcareSrvHistory.AllowSelect = hsnSetup?.EnableHighcareFunction ?? false;

            this.SrvScope.AllowDelete = this.SrvScope.AllowInsert = this.SrvScope.AllowUpdate = false;
            this.SrvScope.AllowSelect = hsnSetup?.EnableHighcareFunction ?? false;
        }
        #endregion

        #region Event

        public virtual void _(Events.FieldUpdated<FSSODet.SMequipmentID> e, PXFieldUpdated baseMethod)
        {
            baseMethod?.Invoke(e.Cache, e.Args);
            if (this.hsnSetup.Current?.EnableHighcareFunction ?? false)
                GetHighcareDiscount(e);
        }

        #endregion

        #region Method

        public void GetHighcareDiscount(Events.FieldUpdated<FSSODet.SMequipmentID> e)
        {
            var doc = Base.ServiceOrderRecords.Current; if (e.Row is FSSODet row && row != null && row.SMEquipmentID.HasValue && doc != null)
            {
                var itemClassInfo = SelectFrom<INItemClass>
                                       .InnerJoin<InventoryItem>.On<INItemClass.itemClassID.IsEqual<InventoryItem.itemClassID>>
                                       .Where<InventoryItem.inventoryID.IsEqual<P.AsInt>>
                                       .View.Select(Base, row.InventoryID).RowCast<INItemClass>().FirstOrDefault();
                var customerInfo = Customer.PK.Find(Base, doc.CustomerID);
                if (customerInfo.ClassID != "HIGHCARE")
                    return;
                var currentPINCode = FSEquipment.PK.Find(Base, (int)e.NewValue).GetExtension<FSEquipmentExtension>()?.UsrPINCode;
                if (string.IsNullOrEmpty(currentPINCode))
                    return;
                var pinCodeInfo = SelectFrom<LumCustomerPINCode>
                                  .Where<LumCustomerPINCode.pin.IsEqual<P.AsString>
                                    .And<LumCustomerPINCode.bAccountID.IsEqual<P.AsInt>>>
                                  .View.Select(Base, currentPINCode, customerInfo.BAccountID)
                                  .RowCast<LumCustomerPINCode>().ToList()
                                  .Where(x => DateTime.Now.Date >= x.StartDate?.Date && DateTime.Now.Date <= x.EndDate?.Date).FirstOrDefault();
                if (pinCodeInfo == null)
                    return;
                var scopeInfo = SelectFrom<LUMServiceScope>
                                .Where<LUMServiceScope.cPriceClassID.IsEqual<P.AsString>
                                  .And<LUMServiceScope.itemClassID.IsEqual<P.AsInt>.Or<LUMServiceScope.inventoryID.IsEqual<P.AsInt>>>>
                                .View.Select(Base, pinCodeInfo.CPriceClassID, itemClassInfo?.ItemClassID, row.InventoryID)
                                .RowCast<LUMServiceScope>().FirstOrDefault();
                if (scopeInfo == null)
                    return;
                var usedServiceCount = this.HighcareSrvHistory.Select()
                                       .RowCast<v_HighcareServiceHistory>()
                                       .Where(x => x.ItemClassID == scopeInfo.ItemClassID || x.InventoryID == scopeInfo.InventoryID)
                                       .Count();
                // 不限次數，直接給折扣
                if (scopeInfo.LimitedCount == 0)
                    Base.ServiceOrderDetails.Cache.SetValueExt<FSSODet.discPct>(row, (scopeInfo?.DiscountPrecent ?? 0));
                // 限制次數，給予折扣
                else if (scopeInfo.LimitedCount - usedServiceCount > 0)
                    Base.ServiceOrderDetails.Cache.SetValueExt<FSSODet.discPct>(row, (scopeInfo?.DiscountPrecent ?? 0));
                // 次數不夠，跳出警示
                else
                    e.Cache.RaiseExceptionHandling<FSSODet.SMequipmentID>(
                        row,
                        e.NewValue,
                        new PXSetPropertyException<FSSODet.SMequipmentID>("Limited count for this service has been reached", PXErrorLevel.RowWarning));
            }
        }

        #endregion
    }
}
