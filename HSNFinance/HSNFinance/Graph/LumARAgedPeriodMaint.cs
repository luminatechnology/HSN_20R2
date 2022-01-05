using System;
using System.Collections;
using System.Collections.Generic;
using HSNFinance.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace HSNFinance.Graph
{
    public class LumARAgedPeriodMaint : PXGraph<LumARAgedPeriodMaint>
    {

        public PXFilter<LumARAgedPeriodFilter> MasterFilter;
        public SelectFrom<LumARAgedPeriod>.OrderBy<Asc<LumARAgedPeriod.lineNbr>>.View DetailsView;

        public LumARAgedPeriodMaint()
        {
            DetailsView.AllowInsert = DetailsView.AllowUpdate = DetailsView.AllowDelete = false;
        }

        #region Delegate DataView
        public IEnumerable detailsView()
        {
            List<object> result = new List<object>();
            int repeatTime = 6;
            var periodID = ((LumARAgedPeriodFilter)this.Caches[typeof(LumARAgedPeriodFilter)].Current)?.FinPeriodID;
            
            if (periodID != null)
            {
                var pars = new List<PXSPParameter>();
                PXSPParameter conditionPeriodID = new PXSPInParameter("@ConditionPeriodID", PXDbType.Char, periodID);
                PXSPParameter companyID = new PXSPInParameter("@companyID", PXDbType.Int, PX.Data.Update.PXInstanceHelper.CurrentCompany);
                PXSPParameter repeatTimes = new PXSPInParameter("@RepeatTimes", PXDbType.Int, repeatTime);
                pars.Add(conditionPeriodID);
                pars.Add(companyID);
                pars.Add(repeatTimes);

                using (new PXConnectionScope())
                {
                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        PXDatabase.Execute("SP_GenerateLumARAgedPeriod", pars.ToArray());
                        ts.Complete();
                    }
                }

                PXView select = new PXView(this, true, DetailsView.View.BqlSelect);
                int totalrow = 0;
                int startrow = PXView.StartRow;
                result = select.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startrow, PXView.MaximumRows, ref totalrow);
                PXView.StartRow = 0;
                return result;
            }
            return result;
        }
        #endregion

        #region LumARAgedPeriodFilter
        [Serializable]
        [PXCacheName("LumARAgedPeriodFilter")]
        public class LumARAgedPeriodFilter : IBqlTable
        {
            #region FinPeriodID
            [FinPeriodID()]
            [PXUIField(DisplayName = "Finanical Period", Visibility = PXUIVisibility.SelectorVisible)]
            [PXSelector(typeof(Search<FinPeriod2.finPeriodID, Where<FinPeriod2.closed.IsEqual<False>.And<FinPeriod2.active.IsEqual<True>>>, OrderBy<Desc<FinPeriod2.finPeriodID>>>), typeof(FinPeriod2.finPeriodID))]
            public virtual string FinPeriodID { get; set; }
            public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
            #endregion
        }
        #endregion

    }
}