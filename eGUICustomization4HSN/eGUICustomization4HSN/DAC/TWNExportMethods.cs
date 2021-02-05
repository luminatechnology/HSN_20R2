using System;
using PX.Data;
using eGUICustomization4HSN.Graph;
using PX.Objects.RQ;

namespace eGUICustomization4HSN.DAC
{
    [Serializable]
    [PXCacheName("Export Methods")]
    [PXPrimaryGraph(typeof(TWNExportMthsMaint))]
    public class TWNExportMethods : PX.Data.IBqlTable
    {
        #region ExportMethod
        [PXDBString(1, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Export Method")]
        public virtual string ExportMethod { get; set; }
        public abstract class exportMethod : PX.Data.BQL.BqlString.Field<exportMethod> { }
        #endregion

        #region Description
        [PXDBString(40, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string Description { get; set; }
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
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
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime,
                   Enabled = false,
                   IsReadOnly = true)]
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

    public class ExportMthSelectorAttribute : PXSelectorAttribute
    {
        public ExportMthSelectorAttribute() : base(typeof(Search<TWNExportMethods.exportMethod>),
                                                   typeof(TWNExportMethods.description))
        {
            Filterable = true;
            DirtyRead = true;
            DescriptionField = typeof(TWNExportMethods.description);
        }
    }
}