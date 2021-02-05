using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;
using eGUICustomization4HSN.StringList;

namespace eGUICustomization4HSN.Graph
{
    public class TWNGenZeroTaxRateMedFile : PXGraph<TWNGenZeroTaxRateMedFile>
    {
        #region Features & Setup
        public PXCancel<GUITransFilter> Cancel;
        public PXFilter<GUITransFilter> FilterGUITran;
        public PXFilteredProcessing<TWNGUITrans, GUITransFilter,
                                    Where<TWNGUITrans.gUIDecPeriod, GreaterEqual<Current<GUITransFilter.fromDate>>,
                                          And<TWNGUITrans.gUIDecPeriod, LessEqual<Current<GUITransFilter.toDate>>,
                                              And<TWNGUITrans.vATType, Equal<TWNGUIVATType.zero>,
                                                  And<TWNGUITrans.gUIStatus, NotEqual<TWNGUIStatus.voided>,
                                                      And<Where<TWNGUITrans.gUIFormatcode, NotEqual<ARRegisterExt.VATOut34Att>,
                                                                Or<TWNGUITrans.gUIFormatcode, NotEqual<ARRegisterExt.VATOut33Att>>>>>>>>> GUITranProc;
        public PXSetup<TWNGUIPreferences> gUIPreferSetup;
        #endregion

        #region Constructor
        public TWNGenZeroTaxRateMedFile()
        {
            GUITranProc.SetProcessCaption(ActionsMessages.Export);
            GUITranProc.SetProcessAllCaption(TWMessages.ExportAll);
            GUITranProc.SetProcessDelegate(Export);

            //GUITranProc.SetProcessDelegate<TWNGenZeroTaxRateMedFile>(delegate (TWNGenZeroTaxRateMedFile graph, TWNGUITrans order)
            //{
            //    try
            //    {
            //        graph.Clear();
            //        graph.Export(order);
            //    }
            //    catch (Exception e)
            //    {
            //        PXProcessing<TWNGUITrans>.SetError(e);
            //    }
            //});
        }
        #endregion

        #region Event Handler
        protected void _(Events.FieldUpdated<GUITransFilter.toDate> e)
        {
            e.Cache.SetValue<GUITransFilter.toDate>(e.Row, DateTime.Parse(e.NewValue.ToString()).AddDays(1).Date.AddSeconds(-1));
        }
        #endregion

        #region Function
        public void Export(List<TWNGUITrans> tWNGUITrans)
        {
            try
            {
                TWNGenGUIMediaFile genGUIMediaFile = PXGraph.CreateInstance<TWNGenGUIMediaFile>();
                TWNGUIPreferences  gUIPreferences  = gUIPreferSetup.Current;

                int    count = 1;
                string lines = "", fileName = "", ticketType = "X7";

                using (MemoryStream ms = new MemoryStream())
                {
                    using (StreamWriter sw = new StreamWriter(ms, Encoding.ASCII))
                    {
                        fileName = gUIPreferences.OurTaxNbr + ".t02";

                        foreach (TWNGUITrans gUITrans in tWNGUITrans)
                        {
                            // Tax ID
                            lines =  gUIPreferences.OurTaxNbr;
                            // Country No
                            lines += gUIPreferences.ZeroTaxTaxCntry;
                            // Tax Registration ID
                            lines += gUIPreferences.TaxRegistrationID;
                            // Tax Filling Date
                            lines += genGUIMediaFile.GetGUILegal(FilterGUITran.Current.ToDate.Value);
                            // GUI Date
                            lines += genGUIMediaFile.GetGUILegal(gUITrans.GUIDate.Value);
                            // GUI Number
                            lines += genGUIMediaFile.GetGUINbr(gUITrans);
                            // Customer Tax ID
                            lines += gUITrans.TaxNbr ?? new string(genGUIMediaFile.space, 8);
                            // Export Method
                            lines += gUITrans.ExportMethods;                               
                            // Custom Method
                            lines += gUITrans.CustomType;
                            // Ticket Type
                            // Ticket Number
                            if (gUITrans.ExportTicketType == ticketType)
                            {
                                lines += new String(genGUIMediaFile.space, 2);
                                lines += new String(genGUIMediaFile.space, 14);
                            }
                            else
                            {
                                lines += gUITrans.ExportTicketType;
                                lines += gUITrans.ExportTicketNbr;
                            }
                            // Amount
                            lines += genGUIMediaFile.GetNetAmt(gUITrans);
                            // Custom Clearing Date
                            lines += GetTWNDate(gUITrans.ClearingDate.Value);

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

                        //Write to file
                        //FixedLengthFile flatFile = new FixedLengthFile();
                        //flatFile.WriteToFile(recordList, sw);
                        //sw.Flush();

                        sw.Close();

                        PX.SM.FileInfo info = new PX.SM.FileInfo(fileName, null, ms.ToArray());

                        throw new PXRedirectToFileException(info, true);
                    }
                }
            }
            catch (Exception ex)
            {
                PXProcessing<TWNGUITrans>.SetError(ex);
                throw;
            }
        }
        #endregion

        #region Static Method
        public static string GetTWNDate(DateTime dateTime)
        {
            var tWCalendar = new System.Globalization.TaiwanCalendar();

            int year     = tWCalendar.GetYear(dateTime);
            string month = DateTime.Parse(dateTime.ToString()).ToString("MM");
            string day   = DateTime.Parse(dateTime.ToString()).ToString("dd");

            return string.Format("{0}{1}{2}", year, month, day);
        }
        #endregion
    }
}