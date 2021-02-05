using System;
using PX.Data;
using eGUICustomization4HSN.Graph;

namespace eGUICustomization4HSN.DAC
{
    [Serializable]
    public class TWNNPOTable : PX.Data.IBqlTable
    {
        #region NPONbr
        [PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "NPO Nbr")]
        public virtual string NPONbr { get; set; }
        public abstract class nPONbr : PX.Data.BQL.BqlString.Field<nPONbr> { }
        #endregion

        #region Descr
        [PXDBString(40, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string Descr { get; set; }
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        #endregion

        #region TaxNbr
        [TaxNbrVerify(8, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax ID")]
        public virtual string TaxNbr { get; set; }
        public abstract class taxNbr : PX.Data.BQL.BqlString.Field<taxNbr> { }
        #endregion

        #region CreatedByID
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion

        #region CreatedByScreenID
        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        #endregion

        #region CreatedDateTime
        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime,
                   Enabled = false,
                   IsReadOnly = true)]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion

        #region LastModifiedByID
        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        #endregion

        #region LastModifiedByScreenID       
        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        #endregion

        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime]
        //[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime,
        //           Enabled = false,
        //           IsReadOnly = true)]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion

        #region NoteID
        [PXNote]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion

        #region tstamp        
        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        #endregion
    }

    public class NPONbrSelectorAttribute : PXSelectorAttribute
    {
        public NPONbrSelectorAttribute(): base(typeof(Search<TWNNPOTable.nPONbr>),
                                               typeof(TWNNPOTable.descr),
                                               typeof(TWNNPOTable.taxNbr))
        {
            Filterable = true;
            DirtyRead = true;
            DescriptionField = typeof(TWNNPOTable.descr);
        }
    }
}
