using PX.Data;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;
using eGUICustomization4HSN.Graph_Release;
using static PX.Objects.AR.ARReleaseProcess_Extension2;

namespace eGUICustomization4HSN.Graph
{
    public class eGUIInquiry_Extension : PXGraphExtension<eGUIInquiry>
    {
        #region Delegate Actions
        public PXAction<TWNGUITrans> PatchPrint;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = TWMessages.PatchPrint)]
        public void patchPrint()
        {
            eGUIInquiry.ButtonValidation(Base.ViewGUITrans.Current);

            PXGraph.CreateInstance<eGUIInquiry2>().PrintReport(Base.ARTranView.Select(Base.ViewGUITrans.Current.OrderNbr), Base.ViewGUITrans.Current, false);
        }

        public PXAction<TWNGUITrans> RePrint;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = TWMessages.RePrint)]
        public void rePrint()
        {
            eGUIInquiry.ButtonValidation(Base.ViewGUITrans.Current);

            PXGraph.CreateInstance<eGUIInquiry2>().PrintReport(Base.ARTranView.Select(Base.ViewGUITrans.Current.OrderNbr), Base.ViewGUITrans.Current, true);
        }

        public PXAction<TWNGUITrans> CreateQR;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Create QR")]
        public void createQR()
        {
            TWNGUITrans gUITran = Base.ViewGUITrans.Current;

            gUITran.QREncrypter = PXGraph.CreateInstance<TWNReleaseProcess>().GetQREncrypter(new STWNGUITran() 
                                                                                             { 
                                                                                                 GUINbr    = gUITran.GUINbr,
                                                                                                 GUIDate   = gUITran.GUIDate,
                                                                                                 BatchNbr  = gUITran.BatchNbr,
                                                                                                 OrderNbr  = gUITran.OrderNbr,
                                                                                                 NetAmount = gUITran.NetAmount,
                                                                                                 TaxAmount = gUITran.TaxAmount,
                                                                                                 TaxNbr    = gUITran.TaxNbr,
                                                                                                 OurTaxNbr = gUITran.OurTaxNbr
                                                                                             });
            
            Base.ViewGUITrans.Cache.Update(gUITran);
        }
        #endregion
    }
}