using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.TX;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;

namespace PX.Objects.EP
{
    public class ExpenseClaimEntry_Extension : PXGraphExtension<ExpenseClaimEntry>
    {
        #region Select
        public SelectFrom<TWNManualGUIExpense>
                         .Where<TWNManualGUIExpense.refNbr.IsEqual<EPExpenseClaim.refNbr.FromCurrent>>.View manGUIExpense;
        #endregion

        #region Event Handlers
        TWNGUIValidation tWNGUIValidation = new TWNGUIValidation();

        protected void _(Events.RowSelected<EPExpenseClaim> e, PXRowSelected InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(e.Cache, e.Args);

            manGUIExpense.Cache.AllowSelect = TWNGUIValidation.ActivateTWGUI(Base);
            manGUIExpense.Cache.AllowDelete = manGUIExpense.Cache.AllowInsert = manGUIExpense.Cache.AllowUpdate = !e.Row.Status.Equals(EPExpenseClaimStatus.ReleasedStatus);
        }

        protected void _(Events.RowPersisting<TWNManualGUIExpense> e)
        {
            if (Base.ExpenseClaimCurrent.Current == null) { return; }

            tWNGUIValidation.CheckCorrespondingInv(Base, e.Row.GUINbr, e.Row.VATInCode);

            if (tWNGUIValidation.errorOccurred.Equals(true) )
            {
                e.Cache.RaiseExceptionHandling<TWNManualGUIExpense.gUINbr>(e.Row, e.Row.GUINbr, new PXSetPropertyException(tWNGUIValidation.errorMessage, PXErrorLevel.RowError));
            }

            decimal taxSum = 0;

            foreach (TWNManualGUIExpense row in manGUIExpense.Cache.Cached)
            {
                taxSum += row.TaxAmt.Value;
            }

            if (!taxSum.Equals(Base.ExpenseClaimCurrent.Current.CuryTaxTotal))
            {
                throw new PXException(TWMessages.ChkTotalGUIAmt);
            }
        }

        protected void _(Events.FieldDefaulting<TWNManualGUIExpense, TWNManualGUIExpense.deduction> e)
        {
            var row = (TWNManualGUIExpense)e.Row;

            /// If user doesn't choose a vendor then bring the fixed default value from Attribure "DEDUCTCODE" first record.
            e.NewValue = row.VendorID == null ? "1" : e.NewValue;
        }

        protected void _(Events.FieldDefaulting<TWNManualGUIExpense, TWNManualGUIExpense.ourTaxNbr> e)
        {
            var row = (TWNManualGUIExpense)e.Row;

            TWNGUIPreferences preferences = SelectFrom<TWNGUIPreferences>.View.Select(Base);

            e.NewValue = row.VendorID == null ? preferences.OurTaxNbr : e.NewValue;
        }

        protected void _(Events.FieldVerifying<TWNManualGUIExpense, TWNManualGUIExpense.gUINbr> e)
        {
            var row = (TWNManualGUIExpense)e.Row;

            tWNGUIValidation.CheckGUINbrExisted(Base, (string)e.NewValue, row.VATInCode);
        }

        protected void _(Events.FieldVerifying<TWNManualGUIExpense, TWNManualGUIExpense.taxAmt> e)
        {
            var row = (TWNManualGUIExpense)e.Row;

            tWNGUIValidation.CheckTaxAmount((decimal)row.NetAmt, (decimal)e.NewValue);
        }

        protected void _(Events.FieldUpdated<TWNManualGUIExpense, TWNManualGUIExpense.netAmt> e)
        {         
            var row = (TWNManualGUIExpense)e.Row;

            foreach (TaxRev taxRev in SelectFrom<TaxRev>.Where<TaxRev.taxID.IsEqual<@P.AsString>
                                                               .And<TaxRev.taxType.IsEqual<TaxRev.taxType>>>.View.Select(Base, row.TaxID, "P")) // P = Group type (Input)
            {
                row.TaxAmt = row.NetAmt * (taxRev.TaxRate / taxRev.NonDeductibleTaxRate);
            }
        }
        #endregion
    }
}