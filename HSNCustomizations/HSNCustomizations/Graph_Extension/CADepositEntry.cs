using HSNCustomizations.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PX.Data.PXImportAttribute;

namespace PX.Objects.CA
{
    public class CADepositEntryExt : PXGraphExtension<CADepositEntry>, PXImportAttribute.IPXPrepareItems, IPXProcess
    {
        // 紀錄Excel匯入的資料
        protected string importRefValue = string.Empty;

        [PXImport(typeof(CADepositDetail))]
        public PXSelectJoin<CADepositDetail, LeftJoin<PaymentInfo, On<CADepositDetail.origDocType, Equal<PaymentInfo.docType>,
                    And<CADepositDetail.origRefNbr, Equal<PaymentInfo.refNbr>>>>,
                    Where<CADepositDetail.refNbr, Equal<Current<CADeposit.refNbr>>,
                             And<CADepositDetail.tranType, Equal<Current<CADeposit.tranType>>>>> DepositPayments;

        public delegate IEnumerable depositPaymentsDelegate();

        [PXOverride]
        public virtual IEnumerable depositPayments(depositPaymentsDelegate baseMethod)
        {
            var baseResult = baseMethod.Invoke();
            foreach (var item in baseResult)
            {
                yield return item;
            }
        }

        #region Override DAC
        [LUMGetAvailablePayments]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        public virtual void _(Events.CacheAttached<CADepositDetail.origRefNbr> e) { }
        #endregion

        #region Event Handlers
        public virtual void _(Events.RowSelected<CADeposit> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);
            this.DepositPayments.Cache.AllowInsert = this.DepositPayments.Cache.AllowUpdate = true;
            Base.Details.Cache.AllowInsert = Base.Details.Cache.AllowUpdate = true;
        }

        public virtual void _(Events.RowSelected<CADepositDetail> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);
            PXUIFieldAttribute.SetEnabled<CADepositDetail.curyTranAmt>(e.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<CADepositDetail.curyOrigAmt>(e.Cache, null, false);
        }

        public virtual void _(Events.FieldUpdated<CADepositDetail.origRefNbr> e)
        {
            var row = e.Row as CADepositDetail;

            // Check is Manual Insert New Record
            if (row == null || !string.IsNullOrEmpty(row.OrigDocType)) { return; }

            try
            {
                // Get ARInfomation
                var ARInfo = SelectFrom<ARPayment>.Where<ARPayment.refNbr.IsEqual<P.AsString>>.View.Select(Base, (string)e.NewValue).RowCast<ARPayment>().FirstOrDefault();

                // Setting filter
                Base.filter.Current.StartDate = ARInfo.DepositAfter;
                Base.filter.Current.EndDate = ARInfo.DepositAfter;
                Base.filter.Current.PaymentMethodID = null;
                // Select Available Payments
                Base.AvailablePayments.Select().ToList();

                // Get Availalbe Payment and set current record Selected
                Base.AvailablePayments.Cache.AllowInsert = Base.AvailablePayments.Cache.AllowUpdate = true;

                try
                {
                    Base.AvailablePayments.Cache.Inserted.Cast<PaymentInfo>().Where(p => p.RefNbr == (string)e.NewValue).FirstOrDefault().Selected = true;
                }
                catch (NullReferenceException)
                {
                    throw new Exception("Can not find data in Available PAYMENT!");
                }

                // Insert Data
                IEnumerable<PaymentInfo> toAdd = Base.AvailablePayments.Cache.Inserted.Cast<PaymentInfo>().Where(p => p.Selected == true && p.RefNbr == (string)e.NewValue);

                if (!Base.IsImportFromExcel)
                    this.DepositPayments.Cache.Clear();
                Base.AddPaymentInfoBatch(toAdd);

                Base.AvailablePayments.Cache.AllowInsert = Base.AvailablePayments.Cache.AllowUpdate = false;
                Base.AvailablePayments.Cache.Clear();

                this.DepositPayments.View.RequestRefresh();

            }
            catch (Exception)
            {
                throw;
            }

        }

        #endregion

        #region Method

        public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
        {
            this.importRefValue = values["RefNbr"].ToString();
            return true;
        }

        public bool RowImporting(string viewName, object row)
        {
            return true;
        }

        public bool RowImported(string viewName, object row, object oldRow)
        {
            // 根據匯入的資料去找出對應的RefNbr
            var arPaymentInfo = SelectFrom<ARPayment>
                                .Where<ARPayment.refNbr.IsEqual<P.AsString>
                                       .Or<ARPayment.extRefNbr.IsEqual<P.AsString>>>
                                .View.Select(new PXGraph(), importRefValue, importRefValue)
                                .RowCast<ARPayment>().FirstOrDefault();
            if (arPaymentInfo == null || string.IsNullOrEmpty(arPaymentInfo.RefNbr))
                return false;
            this.DepositPayments.SetValueExt<CADepositDetail.origRefNbr>((CADepositDetail)row, arPaymentInfo.RefNbr);
            // 因為原生的Code會去Insert正確資料，所以這裡把Import 的資料移除
            this.DepositPayments.Cache.Delete(row);
            return true;
        }

        public void PrepareItems(string viewName, IEnumerable items)
        {

        }

        public void ImportDone(ImportMode.Value mode)
        {
            this.DepositPayments.View.RequestRefresh();
        }
        #endregion
    }
}
