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
            var doc = Base.ServiceOrderRecords.Current;
            if (e.Row is FSSODet row && row != null && row.SMEquipmentID.HasValue && doc != null)
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
                if (scopeInfo.ScopeType == "Discount")
                    Base.ServiceOrderDetails.Cache.SetValueExt<FSSODet.discPct>(row, (scopeInfo?.DiscountPrecent ?? 0));
                else
                {
                    var usedServiceCount = this.HighcareSrvHistory.Select()
                                          .RowCast<v_HighcareServiceHistory>()
                                          .Where(x => x.ItemClassID == scopeInfo.ItemClassID || x.InventoryID == scopeInfo.InventoryID)
                                          .Count();
                    if (scopeInfo?.LimitedCount - usedServiceCount > 0)
                        Base.ServiceOrderDetails.Cache.SetValueExt<FSSODet.curyBillableTranAmt>(row, (decimal)0);
                }
            }
        }

        #endregion
    }
}
