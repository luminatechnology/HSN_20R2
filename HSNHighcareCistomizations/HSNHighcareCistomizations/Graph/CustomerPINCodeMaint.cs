using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Objects.CR;
using System.Threading.Tasks;
using HSNHighcareCistomizations.DAC;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Data.EP;

namespace HSNHighcareCistomizations.Graph
{
    public class CustomerPINCodeMaint : PXGraph<CustomerPINCodeMaint>
    {
        public PXSave<Customer> Save;
        public PXCancel<Customer> Cancel;

        //public SelectFrom<Customer>.View Document;
        public PXSelect<
                Customer,
            Where2<
                Match<Current<AccessInfo.userName>>,
                And<Where<BAccount.type, Equal<BAccountType.customerType>,
                    Or<BAccount.type, Equal<BAccountType.combinedType>>>>>> Document;

        public SelectFrom<LumCustomerPINCode>
               .Where<LumCustomerPINCode.bAccountID.IsEqual<Customer.bAccountID.FromCurrent>>.View Transaction;

        public virtual void _(Events.RowPersisting<LumCustomerPINCode> e)
        {
            if (e.Row is LumCustomerPINCode row && row != null && this.Document.Current != null)
            {
                row.BAccountID = this.Document.Current.BAccountID;
                row.StartDate = DateTime.Now;
                row.EndDate = DateTime.Now.AddDays(364);
            }
        }
    }

    public class HighcareAttr : PX.Data.BQL.BqlString.Constant<HighcareAttr>
    {
        public HighcareAttr() : base("HIGHCARE") { }
    }
}
