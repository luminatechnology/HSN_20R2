using System;
using System.Collections;
using HSNCustomizations.DAC;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CM;
using System.Collections.Generic;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.GL;
using System.IO;
using System.Text;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using static PX.Data.PXAccess;
using BAccount = PX.Objects.CR.BAccount;
using Branch = PX.Objects.GL.Branch;
using System.Linq;

namespace HSNCustomizations.Graph
{
	public class ProcessCitiBankTTPaymentMaint : PXGraph<ProcessCitiBankTTPaymentMaint>
	{
		public PXFilter<LumProcessCitiBankPaymentFile> Filter;
		public PXCancel<LumProcessCitiBankPaymentFile> Cancel;
		public PXAction<LumProcessCitiBankPaymentFile> ViewDocument;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual IEnumerable viewDocument(PXAdapter adapter)
		{
			if (APPaymentList.Current != null)
			{
				PXRedirectHelper.TryRedirect(APPaymentList.Cache, APPaymentList.Current, "Document", PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		[PXFilterable]
		public PXFilteredProcessingJoin<APPayment, LumProcessCitiBankPaymentFile,
			InnerJoin<Vendor, On<Vendor.bAccountID, Equal<APPayment.vendorID>>>,
			Where<APPayment.released, Equal<True>, And<APPayment.docType, Equal<APDocType.check>, And<APPayment.status, Equal<APDocStatus.closed>>>>,
			OrderBy<Desc<APPayment.refNbr>>> APPaymentList;

		public PXSelect<CurrencyInfo> currencyinfo;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;

		#region Vendor Payment Details
		//Bank Swift Code
		private const string PaymentBankSwiftCode = "SWIFT";
		public class _PaymentBankSwiftCode : PX.Data.BQL.BqlString.Constant<_PaymentBankSwiftCode>
		{
			public _PaymentBankSwiftCode() : base(PaymentBankSwiftCode) { }
		}
		//Bank Number
		private const string PaymentBankNumber = "BANKBNR";
		public class _PaymentBankNumber : PX.Data.BQL.BqlString.Constant<_PaymentBankNumber>
		{
			public _PaymentBankNumber() : base(PaymentBankNumber) { }
		}
		//Bank Branch Number
		private const string PaymentBankBranchNumber = "BANKBRCH";
		public class _PaymentBankBranchNumber : PX.Data.BQL.BqlString.Constant<_PaymentBankBranchNumber>
		{
			public _PaymentBankBranchNumber() : base(PaymentBankBranchNumber) { }
		}
		//Bank Account Number
		private const string PaymentBankAccountNumber = "ACCTNBR";
		public class _PaymentBankAccountNumber : PX.Data.BQL.BqlString.Constant<_PaymentBankAccountNumber>
		{
			public _PaymentBankAccountNumber() : base(PaymentBankAccountNumber) { }
		}
		//Bank Account Name
		private const string PaymentBankAccountName = "BANKACCT";
		public class _PaymentBankAccountName : PX.Data.BQL.BqlString.Constant<_PaymentBankAccountName>
		{
			public _PaymentBankAccountName() : base(PaymentBankAccountName) { }
		}
		//Charge Indicator
		private const string PaymentCHGINDICAT = "CHGINDICAT";
		public class _PaymentCHGINDICAT : PX.Data.BQL.BqlString.Constant<_PaymentCHGINDICAT>
		{
			public _PaymentCHGINDICAT() : base(PaymentCHGINDICAT) { }
		}
		#endregion


		public ProcessCitiBankTTPaymentMaint()
		{
			//APSetup setup = APSetup.Current;
			PXUIFieldAttribute.SetEnabled(APPaymentList.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<APPayment.selected>(APPaymentList.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<APPayment.extRefNbr>(APPaymentList.Cache, null, true);

			APPaymentList.SetSelected<APPayment.selected>();

			APPaymentList.SetProcessVisible(false);
			//APPaymentList.SetProcessAllVisible(false);
			APPaymentList.SetProcessDelegate(list => DownlodAppayments(list));
		}

		#region Action
		public PXAction<LumProcessCitiBankPaymentFile> PrintAPPaymentRegister;
		[PXButton]
		[PXUIField(DisplayName = "Print Payment Register", Enabled = true, MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable printAPPaymentRegister(PXAdapter adapter)
		{
			var currentFiler = this.Caches[typeof(LumProcessCitiBankPaymentFile)].Cached.Cast<LumProcessCitiBankPaymentFile>().ToList();

			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters["AdjDate"] = ((DateTime)currentFiler[0].AdjDate).ToString("yyyy-MM-dd");
			parameters["PayAccountID"] = SelectFrom<CashAccount>.Where<CashAccount.accountID.IsEqual<@P.AsInt>>.View.Select(this, currentFiler[0].PayAccountID).TopFirst?.CashAccountCD;
			parameters["PayTypeID"] = currentFiler[0].PayTypeID;
			parameters["CuryID"] = currentFiler[0].CuryID;
			throw new PXReportRequiredException(parameters, "LM622505", "LM622505") { Mode = PXBaseRedirectException.WindowMode.New };
		}
		#endregion

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[APDocType.List]
		protected virtual void APPayment_DocType_CacheAttached(PXCache sender) { }

		private bool cleared;
		public override void Clear()
		{
			Filter.Current.CurySelTotal = 0m;
			Filter.Current.SelCount = 0;
			cleared = true;
			base.Clear();
		}

		private readonly Dictionary<object, object> _copies = new Dictionary<object, object>();

		protected virtual IEnumerable appaymentlist()
		{
			if (this.cleared)
			{
				foreach (APPayment doc in this.APPaymentList.Cache.Updated)
				{
					doc.Passed = false;
				}
			}

			
			foreach (PXResult<APPayment> doc in PXSelectJoin<APPayment,
					LeftJoin<Vendor, On<Vendor.bAccountID, Equal<APPayment.vendorID>>,
					LeftJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<APPayment.paymentMethodID>>>>,
						Where<APPayment.cashAccountID, Equal<Current<LumProcessCitiBankPaymentFile.payAccountID>>,
						And<APPayment.paymentMethodID, Equal<Current<LumProcessCitiBankPaymentFile.payTypeID>>,
						And<APPayment.adjDate, Equal<Current<LumProcessCitiBankPaymentFile.adjDate>>,
						And<APPayment.released, Equal<True>,
						And<APPayment.docType, Equal<APDocType.check>,
						And<APPayment.status, Equal<APDocStatus.closed>,
						And<Match<Vendor, Current<AccessInfo.userName>>>>>>>>>>.Select(this))
			{
				APPayment line = (APPayment)doc;

				var VendorBankNumber = SelectFrom<VendorPaymentMethodDetail>.
											Where<VendorPaymentMethodDetail.bAccountID.IsEqual<@P.AsInt>.And<VendorPaymentMethodDetail.locationID.IsEqual<@P.AsInt>.
											And<VendorPaymentMethodDetail.paymentMethodID.IsEqual<@P.AsString>.And<VendorPaymentMethodDetail.detailID.IsEqual<_PaymentBankNumber>>>>>.
											View.Select(this, line.VendorID, line.VendorLocationID, line.PaymentMethodID).TopFirst?.DetailValue;
				var VendorBankBranchNumber = SelectFrom<VendorPaymentMethodDetail>.
												Where<VendorPaymentMethodDetail.bAccountID.IsEqual<@P.AsInt>.And<VendorPaymentMethodDetail.locationID.IsEqual<@P.AsInt>.
												And<VendorPaymentMethodDetail.paymentMethodID.IsEqual<@P.AsString>.And<VendorPaymentMethodDetail.detailID.IsEqual<_PaymentBankBranchNumber>>>>>.
												View.Select(this, line.VendorID, line.VendorLocationID, line.PaymentMethodID).TopFirst?.DetailValue;

				if (VendorBankNumber != null && VendorBankBranchNumber != null)
					line.GetExtension<APPaymentExt>().UsrCitiBankNumber = string.Concat(VendorBankNumber.Length > 3 ? VendorBankNumber.Substring(0, 3) : VendorBankNumber,
																			VendorBankBranchNumber.Length > 4 ? VendorBankBranchNumber.Substring(0, 4) : VendorBankBranchNumber);
				else line.GetExtension<APPaymentExt>().UsrCitiBankNumber = "";

				line.GetExtension<APPaymentExt>().UsrCitiBankAccountNbr = SelectFrom<VendorPaymentMethodDetail>.
																			Where<VendorPaymentMethodDetail.bAccountID.IsEqual<@P.AsInt>.
																				And<VendorPaymentMethodDetail.locationID.IsEqual<@P.AsInt>.
																				And<VendorPaymentMethodDetail.paymentMethodID.IsEqual<@P.AsString>.
																				And<VendorPaymentMethodDetail.detailID.IsEqual<_PaymentBankAccountNumber>>>>>.
																			View.Select(this, line.VendorID, line.VendorLocationID, line.PaymentMethodID).TopFirst?.DetailValue;

				//TT need this condition, otherwise, none
				var curLumProcessCitiBankPaymentFile = this.Caches[typeof(LumProcessCitiBankPaymentFile)].Cached.RowCast<LumProcessCitiBankPaymentFile>().ToList();
				if (curLumProcessCitiBankPaymentFile[0].PayTypeID == "TT")
                {
					if (line.GetExtension<APPaymentExt>().UsrCitiBankAccountNbr != null) yield return new PXResult<APPayment>(doc);
				}					
				else yield return new PXResult<APPayment>(doc);
			}
		}
		protected virtual void _(Events.FieldUpdated<APPayment.selected> e)
        {
			var curLumProcessCitiBankPaymentFile = this.Caches[typeof(LumProcessCitiBankPaymentFile)].Cached.RowCast<LumProcessCitiBankPaymentFile>().ToList();

			var selectedAPPaymentList = this.Caches[typeof(APPayment)].Updated.Cast<APPayment>().Where(x => x.Selected ?? true).ToList();
			if (selectedAPPaymentList.Count > 0)
			{
				curLumProcessCitiBankPaymentFile[0].SelCount = selectedAPPaymentList.Count;
				curLumProcessCitiBankPaymentFile[0].CurySelTotal = selectedAPPaymentList.Select(x => x.CuryOrigDocAmt).Sum();
			}
            else
            {
				curLumProcessCitiBankPaymentFile[0].SelCount = 0;
				curLumProcessCitiBankPaymentFile[0].CurySelTotal = selectedAPPaymentList.Select(x => x.CuryOrigDocAmt).Sum();
			}
		}
		public void DownlodAppayments(IEnumerable<APPayment> aPPaymentLists)
		{
			try
			{
				using (MemoryStream stream = new MemoryStream())
				{
					using (StreamWriter sw = new StreamWriter(stream, Encoding.ASCII))
					{
						string fileName = DateTime.Now.ToString("yyyyMMddHHmm") + "-CitiTT.txt";

						foreach (APPayment aPPayment in aPPaymentLists)
						{
							string line = "";
							int count = 1;
							var currentBranch = SelectFrom<Branch>.Where<Branch.branchID.IsEqual<@P.AsInt>>.View.Select(this, aPPayment.BranchID).TopFirst;
							//3: ExtRefNbr
							var ExtRefNbr = SelectFrom<CashAccount>.Where<CashAccount.cashAccountID.IsEqual<@P.AsInt>>.View.Select(this, aPPayment.CashAccountID).TopFirst?.ExtRefNbr;
							//14: Companies.AccontName
							var curOrganizationInfo = SelectFrom<Organization>.View.Select(this).TopFirst;
							var CompanyInfo = SelectFrom<BAccount2>.Where<BAccount2.bAccountID.IsEqual<@P.AsInt>>.View.Select(this, curOrganizationInfo?.BAccountID).TopFirst;
							//15-17: Company Address
							var CompanyAddress = SelectFrom<Address>.
													Where<Address.bAccountID.IsEqual<@P.AsInt>>.
													View.Select(this, CompanyInfo.BAccountID).TopFirst;
							//20: Vendor Name
							var VendorInfo = SelectFrom<VendorR>.
													Where<VendorR.bAccountID.IsEqual<@P.AsInt>>.
													View.Select(this, aPPayment.VendorID).TopFirst;

							var VendorContact = SelectFrom<Contact>.
													Where<Contact.bAccountID.IsEqual<@P.AsInt>>.
													View.Select(this, VendorInfo.BAccountID).TopFirst;
							var RemitContact = SelectFrom<APContact>.
													Where<APContact.contactID.IsEqual<@P.AsInt>>.
													View.Select(this, aPPayment.RemitContactID).TopFirst;

							//21-23: APAddress
							//var APAddress = SelectFrom<APAddress>.Where<APAddress.addressID.IsEqual<@P.AsInt>>.View.Select(this, aPPayment.RemitAddressID).TopFirst;
							//Vendor Payment Details
							/*
							var VendorBankSwiftCode = SelectFrom<VendorPaymentMethodDetail>.
														Where<VendorPaymentMethodDetail.bAccountID.IsEqual<@P.AsInt>.
														And<VendorPaymentMethodDetail.locationID.IsEqual<@P.AsInt>.
														And<VendorPaymentMethodDetail.paymentMethodID.IsEqual<@P.AsString>.
														And<VendorPaymentMethodDetail.detailID.IsEqual<_PaymentBankSwiftCode>>>>>.
														View.Select(this, aPPayment.VendorID, aPPayment.VendorLocationID, aPPayment.PaymentMethodID).TopFirst;
							*/
							var VendorBankNumber = SelectFrom<VendorPaymentMethodDetail>.
													Where<VendorPaymentMethodDetail.bAccountID.IsEqual<@P.AsInt>.
													And<VendorPaymentMethodDetail.locationID.IsEqual<@P.AsInt>.
													And<VendorPaymentMethodDetail.paymentMethodID.IsEqual<@P.AsString>.
													And<VendorPaymentMethodDetail.detailID.IsEqual<_PaymentBankNumber>>>>>.
													View.Select(this, aPPayment.VendorID, aPPayment.VendorLocationID, aPPayment.PaymentMethodID).TopFirst;

							var VendorBankBranchNumber = SelectFrom<VendorPaymentMethodDetail>.
															Where<VendorPaymentMethodDetail.bAccountID.IsEqual<@P.AsInt>.
															And<VendorPaymentMethodDetail.locationID.IsEqual<@P.AsInt>.
															And<VendorPaymentMethodDetail.paymentMethodID.IsEqual<@P.AsString>.
															And<VendorPaymentMethodDetail.detailID.IsEqual<_PaymentBankBranchNumber>>>>>.
															View.Select(this, aPPayment.VendorID, aPPayment.VendorLocationID, aPPayment.PaymentMethodID).TopFirst;

							var VendorBankAccountNumber = SelectFrom<VendorPaymentMethodDetail>.
															Where<VendorPaymentMethodDetail.bAccountID.IsEqual<@P.AsInt>.
															And<VendorPaymentMethodDetail.locationID.IsEqual<@P.AsInt>.
															And<VendorPaymentMethodDetail.paymentMethodID.IsEqual<@P.AsString>.
															And<VendorPaymentMethodDetail.detailID.IsEqual<_PaymentBankAccountNumber>>>>>.
															View.Select(this, aPPayment.VendorID, aPPayment.VendorLocationID, aPPayment.PaymentMethodID).TopFirst;

							var VendorBankAccountName = SelectFrom<VendorPaymentMethodDetail>.
															Where<VendorPaymentMethodDetail.bAccountID.IsEqual<@P.AsInt>.
															And<VendorPaymentMethodDetail.locationID.IsEqual<@P.AsInt>.
															And<VendorPaymentMethodDetail.paymentMethodID.IsEqual<@P.AsString>.
															And<VendorPaymentMethodDetail.detailID.IsEqual<_PaymentBankAccountName>>>>>.
															View.Select(this, aPPayment.VendorID, aPPayment.VendorLocationID, aPPayment.PaymentMethodID).TopFirst;

							var VendorCHGINDICAT = SelectFrom<VendorPaymentMethodDetail>.
													Where<VendorPaymentMethodDetail.bAccountID.IsEqual<@P.AsInt>.
													And<VendorPaymentMethodDetail.locationID.IsEqual<@P.AsInt>.
													And<VendorPaymentMethodDetail.paymentMethodID.IsEqual<@P.AsString>.
													And<VendorPaymentMethodDetail.detailID.IsEqual<_PaymentCHGINDICAT>>>>>.
													View.Select(this, aPPayment.VendorID, aPPayment.VendorLocationID, aPPayment.PaymentMethodID).TopFirst;

							//1: PTA
							line = "PTA@";
							count++;
							//2: Companyaddress.CountryID
							line += $"{CompanyAddress.CountryID}@";
							count++;
							//3: CashAccount.ExtRefNbr, 35
							if (ExtRefNbr != null)
							{
								if (ExtRefNbr.Length > 35) line += $"{ExtRefNbr.Substring(0, 35)}@";
								else line += $"{ExtRefNbr}@";
							}
							else line += "@";
							count++;
							//4: APPayment.curyID
							line += $"{aPPayment.CuryID}@";
							count++;
							//5: APPayment.curyOrigDocAmt	*No thousands separator
							line += $"{Math.Round((Decimal)aPPayment.CuryOrigDocAmt, 2)}@";
							count++;
							//6: Null
							line += "@";
							count++;
							//7: APPayment.adjDate			*YYYYMMDD
							line += $"{((DateTime)aPPayment.AdjDate).ToString("yyyyMMdd")}@";
							count++;
							//8: APPayment.ExtRefNbr
							line += $"{aPPayment.ExtRefNbr}@";
							count++;
							//9-13: Null
							for (int i = count; i <= 13; i++)
							{
								line += "@";
								count++;
							}
							//14: CHGINDICAT = null or 'OUR', AcctName, otherwise LegalName. *Left(Companies.AccontName, 35)
							if (VendorCHGINDICAT?.DetailValue == null || VendorCHGINDICAT?.DetailValue == "OUR")
                            {
								line += "HIGHPOINT SERViCE (TH) - PTA@";
								/*
								if (CompanyInfo?.AcctName != null)
								{
									if (CompanyInfo?.AcctName.Length > 35) line += $"{CompanyInfo?.AcctName.Substring(0, 35).ToUpper()}@";
									else line += $"{CompanyInfo?.AcctName.ToUpper()}@";
								}
								else line += "@";
								*/
							}
							else //if (VendorCHGINDICAT?.DetailValue == "BEN")
                            {
								line += "HIGHPOINT SERViCE NETWORK - BEN@";
								/*
								if (CompanyInfo?.LegalName != null)
								{
									if (CompanyInfo?.LegalName.Length > 35) line += $"{CompanyInfo?.LegalName.Substring(0, 35).ToUpper()}@";
									else line += $"{CompanyInfo?.LegalName.ToUpper()}@";
								}
								else line += "@";
								*/
							}
							count++;
							//15-19: Null
							for (int i = count; i <= 19; i++)
							{
								line += "@";
								count++;
							}
							//20: Vendor Name, 70
							if (VendorInfo?.AcctName != null)
							{
								if (VendorInfo?.AcctName.Length > 70) line += $"{VendorInfo?.AcctName.Substring(0, 70).ToUpper()}@";
								else line += $"{VendorInfo?.AcctName.ToUpper()}@";
							}
							else line += "@";
							count++;
							//21-24: Null
							for (int i = count; i <= 24; i++)
							{
								line += "@";
								count++;
							}
							//25: Vendor Payment Detail: BANKACCNBR, 15
							if (VendorBankAccountNumber?.DetailValue != null)
							{
								if (VendorBankAccountNumber?.DetailValue.Length > 15) line += $"{VendorBankAccountNumber?.DetailValue.Substring(0, 15).ToUpper()}@";
								else line += $"{VendorBankAccountNumber?.DetailValue.ToUpper()}@";
							}
							else line += "@";
							count++;
							//26-31: Null
							for (int i = count; i <= 31; i++)
							{
								line += "@";
								count++;
							}
							//32: Vendor Payment Detail: Bank Number + Bank Branch Number, 7
							if (VendorBankNumber?.DetailValue != null && VendorBankBranchNumber?.DetailValue != null)
							{
								if ((VendorBankNumber?.DetailValue.Length + VendorBankBranchNumber?.DetailValue.Length) > 7) line += $"{VendorBankNumber?.DetailValue.Substring(0, 3).ToUpper()}{VendorBankBranchNumber?.DetailValue.Substring(0, 4).ToUpper()}@";
								else line += $"{VendorBankNumber?.DetailValue.ToUpper()}{VendorBankBranchNumber?.DetailValue.ToUpper()}@";
							}
							else line += "@";
							count++;
							//33-57: Null
							for (int i = count; i <= 57; i++)
							{
								line += "@";
								count++;
							}

							//58: PaymentMethodDetail.Chgindicat != null, or 'OUR', 35
							if (VendorCHGINDICAT?.DetailValue != null)
							{
								if (VendorCHGINDICAT.DetailValue.Length > 35) line += $"{VendorCHGINDICAT.DetailValue.Substring(0, 35).ToUpper()}@";
								else line += $"{VendorCHGINDICAT.DetailValue}@";
							}
							else line += "OUR@";
							count++;
							//59-75: Null
							for (int i = count; i <= 75; i++)
							{
								line += "@";
								count++;
							}
							//76: VendorContact.EMail, 50
							if (RemitContact?.Email != null)
							{
								if (RemitContact?.Email.Length > 50) line += $"{RemitContact?.Email.Substring(0, 50).ToUpper()}@";
								else line += $"{RemitContact?.Email.ToUpper()}@";
							}
							//if (VendorContact?.EMail != null)
							//{
							//	if (VendorContact?.EMail.Length > 50) line += $"{VendorContact?.EMail.Substring(0, 50).ToUpper()}@";
							//	else line += $"{VendorContact?.EMail.ToUpper()}@";
							//}
							else line += "@";
							count++;
							//77: Null
							line += "@";
							count++;
							//78: ='04'
							line += "04@";
							count++;
							//79-95: Null
							for (int i = count; i <= 95; i++)
							{
								line += "@";
								count++;
							}
							//96: APPayment.curyOrigDocAmt *2 decimal places, no thousand separator, 14
							line += $"{Math.Round((Decimal)aPPayment.CuryOrigDocAmt, 2)}@";
							count++;
							//97: 0
							line += "0@";
							count++;
							//98: 0
							line += "0@";
							count++;
							//99: 0
							line += "0@";
							count++;
							//100-114: Null cause ilne 100 contains one @
							for (int i = count; i < 113; i++)
							{
								line += "@";
								count++;
							}

							line += "\n";
							sw.Write(line);

							aPPayment.GetExtension<APPaymentExt>().UsrCitiPaymentExported = true;
							aPPayment.GetExtension<APPaymentExt>().UsrCitiPaymentDateTime = DateTime.Now;
							
							this.Caches[typeof(APPayment)].Update(aPPayment);
						}

						this.Actions.PressSave();
						sw.Close();

                        // Redirect browser to file created in memory on server
						throw new PXRedirectToFileException(new PX.SM.FileInfo(Guid.NewGuid(), fileName, null, stream.ToArray(), string.Empty), true);
					}
				}
			}
			catch (PXException ex)
			{
				PXProcessing<APPayment>.SetError(ex);
				throw;
			}
		}
	}
}