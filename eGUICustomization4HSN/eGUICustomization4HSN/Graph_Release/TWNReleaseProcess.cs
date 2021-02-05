using System;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.GL;
using PX.Objects.TX;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;
using eGUICustomization4HSN.Graph;
using eGUICustomization4HSN.StringList;

namespace eGUICustomization4HSN.Graph_Release
{
    public class TWNReleaseProcess : PXGraph<TWNReleaseProcess>, ITWNGUITran
    {
        public SelectFrom<TWNGUITrans>.View ViewGUITrans;

        #region Property
        public int SequenceNo { get; set; }
        #endregion

        #region Functions
        public TWNGUITrans InitAndCheckOnAP(string gUINbr, string vATInCode)
        {
            SequenceNo = (int)PXSelectGroupBy<TWNGUITrans,
                                              Where<TWNGUITrans.gUINbr, Equal<Required<TWNGUITrans.gUINbr>>,
                                                    And<TWNGUITrans.gUIFormatcode, Equal<Required<TWNGUITrans.gUIFormatcode>>>>,
                                              Aggregate<Count>>
                                              .Select(this, gUINbr, vATInCode).RowCount;

            TWNGUIValidation gUIValidation = new TWNGUIValidation();

            gUIValidation.CheckCorrespondingInv(this, gUINbr, vATInCode);

            return gUIValidation.tWNGUITrans;
        }

        public TWNGUITrans InitAndCheckOnAR(string gUINbr, string vATOutCode)
        {
            SequenceNo = (int)PXSelectGroupBy<TWNGUITrans,
                                              Where<TWNGUITrans.gUINbr, Equal<Required<TWNGUITrans.gUINbr>>,
                                                    And<TWNGUITrans.gUIFormatcode, Equal<Required<TWNGUITrans.gUIFormatcode>>>>,
                                              Aggregate<Count>>
                                              .Select(this, gUINbr, vATOutCode).RowCount;

            TWNGUIValidation gUIValidation = new TWNGUIValidation();

            gUIValidation.CheckCorrespondingInv(this, gUINbr, vATOutCode);

            return gUIValidation.tWNGUITrans;
        }

        public string GetBranchCD(int? branchID)
        {
            int parm = branchID is null ? base.Accessinfo.BranchID.Value : branchID.Value;

            Branch branch = SelectFrom<Branch>.Where<Branch.branchID.IsEqual<@P.AsInt>>
                                              .View.ReadOnly.SelectSingleBound(this, null, parm);

            return branch.BranchCD;
        }

        public string GetVATType(string taxID)
        {
            Tax tax = SelectFrom<Tax>.Where<Tax.taxID.IsEqual<@P.AsString>>.View.ReadOnly.SelectSingleBound(this, null, taxID);

            if (tax != null)
            {
                return tax.GetExtension<TaxExt>().UsrGUIType;
            }
            else
            { 
                return string.Empty; 
            }            
        }
       
        public string GetQREncrypter(STWNGUITran sGUITran)
        {
            com.tradevan.qrutil.QREncrypter qrEncrypter = new com.tradevan.qrutil.QREncrypter();

            string result;

            try
            {
                string[][] abc = new string[1][];

                TWNGUIPreferences gUIPreferences = PXSelect<TWNGUIPreferences>.Select(this);

                if (string.IsNullOrEmpty(gUIPreferences.AESKey))
                {
                    throw new MissingFieldException(string.Format("{0} {1}", nameof(TWNGUIPreferences.AESKey), PX.Data.InfoMessages.IsNull));
                }

                // a) Invoice number = GUITrans.GUINbr
                result = qrEncrypter.QRCodeINV(sGUITran.GUINbr,
                                               // b) Invoice date = GUITrans.GUIDate(If it is 2019 / 12 / 01, please change to 1081201.  107 = YYYY – 1911)
                                               TWNGenZeroTaxRateMedFile.GetTWNDate(sGUITran.GUIDate.Value),
                                               // c) Invoice time = “hhmmss” of GUITrans.GUIDate
                                               sGUITran.GUIDate.Value.ToString("hhmmss"),
                                               // d) Random number = If GUITrans.BatchNbr is not null then Right(Guitrans.bachNbrr,4) else Right(Guitrans.OrderNbrr, 4)
                                               string.IsNullOrEmpty(sGUITran.BatchNbr) ? sGUITran.BatchNbr.Substring(sGUITran.BatchNbr.Length - 4) : sGUITran.OrderNbr.Substring(sGUITran.OrderNbr.Length - 4),
                                               // e) Sales amount = GUITrans.Amount (No thousands separator, no decimal places)
                                               (int)sGUITran.NetAmount.Value,
                                               // f) Tax amount = GUITrans.Taxamount  (No thousands separator, no decimal places)
                                               (int)sGUITran.TaxAmount.Value,
                                               // g) Total amount = GUITrans.Amount + GUITrans.TaxAmount(No thousands separator, no decimal places)
                                               (int)(sGUITran.NetAmount + sGUITran.TaxAmount).Value,
                                               // h) Buyer identifier = GUITrans.TaxNbr(If it's blank or null, please use “00000000”)
                                               string.IsNullOrEmpty(sGUITran.TaxNbr) ? "00000000" : sGUITran.TaxNbr,
                                               // i) Representative identifier = “00000000”
                                               "00000000",
                                               // j) Sales identifier = GUITrans.OurTaxNbr
                                               sGUITran.OurTaxNbr,
                                               // k) Business identifier = GUITrans.OurTaxNbr
                                               sGUITran.OurTaxNbr,
                                               // l) AESKEY = GUIParameters.AESKEY
                                               gUIPreferences.AESKey);
            }
            catch
            {
                throw;
            }

            return result;
        }

        public void CreateGUITrans(STWNGUITran sGUITran)
        {
            TWNGUITrans row = ViewGUITrans.Cache.CreateInstance() as TWNGUITrans;

            row.GUIFormatcode = sGUITran.VATCode;
            row.GUINbr        = sGUITran.GUINbr;
            row.SequenceNo    = SequenceNo;

            row = ViewGUITrans.Insert(row);

            row.GUIStatus     = sGUITran.GUIStatus;
            row.Branch        = GetBranchCD(sGUITran.BranchID);
            row.GUIDirection  = sGUITran.GUIDirection;
            row.GUIDate       = row.GUIDecPeriod = sGUITran.GUIDate;
            row.GUITitle      = sGUITran.GUITitle;
            row.TaxZoneID     = sGUITran.TaxZoneID;
            row.TaxCategoryID = sGUITran.TaxCategoryID;
            row.TaxID         = sGUITran.TaxID;
            row.VATType       = GetVATType(row.TaxID);
            row.TaxNbr        = sGUITran.TaxNbr;
            row.OurTaxNbr     = sGUITran.OurTaxNbr;
            row.NetAmount     = row.NetAmtRemain = sGUITran.NetAmount;
            row.TaxAmount     = row.TaxAmtRemain = sGUITran.TaxAmount;
            row.CustVend      = sGUITran.AcctCD;
            row.CustVendName  = sGUITran.AcctName;
            row.DeductionCode = sGUITran.DeductionCode;
            row.TransDate     = base.Accessinfo.BusinessDate;
            row.EGUIExcluded  = sGUITran.eGUIExcluded;
            row.Remark        = sGUITran.Remark;
            row.BatchNbr      = sGUITran.BatchNbr;
            row.OrderNbr      = sGUITran.OrderNbr;
            row.CarrierType   = sGUITran.CarrierType;
            row.CarrierID     = sGUITran.CarrierID;
            row.NPONbr        = sGUITran.NPONbr;
            row.B2CPrinted    = sGUITran.B2CPrinted;
            row.QREncrypter   = sGUITran.GUIDirection.Equals(TWNGUIDirection.Issue) && sGUITran.NetAmount > 0 && sGUITran.eGUIExcluded.Equals(false) ? GetQREncrypter(sGUITran) : null;

            ViewGUITrans.Update(row);

            this.Actions.PressSave();
        }
        #endregion

        #region Static Method
        public static void UpdateGUIStatus(string gUINbr)
        {
            PXUpdate<Set<TWNGUITrans.gUIStatus, Required<TWNGUITrans.gUIStatus>>,
                     TWNGUITrans,
                     Where<TWNGUITrans.gUIStatus, Equal<TWNGUIStatus.used>,
                           And<TWNGUITrans.gUIDirection, Equal<TWNGUIDirection.issue>,
                               And<TWNGUITrans.sequenceNo, Equal<Zero>,
                                   And<TWNGUITrans.gUINbr, Equal<Required<TWNGUITrans.gUINbr>>>>>>>
                                   .Update(new PXGraph(), TWNGUIStatus.Voided, gUINbr);
        }
        #endregion
    }

    public interface ITWNGUITran
    {
        int SequenceNo { get; set; }
    }

    public struct STWNGUITran
    {
        public string VATCode;
        public string GUINbr;
        public string GUIDirection;
        public string GUIStatus;
        public string GUITitle;
        public string TaxZoneID;
        public string TaxCategoryID;
        public string TaxID;
        public string TaxNbr;
        public string AcctCD;
        public string AcctName;
        public string OurTaxNbr;
        public string DeductionCode;
        public string Remark;
        public string BatchNbr;
        public string OrderNbr;
        public string CarrierType;
        public string CarrierID;
        public string NPONbr;
        public string QREncrypter;
        
        public int? BranchID;

        public bool eGUIExcluded;
        public bool B2CPrinted;

        public DateTime? GUIDate;

        public decimal? NetAmount;
        public decimal? TaxAmount;

        //public STWNGUITran(string a, string b, string c, string d )
        //{
        //    VATCode = a;
        //    GUINbr = b;
        //    GUIDirection = c;
        //    GUIStatus = d;
        //}
    }
}
