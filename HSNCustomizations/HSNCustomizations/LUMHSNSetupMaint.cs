using PX.Data;
using PX.Data.BQL.Fluent;
using PX.SiteMap.DAC;
using HSNCustomizations.DAC;
using System.Linq;
using PX.Web.UI.Frameset.Model.DAC;
using PX.Data.BQL;
using System;
using PX.SiteMap.Graph;
using PX.Caching;
using PX.Metadata;
using System.Collections.Generic;

namespace HSNCustomizations
{
    public class LUMHSNSetupMaint : PXGraph<LUMHSNSetupMaint>
    {
        public PXSave<LUMHSNSetup> Save;
        public PXCancel<LUMHSNSetup> Cancel;

        public SelectFrom<LUMHSNSetup>.View hSNSetup;

        [PXImport(typeof(LUMBranchWarehouse))]
        public SelectFrom<LUMBranchWarehouse>.View BranchWarehouse;

        [PXImport(typeof(LUMTermsConditions))]
        public SelectFrom<LUMTermsConditions>.View TermsConditions;

        [InjectDependency]
        protected ICacheControl<PageCache> PageCacheControl { get; set; }

        private Dictionary<string, string> dicProcesses = new Dictionary<string, string>()
        {
            { "SCBPayment", "LM505000" },
            { "CitiTTPayment", "LM505010" },
            { "CitiReturnCheck", "LM505020" },
            { "CitiOutSourceCheck", "LM505030" }
        };

        protected void _(Events.RowUpdating<LUMHSNSetup> e, PXRowUpdating rowUpdating)
        {
            Guid newGUID = new Guid("00000000-0000-0000-0000-000000000000");
            Guid payablesGuid = (Guid)(SelectFrom<MUIWorkspace>.Where<MUIWorkspace.title.IsEqual<@P.AsString>>.View.Select(this, "Payables").TopFirst?.WorkspaceID);
            Guid processGuid = (Guid)(SelectFrom<MUISubcategory>.Where<MUISubcategory.name.IsEqual<@P.AsString>>.View.Select(this, "Processes").TopFirst?.SubcategoryID);
            LUMHSNSetup curLumLUMHSNSetup = this.Caches[typeof(LUMHSNSetup)].Current as LUMHSNSetup;

            //SCBPayment
            if (!curLumLUMHSNSetup?.EnableSCBPaymentFile == true) updateSiteMap(newGUID, newGUID, dicProcesses["SCBPayment"]);
            else updateSiteMap(payablesGuid, processGuid, dicProcesses["SCBPayment"]);
            //CitiTTPayment
            if (!curLumLUMHSNSetup?.EnableCitiPaymentFile == true) updateSiteMap(newGUID, newGUID, dicProcesses["CitiTTPayment"]);
            else updateSiteMap(payablesGuid, processGuid, dicProcesses["CitiTTPayment"]);
            //CitiReturnCheck
            if (!curLumLUMHSNSetup?.EnableCitiReturnCheckFile == true) updateSiteMap(newGUID, newGUID, dicProcesses["CitiReturnCheck"]);
            else updateSiteMap(payablesGuid, processGuid, dicProcesses["CitiReturnCheck"]);
            //CitiOutSourceCheck
            if (!curLumLUMHSNSetup?.EnableCitiOutSourceCheckFile == true) updateSiteMap(newGUID, newGUID, dicProcesses["CitiOutSourceCheck"]);
            else updateSiteMap(payablesGuid, processGuid, dicProcesses["CitiOutSourceCheck"]);

            //clear cache
            PageCacheControl.InvalidateCache();
            //refresh page
            Redirector.Refresh(System.Web.HttpContext.Current);
        }

        protected void updateSiteMap(Guid guidWorkspaceID, Guid guidSubcategoryID, string screenID)
        {
            var vSiteMap = SelectFrom<SiteMap>.View.Select(this).RowCast<SiteMap>().ToList();
            var curSiteMap = vSiteMap.FirstOrDefault(x => x.ScreenID == screenID);

            //Update site map workspace
            PXUpdate<Set<MUIScreen.workspaceID, Required<MUIScreen.workspaceID>, Set<MUIScreen.subcategoryID, Required<MUIScreen.subcategoryID>>>,
                         MUIScreen,
                         Where<MUIScreen.nodeID, Equal<Required<MUIScreen.nodeID>>
                     >>.Update(this, guidWorkspaceID, guidSubcategoryID, curSiteMap?.NodeID);
        }
    }
}