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
	public class ProcessSCBPaymentMaint : PXGraph<ProcessSCBPaymentMaint>
	{
		public PXFilter<LumProcessSCBPaymentFile> Filter;
		public PXCancel<LumProcessSCBPaymentFile> Cancel;
		public PXAction<LumProcessSCBPaymentFile> ViewDocument;
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
		public PXFilteredProcessingJoin<APPayment, LumProcessSCBPaymentFile,
			InnerJoin<Vendor, On<Vendor.bAccountID, Equal<APPayment.vendorID>>>,
			Where<APPayment.released, Equal<True>>,
			OrderBy<Desc<APPayment.refNbr>>> APPaymentList;

		public PXSelect<CurrencyInfo> currencyinfo;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;

		#region Vendor AttributeID
		//Bank Swift Code
		private const string BankSwiftCodeAttr = "BANKSWIFT";
		public class _BankSwiftCodeAttr : PX.Data.BQL.BqlString.Constant<_BankSwiftCodeAttr>
		{
			public _BankSwiftCodeAttr() : base(BankSwiftCodeAttr) { }
		}
		//Bank Account Number
		private const string BankAccountNumberAttr = "BANKACCNBR";
		public class _BankAccountNumberAttr : PX.Data.BQL.BqlString.Constant<_BankAccountNumberAttr>
		{
			public _BankAccountNumberAttr() : base(BankAccountNumberAttr) { }
		}
		//Bank Account Name
		private const string BankAccountNameAttr = "BANKACCNAM";
		#endregion


		public ProcessSCBPaymentMaint()
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
		public PXAction<LumProcessSCBPaymentFile> PrintAPPaymentRegister;
		[PXButton]
		[PXUIField(DisplayName = "Print Payment Register", Enabled = true, MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable printAPPaymentRegister(PXAdapter adapter)
		{
			var currentFiler = this.Caches[typeof(LumProcessSCBPaymentFile)].Cached.Cast<LumProcessSCBPaymentFile>().ToList();

			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters["AdjDate"] = ((DateTime)currentFiler[0].AdjDate).ToString("yyyy-MM-dd");
			parameters["PayAccountID"] = SelectFrom<CashAccount>.Where<CashAccount.accountID.IsEqual<@P.AsInt>>.View.Select(this, currentFiler[0].PayAccountID).TopFirst?.CashAccountCD;
			parameters["PayTypeID"] = currentFiler[0].PayTypeID;
			parameters["CuryID"] = currentFiler[0].CuryID;
			throw new PXReportRequiredException(parameters, "LM622500", "LM622500") { Mode = PXBaseRedirectException.WindowMode.New };

			/* Old version
			var selectedAPPaymentList = this.Caches[typeof(APPayment)].Updated.Cast<APPayment>().Where(x => x.Selected ?? true).ToList();
			this.Actions.PressSave();
			
			PXReportRequiredException ex = null;
			foreach (var aPPayment in selectedAPPaymentList)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["RefNbr"] = aPPayment.RefNbr;

				if (ex == null)
				{
					ex = new PXReportRequiredException(parameters, "LM622500", "LM622500");
				}
				else
				{
					ex.AddSibling("LM622500", parameters, false);
				}
			}
			if (ex != null) throw ex;
			*/
			return adapter.Get();
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
						Where<APPayment.cashAccountID, Equal<Current<LumProcessSCBPaymentFile.payAccountID>>,
						And<APPayment.paymentMethodID, Equal<Current<LumProcessSCBPaymentFile.payTypeID>>,
						And<APPayment.adjDate, Equal<Current<LumProcessSCBPaymentFile.adjDate>>,
						And<APPayment.released, Equal<True>,
						And<Match<Vendor, Current<AccessInfo.userName>>>>>>>>.Select(this))
			{
				APPayment line = (APPayment)doc;
				line.GetExtension<APPaymentExt>().UsrBankSwiftAttributes = SelectFrom<CSAnswers>.
																			LeftJoin<Vendor>.On<CSAnswers.refNoteID.IsEqual<Vendor.noteID>>.
																			Where<Vendor.bAccountID.IsEqual<@P.AsInt>.And<CSAnswers.attributeID.IsEqual<_BankSwiftCodeAttr>>>.
																			View.Select(this, line.VendorID).TopFirst?.Value;
				line.GetExtension<APPaymentExt>().UsrBankAccountNbr = SelectFrom<CSAnswers>.
																		LeftJoin<Vendor>.On<CSAnswers.refNoteID.IsEqual<Vendor.noteID>>.
																		Where<Vendor.bAccountID.IsEqual<@P.AsInt>.And<CSAnswers.attributeID.IsEqual<_BankAccountNumberAttr>>>.
																		View.Select(this, line.VendorID).TopFirst?.Value;
				if (line.GetExtension<APPaymentExt>().UsrBankAccountNbr != null) yield return new PXResult<APPayment>(doc);
			}
		}
		protected virtual void _(Events.FieldUpdated<APPayment.selected> e)
        {
			var curLumProcessSCBPaymentFile = this.Caches[typeof(LumProcessSCBPaymentFile)].Cached.RowCast<LumProcessSCBPaymentFile>().ToList();

			var selectedAPPaymentList = this.Caches[typeof(APPayment)].Updated.Cast<APPayment>().Where(x => x.Selected ?? true).ToList();
			if (selectedAPPaymentList.Count > 0)
			{
				curLumProcessSCBPaymentFile[0].SelCount = selectedAPPaymentList.Count;
				curLumProcessSCBPaymentFile[0].CurySelTotal = selectedAPPaymentList.Select(x => x.CuryOrigDocAmt).Sum();
			}
            else
            {
				curLumProcessSCBPaymentFile[0].SelCount = 0;
				curLumProcessSCBPaymentFile[0].CurySelTotal = selectedAPPaymentList.Select(x => x.CuryOrigDocAmt).Sum();
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
						string fileName = DateTime.Now.ToString("yyyyMMddHHmm") + "-SCB.txt";

						foreach (APPayment aPPayment in aPPaymentLists)
						{
							string line = "";
							int count = 1;
							var currentBranch = SelectFrom<Branch>.Where<Branch.branchID.IsEqual<@P.AsInt>>.View.Select(this, aPPayment.BranchID).TopFirst;
							//3: ExtRefNbr
							var ExtRefNbr = SelectFrom<CashAccount>.Where<CashAccount.cashAccountID.IsEqual<@P.AsInt>>.View.Select(this, aPPayment.CashAccountID).TopFirst?.ExtRefNbr;
							//14: Companies.AccontName
							var CompanyInfo = SelectFrom<BAccount2>.Where<BAccount2.bAccountID.IsEqual<@P.AsInt>>.View.Select(this, currentBranch?.BAccountID).TopFirst;
							//15-17: Company Address
							var CompanyAddress = SelectFrom<Address>.
													Where<Address.bAccountID.IsEqual<@P.AsInt>>.
													View.Select(this, CompanyInfo.BAccountID).TopFirst;
							//21-23: APAddress
							var APAddress = SelectFrom<APAddress>.Where<APAddress.addressID.IsEqual<@P.AsInt>>.View.Select(this, aPPayment.RemitAddressID).TopFirst;
							//20, 25, 32: Vendor attribute
							var VendorBankSwiftCodeAttr = SelectFrom<CSAnswers>.
															LeftJoin<Vendor>.On<Vendor.bAccountID.IsEqual<@P.AsInt>>.
															Where<CSAnswers.refNoteID.IsEqual<Vendor.noteID>.And<CSAnswers.attributeID.IsEqual<@P.AsString>>>.
															View.Select(this, aPPayment.VendorID, BankSwiftCodeAttr).TopFirst;

							var VendorBankAccountNumberAttr = SelectFrom<CSAnswers>.
																LeftJoin<Vendor>.On<Vendor.bAccountID.IsEqual<@P.AsInt>>.
																Where<CSAnswers.refNoteID.IsEqual<Vendor.noteID>.And<CSAnswers.attributeID.IsEqual<@P.AsString>>>.
																View.Select(this, aPPayment.VendorID, BankAccountNumberAttr).TopFirst;

							var VendorBankAccountNameAttr = SelectFrom<CSAnswers>.
																LeftJoin<Vendor>.On<Vendor.bAccountID.IsEqual<@P.AsInt>>.
																Where<CSAnswers.refNoteID.IsEqual<Vendor.noteID>.And<CSAnswers.attributeID.IsEqual<@P.AsString>>>.
																View.Select(this, aPPayment.VendorID, BankAccountNameAttr).TopFirst;

							//1: PLG
							line = "PLG|";
							count++;
							//2: Null
							line += "|";
							count++;
							//3: If CashAccount.ExtRefNbr<> blank then CashAccount.ExtRefNbr Else '312143582508'
							if (ExtRefNbr != null) line += ExtRefNbr + "|";
							else line += "312143582508|";
							count++;
							//4: APPayment.curyID
							line += aPPayment.CuryID + "|";
							count++;
							//5: APPayment.curyOrigDocAmt	*No thousands separator
							line += Math.Round((Decimal)aPPayment.CuryOrigDocAmt, 2) + "|";
							count++;
							//6: Null
							line += "|";
							count++;
							//7: APPayment.adjDate			*YYYYMMDD
							line += ((DateTime)aPPayment.AdjDate).ToString("yyyyMMdd") + "|";
							count++;
							//8: APPayment.refNbr
							line += aPPayment.RefNbr + "|";
							count++;
							//9-13: Null
							for (int i = count; i <= 13; i++)
							{
								line += "|";
								count++;
							}
							//14: Left(Companies.AccontName, 30)
							if (CompanyInfo?.AcctName != null)
							{
								if (CompanyInfo?.AcctName.Length > 30) line += CompanyInfo?.AcctName.Substring(0, 30).ToUpper() + "|";
								else line += CompanyInfo?.AcctName.ToUpper() + "|";
							}
							else line += "|";
							count++;
							//15: Left(CompaniesAddress.Addressline1,30)
							if (CompanyAddress?.AddressLine1 != null)
							{ 
								if (CompanyAddress?.AddressLine1.Length > 30) line += CompanyAddress?.AddressLine1.Substring(0, 30).ToUpper() + "|";
								else line += CompanyAddress?.AddressLine1.ToUpper() + "|";
							} 
							else line += "|";
							count++;
							//16: Left(CompaniesAddress.Addressline2 + ．, ・+ Postalcode,30)
							if (CompanyAddress?.AddressLine2 != null && CompanyAddress?.PostalCode != null)
                            {
								if ((CompanyAddress?.AddressLine2 + ", " + CompanyAddress?.PostalCode).Length > 30) line += (CompanyAddress?.AddressLine2 + ", " + CompanyAddress?.PostalCode).Substring(0, 30).ToUpper() + "|";
								else line += (CompanyAddress?.AddressLine2 + ", " + CompanyAddress?.PostalCode).ToUpper() + "|";
							}
							else if (CompanyAddress?.AddressLine2 != null && CompanyAddress?.PostalCode == null)
							{
								if (CompanyAddress?.AddressLine2.Length > 30) line += (CompanyAddress?.AddressLine2).Substring(0, 30).ToUpper() + "|";
								else line += (CompanyAddress?.AddressLine2).ToUpper() + "|";
							}
							else if (CompanyAddress?.AddressLine2 == null && CompanyAddress?.PostalCode != null) line += (CompanyAddress?.PostalCode).ToUpper() + "|";
							else line += "|";
							count++;
							//17: Left(CompaniesAddress.City+・, ．+State,30)
							if (CompanyAddress?.City != null && CompanyAddress?.State != null)
							{
								if ((CompanyAddress?.City + ", " + CompanyAddress?.State).Length > 30) line += (CompanyAddress?.City + ", " + CompanyAddress?.State).Substring(0, 30).ToUpper() + "|";
								else line += (CompanyAddress?.City + ", " + CompanyAddress?.State).ToUpper() + "|";
							}
							else if (CompanyAddress?.City != null && CompanyAddress?.State == null)
							{
								if ((CompanyAddress?.City).Length > 30) line += (CompanyAddress?.City).Substring(0, 30).ToUpper() + "|";
								else line += (CompanyAddress?.City).ToUpper() + "|";
							}
							else if (CompanyAddress?.City == null && CompanyAddress?.State != null)
							{
								if ((CompanyAddress?.State).Length > 30) line += (CompanyAddress?.State).Substring(0, 30).ToUpper() + "|";
								else line += (CompanyAddress?.State).ToUpper() + "|";
							} 
							else line += "|";
							count++;
							//18: Null
							line += "|";
							count++;
							//19: Null
							line += "|";
							count++;
							//20: Left(Vendor attribute BANKACCNAM, 30)
							if (VendorBankAccountNameAttr?.Value != null)
                            {
								if (VendorBankAccountNameAttr?.Value.Length > 30) line += VendorBankAccountNameAttr?.Value.Substring(0, 30).ToUpper() + "|";
								else line += VendorBankAccountNameAttr?.Value.ToUpper() + "|";
							}
							else line += "|";
							count++;
							//21: Left(APAddress.Addressline1,30)
							if (APAddress?.AddressLine1 != null)
                            {
								if (APAddress?.AddressLine1.Length > 30) line += APAddress?.AddressLine1.Substring(0, 30).ToUpper() + "|";
								else line += APAddress?.AddressLine1.ToUpper() + "|";
							}
							else line += "|";
							count++;
							//22: Left(APAddress.Addressline2 + ．, ・+ Postalcode,30)
							if (APAddress?.AddressLine2 != null && APAddress?.PostalCode != null)
							{
								if ((APAddress?.AddressLine2 + ", " + APAddress?.PostalCode).Length > 30) line += (APAddress?.AddressLine2 + ", " + APAddress?.PostalCode).Substring(0, 30).ToUpper() + "|";
								else line += (APAddress?.AddressLine2 + ", " + APAddress?.PostalCode).ToUpper() + "|";
							}
							else if (APAddress?.AddressLine2 != null && APAddress?.PostalCode == null)
							{
								if (APAddress?.AddressLine2.Length > 30) line += (APAddress?.AddressLine2).Substring(0, 30).ToUpper() + "|";
								else line += (APAddress?.AddressLine2).ToUpper() + "|";
							}
							else if (APAddress?.AddressLine2 == null && APAddress?.PostalCode != null)
							{
								if (APAddress?.PostalCode.Length > 30) line += (APAddress?.PostalCode).Substring(0, 30).ToUpper() + "|";
								else line += (APAddress?.PostalCode).ToUpper() + "|";
							}
							else line += "|";
							count++;
							//23: Left(APAddress.City+・, ．+State,30)
							if (APAddress?.City != null && APAddress?.State != null)
                            {
								if ((APAddress?.City + ", " + APAddress?.State).Length > 30) line += (APAddress?.City + ", " + APAddress?.State).ToUpper() + "|";
								else line += (APAddress?.City + ", " + APAddress?.State).ToUpper() + "|";
							}
							else if (APAddress?.City != null && APAddress?.State == null)
							{
								if ((APAddress?.City).Length > 30) line += (APAddress?.City).ToUpper() + "|";
								else line += (APAddress?.City).ToUpper() + "|";
							}
							else if (APAddress?.City == null && APAddress?.State != null)
                            {
								if ((APAddress?.State).Length > 30) line += (APAddress?.State).ToUpper() + "|";
								else line += (APAddress?.State).ToUpper() + "|";
							}
							else line += "|";
							count++;
							//24: Null
							line += "|";
							count++;
							//25: Vendor attribute BANKACCNBR
							if (VendorBankAccountNumberAttr?.Value != null) line += VendorBankAccountNumberAttr?.Value + "|";
							else line += "|";
							count++;
							//26-31: Null
							for (int i = count; i <= 31; i++)
							{
								line += "|";
								count++;
							}
							//32: Vendor attribute BANKSWIFT
							if (VendorBankSwiftCodeAttr?.Value != null) line += VendorBankSwiftCodeAttr?.Value + "|";
							else line += "|";
							count++;
							//33-114: Null
							for (int i = count; i < 114; i++)
							{
								line += "|";
								count++;
							}

							line += "\n";
							sw.Write(line);

							//Update UsrSCBPaymentExported and UsrSCBPaymentDateTime
							aPPayment.GetExtension<APPaymentExt>().UsrSCBPaymentExported = true;
							aPPayment.GetExtension<APPaymentExt>().UsrSCBPaymentDateTime = DateTime.Now;
							
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