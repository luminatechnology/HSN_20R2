using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;

namespace PX.Objects.SO
{
    public class SOInvoiceEntry_Extensions : PXGraphExtension<PX.Objects.SO.SOInvoiceEntry>
    {
        #region Delegate Methods
        public delegate void ReleaseInvoiceProcDelegate(List<ARRegister> list, bool isMassProcess);
        [PXOverride]
        public void ReleaseInvoiceProc(List<ARRegister> list, bool isMassProcess, ReleaseInvoiceProcDelegate baseMethod)
        {
            baseMethod(list, isMassProcess);

            if (Base.Document.Current?.Released == true)
            {
                UpdateDocDateFromOppr(Base, Base.Document.Current.DocType, Base.Document.Current.RefNbr);
            }
        }
        #endregion

        #region Static Method
        /// <summary>
        /// When user release the SO invoice, add a validation to check whether there is an opportunity linked with the original sales order. If yes, update the Billed Date = ARInvoice.DocDate
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="docType"></param>
        /// <param name="refNbr"></param>
        public static void UpdateDocDateFromOppr(PXGraph graph, string docType, string refNbr)
        {
            PXUpdateJoin<Set<ARInvoice.docDate, CROpportunityExt.usrBilledDate>,
                         ARInvoice,
                         InnerJoin<CRRelation, On<CRRelation.refNoteID, Equal<ARInvoice.noteID>>,
                                   InnerJoin<CROpportunity, On<CROpportunity.noteID, Equal<CRRelation.targetNoteID>,
                                                               And<CRRelation.role, Equal<CRRoleTypeList.source>>>>>,
                         Where<ARInvoice.docType, Equal<Required<SOInvoice.docType>>,
                               And<ARInvoice.refNbr, Equal<Required<SOInvoice.refNbr>>>>>.Update(graph, docType, refNbr);
        }
        #endregion
    }
}
