global using TDateTime = go.modernc.org.sqlite.lib.sqlite3_package.TDateTimeᴛ1;
global using TJsonString = go.modernc.org.sqlite.lib.sqlite3_package.TJsonStringᴛ1;
global using TPager = go.modernc.org.sqlite.lib.sqlite3_package.TPagerᴛ1;
global using Tsqlite3rbu = go.modernc.org.sqlite.lib.sqlite3_package.Tsqlite3rbuᴛ1;

namespace go.modernc.org.sqlite.lib;

using System.Runtime.InteropServices;
using go.arrays;
using go.golib;

partial class sqlite3_package {

partial struct TDateTimeᴛ1 /*dyn*/ {
    public Tsqlite3_int64 FiJD;
    public int32 FY;
    public int32 FM;
    public int32 FD;
    public int32 Fh;
    public int32 Fm;
    public int32 Ftz;
    public float64 Fs;
    public int8 FvalidJD;
    public int8 FvalidYMD;
    public int8 FvalidHMS;
    public int8 FnFloor;
    public uint8 F__ccgo44;
}

[global::go.golib.GoInlineLayout(Size = 136, Align = 8), StructLayout(LayoutKind.Explicit, Size = 136)] partial struct TJsonStringᴛ1 /*dyn*/ {
    [FieldOffset(0)] public uintptr FpCtx;
    [FieldOffset(8)] public uintptr FzBuf;
    [FieldOffset(16)] public Tu64 FnAlloc;
    [FieldOffset(24)] public Tu64 FnUsed;
    [FieldOffset(32)] public Tu8 FbStatic;
    [FieldOffset(33)] public Tu8 FeErr;
    [FieldOffset(34)] public array100<int8> FzSpace;
}

[global::go.golib.GoInlineLayout(Size = 312, Align = 8), StructLayout(LayoutKind.Explicit, Size = 312)] partial struct TPagerᴛ1 /*dyn*/ {
    [FieldOffset(0)] public uintptr FpVfs;
    [FieldOffset(8)] public Tu8 FexclusiveMode;
    [FieldOffset(9)] public Tu8 FjournalMode;
    [FieldOffset(10)] public Tu8 FuseJournal;
    [FieldOffset(11)] public Tu8 FnoSync;
    [FieldOffset(12)] public Tu8 FfullSync;
    [FieldOffset(13)] public Tu8 FextraSync;
    [FieldOffset(14)] public Tu8 FsyncFlags;
    [FieldOffset(15)] public Tu8 FwalSyncFlags;
    [FieldOffset(16)] public Tu8 FtempFile;
    [FieldOffset(17)] public Tu8 FnoLock;
    [FieldOffset(18)] public Tu8 FreadOnly;
    [FieldOffset(19)] public Tu8 FmemDb;
    [FieldOffset(20)] public Tu8 FmemVfs;
    [FieldOffset(21)] public Tu8 FeState;
    [FieldOffset(22)] public Tu8 FeLock;
    [FieldOffset(23)] public Tu8 FchangeCountDone;
    [FieldOffset(24)] public Tu8 FsetSuper;
    [FieldOffset(25)] public Tu8 FdoNotSpill;
    [FieldOffset(26)] public Tu8 FsubjInMemory;
    [FieldOffset(27)] public Tu8 FbUseFetch;
    [FieldOffset(28)] public Tu8 FhasHeldSharedLock;
    [FieldOffset(32)] public TPgno FdbSize;
    [FieldOffset(36)] public TPgno FdbOrigSize;
    [FieldOffset(40)] public TPgno FdbFileSize;
    [FieldOffset(44)] public TPgno FdbHintSize;
    [FieldOffset(48)] public int32 FerrCode;
    [FieldOffset(52)] public int32 FnRec;
    [FieldOffset(56)] public Tu32 FcksumInit;
    [FieldOffset(60)] public Tu32 FnSubRec;
    [FieldOffset(64)] public uintptr FpInJournal;
    [FieldOffset(72)] public uintptr Ffd;
    [FieldOffset(80)] public uintptr Fjfd;
    [FieldOffset(88)] public uintptr Fsjfd;
    [FieldOffset(96)] public Ti64 FjournalOff;
    [FieldOffset(104)] public Ti64 FjournalHdr;
    [FieldOffset(112)] public uintptr FpBackup;
    [FieldOffset(120)] public uintptr FaSavepoint;
    [FieldOffset(128)] public int32 FnSavepoint;
    [FieldOffset(132)] public Tu32 FiDataVersion;
    [FieldOffset(136)] public array16<int8> FdbFileVers;
    [FieldOffset(152)] public int32 FnMmapOut;
    [FieldOffset(160)] public Tsqlite3_int64 FszMmap;
    [FieldOffset(168)] public uintptr FpMmapFreelist;
    [FieldOffset(176)] public Tu16 FnExtra;
    [FieldOffset(178)] public Ti16 FnReserve;
    [FieldOffset(180)] public Tu32 FvfsFlags;
    [FieldOffset(184)] public Tu32 FsectorSize;
    [FieldOffset(188)] public TPgno FmxPgno;
    [FieldOffset(192)] public TPgno FlckPgno;
    [FieldOffset(200)] public Ti64 FpageSize;
    [FieldOffset(208)] public Ti64 FjournalSizeLimit;
    [FieldOffset(216)] public uintptr FzFilename;
    [FieldOffset(224)] public uintptr FzJournal;
    [FieldOffset(232)] public uintptr FxBusyHandler;
    [FieldOffset(240)] public uintptr FpBusyHandlerArg;
    [FieldOffset(248)] public array4<Tu32> FaStat;
    [FieldOffset(264)] public uintptr FxReiniter;
    [FieldOffset(272)] public uintptr FxGet;
    [FieldOffset(280)] public uintptr FpTmpSpace;
    [FieldOffset(288)] public uintptr FpPCache;
    [FieldOffset(296)] public uintptr FpWal;
    [FieldOffset(304)] public uintptr FzWal;
}

[global::go.golib.GoInlineLayout(Size = 416, Align = 8), StructLayout(LayoutKind.Explicit, Size = 416)] partial struct Tsqlite3rbuᴛ1 /*dyn*/ {
    [FieldOffset(0)] public int32 FeStage;
    [FieldOffset(8)] public uintptr FdbMain;
    [FieldOffset(16)] public uintptr FdbRbu;
    [FieldOffset(24)] public uintptr FzTarget;
    [FieldOffset(32)] public uintptr FzRbu;
    [FieldOffset(40)] public uintptr FzState;
    [FieldOffset(48)] public array5<int8> FzStateDb;
    [FieldOffset(56)] public int32 Frc;
    [FieldOffset(64)] public uintptr FzErrmsg;
    [FieldOffset(72)] public int32 FnStep;
    [FieldOffset(80)] public Tsqlite3_int64 FnProgress;
    [FieldOffset(88)] public TRbuObjIter Fobjiter;
    [FieldOffset(280)] public uintptr FzVfsName;
    [FieldOffset(288)] public uintptr FpTargetFd;
    [FieldOffset(296)] public int32 FnPagePerSector;
    [FieldOffset(304)] public Ti64 FiOalSz;
    [FieldOffset(312)] public Ti64 FnPhaseOneStep;
    [FieldOffset(320)] public uintptr FpRenameArg;
    [FieldOffset(328)] public uintptr FxRename;
    [FieldOffset(336)] public Tu32 FiMaxFrame;
    [FieldOffset(340)] public Tu32 FmLock;
    [FieldOffset(344)] public int32 FnFrame;
    [FieldOffset(348)] public int32 FnFrameAlloc;
    [FieldOffset(352)] public uintptr FaFrame;
    [FieldOffset(360)] public int32 Fpgsz;
    [FieldOffset(368)] public uintptr FaBuf;
    [FieldOffset(376)] public Ti64 FiWalCksum;
    [FieldOffset(384)] public Ti64 FszTemp;
    [FieldOffset(392)] public Ti64 FszTempLimit;
    [FieldOffset(400)] public int32 FnRbu;
    [FieldOffset(408)] public uintptr FpRbuFd;
}

} // end sqlite3_package
