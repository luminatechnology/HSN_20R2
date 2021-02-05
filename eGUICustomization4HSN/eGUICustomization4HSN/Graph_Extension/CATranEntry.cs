using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.TX;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;

namespace PX.Objects.CA
{
    public class CATranEntry_Extension : PXGraphExtension<CATranEntry>
    {
        #region Select & Setup
        // Retrieves detail records by CAAdj.adjRefNbr of the current master record.
        public SelectFrom<TWNManualGUIBank>
                         .Where<TWNManualGUIBank.adjRefNbr.IsEqual<CAAdj.adjRefNbr.FromCurrent>>.View ManGUIBank;

        public PXSetup<TWNGUIPreferences> GUIPreferences;
        #endregion

        #region Event Handlers
        public bool activateGUI = TWNGUIValidation.ActivateTWGUI(new PXGraph());

        TWNGUIValidation tWNGUIValidation = new TWNGUIValidation();

        protected void _(Events.RowSelected<CAAdj> e, PXRowSelected InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(e.Cache, e.Args);

            ManGUIBank.Cache.AllowSelect = activateGUI;
            ManGUIBank.Cache.AllowDelete = ManGUIBank.Cache.AllowInsert = ManGUIBank.Cache.AllowUpdate = !e.Row.Status.Equals(CATransferStatus.Released);
        }

        protected void _(Events.RowPersisting<TWNManualGUIBank> e)
        {
            if (Base.CurrentDocument.Current == null || activateGUI.Equals(false)) { return; }

            tWNGUIValidation.CheckCorrespondingInv(Base, e.Row.GUINbr, e.Row.VATInCode);

            if (tWNGUIValidation.errorOccurred.Equals(true) )
            {
                e.Cache.RaiseExceptionHandling<TWNManualGUIExpense.gUINbr>(e.Row, e.Row.GUINbr, new PXSetPropertyException(tWNGUIValidation.errorMessage, PXErrorLevel.RowError));
            }

            decimal taxSum = 0;

            foreach (TWNManualGUIBank row in ManGUIBank.Cache.Cached)
            {
                taxSum += row.TaxAmt.Value;
            }

            if (!taxSum.Equals(Base.CurrentDocument.Current.CuryTaxTotal))
            {
                throw new PXException(TWMessages.ChkTotalGUIAmt);
            }
        }

        protected void _(Events.FieldDefaulting<TWNManualGUIBank, TWNManualGUIBank.deduction> e)
        {
            var row = (TWNManualGUIBank)e.Row;

            /// If user doesn't choose a vendor then bring the fixed default value from Attribure "DEDUCTCODE" first record.
            e.NewValue = row.VendorID == null ? "1" : e.NewValue;
        }

        protected void _(Events.FieldDefaulting<TWNManualGUIBank, TWNManualGUIBank.ourTaxNbr> e)
        {
            var row = e.Row as TWNManualGUIBank;

            e.NewValue = row.VendorID == null ? GUIPreferences.Current.OurTaxNbr : e.NewValue;
        }

        protected void _(Events.FieldVerifying<TWNManualGUIBank, TWNManualGUIBank.gUINbr> e)
        {
            var row = (TWNManualGUIBank)e.Row;

            tWNGUIValidation.CheckGUINbrExisted(Base, (string)e.NewValue, row.VATInCode);
        }

        protected void _(Events.FieldVerifying<TWNManualGUIBank, TWNManualGUIBank.taxAmt> e)
        {
            var row = (TWNManualGUIBank)e.Row;

            tWNGUIValidation.CheckTaxAmount((decimal)row.NetAmt, (decimal)e.NewValue);
        }

        protected void _(Events.FieldUpdated<TWNManualGUIBank, TWNManualGUIBank.netAmt> e)
        {        
            var row = (TWNManualGUIBank)e.Row;

            foreach (TaxRev taxRev in SelectFrom<TaxRev>.Where<TaxRev.taxID.IsEqual<@P.AsString>
                                                               .And<TaxRev.taxType.IsEqual<TaxRev.taxType>>>.View.Select(Base, row.TaxID, "P")) // P = Group type (Input)
            {
                row.TaxAmt = row.NetAmt * (taxRev.TaxRate / taxRev.NonDeductibleTaxRate);
            }
        }
        #endregion
    }
}