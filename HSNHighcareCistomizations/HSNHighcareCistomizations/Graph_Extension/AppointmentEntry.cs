﻿using HSNCustomizations.DAC;
using HSNHighcareCistomizations.DAC;
using HSNHighcareCistomizations.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS
{
    public class AppointmentEntryExt : PXGraphExtension<AppointmentEntry>
    {
        public SelectFrom<v_HighcareServiceHistory>
               .Where<v_HighcareServiceHistory.aptRefNbr.IsNotEqual<FSAppointment.refNbr.FromCurrent>>
               .View HighcareSrvHistory;

        public SelectFrom<LUMServiceScope>
               .InnerJoin<PX.Objects.CR.Location>.On<LUMServiceScope.cPriceClassID.IsEqual<PX.Objects.CR.Location.cPriceClassID>>
               .InnerJoin<Customer>.On<PX.Objects.CR.Location.locationID.IsEqual<Customer.defLocationID>>
               .Where<Customer.bAccountID.IsEqual<FSAppointment.customerID.FromCurrent>>
               .View SrvScope;

        [PXHidden]
        public SelectFrom<LUMHSNSetup>.View hsnSetup;


        #region Override Method
        public override void Initialize()
        {
            base.Initialize();

            var hsnSetup = SelectFrom<LUMHSNSetup>.View.Select(Base).RowCast<LUMHSNSetup>().FirstOrDefault();

            this.HighcareSrvHistory.AllowDelete = this.HighcareSrvHistory.AllowInsert = this.HighcareSrvHistory.AllowUpdate = false;
            this.HighcareSrvHistory.AllowSelect = hsnSetup.GetExtension<LUMHSNSetupExtension>()?.EnableHighcareFunction ?? false;

            this.SrvScope.AllowDelete = SrvScope.AllowInsert = this.SrvScope.AllowUpdate = false;
            this.SrvScope.AllowSelect = hsnSetup.GetExtension<LUMHSNSetupExtension>()?.EnableHighcareFunction ?? false;
        }
        #endregion

        #region Event

        public virtual void _(Events.FieldUpdated<FSAppointmentDet.SMequipmentID> e, PXFieldUpdated baseMethod)
        {
            baseMethod?.Invoke(e.Cache, e.Args);
            if (this.hsnSetup.Current.GetExtension<LUMHSNSetupExtension>()?.EnableHighcareFunction ?? false)
                GetHighcareDiscount(e);
        }

        #endregion

        #region Method

        public void GetHighcareDiscount(Events.FieldUpdated<FSAppointmentDet.SMequipmentID> e)
        {
            var doc = Base.AppointmentRecords.Current;
            if (e.Row is FSAppointmentDet row && row != null && row.SMEquipmentID.HasValue && doc != null)
            {
                HighcareHelper helper = new HighcareHelper();
                var itemClassInfo = helper.GetItemclass(row.InventoryID);
                var customerInfo = Customer.PK.Find(Base, doc.CustomerID);
                if (customerInfo.ClassID != "HIGHCARE")
                    return;
                var currentPINCode = helper.GetEquipmentPINCode((int)e.NewValue);
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
                var servicescopeInfo = SelectFrom<LUMServiceScope>
                                .Where<LUMServiceScope.cPriceClassID.IsEqual<P.AsString>
                                  .And<LUMServiceScope.itemClassID.IsEqual<P.AsInt>.Or<LUMServiceScope.inventoryID.IsEqual<P.AsInt>>>>
                                .View.Select(Base, pinCodeInfo.CPriceClassID, itemClassInfo?.ItemClassID, row.InventoryID)
                                .RowCast<LUMServiceScope>().FirstOrDefault();
                if (servicescopeInfo == null)
                    return;
                // Service History
                var usedServiceCountHist = this.HighcareSrvHistory.Select()
                                           .RowCast<v_HighcareServiceHistory>()
                                           .Where(x => (x?.ItemClassID == servicescopeInfo?.ItemClassID || x?.InventoryID == servicescopeInfo.InventoryID) && x.Pincode == currentPINCode)
                                           .Count();
                // Detail Cache
                var usedServiceCountCache = Base.AppointmentDetails
                                            .Select().RowCast<FSAppointmentDet>()
                                            .Where(x => x != row && x.InventoryID == row.InventoryID && helper.GetEquipmentPINCode(x.SMEquipmentID) == currentPINCode).Count();
                // 不限次數，直接給折扣
                if (servicescopeInfo.LimitedCount == 0)
                    Base.AppointmentDetails.Cache.SetValueExt<FSAppointmentDet.discPct>(row, (servicescopeInfo?.DiscountPrecent ?? 0));
                // 限制次數，給予折扣
                else if (servicescopeInfo.LimitedCount - usedServiceCountHist - usedServiceCountCache > 0)
                    Base.AppointmentDetails.Cache.SetValueExt<FSAppointmentDet.discPct>(row, (servicescopeInfo?.DiscountPrecent ?? 0));
                // 次數不夠，跳出警示
                else
                    e.Cache.RaiseExceptionHandling<FSAppointmentDet.SMequipmentID>(
                        row,
                        e.NewValue,
                        new PXSetPropertyException<FSAppointmentDet.SMequipmentID>("Limited count for this service has been reached", PXErrorLevel.RowWarning));
            }
        }

        #endregion
    }
}