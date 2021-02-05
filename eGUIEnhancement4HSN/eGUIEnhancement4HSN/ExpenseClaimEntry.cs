using PX.Data;
using PX.Objects.CA;
using eGUICustomization4HSN.Descriptor;

namespace PX.Objects.EP
{
    public class ExpenseClaimEntry_Extension2 : PXGraphExtension<ExpenseClaimEntry_Extension, ExpenseClaimEntry>
    {
        #region Event Handlers
        protected void _(Events.RowPersisting<EPExpenseClaim> e, PXRowPersisting InvokeBaseHandler)
        {
            if (TWNGUIValidation.ActivateTWGUI(Base).Equals(true) && Base1.manGUIExpense.Select().Count.Equals(0) && !Base.Taxes.Select().Count.Equals(0))
            {
                string errorMsg = CATranEntry_Extension2.NoGUIWithTax;

                throw new PXException(errorMsg);
            }

            InvokeBaseHandler.Invoke(e.Cache, e.Args);
        }
        #endregion
    }
}