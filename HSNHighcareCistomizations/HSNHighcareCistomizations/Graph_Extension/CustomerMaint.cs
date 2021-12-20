using HSNCustomizations.DAC;
using HSNHighcareCistomizations.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AR
{
    public class CustomerMaintExt : PXGraphExtension<CustomerMaint>
    {
        public SelectFrom<LUMCustomerPINCode>
               .Where<LUMCustomerPINCode.bAccountID.IsEqual<Customer.bAccountID.FromCurrent>>
               .View CustomerPINCode;

        #region Override Method
        public override void Initialize()
        {
            base.Initialize();
            var hsnSetup = SelectFrom<LUMHSNSetup>.View.Select(Base).RowCast<LUMHSNSetup>().FirstOrDefault();
            this.CustomerPINCode.AllowDelete = this.CustomerPINCode.AllowInsert = this.CustomerPINCode.AllowUpdate = false;
            this.CustomerPINCode.AllowSelect = hsnSetup.GetExtension<LUMHSNSetupExtension>()?.EnableHighcareFunction ?? false;
        }
        #endregion

        #region Events
        public virtual void _(Events.RowSelected<LUMCustomerPINCode> e)
        {
            if (e.Row != null)
                this.CustomerPINCode.Cache.SetValueExt<LUMCustomerPINCode.isActive>(e.Row, DateTime.Now.Date >= e.Row.StartDate?.Date && DateTime.Now.Date <= e.Row.EndDate?.Date);
        }
        #endregion

    }
}
