using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using HSNFinance.DAC;

namespace ReportUDF
{
    public class UtilFunctions
    {
        /// <summary>
        /// Summarize all settlement records by the report parameter of "End Date", If the report parameter of the "Settlement Nbr." has the same balance as the aggregated record, then return true.
        /// </summary>
        public bool IncludeSettledAmt(DateTime endDate, string settlementNbr)
        {
            try
            {
                if (string.IsNullOrEmpty(settlementNbr)) { return true; }

                List<LSLedgerSettlement> list = SelectFrom<LSLedgerSettlement>.Where<LSLedgerSettlement.tranDate.IsLessEqual<@P.AsDateTime>>
                                                                                     .OrderBy<Asc<LSLedgerSettlement.tranDate,
                                                                                                  Asc<LSLedgerSettlement.batchNbr>>>.View.Select(new PXGraph(), endDate).RowCast<LSLedgerSettlement>().ToList();

                var aggregate = list.GroupBy(x => new { x.SettlementNbr }).Select(x => new  {
                                                                                                SettlementNbr = x.Key.SettlementNbr,
                                                                                                TotalDebit = x.Sum(y => y.SettledDebitAmt),
                                                                                                TotalCredit = x.Sum(y => y.SettledCreditAmt)
                                                                                            }).ToList();

                return aggregate.Find(x => x.TotalCredit - x.TotalDebit == decimal.Zero && x.SettlementNbr == settlementNbr)?.SettlementNbr != null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Summarize all settlement records by the report parameter of "End Date", and return true to display line if the report parameter of the "Settlement Nbr." is the same as the aggregation record and has no balance.
        /// </summary>
        public bool DisplayGLTran(DateTime endDate, string settlementNbr)
        {
            try
            {
                if (string.IsNullOrEmpty(settlementNbr)) { return true; }

                List<LSLedgerSettlement> list = SelectFrom<LSLedgerSettlement>.Where<LSLedgerSettlement.tranDate.IsLessEqual<@P.AsDateTime>>
                                                                                     .OrderBy<Asc<LSLedgerSettlement.tranDate,
                                                                                                  Asc<LSLedgerSettlement.batchNbr>>>.View.Select(new PXGraph(), endDate).RowCast<LSLedgerSettlement>().ToList();

                var aggregate = list.GroupBy(x => new { x.SettlementNbr }).Select(x => new {
                                                                                               SettlementNbr = x.Key.SettlementNbr,
                                                                                               TotalDebit = x.Sum(y => y.SettledDebitAmt),
                                                                                               TotalCredit = x.Sum(y => y.SettledCreditAmt)
                                                                                           }).ToList();

                return aggregate.Find(x => x.TotalCredit - x.TotalDebit != decimal.Zero && x.SettlementNbr == settlementNbr)?.SettlementNbr != null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
