using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.CS;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;
using eGUICustomization4HSN.StringList;

namespace eGUICustomization4HSN.Graph
{
    public class TWNGenGUIMediaFile : PXGraph<TWNGenGUIMediaFile>
    {
        #region Variable Defintions
        public char   zero      = '0';
        public char   space     = ' ';       
        public int    fixedLen  = 0;
        public string ourTaxNbr = "";
        public string combinStr = "";
        #endregion

        #region Features & Setup
        public PXCancel<GUITransFilter> Cancel; 
        public PXFilter<GUITransFilter> Filter;
        public PXFilteredProcessing<TWNGUITrans, GUITransFilter,
                                    Where<TWNGUITrans.gUIDecPeriod, LessEqual<Current<GUITransFilter.toDate>>,
                                          And<TWNGUITrans.gUIDecPeriod, GreaterEqual<Current<GUITransFilter.fromDate>>>>> GUITransList;
        public PXSetup<TWNGUIPreferences> gUIPreferSetup;
        #endregion

        #region Delegate Data View
        public IEnumerable gUITransList()
        {
            GUITransFilter filter = Filter.Current;

            foreach (TWNGUITrans row in SelectFrom<TWNGUITrans>.Where<TWNGUITrans.gUIDecPeriod.IsLessEqual<@P.AsDateTime>
                                                                      .And<TWNGUITrans.gUIDecPeriod.IsGreaterEqual<@P.AsDateTime>>>.View.Select(this, filter.ToDate.Value.AddDays(1).Date.AddSeconds(-1), filter.FromDate))
            {
                yield return row;
            }
        }
        #endregion

        #region Constructor
        public TWNGenGUIMediaFile()
        {
            GUITransList.SetProcessCaption(ActionsMessages.Export);
            GUITransList.SetProcessAllCaption(TWMessages.ExportAll);
            GUITransList.SetProcessDelegate(Export);
        }
        #endregion

        #region Constant String Classes
        public const string fixedGUI2 = "GUI2%";
        public const string fixedGUI3 = "GUI3%";

        public class GUI2x : PX.Data.BQL.BqlString.Constant<GUI2x>
        {
            public GUI2x() : base(fixedGUI2) { }
        }

        public class GUI3x : PX.Data.BQL.BqlString.Constant<GUI3x>
        {
            public GUI3x() : base(fixedGUI3) { }
        }
        #endregion

        #region Event Handler
        protected void _(Events.FieldUpdated<GUITransFilter.toDate> e)
        {
            e.Cache.SetValue<GUITransFilter.toDate>(e.Row, DateTime.Parse(e.NewValue.ToString()).AddDays(1).Date.AddSeconds(-1) );
        }
        #endregion

        #region Functions
        public void Export(List<TWNGUITrans> tWNGUITrans)
        {
            try
            {
                TWNGUIPreferences gUIPreferences = gUIPreferSetup.Current;

                int    count = 1;
                string lines = "", fileName = "";

                using (MemoryStream stream = new MemoryStream())
                {
                    using (StreamWriter sw = new StreamWriter(stream, Encoding.ASCII))
                    {
                        fileName = gUIPreferences.OurTaxNbr + ".txt";

                        foreach (TWNGUITrans gUITrans in tWNGUITrans)
                        {
                            ourTaxNbr = gUITrans.OurTaxNbr;

                            // Reporting Code
                            lines = gUITrans.GUIFormatcode;
                            // Tax Registration
                            lines += gUIPreferences.TaxRegistrationID;
                            // Sequence Number
                            lines += AutoNumberAttribute.GetNextNumber(GUITransList.Cache, gUITrans, gUIPreferences.MediaFileNumbering, Accessinfo.BusinessDate);
                            // GUI LegalYM
                            lines += GetGUILegal(gUITrans.GUIDate.Value);
                            // Tax ID (Buyer)
                            lines += GetBuyerTaxID(gUITrans);
                            // Tax ID (Seller)
                            lines += GetSellerTaxID(gUITrans);
                            // GUI Number
                            lines += GetGUINbr(gUITrans);
                            // Net Amount
                            lines += GetNetAmt(gUITrans);
                            // Tax Group
                            lines += GetTaxGroup(gUITrans);
                            // Tax Amount
                            lines += GetTaxAmt(gUITrans);
                            // Deduction Code
                            lines += (gUITrans.DeductionCode != null || gUITrans.GUIFormatcode.StartsWith("2")) ? gUITrans.DeductionCode : new string(space, 1);
                            // Blank
                            lines += new string(space, 5);
                            // Special Tax Rate
                            lines += new string(space, 1);
                            // Summary Remark
                            lines += GetSummaryRemark(gUITrans);
                            // Export Method
                            lines += GetExportMethod(gUITrans);

                            // Only the last line does not need to be broken.
                            if (count < tWNGUITrans.Count)
                            {
                                sw.WriteLine(lines);
                                count++;
                            }
                            else
                            {
                                sw.Write(lines);
                            }
                        }

                        count = 1;
                        PXSelectBase<NumberingSequence> query = new PXSelect<NumberingSequence,
                                                                             Where<NumberingSequence.numberingID, Like<GUI2x>,
                                                                                   Or<NumberingSequence.numberingID, Like<GUI3x>,
                                                                                      And<NumberingSequence.startDate, GreaterEqual<Current<GUITransFilter.fromDate>>,
                                                                                          And<NumberingSequence.startDate, LessEqual<Current<GUITransFilter.toDate>>>>>>>(this);

                        foreach (NumberingSequence numSeq in query.Select())
                        {
                            int endNbr  = Int32.Parse(numSeq.EndNbr.Substring(2));
                            int lastNbr = Int32.Parse(numSeq.LastNbr.Substring(2));

                            if (numSeq.StartNbr.Equals(numSeq.LastNbr) || lastNbr <= endNbr)
                            {

                                lines = "\r\n";
                                // Reporting Code
                                lines += numSeq.NumberingID.Substring(numSeq.NumberingID.IndexOf('I') + 1, 2);
                                // Tax Registration
                                lines += gUIPreferences.TaxRegistrationID;
                                // Sequence Number
                                lines += AutoNumberAttribute.GetNextNumber(GUITransList.Cache, numSeq, gUIPreferences.MediaFileNumbering, Accessinfo.BusinessDate);
                                // GUI LegalYM
                                lines += GetGUILegal(Filter.Current.ToDate.Value);
                                // Tax ID (Buyer)
                                lines += numSeq.EndNbr.Substring(2);
                                // Tax ID (Seller)
                                lines += ourTaxNbr;
                                // GUI Number
                                lines += string.Format("{0}{1}", numSeq.LastNbr.Substring(0, 2), lastNbr + 1);
                                // Net Amount
                                lines += new string(zero, 12);
                                // Tax Group
                                lines += "D";
                                // Tax Amount
                                lines += new string(zero, 10);
                                // Deduction Code
                                lines += new string(space, 1);
                                // Blank
                                lines += new string(space, 5);
                                // Special Tax Rate
                                lines += new string(space, 1);
                                // Summary Remark
                                lines += "A";
                                // Export Method
                                lines += new string(space, 1);

                                sw.Write(lines);
                            }
                        }

                        sw.Close();

                        // Redirect browser to file created in memory on server
                        throw new PXRedirectToFileException(new PX.SM.FileInfo(Guid.NewGuid(), fileName, 
                                                                               null, stream.ToArray(), string.Empty), 
                                                            true);
                    }
                }
            }
            catch (PXException ex)
            {
                PXProcessing<TWNGUITrans>.SetError(ex);
                throw;
            }
        }

        public string GetGUILegal(DateTime dateTime)
        {
            var tWCalendar = new System.Globalization.TaiwanCalendar();

            int    year  = tWCalendar.GetYear(dateTime);
            string month = DateTime.Parse(dateTime.ToString()).ToString("MM");

            return string.Format("{0}{1}", year, month);
        }

        public string GetNetAmt(TWNGUITrans tWNGUITrans)
        {
            fixedLen = 12;

            bool sellerFmtCode = false;
            bool buyerFmtCode  = false;

            string sellerTaxID = GetSellerTaxID(tWNGUITrans);
            string buyerTaxID  = GetBuyerTaxID(tWNGUITrans);

            if (tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATOutCode32) ||
                tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATInCode22) ||
                tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATInCode27) ) 
            { sellerFmtCode = true; }

            if (tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATOutCode31) ||
                tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATOutCode35) )
            { buyerFmtCode = true; }

            if (tWNGUITrans.GUIStatus.Equals(TWNGUIStatus.Voided) )
            {
                combinStr = new string(zero, fixedLen);
            }
            else if (sellerFmtCode ||
                     (tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATInCode25) && sellerTaxID.Length != 8) ||
                     (buyerFmtCode && (string.IsNullOrEmpty(buyerTaxID) || buyerTaxID == new string(space, 8)))
                    )
            {
                combinStr = (tWNGUITrans.NetAmount + tWNGUITrans.TaxAmount).ToString();
            }
            else
            {
                combinStr = tWNGUITrans.NetAmount.ToString();
            }

            return combinStr.PadLeft(fixedLen, zero);
        }

        public string GetGUINbr(TWNGUITrans tWNGUITrans)
        {
            if (tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATInCode28) )
            {
                return tWNGUITrans.GUINbr.Substring(4, tWNGUITrans.GUINbr.Length - 4);
            }
            else
            {
                return tWNGUITrans.GUINbr.Substring(0, 10);
            }
        }

        private string GetBuyerTaxID(TWNGUITrans tWNGUITrans)
        {
            if (tWNGUITrans.GUIFormatcode.StartsWith("2"))
            {
                return tWNGUITrans.OurTaxNbr;
            }
            else if (tWNGUITrans.GUIFormatcode.StartsWith("3") &&
                     tWNGUITrans.GUIStatus != TWNGUIStatus.Voided)
            {
                return tWNGUITrans.TaxNbr ?? new string(space, 8);
            }

            return new string(space, 8);
        }

        private string GetSellerTaxID(TWNGUITrans tWNGUITrans)
        {
            if (tWNGUITrans.GUIFormatcode.StartsWith("2"))
            {
                if (tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATInCode28) )
                {
                    return (new string(space, 4) + tWNGUITrans.GUINbr.Substring(0, 4));
                }
                else if (tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATInCode26) || tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATInCode27) )
                {
                    return tWNGUITrans.TaxNbr + new string(space, (8 - tWNGUITrans.TaxNbr.Length));
                }
                else
                {
                    return tWNGUITrans.TaxNbr ?? new string(space, 8);
                }
            }

            return tWNGUITrans.OurTaxNbr;
        }

        private string GetTaxGroup(TWNGUITrans tWNGUITrans)
        {
            if      (tWNGUITrans.GUIStatus.Equals(TWNGUIStatus.Voided)) { return "F"; }
            else if (tWNGUITrans.VATType.Equals(TWNGUIVATType.Five))    { return "1"; }
            else if (tWNGUITrans.VATType.Equals(TWNGUIVATType.Zero))    { return "2"; }
            else if (tWNGUITrans.VATType.Equals(TWNGUIVATType.Exclude)) { return "3"; }
            else                                                        { return new string(space, 1); }
        }

        private string GetTaxAmt(TWNGUITrans tWNGUITrans)
        {
            fixedLen = 10;

            bool fixedFmtCode  = false;
            bool sellerFmtCode = false;
            bool buyerFmtCode  = false;

            string buyerTaxID = GetBuyerTaxID(tWNGUITrans);

            if (tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATOutCode32) ||
                tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATInCode22) ||
                tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATInCode27) ) 
            { fixedFmtCode = true; }

            if (tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATInCode25) &&
                GetSellerTaxID(tWNGUITrans).Length != 8) 
            { sellerFmtCode = true; }

            if ((tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATOutCode31) && (string.IsNullOrEmpty(buyerTaxID) || buyerTaxID == new string(space, 8)) ) ||
                (tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATOutCode35) && (string.IsNullOrEmpty(buyerTaxID) || buyerTaxID == new string(space, 8)) )
               ) { buyerFmtCode = true; }

            if (tWNGUITrans.GUIStatus.Equals(TWNGUIStatus.Voided) ||
                fixedFmtCode || sellerFmtCode || buyerFmtCode)
            {
                return new string(zero, fixedLen);
            }
            else
            {
                combinStr = tWNGUITrans.TaxAmount.ToString();

                return combinStr.PadLeft(fixedLen, zero);
            }
        }

        private string GetSummaryRemark(TWNGUITrans tWNGUITrans)
        {
            if (tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATInCode26) ||
                tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATInCode27) )
            {
                return "A";
            }
            else if (tWNGUITrans.GUIFormatcode.Equals(TWGUIFormatCode.vATInCode25) && GetSellerTaxID(tWNGUITrans).Length != 8)
            {
                return "A";
            }
            else
            {
                return new string(space, 1);
            }
        }

        private string GetExportMethod(TWNGUITrans tWNGUITrans)
        {
            if (tWNGUITrans.VATType.Equals(TWNGUIVATType.Zero) &&
                tWNGUITrans.CustomType == TWNGUICustomType.NotThruCustom &&
                tWNGUITrans.GUIStatus != TWNGUIStatus.Voided &&
                tWNGUITrans.SequenceNo.Equals(0) )
            {
                return "1";
            }
            else if (tWNGUITrans.VATType.Equals(TWNGUIVATType.Zero) &&
                     tWNGUITrans.CustomType == TWNGUICustomType.ThruCustom &&
                     tWNGUITrans.GUIStatus != TWNGUIStatus.Voided &&
                     tWNGUITrans.SequenceNo.Equals(0) )
            {
                return "2";
            }
            else
            {
                return new string(space, 1);
            }
        }
        #endregion
    }

    #region Filter DAC
    [Serializable]
    [PXCacheName("GUI Trans Filter DAC")]
    public partial class GUITransFilter : IBqlTable
    {
        public class ToDateWithMaxTime : PX.Data.BQL.BqlDateTime.Constant<ToDateWithMaxTime> 
        {
            public ToDateWithMaxTime() : base(DateTime.UtcNow.AddDays(1).Date.AddSeconds(-1)) { }
        }

        #region FromDate
        [PXDBDate(PreserveTime = true)]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "From Date", Visibility = PXUIVisibility.Visible)]
        public virtual DateTime? FromDate { get; set; }
        public abstract class fromDate : IBqlField { }
        #endregion

        #region ToDate
        [PXDBDate(PreserveTime = true)]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "To Date", Visibility = PXUIVisibility.Visible)]
        public virtual DateTime? ToDate { get; set; }
        public abstract class toDate : IBqlField { }
        #endregion
    }
    #endregion
}