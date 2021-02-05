using PX.Data;
using eGUICustomization4HSN.Descriptor;

namespace PX.Objects.TX
{
    public class SalesTaxMaint_Extension : PXGraphExtension<SalesTaxMaint>
    {
        #region Event Handlers
        protected void _(Events.RowSelected<Tax> e, PXRowSelected InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(e.Cache, e.Args);

            PXUIFieldAttribute.SetVisible<TaxExt.usrGUIType>(e.Cache, null, TWNGUIValidation.ActivateTWGUI(Base));
            PXUIFieldAttribute.SetVisible<TaxExt.usrTWNGUI> (e.Cache, null, TWNGUIValidation.ActivateTWGUI(Base));
        }
        #endregion
    }
}