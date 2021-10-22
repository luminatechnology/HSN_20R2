using System;
using System.Collections;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.IN;
using HSNCustomizations.DAC;

namespace HSNCustomizations.Descriptor
{
    #region WorkflowRuleSelectorAttribute
    #region enum
    public enum WFRule
    {
        OPEN01 = 0,
        ASSIGN01 = 1,
        ASSIGN03 = 2,
        START01 = 3,
        QUOTATION01 = 4,
        QUOTATION03 = 5,
        AWSPARE01 = 6,
        AWSPARE03 = 7,
        AWSPARE05 = 8,
        AWSPARE07 = 9,
        FINISH01 = 10,
        COMPLETE01 = 11,
        COMPLETE03 = 12,
        INVOICE01 = 13
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
            "Change to Waiting Stage when driver is arranged to pick up machine",
            "Change to Under Diagnose Stage when appointment is started",
            "Change to Quotation Required Stage when parts is required",
            "Change to Quoted when email is sent to customer",
            "Change to Awaiting Spare Parts Stage when part request is initiated",
            "Change to Under Repair Stage when 1-step transfer is released",
            "Change to Part in Transit Stage when 2-step transfer out is released",
            "Change to Under Repair Stage when 2-step transfer is received and released",
            "Change to Under Testing when Finished Check Box is ticked",
            "Change to Repair Complete when appointment is 'completed' by QC/ Engineer",
            "Change to RTS when service order is 'completed' by",
            "Change to Closed when invoice is generated and released."
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

    #region ARPymtNumberingAttribute
    /// <summary>
    /// If “Customer Prepayment Numbering Sequence” is blank, follow the standard numbering sequence when the transaction Type is “Prepayment”.
    /// </summary>
    public class ARPymtNumberingAttribute : ARPaymentType.NumberingAttribute
    {
        public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            string curDoType = (string)sender.GetValue<ARPayment.docType>(e.Row);

            LUMHSNSetup hSNSetup = SelectFrom<LUMHSNSetup>.View.Select(sender.Graph);

            if (curDoType == ARDocType.Prepayment && !string.IsNullOrEmpty(hSNSetup?.CPrepaymentNumberingID) && this.UserNumbering == false && e.Operation == PXDBOperation.Insert)
            {
                string generated = PX.Objects.CS.AutoNumberAttribute.GetNextNumber(sender, e.Row, hSNSetup.CPrepaymentNumberingID, (e.Row as ARPayment).DocDate);

                sender.SetValue(e.Row, _FieldName, generated);
            }
            else
            { base.RowPersisting(sender, e); }
        }
    }
    #endregion

    #region INTotalQtyVerificationAttribute
    /// <summary>
    /// If LUMHSNSetup.EnablePartReqInAppt=True, then if INRegister.TotalQty=0, then display error “System cannot save records with 0 quantity” when user click SAVE button.
    /// This rule should apply to Inventory Receipts, Issue, and Transfer.
    /// </summary>
    public class INTotalQtyVerificationAttribute : PXDBQuantityAttribute, IPXRowPersistingSubscriber
    {
        public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            string  docType  = (string)sender.GetValue<INTran.docType>(e.Row);
            decimal totalQty = (decimal)sender.GetValue<INTran.qty>(e.Row);

            if (e.Operation != PXDBOperation.Delete && docType.IsIn(INDocType.Receipt, INDocType.Issue, INDocType.Transfer) && 
                SelectFrom<LUMHSNSetup>.View.Select(sender.Graph).TopFirst?.EnablePartReqInAppt == true && totalQty <= 0)
            {
                sender.RaiseExceptionHandling<INTran.qty>(e.Row, totalQty, new PXSetPropertyException(HSNMessages.TotalQtyIsZero, PXErrorLevel.Error));
            }
        }
    }
    #endregion
}
