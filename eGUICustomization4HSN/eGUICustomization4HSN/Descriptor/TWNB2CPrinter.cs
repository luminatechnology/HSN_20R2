using System.Text;
using System.Collections.Generic;
using eInvoiceLib;
using PX.Objects.AR;

namespace eGUICustomization4HSN.Descriptor
{
    public /*static*/ class TWNB2CPrinter
    {
        public static string GetPrinter { get; set; }

        public static void PrintOnRP100(List<string> header, bool rePrint, List<ARTran> result)
        {
            // Register BIG5 Code
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ESCPOS eSCPOS = new ESCPOS();

            eSCPOS.StartLPTPrinter(GetPrinter, TWMessages.eGUI); // Printer name & Task name in printing

            //using (Bitmap pLogo = (Bitmap)Image.FromFile(@"C:\HSNLOGO.bmp", true))
            //{
            //    eSCPOS.LoadImageToPrinter(pLogo);
            //}

            new AIDA_P1226D(eSCPOS).PrintInvoice(header[0], header[1], header[2], header[3], header[4], header[5],
                                                 header[6], header[7], header[8], rePrint, header[9], header[10]);

            eSCPOS.SendTo("退貨憑電子發票證明聯正本辦理\n");
            eSCPOS.SendTo("----------------------------\n");

            // If GUITrans.TaxNbr is blank / null(二聯式), all price are tax included.
            // If GUITrans.TaxNbr is not blank (三聯式), all price are tax excluded.We need to print out tax separately.
            // Cut the paper to separate header and details.
            if (string.IsNullOrEmpty(header[9]))
            {
                eSCPOS.CutPaper(0x42, 0x00);
            }
            eSCPOS.Align(0);
            // Font size
            eSCPOS.SelectCharSize(0x0E); // 0X0E -> 14
            // Print details
            eSCPOS.SendTo("\t銷售明細表\n");
            eSCPOS.SendTo(string.Format("名稱: {0}\n", header[11]));
            eSCPOS.SendTo(string.Format("發票號碼: {0}\n", header[4].Trim(new char[] { '-' })));
            eSCPOS.SendTo("品名/數量   單價       金額\n");

            decimal netAmt = 0;
            ARRegister register = new ARRegister();

            foreach (ARTran aRTran in result)
            {
                eSCPOS.SendTo(string.Format("{0}\n", aRTran.TranDesc));
                eSCPOS.SendTo(string.Format("  {0:N0}   {1:N2}   {2:N2}\n",
                                            aRTran.Qty,
                                            string.IsNullOrEmpty(header[9]) ? aRTran.UnitPrice * (decimal)1.05 : aRTran.UnitPrice,
                                            string.IsNullOrEmpty(header[9]) ? aRTran.CuryExtPrice * (decimal)1.05 : aRTran.CuryExtPrice));

                netAmt += string.IsNullOrEmpty(header[9]) ? aRTran.CuryExtPrice.Value * (decimal)1.05 : aRTran.CuryExtPrice.Value;

                //register = PXSelectReadonly<ARRegister,
                //                                Where<ARRegister.docType, Equal<Required<ARTran.tranType>>,
                //                                      And<ARRegister.refNbr, Equal<Required<ARTran.refNbr>>>>>.Select(new PXGraph(), aRTran.TranType, aRTran.RefNbr);
            }

            eSCPOS.SendTo(string.Format("共 {0} 項\n", result.Count));

            if (string.IsNullOrEmpty(header[9]) == false)
            {
                eSCPOS.SendTo(string.Format("銷售額:{0:N0}\n", netAmt));
                eSCPOS.SendTo(string.Format("稅  額:{0:N0}\n", decimal.Parse(header[6]) - netAmt));
            }

            eSCPOS.SendTo(string.Format("總  計:{0:N0}\n", header[6]));
            eSCPOS.SendTo(string.Format("課稅別:{0}\n", header[12]));
            eSCPOS.SendTo(string.Format("備  註:{0}\n", header[13]));

            if (string.IsNullOrEmpty(header[9]) == false)
            {
                eSCPOS.SendTo(string.Format("採購號碼:{0}\n", header[14]));
            }

            eSCPOS.SendTo(string.Format("發票開立部門:{0}\n", header[15]));
            //if (PXCacheEx.GetExtension<ARRegisterExt2>(register).UsrPrnPayment.Equals(true))
            //{
            eSCPOS.SendTo(string.Format("付款方式:{0}\n", header[16]));
            //}

            if (string.IsNullOrEmpty(header[9]) == false)// && PXCacheEx.GetExtension<ARRegisterExt2>(register).UsrPrnGUITitle.Equals(true))
            {
                eSCPOS.SendTo(string.Format("發票抬頭:{0}\n", header[17]));
            }
            // Don’t print 發票費用類別

            // Cut the the end of invoice
            eSCPOS.CutPaper(0x42, 0x00);

            eSCPOS.EndLPTPrinter();
        }
    } 
}
