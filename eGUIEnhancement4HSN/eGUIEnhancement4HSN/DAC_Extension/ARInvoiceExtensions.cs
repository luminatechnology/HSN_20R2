using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CA;

namespace PX.Objects.AR
{
	public class ARInvoiceExt : PXCacheExtension<ARInvoice>
    {
		#region UsrPaymMethodID	
		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.paymentMethodID,
										   InnerJoin<Customer, On<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>,
													 And<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
														 And<CustomerPaymentMethod.isActive, Equal<True>>>>,
										   InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
													 And<PaymentMethod.useForAR, Equal<True>,
														 And<PaymentMethod.isActive, Equal<True>>>>>>,
										   Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>,
								   Search2<Customer.defPaymentMethodID, InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<Customer.defPaymentMethodID>,
												And<PaymentMethod.useForAR, Equal<True>,
												And<PaymentMethod.isActive, Equal<True>>>>>,
										 Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search5<PaymentMethod.paymentMethodID, LeftJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
									And<CustomerPaymentMethod.bAccountID, Equal<Current<ARInvoice.customerID>>>>>,
								Where<PaymentMethod.isActive, Equal<True>,
								And<PaymentMethod.useForAR, Equal<True>,
								And<Where<PaymentMethod.aRIsOnePerCustomer, Equal<True>,
									Or<Where<CustomerPaymentMethod.pMInstanceID, IsNotNull>>>>>>, Aggregate<GroupBy<PaymentMethod.paymentMethodID>>>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXUIFieldAttribute(DisplayName = "Payment Method")]
		[PXForeignReference(typeof(Field<ARInvoice.paymentMethodID>.IsRelatedTo<PaymentMethod.paymentMethodID>))]
		public virtual string UsrPaymMethodID
		{
			get
			{
				return Base.PaymentMethodID;
			}
			set
			{
				Base.PaymentMethodID = value;
			}
		}
		public abstract class usrPaymMethodID : PX.Data.BQL.BqlString.Field<usrPaymMethodID> { }
		#endregion
	}
}
