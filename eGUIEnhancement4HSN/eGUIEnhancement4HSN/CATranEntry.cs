using PX.Data;
using eGUICustomization4HSN.Descriptor;

namespace PX.Objects.CA
{
    public class CATranEntry_Extension2 : PXGraphExtension<CATranEntry_Extension, CATranEntry>
    {
        public const string NoGUIWithTax = "There Are Tax Details, But There Is No Record TW GUI.";

        #region Event Handlers
        protected void _(Events.RowPersisting<CAAdj> e, PXRowPersisting InvokeBaseHandler)
        {
            if (Base1.activateGUI && Base1.ManGUIBank.Select().Count.Equals(0) && !Base.Taxes.Select().Count.Equals(0))
            {
                throw new PXException(NoGUIWithTax);
            }

            InvokeBaseHandler.Invoke(e.Cache, e.Args);
        }
        #endregion
    }
}