using PX.Data;
using PX.Objects.AR;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class CreateInvoiceByAppointmentPost_Extension : PXGraphExtension<CreateInvoiceByAppointmentPost>
    {
        #region Delegate Methods
        //public delegate void UpdatePostInfoAndPostDetDelegate(List<DocLineExt> docLinesWithPostInfo, FSPostBatch fsPostBatchRow, PostInfoEntry graphPostInfoEntry, PXCache<FSPostDet> cacheFSPostDet, FSCreatedDoc fsCreatedDocRow);
        //[PXOverride]
        //public virtual void UpdatePostInfoAndPostDet(List<DocLineExt> docLinesWithPostInfo, FSPostBatch fsPostBatchRow, PostInfoEntry graphPostInfoEntry, PXCache<FSPostDet> cacheFSPostDet,
        //                                             FSCreatedDoc fsCreatedDocRow, UpdatePostInfoAndPostDetDelegate baseMethod)
        //{
        //    baseMethod(docLinesWithPostInfo, fsPostBatchRow, graphPostInfoEntry, cacheFSPostDet, fsCreatedDocRow);

        //    ARRegister register = PXSelect<ARRegister,
        //                                   Where<ARRegister.docType, Equal<Required<FSCreatedDoc.createdDocType>>,
        //                                         And<ARRegister.refNbr, Equal<Required<FSCreatedDoc.createdRefNbr>>>>>
        //                                   .Select(Base, fsCreatedDocRow.CreatedDocType, fsCreatedDocRow.CreatedRefNbr);

        //    if (register != null && Base.PostLines.Current.CuryDocTotal.Equals(decimal.Zero))
        //    {
        //        PXCache<ARRegister>.GetExtension<ARRegisterExt>(register).UsrVATOutCode = string.Empty;
        //    }
        //}
        #endregion

        #region Delegate Data View
        protected virtual IEnumerable postLines()
        {
            BqlCommand cmd = Base.PostLines.View.BqlSelect;

            //cmd.WhereAnd<Where<AppointmentToPost.curyDocTotal, Greater<decimal0>>>();

            int totalRows = 0;
            int startRow = PXView.StartRow;
            PXView view = new PXView(Base, false, cmd);
            view.Clear();

            List<object> result = view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns,
                                              PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);

            foreach (PXResult<AppointmentToPost, Customer> row in result)
            {
                if (Base.Filter.Current.PostTo.Equals(ID.Batch_PostTo.AR_AP))
                {
                    if (row.GetItem<AppointmentToPost>().CuryDocTotal <= 0) { continue; }
                }

                yield return row;
            }
        }
        #endregion
    }
}