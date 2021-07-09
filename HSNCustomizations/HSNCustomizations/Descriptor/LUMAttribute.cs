using System;
using System.Collections;
using PX.Data;

namespace HSNCustomizations.Descriptor
{
    #region WorkflowRuleSelectorAttribute
    #region enum
    public enum WFRule
    {
        OPEN01 = 0,
        ASSIGN01 = 1,
        DIAGNOSE01 = 2,
        QUOTATION01 = 3,
        QUOTATION03 = 4,
        AWSPARE01 = 5
    }
    #endregion

    public class WorkflowRuleSelectorAttribute : PXCustomSelectorAttribute
    {
		public WorkflowRuleSelectorAttribute() : base(typeof(LUMWorkflowRule.ruleID),
                                                      typeof(LUMWorkflowRule.ruleID),
                                                      typeof(LUMWorkflowRule.descr))
		{
			DescriptionField = typeof(LUMWorkflowRule.descr);
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e) { }

        public static string[] WFRuleDescr =
        {
            "Change to Open Stage when appointment is created",
            "Change to Assigned Stage when staff is assigned",
            "Change to Under Diagnose Stage when appointment is started",
            "Change to Quotation Required Stage when parts is required",
            "Change to Quotation Required Stage when parts is required",
            "Change to Awaiting Spare Parts Stage when part request is initiated"
        };

        protected virtual IEnumerable GetRecords()
		{
			foreach (string wFRule in Enum.GetNames(typeof(WFRule)))
            {
                LUMWorkflowRule wfRule = new LUMWorkflowRule()
				{
                    RuleID = wFRule,
                    Descr  = WFRuleDescr[(int)Enum.Parse(typeof(WFRule), wFRule)]
				};

				yield return wfRule;
			}
		}

        #region Unbound DAC
        [PXHidden]
        [Serializable]
        public partial class LUMWorkflowRule : PX.Data.IBqlTable
        {
            #region RuleID
            [PXString(12, IsUnicode = true, IsKey = true)]
            [PXUIField(DisplayName = "Rule")]
            public virtual string RuleID { get; set; }
            public abstract class ruleID : PX.Data.BQL.BqlString.Field<ruleID> { }
            #endregion

            #region Descr
            [PXString(256, IsUnicode = true)]
            [PXUIField(DisplayName = "Description")]
            public virtual string Descr { get; set; }
            public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
            #endregion
        }
        #endregion
    }
    #endregion
}
