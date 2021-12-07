using HSNHighcareCistomizations.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS
{
    public class ServiceOrderEntryExt : PXGraphExtension<ServiceOrderEntry>
    {

        public SelectFrom<v_HighcareServiceHistory>
               .Where<v_HighcareServiceHistory.soRefNbr.IsNotEqual<FSServiceOrder.refNbr.FromCurrent>>
               .View HighcareSrvHistory;


        #region Override Method
        public override void Initialize()
        {
            base.Initialize();
            this.HighcareSrvHistory.AllowDelete = this.HighcareSrvHistory.AllowInsert = this.HighcareSrvHistory.AllowUpdate = false;
        }
        #endregion

        #region Event

        public virtual void _(Events.FieldUpdated<FSSODet.SMequipmentID> e, PXFieldUpdated baseMethod)
        {
            baseMethod?.Invoke(e.Cache, e.Args);

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
                var currentPINCode = FSEquipment.PK.Find(Base, (int)e.NewValue)?.TagNbr;
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
                                .View.Select(Base, pinCodeInfo.CPriceClassID, itemClassInfo.ItemClassID, row.InventoryID)
                                .RowCast<LUMServiceScope>().FirstOrDefault();
                if (scopeInfo == null)
                    return;
                var usedServiceCount = this.HighcareSrvHistory.Select()
                                       .RowCast<v_HighcareServiceHistory>()
                                       .Where(x => x.ItemClassID == scopeInfo.ItemClassID || x.InventoryID == scopeInfo.InventoryID)
                                       .Count();
                // 不限次數，直接給折扣
                if (scopeInfo.ScopeType == "Discount" && scopeInfo.LimitedCount == 0)
                    Base.ServiceOrderDetails.Cache.SetValueExt<FSSODet.discPct>(row, (scopeInfo?.DiscountPrecent ?? 0));
                // 限制次數，給予折扣
                else if (scopeInfo.ScopeType == "Discount" && scopeInfo.LimitedCount - usedServiceCount > 0)
                    Base.ServiceOrderDetails.Cache.SetValueExt<FSSODet.discPct>(row, (scopeInfo?.DiscountPrecent ?? 0));
                // 限制次數，跳出警示
                else if (scopeInfo.ScopeType == "Count" && scopeInfo.LimitedCount - usedServiceCount <= 0)
                    e.Cache.RaiseExceptionHandling<FSSODet.SMequipmentID>(
                        row,
                        e.NewValue,
                        new PXSetPropertyException<FSSODet.SMequipmentID>("Limited count for this service has been reached", PXErrorLevel.RowWarning));
            }
        }

        #endregion
    }
}
