global using Titimerval = go.modernc.org.libc_package.Titimervalᴛ1;
global using TunixFileId = go.modernc.org.sqlite.lib.sqlite3_package.TunixFileIdᴛ1;

namespace go.modernc.org.sqlite.lib;

using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

partial struct TunixFileIdᴛ1 /*dyn*/ {
    public Tdev_t Fdev;
    public Tu64 Fino;
}


partial struct _aFlagOpᴛ1 /*dyn*/ {
    public int32 Fop;
    public Tu64 Fmask;
}
internal static array<_aFlagOpᴛ1> _aFlagOp = new array<_aFlagOpᴛ1>(21){
    [0] = new(
        Fop: (int32)SQLITE_DBCONFIG_ENABLE_FKEY,
        Fmask: (uint64)SQLITE_ForeignKeys
    ),
    [1] = new(
        Fop: (int32)SQLITE_DBCONFIG_ENABLE_TRIGGER,
        Fmask: (uint64)SQLITE_EnableTrigger
    ),
    [2] = new(
        Fop: (int32)SQLITE_DBCONFIG_ENABLE_VIEW,
        Fmask: (uint64)SQLITE_EnableView
    ),
    [3] = new(
        Fop: (int32)SQLITE_DBCONFIG_ENABLE_FTS3_TOKENIZER,
        Fmask: (uint64)SQLITE_Fts3Tokenizer
    ),
    [4] = new(
        Fop: (int32)SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION,
        Fmask: (uint64)SQLITE_LoadExtension
    ),
    [5] = new(
        Fop: (int32)SQLITE_DBCONFIG_NO_CKPT_ON_CLOSE,
        Fmask: (uint64)SQLITE_NoCkptOnClose
    ),
    [6] = new(
        Fop: (int32)SQLITE_DBCONFIG_ENABLE_QPSG,
        Fmask: (uint64)SQLITE_EnableQPSG
    ),
    [7] = new(
        Fop: (int32)SQLITE_DBCONFIG_TRIGGER_EQP,
        Fmask: (uint64)SQLITE_TriggerEQP
    ),
    [8] = new(
        Fop: (int32)SQLITE_DBCONFIG_RESET_DATABASE,
        Fmask: (uint64)SQLITE_ResetDatabase
    ),
    [9] = new(
        Fop: (int32)SQLITE_DBCONFIG_DEFENSIVE,
        Fmask: (uint64)SQLITE_Defensive
    ),
    [10] = new(
        Fop: (int32)SQLITE_DBCONFIG_WRITABLE_SCHEMA,
        Fmask: libc.Uint64FromInt32(libc.Int32FromInt32(SQLITE_WriteSchema) | libc.Int32FromInt32(SQLITE_NoSchemaError))
    ),
    [11] = new(
        Fop: (int32)SQLITE_DBCONFIG_LEGACY_ALTER_TABLE,
        Fmask: (uint64)SQLITE_LegacyAlter
    ),
    [12] = new(
        Fop: (int32)SQLITE_DBCONFIG_DQS_DDL,
        Fmask: (uint64)SQLITE_DqsDDL
    ),
    [13] = new(
        Fop: (int32)SQLITE_DBCONFIG_DQS_DML,
        Fmask: (uint64)SQLITE_DqsDML
    ),
    [14] = new(
        Fop: (int32)SQLITE_DBCONFIG_LEGACY_FILE_FORMAT,
        Fmask: (uint64)SQLITE_LegacyFileFmt
    ),
    [15] = new(
        Fop: (int32)SQLITE_DBCONFIG_TRUSTED_SCHEMA,
        Fmask: (uint64)SQLITE_TrustedSchema
    ),
    [16] = new(
        Fop: (int32)SQLITE_DBCONFIG_STMT_SCANSTATUS,
        Fmask: (uint64)SQLITE_StmtScanStatus
    ),
    [17] = new(
        Fop: (int32)SQLITE_DBCONFIG_REVERSE_SCANORDER,
        Fmask: (uint64)SQLITE_ReverseOrder
    ),
    [18] = new(
        Fop: (int32)SQLITE_DBCONFIG_ENABLE_ATTACH_CREATE,
        Fmask: libc.Uint64FromInt32(libc.Int32FromInt32(0x00010)).Lsh((int64)(libc.Int32FromInt32(32)))
    ),
    [19] = new(
        Fop: (int32)SQLITE_DBCONFIG_ENABLE_ATTACH_WRITE,
        Fmask: libc.Uint64FromInt32(libc.Int32FromInt32(0x00020)).Lsh((int64)(libc.Int32FromInt32(32)))
    ),
    [20] = new(
        Fop: (int32)SQLITE_DBCONFIG_ENABLE_COMMENTS,
        Fmask: libc.Uint64FromInt32(libc.Int32FromInt32(0x00040)).Lsh((int64)(libc.Int32FromInt32(32)))
    )
};

} // end sqlite3_package
