using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.CS;

namespace PX.Objects.SO
{
    public class SOOrderEntry_Extension : PXGraphExtension<SOOrderEntry>
    {
        #region CacheAttached
        [PXString(15, IsUnicode = true, InputMask = "")]
        [PXDefault()]
        [PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
        [ARInvoiceType2.RefNbr2(typeof(Search2<AR.Standalone.ARRegisterAlias.refNbr,
                                               InnerJoinSingleTable<ARInvoice, On<ARInvoice.docType, Equal<AR.Standalone.ARRegisterAlias.docType>,
                                                                    And<ARInvoice.refNbr, Equal<AR.Standalone.ARRegisterAlias.refNbr>>>>,
                                               Where<AR.Standalone.ARRegisterAlias.docType, Equal<Optional<AddInvoiceFilter.docType>>,
                                                     And<AR.Standalone.ARRegisterAlias.released, Equal<boolTrue>,
                                                         And<AR.Standalone.ARRegisterAlias.origModule, Equal<BatchModule.moduleSO>,
                                                             And<AR.Standalone.ARRegisterAlias.customerID, Equal<Current<SOOrder.customerID>>>>>>,
                                               OrderBy<Desc<AR.Standalone.ARRegisterAlias.refNbr>>>), Filterable = true)]
        [PXFormula(typeof(Default<AddInvoiceFilter.docType>))]
        protected void _(Events.CacheAttached<AddInvoiceFilter.refNbr> e) { }
        #endregion
    }

    /// <summary>
    /// Extend the standard Selector attibute
    /// </summary>
    public class ARInvoiceType2 : ARInvoiceType
    {
        public class RefNbr2Attribute : PXSelectorAttribute
        {
            /// <summary>
            /// Add a custom extension field on search form.
            /// </summary>
            /// <param name="SearchType">Must be IBqlSearch, returning ARInvoice.refNbr</param>
            public RefNbr2Attribute(Type SearchType) : base(SearchType,
                                                            typeof(ARRegister.refNbr),
                                                            typeof(ARInvoice.invoiceNbr),
                                                            typeof(ARRegisterExt.usrGUINo),
                                                            typeof(ARRegister.docDate),
                                                            typeof(ARRegister.finPeriodID),
                                                            typeof(ARRegister.customerID),
                                                            typeof(ARRegister.customerID_Customer_acctName),
                                                            typeof(ARRegister.customerLocationID),
                                                            typeof(ARRegister.curyID),
                                                            typeof(ARRegister.curyOrigDocAmt),
                                                            typeof(ARRegister.curyDocBal),
                                                            typeof(ARRegister.status),
                                                            typeof(ARRegister.dueDate))
            { }
        }
    }
}