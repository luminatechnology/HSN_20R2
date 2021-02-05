using PX.Data;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class CreateInvoiceByServiceOrderPost_Extension : PXGraphExtension<CreateInvoiceByServiceOrderPost>
    {
        #region Delegate Data View
        protected virtual IEnumerable postLines()
        {
            BqlCommand cmd = Base.PostLines.View.BqlSelect;

            int totalRows = 0;
            int startRow = PXView.StartRow;
            PXView view = new PXView(Base, false, cmd);
            view.Clear();

            List<object> result = view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns,
                                              PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);

            foreach (ServiceOrderToPost row in result)
            {
                if (row.CuryDocTotal <= 0) { continue; }

                yield return row;
            }
        }
        #endregion
    }
}