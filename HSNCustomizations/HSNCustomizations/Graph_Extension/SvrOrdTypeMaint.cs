using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;

namespace PX.Objects.FS
{
    public class SvrOrdTypeMaint_Extension : PXGraphExtension<SvrOrdTypeMaint>
    {
        #region Selects
        [PXImport(typeof(LUMAutoWorkflowStage))]
        public SelectFrom<LUMAutoWorkflowStage>.Where<LUMAutoWorkflowStage.srvOrdType.IsEqual<FSSrvOrdType.srvOrdType.FromCurrent>>.View WorkflowStage;

        [PXImport(typeof(LumStageControl))]
        public SelectFrom<LumStageControl>.Where<LumStageControl.srvOrdType.IsEqual<FSSrvOrdType.srvOrdType.FromCurrent>>.View StageControl;
        #endregion

        #region Event Handler
        protected void _(Events.RowSelected<FSSrvOrdType> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache,e.Args);
            WorkflowStage.AllowSelect = StageControl.AllowSelect = SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst?.EnableWFStageCtrlInAppt == true;
        }

        protected void _(Events.FieldDefaulting<LUMAutoWorkflowStage.descr> e)
        {
            var row = e.Row as LUMAutoWorkflowStage;

            if (row.WFRule != null)
            {
                e.NewValue = WorkflowRuleSelectorAttribute.WFRuleDescr[(int)Enum.Parse(typeof(WFRule), row.WFRule)];
            }
        }
        #endregion
    }
}