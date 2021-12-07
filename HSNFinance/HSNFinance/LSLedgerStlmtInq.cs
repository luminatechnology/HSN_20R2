using System.Collections;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.GL;
using HSNFinance.DAC;

namespace HSNFinance
{
    public class LSLedgerStlmtInq : PXGraph<LSLedgerStlmtInq>
    {
        #region Select & Features
        public PXCancel<LSLedgerSettlement> Cancel;
        public PXSavePerRow<LSLedgerSettlement> Save;
      
        [PXFilterable()]
        public SelectFrom<LSLedgerSettlement>.View LedgerStlmt;

        public SelectFrom<GLTran>.Where<GLTran.module.IsEqual<LSLedgerSettlement.module.FromCurrent>
                                        .And<GLTran.batchNbr.IsEqual<LSLedgerSettlement.batchNbr.FromCurrent>
                                             .And<GLTran.lineNbr.IsEqual<LSLedgerSettlement.lineNbr.FromCurrent>>>>.View GLTranView;

        #endregion

        #region Action
        public PXAction<LSLedgerSettlement> Unmatch;
        [PXProcessButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Unmatch")]
        public virtual IEnumerable unmatch (PXAdapter adapter)
        {
            foreach (LSLedgerSettlement ls in LedgerStlmt.Cache.Updated)
            {
                if (ls.Selected == true)
                {
                    foreach (LSLedgerSettlement delRow in SelectFrom<LSLedgerSettlement>
                                                                     .Where<LSLedgerSettlement.settlementNbr.IsEqual<@P.AsString>>.View.Select(this, ls.SettlementNbr))
                    {
                        LedgerStlmt.Cache.Delete(delRow);

                        GLTranView.Current = SelectFrom<GLTran>.Where<GLTran.module.IsEqual<@P.AsString>
                                                                      .And<GLTran.batchNbr.IsEqual<@P.AsString>
                                                                           .And<GLTran.lineNbr.IsEqual<@P.AsInt>>>>
                                                               .View.ReadOnly.SelectSingleBound(this, null, delRow.Module, delRow.BatchNbr, delRow.LineNbr);

                        LSLedgerStlmtEntry.UpdateGLTranUOM(GLTranView.Cache, null);
                    }
                }
            }

            this.Save.Press();

            return adapter.Get();
        }
        #endregion
    }
}