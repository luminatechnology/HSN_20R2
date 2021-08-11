using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNCustomizations.Descriptor
{
    public class LUMGetAvailablePaymentsAttribute : PXCustomSelectorAttribute
    {
        public LUMGetAvailablePaymentsAttribute() : base(typeof(PaymentInfo.refNbr),
            typeof(PaymentInfo.bAccountID),
            typeof(PaymentInfo.extRefNbr),
            typeof(PaymentInfo.cashAccountID),
            typeof(PaymentInfo.curyOrigDocAmt))
        { }

        public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e) { }

        protected virtual IEnumerable GetRecords()
        {
            PXSelectBase<PX.Objects.CA.Light.ARPayment> paymentSelect = new PXSelectJoin<PX.Objects.CA.Light.ARPayment,
                                    LeftJoin<CADepositDetail, On<CADepositDetail.origDocType, Equal<PX.Objects.CA.Light.ARPayment.docType>,
                                        And<CADepositDetail.origRefNbr, Equal<PX.Objects.CA.Light.ARPayment.refNbr>,
                                        And<CADepositDetail.origModule, Equal<PX.Objects.GL.BatchModule.moduleAR>,
                                        And<CADepositDetail.tranType, Equal<CATranType.cADeposit>>>>>,
                                    LeftJoin<CADeposit, On<CADeposit.tranType, Equal<CADepositDetail.tranType>,
                                        And<CADeposit.refNbr, Equal<CADepositDetail.refNbr>>>,
                                    InnerJoin<CashAccountDeposit, On<CashAccountDeposit.depositAcctID, Equal<PX.Objects.CA.Light.ARPayment.cashAccountID>,
                                    And<Where<CashAccountDeposit.paymentMethodID, Equal<PX.Objects.CA.Light.ARPayment.paymentMethodID>,
                                        Or<CashAccountDeposit.paymentMethodID, Equal<PX.Objects.BQLConstants.EmptyString>>>>>,
                                    InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<PX.Objects.CA.Light.ARPayment.paymentMethodID>>>>>>,
                                    Where<CashAccountDeposit.accountID, Equal<Current<CADeposit.cashAccountID>>,
                                    And<PX.Objects.CA.Light.ARPayment.released, Equal<True>,
                                    And<PX.Objects.CA.Light.ARPayment.curyOrigDocAmt, NotEqual<Zero>,
                                    And<PX.Objects.CA.Light.ARPayment.depositAsBatch, Equal<True>,
                                    And<PX.Objects.CA.Light.ARPayment.depositNbr, IsNull,
                                    And<Where<CADepositDetail.refNbr, IsNull, Or<CADeposit.voided, Equal<True>>>>>>>>>,
                                    OrderBy<Asc<ARPayment.docType, Asc<PX.Objects.CA.Light.ARPayment.refNbr, Desc<CashAccountDeposit.paymentMethodID>>>>>(this._Graph);
            PX.Objects.CA.Light.ARPayment last = null;
            foreach (PXResult<PX.Objects.CA.Light.ARPayment, CADepositDetail, CADeposit, CashAccountDeposit, PaymentMethod> it in paymentSelect.Select())
            {
                PX.Objects.CA.Light.ARPayment payment = it;
                CADepositDetail detail = it;
                PaymentMethod pmDef = it;
                if (pmDef.ARVoidOnDepositAccount == false && (payment.Voided == true || ARPaymentType.AllVoidingTypes.Contains(payment.DocType))) continue; //Skip voided and voiding Documents
                if (last != null && last.DocType == payment.DocType && last.RefNbr == payment.RefNbr) continue; //Skip duplicates 
                last = payment;
                PaymentInfo paymentInfo = PXGraph.CreateInstance<CADepositEntry>().Copy(payment, new PaymentInfo());
                yield return paymentInfo;
            }
        }
    }
}
