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
            { "CitiOutSourceCheck", "LM505030" },
            { "PrintTransferProcess", "LM501000" },
            { "PrintTransferPickingList", "LM502000" },
            { "PrintTransferDeliveryOrder", "LM502010" },
            //Reports
            { "PrintPrepaymentReceipt", "LM602000" },
            { "Quote-MY", "LM604500" },
            { "Quote-MY2", "LM604501" },
            { "PrintRegisterDetailed", "LM622500"},
            { "PrintRegisterDetailed(CitiBank)", "LM622505"},
            { "PrintPickingListReport", "LM644005"},
            { "PrintDeliveryOrderReport", "LM644010"}
        };

        protected void _(Events.RowUpdating<LUMHSNSetup> e, PXRowUpdating rowUpdating)
        {
            Guid newGUID = new Guid("00000000-0000-0000-0000-000000000000");
            //SCB, CitiBank
            Guid payablesGuid = (Guid)(SelectFrom<MUIWorkspace>.Where<MUIWorkspace.title.IsEqual<@P.AsString>>.View.Select(this, "Payables").TopFirst?.WorkspaceID);
            Guid processGuid = (Guid)(SelectFrom<MUISubcategory>.Where<MUISubcategory.name.IsEqual<@P.AsString>>.View.Select(this, "Processes").TopFirst?.SubcategoryID);
            //Quote
            Guid opportunitiesGuid = (Guid)(SelectFrom<MUIWorkspace>.Where<MUIWorkspace.title.IsEqual<@P.AsString>>.View.Select(this, "Opportunities").TopFirst?.WorkspaceID);
            Guid printedForms = (Guid)(SelectFrom<MUISubcategory>.Where<MUISubcategory.name.IsEqual<@P.AsString>>.View.Select(this, "Printed Forms").TopFirst?.SubcategoryID);
            //Report
            Guid inventoryGuid = (Guid)(SelectFrom<MUIWorkspace>.Where<MUIWorkspace.title.IsEqual<@P.AsString>>.View.Select(this, "Inventory").TopFirst?.WorkspaceID);
            Guid reportGiud = (Guid)(SelectFrom<MUISubcategory>.Where<MUISubcategory.name.IsEqual<@P.AsString>>.View.Select(this, "Reports").TopFirst?.SubcategoryID);
            LUMHSNSetup curLumLUMHSNSetup = this.Caches[typeof(LUMHSNSetup)].Current as LUMHSNSetup;

            //PrintTransferProcess
            if (!curLumLUMHSNSetup?.EnablePrintTransferProcess == true)
            {
                updateSiteMap(newGUID, newGUID, dicProcesses["PrintTransferProcess"]);
                updateSiteMap(newGUID, newGUID, dicProcesses["PrintTransferPickingList"]);
                updateSiteMap(newGUID, newGUID, dicProcesses["PrintTransferDeliveryOrder"]);
                //Report
                updateSiteMap(newGUID, newGUID, dicProcesses["PrintPickingListReport"]);
                updateSiteMap(newGUID, newGUID, dicProcesses["PrintDeliveryOrderReport"]);
            }
            else
            {
                updateSiteMap(inventoryGuid, processGuid, dicProcesses["PrintTransferProcess"]);
                updateSiteMap(inventoryGuid, processGuid, dicProcesses["PrintTransferPickingList"]);
                updateSiteMap(inventoryGuid, processGuid, dicProcesses["PrintTransferDeliveryOrder"]);
                //Report
                updateSiteMap(inventoryGuid, reportGiud, dicProcesses["PrintPickingListReport"]);
                updateSiteMap(inventoryGuid, reportGiud, dicProcesses["PrintDeliveryOrderReport"]);
            }

            //SCBPayment, and Register Detailed Report
            if (!curLumLUMHSNSetup?.EnableSCBPaymentFile == true)
            {
                updateSiteMap(newGUID, newGUID, dicProcesses["SCBPayment"]);
                updateSiteMap(newGUID, newGUID, dicProcesses["PrintRegisterDetailed"]);
            }
            else
            {
                updateSiteMap(payablesGuid, processGuid, dicProcesses["SCBPayment"]);
                updateSiteMap(payablesGuid, reportGiud, dicProcesses["PrintRegisterDetailed"]);
            }
            //CitiTTPayment
            if (!curLumLUMHSNSetup?.EnableCitiPaymentFile == true) updateSiteMap(newGUID, newGUID, dicProcesses["CitiTTPayment"]);
            else updateSiteMap(payablesGuid, processGuid, dicProcesses["CitiTTPayment"]);
            //CitiReturnCheck
            if (!curLumLUMHSNSetup?.EnableCitiReturnCheckFile == true) updateSiteMap(newGUID, newGUID, dicProcesses["CitiReturnCheck"]);
            else updateSiteMap(payablesGuid, processGuid, dicProcesses["CitiReturnCheck"]);
            //CitiOutSourceCheck
            if (!curLumLUMHSNSetup?.EnableCitiOutSourceCheckFile == true) updateSiteMap(newGUID, newGUID, dicProcesses["CitiOutSourceCheck"]);
            else updateSiteMap(payablesGuid, processGuid, dicProcesses["CitiOutSourceCheck"]);

            //Register Detailed Report (CitiBank)
            if (curLumLUMHSNSetup?.EnableCitiPaymentFile == true || curLumLUMHSNSetup?.EnableCitiReturnCheckFile == true || curLumLUMHSNSetup?.EnableCitiOutSourceCheckFile == true) updateSiteMap(inventoryGuid, reportGiud, dicProcesses["PrintRegisterDetailed(CitiBank)"]);
            else updateSiteMap(newGUID, newGUID, dicProcesses["PrintRegisterDetailed(CitiBank)"]);

            //Quote
            if (!curLumLUMHSNSetup?.EnableOpportunityEnhance == true)
            {
                updateSiteMap(newGUID, newGUID, dicProcesses["Quote-MY"]);
                updateSiteMap(newGUID, newGUID, dicProcesses["Quote-MY2"]);
            }
            else
            {
                updateSiteMap(opportunitiesGuid, printedForms, dicProcesses["Quote-MY"]);
                updateSiteMap(opportunitiesGuid, printedForms, dicProcesses["Quote-MY2"]);
            }
            

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