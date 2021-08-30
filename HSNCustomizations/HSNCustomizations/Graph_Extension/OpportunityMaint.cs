using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;
using PX.Objects.IN;
using HSNCustomizations.DAC;

namespace PX.Objects.CR
{
    public class OpportunityMaint_Extension : PXGraphExtension<SM_OpportunityMaint, OpportunityMaint>
    {
        #region Selects
        public SelectFrom<LUMHSNSetup>.View HSNSetupView;
        public SelectFrom<LUMOpprTermCond>.Where<LUMOpprTermCond.opportunityID.IsEqual<CROpportunity.opportunityID.FromCurrent>>.OrderBy<LUMOpprTermCond.sortOrder.Asc>.View TermsConditions;
        #endregion

        #region Delegate Methods
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            if (Base.Opportunity.Current != null && TermsConditions.Select().Count <= 0)
            {
                CopyRecordFromHSNSetup();
            }

            baseMethod();
        }
        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<CROpportunity> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            bool isActived = HSNSetupView.Select().TopFirst?.EnableOpportunityEnhance ?? false;

            TermsConditions.AllowSelect = isActived;
            
            PXUIFieldAttribute.SetVisible<CROpportunityExt.usrTotalMargin>(e.Cache, e.Row, isActived);
            PXUIFieldAttribute.SetVisible<CROpportunityExt.usrTotalMarginPct>(e.Cache, e.Row, isActived);
            PXUIFieldAttribute.SetVisible<CROpportunityExt.usrValidityDate>(e.Cache, e.Row, isActived);
            PXUIFieldAttribute.SetVisible<CROpportunityExt.usrPODate>(e.Cache, e.Row, isActived);
            PXUIFieldAttribute.SetVisible<CROpportunityExt.usrBilledDate>(e.Cache, e.Row, isActived);
        }

        protected void _(Events.RowSelected<CROpportunityProducts> e, PXRowSelected baseHandler)
        {
            PXUIFieldAttribute.SetEnabled<CROpportunityProducts.curyUnitCost>(e.Cache, null, true);
        }

        protected void _(Events.FieldDefaulting<CROpportunityProducts.curyUnitCost> e)
        {
            var row = e.Row as CROpportunityProducts;

            if (row != null)
            {
                var itemCost = INItemCost.PK.Find(Base, row.InventoryID);

                e.NewValue = itemCost?.AvgCost > 0 ? itemCost?.AvgCost : itemCost?.LastCost;
            }
        }
        #endregion

        #region Methods
        public virtual void CopyRecordFromHSNSetup()
        {
            foreach (LUMTermsConditions row in SelectFrom<LUMTermsConditions>.View.Select(Base) )
            {
                LUMOpprTermCond termCond = new LUMOpprTermCond()
                {
                    SortOrder     = row.SortOrder,
                    Title         = row.Title,
                    Definition    = row.Definition
                };

                termCond.QuoteID = null;

                TermsConditions.Insert(termCond);
            }
        }
        #endregion
    }
}