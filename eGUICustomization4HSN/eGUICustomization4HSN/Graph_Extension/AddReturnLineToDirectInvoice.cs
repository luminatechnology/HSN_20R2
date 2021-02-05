using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.SO.DAC.Projections;
using System.Collections;

namespace PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt
{
    public class AddReturnLineToDirectInvoice_Extension : PXGraphExtension<AddReturnLineToDirectInvoice, SOInvoiceEntry>
    {
        #region Delegate Action
        public delegate IEnumerable AddARTranDelegate(PXAdapter adapter);
        [PXOverride]
        public IEnumerable AddARTran(PXAdapter adapter, AddARTranDelegate baseMethod)
        {
            PXCache<ARRegister>.GetExtension<ARRegisterExt>(Base.ARInvoice_CustomerID_DocType_RefNbr.Current).UsrGUINo = (string)Base1.arTranList.GetValueExt<ARTranForDirectInvoiceExt.usrGUINbr>(Base1.arTranList.Current);

            return baseMethod(adapter);
        }
        #endregion

        #region Event Handlers
        protected void _(Events.FieldSelecting<ARTranForDirectInvoiceExt.usrGUINbr> e)
        {
            var row = e.Row as ARTranForDirectInvoice;

            if (row != null)
            {
                ARRegister register = SelectFrom<ARRegister>.Where<ARRegister.docType.IsEqual<@P.AsString>
                                                                  .And<ARRegister.refNbr.IsEqual<@P.AsString>>>.View.ReadOnly.Select(Base, row.TranType, row.RefNbr);

                e.ReturnValue = register.GetExtension<ARRegisterExt>().UsrGUINo;
            }
        }
        #endregion
    }

    public class ARTranForDirectInvoiceExt : PXCacheExtension<ARTranForDirectInvoice>
    {
        #region UsrGUINbr
        [PXDBString(14, IsUnicode = true)]
        [PXUIField(DisplayName = "GUI Nbr", Visibility = PXUIVisibility.Visible)]
        public virtual string UsrGUINbr { get; set; }
        public abstract class usrGUINbr : PX.Data.BQL.BqlString.Field<usrGUINbr> { }
        #endregion
    }
}