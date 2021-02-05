using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CS;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;

namespace PX.Objects.AP
{
    public class APInvoiceEntry_Extension : PXGraphExtension<APInvoiceEntry>
    {
        #region Event Handlers
        public bool activateGUI = TWNGUIValidation.ActivateTWGUI(new PXGraph());

        //protected void APInvoice_RowPersisting(PXCache cache, PXRowPersistingEventArgs e, PXRowPersisting InvokeBaseHandler)
        protected void _(Events.RowPersisting<APInvoice> e, PXRowPersisting InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(e.Cache, e.Args);

            if (activateGUI.Equals(false) || e.Row.Status.Equals(APDocStatus.Open)) { return; }

            APRegisterExt regisExt = PXCache<APRegister>.GetExtension<APRegisterExt>(e.Row);

            TWNGUIValidation tWNGUIValidation = new TWNGUIValidation();

            if (regisExt.UsrVATINCODE == TWGUIFormatCode.vATInCode21 ||
                regisExt.UsrVATINCODE == TWGUIFormatCode.vATInCode22 ||
                regisExt.UsrVATINCODE == TWGUIFormatCode.vATInCode25)
            {
                tWNGUIValidation.CheckGUINbrExisted(Base, regisExt.UsrGUINO, regisExt.UsrVATINCODE);
            }
            else
            {
                tWNGUIValidation.CheckCorrespondingInv(Base, regisExt.UsrGUINO, regisExt.UsrVATINCODE);

                if (tWNGUIValidation.errorOccurred.Equals(true))
                {
                    e.Cache.RaiseExceptionHandling<APRegisterExt.usrGUINO>(e.Row, regisExt.UsrGUINO, new PXSetPropertyException(tWNGUIValidation.errorMessage, PXErrorLevel.Error));
                }
            }
        }

        protected void _(Events.RowInserting<APInvoice> e)
        {
            if (e.Row == null || Base.vendor.Current == null || activateGUI.Equals(false)) { return; }

            APRegisterExt regisExt = PXCache<APRegister>.GetExtension<APRegisterExt>(e.Row);

            string vATIncode = regisExt.UsrVATINCODE?? string.Empty;

            if (string.IsNullOrEmpty(vATIncode))
            {
                CSAnswers cSAnswers = SelectCSAnswers(Base, Base.vendor.Current.NoteID);

                vATIncode = (e.Row.IsRetainageDocument == true || cSAnswers == null) ? null : cSAnswers.Value;
            }

            regisExt.UsrVATINCODE = e.Row.DocType.Equals(APDocType.DebitAdj, System.StringComparison.CurrentCulture) && !string.IsNullOrEmpty(vATIncode) ? (int.Parse(vATIncode) + 2).ToString() : vATIncode;
        }

        protected void _(Events.RowSelected<APRegister> e)
        {
            PXUIFieldAttribute.SetVisible<APRegisterExt.usrDEDUCTION>(e.Cache, null, activateGUI);
            PXUIFieldAttribute.SetVisible<APRegisterExt.usrGUIDATE>  (e.Cache, null, activateGUI);
            PXUIFieldAttribute.SetVisible<APRegisterExt.usrGUINO>    (e.Cache, null, activateGUI);
            PXUIFieldAttribute.SetVisible<APRegisterExt.usrOurTaxID> (e.Cache, null, activateGUI);
            PXUIFieldAttribute.SetVisible<APRegisterExt.usrTaxID>    (e.Cache, null, activateGUI);
            PXUIFieldAttribute.SetVisible<APRegisterExt.usrVATINCODE>(e.Cache, null, activateGUI);
        }

        protected void _(Events.FieldUpdated<APInvoice.vendorID> e)
        {
            var row = e.Row as APInvoice;

            if (row == null || activateGUI.Equals(false)) { return; }

            switch (row.DocType)
            {
                case APDocType.DebitAdj:
                    PXCache<APRegister>.GetExtension<APRegisterExt>(row).UsrVATINCODE = TWGUIFormatCode.vATInCode23;
                    break;

                case APDocType.Invoice:
                    CSAnswers cSAnswers = SelectCSAnswers(Base, row.NoteID);

                    PXCache<APRegister>.GetExtension<APRegisterExt>(row).UsrVATINCODE = cSAnswers?.Value;
                    break;
            }
        }
        #endregion

        #region Static Method
        public static CSAnswers SelectCSAnswers(PXGraph graph, System.Guid? noteID)
        {
            return SelectFrom<CSAnswers>.Where<CSAnswers.refNoteID.IsEqual<@P.AsGuid>
                                               .And<CSAnswers.attributeID.IsEqual<APRegisterExt.VATINFRMTNameAtt>>>
                                        .View.Select(graph, noteID);
        }
        #endregion
    }
}