using PX.Data;

namespace PX.Objects.CA
{
    public class CATranEnq_Extensions : PXGraphExtension<PX.Objects.CA.CATranEnq>
    {
        #region Selects (Only 4 User-Defined Field Attributes)
        public PXSelect<AP.APPayment> APPaymView;
        public PXSelect<AR.ARPayment> ARPaymView;
        public PXSelect<GL.Batch> BatchView;
        #endregion

        #region Attribute Constant Variables & Classes
        public const string CFGROUP1 = "CFGROUP1";
        public class CFGROUP1Attr : PX.Data.BQL.BqlString.Constant<CFGROUP1Attr>
        {
            public CFGROUP1Attr() : base(CFGROUP1) { }
        }

        public const string CFGROUP2 = "CFGROUP2";
        public class CFGROUP2Attr : PX.Data.BQL.BqlString.Constant<CFGROUP1Attr>
        {
            public CFGROUP2Attr() : base(CFGROUP2) { }
        }
        #endregion

        #region Event Handlers
        protected virtual void _(Events.FieldSelecting<CATranExt.usrCFGroup1> e)
        {
            var row = e.Row as CATran;

            if (row != null && e.ReturnValue == null)
            {
                e.ReturnValue = GetUserDefinedFields(Base, row, CFGROUP1);
            }
        }

        protected virtual void _(Events.FieldSelecting<CATranExt.usrCFGroup2> e)
        {
            var row = e.Row as CATran;

            if (row != null && e.ReturnValue == null)
            {
                e.ReturnValue = GetUserDefinedFields(Base, row, CFGROUP2);
            }
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Get original source user defined field attribute values.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="tran"></param>
        /// <param name="attributeID"></param>
        /// <returns></returns>
        public static object GetUserDefinedFields(PXGraph graph, CATran tran, string attributeID)
        {
            PXStringState field = new object() as PXStringState;

            CATranEnq_Extensions tranEnqExt = (graph as CATranEnq).GetExtension<CATranEnq_Extensions>();

            switch (tran.OrigModule)
            {
                case GL.BatchModule.AP:
                    field = (PXStringState)tranEnqExt.APPaymView.Cache.GetValueExt(AP.APPayment.PK.Find(graph, tran.OrigTranType, tran.OrigRefNbr), CS.Messages.Attribute + attributeID);
                    break;
                case GL.BatchModule.AR:
                    field = (PXStringState)tranEnqExt.ARPaymView.Cache.GetValueExt(AR.ARPayment.PK.Find(graph, tran.OrigTranType, tran.OrigRefNbr), CS.Messages.Attribute + attributeID);
                    break;
                case GL.BatchModule.GL:
                    field = (PXStringState)tranEnqExt.BatchView.Cache.GetValueExt(GL.Batch.PK.Find(graph, tran.OrigModule, tran.OrigRefNbr), CS.Messages.Attribute + attributeID);
                    break;
            }

            return field?.Value == null ? string.Empty : $"{field?.Value.ToString()} - {CS.CSAttributeDetail.PK.Find(graph, attributeID, field?.Value.ToString())?.Description}";
        }
        #endregion
    }
}
