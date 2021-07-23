using System.Collections;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.AR
{
    public class ARPaymentEntry_Extension : PXGraphExtension<ARPaymentEntry>
    {
        public override void Initialize()
        {
            base.Initialize();
            Base.report.AddMenuAction(PrepaymentInvoice);
        }

        #region Action
        public PXAction<ARPayment> PrepaymentInvoice;
        [PXButton]
        [PXUIField(DisplayName = "Print Prepayment Invoice", Enabled = true, MapEnableRights = PXCacheRights.Select)]
        protected virtual IEnumerable prepaymentInvoice(PXAdapter adapter)
        {
            if (Base.Document.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["DocType"] = Base.Document.Current.DocType;
                parameters["RefNbr"] = Base.Document.Current.RefNbr;
                throw new PXReportRequiredException(parameters, "LM602000", "Report LM602000");
            }
            return adapter.Get();
        }
        #endregion

        #region Cache Attached 
        [PXRemoveBaseAttribute(typeof(ARPaymentType.NumberingAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [HSNCustomizations.Descriptor.ARPymtNumbering()]
        protected void _(Events.CacheAttached<ARPayment.refNbr> e) { }
        #endregion
    }
}