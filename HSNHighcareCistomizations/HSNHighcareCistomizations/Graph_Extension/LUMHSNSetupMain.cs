using HSNCustomizations;
using HSNCustomizations.DAC;
using HSNHighcareCistomizations.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.SM;
using PX.Web.UI.Frameset.Model.DAC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNHighcareCistomizations.Graph_Extension
{
    public class LUMHSNSetupMainExt : PXGraphExtension<LUMHSNSetupMaint>
    {
        public const string clenGUID = "00000000-0000-0000-0000-000000000000";

        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            baseMethod?.Invoke();
            PXGraph.CreateInstance<UpdateMaint>().ResetCachesCommand.Press();
        }

        protected void _(Events.RowUpdated<LUMHSNSetup> e, PXRowUpdated rowUpdated)
        {
            rowUpdated?.Invoke(e.Cache, e.Args);
            if (e.Row is LUMHSNSetup row && row != null)
            {
                var MUIWorkspaceDatas = SelectFrom<MUIWorkspace>.View.Select(Base).RowCast<MUIWorkspace>().ToList();
                var MUISubcategoryDatas = SelectFrom<MUISubcategory>.View.Select(Base).RowCast<MUISubcategory>().ToList();
                if (row.GetExtension<LUMHSNSetupExtension>()?.EnableHighcareFunction ?? false)
                {
                    updateSiteMapManual(
                        MUIWorkspaceDatas.FirstOrDefault(x => x.Title == "Receivables")?.WorkspaceID,
                        MUISubcategoryDatas.FirstOrDefault(x => x.Name == "Profiles")?.SubcategoryID,
                        "LM303000");
                    updateSiteMapManual(
                        MUIWorkspaceDatas.FirstOrDefault(x => x.Title == "Services")?.WorkspaceID,
                        MUISubcategoryDatas.FirstOrDefault(x => x.Name == "Preferences")?.SubcategoryID,
                        "LM304000");
                    updateSiteMapManual(
                        MUIWorkspaceDatas.FirstOrDefault(x => x.Title == "Deferred Revenue")?.WorkspaceID,
                        MUISubcategoryDatas.FirstOrDefault(x => x.Name == "Processes")?.SubcategoryID,
                        "LM505040");
                    updateSiteMapManual(
                       MUIWorkspaceDatas.FirstOrDefault(x => x.Title == "Receivables")?.WorkspaceID,
                       MUISubcategoryDatas.FirstOrDefault(x => x.Name == "Profiles")?.SubcategoryID,
                       "LM505050");
                }
                else
                {
                    updateSiteMapManual(new Guid(clenGUID), new Guid(clenGUID), "LM303000");
                    updateSiteMapManual(new Guid(clenGUID), new Guid(clenGUID), "LM304000");
                    updateSiteMapManual(new Guid(clenGUID), new Guid(clenGUID), "LM505040");
                    updateSiteMapManual(new Guid(clenGUID), new Guid(clenGUID), "LM505050");
                }
            }

        }

        public virtual void updateSiteMapManual(Guid? guidWorkspaceID, Guid? guidSubcategoryID, string screenID)
        {
            var vSiteMap = SelectFrom<SiteMap>.View.Select(Base).RowCast<SiteMap>().ToList();
            var curSiteMap = vSiteMap.FirstOrDefault(x => x.ScreenID == screenID);

            //Update site map workspace
            PXUpdate<Set<MUIScreen.workspaceID, Required<MUIScreen.workspaceID>, Set<MUIScreen.subcategoryID, Required<MUIScreen.subcategoryID>>>,
                         MUIScreen,
                         Where<MUIScreen.nodeID, Equal<Required<MUIScreen.nodeID>>
                     >>.Update(Base, guidWorkspaceID, guidSubcategoryID, curSiteMap?.NodeID);
        }
    }
}
