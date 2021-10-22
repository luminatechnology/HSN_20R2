using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.GL;

namespace HSNCustomizations.DAC
{
    [Serializable]
    [PXCacheName("LumProcessCitiBankPaymentFile")]
    public class LumProcessCitiBankPaymentFile : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[PXDefault(typeof(AccessInfo.branchID))]
		[Branch(Visible = true, Enabled = true)]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion

		#region PayTypeID
		public abstract class payTypeID : PX.Data.BQL.BqlString.Field<payTypeID> { }
		[PXDBString(10, IsUnicode = true)]
		//[PXDefault("TT")]
		//[PXDefault(typeof(Search<Location.paymentMethodID,
		//					Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>, And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
						  Where<PaymentMethod.useForAP, Equal<True>,
							And<PaymentMethod.isActive, Equal<True>>>>))]
		[PXForeignReference(typeof(Field<payTypeID>.IsRelatedTo<PaymentMethod.paymentMethodID>))]
		public virtual string PayTypeID { get; set; }
		#endregion

		#region PayAccountID
		public abstract class payAccountID : PX.Data.BQL.BqlInt.Field<payAccountID> { }
		protected Int32? _PayAccountID;
		//[PXDefault(typeof(Coalesce<Search2<Location.cashAccountID,
		//					InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<Location.cashAccountID>,
		//						And<PaymentMethodAccount.paymentMethodID, Equal<Location.vPaymentMethodID>,
		//							And<PaymentMethodAccount.useForAP, Equal<True>>>>>,
		//					Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>,
		//						And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>,
		//							And<Location.vPaymentMethodID, Equal<Current<APPayment.paymentMethodID>>>>>>,
		//	Search2<PaymentMethodAccount.cashAccountID, InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>,
		//		Where<PaymentMethodAccount.paymentMethodID, Equal<Current<APPayment.paymentMethodID>>,
		//			And<CashAccount.branchID, Equal<Current<APPayment.branchID>>,
		//				And<PaymentMethodAccount.useForAP, Equal<True>, And<PaymentMethodAccount.aPIsDefault, Equal<True>>>>>>>),
		//	PersistingCheck = PXPersistingCheck.Nothing)]
		[CashAccount(typeof(APPayment.branchID), typeof(Search2<CashAccount.cashAccountID,
                            InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
                            Where2<Match<Current<AccessInfo.userName>>,
                                And<PaymentMethodAccount.paymentMethodID, Equal<Current<payTypeID>>,
                                And<PaymentMethodAccount.useForAP, Equal<True>,
                                    And<Where<CashAccount.clearingAccount, Equal<False>, Or<Current<APPayment.docType>, In3<APDocType.refund, APDocType.voidRefund>>>>>>>>),
            Visibility = PXUIVisibility.Visible, SuppressCurrencyValidation = true)]
		[PXDefault(typeof(Search2<PaymentMethodAccount.cashAccountID,
							InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>,
										Where<PaymentMethodAccount.paymentMethodID, Equal<Current<LumProcessCitiBankPaymentFile.payTypeID>>,
											And<PaymentMethodAccount.useForAP, Equal<True>,
											And<PaymentMethodAccount.aPIsDefault, Equal<True>,
											And<CashAccount.branchID, Equal<Current<AccessInfo.branchID>>>>>>>))]
		public virtual Int32? PayAccountID
		{
			get
			{
				return this._PayAccountID;
			}
			set
			{
				this._PayAccountID = value;
			}
		}
		#endregion

		#region Balance
		public abstract class balance : PX.Data.BQL.BqlDecimal.Field<balance> { }
		protected Decimal? _Balance;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? Balance
		{
			get
			{
				return this._Balance;
			}
			set
			{
				this._Balance = value;
			}
		}
		#endregion

		#region CurySelTotal
		public abstract class curySelTotal : PX.Data.BQL.BqlDecimal.Field<curySelTotal> { }
		protected Decimal? _CurySelTotal;
		[PXDBDecimal()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Selection Total", Enabled = false)]
		public virtual Decimal? CurySelTotal { get; set; }
		#endregion

		#region SelCount
		public abstract class selCount : PX.Data.BQL.BqlInt.Field<selCount> { }
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Number of Payments", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual int? SelCount { get; set; }
		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<LumProcessCitiBankPaymentFile.payAccountID>>>>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(ModuleCode = BatchModule.AP)]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion

		#region CashBalance
		public abstract class cashBalance : PX.Data.BQL.BqlDecimal.Field<cashBalance> { }
		protected Decimal? _CashBalance;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(LumProcessCitiBankPaymentFile.curyID))]
		[PXUIField(DisplayName = "Available Balance", Enabled = false)]
		[CashBalance(typeof(LumProcessCitiBankPaymentFile.payAccountID))]
		public virtual Decimal? CashBalance
		{
			get
			{
				return this._CashBalance;
			}
			set
			{
				this._CashBalance = value;
			}
		}
		#endregion

		#region PayFinPeriodID
		public abstract class payFinPeriodID : PX.Data.BQL.BqlString.Field<payFinPeriodID> { }
		protected string _PayFinPeriodID;
		[FinPeriodID(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.Visible)]
		public virtual String PayFinPeriodID
		{
			get
			{
				return this._PayFinPeriodID;
			}
			set
			{
				this._PayFinPeriodID = value;
			}
		}
		#endregion

		#region GLBalance
		public abstract class gLBalance : PX.Data.BQL.BqlDecimal.Field<gLBalance> { }
		protected decimal? _GLBalance;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(curyID))]
		[PXUIField(DisplayName = "GL Balance", Enabled = false)]
		[GLBalance(typeof(payAccountID), typeof(payFinPeriodID))]
		public virtual decimal? GLBalance
		{
			get
			{
				return this._GLBalance;
			}
			set
			{
				this._GLBalance = value;
			}
		}
		#endregion

		#region Application Date
		public abstract class adjDate : PX.Data.BQL.BqlDateTime.Field<adjDate> { }
		protected DateTime? _AdjDate;
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Application Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? AdjDate
		{
			get
			{
				return this._AdjDate;
			}
			set
			{
				this._AdjDate = value;
			}
		}
		#endregion
	}
}