using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace HSNHighcareCistomizations.DAC
{
    [Serializable]
    [PXCacheName("LUMPINCodeMapping")]
    public class LUMPINCodeMapping : IBqlTable
    {
        public class PK : PrimaryKeyOf<LUMPINCodeMapping>.By<pin>
        {
            public static LUMPINCodeMapping Find(PXGraph graph, string pin) => FindBy(graph, pin);
        }

        #region Pin
        [PXDBString(100, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Pin Code")]
        public virtual string Pin { get; set; }
        public abstract class pin : PX.Data.BQL.BqlString.Field<pin> { }
        #endregion

        #region SerialNbr
        [PXDefault]
        [PXDBString(50, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "HC Serial Nbr.")]
        public virtual string SerialNbr { get; set; }
        public abstract class serialNbr : PX.Data.BQL.BqlString.Field<serialNbr> { }
        #endregion

        #region Noteid
        [PXNote()]
        public virtual Guid? Noteid { get; set; }
        public abstract class noteid : PX.Data.BQL.BqlGuid.Field<noteid> { }
        #endregion

        #region CreatedByID
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion

        #region CreatedByScreenID
        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        #endregion

        #region CreatedDateTime
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion

        #region LastModifiedByID
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        #endregion

        #region LastModifiedByScreenID
        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        #endregion

        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion

        #region Tstamp
        [PXDBTimestamp()]
        [PXUIField(DisplayName = "Tstamp")]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion
    }
}