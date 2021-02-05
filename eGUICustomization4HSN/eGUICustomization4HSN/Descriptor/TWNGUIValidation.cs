using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Data.Licensing;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.StringList;
using Branch = PX.Objects.GL.Branch;

namespace eGUICustomization4HSN.Descriptor
{
    public class TWNGUIValidation
    {
        #region Variables & table buffer
        public int errorLevel      = 0;
        public bool isCreditNote   = false;
        public bool notBeDeleted   = false;
        public bool errorOccurred  = false;
        public string errorMessage = string.Empty;
        
        public TWNGUITrans tWNGUITrans = null;
        #endregion

        #region Static Method
        public static bool ActivateTWGUI(PXGraph graph)
        {
            if (graph.Accessinfo.BranchID == null) { return false; }

            Address address = SelectFrom<Address>.InnerJoin<BAccount>.On<BAccount.bAccountID.IsEqual<Address.bAccountID>
                                                                        .And<BAccount.defAddressID.IsEqual<Address.addressID>>>
                                                  .InnerJoin<Branch>.On<Branch.bAccountID.IsEqual<BAccount.bAccountID>>
                                                  .Where<Branch.branchID.IsEqual<@P.AsInt>>.View.ReadOnly.Select(graph, graph.Accessinfo.BranchID);

            return address.CountryID.Equals("TW"); // Hard code country ID in condition.
        }
        #endregion

        #region Functions
        public virtual void CheckGUINbrExisted(PXGraph graph, string gUINbr, string gUIFmtCode)
        {
            if (SelectFrom<TWNGUITrans>.Where<TWNGUITrans.gUINbr.IsEqual<@P.AsString>
                                             .And<TWNGUITrans.gUIFormatcode.IsEqual<@P.AsString>>
                                                  .And<TWNGUITrans.gUIStatus.IsEqual<TWNGUIStatus.used>>>.View.ReadOnly.Select(graph, gUINbr, gUIFmtCode).Count > 0)
            {
                throw new PXSetPropertyException(TWMessages.GUINbrExisted, PXErrorLevel.RowError);
            }
        }

        public virtual void CheckCorrespondingInv(PXGraph graph, string gUINbr, string gUIFmtCode)
        {
            isCreditNote = false;

            switch (gUIFmtCode)
            {
                case TWGUIFormatCode.vATInCode23 :
                    tWNGUITrans = SelectFrom<TWNGUITrans>.Where<TWNGUITrans.gUINbr.IsEqual<@P.AsString>
                                                                .And<Where<TWNGUITrans.gUIFormatcode.IsEqual<@P.AsString>
                                                                          .Or<TWNGUITrans.gUIFormatcode.IsEqual<@P.AsString>>>>>
                                                         .View.ReadOnly.Select(graph, gUINbr, TWGUIFormatCode.vATInCode21, TWGUIFormatCode.vATInCode25);
                    isCreditNote = (tWNGUITrans == null);
                    break;

                case TWGUIFormatCode.vATInCode24 :
                    tWNGUITrans = SelectFrom<TWNGUITrans>.Where<TWNGUITrans.gUINbr.IsEqual<@P.AsString>
                                                                .And<TWNGUITrans.gUIFormatcode.IsEqual<@P.AsString>>>
                                                         .View.ReadOnly.Select(graph, gUINbr, TWGUIFormatCode.vATInCode22);
                    isCreditNote = (tWNGUITrans == null);
                    break;

                case TWGUIFormatCode.vATOutCode33:
                    tWNGUITrans = SelectFrom<TWNGUITrans>.Where<TWNGUITrans.gUINbr.IsEqual<@P.AsString>
                                                                .And<Where<TWNGUITrans.gUIFormatcode.IsEqual<@P.AsString>
                                                                          .Or<TWNGUITrans.gUIFormatcode.IsEqual<@P.AsString>>>>>
                                                         .View.ReadOnly.Select(graph, gUINbr, TWGUIFormatCode.vATOutCode31 ,TWGUIFormatCode.vATOutCode35);
                    isCreditNote = (tWNGUITrans == null);
                    break;

                case TWGUIFormatCode.vATOutCode34:
                    tWNGUITrans = SelectFrom<TWNGUITrans>.Where<TWNGUITrans.gUINbr.IsEqual<@P.AsString>
                                                                .And<TWNGUITrans.gUIFormatcode.IsEqual<@P.AsString>>>
                                                         .View.ReadOnly.Select(graph, gUINbr, TWGUIFormatCode.vATOutCode32);
                    isCreditNote = (tWNGUITrans == null);
                    break;
            }

            if (isCreditNote.Equals(true))
            {
                errorOccurred = true;
                errorMessage  = TWMessages.CNIsNotFound;
            }
        }

        public virtual void CheckTaxAmount(decimal netAmt, decimal taxAmt)
        {
            const decimal fivePercent = (decimal)0.05;

            if ((netAmt * fivePercent) - taxAmt > 1 && netAmt != 0)
            {
                throw new PXSetPropertyException(TWMessages.TaxAmtIsWrong, PXErrorLevel.Warning);
            }
        }

        public virtual void CheckTabNbr(string taxNbr)
        {
            if (taxNbr.Length < 8)
            {
                errorLevel    = (int)PXErrorLevel.Error;
                errorOccurred = true;
                errorMessage  = TWMessages.TaxNbrLenNot8;
            }
            else
            {
                int total = 0,
                    num_1 = int.Parse(taxNbr.Substring(0, 1)) * 1,
                    num_2 = int.Parse(taxNbr.Substring(1, 1)) * 2,
                    num_3 = int.Parse(taxNbr.Substring(2, 1)) * 1,
                    num_4 = int.Parse(taxNbr.Substring(3, 1)) * 2,
                    num_5 = int.Parse(taxNbr.Substring(4, 1)) * 1,
                    num_6 = int.Parse(taxNbr.Substring(5, 1)) * 2,
                    num_7 = int.Parse(taxNbr.Substring(6, 1)) * 4,
                    num_8 = int.Parse(taxNbr.Substring(7, 1)) * 1;

                bool is7 = taxNbr.Substring(6, 1) == "7";

                if (is7 == true) { num_7 = (num_7 / 10) + (num_7 % 10); }

                int[,] array = new int[8, 2] { {num_1 / 10, num_1 % 10},
                                               {num_2 / 10, num_2 % 10},
                                               {num_3 / 10, num_3 % 10},
                                               {num_4 / 10, num_4 % 10},
                                               {num_5 / 10, num_5 % 10},
                                               {num_6 / 10, num_6 % 10},
                                               {num_7 / 10, num_7 % 10},
                                               {num_8 / 10, num_8 % 10} 
                                             };

                for (int i = 0; i < array.GetLength(0); i++)
                {
                    total += array[i, 0] + array[i, 1];
                }

                if (is7 == true && total % 10 > 0) { total -= 1; }

                if (total % 10 != 0)
                {
                    errorLevel    = (int)PXErrorLevel.Warning;
                    errorOccurred = true;
                    errorMessage  = TWMessages.TaxNbrWarning;
                }
            }
        }

        public bool ConfirmDeletion(PXView view, string gUIFmtCode)
        {
            isCreditNote = true;

            if (gUIFmtCode != TWGUIFormatCode.vATOutCode33 && gUIFmtCode != TWGUIFormatCode.vATOutCode34 && gUIFmtCode != null)
            {
                WebDialogResult result = view.Ask(TWMessages.CfmMegOnDelete, MessageButtons.YesNo);

                isCreditNote = false;
                notBeDeleted = (result == WebDialogResult.No);
            }

            return notBeDeleted;
        }
        #endregion
    }
}
