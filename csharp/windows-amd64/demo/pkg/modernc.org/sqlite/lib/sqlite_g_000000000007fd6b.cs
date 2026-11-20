global using TBitvec = go.modernc.org.sqlite.lib.sqlite3_package.TBitvecᴛ1;
global using TExprList = go.modernc.org.sqlite.lib.sqlite3_package.TExprListᴛ1;
global using TFts5DlidxIter = go.modernc.org.sqlite.lib.sqlite3_package.TFts5DlidxIterᴛ1;
global using TFts5TombstoneArray = go.modernc.org.sqlite.lib.sqlite3_package.TFts5TombstoneArrayᴛ1;
global using TIdList = go.modernc.org.sqlite.lib.sqlite3_package.TIdListᴛ1;
global using TIndex = go.modernc.org.sqlite.lib.sqlite3_package.TIndexᴛ1;
global using TIndexIterator = go.modernc.org.sqlite.lib.sqlite3_package.TIndexIteratorᴛ1;
global using TParse = go.modernc.org.sqlite.lib.sqlite3_package.TParseᴛ1;
global using TPreUpdate = go.modernc.org.sqlite.lib.sqlite3_package.TPreUpdateᴛ1;
global using TRowSetChunk = go.modernc.org.sqlite.lib.sqlite3_package.TRowSetChunkᴛ1;
global using TSrcList = go.modernc.org.sqlite.lib.sqlite3_package.TSrcListᴛ1;
global using TUnpackedRecord = go.modernc.org.sqlite.lib.sqlite3_package.TUnpackedRecordᴛ1;
global using TVdbe = go.modernc.org.sqlite.lib.sqlite3_package.TVdbeᴛ1;
global using TWalIterator = go.modernc.org.sqlite.lib.sqlite3_package.TWalIteratorᴛ1;
global using TWhereInfo = go.modernc.org.sqlite.lib.sqlite3_package.TWhereInfoᴛ1;
global using TYYMINORTYPE = go.modernc.org.sqlite.lib.sqlite3_package.TYYMINORTYPEᴛ1;
global using Tfts5YYMINORTYPE = go.modernc.org.sqlite.lib.sqlite3_package.Tfts5YYMINORTYPEᴛ1;
global using Tintptr_t = long;
global using Tp4union = go.modernc.org.sqlite.lib.sqlite3_package.Tp4unionᴛ1;
global using Tptrdiff_t = long;
global using Tsize_t = ulong;
global using Tsqlite3StatValueType = long;
global using Tssize_t = long;
global using Tuintptr_t = ulong;
global using Tuptr = ulong;
global using t__predefined_ptrdiff_t = long;
global using t__predefined_size_t = ulong;

namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using System.Runtime.InteropServices;
using go.arrays;
using go.golib;
using go.modernc.org;

partial class sqlite3_package {

public static UntypedInt BITVEC_NBIT => 3968;

public static UntypedInt BITVEC_NELEM => 496;

public static UntypedInt BITVEC_NINT => 124;

public static UntypedInt BITVEC_NPTR => 62;

public static UntypedInt BITVEC_USIZE => 496;

public static UntypedInt EXPR_FULLSIZE => 72;

public static UntypedInt EXPR_REDUCEDSIZE => 44;

public static UntypedInt EXPR_TOKENONLYSIZE => 16;

public static UntypedInt INTPTR_MAX => 9223372036854775807;

public static UntypedInt INTPTR_MIN => -9223372036854775808;

public static UntypedInt MEMCELLSIZE => 24;

public static UntypedInt PARSE_HDR_SZ => 168;

public static UntypedInt PARSE_RECURSE_SZ => 288;

public static UntypedInt PARSE_TAIL_SZ => 152;

public static UntypedInt PTRDIFF_MAX => 9223372036854775807;

public static UntypedInt PTRDIFF_MIN => -9223372036854775808;

public static UntypedInt ROWSET_ENTRY_PER_CHUNK => 42;

public static UntypedInt SIZE_MAX => 18446744073709551615;

public static UntypedInt SQLITE_PTRSIZE => 8;

public static UntypedInt SZ_KEYINFO_0 => 32;

public static UntypedInt SZ_SRCLIST_1 => 88;

[global::go.golib.GoInlineLayout(Size = 496, Align = 8), StructLayout(LayoutKind.Explicit, Size = 496)] partial struct TBitvecᴛ1_Fu /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<array124<Tu32>>))] public readonly /*[0][124]*/ int FaHash;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<array62<uintptr>>))] public readonly /*[0][62]*/ long FapSub;
    [FieldOffset(0)] public array496<Tu8> FaBitmap;
}

[global::go.golib.GoInlineLayout(Size = 512, Align = 8), StructLayout(LayoutKind.Explicit, Size = 512)] partial struct TBitvecᴛ1 /*dyn*/ {
    [FieldOffset(0)] public Tu32 FiSize;
    [FieldOffset(4)] public Tu32 FnSet;
    [FieldOffset(8)] public Tu32 FiDivisor;
    [FieldOffset(16)] public TBitvecᴛ1_Fu Fu;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct TExprListᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uint64>))] public readonly long F__ccgo_align;
    [FieldOffset(0)] public int32 FnExpr;
    [FieldOffset(4)] public int32 FnAlloc;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct TFts5DlidxIterᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uint64>))] public readonly long F__ccgo_align;
    [FieldOffset(0)] public int32 FnLvl;
    [FieldOffset(4)] public int32 FiSegid;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct TFts5TombstoneArrayᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uint64>))] public readonly long F__ccgo_align;
    [FieldOffset(0)] public int32 FnRef;
    [FieldOffset(4)] public int32 FnTombstone;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct TIdListᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uint64>))] public readonly long F__ccgo_align;
    [FieldOffset(0)] public int32 FnId;
}

partial struct TIndexᴛ1 /*dyn*/ {
    public uintptr FzName;
    public uintptr FaiColumn;
    public uintptr FaiRowLogEst;
    public uintptr FpTable;
    public uintptr FzColAff;
    public uintptr FpNext;
    public uintptr FpSchema;
    public uintptr FaSortOrder;
    public uintptr FazColl;
    public uintptr FpPartIdxWhere;
    public uintptr FaColExpr;
    public TPgno Ftnum;
    public TLogEst FszIdxRow;
    public Tu16 FnKeyCol;
    public Tu16 FnColumn;
    public Tu8 FonError;
    public uint16 F__ccgo100;
    public int32 FnSample;
    public int32 FmxSample;
    public int32 FnSampleCol;
    public uintptr FaAvgEq;
    public uintptr FaSample;
    public uintptr FaiRowEst;
    public TtRowcnt FnRowEst0;
    public TBitmask FcolNotIdxed;
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 8), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct TIndexIteratorᴛ1_Fu_Fax /*dyn*/ {
    [FieldOffset(0)] public int32 FnIdx;
    [FieldOffset(8)] public uintptr FaIdx;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct TIndexIteratorᴛ1_Fu_Flx /*dyn*/ {
    [FieldOffset(0)] public uintptr FpIdx;
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 8), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct TIndexIteratorᴛ1_Fu /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<TIndexIteratorᴛ1_Fu_Fax>))] public readonly long Fax;
    [FieldOffset(0)] public TIndexIteratorᴛ1_Fu_Flx Flx;
    [FieldOffset(8)] public array8<byte> F__ccgo_pad2;
}

[global::go.golib.GoInlineLayout(Size = 24, Align = 8), StructLayout(LayoutKind.Explicit, Size = 24)] partial struct TIndexIteratorᴛ1 /*dyn*/ {
    [FieldOffset(0)] public int32 FeType;
    [FieldOffset(4)] public int32 Fi;
    [FieldOffset(8)] public TIndexIteratorᴛ1_Fu Fu;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct TParseᴛ1_Fu1_Fd /*dyn*/ {
    [FieldOffset(0)] public uintptr FpReturning;
}

[global::go.golib.GoInlineLayout(Size = 32, Align = 8), StructLayout(LayoutKind.Explicit, Size = 32)] partial struct TParseᴛ1_Fu1_Fcr /*dyn*/ {
    [FieldOffset(0)] public int32 FaddrCrTab;
    [FieldOffset(4)] public int32 FregRowid;
    [FieldOffset(8)] public int32 FregRoot;
    [FieldOffset(16)] public TToken FconstraintName;
}

[global::go.golib.GoInlineLayout(Size = 32, Align = 8), StructLayout(LayoutKind.Explicit, Size = 32)] partial struct TParseᴛ1_Fu1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<TParseᴛ1_Fu1_Fd>))] public readonly long Fd;
    [FieldOffset(0)] public TParseᴛ1_Fu1_Fcr Fcr;
}

[global::go.golib.GoInlineLayout(Size = 440, Align = 8), StructLayout(LayoutKind.Explicit, Size = 440)] partial struct TParseᴛ1 /*dyn*/ {
    [FieldOffset(0)] public uintptr Fdb;
    [FieldOffset(8)] public uintptr FzErrMsg;
    [FieldOffset(16)] public uintptr FpVdbe;
    [FieldOffset(24)] public int32 Frc;
    [FieldOffset(28)] public TLogEst FnQueryLoop;
    [FieldOffset(30)] public Tu8 Fnested;
    [FieldOffset(31)] public Tu8 FnTempReg;
    [FieldOffset(32)] public Tu8 FisMultiWrite;
    [FieldOffset(33)] public Tu8 FdisableLookaside;
    [FieldOffset(34)] public Tu8 FprepFlags;
    [FieldOffset(35)] public Tu8 FwithinRJSubrtn;
    [FieldOffset(36)] public Tu8 FmSubrtnSig;
    [FieldOffset(37)] public Tu8 FeTriggerOp;
    [FieldOffset(38)] public Tu8 FeOrconf;
    [FieldOffset(40)] public uint16 F__ccgo40;
    [FieldOffset(44)] public int32 FnRangeReg;
    [FieldOffset(48)] public int32 FiRangeReg;
    [FieldOffset(52)] public int32 FnErr;
    [FieldOffset(56)] public int32 FnTab;
    [FieldOffset(60)] public int32 FnMem;
    [FieldOffset(64)] public int32 FszOpAlloc;
    [FieldOffset(68)] public int32 FiSelfTab;
    [FieldOffset(72)] public int32 FnNestSel;
    [FieldOffset(76)] public int32 FnLabel;
    [FieldOffset(80)] public int32 FnLabelAlloc;
    [FieldOffset(88)] public uintptr FaLabel;
    [FieldOffset(96)] public uintptr FpConstExpr;
    [FieldOffset(104)] public uintptr FpIdxEpr;
    [FieldOffset(112)] public uintptr FpIdxPartExpr;
    [FieldOffset(120)] public TyDbMask FwriteMask;
    [FieldOffset(124)] public TyDbMask FcookieMask;
    [FieldOffset(128)] public int32 FnMaxArg;
    [FieldOffset(132)] public int32 FnSelect;
    [FieldOffset(136)] public Tu32 FnProgressSteps;
    [FieldOffset(140)] public int32 FnTableLock;
    [FieldOffset(144)] public uintptr FpToplevel;
    [FieldOffset(152)] public uintptr FpTriggerTab;
    [FieldOffset(160)] public uintptr FpTriggerPrg;
    [FieldOffset(168)] public uintptr FpCleanup;
    [FieldOffset(176)] public array8<int32> FaTempReg;
    [FieldOffset(208)] public uintptr FpOuterParse;
    [FieldOffset(216)] public TToken FsNameToken;
    [FieldOffset(232)] public Tu32 Foldmask;
    [FieldOffset(236)] public Tu32 Fnewmask;
    [FieldOffset(240)] public TParseᴛ1_Fu1 Fu1;
    [FieldOffset(272)] public uintptr FpAinc;
    [FieldOffset(280)] public uintptr FaTableLock;
    [FieldOffset(288)] public TToken FsLastToken;
    [FieldOffset(304)] public TynVar FnVar;
    [FieldOffset(312)] public array2<Tu64> FaVnbmc;
    [FieldOffset(328)] public Tu8 FiPkSortOrder;
    [FieldOffset(329)] public Tu8 Fexplain;
    [FieldOffset(330)] public Tu8 FeParseMode;
    [FieldOffset(332)] public int32 FnVtabLock;
    [FieldOffset(336)] public int32 FnHeight;
    [FieldOffset(340)] public int32 FaddrExplain;
    [FieldOffset(344)] public uintptr FpVList;
    [FieldOffset(352)] public uintptr FpReprepare;
    [FieldOffset(360)] public uintptr FzTail;
    [FieldOffset(368)] public uintptr FpNewTable;
    [FieldOffset(376)] public uintptr FpNewIndex;
    [FieldOffset(384)] public uintptr FpNewTrigger;
    [FieldOffset(392)] public uintptr FzAuthContext;
    [FieldOffset(400)] public TToken FsArg;
    [FieldOffset(416)] public uintptr FapVtabLock;
    [FieldOffset(424)] public uintptr FpWith;
    [FieldOffset(432)] public uintptr FpRename;
}

[global::go.golib.GoInlineLayout(Size = 32, Align = 1), StructLayout(LayoutKind.Explicit, Size = 32)] partial struct TPreUpdateᴛ1_FuKey /*dyn*/ {
    [FieldOffset(0)] public array32<Tu8> FkeyinfoSpace;
}

[global::go.golib.GoInlineLayout(Size = 200, Align = 8), StructLayout(LayoutKind.Explicit, Size = 200)] partial struct TPreUpdateᴛ1 /*dyn*/ {
    [FieldOffset(0)] public uintptr Fv;
    [FieldOffset(8)] public uintptr FpCsr;
    [FieldOffset(16)] public int32 Fop;
    [FieldOffset(24)] public uintptr FaRecord;
    [FieldOffset(32)] public uintptr FpKeyinfo;
    [FieldOffset(40)] public uintptr FpUnpacked;
    [FieldOffset(48)] public uintptr FpNewUnpacked;
    [FieldOffset(56)] public int32 FiNewReg;
    [FieldOffset(60)] public int32 FiBlobWrite;
    [FieldOffset(64)] public Ti64 FiKey1;
    [FieldOffset(72)] public Ti64 FiKey2;
    [FieldOffset(80)] public TMem Foldipk;
    [FieldOffset(136)] public uintptr FaNew;
    [FieldOffset(144)] public uintptr FpTab;
    [FieldOffset(152)] public uintptr FpPk;
    [FieldOffset(160)] public uintptr FapDflt;
    [FieldOffset(168)] public TPreUpdateᴛ1_FuKey FuKey;
}

[global::go.golib.GoInlineLayout(Size = 1016, Align = 8), StructLayout(LayoutKind.Explicit, Size = 1016)] partial struct TRowSetChunkᴛ1 /*dyn*/ {
    [FieldOffset(0)] public uintptr FpNextChunk;
    [FieldOffset(8)] public array42<TRowSetEntry> FaEntry;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct TSrcListᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uint64>))] public readonly long F__ccgo_align;
    [FieldOffset(0)] public int32 FnSrc;
    [FieldOffset(4)] public Tu32 FnAlloc;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct TUnpackedRecordᴛ1_Fu /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<Ti64>))] public readonly long Fi;
    [FieldOffset(0)] public uintptr Fz;
}

[global::go.golib.GoInlineLayout(Size = 40, Align = 8), StructLayout(LayoutKind.Explicit, Size = 40)] partial struct TUnpackedRecordᴛ1 /*dyn*/ {
    [FieldOffset(0)] public uintptr FpKeyInfo;
    [FieldOffset(8)] public uintptr FaMem;
    [FieldOffset(16)] public TUnpackedRecordᴛ1_Fu Fu;
    [FieldOffset(24)] public int32 Fn;
    [FieldOffset(28)] public Tu16 FnField;
    [FieldOffset(30)] public Ti8 Fdefault_rc;
    [FieldOffset(31)] public Tu8 FerrCode;
    [FieldOffset(32)] public Ti8 Fr1;
    [FieldOffset(33)] public Ti8 Fr2;
    [FieldOffset(34)] public Tu8 FeqSeen;
}

[global::go.golib.GoInlineLayout(Size = 312, Align = 8), StructLayout(LayoutKind.Explicit, Size = 312)] partial struct TVdbeᴛ1 /*dyn*/ {
    [FieldOffset(0)] public uintptr Fdb;
    [FieldOffset(8)] public uintptr FppVPrev;
    [FieldOffset(16)] public uintptr FpVNext;
    [FieldOffset(24)] public uintptr FpParse;
    [FieldOffset(32)] public TynVar FnVar;
    [FieldOffset(36)] public int32 FnMem;
    [FieldOffset(40)] public int32 FnCursor;
    [FieldOffset(44)] public Tu32 FcacheCtr;
    [FieldOffset(48)] public int32 Fpc;
    [FieldOffset(52)] public int32 Frc;
    [FieldOffset(56)] public Ti64 FnChange;
    [FieldOffset(64)] public int32 FiStatement;
    [FieldOffset(72)] public Ti64 FiCurrentTime;
    [FieldOffset(80)] public Ti64 FnFkConstraint;
    [FieldOffset(88)] public Ti64 FnStmtDefCons;
    [FieldOffset(96)] public Ti64 FnStmtDefImmCons;
    [FieldOffset(104)] public uintptr FaMem;
    [FieldOffset(112)] public uintptr FapArg;
    [FieldOffset(120)] public uintptr FapCsr;
    [FieldOffset(128)] public uintptr FaVar;
    [FieldOffset(136)] public uintptr FaOp;
    [FieldOffset(144)] public int32 FnOp;
    [FieldOffset(148)] public int32 FnOpAlloc;
    [FieldOffset(152)] public uintptr FaColName;
    [FieldOffset(160)] public uintptr FpResultRow;
    [FieldOffset(168)] public uintptr FzErrMsg;
    [FieldOffset(176)] public uintptr FpVList;
    [FieldOffset(184)] public Ti64 FstartTime;
    [FieldOffset(192)] public Tu16 FnResColumn;
    [FieldOffset(194)] public Tu16 FnResAlloc;
    [FieldOffset(196)] public Tu8 FerrorAction;
    [FieldOffset(197)] public Tu8 FminWriteFileFormat;
    [FieldOffset(198)] public Tu8 FprepFlags;
    [FieldOffset(199)] public Tu8 FeVdbeState;
    [FieldOffset(200)] public uint16 F__ccgo200;
    [FieldOffset(204)] public TyDbMask FbtreeMask;
    [FieldOffset(208)] public TyDbMask FlockMask;
    [FieldOffset(212)] public array9<Tu32> FaCounter;
    [FieldOffset(248)] public uintptr FzSql;
    [FieldOffset(256)] public uintptr FpFree;
    [FieldOffset(264)] public uintptr FpFrame;
    [FieldOffset(272)] public uintptr FpDelFrame;
    [FieldOffset(280)] public int32 FnFrame;
    [FieldOffset(284)] public Tu32 Fexpmask;
    [FieldOffset(288)] public Tu32 Fsmimask;
    [FieldOffset(296)] public uintptr FpProgram;
    [FieldOffset(304)] public uintptr FpAuxData;
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 8), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct TWalIteratorᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uint64>))] public readonly long F__ccgo_align;
    [FieldOffset(0)] public Tu32 FiPrior;
    [FieldOffset(4)] public int32 FnSegment;
    [FieldOffset(8)] public Tu32 FnPagemap;
}

[global::go.golib.GoInlineLayout(Size = 856, Align = 8), StructLayout(LayoutKind.Explicit, Size = 856)] partial struct TWhereInfoᴛ1 /*dyn*/ {
    [FieldOffset(0)] public uintptr FpParse;
    [FieldOffset(8)] public uintptr FpTabList;
    [FieldOffset(16)] public uintptr FpOrderBy;
    [FieldOffset(24)] public uintptr FpResultSet;
    [FieldOffset(32)] public uintptr FpSelect;
    [FieldOffset(40)] public array2<int32> FaiCurOnePass;
    [FieldOffset(48)] public int32 FiContinue;
    [FieldOffset(52)] public int32 FiBreak;
    [FieldOffset(56)] public int32 FsavedNQueryLoop;
    [FieldOffset(60)] public Tu16 FwctrlFlags;
    [FieldOffset(62)] public TLogEst FiLimit;
    [FieldOffset(64)] public Tu8 FnLevel;
    [FieldOffset(65)] public Ti8 FnOBSat;
    [FieldOffset(66)] public Tu8 FeOnePass;
    [FieldOffset(67)] public Tu8 FeDistinct;
    [FieldOffset(68)] public uint8 F__ccgo68;
    [FieldOffset(70)] public TLogEst FnRowOut;
    [FieldOffset(72)] public int32 FiTop;
    [FieldOffset(76)] public int32 FiEndWhere;
    [FieldOffset(80)] public uintptr FpLoops;
    [FieldOffset(88)] public uintptr FpMemToFree;
    [FieldOffset(96)] public TBitmask FrevMask;
    [FieldOffset(104)] public TWhereClause FsWC;
    [FieldOffset(592)] public TWhereMaskSet FsMaskSet;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 4), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct TYYMINORTYPEᴛ1_Fyy383 /*dyn*/ {
    [FieldOffset(0)] public int32 Fvalue;
    [FieldOffset(4)] public int32 Fmask;
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 8), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct TYYMINORTYPEᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<TToken>))] public readonly long Fyy0;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Fyy14;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Fyy59;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Fyy67;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Fyy122;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Fyy132;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<int32>))] public readonly int Fyy144;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Fyy168;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Fyy203;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Fyy211;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<TOnOrUsing>))] public readonly long Fyy269;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<TTrigEvent>))] public readonly long Fyy286;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<TYYMINORTYPEᴛ1_Fyy383>))] public readonly int Fyy383;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<Tu32>))] public readonly int Fyy391;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Fyy427;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Fyy454;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<Tu8>))] public readonly byte Fyy462;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<TFrameBound>))] public readonly long Fyy509;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Fyy555;
    [FieldOffset(0)] public int32 Fyyinit;
    [FieldOffset(4)] public array12<byte> F__ccgo_pad20;
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 8), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct Tfts5YYMINORTYPEᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<TFts5Token>))] public readonly long Ffts5yy0;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<int32>))] public readonly int Ffts5yy4;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Ffts5yy11;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Ffts5yy24;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Ffts5yy46;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Ffts5yy53;
    [FieldOffset(0)] public int32 Ffts5yyinit;
    [FieldOffset(4)] public array12<byte> F__ccgo_pad7;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct Tp4unionᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Fp;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Fz;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long FpFunc;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long FpCtx;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long FpColl;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long FpMem;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long FpVtab;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long FpKeyInfo;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long Fai;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long FpProgram;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long FpTab;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long FpSubrtnSig;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long FpIdx;
    [FieldOffset(0)] public int32 Fi;
    [FieldOffset(4)] public array4<byte> F__ccgo_pad14;
}

public static UntypedInt UINTPTR_MAX => 18446744073709551615;

public static UntypedInt WHERE_LOOP_XFER_SZ => 56;

public static int32 /*r*/ Xsqlite3_auto_extension(ж<libc.TLS> tls, uintptr __ccgo_fp_xInit) {
    int32 r = default;

    uintptr aNew = default;
    uintptr mutex = default;
    Tu32 i = default;
    Tu64 nByte = default;
    int32 rc = default;
    _ = aNew;
    _ = i;
    _ = mutex;
    _ = nByte;
    _ = rc;
    rc = SQLITE_OK;
    rc = Xsqlite3_initialize(tls);
    if (rc != 0) {
        return rc;
    } else {
        mutex = _sqlite3MutexAlloc(tls, (int32)SQLITE_MUTEX_STATIC_MAIN);
        Xsqlite3_mutex_enter(tls, mutex);
        i = (uint32)0;
        for (;;) {
            if (!(i < _sqlite3Autoext.FnExt)) {
                break;
            }
            if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(_sqlite3Autoext.FaExt + (uintptr)i * 8)) == __ccgo_fp_xInit) {
                break;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        if (i == _sqlite3Autoext.FnExt) {
            nByte = (uint64)(_sqlite3Autoext.FnExt + libc.Uint32FromInt32(1)) * (uint64)8;
            aNew = Xsqlite3_realloc64(tls, _sqlite3Autoext.FaExt, nByte);
            if (aNew == (uintptr)0) {
                rc = (int32)SQLITE_NOMEM;
            } else {
                _sqlite3Autoext.FaExt = aNew;
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(_sqlite3Autoext.FaExt + (uintptr)_sqlite3Autoext.FnExt * 8))).Value = __ccgo_fp_xInit;
                _sqlite3Autoext.FnExt = _sqlite3Autoext.FnExt + 1;
            }
        }
        Xsqlite3_mutex_leave(tls, mutex);
        return rc;
    }
    return r;
}

public static int32 /*r*/ Xsqlite3_backup_finish(ж<libc.TLS> tls, uintptr p) {
    uintptr pSrcDb = default;
    uintptr pp = default;
    int32 rc = default;
    int32 v1 = default;
    _ = pSrcDb;
    _ = pp;
    _ = rc;
    _ = v1;
    if (p == (uintptr)0) {
        return SQLITE_OK;
    }
    pSrcDb = (@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpSrcDb;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)pSrcDb)).Value.Fmutex);
    _sqlite3BtreeEnter(tls, (@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpSrc);
    if ((@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpDestDb != 0) {
        Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpDestDb)).Value.Fmutex);
    }
    if ((@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpDestDb != 0) {
        (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpSrc)).Value.FnBackup = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpSrc)).Value.FnBackup - 1;
    }
    if ((@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FisAttached != 0) {
        pp = _sqlite3PagerBackupPtr(ref (tls).DerefOrNull(), _sqlite3BtreePager(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpSrc));
        while (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != p) {
            pp = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) + 72;
        }
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpNext;
    }
    if ((@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpDest != 0) {
        _sqlite3BtreeRollback(tls, (@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpDest, SQLITE_OK, 0);
    }
    if ((@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.Frc == (int32)SQLITE_DONE) {
        v1 = SQLITE_OK;
    } else {
        v1 = (@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.Frc;
    }
    rc = v1;
    if ((@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpDestDb != 0) {
        _sqlite3Error(tls, (@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpDestDb, rc);
        _sqlite3LeaveMutexAndCloseZombie(tls, (@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpDestDb);
    }
    _sqlite3BtreeLeave(tls, (@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpSrc);
    if ((@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpDestDb != 0) {
        Xsqlite3_free(tls, p);
    }
    _sqlite3LeaveMutexAndCloseZombie(tls, pSrcDb);
    return rc;
}

public static uintptr /*r*/ Xsqlite3_compileoption_get(ж<libc.TLS> tls, int32 N) {
    uintptr azCompileOpt = default;
    ref var nOpt = ref heap(new int32(), out var ᏑnOpt);
    _ = azCompileOpt;
    _ = nOpt;
    azCompileOpt = _sqlite3CompileOptions(ref (tls).DerefOrNull(), @unsafe.Pointer.FromPinnedBox(ᏑnOpt));
    if (N >= 0 && N < nOpt) {
        return ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(azCompileOpt + (uintptr)N * 8));
    }
    return (uintptr)0;
}

public static int32 /*r*/ Xsqlite3_db_cacheflush(ж<libc.TLS> tls, uintptr db) {
    int32 bSeenBusy = default;
    int32 i = default;
    int32 rc = default;
    int32 v2 = default;
    uintptr pBt = default;
    uintptr pPager = default;
    _ = bSeenBusy;
    _ = i;
    _ = pBt;
    _ = pPager;
    _ = rc;
    _ = v2;
    rc = SQLITE_OK;
    bSeenBusy = 0;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    _sqlite3BtreeEnterAll(tls, db);
    i = 0;
    for (;;) {
        if (!(rc == SQLITE_OK && i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
            break;
        }
        pBt = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)i * 32))).Value.FpBt;
        if (pBt != 0 && _sqlite3BtreeTxnState(ref (tls).DerefOrNull(), pBt) == (int32)SQLITE_TXN_WRITE) {
            pPager = _sqlite3BtreePager(ref (tls).DerefOrNull(), pBt);
            rc = _sqlite3PagerFlush(tls, pPager);
            if (rc == (int32)SQLITE_BUSY) {
                bSeenBusy = (int32)1;
                rc = SQLITE_OK;
            }
        }
        goto _1;
_1:;
        i = i + 1;
    }
    _sqlite3BtreeLeaveAll(tls, db);
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    if (rc == SQLITE_OK && bSeenBusy != 0) {
        v2 = (int32)SQLITE_BUSY;
    } else {
        v2 = rc;
    }
    return v2;
}

public static uintptr /*r*/ Xsqlite3_db_name(ж<libc.TLS> tls, uintptr db, int32 N) {
    uintptr zRet = default;
    _ = zRet;
    zRet = (uintptr)0;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    if (N >= 0 && N < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb) {
        zRet = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)N * 32))).Value.FzDbSName;
    }
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return zRet;
}

public static int32 /*r*/ Xsqlite3_db_release_memory(ж<libc.TLS> tls, uintptr db) {
    int32 i = default;
    uintptr pBt = default;
    uintptr pPager = default;
    _ = i;
    _ = pBt;
    _ = pPager;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    _sqlite3BtreeEnterAll(tls, db);
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
            break;
        }
        pBt = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)i * 32))).Value.FpBt;
        if (pBt != 0) {
            pPager = _sqlite3BtreePager(ref (tls).DerefOrNull(), pBt);
            _sqlite3PagerShrink(tls, pPager);
        }
        goto _1;
_1:;
        i = i + 1;
    }
    _sqlite3BtreeLeaveAll(tls, db);
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return SQLITE_OK;
}

public static int32 /*r*/ Xsqlite3_drop_modules(ж<libc.TLS> tls, uintptr db, uintptr azNames) {
    int32 ii = default;
    uintptr pMod = default;
    uintptr pNext = default;
    uintptr pThis = default;
    _ = ii;
    _ = pMod;
    _ = pNext;
    _ = pThis;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    pThis = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)(db + 584))).Value.Ffirst;
    for (;;) {
        if (!(pThis != 0)) {
            break;
        }
        pMod = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)pThis)).Value.Fdata;
        pNext = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)pThis)).Value.Fnext;
        if (azNames != 0) {
            ii = 0;
            for (;;) {
                if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(azNames + (uintptr)ii * 8)) != (uintptr)0 && libc.Xstrcmp(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(azNames + (uintptr)ii * 8)), (@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FzName) != 0)) {
                    break;
                }
                goto _2;
_2:;
                ii = ii + 1;
            }
            if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(azNames + (uintptr)ii * 8)) != (uintptr)0) {
                goto _1;
            }
        }
        _createModule(tls, db, (@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FzName, (uintptr)0, (uintptr)0, (uintptr)0);
        goto _1;
_1:;
        pThis = pNext;
    }
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return SQLITE_OK;
}

public static void Xsqlite3_free_table(ж<libc.TLS> tls, uintptr azResult) {
    int32 i = default;
    int32 n = default;
    _ = i;
    _ = n;
    if (azResult != 0) {
        azResult -= 8;
        n = (int32)(int64)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(azResult)));
        i = (int32)1;
        for (;;) {
            if (!(i < n)) {
                break;
            }
            if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(azResult + (uintptr)i * 8)) != 0) {
                Xsqlite3_free(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(azResult + (uintptr)i * 8)));
            }
            goto _1;
_1:;
            i = i + 1;
        }
        Xsqlite3_free(tls, azResult);
    }
}

public static uintptr /*r*/ Xsqlite3_get_clientdata(ж<libc.TLS> tls, uintptr db, uintptr zName) {
    uintptr p = default;
    uintptr pResult = default;
    _ = p;
    _ = pResult;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    p = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpDbData;
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        if (libc.Xstrcmp(tls, p + 24, zName) == 0) {
            pResult = (@unsafe.Pointer.To<TDbClientData>((@unsafe.Pointer)p)).Value.FpData;
            Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
            return pResult;
        }
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TDbClientData>((@unsafe.Pointer)p)).Value.FpNext;
    }
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return (uintptr)0;
}

public static void Xsqlite3_interrupt(ж<libc.TLS> tls, uintptr db) {
    libc.AtomicStoreNInt32(db + 440, libc.Int32FromInt32(1), libc.Int32FromInt32(__ATOMIC_RELAXED));
}

public static int32 /*r*/ Xsqlite3_is_interrupted(ж<libc.TLS> tls, uintptr db) {
    return libc.BoolInt32(libc.AtomicLoadNInt32(db + 440, libc.Int32FromInt32(__ATOMIC_RELAXED)) != 0);
}

public static int32 /*r*/ Xsqlite3_limit(ж<libc.TLS> tls, uintptr db, int32 limitId, int32 newLimit) {
    int32 oldLimit = default;
    _ = oldLimit;
    if (limitId < 0 || limitId >= libc.Int32FromInt32(SQLITE_LIMIT_TRIGGER_STEPS) + libc.Int32FromInt32(1)) {
        return -(int32)1;
    }
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    oldLimit = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(db + 136 + (uintptr)limitId * 4));
    if (newLimit >= 0) {
        if (newLimit > _aHardLimit[limitId]) {
            newLimit = _aHardLimit[limitId];
        } else {
            if (newLimit < (int32)SQLITE_MIN_LENGTH && limitId == SQLITE_LIMIT_LENGTH) {
                newLimit = (int32)SQLITE_MIN_LENGTH;
            }
        }
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(db + 136 + (uintptr)limitId * 4))).Value = newLimit;
    }
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return oldLimit;
}

public static uintptr /*r*/ Xsqlite3_profile(ж<libc.TLS> tls, uintptr db, uintptr __ccgo_fp_xProfile, uintptr pArg) {
    uintptr pOld = default;
    uintptr v1 = default;
    _ = pOld;
    _ = v1;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    pOld = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpProfileArg;
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxProfile = __ccgo_fp_xProfile;
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpProfileArg = pArg;
    v1 = db + 110;
    (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & libc.Int32FromInt32(SQLITE_TRACE_NONLEGACY_MASK));
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxProfile != 0) {
        v1 = db + 110;
        (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(SQLITE_TRACE_XPROFILE));
    }
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return pOld;
}

public static void Xsqlite3_result_subtype(ж<libc.TLS> tls, uintptr pCtx, uint32 eSubtype) {
    uintptr pOut = default;
    uintptr v1 = default;
    _ = pOut;
    _ = v1;
    pOut = (@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)pCtx)).Value.FpOut;
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pOut)).Value.FeSubtype = (uint8)(eSubtype & (uint32)0xff);
    v1 = pOut + 20;
    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(MEM_Subtype));
}

public static int32 /*r*/ Xsqlite3_rtree_geometry_callback(ж<libc.TLS> tls, uintptr db, uintptr zGeom, uintptr __ccgo_fp_xGeom, uintptr pContext) {
    uintptr pGeomCtx = default;
    _ = pGeomCtx;
    pGeomCtx = Xsqlite3_malloc(tls, (int32)32);
    if (!(pGeomCtx != 0)) {
        return (int32)SQLITE_NOMEM;
    }
    (@unsafe.Pointer.To<TRtreeGeomCallback>((@unsafe.Pointer)pGeomCtx)).Value.FxGeom = __ccgo_fp_xGeom;
    (@unsafe.Pointer.To<TRtreeGeomCallback>((@unsafe.Pointer)pGeomCtx)).Value.FxQueryFunc = (uintptr)0;
    (@unsafe.Pointer.To<TRtreeGeomCallback>((@unsafe.Pointer)pGeomCtx)).Value.FxDestructor = (uintptr)0;
    (@unsafe.Pointer.To<TRtreeGeomCallback>((@unsafe.Pointer)pGeomCtx)).Value.FpContext = pContext;
    return Xsqlite3_create_function_v2(tls, db, zGeom, -(int32)1, (int32)SQLITE_ANY, pGeomCtx, __ccgo_fp(_geomCallback), (uintptr)0, (uintptr)0, __ccgo_fp(_rtreeFreeCallback));
}

public static int32 /*r*/ Xsqlite3_rtree_query_callback(ж<libc.TLS> tls, uintptr db, uintptr zQueryFunc, uintptr __ccgo_fp_xQueryFunc, uintptr pContext, uintptr __ccgo_fp_xDestructor) {
    uintptr pGeomCtx = default;
    _ = pGeomCtx;
    pGeomCtx = Xsqlite3_malloc(tls, (int32)32);
    if (!(pGeomCtx != 0)) {
        if (__ccgo_fp_xDestructor != 0) {
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>(__ccgo_fp_xDestructor)(tls, pContext);
        }
        return (int32)SQLITE_NOMEM;
    }
    (@unsafe.Pointer.To<TRtreeGeomCallback>((@unsafe.Pointer)pGeomCtx)).Value.FxGeom = (uintptr)0;
    (@unsafe.Pointer.To<TRtreeGeomCallback>((@unsafe.Pointer)pGeomCtx)).Value.FxQueryFunc = __ccgo_fp_xQueryFunc;
    (@unsafe.Pointer.To<TRtreeGeomCallback>((@unsafe.Pointer)pGeomCtx)).Value.FxDestructor = __ccgo_fp_xDestructor;
    (@unsafe.Pointer.To<TRtreeGeomCallback>((@unsafe.Pointer)pGeomCtx)).Value.FpContext = pContext;
    return Xsqlite3_create_function_v2(tls, db, zQueryFunc, -(int32)1, (int32)SQLITE_ANY, pGeomCtx, __ccgo_fp(_geomCallback), (uintptr)0, (uintptr)0, __ccgo_fp(_rtreeFreeCallback));
}

public static void Xsqlite3_set_auxdata(ж<libc.TLS> tls, uintptr pCtx, int32 iArg, uintptr pAux, uintptr __ccgo_fp_xDelete) {
    uintptr pAuxData = default;
    uintptr pVdbe = default;
    _ = pAuxData;
    _ = pVdbe;
    pVdbe = (@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)pCtx)).Value.FpVdbe;
    if (pVdbe == (uintptr)0) {
        goto failed;
    }
    pAuxData = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pVdbe)).Value.FpAuxData;
    for (;;) {
        if (!(pAuxData != 0)) {
            break;
        }
        if ((@unsafe.Pointer.To<TAuxData>((@unsafe.Pointer)pAuxData)).Value.FiAuxArg == iArg && ((@unsafe.Pointer.To<TAuxData>((@unsafe.Pointer)pAuxData)).Value.FiAuxOp == (@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)pCtx)).Value.FiOp || iArg < 0)) {
            break;
        }
        goto _1;
_1:;
        pAuxData = (@unsafe.Pointer.To<TAuxData>((@unsafe.Pointer)pAuxData)).Value.FpNextAux;
    }
    if (pAuxData == (uintptr)0) {
        pAuxData = _sqlite3DbMallocZero(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pVdbe)).Value.Fdb, (uint64)32);
        if (!(pAuxData != 0)) {
            goto failed;
        }
        (@unsafe.Pointer.To<TAuxData>((@unsafe.Pointer)pAuxData)).Value.FiAuxOp = (@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)pCtx)).Value.FiOp;
        (@unsafe.Pointer.To<TAuxData>((@unsafe.Pointer)pAuxData)).Value.FiAuxArg = iArg;
        (@unsafe.Pointer.To<TAuxData>((@unsafe.Pointer)pAuxData)).Value.FpNextAux = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pVdbe)).Value.FpAuxData;
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pVdbe)).Value.FpAuxData = pAuxData;
        if ((@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)pCtx)).Value.FisError == 0) {
            (@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)pCtx)).Value.FisError = -(int32)1;
        }
    } else {
        if ((@unsafe.Pointer.To<TAuxData>((@unsafe.Pointer)pAuxData)).Value.FxDeleteAux != 0) {
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>((@unsafe.Pointer.To<TAuxData>((@unsafe.Pointer)pAuxData)).Value.FxDeleteAux)(tls, (@unsafe.Pointer.To<TAuxData>((@unsafe.Pointer)pAuxData)).Value.FpAux);
        }
    }
    (@unsafe.Pointer.To<TAuxData>((@unsafe.Pointer)pAuxData)).Value.FpAux = pAux;
    (@unsafe.Pointer.To<TAuxData>((@unsafe.Pointer)pAuxData)).Value.FxDeleteAux = __ccgo_fp_xDelete;
    return;
    goto failed;
failed:;
    if (__ccgo_fp_xDelete != 0) {
        GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>(__ccgo_fp_xDelete)(tls, pAux);
    }
}

public static int32 /*r*/ Xsqlite3_shutdown(ж<libc.TLS> tls) {
    if (libc.AtomicLoadPInt32((uintptr)Ꮡ_sqlite3Config + 340) != 0) {
        Xsqlite3_os_end(tls);
        Xsqlite3_reset_auto_extension(tls);
        libc.AtomicStorePInt32((uintptr)Ꮡ_sqlite3Config + 340, 0);
    }
    if (_sqlite3Config.FisPCacheInit != 0) {
        _sqlite3PcacheShutdown(tls);
        _sqlite3Config.FisPCacheInit = 0;
    }
    if (_sqlite3Config.FisMallocInit != 0) {
        _sqlite3MallocEnd(tls);
        _sqlite3Config.FisMallocInit = 0;
        Xsqlite3_data_directory = (uintptr)0;
        Xsqlite3_temp_directory = (uintptr)0;
    }
    if (_sqlite3Config.FisMutexInit != 0) {
        _sqlite3MutexEnd(tls);
        _sqlite3Config.FisMutexInit = 0;
    }
    return SQLITE_OK;
}

public static int32 /*r*/ Xsqlite3_snapshot_recover(ж<libc.TLS> tls, uintptr db, uintptr zDb) {
    int32 iDb = default;
    int32 rc = default;
    uintptr pBt = default;
    _ = iDb;
    _ = pBt;
    _ = rc;
    rc = (int32)SQLITE_ERROR;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    iDb = _sqlite3FindDbName(tls, db, zDb);
    if (iDb == 0 || iDb > (int32)1) {
        pBt = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpBt;
        if (SQLITE_TXN_NONE == _sqlite3BtreeTxnState(ref (tls).DerefOrNull(), pBt)) {
            rc = _sqlite3BtreeBeginTrans(tls, pBt, 0, nil);
            if (rc == SQLITE_OK) {
                rc = _sqlite3PagerSnapshotRecover(tls, _sqlite3BtreePager(ref (tls).DerefOrNull(), pBt));
                _sqlite3BtreeCommit(tls, pBt);
            }
        }
    }
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return rc;
}

public static uintptr /*r*/ Xsqlite3_str_new(ж<libc.TLS> tls, uintptr db) {
    uintptr p = default;
    int32 v1 = default;
    _ = p;
    _ = v1;
    p = Xsqlite3_malloc64(tls, (uint64)32);
    if (p != 0) {
        if (db != 0) {
            v1 = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(db + 136));
        } else {
            v1 = (int32)SQLITE_MAX_LENGTH;
        }
        _sqlite3StrAccumInit(ref (tls).DerefOrNull(), (@unsafe.Pointer)p, (uintptr)0, (uintptr)0, 0, v1);
    } else {
        p = (uintptr)Ꮡ_sqlite3OomStr;
    }
    return p;
}

public static int32 /*r*/ Xsqlite3_trace_v2(ж<libc.TLS> tls, uintptr db, uint32 mTrace, uintptr __ccgo_fp_xTrace, uintptr pArg) {
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    if (mTrace == (uint32)0) {
        __ccgo_fp_xTrace = (uintptr)0;
    }
    if (__ccgo_fp_xTrace == (uintptr)0) {
        mTrace = (uint32)0;
    }
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmTrace = (uint8)mTrace;
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(db + 256))).Value = __ccgo_fp_xTrace;
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpTraceArg = pArg;
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return SQLITE_OK;
}

public static int32 /*r*/ Xsqlite3_txn_state(ж<libc.TLS> tls, uintptr db, uintptr zSchema) {
    int32 iDb = default;
    int32 iTxn = default;
    int32 nDb = default;
    int32 x = default;
    int32 v1 = default;
    uintptr pBt = default;
    _ = iDb;
    _ = iTxn;
    _ = nDb;
    _ = pBt;
    _ = x;
    _ = v1;
    iTxn = -(int32)1;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    if (zSchema != 0) {
        v1 = _sqlite3FindDbName(tls, db, zSchema);
        iDb = v1;
        nDb = v1;
        if (iDb < 0) {
            nDb = nDb - 1;
        }
    } else {
        iDb = 0;
        nDb = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb - (int32)1;
    }
    for (;;) {
        if (!(iDb <= nDb)) {
            break;
        }
        pBt = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpBt;
        if (pBt != (uintptr)0) {
            v1 = _sqlite3BtreeTxnState(ref (tls).DerefOrNull(), pBt);
        } else {
            v1 = SQLITE_TXN_NONE;
        }
        x = v1;
        if (x > iTxn) {
            iTxn = x;
        }
        goto _2;
_2:;
        iDb = iDb + 1;
    }
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return iTxn;
}

public static uintptr /*r*/ Xsqlite3_vtab_collation(ж<libc.TLS> tls, uintptr pIdxInfo, int32 iCons) {
    int32 iTerm = default;
    uintptr pC = default;
    uintptr pHidden = default;
    uintptr pX = default;
    uintptr zRet = default;
    uintptr v1 = default;
    _ = iTerm;
    _ = pC;
    _ = pHidden;
    _ = pX;
    _ = zRet;
    _ = v1;
    pHidden = pIdxInfo + 1 * 96;
    zRet = (uintptr)0;
    if (iCons >= 0 && iCons < (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnConstraint) {
        pC = (uintptr)0;
        iTerm = (@unsafe.Pointer.To<Tsqlite3_index_constraint>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraint + (uintptr)iCons * 12))).Value.FiTermOffset;
        pX = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)_termFromWhereClause(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<THiddenIndexInfo>((@unsafe.Pointer)pHidden)).Value.FpWC, iTerm))).Value.FpExpr;
        if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.FpLeft != 0) {
            pC = _sqlite3ExprCompareCollSeq(tls, (@unsafe.Pointer.To<THiddenIndexInfo>((@unsafe.Pointer)pHidden)).Value.FpParse, (@unsafe.Pointer)pX);
        }
        if (pC != 0) {
            v1 = (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pC)).Value.FzName;
        } else {
            v1 = (uintptr)Ꮡ_sqlite3StrBINARY;
        }
        zRet = v1;
    }
    return zRet;
}

public static int32 /*r*/ Xsqlite3_vtab_distinct(ж<libc.TLS> tls, uintptr pIdxInfo) {
    uintptr pHidden = default;
    _ = pHidden;
    pHidden = pIdxInfo + 1 * 96;
    return (@unsafe.Pointer.To<THiddenIndexInfo>((@unsafe.Pointer)pHidden)).Value.FeDistinct;
}

public static int32 /*r*/ Xsqlite3_vtab_in(ж<libc.TLS> tls, uintptr pIdxInfo, int32 iCons, int32 bHandle) {
    Tu32 m = default;
    uintptr pHidden = default;
    uint32 v1 = default;
    _ = m;
    _ = pHidden;
    _ = v1;
    pHidden = pIdxInfo + 1 * 96;
    if (iCons <= (int32)31) {
        v1 = libc.Uint32FromInt32(1).Lsh((int64)(iCons));
    } else {
        v1 = (uint32)0;
    }
    m = v1;
    if ((Tu32)(m & (@unsafe.Pointer.To<THiddenIndexInfo>((@unsafe.Pointer)pHidden)).Value.FmIn) != 0) {
        if (bHandle == 0) {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pHidden + 24))).Value &= ~m;
        } else {
            if (bHandle > 0) {
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pHidden + 24))).Value |= m;
            }
        }
        return (int32)1;
    }
    return 0;
}

public static int32 /*r*/ Xsqlite3_vtab_rhs_value(ж<libc.TLS> tls, uintptr pIdxInfo, int32 iCons, uintptr ppVal) {
    uintptr pH = default;
    uintptr pTerm = default;
    uintptr pVal = default;
    int32 rc = default;
    _ = pH;
    _ = pTerm;
    _ = pVal;
    _ = rc;
    pH = pIdxInfo + 1 * 96;
    pVal = (uintptr)0;
    rc = SQLITE_OK;
    if (iCons < 0 || iCons >= (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnConstraint) {
        rc = _sqlite3MisuseError(tls, (int32)174731);
    } else {
        if (~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pH + 32 + (uintptr)iCons * 8)) == (uintptr)0) {
            pTerm = _termFromWhereClause(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<THiddenIndexInfo>((@unsafe.Pointer)pH)).Value.FpWC, (@unsafe.Pointer.To<Tsqlite3_index_constraint>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraint + (uintptr)iCons * 12))).Value.FiTermOffset);
            rc = _sqlite3ValueFromExpr(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<THiddenIndexInfo>((@unsafe.Pointer)pH)).Value.FpParse)).Value.Fdb, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.FpRight, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<THiddenIndexInfo>((@unsafe.Pointer)pH)).Value.FpParse)).Value.Fdb)).Value.Fenc, (uint8)SQLITE_AFF_BLOB, (@unsafe.Pointer)(pH + 32 + (uintptr)iCons * 8));
        }
        pVal = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pH + 32 + (uintptr)iCons * 8));
    }
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppVal))).Value = pVal;
    if (rc == SQLITE_OK && pVal == (uintptr)0) {
        rc = (int32)SQLITE_NOTFOUND;
    }
    return rc;
}

public static int32 /*r*/ Xsqlite3_wal_autocheckpoint(ж<libc.TLS> tls, uintptr db, int32 nFrame) {
    if (nFrame > 0) {
        Xsqlite3_wal_hook(tls, db, __ccgo_fp(_sqlite3WalDefaultHook), (uintptr)(int64)nFrame);
    } else {
        Xsqlite3_wal_hook(tls, db, (uintptr)0, (uintptr)0);
    }
    return SQLITE_OK;
}

public static void Xsqlite3changegroup_delete(ж<libc.TLS> tls, uintptr pGrp) {
    int32 ii = default;
    _ = ii;
    if (pGrp != 0) {
        ii = 0;
        for (;;) {
            if (!(ii < (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fcd.FnBufAlloc)) {
                break;
            }
            Xsqlite3_free(tls, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fcd.FaBuf + (uintptr)ii * 16))).Value.FaBuf);
            goto _1;
_1:;
            ii = ii + 1;
        }
        Xsqlite3_free(tls, (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fcd.Frecord.FaBuf);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fcd.FaBuf);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.FzDb);
        _sessionDeleteTable(tls, (uintptr)0, (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.FpList);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Frec.FaBuf);
        Xsqlite3_free(tls, pGrp);
    }
}

public static int32 /*r*/ Xsqlite3changeset_finalize(ж<libc.TLS> tls, uintptr p) {
    int32 i = default;
    int32 rc = default;
    _ = i;
    _ = rc;
    rc = SQLITE_OK;
    if (p != 0) {
        rc = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc;
        if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue != 0) {
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol * (int32)2)) {
                    break;
                }
                _sqlite3ValueFree(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue + (uintptr)i * 8)));
                goto _1;
_1:;
                i = i + 1;
            }
        }
        Xsqlite3_free(tls, (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Ftblhdr.FaBuf);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.Fbuf.FaBuf);
        Xsqlite3_free(tls, p);
    }
    return rc;
}

public static void Xsqlite3session_delete(ж<libc.TLS> tls, uintptr pSession) {
    uintptr db = default;
    ref var pHead = ref heap(new uintptr(), out var ᏑpHead);
    @unsafe.Pointer pp = default;
    _ = db;
    _ = pHead;
    _ = pp;
    db = (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fdb;
    Xsqlite3_mutex_enter(tls, Xsqlite3_db_mutex(tls, db));
    pHead = Xsqlite3_preupdate_hook(tls, db, (uintptr)0, (uintptr)0);
    pp = @unsafe.Pointer.FromBox(ᏑpHead);
    for (;;) {
        if (!(~@unsafe.Pointer.To<uintptr>(pp) != (uintptr)0)) {
            break;
        }
        if (~@unsafe.Pointer.To<uintptr>(pp) == pSession) {
            (@unsafe.Pointer.To<uintptr>(pp)).Value = (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pp)))).Value.FpNext;
            if (pHead != 0) {
                Xsqlite3_preupdate_hook(tls, db, __ccgo_fp(_xPreUpdate), pHead);
            }
            break;
        }
        goto _1;
_1:;
        pp = (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pp) + 80);
    }
    Xsqlite3_mutex_leave(tls, Xsqlite3_db_mutex(tls, db));
    _sqlite3ValueFree(tls, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.FpZeroBlob);
    _sessionDeleteTable(tls, pSession, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.FpTable);
    Xsqlite3_free(tls, pSession);
}

internal static UntypedInt __INTPTR_MAX__ => 9223372036854775807;

internal static UntypedInt __INTPTR_WIDTH__ => 64;

internal static UntypedInt __PTRDIFF_MAX__ => 9223372036854775807;

internal static UntypedInt __PTRDIFF_WIDTH__ => 64;

internal static UntypedInt __SIZEOF_INT128__ => 16;

internal static UntypedInt __SIZEOF_POINTER__ => 8;

internal static UntypedInt __SIZEOF_PTRDIFF_T__ => 8;

internal static UntypedInt __SIZEOF_SIZE_T__ => 8;

internal static UntypedInt __SIZE_MAX__ => 18446744073709551615;

internal static UntypedInt __SIZE_WIDTH__ => 64;

internal static UntypedInt __UINTPTR_MAX__ => 18446744073709551615;

internal static int32 /*r*/ _addAggInfoColumn(ж<libc.TLS> tls, uintptr db, uintptr pInfo) {
    ref var i = ref heap(new int32(), out var Ꮡi);
    _ = i;
    (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pInfo)).Value.FaCol = _sqlite3ArrayAllocate(tls, db, (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pInfo)).Value.FaCol, (int32)32, pInfo + 40, @unsafe.Pointer.FromPinnedBox(Ꮡi));
    return i;
}

internal static int32 /*r*/ _addAggInfoFunc(ж<libc.TLS> tls, uintptr db, uintptr pInfo) {
    ref var i = ref heap(new int32(), out var Ꮡi);
    _ = i;
    (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pInfo)).Value.FaFunc = _sqlite3ArrayAllocate(tls, db, (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pInfo)).Value.FaFunc, (int32)32, pInfo + 56, @unsafe.Pointer.FromPinnedBox(Ꮡi));
    return i;
}

internal static void _addToBlockedList(ref libc.TLS tls, uintptr db) {
    uintptr pp = default;
    _ = pp;
    pp = (uintptr)@unsafe.Pointer.FromBox(Ꮡ_sqlite3BlockedList);
    for (;;) {
        if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != 0 && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))))).Value.FxUnlockNotify != (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxUnlockNotify)) {
            break;
        }
        goto _1;
_1:;
        pp = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) + 864;
    }
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpNextBlocked = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp));
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = db;
}

internal static int32 /*r*/ _addToSavepointBitvecs(ж<libc.TLS> tls, uintptr pPager, TPgno pgno) {
    int32 ii = default;
    int32 rc = default;
    uintptr p = default;
    _ = ii;
    _ = p;
    _ = rc;
    rc = SQLITE_OK;
    ii = 0;
    for (;;) {
        if (!(ii < (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnSavepoint)) {
            break;
        }
        p = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FaSavepoint + (uintptr)ii * 56;
        if (pgno <= (@unsafe.Pointer.To<TPagerSavepoint>((@unsafe.Pointer)p)).Value.FnOrig) {
            rc = rc | _sqlite3BitvecSet(tls, (@unsafe.Pointer.To<TPagerSavepoint>((@unsafe.Pointer)p)).Value.FpInSavepoint, pgno);
        }
        goto _1;
_1:;
        ii = ii + 1;
    }
    return rc;
}

internal static void _addToVTrans(ref libc.TLS tls, uintptr db, uintptr pVTab) {
    int32 v1 = default;
    uintptr v2 = default;
    _ = v1;
    _ = v2;
    v2 = db + 580;
    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaVTrans + (uintptr)v1 * 8))).Value = pVTab;
    _sqlite3VtabLock(ref tls, pVTab);
}

internal static uintptr /*r*/ _allocSpace(ref libc.TLS tls, uintptr p, uintptr pBuf, Tsqlite3_int64 nByte) {
    if (pBuf == (uintptr)0) {
        nByte = nByte;
        if (nByte <= (@unsafe.Pointer.To<TReusableSpace>((@unsafe.Pointer)p)).Value.FnFree) {
            (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(p + 8))).Value -= nByte;
            pBuf = (@unsafe.Pointer.To<TReusableSpace>((@unsafe.Pointer)p)).Value.FpSpace + (uintptr)(@unsafe.Pointer.To<TReusableSpace>((@unsafe.Pointer)p)).Value.FnFree;
        } else {
            (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(p + 16))).Value += nByte;
        }
    }
    return pBuf;
}

internal static int32 /*r*/ _alsoAnInt(ж<libc.TLS> tls, @unsafe.Pointer pRec, float64 rValue, @unsafe.Pointer piValue) {
    Ti64 iValue = default;
    _ = iValue;
    iValue = _sqlite3RealToI64(ref (tls).DerefOrNull(), rValue);
    if (_sqlite3RealSameAsInt(tls, rValue, iValue) != 0) {
        (@unsafe.Pointer.To<Ti64>(piValue)).Value = iValue;
        return (int32)1;
    }
    return libc.BoolInt32(0 == _sqlite3Atoi64(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pRec, 8)), piValue, ~@unsafe.Pointer.To<int32>(@unsafe.Add(pRec, 16)), ~@unsafe.Pointer.To<Tu8>(@unsafe.Add(pRec, 22))));
}

internal static uintptr /*r*/ _alterFindTable(ж<libc.TLS> tls, uintptr pParse, uintptr pSrc, @unsafe.Pointer piDb, @unsafe.Pointer pzDb, int32 bAuth) {
    uintptr db = default;
    uintptr pTab = default;
    int32 iDb = default;
    _ = db;
    _ = iDb;
    _ = pTab;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    pTab = (uintptr)0;
    pTab = _sqlite3LocateTableItem(tls, pParse, (uint32)0, pSrc + 8);
    if (pTab != 0) {
        iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema);
        (@unsafe.Pointer.To<uintptr>(pzDb)).Value = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName;
        (@unsafe.Pointer.To<int32>(piDb)).Value = iDb;
        if (SQLITE_OK != _isRealTable(tls, pParse, pTab, (int32)2) || SQLITE_OK != _isAlterableTable(tls, pParse, pTab)) {
            pTab = (uintptr)0;
        }
    }
    if (pTab != 0 && bAuth != 0) {
        if (_sqlite3AuthCheck(tls, pParse, (int32)SQLITE_ALTER_TABLE, ~@unsafe.Pointer.To<uintptr>(pzDb), (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, (uintptr)0) != 0) {
            pTab = (uintptr)0;
        }
    }
    _sqlite3SrcListDelete(tls, (@unsafe.Pointer)db, pSrc);
    return pTab;
}

internal static void _analyzeDatabase(ж<libc.TLS> tls, uintptr pParse, int32 iDb) {
    uintptr db = default;
    uintptr k = default;
    uintptr pSchema = default;
    uintptr pTab = default;
    int32 iMem = default;
    int32 iStatCur = default;
    int32 iTab = default;
    _ = db;
    _ = iMem;
    _ = iStatCur;
    _ = iTab;
    _ = k;
    _ = pSchema;
    _ = pTab;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    pSchema = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpSchema;
    _sqlite3BeginWriteOperation(tls, pParse, 0, iDb);
    iStatCur = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab;
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 56))).Value += (int32)3;
    _openStatTable(tls, pParse, iDb, iStatCur, (uintptr)0, (uintptr)0);
    iMem = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
    iTab = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab;
    k = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)(pSchema + 8))).Value.Ffirst;
    for (;;) {
        if (!(k != 0)) {
            break;
        }
        pTab = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)k)).Value.Fdata;
        _analyzeOneTable(tls, pParse, pTab, (uintptr)0, iStatCur, iMem, iTab);
        iMem = _sqlite3FirstAvailableRegister(ref (tls).DerefOrNull(), pParse, iMem);
        goto _1;
_1:;
        k = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)k)).Value.Fnext;
    }
    _loadAnalysis(tls, pParse, iDb);
}

internal static void _applyNumericAffinity(ж<libc.TLS> tls, @unsafe.Pointer pRec, int32 bTryForInt) {
    ref var rValue = ref heap(new float64(), out var ᏑrValue);
    int32 rc = default;
    @unsafe.Pointer v1 = default;
    _ = rValue;
    _ = rc;
    _ = v1;
    rc = _sqlite3MemRealValueRC(tls, pRec, @unsafe.Pointer.FromPinnedBox(ᏑrValue));
    if (rc <= 0) {
        return;
    }
    if ((rc & (int32)2) == 0 && _alsoAnInt(tls, pRec, rValue, pRec) != 0) {
        v1 = @unsafe.Add(pRec, 20);
        (@unsafe.Pointer.To<Tu16>(v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>(v1)) | libc.Int32FromInt32(MEM_Int));
    } else {
        (@unsafe.Pointer.To<float64>(pRec)).Value = rValue;
        v1 = @unsafe.Add(pRec, 20);
        (@unsafe.Pointer.To<Tu16>(v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>(v1)) | libc.Int32FromInt32(MEM_Real));
        if (bTryForInt != 0) {
            _sqlite3VdbeIntegerAffinity(ref (tls).DerefOrNull(), pRec);
        }
    }
    v1 = @unsafe.Add(pRec, 20);
    (@unsafe.Pointer.To<Tu16>(v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>(v1)) & ~libc.Int32FromInt32(MEM_Str));
}

internal static void _assignAggregateRegisters(ref libc.TLS tls, uintptr pParse, uintptr pAggInfo) {
    (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FiFirstReg = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnColumn + (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnFunc;
}

internal static void _btreeEnterAll(ж<libc.TLS> tls, uintptr db) {
    int32 i = default;
    uintptr p = default;
    Tu8 skipOk = default;
    _ = i;
    _ = p;
    _ = skipOk;
    skipOk = (uint8)1;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
            break;
        }
        p = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)i * 32))).Value.FpBt;
        if (p != 0 && (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.Fsharable != 0) {
            _sqlite3BtreeEnter(tls, p);
            skipOk = (uint8)0;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnoSharedCache = skipOk;
}

internal static int32 /*r*/ _btreeInvokeBusyHandler(ж<libc.TLS> tls, uintptr pArg) {
    uintptr pBt = default;
    _ = pBt;
    pBt = pArg;
    return _sqlite3InvokeBusyHandler(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.Fdb + 680);
}

internal static void _btreeLeaveAll(ж<libc.TLS> tls, uintptr db) {
    int32 i = default;
    uintptr p = default;
    _ = i;
    _ = p;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
            break;
        }
        p = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)i * 32))).Value.FpBt;
        if (p != 0) {
            _sqlite3BtreeLeave(tls, p);
        }
        goto _1;
_1:;
        i = i + 1;
    }
}

internal static void _btreeReleaseAllCursorPages(ж<libc.TLS> tls, uintptr pCur) {
    int32 i = default;
    _ = i;
    if ((int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage >= 0) {
        i = 0;
        for (;;) {
            if (!(i < (int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage)) {
                break;
            }
            _releasePageNotNull(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pCur + 144 + (uintptr)i * 8)));
            goto _1;
_1:;
            i = i + 1;
        }
        _releasePageNotNull(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage);
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage = (int8)(-(int32)1);
    }
}

internal static Tu16 /*r*/ _cachedCellSize(ж<libc.TLS> tls, @unsafe.Pointer p, int32 N) {
    if (~@unsafe.Pointer.To<Tu16>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24)) + (uintptr)N * 2)) != 0) {
        return ~@unsafe.Pointer.To<Tu16>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24)) + (uintptr)N * 2));
    }
    return _computeCellSize(tls, p, N);
}

internal static void _callFinaliser(ж<libc.TLS> tls, uintptr db, int32 offset) {
    uintptr aVTrans = default;
    uintptr p = default;
    uintptr pVTab = default;
    ref var x = ref heap(new uintptr(), out var Ꮡx);
    int32 i = default;
    _ = aVTrans;
    _ = i;
    _ = p;
    _ = pVTab;
    _ = x;
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaVTrans != 0) {
        aVTrans = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaVTrans;
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaVTrans = (uintptr)0;
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVTrans)) {
                break;
            }
            pVTab = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aVTrans + (uintptr)i * 8));
            p = (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVTab)).Value.FpVtab;
            if (p != 0) {
                x = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_vtab>((@unsafe.Pointer)p)).Value.FpModule + (uintptr)offset));
                if (x != 0) {
                    GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>(x)(tls, p);
                }
            }
            (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVTab)).Value.FiSavepoint = 0;
            _sqlite3VtabUnlock(tls, pVTab);
            goto _1;
_1:;
            i = i + 1;
        }
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, aVTrans);
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVTrans = 0;
    }
}

internal static int32 /*r*/ _checkChangeParams(ж<libc.TLS> tls, uintptr pGrp, int32 bNew, int32 iCol, Tsqlite3_int64 nReq, @unsafe.Pointer ppBuf) {
    uintptr pBuf = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    _ = pBuf;
    _ = rc;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fcd.FpTab == (uintptr)0) {
        rc = (int32)SQLITE_MISUSE;
    } else {
        if (iCol < 0 || iCol >= (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fcd.FpTab)).Value.FnCol) {
            rc = (int32)SQLITE_RANGE;
        } else {
            if (bNew != 0 && (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fcd.FeOp == (int32)SQLITE_DELETE || !(bNew != 0) && (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fcd.FeOp == (int32)SQLITE_INSERT) {
                rc = (int32)SQLITE_ERROR;
            } else {
                pBuf = (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fcd.FaBuf + (uintptr)iCol * 16;
                if ((@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fcd.FeOp == (int32)SQLITE_UPDATE && bNew != 0) {
                    pBuf = pBuf + (uintptr)(@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fcd.FpTab)).Value.FnCol * 16;
                }
                (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FnBuf = 0;
                _sessionBufferGrow(tls, (@unsafe.Pointer)pBuf, nReq, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FnBuf = (int32)nReq;
                (@unsafe.Pointer.To<uintptr>(ppBuf)).Value = pBuf;
            }
        }
    }
    return rc;
}

internal static int32 /*r*/ _checkColumnOverlap(ref libc.TLS tls, uintptr pIdList, uintptr pEList) {
    int32 e = default;
    _ = e;
    if (pIdList == (uintptr)0 || pEList == (uintptr)0) {
        return (int32)1;
    }
    e = 0;
    for (;;) {
        if (!(e < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr)) {
            break;
        }
        if (_sqlite3IdListIndex(ref tls, pIdList, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)e * 32))).Value.FzEName) >= 0) {
            return (int32)1;
        }
        goto _1;
_1:;
        e = e + 1;
    }
    return 0;
}

internal static void _checkOom(ref libc.TLS tls, @unsafe.Pointer pCheck) {
    (@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 36))).Value = (int32)SQLITE_NOMEM;
    (@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 28))).Value = 0;
    if (~@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 32)) == 0) {
        (@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 32))).Value = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 32)) + 1;
    }
}

internal static void _checkProgress(ж<libc.TLS> tls, uintptr pCheck) {
    uintptr db = default;
    _ = db;
    db = (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.Fdb;
    if (libc.AtomicLoadNInt32(db + 440, libc.Int32FromInt32(__ATOMIC_RELAXED)) != 0) {
        (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.Frc = (int32)SQLITE_INTERRUPT;
        (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FnErr = (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FnErr + 1;
        (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FmxErr = 0;
    }
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxProgress != 0) {
        (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FnStep = (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FnStep + 1;
        if ((@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FnStep % (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnProgressOps == (uint32)0 && GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxProgress)(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpProgressArg) != 0) {
            (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.Frc = (int32)SQLITE_INTERRUPT;
            (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FnErr = (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FnErr + 1;
            (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FmxErr = 0;
        }
    }
}

internal static void _clearAllSharedCacheTableLocks(ж<libc.TLS> tls, uintptr p) {
    uintptr pBt = default;
    uintptr pLock = default;
    uintptr ppIter = default;
    uintptr v1 = default;
    _ = pBt;
    _ = pLock;
    _ = ppIter;
    _ = v1;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    ppIter = pBt + 120;
    while (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppIter)) != 0) {
        pLock = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppIter));
        if ((@unsafe.Pointer.To<TBtLock>((@unsafe.Pointer)pLock)).Value.FpBtree == p) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppIter))).Value = (@unsafe.Pointer.To<TBtLock>((@unsafe.Pointer)pLock)).Value.FpNext;
            if ((@unsafe.Pointer.To<TBtLock>((@unsafe.Pointer)pLock)).Value.FiTable != (uint32)1) {
                Xsqlite3_free(tls, pLock);
            }
        } else {
            ppIter = pLock + 16;
        }
    }
    if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpWriter == p) {
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpWriter = (uintptr)0;
        v1 = pBt + 40;
        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(BTS_EXCLUSIVE) | libc.Int32FromInt32(BTS_PENDING)));
    } else {
        if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnTransaction == (int32)2) {
            v1 = pBt + 40;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(BTS_PENDING));
        }
    }
}

internal static void _closeAllCursors(ж<libc.TLS> tls, uintptr p) {
    uintptr pDel = default;
    uintptr pFrame = default;
    _ = pDel;
    _ = pFrame;
    if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpFrame != 0) {
        pFrame = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpFrame;
        for (;;) {
            if (!((@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pFrame)).Value.FpParent != 0)) {
                break;
            }
            goto _1;
_1:;
            pFrame = (@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pFrame)).Value.FpParent;
        }
        _sqlite3VdbeFrameRestore(tls, pFrame);
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpFrame = (uintptr)0;
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnFrame = 0;
    }
    _closeCursorsInFrame(tls, p);
    _releaseMemArray(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaMem, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnMem);
    while ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpDelFrame != 0) {
        pDel = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpDelFrame;
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpDelFrame = (@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pDel)).Value.FpParent;
        _sqlite3VdbeFrameDelete(tls, pDel);
    }
    if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpAuxData != 0) {
        _sqlite3VdbeDeleteAuxData(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb, p + 304, -(int32)1, 0);
    }
}

internal static void _closeCursorsInFrame(ж<libc.TLS> tls, uintptr p) {
    int32 i = default;
    uintptr pC = default;
    _ = i;
    _ = pC;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnCursor)) {
            break;
        }
        pC = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FapCsr + (uintptr)i * 8));
        if (pC != 0) {
            _sqlite3VdbeFreeCursorNN(tls, p, pC);
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FapCsr + (uintptr)i * 8))).Value = (uintptr)0;
        }
        goto _1;
_1:;
        i = i + 1;
    }
}

internal static int32 /*r*/ _codeDistinct(ж<libc.TLS> tls, uintptr pParse, int32 eTnctType, int32 iTab, int32 addrRepeat, uintptr pEList, int32 regElem) {
    int32 i = default;
    int32 iJump = default;
    int32 iRet = default;
    int32 nResultCol = default;
    int32 r1 = default;
    int32 regPrev = default;
    int32 v1 = default;
    uintptr pColl = default;
    uintptr v = default;
    _ = i;
    _ = iJump;
    _ = iRet;
    _ = nResultCol;
    _ = pColl;
    _ = r1;
    _ = regPrev;
    _ = v;
    _ = v1;
    iRet = 0;
    nResultCol = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr;
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    switch (eTnctType) {
    case var ᴛ when ᴛ == (int32)WHERE_DISTINCT_ORDERED: {
        v1 = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + libc.Int32FromInt32(1);
        regPrev = v1;
        iRet = v1;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += nResultCol;
        iJump = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + nResultCol;
        i = 0;
        for (;;) {
            if (!(i < nResultCol)) {
                break;
            }
            pColl = _sqlite3ExprCollSeq(tls, pParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)i * 32))).Value.FpExpr);
            if (i < nResultCol - (int32)1) {
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Ne, regElem + i, iJump, regPrev + i);
            } else {
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Eq, regElem + i, addrRepeat, regPrev + i);
            }
            _sqlite3VdbeChangeP4(tls, v, -(int32)1, pColl, -(int32)2);
            _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)SQLITE_NULLEQ);
            goto _2;
_2:;
            i = i + 1;
        }
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Copy, regElem, regPrev, nResultCol - (int32)1);
        break;
    }
    case var ᴛ when ᴛ == (int32)WHERE_DISTINCT_UNIQUE: {
        break;
    }
    default: {
        r1 = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
        _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_Found, iTab, addrRepeat, regElem, nResultCol);
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_MakeRecord, regElem, nResultCol, r1);
        _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_IdxInsert, iTab, r1, regElem, nResultCol);
        _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)OPFLAG_USESEEKRESULT);
        _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, r1);
        iRet = iTab;
        break;
        break;
    }}

    return iRet;
}

internal static void _codeExprOrVector(ж<libc.TLS> tls, uintptr pParse, uintptr p, int32 iReg, int32 nReg) {
    int32 i = default;
    int32 iSelect = default;
    uintptr pList = default;
    uintptr v = default;
    _ = i;
    _ = iSelect;
    _ = pList;
    _ = v;
    if (p != 0 && _sqlite3ExprIsVector(ref (tls).DerefOrNull(), p) != 0) {
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fflags & (uint32)EP_xIsSelect) != (uint32)0) {
            v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
            iSelect = _sqlite3CodeSubselect(tls, pParse, p);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Copy, iSelect, iReg, nReg - (int32)1);
        } else {
            pList = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 32));
            i = 0;
            for (;;) {
                if (!(i < nReg)) {
                    break;
                }
                _sqlite3ExprCode(tls, pParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32))).Value.FpExpr, iReg + i);
                goto _1;
_1:;
                i = i + 1;
            }
        }
    } else {
        _sqlite3ExprCode(tls, pParse, p, iReg);
    }
}

internal static Tu16 /*r*/ _computeCellSize(ж<libc.TLS> tls, @unsafe.Pointer p, int32 N) {
    (@unsafe.Pointer.To<Tu16>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24)) + (uintptr)N * 2))).Value = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, Tu16>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))))).Value.FxCellSize)(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8)), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16)) + (uintptr)N * 8)));
    return ~@unsafe.Pointer.To<Tu16>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24)) + (uintptr)N * 2));
}

internal static void _concatwsFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    int32 nSep = default;
    uintptr zSep = default;
    _ = nSep;
    _ = zSep;
    nSep = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    zSep = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    if (zSep == (uintptr)0) {
        return;
    }
    _concatFuncCore(tls, context, argc - (int32)1, argv + (uintptr)1 * 8, nSep, zSep);
}

internal static int32 /*r*/ _connectionIsBusy(ref libc.TLS tls, uintptr db) {
    int32 j = default;
    uintptr pBt = default;
    _ = j;
    _ = pBt;
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpVdbe != 0) {
        return (int32)1;
    }
    j = 0;
    for (;;) {
        if (!(j < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
            break;
        }
        pBt = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)j * 32))).Value.FpBt;
        if (pBt != 0 && _sqlite3BtreeIsInBackup(ref tls, pBt) != 0) {
            return (int32)1;
        }
        goto _1;
_1:;
        j = j + 1;
    }
    return 0;
}

internal static int32 /*r*/ _createFunctionApi(ж<libc.TLS> tls, uintptr db, uintptr zFunc, int32 nArg, int32 enc, uintptr p, uintptr __ccgo_fp_xSFunc, uintptr __ccgo_fp_xStep, uintptr __ccgo_fp_xFinal, uintptr __ccgo_fp_xValue, uintptr __ccgo_fp_xInverse, uintptr __ccgo_fp_xDestroy) {
    uintptr pArg = default;
    int32 rc = default;
    _ = pArg;
    _ = rc;
    rc = (int32)SQLITE_ERROR;
    pArg = (uintptr)0;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    if (__ccgo_fp_xDestroy != 0) {
        pArg = _sqlite3Malloc(tls, (uint64)24);
        if (!(pArg != 0)) {
            _sqlite3OomFault(tls, db);
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>(__ccgo_fp_xDestroy)(tls, p);
            goto @out;
        }
        (@unsafe.Pointer.To<TFuncDestructor>((@unsafe.Pointer)pArg)).Value.FnRef = 0;
        (@unsafe.Pointer.To<TFuncDestructor>((@unsafe.Pointer)pArg)).Value.FxDestroy = __ccgo_fp_xDestroy;
        (@unsafe.Pointer.To<TFuncDestructor>((@unsafe.Pointer)pArg)).Value.FpUserData = p;
    }
    rc = _sqlite3CreateFunc(tls, db, zFunc, nArg, enc, p, __ccgo_fp_xSFunc, __ccgo_fp_xStep, __ccgo_fp_xFinal, __ccgo_fp_xValue, __ccgo_fp_xInverse, pArg);
    if (pArg != 0 && (@unsafe.Pointer.To<TFuncDestructor>((@unsafe.Pointer)pArg)).Value.FnRef == 0) {
        GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>(__ccgo_fp_xDestroy)(tls, p);
        Xsqlite3_free(tls, pArg);
    }
    goto @out;
@out:;
    rc = _sqlite3ApiExit(tls, db, rc);
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return rc;
}

internal static int32 /*r*/ _dbpageBeginTrans(ж<libc.TLS> tls, uintptr pTab) {
    uintptr db = default;
    uintptr pBt = default;
    int32 i = default;
    int32 rc = default;
    _ = db;
    _ = i;
    _ = pBt;
    _ = rc;
    db = (@unsafe.Pointer.To<TDbpageTable>((@unsafe.Pointer)pTab)).Value.Fdb;
    rc = SQLITE_OK;
    i = 0;
    for (;;) {
        if (!(rc == SQLITE_OK && i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
            break;
        }
        pBt = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)i * 32))).Value.FpBt;
        if (pBt != 0) {
            rc = _sqlite3BtreeBeginTrans(tls, pBt, (int32)1, nil);
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return rc;
}

internal static int32 /*r*/ _dbpageSync(ж<libc.TLS> tls, uintptr pVtab) {
    uintptr pBt = default;
    uintptr pPager = default;
    uintptr pTab = default;
    _ = pBt;
    _ = pPager;
    _ = pTab;
    pTab = pVtab;
    if ((@unsafe.Pointer.To<TDbpageTable>((@unsafe.Pointer)pTab)).Value.FpgnoTrunc > (uint32)0) {
        pBt = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TDbpageTable>((@unsafe.Pointer)pTab)).Value.Fdb)).Value.FaDb + (uintptr)(@unsafe.Pointer.To<TDbpageTable>((@unsafe.Pointer)pTab)).Value.FiDbTrunc * 32))).Value.FpBt;
        pPager = _sqlite3BtreePager(ref (tls).DerefOrNull(), pBt);
        _sqlite3BtreeEnter(tls, pBt);
        if ((@unsafe.Pointer.To<TDbpageTable>((@unsafe.Pointer)pTab)).Value.FpgnoTrunc < _sqlite3BtreeLastPage(ref (tls).DerefOrNull(), pBt)) {
            _sqlite3PagerTruncateImage(ref (tls).DerefOrNull(), pPager, (@unsafe.Pointer.To<TDbpageTable>((@unsafe.Pointer)pTab)).Value.FpgnoTrunc);
        }
        _sqlite3BtreeLeave(tls, pBt);
    }
    (@unsafe.Pointer.To<TDbpageTable>((@unsafe.Pointer)pTab)).Value.FpgnoTrunc = (uint32)0;
    return SQLITE_OK;
}

internal static void _detachTempTriggers(ref libc.TLS tls, uintptr db, uintptr pSchema) {
    uintptr pEntry = default;
    uintptr pTrig = default;
    _ = pEntry;
    _ = pTrig;
    pEntry = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)((@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + 1 * 32))).Value.FpSchema + 56))).Value.Ffirst;
    while (pEntry != 0) {
        pTrig = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)pEntry)).Value.Fdata;
        if ((@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrig)).Value.FpTabSchema == pSchema) {
            (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrig)).Value.FpTabSchema = (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrig)).Value.FpSchema;
        }
        pEntry = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)pEntry)).Value.Fnext;
    }
}

internal static int32 /*r*/ _doWalCallbacks(ж<libc.TLS> tls, uintptr db) {
    int32 i = default;
    int32 nEntry = default;
    int32 rc = default;
    uintptr pBt = default;
    _ = i;
    _ = nEntry;
    _ = pBt;
    _ = rc;
    rc = SQLITE_OK;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
            break;
        }
        pBt = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)i * 32))).Value.FpBt;
        if (pBt != 0) {
            _sqlite3BtreeEnter(tls, pBt);
            nEntry = _sqlite3PagerWalCallback(ref (tls).DerefOrNull(), _sqlite3BtreePager(ref (tls).DerefOrNull(), pBt));
            _sqlite3BtreeLeave(tls, pBt);
            if (nEntry > 0 && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxWalCallback != 0 && rc == SQLITE_OK) {
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, uintptr, int32, int32>>((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxWalCallback)(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpWalArg, db, (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)i * 32))).Value.FzDbSName, nEntry);
            }
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return rc;
}

internal static void _downgradeAllSharedCacheTableLocks(ref libc.TLS tls, uintptr p) {
    uintptr pBt = default;
    uintptr pLock = default;
    uintptr v1 = default;
    _ = pBt;
    _ = pLock;
    _ = v1;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpWriter == p) {
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpWriter = (uintptr)0;
        v1 = pBt + 40;
        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(BTS_EXCLUSIVE) | libc.Int32FromInt32(BTS_PENDING)));
        pLock = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpLock;
        for (;;) {
            if (!(pLock != 0)) {
                break;
            }
            (@unsafe.Pointer.To<TBtLock>((@unsafe.Pointer)pLock)).Value.FeLock = (uint8)READ_LOCK;
            goto _2;
_2:;
            pLock = (@unsafe.Pointer.To<TBtLock>((@unsafe.Pointer)pLock)).Value.FpNext;
        }
    }
}

internal static int32 /*r*/ _estLikePatternLength(ж<libc.TLS> tls, uintptr p, Tu16 eCode) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        (@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0))).Value = 0;
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FeCode = eCode;
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_exprNodePatternLengthEst);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_sqlite3SelectWalkFail);
        _sqlite3WalkExpr(tls, bp, p);
        r = ~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _estimateTableWidth(ref libc.TLS tls, uintptr pTab) {
    int32 i = default;
    uintptr pTabCol = default;
    uint32 wTable = default;
    _ = i;
    _ = pTabCol;
    _ = wTable;
    wTable = (uint32)0;
    i = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol;
    pTabCol = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol;
    for (;;) {
        if (!(i > 0)) {
            break;
        }
        wTable = wTable + (uint32)(@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pTabCol)).Value.FszEst;
        goto _1;
_1:;
        i = i - 1;
        pTabCol += 16;
    }
    if ((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey < 0) {
        wTable = wTable + 1;
    }
    (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FszTabRow = _sqlite3LogEst(ref tls, (uint64)(wTable * (uint32)4));
}

internal static void _exprListDeleteNN(ж<libc.TLS> tls, @unsafe.Pointer db, uintptr pList) {
    int32 i = default;
    int32 v1 = default;
    uintptr pItem = default;
    _ = i;
    _ = pItem;
    _ = v1;
    i = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr;
    pItem = pList + 8;
    for (;;) {
        _sqlite3ExprDelete(tls, db, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FpExpr);
        if ((@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FzEName != 0) {
            _sqlite3DbNNFreeNN(tls, db, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FzEName);
        }
        pItem += 32;
        goto _2;
_2:;
        i = i - 1;
        v1 = i;
        if (!(v1 > 0)) {
            break;
        }
    }
    _sqlite3DbNNFreeNN(tls, db, pList);
}

internal static int32 /*r*/ _exprNodeIsConstantOrGroupBy(ж<libc.TLS> tls, uintptr pWalker, uintptr pExpr) {
    int32 i = default;
    uintptr p = default;
    uintptr pColl = default;
    uintptr pGroupBy = default;
    _ = i;
    _ = p;
    _ = pColl;
    _ = pGroupBy;
    pGroupBy = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWalker + 40));
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pGroupBy)).Value.FnExpr)) {
            break;
        }
        p = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pGroupBy + 8 + (uintptr)i * 32))).Value.FpExpr;
        if (_sqlite3ExprCompare(tls, (uintptr)0, pExpr, p, -(int32)1) < (int32)2) {
            pColl = _sqlite3ExprNNCollSeq(tls, (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FpParse, p);
            if (_sqlite3IsBinary(ref (tls).DerefOrNull(), pColl) != 0) {
                return (int32)WRC_Prune;
            }
        }
        goto _1;
_1:;
        i = i + 1;
    }
    if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & (uint32)EP_xIsSelect) != (uint32)0) {
        (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FeCode = (uint16)0;
        return (int32)WRC_Abort;
    }
    return _exprNodeIsConstant(tls, pWalker, pExpr);
}

internal static int32 /*r*/ _exprPartidxExprLookup(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, int32 iTarget) {
    int32 addr = default;
    int32 ret = default;
    uintptr p = default;
    uintptr v = default;
    _ = addr;
    _ = p;
    _ = ret;
    _ = v;
    p = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpIdxPartExpr;
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        if ((int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiColumn == (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiIdxCol && (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiTable == (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiDataCur) {
            v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
            addr = 0;
            if ((@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FbMaybeNullRow != 0) {
                addr = _sqlite3VdbeAddOp1(tls, v, (int32)OP_IfNullRow, (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiIdxCur);
            }
            ret = _sqlite3ExprCodeTarget(tls, pParse, (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FpExpr, iTarget);
            _sqlite3VdbeAddOp4(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe, (int32)OP_Affinity, ret, (int32)1, 0, p + 21, (int32)1);
            if (_sqlite3ExprCanReturnSubtype(tls, pParse, (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FpExpr) != 0) {
                _sqlite3VdbeAddOp1(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe, (int32)OP_ClrSubtype, ret);
            }
            if (addr != 0) {
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addr);
                _sqlite3VdbeChangeP3(ref (tls).DerefOrNull(), v, addr, ret);
            }
            return ret;
        }
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FpIENext;
    }
    return 0;
}

internal static uintptr /*r*/ _exprTableColumn(ж<libc.TLS> tls, uintptr db, uintptr pTab, int32 iCursor, Ti16 iCol) {
    uintptr pExpr = default;
    _ = pExpr;
    pExpr = _sqlite3Expr(tls, db, (int32)TK_COLUMN, (uintptr)0);
    if (pExpr != 0) {
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 64))).Value = pTab;
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiTable = iCursor;
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiColumn = iCol;
    }
    return pExpr;
}

internal static void _failConstraintFunc(ж<libc.TLS> tls, uintptr ctx, int32 NotUsed, uintptr argv) {
    int32 err = default;
    uintptr zText = default;
    _ = err;
    _ = zText;
    zText = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    err = Xsqlite3_value_int(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
    _ = NotUsed;
    Xsqlite3_result_error(tls, ctx, zText, -(int32)1);
    Xsqlite3_result_error_code(tls, ctx, err);
}

internal static void _findConstraintFunc(ж<libc.TLS> tls, uintptr ctx, int32 NotUsed, uintptr argv) {
    ref var cmp = ref heap(new int32(), out var Ꮡcmp);
    int32 iOff = default;
    int32 nTok = default;
    ref var t = ref heap(new int32(), out var Ꮡt);
    uintptr zCons = default;
    uintptr zSql = default;
    _ = cmp;
    _ = iOff;
    _ = nTok;
    _ = t;
    _ = zCons;
    _ = zSql;
    zSql = (uintptr)0;
    zCons = (uintptr)0;
    iOff = 0;
    t = 0;
    _ = NotUsed;
    zSql = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    zCons = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
    if (zSql == (uintptr)0 || zCons == (uintptr)0) {
        return;
    }
    while (t != (int32)TK_LP && t != (int32)TK_ILLEGAL) {
        iOff = (int32)((int64)iOff + _sqlite3GetToken(ref (tls).DerefOrNull(), zSql + (uintptr)iOff, @unsafe.Pointer.FromPinnedBox(Ꮡt)));
    }
    while ((int32)1 != 0) {
        iOff = iOff + _getConstraintToken(ref (tls).DerefOrNull(), zSql + (uintptr)iOff, @unsafe.Pointer.FromPinnedBox(Ꮡt));
        if (t == (int32)TK_CONSTRAINT) {
            nTok = 0;
            cmp = 0;
            iOff = iOff + _getWhitespace(ref (tls).DerefOrNull(), zSql + (uintptr)iOff);
            nTok = _getConstraintToken(ref (tls).DerefOrNull(), zSql + (uintptr)iOff, @unsafe.Pointer.FromPinnedBox(Ꮡt));
            if (_quotedCompare(tls, ctx, t, zSql + (uintptr)iOff, nTok, zCons, @unsafe.Pointer.FromPinnedBox(Ꮡcmp)) != 0) {
                return;
            }
            if (cmp == 0) {
                Xsqlite3_result_int(tls, ctx, (int32)1);
                return;
            }
        } else {
            if (t == (int32)TK_ILLEGAL) {
                break;
            }
        }
    }
    Xsqlite3_result_int(tls, ctx, 0);
}

internal static uintptr /*r*/ _findElementWithHash(ref libc.TLS tls, @unsafe.Pointer pH, uintptr pKey, @unsafe.Pointer pHash) {
    uint32 count = default;
    uint32 h = default;
    uintptr elem = default;
    @unsafe.Pointer pEntry = default;
    _ = count;
    _ = elem;
    _ = h;
    _ = pEntry;
    h = _strHash(ref tls, pKey);
    if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pH, 16)) != 0) {
        pEntry = (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pH, 16)) + (uintptr)(h % ~@unsafe.Pointer.To<uint32>(pH)) * 16);
        elem = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pEntry, 8));
        count = ~@unsafe.Pointer.To<uint32>(pEntry);
    } else {
        elem = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pH, 8));
        count = ~@unsafe.Pointer.To<uint32>(@unsafe.Add(pH, 4));
    }
    if (pHash != nil) {
        (@unsafe.Pointer.To<uint32>(pHash)).Value = h;
    }
    while (count != 0) {
        if (h == (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fh && _sqlite3StrICmp(ref tls, (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.FpKey, pKey) == 0) {
            return elem;
        }
        elem = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fnext;
        count = count - 1;
    }
    return (uintptr)Ꮡ_nullElement;
}

internal static int32 /*r*/ _fkChildIsModified(ref libc.TLS tls, uintptr pTab, uintptr p, uintptr aChange, int32 bChngRowid) {
    int32 i = default;
    int32 iChildKey = default;
    _ = i;
    _ = iChildKey;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)p)).Value.FnCol)) {
            break;
        }
        iChildKey = (@unsafe.Pointer.To<TsColMap>((@unsafe.Pointer)(p + 64 + (uintptr)i * 16))).Value.FiFrom;
        if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(aChange + (uintptr)iChildKey * 4)) >= 0) {
            return (int32)1;
        }
        if (iChildKey == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey && bChngRowid != 0) {
            return (int32)1;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return 0;
}

internal static void _freeIndexInfo(ж<libc.TLS> tls, uintptr db, uintptr pIdxInfo) {
    int32 i = default;
    uintptr pHidden = default;
    _ = i;
    _ = pHidden;
    pHidden = pIdxInfo + 1 * 96;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnConstraint)) {
            break;
        }
        _sqlite3ValueFree(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pHidden + 32 + (uintptr)i * 8)));
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pHidden + 32 + (uintptr)i * 8))).Value = (uintptr)0;
        goto _1;
_1:;
        i = i + 1;
    }
    _freeIdxStr(tls, pIdxInfo);
    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, pIdxInfo);
}

internal static void _freeTempSpace(ж<libc.TLS> tls, uintptr pBt) {
    if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpTmpSpace != 0) {
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pBt + 136))).Value -= (uintptr)4;
        _sqlite3PageFree(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpTmpSpace);
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpTmpSpace = (uintptr)0;
    }
}

internal static int32 /*r*/ _fts5ApiQueryPhrase(ж<libc.TLS> tls, uintptr pCtx, int32 iPhrase, uintptr pUserData, uintptr __ccgo_fp_xCallback) {
    uintptr pCsr = default;
    ref var pNew = ref heap(new uintptr(), out var ᏑpNew);
    uintptr pTab = default;
    int32 rc = default;
    _ = pCsr;
    _ = pNew;
    _ = pTab;
    _ = rc;
    pCsr = pCtx;
    pTab = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab;
    pNew = (uintptr)0;
    rc = _fts5OpenMethod__ccgo_gp(tls, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab, @unsafe.Pointer.FromBox(ᏑpNew));
    if (rc == SQLITE_OK) {
        (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pNew)).Value.FePlan = (int32)FTS5_PLAN_MATCH;
        (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pNew)).Value.FiFirstRowid = (int64)(-libc.Int32FromInt32(1)) - (libc.Int64FromUint32(0xffffffffU) | libc.Int64FromInt32(0x7fffffff).Lsh((int64)(libc.Int32FromInt32(32))));
        (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pNew)).Value.FiLastRowid = libc.Int64FromUint32(0xffffffffU) | libc.Int64FromInt32(0x7fffffff).Lsh((int64)(libc.Int32FromInt32(32)));
        (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pNew)).Value.Fbase.FpVtab = pTab;
        rc = _sqlite3Fts5ExprClonePhrase(tls, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr, iPhrase, pNew + 64);
    }
    if (rc == SQLITE_OK) {
        rc = _fts5CursorFirst(tls, pTab, pNew, 0);
        for (;;) {
            if (!(rc == SQLITE_OK && ((@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pNew)).Value.Fcsrflags & (int32)FTS5CSR_EOF) == 0)) {
                break;
            }
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, uintptr, int32>>(__ccgo_fp_xCallback)(tls, (uintptr)Ꮡ_sFts5Api, pNew, pUserData);
            if (rc != SQLITE_OK) {
                if (rc == (int32)SQLITE_DONE) {
                    rc = SQLITE_OK;
                }
                break;
            }
            goto _1;
_1:;
            rc = _fts5NextMethod(tls, pNew);
        }
    }
    _fts5CloseMethod(tls, pNew);
    return rc;
}

internal static int32 /*r*/ _fts5ApiSetAuxdata(ж<libc.TLS> tls, uintptr pCtx, uintptr pPtr, uintptr __ccgo_fp_xDelete) {
    uintptr pCsr = default;
    uintptr pData = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    _ = pCsr;
    _ = pData;
    _ = rc;
    pCsr = pCtx;
    pData = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpAuxdata;
    for (;;) {
        if (!(pData != 0)) {
            break;
        }
        if ((@unsafe.Pointer.To<TFts5Auxdata>((@unsafe.Pointer)pData)).Value.FpAux == (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpAux) {
            break;
        }
        goto _1;
_1:;
        pData = (@unsafe.Pointer.To<TFts5Auxdata>((@unsafe.Pointer)pData)).Value.FpNext;
    }
    if (pData != 0) {
        if ((@unsafe.Pointer.To<TFts5Auxdata>((@unsafe.Pointer)pData)).Value.FxDelete != 0) {
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>((@unsafe.Pointer.To<TFts5Auxdata>((@unsafe.Pointer)pData)).Value.FxDelete)(tls, (@unsafe.Pointer.To<TFts5Auxdata>((@unsafe.Pointer)pData)).Value.FpPtr);
        }
    } else {
        rc = SQLITE_OK;
        pData = _sqlite3Fts5MallocZero(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (int64)32);
        if (pData == (uintptr)0) {
            if (__ccgo_fp_xDelete != 0) {
                GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>(__ccgo_fp_xDelete)(tls, pPtr);
            }
            return rc;
        }
        (@unsafe.Pointer.To<TFts5Auxdata>((@unsafe.Pointer)pData)).Value.FpAux = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpAux;
        (@unsafe.Pointer.To<TFts5Auxdata>((@unsafe.Pointer)pData)).Value.FpNext = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpAuxdata;
        (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpAuxdata = pData;
    }
    (@unsafe.Pointer.To<TFts5Auxdata>((@unsafe.Pointer)pData)).Value.FxDelete = __ccgo_fp_xDelete;
    (@unsafe.Pointer.To<TFts5Auxdata>((@unsafe.Pointer)pData)).Value.FpPtr = pPtr;
    return SQLITE_OK;
}

internal static int32 /*r*/ _fts5CInstIterNext(ж<libc.TLS> tls, @unsafe.Pointer pIter) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 iEnd = default;
        int32 rc = default;
        int32 _ᴛ1 = default;
        int32 _ᴛ2 = default;
        int32 _ᴛ3 = default;
        _ = iEnd;
        _ = rc;
        rc = SQLITE_OK;
        (@unsafe.Pointer.To<int32>(@unsafe.Add(pIter, 28))).Value = -(int32)1;
        (@unsafe.Pointer.To<int32>(@unsafe.Add(pIter, 32))).Value = -(int32)1;
        while (rc == SQLITE_OK && ~@unsafe.Pointer.To<int32>(@unsafe.Add(pIter, 20)) < ~@unsafe.Pointer.To<int32>(@unsafe.Add(pIter, 24))) {
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, uintptr, uintptr, int32>>((@unsafe.Pointer.To<TFts5ExtensionApi>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pIter)))).Value.FxInst)(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pIter, 8)), ~@unsafe.Pointer.To<int32>(@unsafe.Add(pIter, 20)), bp, bp + 4, bp + 8);
            if (rc == SQLITE_OK) {
                if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 4)) == ~@unsafe.Pointer.To<int32>(@unsafe.Add(pIter, 16))) {
                    iEnd = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8)) - (int32)1 + GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32>>((@unsafe.Pointer.To<TFts5ExtensionApi>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pIter)))).Value.FxPhraseSize)(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pIter, 8)), ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)));
                    if (~@unsafe.Pointer.To<int32>(@unsafe.Add(pIter, 28)) < 0) {
                        (@unsafe.Pointer.To<int32>(@unsafe.Add(pIter, 28))).Value = (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8))).Value;
                        (@unsafe.Pointer.To<int32>(@unsafe.Add(pIter, 32))).Value = iEnd;
                    } else {
                        if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8)) <= ~@unsafe.Pointer.To<int32>(@unsafe.Add(pIter, 32))) {
                            if (iEnd > ~@unsafe.Pointer.To<int32>(@unsafe.Add(pIter, 32))) {
                                (@unsafe.Pointer.To<int32>(@unsafe.Add(pIter, 32))).Value = iEnd;
                            }
                        } else {
                            break;
                        }
                    }
                }
                (@unsafe.Pointer.To<int32>(@unsafe.Add(pIter, 20))).Value = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pIter, 20)) + 1;
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _fts5CloseMethod(ж<libc.TLS> tls, uintptr pCursor) {
    uintptr pCsr = default;
    uintptr pTab = default;
    uintptr pp = default;
    _ = pCsr;
    _ = pTab;
    _ = pp;
    if (pCursor != 0) {
        pTab = (@unsafe.Pointer.To<Tsqlite3_vtab_cursor>((@unsafe.Pointer)pCursor)).Value.FpVtab;
        pCsr = pCursor;
        _fts5FreeCursorComponents(tls, pCsr);
        pp = (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.FpGlobal + 88;
        for (;;) {
            if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != pCsr)) {
                break;
            }
            goto _1;
_1:;
            pp = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) + 8;
        }
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpNext;
        Xsqlite3_free(tls, pCsr);
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _fts5ColumnMethod(ж<libc.TLS> tls, uintptr pCursor, uintptr pCtx, int32 iCol) {
    ref var n = ref heap(new int32(), out var Ꮡn);
    int32 rc = default;
    int32 v1 = default;
    uintptr pConfig = default;
    uintptr pCsr = default;
    uintptr pTab = default;
    uintptr pVal = default;
    ref var z = ref heap(new uintptr(), out var Ꮡz);
    bool v2 = default;
    _ = n;
    _ = pConfig;
    _ = pCsr;
    _ = pTab;
    _ = pVal;
    _ = rc;
    _ = z;
    _ = v1;
    _ = v2;
    pTab = (@unsafe.Pointer.To<Tsqlite3_vtab_cursor>((@unsafe.Pointer)pCursor)).Value.FpVtab;
    pConfig = (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpConfig;
    pCsr = pCursor;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FePlan == (int32)FTS5_PLAN_SPECIAL) {
        if (iCol == (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnCol) {
            Xsqlite3_result_int64(tls, pCtx, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FiSpecial);
        }
    } else {
        if (iCol == (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnCol) {
            Xsqlite3_result_int64(tls, pCtx, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FiCsrId);
        } else {
            if (iCol == (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnCol + (int32)1) {
                if ((@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FePlan == (int32)FTS5_PLAN_SOURCE) {
                    _fts5PoslistBlob(tls, pCtx, pCsr);
                } else {
                    if ((@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FePlan == (int32)FTS5_PLAN_MATCH || (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FePlan == (int32)FTS5_PLAN_SORTED_MATCH) {
                        v2 = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpRank != 0; if (!v2) {
                            v1 = _fts5FindRankFunction(tls, pCsr);
                            rc = v1;
                        }
                        if (v2 || SQLITE_OK == v1) {
                            _fts5ApiInvoke(tls, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpRank, pCsr, pCtx, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FnRankArg, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FapRankArg);
                        }
                    }
                }
            } else {
                if (!(Xsqlite3_vtab_nochange(tls, pCtx) != 0) && (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeContent != (int32)FTS5_CONTENT_NONE) {
                    (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FpzErrmsg = pTab + 16;
                    rc = _fts5SeekCursor(tls, pCsr, (int32)1);
                    if (rc == SQLITE_OK) {
                        pVal = Xsqlite3_column_value(tls, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpStmt, iCol + (int32)1);
                        if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FbLocale != 0 && (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeContent == (int32)FTS5_CONTENT_EXTERNAL && _sqlite3Fts5IsLocaleValue(tls, pConfig, pVal) != 0) {
                            z = (uintptr)0;
                            n = 0;
                            rc = _fts5TextFromStmt(tls, pConfig, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpStmt, iCol, @unsafe.Pointer.FromBox(Ꮡz), @unsafe.Pointer.FromPinnedBox(Ꮡn));
                            if (rc == SQLITE_OK) {
                                Xsqlite3_result_text(tls, pCtx, z, n, (uintptr)(-libc.Int32FromInt32(1)));
                            }
                            _sqlite3Fts5ClearLocale(ref (tls).DerefOrNull(), pConfig);
                        } else {
                            Xsqlite3_result_value(tls, pCtx, pVal);
                        }
                    }
                    (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FpzErrmsg = (uintptr)0;
                }
            }
        }
    }
    return rc;
}

internal static void _fts5CsrNewrow(ref libc.TLS tls, uintptr pCsr) {
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pCsr + 80))).Value |= (libc.Int32FromInt32(FTS5CSR_REQUIRE_CONTENT) | libc.Int32FromInt32(FTS5CSR_REQUIRE_DOCSIZE) | libc.Int32FromInt32(FTS5CSR_REQUIRE_INST) | libc.Int32FromInt32(FTS5CSR_REQUIRE_POSLIST));
}

internal static int32 /*r*/ _fts5CsrPoslist(ж<libc.TLS> tls, uintptr pCsr, int32 iPhrase, @unsafe.Pointer pa, @unsafe.Pointer pn) {
    uintptr aPopulator = default;
    uintptr pConfig = default;
    uintptr pSorter = default;
    ref var z = ref heap(new uintptr(), out var Ꮡz);
    int32 bLive = default;
    int32 i = default;
    int32 i1 = default;
    ref var n = ref heap(new int32(), out var Ꮡn);
    int32 rc = default;
    int32 v2 = default;
    _ = aPopulator;
    _ = bLive;
    _ = i;
    _ = i1;
    _ = n;
    _ = pConfig;
    _ = pSorter;
    _ = rc;
    _ = z;
    _ = v2;
    pConfig = (@unsafe.Pointer.To<TFts5Table>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab)).Value.FpConfig;
    rc = SQLITE_OK;
    bLive = libc.BoolInt32((@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpSorter == (uintptr)0);
    if (iPhrase < 0 || iPhrase >= _sqlite3Fts5ExprPhraseCount(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr)) {
        rc = (int32)SQLITE_RANGE;
    } else {
        if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeDetail != FTS5_DETAIL_FULL && _fts5IsContentless(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab, (int32)1) != 0) {
            (@unsafe.Pointer.To<uintptr>(pa)).Value = (uintptr)0;
            (@unsafe.Pointer.To<int32>(pn)).Value = 0;
            return SQLITE_OK;
        } else {
            if (((@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.Fcsrflags & (int32)FTS5CSR_REQUIRE_POSLIST) != 0) {
                if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeDetail != FTS5_DETAIL_FULL) {
                    aPopulator = _sqlite3Fts5ExprClearPoslists(tls, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr, bLive);
                    if (aPopulator == (uintptr)0) {
                        rc = (int32)SQLITE_NOMEM;
                    }
                    if (rc == SQLITE_OK) {
                        rc = _fts5SeekCursor(tls, pCsr, 0);
                    }
                    i = 0;
                    for (;;) {
                        if (!(i < (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnCol && rc == SQLITE_OK)) {
                            break;
                        }
                        z = (uintptr)0;
                        n = 0;
                        rc = _fts5TextFromStmt(tls, pConfig, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpStmt, i, @unsafe.Pointer.FromBox(Ꮡz), @unsafe.Pointer.FromPinnedBox(Ꮡn));
                        if (rc == SQLITE_OK) {
                            rc = _sqlite3Fts5ExprPopulatePoslists(tls, pConfig, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr, aPopulator, i, z, n);
                        }
                        _sqlite3Fts5ClearLocale(ref (tls).DerefOrNull(), pConfig);
                        goto _1;
_1:;
                        i = i + 1;
                    }
                    Xsqlite3_free(tls, aPopulator);
                    if ((@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpSorter != 0) {
                        _sqlite3Fts5ExprCheckPoslists(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr, (@unsafe.Pointer.To<TFts5Sorter>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpSorter)).Value.FiRowid);
                    }
                }
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pCsr + 80))).Value &= ~libc.Int32FromInt32(FTS5CSR_REQUIRE_POSLIST);
            }
        }
    }
    if (rc == SQLITE_OK) {
        if ((@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpSorter != 0 && (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeDetail == FTS5_DETAIL_FULL) {
            pSorter = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpSorter;
            if (iPhrase == 0) {
                v2 = 0;
            } else {
                v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(pSorter + 28 + (uintptr)(iPhrase - (int32)1) * 4));
            }
            i1 = v2;
            (@unsafe.Pointer.To<int32>(pn)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(pSorter + 28 + (uintptr)iPhrase * 4)) - i1;
            (@unsafe.Pointer.To<uintptr>(pa)).Value = (@unsafe.Pointer.To<TFts5Sorter>((@unsafe.Pointer)pSorter)).Value.FaPoslist + (uintptr)i1;
        } else {
            (@unsafe.Pointer.To<int32>(pn)).Value = _sqlite3Fts5ExprPoslist(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr, iPhrase, pa);
        }
    } else {
        (@unsafe.Pointer.To<uintptr>(pa)).Value = (uintptr)0;
        (@unsafe.Pointer.To<int32>(pn)).Value = 0;
    }
    return rc;
}

internal static int32 /*r*/ _fts5CursorFirst(ж<libc.TLS> tls, uintptr pTab, uintptr pCsr, int32 bDesc) {
    uintptr pExpr = default;
    int32 rc = default;
    _ = pExpr;
    _ = rc;
    pExpr = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr;
    rc = _sqlite3Fts5ExprFirst(tls, pExpr, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpIndex, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FiFirstRowid, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FiLastRowid, bDesc);
    if (_sqlite3Fts5ExprEof(ref (tls).DerefOrNull(), pExpr) != 0) {
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pCsr + 80))).Value |= (int32)FTS5CSR_EOF;
    }
    _fts5CsrNewrow(ref (tls).DerefOrNull(), pCsr);
    return rc;
}

internal static int32 /*r*/ _fts5CursorReseek(ж<libc.TLS> tls, uintptr pCsr, @unsafe.Pointer pbSkip) {
    int32 bDesc = default;
    int32 rc = default;
    Ti64 iRowid = default;
    uintptr pTab = default;
    _ = bDesc;
    _ = iRowid;
    _ = pTab;
    _ = rc;
    rc = SQLITE_OK;
    if (((@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.Fcsrflags & (int32)FTS5CSR_REQUIRE_RESEEK) != 0) {
        pTab = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab;
        bDesc = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FbDesc;
        iRowid = _sqlite3Fts5ExprRowid(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr);
        rc = _sqlite3Fts5ExprFirst(tls, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpIndex, iRowid, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FiLastRowid, bDesc);
        if (rc == SQLITE_OK && iRowid != _sqlite3Fts5ExprRowid(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr)) {
            (@unsafe.Pointer.To<int32>(pbSkip)).Value = (int32)1;
        }
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pCsr + 80))).Value &= ~libc.Int32FromInt32(FTS5CSR_REQUIRE_RESEEK);
        _fts5CsrNewrow(ref (tls).DerefOrNull(), pCsr);
        if (_sqlite3Fts5ExprEof(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr) != 0) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pCsr + 80))).Value |= (int32)FTS5CSR_EOF;
            (@unsafe.Pointer.To<int32>(pbSkip)).Value = (int32)1;
        }
    }
    return rc;
}

internal static int32 /*r*/ _fts5ExprCheckPoslists(ref libc.TLS tls, uintptr pNode, Ti64 iRowid) {
    int32 bRet = default;
    int32 i = default;
    int32 i1 = default;
    _ = bRet;
    _ = i;
    _ = i1;
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FiRowid = iRowid;
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbEof = 0;
    var exprᴛ1 = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FeType;
    var matchᴛ1 = false;
    if (exprᴛ1 is 0) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)FTS5_TERM) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)FTS5_STRING) { matchᴛ1 = true;
        return libc.BoolInt32((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FpNear + 24))))).Value.Fposlist.Fn > 0);
    }
    if (exprᴛ1 == (int32)FTS5_AND) { matchᴛ1 = true;
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FnChild)) {
                break;
            }
            if (_fts5ExprCheckPoslists(ref tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48 + (uintptr)i * 8)), iRowid) == 0) {
                _fts5ExprClearPoslists(ref tls, pNode);
                return 0;
            }
            goto _1;
_1:;
            i = i + 1;
        }
    }
    else if (exprᴛ1 == (int32)FTS5_OR) {
        bRet = 0;
        i1 = 0;
        for (;;) {
            if (!(i1 < (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FnChild)) {
                break;
            }
            if (_fts5ExprCheckPoslists(ref tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48 + (uintptr)i1 * 8)), iRowid) != 0) {
                bRet = (int32)1;
            }
            goto _2;
_2:;
            i1 = i1 + 1;
        }
        return bRet;
    }
    else if (!matchᴛ1) { /* default: */
        do {
            if (0 == _fts5ExprCheckPoslists(ref tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48)), iRowid) || 0 != _fts5ExprCheckPoslists(ref tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48 + 1 * 8)), iRowid)) {
                _fts5ExprClearPoslists(ref tls, pNode);
                return 0;
            }
            break;
        } while (false);
    }

    return (int32)1;
}

internal static void _fts5ExprClearPoslists(ref libc.TLS tls, uintptr pNode) {
    int32 i = default;
    _ = i;
    if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FeType == (int32)FTS5_TERM || (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FeType == (int32)FTS5_STRING) {
        (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FpNear + 24))))).Value.Fposlist.Fn = 0;
    } else {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FnChild)) {
                break;
            }
            _fts5ExprClearPoslists(ref tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48 + (uintptr)i * 8)));
            goto _1;
_1:;
            i = i + 1;
        }
    }
}

internal static int32 /*r*/ _fts5ExprNodeFirst(ж<libc.TLS> tls, uintptr pExpr, uintptr pNode) {
    int32 i = default;
    int32 nEof = default;
    int32 rc = default;
    uintptr pChild = default;
    _ = i;
    _ = nEof;
    _ = pChild;
    _ = rc;
    rc = SQLITE_OK;
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbEof = 0;
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbNomatch = 0;
    if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FeType == (int32)FTS5_TERM || (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FeType == (int32)FTS5_STRING) {
        rc = _fts5ExprNearInitAll(tls, pExpr, pNode);
    } else {
        if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FxNext == (uintptr)0) {
            (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbEof = (int32)1;
        } else {
            nEof = 0;
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FnChild && rc == SQLITE_OK)) {
                    break;
                }
                pChild = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48 + (uintptr)i * 8));
                rc = _fts5ExprNodeFirst(tls, pExpr, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48 + (uintptr)i * 8)));
                nEof = nEof + (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pChild)).Value.FbEof;
                goto _1;
_1:;
                i = i + 1;
            }
            (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FiRowid = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48))))).Value.FiRowid;
            switch ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FeType) {
            case var ᴛ when ᴛ == (int32)FTS5_AND: {
                if (nEof > 0) {
                    _fts5ExprSetEof(ref (tls).DerefOrNull(), pNode);
                }
                break;
            }
            case var ᴛ when ᴛ == (int32)FTS5_OR: {
                if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FnChild == nEof) {
                    _fts5ExprSetEof(ref (tls).DerefOrNull(), pNode);
                }
                break;
            }
            default: {
                (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbEof = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48))))).Value.FbEof;
                break;
                break;
            }}

        }
    }
    if (rc == SQLITE_OK) {
        rc = _fts5ExprNodeTest(tls, pExpr, pNode);
    }
    return rc;
}

internal static int32 /*r*/ _fts5ExprNodeNext_AND(ж<libc.TLS> tls, uintptr pExpr, uintptr pNode, int32 bFromValid, Ti64 iFrom) {
    int32 rc = default;
    _ = rc;
    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, Ti64, int32>>((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48))))).Value.FxNext)(tls, pExpr, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48)), bFromValid, iFrom);
    if (rc == SQLITE_OK) {
        rc = _fts5ExprNodeTest_AND(tls, pExpr, pNode);
    } else {
        (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbNomatch = 0;
    }
    return rc;
}

internal static int32 /*r*/ _fts5ExprNodeNext_NOT(ж<libc.TLS> tls, uintptr pExpr, uintptr pNode, int32 bFromValid, Ti64 iFrom) {
    int32 rc = default;
    _ = rc;
    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, Ti64, int32>>((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48))))).Value.FxNext)(tls, pExpr, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48)), bFromValid, iFrom);
    if (rc == SQLITE_OK) {
        rc = _fts5ExprNodeTest_NOT(tls, pExpr, pNode);
    }
    if (rc != SQLITE_OK) {
        (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbNomatch = 0;
    }
    return rc;
}

internal static int32 /*r*/ _fts5ExprNodeNext_OR(ж<libc.TLS> tls, uintptr pExpr, uintptr pNode, int32 bFromValid, Ti64 iFrom) {
    int32 i = default;
    int32 rc = default;
    Ti64 iLast = default;
    uintptr p1 = default;
    _ = i;
    _ = iLast;
    _ = p1;
    _ = rc;
    iLast = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FiRowid;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FnChild)) {
            break;
        }
        p1 = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48 + (uintptr)i * 8));
        if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p1)).Value.FbEof == 0) {
            if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p1)).Value.FiRowid == iLast || bFromValid != 0 && _fts5RowidCmp(ref (tls).DerefOrNull(), pExpr, (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p1)).Value.FiRowid, iFrom) < 0) {
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, Ti64, int32>>((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p1)).Value.FxNext)(tls, pExpr, p1, bFromValid, iFrom);
                if (rc != SQLITE_OK) {
                    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbNomatch = 0;
                    return rc;
                }
            }
        }
        goto _1;
_1:;
        i = i + 1;
    }
    _fts5ExprNodeTest_OR(ref (tls).DerefOrNull(), pExpr, pNode);
    return SQLITE_OK;
}

internal static int32 /*r*/ _fts5ExprNodeTest_AND(ж<libc.TLS> tls, uintptr pExpr, uintptr pAnd) {
    int32 bMatch = default;
    int32 cmp = default;
    int32 iChild = default;
    int32 rc = default;
    Ti64 iLast = default;
    uintptr pChild = default;
    _ = bMatch;
    _ = cmp;
    _ = iChild;
    _ = iLast;
    _ = pChild;
    _ = rc;
    iLast = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pAnd)).Value.FiRowid;
    rc = SQLITE_OK;
    for (var cond = true; cond; cond = bMatch == 0) {
        (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pAnd)).Value.FbNomatch = 0;
        bMatch = (int32)1;
        iChild = 0;
        for (;;) {
            if (!(iChild < (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pAnd)).Value.FnChild)) {
                break;
            }
            pChild = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pAnd + 48 + (uintptr)iChild * 8));
            cmp = _fts5RowidCmp(ref (tls).DerefOrNull(), pExpr, iLast, (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pChild)).Value.FiRowid);
            if (cmp > 0) {
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, Ti64, int32>>((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pChild)).Value.FxNext)(tls, pExpr, pChild, (int32)1, iLast);
                if (rc != SQLITE_OK) {
                    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pAnd)).Value.FbNomatch = 0;
                    return rc;
                }
            }
            if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pChild)).Value.FbEof != 0) {
                _fts5ExprSetEof(ref (tls).DerefOrNull(), pAnd);
                bMatch = (int32)1;
                break;
            } else {
                if (iLast != (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pChild)).Value.FiRowid) {
                    bMatch = 0;
                    iLast = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pChild)).Value.FiRowid;
                }
            }
            if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pChild)).Value.FbNomatch != 0) {
                (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pAnd)).Value.FbNomatch = (int32)1;
            }
            goto _1;
_1:;
            iChild = iChild + 1;
        }
    }
    if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pAnd)).Value.FbNomatch != 0 && pAnd != (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FpRoot) {
        _fts5ExprNodeZeroPoslist(ref (tls).DerefOrNull(), pAnd);
    }
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pAnd)).Value.FiRowid = iLast;
    return SQLITE_OK;
}

internal static int32 /*r*/ _fts5ExprNodeTest_NOT(ж<libc.TLS> tls, uintptr pExpr, uintptr pNode) {
    int32 cmp = default;
    int32 rc = default;
    uintptr p1 = default;
    uintptr p2 = default;
    _ = cmp;
    _ = p1;
    _ = p2;
    _ = rc;
    rc = SQLITE_OK;
    p1 = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48));
    p2 = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48 + 1 * 8));
    while (rc == SQLITE_OK && (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p1)).Value.FbEof == 0) {
        cmp = _fts5NodeCompare(ref (tls).DerefOrNull(), pExpr, p1, p2);
        if (cmp > 0) {
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, Ti64, int32>>((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p2)).Value.FxNext)(tls, pExpr, p2, (int32)1, (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p1)).Value.FiRowid);
            cmp = _fts5NodeCompare(ref (tls).DerefOrNull(), pExpr, p1, p2);
        }
        if (cmp != 0 || (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p2)).Value.FbNomatch != 0) {
            break;
        }
        rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, Ti64, int32>>((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p1)).Value.FxNext)(tls, pExpr, p1, 0, (int64)libc.Int32FromInt32(0));
    }
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbEof = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p1)).Value.FbEof;
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbNomatch = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p1)).Value.FbNomatch;
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FiRowid = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p1)).Value.FiRowid;
    if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p1)).Value.FbEof != 0) {
        _fts5ExprNodeZeroPoslist(ref (tls).DerefOrNull(), p2);
    }
    return rc;
}

internal static void _fts5ExprNodeTest_OR(ref libc.TLS tls, uintptr pExpr, uintptr pNode) {
    int32 cmp = default;
    int32 i = default;
    uintptr pChild = default;
    uintptr pNext = default;
    _ = cmp;
    _ = i;
    _ = pChild;
    _ = pNext;
    pNext = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48));
    i = (int32)1;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FnChild)) {
            break;
        }
        pChild = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48 + (uintptr)i * 8));
        cmp = _fts5NodeCompare(ref tls, pExpr, pNext, pChild);
        if (cmp > 0 || cmp == 0 && (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pChild)).Value.FbNomatch == 0) {
            pNext = pChild;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FiRowid = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNext)).Value.FiRowid;
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbEof = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNext)).Value.FbEof;
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbNomatch = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNext)).Value.FbNomatch;
}

internal static int32 /*r*/ _fts5ExprNodeTest_STRING(ж<libc.TLS> tls, uintptr pExpr, uintptr pNode) {
    int32 bDesc = default;
    int32 bMatch = default;
    int32 i = default;
    int32 j = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    ref var iLast = ref heap(new Ti64(), out var ᏑiLast);
    Ti64 iRowid = default;
    uintptr pIter = default;
    uintptr pLeft = default;
    uintptr pNear = default;
    uintptr pPhrase = default;
    uintptr pTerm = default;
    _ = bDesc;
    _ = bMatch;
    _ = i;
    _ = iLast;
    _ = iRowid;
    _ = j;
    _ = pIter;
    _ = pLeft;
    _ = pNear;
    _ = pPhrase;
    _ = pTerm;
    _ = rc;
    pNear = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FpNear;
    pLeft = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNear + 24));
    rc = SQLITE_OK;
    bDesc = (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FbDesc;
    if ((@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pLeft + 32))).Value.FpSynonym != 0) {
        iLast = _fts5ExprSynonymRowid(ref (tls).DerefOrNull(), pLeft + 32, bDesc, nil);
    } else {
        iLast = (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pLeft + 32))).Value.FpIter)).Value.FiRowid;
    }
    for (var cond = true; cond; cond = bMatch == 0) {
        bMatch = (int32)1;
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FnPhrase)) {
                break;
            }
            pPhrase = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNear + 24 + (uintptr)i * 8));
            j = 0;
            for (;;) {
                if (!(j < (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm)) {
                    break;
                }
                pTerm = pPhrase + 32 + (uintptr)j * 40;
                if ((@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FpSynonym != 0) {
                    iRowid = _fts5ExprSynonymRowid(ref (tls).DerefOrNull(), pTerm, bDesc, nil);
                    if (iRowid == iLast) {
                        goto _2;
                    }
                    bMatch = 0;
                    if (_fts5ExprSynonymAdvanceto(tls, pTerm, bDesc, @unsafe.Pointer.FromPinnedBox(ᏑiLast), @unsafe.Pointer.FromPinnedBox(Ꮡrc)) != 0) {
                        (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbNomatch = 0;
                        (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbEof = (int32)1;
                        return rc;
                    }
                } else {
                    pIter = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pPhrase + 32 + (uintptr)j * 40))).Value.FpIter;
                    if ((@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)pIter)).Value.FiRowid == iLast) {
                        goto _2;
                    }
                    bMatch = 0;
                    if (_fts5ExprAdvanceto(tls, pIter, bDesc, @unsafe.Pointer.FromPinnedBox(ᏑiLast), @unsafe.Pointer.FromPinnedBox(Ꮡrc), pNode + 4) != 0) {
                        return rc;
                    }
                }
                goto _2;
_2:;
                j = j + 1;
            }
            goto _1;
_1:;
            i = i + 1;
        }
    }
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FiRowid = iLast;
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbNomatch = libc.BoolInt32(0 == _fts5ExprNearTest(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), pExpr, pNode) && rc == SQLITE_OK);
    return rc;
}

internal static int32 /*r*/ _fts5ExprNodeTest_TERM(ref libc.TLS tls, uintptr pExpr, uintptr pNode) {
    uintptr pIter = default;
    uintptr pPhrase = default;
    _ = pIter;
    _ = pPhrase;
    pPhrase = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FpNear + 24));
    pIter = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pPhrase + 32))).Value.FpIter;
    (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.Fposlist.Fn = (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)pIter)).Value.FnData;
    if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FpConfig)).Value.FeDetail == FTS5_DETAIL_FULL) {
        (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.Fposlist.Fp = (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)pIter)).Value.FpData;
    }
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FiRowid = (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)pIter)).Value.FiRowid;
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbNomatch = libc.BoolInt32((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.Fposlist.Fn == 0);
    return SQLITE_OK;
}

internal static void _fts5ExprNodeZeroPoslist(ref libc.TLS tls, uintptr pNode) {
    int32 i = default;
    int32 i1 = default;
    uintptr pNear = default;
    uintptr pPhrase = default;
    _ = i;
    _ = i1;
    _ = pNear;
    _ = pPhrase;
    if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FeType == (int32)FTS5_STRING || (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FeType == (int32)FTS5_TERM) {
        pNear = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FpNear;
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FnPhrase)) {
                break;
            }
            pPhrase = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNear + 24 + (uintptr)i * 8));
            (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.Fposlist.Fn = 0;
            goto _1;
_1:;
            i = i + 1;
        }
    } else {
        i1 = 0;
        for (;;) {
            if (!(i1 < (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FnChild)) {
                break;
            }
            _fts5ExprNodeZeroPoslist(ref tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48 + (uintptr)i1 * 8)));
            goto _2;
_2:;
            i1 = i1 + 1;
        }
    }
}

internal static void _fts5ExprPhraseFree(ж<libc.TLS> tls, uintptr pPhrase) {
    int32 i = default;
    uintptr pNext = default;
    uintptr pSyn = default;
    uintptr pTerm = default;
    _ = i;
    _ = pNext;
    _ = pSyn;
    _ = pTerm;
    if (pPhrase != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm)) {
                break;
            }
            pTerm = pPhrase + 32 + (uintptr)i * 40;
            Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FpTerm);
            _sqlite3Fts5IterClose(tls, (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FpIter);
            pSyn = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FpSynonym;
            for (;;) {
                if (!(pSyn != 0)) {
                    break;
                }
                pNext = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pSyn)).Value.FpSynonym;
                _sqlite3Fts5IterClose(tls, (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pSyn)).Value.FpIter);
                _sqlite3Fts5BufferFree(tls, pSyn + 1 * 40);
                Xsqlite3_free(tls, pSyn);
                goto _2;
_2:;
                pSyn = pNext;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        if ((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.Fposlist.FnSpace > 0) {
            _sqlite3Fts5BufferFree(tls, pPhrase + 8);
        }
        Xsqlite3_free(tls, pPhrase);
    }
}

internal static void _fts5ExprSetEof(ref libc.TLS tls, uintptr pNode) {
    int32 i = default;
    _ = i;
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbEof = (int32)1;
    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbNomatch = 0;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FnChild)) {
            break;
        }
        _fts5ExprSetEof(ref tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48 + (uintptr)i * 8)));
        goto _1;
_1:;
        i = i + 1;
    }
}

internal static int32 /*r*/ _fts5FindTokenizer_v2(ж<libc.TLS> tls, uintptr pApi, uintptr zName, uintptr ppUserData, uintptr ppTokenizer) {
    uintptr pMod = default;
    int32 rc = default;
    _ = pMod;
    _ = rc;
    rc = SQLITE_OK;
    pMod = _fts5LocateTokenizer(tls, pApi, zName);
    if (pMod != 0) {
        if ((@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pMod)).Value.FbV2Native != 0) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppUserData))).Value = (@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pMod)).Value.FpUserData;
        } else {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppUserData))).Value = pMod;
        }
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppTokenizer))).Value = pMod + 48;
    } else {
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppTokenizer))).Value = (uintptr)0;
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppUserData))).Value = (uintptr)0;
        rc = (int32)SQLITE_ERROR;
    }
    return rc;
}

internal static int32 /*r*/ _fts5HighlightCb(ж<libc.TLS> tls, uintptr pContext, int32 tflags, uintptr pToken, int32 nToken, int32 iStartOff, int32 iEndOff) {
    int32 iPos = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    int32 v1 = default;
    uintptr p = default;
    uintptr v2 = default;
    _ = iPos;
    _ = p;
    _ = rc;
    _ = v1;
    _ = v2;
    p = pContext;
    rc = SQLITE_OK;
    _ = pToken;
    _ = nToken;
    if ((tflags & (int32)FTS5_TOKEN_COLOCATED) != 0) {
        return SQLITE_OK;
    }
    v2 = p + 80;
    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
    iPos = v1;
    if ((@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiRangeEnd >= 0) {
        if (iPos < (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiRangeStart || iPos > (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiRangeEnd) {
            return SQLITE_OK;
        }
        if ((@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiRangeStart != 0 && iPos == (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiRangeStart) {
            (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiOff = iStartOff;
        }
    }
    if ((@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FbOpen != 0 && (iPos <= (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.Fiter.FiStart || (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.Fiter.FiStart < 0) && iStartOff > (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiOff) {
        _fts5HighlightAppend(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (@unsafe.Pointer)p, (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FzClose, -(int32)1);
        (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FbOpen = 0;
    }
    if (iPos == (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.Fiter.FiStart && (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FbOpen == 0) {
        _fts5HighlightAppend(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (@unsafe.Pointer)p, (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FzIn + (uintptr)(@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiOff, iStartOff - (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiOff);
        _fts5HighlightAppend(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (@unsafe.Pointer)p, (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FzOpen, -(int32)1);
        (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiOff = iStartOff;
        (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FbOpen = (int32)1;
    }
    if (iPos == (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.Fiter.FiEnd) {
        if ((@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FbOpen == 0) {
            _fts5HighlightAppend(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (@unsafe.Pointer)p, (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FzOpen, -(int32)1);
            (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FbOpen = (int32)1;
        }
        _fts5HighlightAppend(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (@unsafe.Pointer)p, (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FzIn + (uintptr)(@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiOff, iEndOff - (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiOff);
        (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiOff = iEndOff;
        if (rc == SQLITE_OK) {
            rc = _fts5CInstIterNext(tls, (@unsafe.Pointer)(p + 40));
        }
    }
    if (iPos == (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiRangeEnd) {
        if ((@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FbOpen != 0) {
            if ((@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.Fiter.FiStart >= 0 && iPos >= (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.Fiter.FiStart) {
                _fts5HighlightAppend(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (@unsafe.Pointer)p, (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FzIn + (uintptr)(@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiOff, iEndOff - (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiOff);
                (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiOff = iEndOff;
            }
            _fts5HighlightAppend(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (@unsafe.Pointer)p, (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FzClose, -(int32)1);
            (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FbOpen = 0;
        }
        _fts5HighlightAppend(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (@unsafe.Pointer)p, (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FzIn + (uintptr)(@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiOff, iEndOff - (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiOff);
        (@unsafe.Pointer.To<THighlightContext>((@unsafe.Pointer)p)).Value.FiOff = iEndOff;
    }
    return rc;
}

internal static uintptr /*r*/ _fts5IdxMalloc(ж<libc.TLS> tls, uintptr p, Tsqlite3_int64 nByte) {
    return _sqlite3Fts5MallocZero(tls, (@unsafe.Pointer)(p + 60), nByte);
}

internal static void _fts5IndexCrisismerge(ж<libc.TLS> tls, uintptr p, @unsafe.Pointer ppStruct) {
    int32 iLvl = default;
    int32 nCrisis = default;
    ref var pStruct = ref heap(new uintptr(), out var ᏑpStruct);
    _ = iLvl;
    _ = nCrisis;
    _ = pStruct;
    nCrisis = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FnCrisisMerge;
    pStruct = ~@unsafe.Pointer.To<uintptr>(ppStruct);
    if (pStruct != 0 && (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnLevel > 0) {
        iLvl = 0;
        while ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pStruct + 32 + (uintptr)iLvl * 16))).Value.FnSeg >= nCrisis) {
            _fts5IndexMergeLevel(tls, p, @unsafe.Pointer.FromBox(ᏑpStruct), iLvl, nil);
            _fts5StructurePromote(tls, p, iLvl + (int32)1, pStruct);
            iLvl = iLvl + 1;
        }
        (@unsafe.Pointer.To<uintptr>(ppStruct)).Value = pStruct;
    }
}

internal static void _fts5IndexFreeArray(ж<libc.TLS> tls, uintptr ap, int32 n) {
    int32 ii = default;
    _ = ii;
    if (ap != 0) {
        ii = 0;
        for (;;) {
            if (!(ii < n)) {
                break;
            }
            _fts5DataRelease(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ap + (uintptr)ii * 8)));
            goto _1;
_1:;
            ii = ii + 1;
        }
        Xsqlite3_free(tls, ap);
    }
}

internal static int32 /*r*/ _fts5IndexMerge(ж<libc.TLS> tls, uintptr p, @unsafe.Pointer ppStruct, int32 nPg, int32 nMin) {
    int32 bRet = default;
    int32 iBestLvl = default;
    int32 iLvl = default;
    int32 nBest = default;
    ref var nRem = ref heap(new int32(), out var ᏑnRem);
    uintptr pLvl = default;
    ref var pStruct = ref heap(new uintptr(), out var ᏑpStruct);
    _ = bRet;
    _ = iBestLvl;
    _ = iLvl;
    _ = nBest;
    _ = nRem;
    _ = pLvl;
    _ = pStruct;
    nRem = nPg;
    bRet = 0;
    pStruct = ~@unsafe.Pointer.To<uintptr>(ppStruct);
    while (nRem > 0 && (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        iBestLvl = 0;
        nBest = 0;
        iLvl = 0;
        for (;;) {
            if (!(iLvl < (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnLevel)) {
                break;
            }
            pLvl = pStruct + 32 + (uintptr)iLvl * 16;
            if ((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnMerge != 0) {
                if ((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnMerge > nBest) {
                    iBestLvl = iLvl;
                    nBest = nMin;
                }
                break;
            }
            if ((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnSeg > nBest) {
                nBest = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnSeg;
                iBestLvl = iLvl;
            }
            goto _1;
_1:;
            iLvl = iLvl + 1;
        }
        if (nBest < nMin) {
            iBestLvl = _fts5IndexFindDeleteMerge(ref (tls).DerefOrNull(), p, pStruct);
        }
        if (iBestLvl < 0) {
            break;
        }
        bRet = (int32)1;
        _fts5IndexMergeLevel(tls, p, @unsafe.Pointer.FromBox(ᏑpStruct), iBestLvl, @unsafe.Pointer.FromPinnedBox(ᏑnRem));
        if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pStruct + 32 + (uintptr)iBestLvl * 16))).Value.FnMerge == 0) {
            _fts5StructurePromote(tls, p, iBestLvl + (int32)1, pStruct);
        }
        if (nMin == (int32)1) {
            nMin = (int32)2;
        }
    }
    (@unsafe.Pointer.To<uintptr>(ppStruct)).Value = pStruct;
    return bRet;
}

internal static int32 /*r*/ _fts5InitVtab(ж<libc.TLS> tls, int32 bCreate, uintptr db, uintptr pAux, int32 argc, uintptr argv, uintptr ppVTab, uintptr pzErr) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr azConfig = default;
        ref var pConfig = ref heap(new uintptr(), out var ᏑpConfig);
        uintptr pGlobal = default;
        uintptr pTab = default;
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        _ = azConfig;
        _ = pConfig;
        _ = pGlobal;
        _ = pTab;
        _ = rc;
        pGlobal = pAux;
        azConfig = argv;
        rc = SQLITE_OK;
        pConfig = (uintptr)0;
        pTab = (uintptr)0;
        pTab = _sqlite3Fts5MallocZero(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (int64)72);
        if (rc == SQLITE_OK) {
            rc = _sqlite3Fts5ConfigParse(tls, pGlobal, db, argc, azConfig, @unsafe.Pointer.FromBox(ᏑpConfig), pzErr);
        }
        if (rc == SQLITE_OK) {
            (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FpzErrmsg = pzErr;
            (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpConfig = pConfig;
            (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.FpGlobal = pGlobal;
            if (bCreate != 0 || _sqlite3Fts5TokenizerPreload(tls, pConfig + 128) != 0) {
                rc = _sqlite3Fts5LoadTokenizer(tls, pConfig);
            }
        }
        if (rc == SQLITE_OK) {
            rc = _sqlite3Fts5IndexOpen(tls, pConfig, bCreate, pTab + 32, pzErr);
        }
        if (rc == SQLITE_OK) {
            rc = _sqlite3Fts5StorageOpen(tls, pConfig, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpIndex, bCreate, pTab + 40, pzErr);
        }
        if (rc == SQLITE_OK) {
            rc = _sqlite3Fts5ConfigDeclareVtab(tls, pConfig);
        }
        if (rc == SQLITE_OK) {
            rc = _sqlite3Fts5ConfigLoad(tls, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpConfig, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpConfig)).Value.FiCookie - (int32)1);
        }
        if (rc == SQLITE_OK && (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeContent == FTS5_CONTENT_NORMAL) {
            rc = Xsqlite3_vtab_config(tls, db, (int32)SQLITE_VTAB_CONSTRAINT_SUPPORT, libc.VaList(bp + 8, libc.Int32FromInt32(1)));
        }
        if (rc == SQLITE_OK) {
            rc = Xsqlite3_vtab_config(tls, db, (int32)SQLITE_VTAB_INNOCUOUS, 0);
        }
        if (pConfig != 0) {
            (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FpzErrmsg = (uintptr)0;
        }
        if (rc != SQLITE_OK) {
            _fts5FreeVtab(tls, pTab);
            pTab = (uintptr)0;
        } else {
            if (bCreate != 0) {
            }
        }
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppVTab))).Value = pTab;
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _fts5IterSetOutputs_Col(ж<libc.TLS> tls, uintptr pIter, uintptr pSeg) {
    _sqlite3Fts5BufferZero(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pIter + 40));
    _fts5SegiterPoslist(tls, (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FpIndex, pSeg, (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FpColset, pIter + 40);
    (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FiRowid = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiRowid;
    (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FpData = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fposlist.Fp;
    (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FnData = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fposlist.Fn;
}

internal static void _fts5IterSetOutputs_Full(ж<libc.TLS> tls, uintptr pIter, uintptr pSeg) {
    uintptr a = default;
    uintptr pColset = default;
    uintptr pRc = default;
    _ = a;
    _ = pColset;
    _ = pRc;
    pColset = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FpColset;
    (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FiRowid = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiRowid;
    if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiLeafOffset + (int64)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FnPos <= (int64)(@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpLeaf)).Value.FszLeaf) {
        a = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpLeaf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiLeafOffset;
        pRc = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FpIndex + 60;
        _sqlite3Fts5BufferZero(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pIter + 40));
        _fts5IndexExtractColset(tls, pRc, pColset, a, (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FnPos, pIter);
    } else {
        _sqlite3Fts5BufferZero(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pIter + 40));
        _fts5SegiterPoslist(tls, (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FpIndex, pSeg, pColset, pIter + 40);
        (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FpData = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fposlist.Fp;
        (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FnData = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fposlist.Fn;
    }
}

internal static void _fts5IterSetOutputs_Nocolset(ж<libc.TLS> tls, uintptr pIter, uintptr pSeg) {
    (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FiRowid = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiRowid;
    (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FnData = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FnPos;
    if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiLeafOffset + (int64)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FnPos <= (int64)(@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpLeaf)).Value.FszLeaf) {
        (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FpData = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpLeaf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiLeafOffset;
    } else {
        _sqlite3Fts5BufferZero(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pIter + 40));
        _fts5SegiterPoslist(tls, (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FpIndex, pSeg, (uintptr)0, pIter + 40);
        (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FpData = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fposlist.Fp;
    }
}

internal static int32 /*r*/ _fts5LookaheadReaderNext(ref libc.TLS tls, uintptr p) {
    (@unsafe.Pointer.To<TFts5LookaheadReader>((@unsafe.Pointer)p)).Value.FiPos = (@unsafe.Pointer.To<TFts5LookaheadReader>((@unsafe.Pointer)p)).Value.FiLookahead;
    if (_sqlite3Fts5PoslistNext64(ref tls, (@unsafe.Pointer.To<TFts5LookaheadReader>((@unsafe.Pointer)p)).Value.Fa, (@unsafe.Pointer.To<TFts5LookaheadReader>((@unsafe.Pointer)p)).Value.Fn, (@unsafe.Pointer)(p + 12), (@unsafe.Pointer)(p + 24)) != 0) {
        (@unsafe.Pointer.To<TFts5LookaheadReader>((@unsafe.Pointer)p)).Value.FiLookahead = libc.Int64FromInt32(1).Lsh((int64)(libc.Int32FromInt32(62)));
    }
    return libc.BoolInt32((@unsafe.Pointer.To<TFts5LookaheadReader>((@unsafe.Pointer)p)).Value.FiPos == libc.Int64FromInt32(1).Lsh((int64)(libc.Int32FromInt32(62))));
}

internal static void _fts5MultiIterAdvanced(ж<libc.TLS> tls, uintptr p, uintptr pIter, int32 iChanged, int32 iMinset) {
    int32 i = default;
    int32 iEq = default;
    int32 v2 = default;
    uintptr pSeg = default;
    _ = i;
    _ = iEq;
    _ = pSeg;
    _ = v2;
    i = ((@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FnSeg + iChanged) / (int32)2;
    for (;;) {
        if (!(i >= iMinset && (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK)) {
            break;
        }
        v2 = _fts5MultiIterDoCompare(tls, pIter, i);
        iEq = v2;
        if (v2 != 0) {
            pSeg = pIter + 104 + (uintptr)iEq * 128;
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, uintptr>>((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FxNext)(tls, p, pSeg, (uintptr)0);
            i = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FnSeg + iEq;
        }
        goto _1;
_1:;
        i = i / (int32)2;
    }
}

internal static void _fts5MultiIterFree(ж<libc.TLS> tls, uintptr pIter) {
    int32 i = default;
    _ = i;
    if (pIter != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FnSeg)) {
                break;
            }
            _fts5SegIterClear(tls, pIter + 104 + (uintptr)i * 128);
            goto _1;
_1:;
            i = i + 1;
        }
        _sqlite3Fts5BufferFree(tls, pIter + 40);
        Xsqlite3_free(tls, pIter);
    }
}

internal static int32 /*r*/ _fts5MultiIterIsEmpty(ref libc.TLS tls, uintptr p, uintptr pIter) {
    uintptr pSeg = default;
    _ = pSeg;
    pSeg = pIter + 104 + (uintptr)(@unsafe.Pointer.To<TFts5CResult>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FaFirst + 1 * 4))).Value.FiFirst * 128;
    return libc.BoolInt32((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpLeaf != 0 && (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FnPos == 0);
}

internal static void _fts5MultiIterNew(ж<libc.TLS> tls, uintptr p, uintptr pStruct, int32 flags, uintptr pColset, uintptr pTerm, int32 nTerm, int32 iLevel, int32 nSegment, @unsafe.Pointer ppOut) {
    int32 iIter = default;
    int32 iSeg = default;
    int32 nSeg = default;
    int32 v1 = default;
    uintptr pEnd = default;
    uintptr pIter = default;
    uintptr pIter1 = default;
    uintptr pLvl = default;
    uintptr pNew = default;
    uintptr pSeg = default;
    uintptr v2 = default;
    _ = iIter;
    _ = iSeg;
    _ = nSeg;
    _ = pEnd;
    _ = pIter;
    _ = pIter1;
    _ = pLvl;
    _ = pNew;
    _ = pSeg;
    _ = v1;
    _ = v2;
    nSeg = 0;
    iIter = 0;
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        if (iLevel < 0) {
            nSeg = (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnSegment;
            nSeg = nSeg + libc.BoolInt32((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpHash != 0 && 0 == (flags & (int32)FTS5INDEX_QUERY_SKIPHASH));
        } else {
            if ((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pStruct + 32 + (uintptr)iLevel * 16))).Value.FnSeg < nSegment) {
                v1 = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pStruct + 32 + (uintptr)iLevel * 16))).Value.FnSeg;
            } else {
                v1 = nSegment;
            }
            nSeg = v1;
        }
    }
    v2 = _fts5MultiIterAlloc(tls, p, nSeg);
    pNew = v2;
    (@unsafe.Pointer.To<uintptr>(ppOut)).Value = v2;
    if (pNew == (uintptr)0) {
        goto fts5MultiIterNew_post_check;
    }
    (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pNew)).Value.FbRev = libc.BoolInt32(0 != (flags & (int32)FTS5INDEX_QUERY_DESC));
    (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pNew)).Value.FbSkipEmpty = libc.BoolUint8(libc.Int32FromInt32(0) != (flags & libc.Int32FromInt32(FTS5INDEX_QUERY_SKIPEMPTY)));
    (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pNew)).Value.FpColset = pColset;
    if ((flags & (int32)FTS5INDEX_QUERY_NOOUTPUT) == 0) {
        _fts5IterSetOutputCb(tls, p + 60, pNew);
    }
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        if (iLevel < 0) {
            pEnd = pStruct + 32 + (uintptr)(@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnLevel * 16;
            if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpHash != 0 && 0 == (flags & (int32)FTS5INDEX_QUERY_SKIPHASH)) {
                v1 = iIter;
                iIter = iIter + 1;
                pIter = pNew + 104 + (uintptr)v1 * 128;
                _fts5SegIterHashInit(tls, p, pTerm, nTerm, flags, pIter);
            }
            pLvl = pStruct + 32;
            for (;;) {
                if (!(pLvl < pEnd)) {
                    break;
                }
                iSeg = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnSeg - (int32)1;
                for (;;) {
                    if (!(iSeg >= 0)) {
                        break;
                    }
                    pSeg = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FaSeg + (uintptr)iSeg * 56;
                    v1 = iIter;
                    iIter = iIter + 1;
                    pIter1 = pNew + 104 + (uintptr)v1 * 128;
                    if (pTerm == (uintptr)0) {
                        _fts5SegIterInit(tls, p, pSeg, pIter1);
                    } else {
                        _fts5SegIterSeekInit(tls, p, pTerm, nTerm, flags, pSeg, pIter1);
                    }
                    goto _5;
_5:;
                    iSeg = iSeg - 1;
                }
                goto _4;
_4:;
                pLvl += 16;
            }
        } else {
            pLvl = pStruct + 32 + (uintptr)iLevel * 16;
            iSeg = nSeg - (int32)1;
            for (;;) {
                if (!(iSeg >= 0)) {
                    break;
                }
                v1 = iIter;
                iIter = iIter + 1;
                _fts5SegIterInit(tls, p, (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FaSeg + (uintptr)iSeg * 56, pNew + 104 + (uintptr)v1 * 128);
                goto _7;
_7:;
                iSeg = iSeg - 1;
            }
        }
    }
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        _fts5MultiIterFinishSetup(tls, p, pNew);
    } else {
        _fts5MultiIterFree(tls, pNew);
        (@unsafe.Pointer.To<uintptr>(ppOut)).Value = (uintptr)0;
    }
    goto fts5MultiIterNew_post_check;
fts5MultiIterNew_post_check:;
    return;
}

internal static Ti64 /*r*/ _fts5MultiIterRowid(ref libc.TLS tls, uintptr pIter) {
    return (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)(pIter + 104 + (uintptr)(@unsafe.Pointer.To<TFts5CResult>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FaFirst + 1 * 4))).Value.FiFirst * 128))).Value.FiRowid;
}

internal static void _fts5MultiIterSetEof(ref libc.TLS tls, uintptr pIter) {
    uintptr pSeg = default;
    _ = pSeg;
    pSeg = pIter + 104 + (uintptr)(@unsafe.Pointer.To<TFts5CResult>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FaFirst + 1 * 4))).Value.FiFirst * 128;
    (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FbEof = libc.BoolUint8((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpLeaf == (uintptr)0);
    (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FiSwitchRowid = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiRowid;
}

internal static uintptr /*r*/ _fts5MultiIterTerm(ref libc.TLS tls, uintptr pIter, @unsafe.Pointer pn) {
    uintptr p = default;
    _ = p;
    p = pIter + 104 + (uintptr)(@unsafe.Pointer.To<TFts5CResult>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FaFirst + 1 * 4))).Value.FiFirst * 128;
    (@unsafe.Pointer.To<int32>(pn)).Value = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)p)).Value.Fterm.Fn;
    return (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)p)).Value.Fterm.Fp;
}

internal static void _fts5ParseSetColset(ж<libc.TLS> tls, @unsafe.Pointer pParse, uintptr pNode, uintptr pColset, @unsafe.Pointer ppFree) {
    int32 i = default;
    uintptr pNear = default;
    _ = i;
    _ = pNear;
    if (~@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 16)) == SQLITE_OK) {
        if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FeType == (int32)FTS5_STRING || (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FeType == (int32)FTS5_TERM) {
            pNear = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FpNear;
            if ((@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FpColset != 0) {
                _fts5MergeColset(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FpColset, pColset);
                if ((@unsafe.Pointer.To<TFts5Colset>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FpColset)).Value.FnCol == 0) {
                    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FeType = FTS5_EOF;
                    (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FxNext = (uintptr)0;
                }
            } else {
                if (~@unsafe.Pointer.To<uintptr>(ppFree) != 0) {
                    (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FpColset = pColset;
                    (@unsafe.Pointer.To<uintptr>(ppFree)).Value = (uintptr)0;
                } else {
                    (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FpColset = _fts5CloneColset(tls, @unsafe.Add(pParse, 16), pColset);
                }
            }
        } else {
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FnChild)) {
                    break;
                }
                _fts5ParseSetColset(tls, pParse, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNode + 48 + (uintptr)i * 8)), pColset, ppFree);
                goto _1;
_1:;
                i = i + 1;
            }
        }
    }
}

internal static int32 /*r*/ _fts5PorterTokenize(ж<libc.TLS> tls, uintptr pTokenizer, uintptr pCtx, int32 flags, uintptr pText, int32 nText, uintptr pLoc, int32 nLoc, uintptr __ccgo_fp_xToken) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr p = default;
        TPorterContext _ᴛ1 = default;
        _ = p;
        p = pTokenizer;
        (@unsafe.Pointer.To<TPorterContext>(__ccgo_ptr(bp))).Value.FxToken = __ccgo_fp_xToken;
        (@unsafe.Pointer.To<TPorterContext>(__ccgo_ptr(bp))).Value.FpCtx = pCtx;
        (@unsafe.Pointer.To<TPorterContext>(__ccgo_ptr(bp))).Value.FaBuf = p + 40;
        r = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, uintptr, int32, uintptr, int32, uintptr, int32>>((@unsafe.Pointer.To<TPorterTokenizer>((@unsafe.Pointer)p)).Value.Ftokenizer_v2.FxTokenize)(tls, (@unsafe.Pointer.To<TPorterTokenizer>((@unsafe.Pointer)p)).Value.FpTokenizer, bp, flags, pText, nText, pLoc, nLoc, __ccgo_fp(_fts5PorterCb));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _fts5PrefixMergerInsertByPosition(ref libc.TLS tls, @unsafe.Pointer ppHead, uintptr p) {
    @unsafe.Pointer pp = default;
    _ = pp;
    if ((@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)p)).Value.FiPos >= 0) {
        pp = ppHead;
        while (~@unsafe.Pointer.To<uintptr>(pp) != 0 && (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)p)).Value.FiPos > (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pp)))).Value.FiPos) {
            pp = (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pp) + 56);
        }
        (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)p)).Value.FpNext = ~@unsafe.Pointer.To<uintptr>(pp);
        (@unsafe.Pointer.To<uintptr>(pp)).Value = p;
    }
}

internal static void _fts5PrefixMergerInsertByRowid(ref libc.TLS tls, @unsafe.Pointer ppHead, uintptr p) {
    @unsafe.Pointer pp = default;
    _ = pp;
    if ((@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)p)).Value.Fiter.FaPoslist != 0) {
        pp = ppHead;
        while (~@unsafe.Pointer.To<uintptr>(pp) != 0 && (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)p)).Value.Fiter.FiRowid > (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pp)))).Value.Fiter.FiRowid) {
            pp = (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pp) + 56);
        }
        (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)p)).Value.FpNext = ~@unsafe.Pointer.To<uintptr>(pp);
        (@unsafe.Pointer.To<uintptr>(pp)).Value = p;
    }
}

internal static void _fts5SegIterNextPage(ж<libc.TLS> tls, uintptr p, uintptr pIter) {
    uintptr pLeaf = default;
    uintptr pSeg = default;
    _ = pLeaf;
    _ = pSeg;
    pSeg = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpSeg;
    _fts5DataRelease(tls, (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf);
    (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiLeafPgno = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiLeafPgno + 1;
    if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpNextLeaf != 0) {
        (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpNextLeaf;
        (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpNextLeaf = (uintptr)0;
    } else {
        if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiLeafPgno <= (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FpgnoLast) {
            (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf = _fts5LeafRead(tls, p, ((int64)(@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FiSegid).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B) + libc.Int32FromInt32(FTS5_DATA_DLI_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B))) + (int64)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiLeafPgno);
        } else {
            (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf = (uintptr)0;
        }
    }
    pLeaf = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf;
    if (pLeaf != 0) {
        (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiPgidxOff = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.FszLeaf;
        if ((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.FszLeaf >= (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.Fnn) {
            (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiEndofDoclist = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.Fnn + (int32)1;
        } else {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pIter + 64))).Value += _sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiPgidxOff, (@unsafe.Pointer)(pIter + 68));
        }
    }
}

internal static void _fts5SegIterNext_Reverse(ж<libc.TLS> tls, uintptr p, uintptr pIter, uintptr pbUnused) {
    uintptr a = default;
    uintptr v1 = default;
    ref var iDelta = ref heap(new Tu64(), out var ᏑiDelta);
    int32 iOff = default;
    _ = a;
    _ = iDelta;
    _ = iOff;
    _ = v1;
    _ = pbUnused;
    if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiRowidOffset > 0) {
        a = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf)).Value.Fp;
        (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiRowidOffset = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiRowidOffset - 1;
        (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiLeafOffset = (int64)(~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FaRowidOffset + (uintptr)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiRowidOffset * 4)));
        _fts5SegIterLoadNPos(ref (tls).DerefOrNull(), p, pIter);
        iOff = (int32)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiLeafOffset;
        if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FeDetail != (int32)FTS5_DETAIL_NONE) {
            iOff = iOff + (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FnPos;
        }
        _sqlite3Fts5GetVarint(ref (tls).DerefOrNull(), (@unsafe.Pointer)(a + (uintptr)iOff), @unsafe.Pointer.FromPinnedBox(ᏑiDelta));
        v1 = pIter + 112;
        (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)v1)).Value = (Ti64)((uint64)(~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)v1)) - iDelta);
    } else {
        _fts5SegIterReverseNewPage(tls, p, pIter);
    }
}

internal static void _fts5SetUniqueFlag(ref libc.TLS tls, uintptr pIdxInfo) {
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pIdxInfo + 80))).Value |= (int32)SQLITE_INDEX_SCAN_UNIQUE;
}

internal static int32 /*r*/ _fts5ShadowName(ж<libc.TLS> tls, uintptr zName) {
    uint32 i = default;
    _ = i;
    i = (uint32)0;
    for (;;) {
        if (!((uint64)i < libc.Uint64FromInt64(40) / libc.Uint64FromInt64(8))) {
            break;
        }
        if (Xsqlite3_stricmp(tls, zName, _azName2[i]) == 0) {
            return (int32)1;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return 0;
}

internal static int32 /*r*/ _fts5SorterNext(ж<libc.TLS> tls, uintptr pCsr) {
    uintptr a = default;
    uintptr aBlob = default;
    uintptr pSorter = default;
    uintptr v1 = default;
    int32 i = default;
    int32 iOff = default;
    ref var iVal = ref heap(new int32(), out var ᏑiVal);
    int32 nBlob = default;
    int32 rc = default;
    _ = a;
    _ = aBlob;
    _ = i;
    _ = iOff;
    _ = iVal;
    _ = nBlob;
    _ = pSorter;
    _ = rc;
    _ = v1;
    pSorter = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpSorter;
    rc = Xsqlite3_step(tls, (@unsafe.Pointer.To<TFts5Sorter>((@unsafe.Pointer)pSorter)).Value.FpStmt);
    if (rc == (int32)SQLITE_DONE) {
        rc = SQLITE_OK;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pCsr + 80))).Value |= (libc.Int32FromInt32(FTS5CSR_EOF) | libc.Int32FromInt32(FTS5CSR_REQUIRE_CONTENT));
    } else {
        if (rc == (int32)SQLITE_ROW) {
            iOff = 0;
            rc = SQLITE_OK;
            (@unsafe.Pointer.To<TFts5Sorter>((@unsafe.Pointer)pSorter)).Value.FiRowid = Xsqlite3_column_int64(tls, (@unsafe.Pointer.To<TFts5Sorter>((@unsafe.Pointer)pSorter)).Value.FpStmt, 0);
            nBlob = Xsqlite3_column_bytes(tls, (@unsafe.Pointer.To<TFts5Sorter>((@unsafe.Pointer)pSorter)).Value.FpStmt, (int32)1);
            v1 = Xsqlite3_column_blob(tls, (@unsafe.Pointer.To<TFts5Sorter>((@unsafe.Pointer)pSorter)).Value.FpStmt, (int32)1);
            a = v1;
            aBlob = v1;
            if (nBlob > 0) {
                i = 0;
                for (;;) {
                    if (!(i < (@unsafe.Pointer.To<TFts5Sorter>((@unsafe.Pointer)pSorter)).Value.FnIdx - (int32)1)) {
                        break;
                    }
                    a = a + (uintptr)_sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), a, @unsafe.Pointer.FromPinnedBox(ᏑiVal));
                    iOff = iOff + iVal;
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)(pSorter + 28 + (uintptr)i * 4))).Value = iOff;
                    goto _2;
_2:;
                    i = i + 1;
                }
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)(pSorter + 28 + (uintptr)i * 4))).Value = (int32)(((t__predefined_ptrdiff_t)(aBlob + (uintptr)nBlob)) - (int64)a);
                (@unsafe.Pointer.To<TFts5Sorter>((@unsafe.Pointer)pSorter)).Value.FaPoslist = a;
            }
            _fts5CsrNewrow(ref (tls).DerefOrNull(), pCsr);
        }
    }
    return rc;
}

internal static int32 /*r*/ _fts5SpecialDelete(ж<libc.TLS> tls, uintptr pTab, uintptr apVal) {
    int32 eType1 = default;
    int32 rc = default;
    Tsqlite3_int64 iDel = default;
    _ = eType1;
    _ = iDel;
    _ = rc;
    rc = SQLITE_OK;
    eType1 = Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apVal + 1 * 8)));
    if (eType1 == (int32)SQLITE_INTEGER) {
        iDel = Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apVal + 1 * 8)));
        rc = _sqlite3Fts5StorageDelete(tls, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.FpStorage, iDel, apVal + 2 * 8, 0);
    }
    return rc;
}

internal static int32 /*r*/ _fts5StorageInsertDocsize(ж<libc.TLS> tls, uintptr p, Ti64 iRowid, @unsafe.Pointer pBuf) {
    ref var iOrigin = ref heap(new Ti64(), out var ᏑiOrigin);
    ref var pReplace = ref heap(new uintptr(), out var ᏑpReplace);
    int32 rc = default;
    _ = iOrigin;
    _ = pReplace;
    _ = rc;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FbColumnsize != 0) {
        pReplace = (uintptr)0;
        rc = _fts5StorageGetStmt(tls, p, (int32)FTS5_STMT_REPLACE_DOCSIZE, @unsafe.Pointer.FromBox(ᏑpReplace), (uintptr)0);
        if (rc == SQLITE_OK) {
            Xsqlite3_bind_int64(tls, pReplace, (int32)1, iRowid);
            if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FbContentlessDelete != 0) {
                iOrigin = 0;
                rc = _sqlite3Fts5IndexGetOrigin(tls, (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpIndex, @unsafe.Pointer.FromPinnedBox(ᏑiOrigin));
                Xsqlite3_bind_int64(tls, pReplace, (int32)3, iOrigin);
            }
        }
        if (rc == SQLITE_OK) {
            Xsqlite3_bind_blob(tls, pReplace, (int32)2, ~@unsafe.Pointer.To<uintptr>(pBuf), ~@unsafe.Pointer.To<int32>(@unsafe.Add(pBuf, 8)), libc.UintptrFromInt32(0));
            Xsqlite3_step(tls, pReplace);
            rc = Xsqlite3_reset(tls, pReplace);
            Xsqlite3_bind_null(tls, pReplace, (int32)2);
        }
    }
    return rc;
}

internal static int32 /*r*/ _fts5StorageLoadTotals(ж<libc.TLS> tls, uintptr p, int32 bCache) {
    int32 rc = default;
    _ = rc;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FbTotalsValid == 0) {
        rc = _sqlite3Fts5IndexGetAverages(tls, (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpIndex, p + 24, (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FaTotalSize);
        (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FbTotalsValid = bCache;
    }
    return rc;
}

internal static void _fts5StructurePromote(ж<libc.TLS> tls, uintptr p, int32 iLvl, uintptr pStruct) {
    int32 i = default;
    int32 iPromote = default;
    int32 iTst = default;
    int32 nSeg = default;
    int32 sz = default;
    int32 szMax = default;
    int32 szPromote = default;
    int32 szSeg = default;
    uintptr pSeg = default;
    uintptr pTst = default;
    _ = i;
    _ = iPromote;
    _ = iTst;
    _ = nSeg;
    _ = pSeg;
    _ = pTst;
    _ = sz;
    _ = szMax;
    _ = szPromote;
    _ = szSeg;
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        iPromote = -(int32)1;
        szPromote = 0;
        nSeg = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pStruct + 32 + (uintptr)iLvl * 16))).Value.FnSeg;
        if (nSeg == 0) {
            return;
        }
        pSeg = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pStruct + 32 + (uintptr)iLvl * 16))).Value.FaSeg + (uintptr)((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pStruct + 32 + (uintptr)iLvl * 16))).Value.FnSeg - (int32)1) * 56;
        szSeg = (int32)1 + (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FpgnoLast - (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FpgnoFirst;
        iTst = iLvl - (int32)1;
        for (;;) {
            if (!(iTst >= 0 && (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pStruct + 32 + (uintptr)iTst * 16))).Value.FnSeg == 0)) {
                break;
            }
            goto _1;
_1:;
            iTst = iTst - 1;
        }
        if (iTst >= 0) {
            szMax = 0;
            pTst = pStruct + 32 + (uintptr)iTst * 16;
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pTst)).Value.FnSeg)) {
                    break;
                }
                sz = (@unsafe.Pointer.To<TFts5StructureSegment>(__ccgo_ptr((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pTst)).Value.FaSeg + (uintptr)i * 56))).Value.FpgnoLast - (@unsafe.Pointer.To<TFts5StructureSegment>(__ccgo_ptr((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pTst)).Value.FaSeg + (uintptr)i * 56))).Value.FpgnoFirst + (int32)1;
                if (sz > szMax) {
                    szMax = sz;
                }
                goto _2;
_2:;
                i = i + 1;
            }
            if (szMax >= szSeg) {
                iPromote = iTst;
                szPromote = szMax;
            }
        }
        if (iPromote < 0) {
            iPromote = iLvl;
            szPromote = szSeg;
        }
        _fts5StructurePromoteTo(tls, p, iPromote, szPromote, pStruct);
    }
}

internal static void _fts5StructureRelease(ж<libc.TLS> tls, uintptr pStruct) {
    int32 i = default;
    int32 v1 = default;
    uintptr v2 = default;
    bool v3 = default;
    _ = i;
    _ = v1;
    _ = v2;
    _ = v3;
    v3 = pStruct != 0; if (v3) {
        v2 = pStruct;
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) - 1;
        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
    }
    if (v3 && 0 >= v1) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnLevel)) {
                break;
            }
            Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pStruct + 32 + (uintptr)i * 16))).Value.FaSeg);
            goto _4;
_4:;
            i = i + 1;
        }
        Xsqlite3_free(tls, pStruct);
    }
}

internal static int32 /*r*/ _fts5SyncMethod(ж<libc.TLS> tls, uintptr pVtab) {
    uintptr pTab = default;
    int32 rc = default;
    _ = pTab;
    _ = rc;
    pTab = pVtab;
    (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpConfig)).Value.FpzErrmsg = pTab + 16;
    rc = _sqlite3Fts5FlushToDisk(tls, pTab);
    (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpConfig)).Value.FpzErrmsg = (uintptr)0;
    return rc;
}

internal static void _fts5TokendataIterDelete(ж<libc.TLS> tls, uintptr pSet) {
    int32 ii = default;
    _ = ii;
    if (pSet != 0) {
        ii = 0;
        for (;;) {
            if (!((int64)ii < (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pSet)).Value.FnIter)) {
                break;
            }
            _fts5MultiIterFree(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSet + 72 + (uintptr)ii * 8)));
            goto _1;
_1:;
            ii = ii + 1;
        }
        _sqlite3Fts5BufferFree(tls, pSet + 24);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pSet)).Value.FaPoslistReader);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pSet)).Value.FaMap);
        Xsqlite3_free(tls, pSet);
    }
}

internal static void _fts5TokendataSetTermIfEof(ж<libc.TLS> tls, uintptr pIter, uintptr pTerm) {
    if (pIter != 0 && (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)(pIter + 104))).Value.FpLeaf == (uintptr)0) {
        _sqlite3Fts5BufferSet(tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FpIndex + 60), pIter + 104 + 96, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pTerm)).Value.Fn, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pTerm)).Value.Fp);
    }
}

internal static void _fts5TombstoneArrayDelete(ж<libc.TLS> tls, uintptr p) {
    int32 ii = default;
    _ = ii;
    if (p != 0) {
        (@unsafe.Pointer.To<TFts5TombstoneArray>((@unsafe.Pointer)p)).Value.FnRef = (@unsafe.Pointer.To<TFts5TombstoneArray>((@unsafe.Pointer)p)).Value.FnRef - 1;
        if ((@unsafe.Pointer.To<TFts5TombstoneArray>((@unsafe.Pointer)p)).Value.FnRef <= 0) {
            ii = 0;
            for (;;) {
                if (!(ii < (@unsafe.Pointer.To<TFts5TombstoneArray>((@unsafe.Pointer)p)).Value.FnTombstone)) {
                    break;
                }
                _fts5DataRelease(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8 + (uintptr)ii * 8)));
                goto _1;
_1:;
                ii = ii + 1;
            }
            Xsqlite3_free(tls, p);
        }
    }
}

internal static void _fts5TripCursors(ref libc.TLS tls, uintptr pTab) {
    uintptr pCsr = default;
    _ = pCsr;
    pCsr = (@unsafe.Pointer.To<TFts5Global>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.FpGlobal)).Value.FpCsr;
    for (;;) {
        if (!(pCsr != 0)) {
            break;
        }
        if ((@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FePlan == (int32)FTS5_PLAN_MATCH && (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab == pTab) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pCsr + 80))).Value |= (int32)FTS5CSR_REQUIRE_RESEEK;
        }
        goto _1;
_1:;
        pCsr = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpNext;
    }
}

internal static int32 /*r*/ _fts5VocabCloseMethod(ж<libc.TLS> tls, uintptr pCursor) {
    uintptr pCsr = default;
    _ = pCsr;
    pCsr = pCursor;
    _fts5VocabResetCursor(tls, pCsr);
    _sqlite3Fts5BufferFree(tls, pCsr + 96);
    Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpStmt);
    Xsqlite3_free(tls, pCsr);
    return SQLITE_OK;
}

internal static int32 /*r*/ _fts5VocabColumnMethod(ж<libc.TLS> tls, uintptr pCursor, uintptr pCtx, int32 iCol) {
    int32 eDetail = default;
    int32 eType = default;
    int32 ii = default;
    int32 ii1 = default;
    Ti64 iVal = default;
    uintptr pCsr = default;
    uintptr z = default;
    uintptr z1 = default;
    _ = eDetail;
    _ = eType;
    _ = iVal;
    _ = ii;
    _ = ii1;
    _ = pCsr;
    _ = z;
    _ = z1;
    pCsr = pCursor;
    eDetail = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Table>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpFts5)).Value.FpConfig)).Value.FeDetail;
    eType = (@unsafe.Pointer.To<TFts5VocabTable>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_vtab_cursor>((@unsafe.Pointer)pCursor)).Value.FpVtab)).Value.FeType;
    iVal = 0;
    if (iCol == 0) {
        Xsqlite3_result_text(tls, pCtx, (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.Fterm.Fp, (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.Fterm.Fn, (uintptr)(-libc.Int32FromInt32(1)));
    } else {
        if (eType == FTS5_VOCAB_COL) {
            if (iCol == (int32)1) {
                if (eDetail != (int32)FTS5_DETAIL_NONE) {
                    z = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Table>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpFts5)).Value.FpConfig)).Value.FazCol + (uintptr)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol * 8));
                    Xsqlite3_result_text(tls, pCtx, z, -(int32)1, libc.UintptrFromInt32(0));
                }
            } else {
                if (iCol == (int32)2) {
                    iVal = ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaDoc + (uintptr)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol * 8));
                } else {
                    iVal = ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaCnt + (uintptr)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol * 8));
                }
            }
        } else {
            if (eType == (int32)FTS5_VOCAB_ROW) {
                if (iCol == (int32)1) {
                    iVal = ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaDoc));
                } else {
                    iVal = ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaCnt));
                }
            } else {
                switch (iCol) {
                case var ᴛ when ᴛ == (int32)1: {
                    Xsqlite3_result_int64(tls, pCtx, (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpIter)).Value.FiRowid);
                    break;
                }
                case var ᴛ when ᴛ == (int32)2: {
                    ii = -(int32)1;
                    if (eDetail == FTS5_DETAIL_FULL) {
                        ii = (int32)((Ti64)((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiInstPos >> (int)(libc.Int32FromInt32(32)) & libc.Int64FromInt32(0x7FFFFFFF)));
                    } else {
                        if (eDetail == (int32)FTS5_DETAIL_COLUMNS) {
                            ii = (int32)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiInstPos;
                        }
                    }
                    if (ii >= 0 && ii < (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Table>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpFts5)).Value.FpConfig)).Value.FnCol) {
                        z1 = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Table>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpFts5)).Value.FpConfig)).Value.FazCol + (uintptr)ii * 8));
                        Xsqlite3_result_text(tls, pCtx, z1, -(int32)1, libc.UintptrFromInt32(0));
                    }
                    break;
                }
                default: {
                    if (eDetail == FTS5_DETAIL_FULL) {
                        ii1 = (int32)((Ti64)((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiInstPos & libc.Int64FromInt32(0x7FFFFFFF)));
                        Xsqlite3_result_int(tls, pCtx, ii1);
                    }
                    break;
                    break;
                }}

            }
        }
    }
    if (iVal > 0) {
        Xsqlite3_result_int64(tls, pCtx, iVal);
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _fts5VocabInstanceNext(ж<libc.TLS> tls, uintptr pCsr) {
    int32 eDetail = default;
    int32 rc = default;
    uintptr pIter = default;
    uintptr po = default;
    uintptr pp = default;
    _ = eDetail;
    _ = pIter;
    _ = po;
    _ = pp;
    _ = rc;
    eDetail = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Table>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpFts5)).Value.FpConfig)).Value.FeDetail;
    rc = SQLITE_OK;
    pIter = (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpIter;
    pp = pCsr + 112;
    po = pCsr + 120;
    while (eDetail == (int32)FTS5_DETAIL_NONE || _sqlite3Fts5PoslistNext64(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)pIter)).Value.FpData, (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)pIter)).Value.FnData, (@unsafe.Pointer)po, (@unsafe.Pointer)pp) != 0) {
        (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiInstPos = 0;
        (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiInstOff = 0;
        rc = _sqlite3Fts5IterNextScan(tls, (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpIter);
        if (rc == SQLITE_OK) {
            rc = _fts5VocabInstanceNewTerm(tls, pCsr);
            if ((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FbEof != 0 || eDetail == (int32)FTS5_DETAIL_NONE) {
                break;
            }
        }
        if (rc != 0) {
            (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FbEof = (int32)1;
            break;
        }
    }
    return rc;
}

internal static int32 /*r*/ _fts5VtoVCreate(ж<libc.TLS> tls, uintptr pCtx, uintptr azArg, int32 nArg, uintptr ppOut) {
    uintptr pMod = default;
    uintptr pNew = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    _ = pMod;
    _ = pNew;
    _ = rc;
    pMod = pCtx;
    pNew = (uintptr)0;
    rc = SQLITE_OK;
    pNew = _sqlite3Fts5MallocZero(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (int64)72);
    if (rc == SQLITE_OK) {
        (@unsafe.Pointer.To<TFts5VtoVTokenizer>((@unsafe.Pointer)pNew)).Value.Fx1 = (@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pMod)).Value.Fx1;
        (@unsafe.Pointer.To<TFts5VtoVTokenizer>((@unsafe.Pointer)pNew)).Value.Fx2 = (@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pMod)).Value.Fx2;
        (@unsafe.Pointer.To<TFts5VtoVTokenizer>((@unsafe.Pointer)pNew)).Value.FbV2Native = (@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pMod)).Value.FbV2Native;
        if ((@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pMod)).Value.FbV2Native != 0) {
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, uintptr, int32>>((@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pMod)).Value.Fx2.FxCreate)(tls, (@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pMod)).Value.FpUserData, azArg, nArg, pNew + 64);
        } else {
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, uintptr, int32>>((@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pMod)).Value.Fx1.FxCreate)(tls, (@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pMod)).Value.FpUserData, azArg, nArg, pNew + 64);
        }
        if (rc != SQLITE_OK) {
            Xsqlite3_free(tls, pNew);
            pNew = (uintptr)0;
        }
    }
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppOut))).Value = pNew;
    return rc;
}

internal static void _fts5WriteBtreeNoTerm(ж<libc.TLS> tls, uintptr p, uintptr pWriter) {
    uintptr pDlidx = default;
    _ = pDlidx;
    if ((@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FbFirstRowidInPage != 0 && (@unsafe.Pointer.To<TFts5DlidxWriter>(__ccgo_ptr((@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FaDlidx))).Value.Fbuf.Fn > 0) {
        pDlidx = (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FaDlidx;
        _sqlite3Fts5BufferAppendVarint(tls, (@unsafe.Pointer)(p + 60), (@unsafe.Pointer)(pDlidx + 16), 0);
    }
    (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FnEmpty = (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FnEmpty + 1;
}

internal static void _fts5WriteBtreeTerm(ж<libc.TLS> tls, uintptr p, uintptr pWriter, int32 nTerm, uintptr pTerm) {
    _fts5WriteFlushBtree(tls, p, pWriter);
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        _sqlite3Fts5BufferSet(tls, (@unsafe.Pointer)(p + 60), pWriter + 96, nTerm, pTerm);
        (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FiBtPage = (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.Fwriter.Fpgno;
    }
}

internal static void _fts5WriteFinish(ж<libc.TLS> tls, uintptr p, uintptr pWriter, @unsafe.Pointer pnLeaf) {
    int32 i = default;
    uintptr pLeaf = default;
    _ = i;
    _ = pLeaf;
    pLeaf = pWriter + 8;
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        if ((@unsafe.Pointer.To<TFts5PageWriter>((@unsafe.Pointer)pLeaf)).Value.Fbuf.Fn > (int32)4) {
            _fts5WriteFlushLeaf(tls, p, pWriter);
        }
        (@unsafe.Pointer.To<int32>(pnLeaf)).Value = (@unsafe.Pointer.To<TFts5PageWriter>((@unsafe.Pointer)pLeaf)).Value.Fpgno - (int32)1;
        if ((@unsafe.Pointer.To<TFts5PageWriter>((@unsafe.Pointer)pLeaf)).Value.Fpgno > (int32)1) {
            _fts5WriteFlushBtree(tls, p, pWriter);
        }
    }
    _sqlite3Fts5BufferFree(tls, pLeaf + 40);
    _sqlite3Fts5BufferFree(tls, pLeaf + 8);
    _sqlite3Fts5BufferFree(tls, pLeaf + 24);
    _sqlite3Fts5BufferFree(tls, pWriter + 96);
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FnDlidx)) {
            break;
        }
        _sqlite3Fts5BufferFree(tls, (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FaDlidx + (uintptr)i * 32 + 16);
        goto _1;
_1:;
        i = i + 1;
    }
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FaDlidx);
}

internal static void _fts5yy_pop_parser_stack(ж<libc.TLS> tls, uintptr pParser) {
    uintptr fts5yytos = default;
    uintptr v1 = default;
    uintptr v2 = default;
    _ = fts5yytos;
    _ = v1;
    _ = v2;
    v2 = pParser;
    v1 = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)v2);
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)v2)).Value -= 24;
    fts5yytos = v1;
    _fts5yy_destructor(tls, pParser, (@unsafe.Pointer.To<Tfts5yyStackEntry>((@unsafe.Pointer)fts5yytos)).Value.Fmajor, (@unsafe.Pointer)(fts5yytos + 8));
}

internal static void _functionDestroy(ж<libc.TLS> tls, uintptr db, uintptr p) {
    uintptr pDestructor = default;
    _ = pDestructor;
    pDestructor = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 64));
    if (pDestructor != 0) {
        (@unsafe.Pointer.To<TFuncDestructor>((@unsafe.Pointer)pDestructor)).Value.FnRef = (@unsafe.Pointer.To<TFuncDestructor>((@unsafe.Pointer)pDestructor)).Value.FnRef - 1;
        if ((@unsafe.Pointer.To<TFuncDestructor>((@unsafe.Pointer)pDestructor)).Value.FnRef == 0) {
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>((@unsafe.Pointer.To<TFuncDestructor>((@unsafe.Pointer)pDestructor)).Value.FxDestroy)(tls, (@unsafe.Pointer.To<TFuncDestructor>((@unsafe.Pointer)pDestructor)).Value.FpUserData);
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, pDestructor);
        }
    }
}

internal static void _gatherSelectWindows(ж<libc.TLS> tls, uintptr p) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_gatherSelectWindowsCallback);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_gatherSelectWindowsSelectCallback);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback2 = (uintptr)0;
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FpParse = (uintptr)0;
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0))).Value = p;
        _sqlite3WalkSelect(tls, bp, p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _gatherSelectWindowsSelectCallback(ж<libc.TLS> tls, uintptr pWalker, uintptr p) {
    int32 v1 = default;
    _ = v1;
    if (p == ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWalker + 40))) {
        v1 = WRC_Continue;
    } else {
        v1 = (int32)WRC_Prune;
    }
    return v1;
}

internal static void _generateColumnTypes(ж<libc.TLS> tls, uintptr pParse, uintptr pTabList, uintptr pEList) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        int32 i = default;
        uintptr p = default;
        uintptr v = default;
        ref var zOrigCol = ref heap(new uintptr(), out var ᏑzOrigCol);
        ref var zOrigDb = ref heap(new uintptr(), out var ᏑzOrigDb);
        ref var zOrigTab = ref heap(new uintptr(), out var ᏑzOrigTab);
        uintptr zType = default;
        TNameContext _ᴛ1 = new();
        _ = i;
        _ = p;
        _ = v;
        _ = zOrigCol;
        _ = zOrigDb;
        _ = zOrigTab;
        _ = zType;
        v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpSrcList = pTabList;
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpNext = (uintptr)0;
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr)) {
                break;
            }
            p = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)i * 32))).Value.FpExpr;
            zOrigDb = (uintptr)0;
            zOrigTab = (uintptr)0;
            zOrigCol = (uintptr)0;
            zType = _columnTypeImpl(tls, bp, p, @unsafe.Pointer.FromBox(ᏑzOrigDb), @unsafe.Pointer.FromBox(ᏑzOrigTab), @unsafe.Pointer.FromBox(ᏑzOrigCol));
            _sqlite3VdbeSetColName(tls, v, i, (int32)COLNAME_DATABASE, zOrigDb, (uintptr)(-libc.Int32FromInt32(1)));
            _sqlite3VdbeSetColName(tls, v, i, (int32)COLNAME_TABLE, zOrigTab, (uintptr)(-libc.Int32FromInt32(1)));
            _sqlite3VdbeSetColName(tls, v, i, (int32)COLNAME_COLUMN, zOrigCol, (uintptr)(-libc.Int32FromInt32(1)));
            _sqlite3VdbeSetColName(tls, v, i, (int32)COLNAME_DECLTYPE, zType, (uintptr)(-libc.Int32FromInt32(1)));
            goto _1;
_1:;
            i = i + 1;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _geopolyContainsPointFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    int32 cnt = default;
    int32 ii = default;
    int32 v = default;
    uintptr p1 = default;
    float64 x0 = default;
    float64 y0 = default;
    _ = cnt;
    _ = ii;
    _ = p1;
    _ = v;
    _ = x0;
    _ = y0;
    p1 = _geopolyFuncParam(tls, context, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)), nil);
    x0 = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
    y0 = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 2 * 8)));
    v = 0;
    cnt = 0;
    _ = argc;
    if (p1 == (uintptr)0) {
        return;
    }
    ii = 0;
    for (;;) {
        if (!(ii < (@unsafe.Pointer.To<TGeoPoly>((@unsafe.Pointer)p1)).Value.FnVertex - (int32)1)) {
            break;
        }
        v = _pointBeneathLine(ref (tls).DerefOrNull(), x0, y0, (float64)(~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p1 + 8 + (uintptr)(ii * (int32)2) * 4))), (float64)(~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p1 + 8 + (uintptr)(ii * (int32)2 + (int32)1) * 4))), (float64)(~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p1 + 8 + (uintptr)((ii + (int32)1) * (int32)2) * 4))), (float64)(~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p1 + 8 + (uintptr)((ii + (int32)1) * (int32)2 + (int32)1) * 4))));
        if (v == (int32)2) {
            break;
        }
        cnt = cnt + v;
        goto _1;
_1:;
        ii = ii + 1;
    }
    if (v != (int32)2) {
        v = _pointBeneathLine(ref (tls).DerefOrNull(), x0, y0, (float64)(~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p1 + 8 + (uintptr)(ii * (int32)2) * 4))), (float64)(~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p1 + 8 + (uintptr)(ii * (int32)2 + (int32)1) * 4))), (float64)(~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p1 + 8 + (uintptr)(libc.Int32FromInt32(0) * libc.Int32FromInt32(2)) * 4))), (float64)(~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p1 + 8 + (uintptr)(libc.Int32FromInt32(0) * libc.Int32FromInt32(2) + libc.Int32FromInt32(1)) * 4))));
    }
    if (v == (int32)2) {
        Xsqlite3_result_int(tls, context, (int32)1);
    } else {
        if (((v + cnt) & (int32)1) == 0) {
            Xsqlite3_result_int(tls, context, 0);
        } else {
            Xsqlite3_result_int(tls, context, (int32)2);
        }
    }
    Xsqlite3_free(tls, p1);
}

internal static uintptr /*r*/ _geopolyEventMerge(ref libc.TLS tls, uintptr pLeft, uintptr pRight) {
    ref var head = ref heap(new TGeoEvent(), out var Ꮡhead);
    @unsafe.Pointer pLast = default;
    uintptr v1 = default;
    _ = head;
    _ = pLast;
    _ = v1;
    head.FpNext = (uintptr)0;
    pLast = @unsafe.Pointer.FromPinnedBox(Ꮡhead);
    while (pRight != 0 && pLeft != 0) {
        if ((@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pRight)).Value.Fx <= (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pLeft)).Value.Fx) {
            (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pLast, 24))).Value = pRight;
            pLast = (@unsafe.Pointer)pRight;
            pRight = (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pRight)).Value.FpNext;
        } else {
            (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pLast, 24))).Value = pLeft;
            pLast = (@unsafe.Pointer)pLeft;
            pLeft = (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pLeft)).Value.FpNext;
        }
    }
    if (pRight != 0) {
        v1 = pRight;
    } else {
        v1 = pLeft;
    }
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pLast, 24))).Value = v1;
    return head.FpNext;
}

internal static void _geopolyOverlapFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    uintptr p1 = default;
    uintptr p2 = default;
    int32 x = default;
    _ = p1;
    _ = p2;
    _ = x;
    p1 = _geopolyFuncParam(tls, context, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)), nil);
    p2 = _geopolyFuncParam(tls, context, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)), nil);
    _ = argc;
    if (p1 != 0 && p2 != 0) {
        x = _geopolyOverlap(tls, p1, p2);
        if (x < 0) {
            Xsqlite3_result_error_nomem(tls, context);
        } else {
            Xsqlite3_result_int(tls, context, x);
        }
    }
    Xsqlite3_free(tls, p1);
    Xsqlite3_free(tls, p2);
}

internal static uintptr /*r1*/ _geopolySegmentMerge(ref libc.TLS tls, uintptr pLeft, uintptr pRight) {
    ref var head = ref heap(new TGeoSegment(), out var Ꮡhead);
    @unsafe.Pointer pLast = default;
    float64 r = default;
    uintptr v1 = default;
    _ = head;
    _ = pLast;
    _ = r;
    _ = v1;
    head.FpNext = (uintptr)0;
    pLast = @unsafe.Pointer.FromPinnedBox(Ꮡhead);
    while (pRight != 0 && pLeft != 0) {
        r = (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pRight)).Value.Fy - (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pLeft)).Value.Fy;
        if (r == (float64)0D) {
            r = (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pRight)).Value.FC - (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pLeft)).Value.FC;
        }
        if (r < (float64)0D) {
            (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pLast, 40))).Value = pRight;
            pLast = (@unsafe.Pointer)pRight;
            pRight = (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pRight)).Value.FpNext;
        } else {
            (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pLast, 40))).Value = pLeft;
            pLast = (@unsafe.Pointer)pLeft;
            pLeft = (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pLeft)).Value.FpNext;
        }
    }
    if (pRight != 0) {
        v1 = pRight;
    } else {
        v1 = pLeft;
    }
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pLast, 40))).Value = v1;
    return head.FpNext;
}

internal static uintptr /*r*/ _geopolySortEventsByX(ref libc.TLS tls, uintptr aEvent, int32 nEvent) {
    array<uintptr> a = new(50);
    int32 i = default;
    int32 j = default;
    int32 mx = default;
    uintptr p = default;
    _ = a;
    _ = i;
    _ = j;
    _ = mx;
    _ = p;
    mx = 0;
    i = 0;
    for (;;) {
        if (!(i < nEvent)) {
            break;
        }
        p = aEvent + (uintptr)i * 32;
        (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)p)).Value.FpNext = (uintptr)0;
        j = 0;
        for (;;) {
            if (!(j < mx && a[j] != 0)) {
                break;
            }
            p = _geopolyEventMerge(ref tls, a[j], p);
            a[j] = (uintptr)0;
            goto _2;
_2:;
            j = j + 1;
        }
        a[j] = p;
        if (j >= mx) {
            mx = j + (int32)1;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    p = (uintptr)0;
    i = 0;
    for (;;) {
        if (!(i < mx)) {
            break;
        }
        p = _geopolyEventMerge(ref tls, a[i], p);
        goto _3;
_3:;
        i = i + 1;
    }
    return p;
}

internal static void _geopolyWithinFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    uintptr p1 = default;
    uintptr p2 = default;
    int32 x = default;
    int32 v1 = default;
    int32 v2 = default;
    _ = p1;
    _ = p2;
    _ = x;
    _ = v1;
    _ = v2;
    p1 = _geopolyFuncParam(tls, context, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)), nil);
    p2 = _geopolyFuncParam(tls, context, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)), nil);
    _ = argc;
    if (p1 != 0 && p2 != 0) {
        x = _geopolyOverlap(tls, p1, p2);
        if (x < 0) {
            Xsqlite3_result_error_nomem(tls, context);
        } else {
            if (x == (int32)2) {
                v1 = (int32)1;
            } else {
                if (x == (int32)4) {
                    v2 = (int32)2;
                } else {
                    v2 = 0;
                }
                v1 = v2;
            }
            Xsqlite3_result_int(tls, context, v1);
        }
    }
    Xsqlite3_free(tls, p1);
    Xsqlite3_free(tls, p2);
}

internal static float64 /*r*/ _getDoubleArg(ж<libc.TLS> tls, uintptr p) {
    int32 v1 = default;
    uintptr v2 = default;
    _ = v1;
    _ = v2;
    if ((@unsafe.Pointer.To<TPrintfArguments>((@unsafe.Pointer)p)).Value.FnArg <= (@unsafe.Pointer.To<TPrintfArguments>((@unsafe.Pointer)p)).Value.FnUsed) {
        return (float64)0D;
    }
    v2 = p + 4;
    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
    return Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TPrintfArguments>((@unsafe.Pointer)p)).Value.FapArg + (uintptr)v1 * 8)));
}

internal static Tsqlite3_int64 /*r*/ _getIntArg(ж<libc.TLS> tls, uintptr p) {
    int32 v1 = default;
    uintptr v2 = default;
    _ = v1;
    _ = v2;
    if ((@unsafe.Pointer.To<TPrintfArguments>((@unsafe.Pointer)p)).Value.FnArg <= (@unsafe.Pointer.To<TPrintfArguments>((@unsafe.Pointer)p)).Value.FnUsed) {
        return 0;
    }
    v2 = p + 4;
    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
    return Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TPrintfArguments>((@unsafe.Pointer)p)).Value.FapArg + (uintptr)v1 * 8)));
}

internal static uintptr /*r*/ _getTextArg(ж<libc.TLS> tls, uintptr p) {
    int32 v1 = default;
    uintptr v2 = default;
    _ = v1;
    _ = v2;
    if ((@unsafe.Pointer.To<TPrintfArguments>((@unsafe.Pointer)p)).Value.FnArg <= (@unsafe.Pointer.To<TPrintfArguments>((@unsafe.Pointer)p)).Value.FnUsed) {
        return (uintptr)0;
    }
    v2 = p + 4;
    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
    return Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TPrintfArguments>((@unsafe.Pointer)p)).Value.FapArg + (uintptr)v1 * 8)));
}

internal static void _heightOfExprList(ref libc.TLS tls, uintptr p, @unsafe.Pointer pnHeight) {
    int32 i = default;
    _ = i;
    if (p != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)p)).Value.FnExpr)) {
                break;
            }
            _heightOfExpr(ref tls, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(p + 8 + (uintptr)i * 32))).Value.FpExpr, pnHeight);
            goto _1;
_1:;
            i = i + 1;
        }
    }
}

internal static int32 /*r*/ _indexColumnIsBeingUpdated(ж<libc.TLS> tls, uintptr pIdx, int32 iCol, uintptr aXRef, int32 chngRowid) {
    Ti16 iIdxCol = default;
    _ = iIdxCol;
    iIdxCol = ~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)iCol * 2));
    if ((int32)iIdxCol >= 0) {
        return libc.BoolInt32(~@unsafe.Pointer.To<int32>(__ccgo_ptr(aXRef + (uintptr)iIdxCol * 4)) >= 0);
    }
    return _sqlite3ExprReferencesUpdatedColumn(tls, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaColExpr + 8 + (uintptr)iCol * 32))).Value.FpExpr, aXRef, chngRowid);
}

internal static uintptr /*r*/ _indexIteratorFirst(ref libc.TLS tls, @unsafe.Pointer pIter, @unsafe.Pointer pIx) {
    uintptr r = default;

    if (~@unsafe.Pointer.To<int32>(pIter) != 0) {
        (@unsafe.Pointer.To<int32>(pIx)).Value = (@unsafe.Pointer.To<TIndexListTerm>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pIter, 16))))).Value.Fix;
        return (@unsafe.Pointer.To<TIndexListTerm>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pIter, 16))))).Value.Fp;
    } else {
        (@unsafe.Pointer.To<int32>(pIx)).Value = 0;
        return ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pIter, 8));
    }
    return r;
}

internal static uintptr /*r*/ _indexIteratorNext(ref libc.TLS tls, @unsafe.Pointer pIter, @unsafe.Pointer pIx) {
    uintptr r = default;

    int32 i = default;
    int32 v1 = default;
    @unsafe.Pointer v2 = default;
    _ = i;
    _ = v1;
    _ = v2;
    if (~@unsafe.Pointer.To<int32>(pIter) != 0) {
        v2 = @unsafe.Add(pIter, 4);
        (@unsafe.Pointer.To<int32>(v2)).Value = ~@unsafe.Pointer.To<int32>(v2) + 1;
        v1 = ~@unsafe.Pointer.To<int32>(v2);
        i = v1;
        if (i >= ~@unsafe.Pointer.To<int32>(@unsafe.Add(pIter, 8))) {
            (@unsafe.Pointer.To<int32>(pIx)).Value = i;
            return (uintptr)0;
        }
        (@unsafe.Pointer.To<int32>(pIx)).Value = (@unsafe.Pointer.To<TIndexListTerm>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pIter, 16)) + (uintptr)i * 16))).Value.Fix;
        return (@unsafe.Pointer.To<TIndexListTerm>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pIter, 16)) + (uintptr)i * 16))).Value.Fp;
    } else {
        (@unsafe.Pointer.To<int32>(pIx)).Value = ~@unsafe.Pointer.To<int32>(pIx) + 1;
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pIter, 8))).Value = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pIter, 8))))).Value.FpNext;
        return ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pIter, 8));
    }
    return r;
}

internal static void _initMemArray(ref libc.TLS tls, uintptr p, int32 N, uintptr db, Tu16 flags) {
    int32 v1 = default;
    _ = v1;
    if (N > 0) {
        for (;;) {
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fflags = flags;
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fdb = db;
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.FszMalloc = 0;
            p += 56;
            goto _2;
_2:;
            N = N - 1;
            v1 = N;
            if (!(v1 > 0)) {
                break;
            }
        }
    }
}

internal static int32 /*r*/ _isDupColumn(ref libc.TLS tls, uintptr pIdx, int32 nKey, uintptr pPk, int32 iCol) {
    int32 i = default;
    int32 j = default;
    _ = i;
    _ = j;
    j = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn + (uintptr)iCol * 2)));
    i = 0;
    for (;;) {
        if (!(i < nKey)) {
            break;
        }
        if ((int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)i * 2))) == j && _sqlite3StrICmp(ref tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FazColl + (uintptr)i * 8)), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FazColl + (uintptr)iCol * 8))) == 0) {
            return (int32)1;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return 0;
}

internal static int32 /*r*/ _isLookaside(ref libc.TLS tls, uintptr db, uintptr p) {
    return libc.BoolInt32((uint64)p >= (uint64)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpStart && (uint64)p < (uint64)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpTrueEnd);
}

internal static void _jsonBlobAppendOneByte(ж<libc.TLS> tls, uintptr pParse, Tu8 c) {
    Tu32 v1 = default;
    uintptr v2 = default;
    _ = v1;
    _ = v2;
    if ((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob >= (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlobAlloc) {
        _jsonBlobExpandAndAppendOneByte(tls, pParse, c);
    } else {
        v2 = pParse + 8;
        v1 = ~@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v2);
        (@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v2) + 1;
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)v1))).Value = c;
    }
}

internal static void _jsonCacheDelete(ж<libc.TLS> tls, uintptr p) {
    int32 i = default;
    _ = i;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.FnUsed)) {
            break;
        }
        _jsonParseFree(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 16 + (uintptr)i * 8)));
        goto _1;
_1:;
        i = i + 1;
    }
    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.Fdb, p);
}

internal static void _jsonEachCursorReset(ж<libc.TLS> tls, uintptr p) {
    _jsonParseReset(tls, (@unsafe.Pointer)(p + 192));
    _jsonStringReset(tls, p + 56);
    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fdb, (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FaParent);
    (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FiRowid = (uint32)0;
    (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fi = (uint32)0;
    (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FaParent = (uintptr)0;
    (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FnParent = (uint32)0;
    (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FnParentAlloc = (uint32)0;
    (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FiEnd = (uint32)0;
    (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FeType = (uint8)0;
}

internal static int32 /*r*/ _jsonEachOpen(ж<libc.TLS> tls, uintptr p, uintptr ppCursor) {
    uintptr pCur = default;
    uintptr pVtab = default;
    _ = pCur;
    _ = pVtab;
    pVtab = p;
    _ = p;
    pCur = _sqlite3DbMallocZero(tls, (@unsafe.Pointer.To<TJsonEachConnection>((@unsafe.Pointer)pVtab)).Value.Fdb, (uint64)264);
    if (pCur == (uintptr)0) {
        return (int32)SQLITE_NOMEM;
    }
    (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)pCur)).Value.Fdb = (@unsafe.Pointer.To<TJsonEachConnection>((@unsafe.Pointer)pVtab)).Value.Fdb;
    (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)pCur)).Value.FeMode = (@unsafe.Pointer.To<TJsonEachConnection>((@unsafe.Pointer)pVtab)).Value.FeMode;
    (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)pCur)).Value.FbRecursive = (@unsafe.Pointer.To<TJsonEachConnection>((@unsafe.Pointer)pVtab)).Value.FbRecursive;
    _jsonStringZero(ref (tls).DerefOrNull(), pCur + 56);
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppCursor))).Value = pCur;
    return SQLITE_OK;
}

internal static int32 /*r*/ _jsonLabelCompare(ж<libc.TLS> tls, uintptr zLeft, Tu32 nLeft, int32 rawLeft, uintptr zRight, Tu32 nRight, int32 rawRight) {
    int32 r = default;

    if (rawLeft != 0 && rawRight != 0) {
        if (nLeft != nRight) {
            return 0;
        }
        return libc.BoolInt32(libc.Xmemcmp(tls, zLeft, zRight, (uint64)nLeft) == 0);
    } else {
        return _jsonLabelCompareEscaped(ref (tls).DerefOrNull(), zLeft, nLeft, rawLeft, zRight, nRight, rawRight);
    }
    return r;
}

internal static void _jsonParseReset(ж<libc.TLS> tls, @unsafe.Pointer pParse) {
    if (~@unsafe.Pointer.To<Tu8>(@unsafe.Add(pParse, 48)) != 0) {
        _sqlite3RCStrUnref(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 16)));
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 16))).Value = (uintptr)0;
        (@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 32))).Value = 0;
        (@unsafe.Pointer.To<Tu8>(@unsafe.Add(pParse, 48))).Value = (uint8)0;
    }
    if (~@unsafe.Pointer.To<Tu32>(@unsafe.Add(pParse, 12)) != 0) {
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 24))), ~@unsafe.Pointer.To<uintptr>(pParse));
        (@unsafe.Pointer.To<uintptr>(pParse)).Value = (uintptr)0;
        (@unsafe.Pointer.To<Tu32>(@unsafe.Add(pParse, 8))).Value = (uint32)0;
        (@unsafe.Pointer.To<Tu32>(@unsafe.Add(pParse, 12))).Value = (uint32)0;
    }
}

internal static void _jsonPrettyIndent(ж<libc.TLS> tls, @unsafe.Pointer pPretty) {
    Tu32 jj = default;
    _ = jj;
    jj = (uint32)0;
    for (;;) {
        if (!(jj < ~@unsafe.Pointer.To<Tu32>(@unsafe.Add(pPretty, 28)))) {
            break;
        }
        _jsonAppendRaw(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pPretty, 8)), ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pPretty, 16)), ~@unsafe.Pointer.To<Tu32>(@unsafe.Add(pPretty, 24)));
        goto _1;
_1:;
        jj = jj + 1;
    }
}

internal static void _jsonQuoteFunc(ж<libc.TLS> tls, uintptr ctx, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(144);
        defer(tls.Free, (nint)144, ref ᒐ);
        TJsonString _ᴛ1 = new();
        _ = argc;
        _jsonStringInit(ref (tls).DerefOrNull(), bp, ctx);
        _jsonAppendSqlValue(tls, bp, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        _jsonReturnString(tls, bp, (uintptr)0, (uintptr)0);
        Xsqlite3_result_subtype(tls, ctx, (uint32)JSON_SUBTYPE);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _jsonStringOom(ж<libc.TLS> tls, uintptr p) {
    uintptr v1 = default;
    _ = v1;
    v1 = p + 33;
    (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(JSTRING_OOM));
    if ((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FpCtx != 0) {
        Xsqlite3_result_error_nomem(tls, (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FpCtx);
    }
    _jsonStringReset(tls, p);
}

internal static void _jsonStringZero(ref libc.TLS tls, uintptr p) {
    (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf = p + 34;
    (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnAlloc = (uint64)100;
    (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed = (uint64)0;
    (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FbStatic = (uint8)1;
}

internal static void _last_valueFinalizeFunc(ж<libc.TLS> tls, uintptr pCtx) {
    uintptr p = default;
    _ = p;
    p = Xsqlite3_aggregate_context(tls, pCtx, (int32)16);
    if (p != 0 && (@unsafe.Pointer.To<TLastValueCtx>((@unsafe.Pointer)p)).Value.FpVal != 0) {
        Xsqlite3_result_value(tls, pCtx, (@unsafe.Pointer.To<TLastValueCtx>((@unsafe.Pointer)p)).Value.FpVal);
        Xsqlite3_value_free(tls, (@unsafe.Pointer.To<TLastValueCtx>((@unsafe.Pointer)p)).Value.FpVal);
        (@unsafe.Pointer.To<TLastValueCtx>((@unsafe.Pointer)p)).Value.FpVal = (uintptr)0;
    }
}

internal static void _last_valueInvFunc(ж<libc.TLS> tls, uintptr pCtx, int32 nArg, uintptr apArg) {
    uintptr p = default;
    _ = p;
    _ = nArg;
    _ = apArg;
    p = Xsqlite3_aggregate_context(tls, pCtx, (int32)16);
    if (p != 0) {
        (@unsafe.Pointer.To<TLastValueCtx>((@unsafe.Pointer)p)).Value.FnVal = (@unsafe.Pointer.To<TLastValueCtx>((@unsafe.Pointer)p)).Value.FnVal - 1;
        if ((@unsafe.Pointer.To<TLastValueCtx>((@unsafe.Pointer)p)).Value.FnVal == 0) {
            Xsqlite3_value_free(tls, (@unsafe.Pointer.To<TLastValueCtx>((@unsafe.Pointer)p)).Value.FpVal);
            (@unsafe.Pointer.To<TLastValueCtx>((@unsafe.Pointer)p)).Value.FpVal = (uintptr)0;
        }
    }
}

internal static void _last_valueStepFunc(ж<libc.TLS> tls, uintptr pCtx, int32 nArg, uintptr apArg) {
    uintptr p = default;
    _ = p;
    _ = nArg;
    p = Xsqlite3_aggregate_context(tls, pCtx, (int32)16);
    if (p != 0) {
        Xsqlite3_value_free(tls, (@unsafe.Pointer.To<TLastValueCtx>((@unsafe.Pointer)p)).Value.FpVal);
        (@unsafe.Pointer.To<TLastValueCtx>((@unsafe.Pointer)p)).Value.FpVal = Xsqlite3_value_dup(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apArg)));
        if ((@unsafe.Pointer.To<TLastValueCtx>((@unsafe.Pointer)p)).Value.FpVal == (uintptr)0) {
            Xsqlite3_result_error_nomem(tls, pCtx);
        } else {
            (@unsafe.Pointer.To<TLastValueCtx>((@unsafe.Pointer)p)).Value.FnVal = (@unsafe.Pointer.To<TLastValueCtx>((@unsafe.Pointer)p)).Value.FnVal + 1;
        }
    }
}

internal static void _logFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    float64 ans = default;
    float64 b = default;
    float64 x = default;
    _ = ans;
    _ = b;
    _ = x;
    var exprᴛ1 = Xsqlite3_value_numeric_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    var matchᴛ1 = false;
    if (exprᴛ1 == (int32)SQLITE_INTEGER) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)SQLITE_FLOAT) {
        x = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        if (x <= (float64)0D) {
            return;
        }
    }
    else if (!matchᴛ1) { /* default: */
        return;
    }

    if (argc == (int32)2) {
        var exprᴛ2 = Xsqlite3_value_numeric_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        var matchᴛ2 = false;
        if (exprᴛ2 == (int32)SQLITE_INTEGER) { matchᴛ2 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ2 && exprᴛ2 == (int32)SQLITE_FLOAT) {
            b = libc.Xlog(tls, x);
            if (b <= (float64)0D) {
                return;
            }
            x = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
            if (x <= (float64)0D) {
                return;
            }
        }
        else if (!matchᴛ2) { /* default: */
            return;
        }

        ans = libc.Xlog(tls, x) / b;
    } else {
        switch ((int32)(int64)Xsqlite3_user_data(tls, context)) {
        case var ᴛ when ᴛ == (int32)1: {
            ans = libc.Xlog10(tls, x);
            break;
        }
        case var ᴛ when ᴛ == (int32)2: {
            ans = libc.X__builtin_log2(tls, x);
            break;
        }
        default: {
            ans = libc.Xlog(tls, x);
            break;
            break;
        }}

    }
    Xsqlite3_result_double(tls, context, ans);
}

internal static int32 /*r*/ _makeSorterRecord(ж<libc.TLS> tls, uintptr pParse, @unsafe.Pointer pSort, uintptr pSelect, int32 regBase, int32 nBase) {
    int32 nOBSat = default;
    int32 regOut = default;
    int32 v1 = default;
    uintptr v = default;
    uintptr v2 = default;
    _ = nOBSat;
    _ = regOut;
    _ = v;
    _ = v1;
    _ = v2;
    nOBSat = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 8));
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    v2 = pParse + 60;
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
    regOut = v1;
    if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pSort, 40)) != 0) {
        _innerLoopLoadRow(tls, pParse, pSelect, (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pSort, 40))));
    }
    _sqlite3VdbeAddOp3(tls, v, (int32)OP_MakeRecord, regBase + nOBSat, nBase - nOBSat, regOut);
    return regOut;
}

internal static void _markTermAsChild(ref libc.TLS tls, uintptr pWC, int32 iChild, int32 iParent) {
    (@unsafe.Pointer.To<TWhereTerm>(__ccgo_ptr((@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)iChild * 56))).Value.FiParent = iParent;
    (@unsafe.Pointer.To<TWhereTerm>(__ccgo_ptr((@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)iChild * 56))).Value.FtruthProb = (@unsafe.Pointer.To<TWhereTerm>(__ccgo_ptr((@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)iParent * 56))).Value.FtruthProb;
    (@unsafe.Pointer.To<TWhereTerm>(__ccgo_ptr((@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)iParent * 56))).Value.FnChild = (Tu8)((@unsafe.Pointer.To<TWhereTerm>(__ccgo_ptr((@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)iParent * 56))).Value.FnChild + 1);
}

internal static void _math2Func(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    float64 ans = default;
    float64 v0 = default;
    float64 v1 = default;
    int32 type0 = default;
    int32 type1 = default;
    ref var x = ref heap(new uintptr(), out var Ꮡx);
    _ = ans;
    _ = type0;
    _ = type1;
    _ = v0;
    _ = v1;
    _ = x;
    type0 = Xsqlite3_value_numeric_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    if (type0 != (int32)SQLITE_INTEGER && type0 != (int32)SQLITE_FLOAT) {
        return;
    }
    type1 = Xsqlite3_value_numeric_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
    if (type1 != (int32)SQLITE_INTEGER && type1 != (int32)SQLITE_FLOAT) {
        return;
    }
    v0 = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    v1 = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
    x = Xsqlite3_user_data(tls, context);
    ans = GoFuncCookie.Func<Func<ж<libc.TLS>, float64, float64, float64>>(x)(tls, v0, v1);
    Xsqlite3_result_double(tls, context, ans);
}

internal static void _measureAllocationSize(ж<libc.TLS> tls, @unsafe.Pointer db, uintptr p) {
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 808))))).Value += _sqlite3DbMallocSize(tls, db, p);
}

internal static int32 /*r*/ _memdbClose(ж<libc.TLS> tls, uintptr pFile) {
    int32 i = default;
    int32 v2 = default;
    uintptr p = default;
    uintptr pVfsMutex = default;
    uintptr v3 = default;
    _ = i;
    _ = p;
    _ = pVfsMutex;
    _ = v2;
    _ = v3;
    p = (@unsafe.Pointer.To<TMemFile>((@unsafe.Pointer)pFile)).Value.FpStore;
    if ((@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FzFName != 0) {
        pVfsMutex = _sqlite3MutexAlloc(tls, (int32)SQLITE_MUTEX_STATIC_VFS1);
        Xsqlite3_mutex_enter(tls, pVfsMutex);
        i = 0;
        for (;;) {
            if (!(i < _memdb_g.FnMemStore)) {
                break;
            }
            if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(_memdb_g.FapMemStore + (uintptr)i * 8)) == p) {
                _memdbEnter(tls, p);
                if ((@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FnRef == (int32)1) {
                    v3 = (uintptr)Ꮡ_memdb_g;
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) - 1;
                    v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(_memdb_g.FapMemStore + (uintptr)i * 8))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(_memdb_g.FapMemStore + (uintptr)v2 * 8))).Value;
                    if (_memdb_g.FnMemStore == 0) {
                        Xsqlite3_free(tls, _memdb_g.FapMemStore);
                        _memdb_g.FapMemStore = (uintptr)0;
                    }
                }
                break;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        Xsqlite3_mutex_leave(tls, pVfsMutex);
    } else {
        _memdbEnter(tls, p);
    }
    (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FnRef = (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FnRef - 1;
    if ((@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FnRef <= 0) {
        if (((@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FmFlags & (uint32)SQLITE_DESERIALIZE_FREEONCLOSE) != 0) {
            Xsqlite3_free(tls, (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FaData);
        }
        _memdbLeave(tls, p);
        Xsqlite3_mutex_free(tls, (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FpMutex);
        Xsqlite3_free(tls, p);
    } else {
        _memdbLeave(tls, p);
    }
    return SQLITE_OK;
}

internal static void _minmaxFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    int32 i = default;
    int32 iBest = default;
    int32 mask = default;
    int32 v1 = default;
    uintptr pColl = default;
    _ = i;
    _ = iBest;
    _ = mask;
    _ = pColl;
    _ = v1;
    if (Xsqlite3_user_data(tls, context) == (uintptr)0) {
        v1 = 0;
    } else {
        v1 = -(int32)1;
    }
    mask = v1;
    pColl = _sqlite3GetFuncCollSeq(ref (tls).DerefOrNull(), context);
    iBest = 0;
    if (Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv))) == (int32)SQLITE_NULL) {
        return;
    }
    i = (int32)1;
    for (;;) {
        if (!(i < argc)) {
            break;
        }
        if (Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)i * 8))) == (int32)SQLITE_NULL) {
            return;
        }
        if ((_sqlite3MemCompare(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)iBest * 8)), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)i * 8)), pColl) ^ mask) >= 0) {
            iBest = i;
        }
        goto _2;
_2:;
        i = i + 1;
    }
    Xsqlite3_result_value(tls, context, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)iBest * 8)));
}

internal static void _minmaxStep(ж<libc.TLS> tls, uintptr context, int32 NotUsed, uintptr argv) {
    int32 cmp = default;
    int32 max = default;
    uintptr pArg = default;
    uintptr pBest = default;
    uintptr pColl = default;
    _ = cmp;
    _ = max;
    _ = pArg;
    _ = pBest;
    _ = pColl;
    pArg = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv));
    _ = NotUsed;
    pBest = Xsqlite3_aggregate_context(tls, context, (int32)56);
    if (!(pBest != 0)) {
        return;
    }
    if (Xsqlite3_value_type(tls, pArg) == (int32)SQLITE_NULL) {
        if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pBest)).Value.Fflags != 0) {
            _sqlite3SkipAccumulatorLoad(ref (tls).DerefOrNull(), context);
        }
    } else {
        if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pBest)).Value.Fflags != 0) {
            pColl = _sqlite3GetFuncCollSeq(ref (tls).DerefOrNull(), context);
            max = libc.BoolInt32(Xsqlite3_user_data(tls, context) != (uintptr)0);
            cmp = _sqlite3MemCompare(tls, pBest, pArg, pColl);
            if (max != 0 && cmp < 0 || !(max != 0) && cmp > 0) {
                _sqlite3VdbeMemCopy(tls, pBest, pArg);
            } else {
                _sqlite3SkipAccumulatorLoad(ref (tls).DerefOrNull(), context);
            }
        } else {
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pBest)).Value.Fdb = Xsqlite3_context_db_handle(tls, context);
            _sqlite3VdbeMemCopy(tls, pBest, pArg);
        }
    }
}

internal static void _moveToParent(ж<libc.TLS> tls, uintptr pCur) {
    uintptr pLeaf = default;
    uintptr v1 = default;
    Ti8 v2 = default;
    _ = pLeaf;
    _ = v1;
    _ = v2;
    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnSize = (uint16)0;
    v1 = pCur + 1;
    (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(BTCF_ValidNKey) | libc.Int32FromInt32(BTCF_ValidOvfl)));
    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix = ~@unsafe.Pointer.To<Tu16>(__ccgo_ptr(pCur + 88 + (uintptr)((int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage - (int32)1) * 2));
    pLeaf = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
    v1 = pCur + 84;
    (@unsafe.Pointer.To<Ti8>((@unsafe.Pointer)v1)).Value = (Ti8)(~@unsafe.Pointer.To<Ti8>((@unsafe.Pointer)v1) - 1);
    v2 = ~@unsafe.Pointer.To<Ti8>((@unsafe.Pointer)v1);
    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pCur + 144 + (uintptr)v2 * 8));
    _releasePageNotNull(tls, pLeaf);
}

internal static uintptr /*r*/ _multiSelectCollSeq(ж<libc.TLS> tls, uintptr pParse, uintptr p, int32 iCol) {
    uintptr pRet = default;
    _ = pRet;
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior != 0) {
        pRet = _multiSelectCollSeq(tls, pParse, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior, iCol);
    } else {
        pRet = (uintptr)0;
    }
    if (pRet == (uintptr)0 && iCol < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList)).Value.FnExpr) {
        pRet = _sqlite3ExprCollSeq(tls, pParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList + 8 + (uintptr)iCol * 32))).Value.FpExpr);
    }
    return pRet;
}

internal static void _nodeHashDelete(ref libc.TLS tls, uintptr pRtree, uintptr pNode) {
    uintptr pp = default;
    _ = pp;
    if ((@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FiNode != 0) {
        pp = pRtree + 200 + (uintptr)_nodeHash(ref tls, (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FiNode) * 8;
        for (;;) {
            if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != pNode)) {
                break;
            }
            goto _1;
_1:;
            pp = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) + 32;
        }
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FpNext;
        (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FpNext = (uintptr)0;
    }
}

internal static uintptr /*r*/ _nodeHashLookup(ref libc.TLS tls, uintptr pRtree, Ti64 iNode) {
    uintptr p = default;
    _ = p;
    p = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pRtree + 200 + (uintptr)_nodeHash(ref tls, iNode) * 8));
    for (;;) {
        if (!(p != 0 && (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)p)).Value.FiNode != iNode)) {
            break;
        }
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)p)).Value.FpNext;
    }
    return p;
}

internal static void _nullifFunc(ж<libc.TLS> tls, uintptr context, int32 NotUsed, uintptr argv) {
    uintptr pColl = default;
    _ = pColl;
    pColl = _sqlite3GetFuncCollSeq(ref (tls).DerefOrNull(), context);
    _ = NotUsed;
    if (_sqlite3MemCompare(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)), pColl) != 0) {
        Xsqlite3_result_value(tls, context, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    }
}

internal static int32 /*r*/ _pagerAddPageToRollbackJournal(ж<libc.TLS> tls, uintptr pPg) {
    Tu32 cksum = default;
    Ti64 iOff = default;
    uintptr pData2 = default;
    uintptr pPager = default;
    uintptr v1 = default;
    int32 rc = default;
    _ = cksum;
    _ = iOff;
    _ = pData2;
    _ = pPager;
    _ = rc;
    _ = v1;
    pPager = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpPager;
    iOff = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff;
    pData2 = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpData;
    cksum = _pager_cksum(ref (tls).DerefOrNull(), pPager, pData2);
    v1 = pPg + 52;
    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(PGHDR_NEED_SYNC));
    rc = _write32bits(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, iOff, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.Fpgno);
    if (rc != SQLITE_OK) {
        return rc;
    }
    rc = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, pData2, (int32)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, iOff + (int64)4);
    if (rc != SQLITE_OK) {
        return rc;
    }
    rc = _write32bits(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, iOff + (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize + (int64)4, cksum);
    if (rc != SQLITE_OK) {
        return rc;
    }
    (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(pPager + 96))).Value += (int64)8 + (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize;
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnRec = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnRec + 1;
    rc = _sqlite3BitvecSet(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpInJournal, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.Fpgno);
    rc = rc | _addToSavepointBitvecs(tls, pPager, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.Fpgno);
    return rc;
}

internal static int32 /*r*/ _pagerSyncHotJournal(ж<libc.TLS> tls, uintptr pPager) {
    int32 rc = default;
    _ = rc;
    rc = SQLITE_OK;
    if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnoSync != 0)) {
        rc = _sqlite3OsSync(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, (int32)SQLITE_SYNC_NORMAL);
    }
    if (rc == SQLITE_OK) {
        rc = _sqlite3OsFileSize(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, pPager + 104);
    }
    return rc;
}

internal static int32 /*r*/ _parserStackSizeLimit(ref libc.TLS tls, uintptr pParse) {
    return ~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb + 136 + 12 * 4));
}

internal static void _pcache1Destroy(ж<libc.TLS> tls, uintptr p) {
    uintptr pCache = default;
    uintptr pGroup = default;
    _ = pCache;
    _ = pGroup;
    pCache = p;
    pGroup = (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FpGroup;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)pGroup)).Value.Fmutex);
    if ((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnPage != 0) {
        _pcache1TruncateUnsafe(tls, pCache, (uint32)0);
    }
    (@unsafe.Pointer.To<uint32>(__ccgo_ptr(pGroup + 8))).Value -= (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnMax;
    (@unsafe.Pointer.To<uint32>(__ccgo_ptr(pGroup + 12))).Value -= (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnMin;
    (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)pGroup)).Value.FmxPinned = (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)pGroup)).Value.FnMaxPage + (uint32)10 - (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)pGroup)).Value.FnMinPage;
    _pcache1EnforceMaxPage(tls, pCache);
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)pGroup)).Value.Fmutex);
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FpBulk);
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FapHash);
    Xsqlite3_free(tls, pCache);
}

internal static uintptr /*r*/ _pcache1FetchNoMutex(ж<libc.TLS> tls, uintptr p, uint32 iKey, int32 createFlag) {
    uintptr r = default;

    uintptr pCache = default;
    uintptr pPage = default;
    _ = pCache;
    _ = pPage;
    pCache = p;
    pPage = (uintptr)0;
    pPage = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FapHash + (uintptr)(iKey & ((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnHash - (uint32)1)) * 8));
    while (pPage != 0 && (@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FiKey != iKey) {
        pPage = (@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FpNext;
    }
    if (pPage != 0) {
        if ((@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FpLruNext != (uintptr)0) {
            return _pcache1PinPage(ref (tls).DerefOrNull(), pPage);
        } else {
            return pPage;
        }
    } else {
        if (createFlag != 0) {
            return _pcache1FetchStage2(tls, pCache, iKey, createFlag);
        } else {
            return (uintptr)0;
        }
    }
    return r;
}

internal static void _pcache1Free(ж<libc.TLS> tls, uintptr p) {
    int32 nFreed = default;
    uintptr pSlot = default;
    _ = nFreed;
    _ = pSlot;
    if (p == (uintptr)0) {
        return;
    }
    if ((uint64)p >= (uint64)_pcache1_g.FpStart && (uint64)p < (uint64)_pcache1_g.FpEnd) {
        Xsqlite3_mutex_enter(tls, _pcache1_g.Fmutex);
        _sqlite3StatusDown(ref (tls).DerefOrNull(), (int32)SQLITE_STATUS_PAGECACHE_USED, (int32)1);
        pSlot = p;
        (@unsafe.Pointer.To<TPgFreeslot>((@unsafe.Pointer)pSlot)).Value.FpNext = _pcache1_g.FpFree;
        _pcache1_g.FpFree = pSlot;
        _pcache1_g.FnFreeSlot = _pcache1_g.FnFreeSlot + 1;
        libc.AtomicStoreNInt32((uintptr)Ꮡ_pcache1_g + 140, libc.BoolInt32(_pcache1_g.FnFreeSlot < _pcache1_g.FnReserve), libc.Int32FromInt32(__ATOMIC_RELAXED));
        Xsqlite3_mutex_leave(tls, _pcache1_g.Fmutex);
    } else {
        nFreed = 0;
        nFreed = _sqlite3MallocSize(tls, p);
        Xsqlite3_mutex_enter(tls, _pcache1_g.Fmutex);
        _sqlite3StatusDown(ref (tls).DerefOrNull(), (int32)SQLITE_STATUS_PAGECACHE_OVERFLOW, nFreed);
        Xsqlite3_mutex_leave(tls, _pcache1_g.Fmutex);
        Xsqlite3_free(tls, p);
    }
}

internal static void _pcache1Rekey(ж<libc.TLS> tls, uintptr p, uintptr pPg, uint32 iOld, uint32 iNew) {
    uint32 hNew = default;
    uint32 hOld = default;
    uintptr pCache = default;
    uintptr pPage = default;
    uintptr pp = default;
    _ = hNew;
    _ = hOld;
    _ = pCache;
    _ = pPage;
    _ = pp;
    pCache = p;
    pPage = pPg;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)(@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FpGroup)).Value.Fmutex);
    hOld = iOld & ((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnHash - (uint32)1);
    pp = (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FapHash + (uintptr)hOld * 8;
    while (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != pPage) {
        pp = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) + 24;
    }
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FpNext;
    hNew = iNew & ((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnHash - (uint32)1);
    (@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FiKey = iNew;
    (@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FpNext = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FapHash + (uintptr)hNew * 8));
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FapHash + (uintptr)hNew * 8))).Value = pPage;
    if (iNew > (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FiMaxKey) {
        (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FiMaxKey = iNew;
    }
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)(@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FpGroup)).Value.Fmutex);
}

internal static void _pcache1RemoveFromHash(ж<libc.TLS> tls, uintptr pPage, int32 freeFlag) {
    uint32 h = default;
    uintptr pCache = default;
    uintptr pp = default;
    _ = h;
    _ = pCache;
    _ = pp;
    pCache = (@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FpCache;
    h = (@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FiKey & ((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnHash - (uint32)1);
    pp = (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FapHash + (uintptr)h * 8;
    for (;;) {
        if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != pPage)) {
            break;
        }
        goto _1;
_1:;
        pp = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) + 24;
    }
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))))).Value.FpNext;
    (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnPage = (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnPage - 1;
    if (freeFlag != 0) {
        _pcache1FreePage(tls, pPage);
    }
}

internal static void _pcache1ResizeHash(ж<libc.TLS> tls, uintptr p) {
    uintptr apNew = default;
    uintptr pNext = default;
    uintptr pPage = default;
    uintptr v2 = default;
    uint32 h = default;
    Tu32 i = default;
    Tu64 nNew = default;
    _ = apNew;
    _ = h;
    _ = i;
    _ = nNew;
    _ = pNext;
    _ = pPage;
    _ = v2;
    nNew = (uint64)2 * (uint64)(@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)p)).Value.FnHash;
    if (nNew < (uint64)256) {
        nNew = (uint64)256;
    }
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)(@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)p)).Value.FpGroup)).Value.Fmutex);
    if ((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)p)).Value.FnHash != 0) {
        _sqlite3BeginBenignMalloc(tls);
    }
    apNew = _sqlite3MallocZero(tls, (uint64)8 * nNew);
    if ((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)p)).Value.FnHash != 0) {
        _sqlite3EndBenignMalloc(tls);
    }
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)(@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)p)).Value.FpGroup)).Value.Fmutex);
    if (apNew != 0) {
        i = (uint32)0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)p)).Value.FnHash)) {
                break;
            }
            pNext = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)p)).Value.FapHash + (uintptr)i * 8));
            for (;;) {
                v2 = pNext;
                pPage = v2;
                if (!(v2 != (uintptr)0)) {
                    break;
                }
                h = (uint32)((uint64)(@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FiKey & (nNew - libc.Uint64FromInt32(1)));
                pNext = (@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FpNext;
                (@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FpNext = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apNew + (uintptr)h * 8));
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apNew + (uintptr)h * 8))).Value = pPage;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)p)).Value.FapHash);
        (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)p)).Value.FapHash = apNew;
        (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)p)).Value.FnHash = (uint32)nNew;
    }
}

internal static void _pcache1TruncateUnsafe(ж<libc.TLS> tls, uintptr pCache, uint32 iLimit) {
    uint32 h = default;
    uint32 iStop = default;
    uintptr pPage = default;
    uintptr pp = default;
    uintptr v2 = default;
    _ = h;
    _ = iStop;
    _ = pPage;
    _ = pp;
    _ = v2;
    if ((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FiMaxKey - iLimit < (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnHash) {
        h = iLimit & ((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnHash - (uint32)1);
        iStop = (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FiMaxKey & ((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnHash - (uint32)1);
    } else {
        h = (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnHash / (uint32)2;
        iStop = h - (uint32)1;
    }
    for (;;) {
        pp = (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FapHash + (uintptr)h * 8;
        for (;;) {
            v2 = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp));
            pPage = v2;
            if (!(v2 != (uintptr)0)) {
                break;
            }
            if ((@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FiKey >= iLimit) {
                (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnPage = (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnPage - 1;
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FpNext;
                if ((@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FpLruNext != (uintptr)0) {
                    _pcache1PinPage(ref (tls).DerefOrNull(), pPage);
                }
                _pcache1FreePage(tls, pPage);
            } else {
                pp = pPage + 24;
            }
        }
        if (h == iStop) {
            break;
        }
        h = (h + (uint32)1) & ((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnHash - (uint32)1);
        goto _1;
_1:;
    }
}

internal static int32 /*r*/ _pcache1UnderMemoryPressure(ref libc.TLS tls, uintptr pCache) {
    int32 r = default;

    if (_pcache1_g.FnSlot != 0 && (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FszPage + (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FszExtra <= _pcache1_g.FszSlot) {
        return libc.AtomicLoadNInt32((uintptr)Ꮡ_pcache1_g + 140, libc.Int32FromInt32(__ATOMIC_RELAXED));
    } else {
        return _sqlite3HeapNearlyFull(ref tls);
    }
    return r;
}

internal static void _pcache1Unpin(ж<libc.TLS> tls, uintptr p, uintptr pPg, int32 reuseUnlikely) {
    uintptr pCache = default;
    uintptr pGroup = default;
    uintptr pPage = default;
    uintptr ppFirst = default;
    uintptr v1 = default;
    _ = pCache;
    _ = pGroup;
    _ = pPage;
    _ = ppFirst;
    _ = v1;
    pCache = p;
    pPage = pPg;
    pGroup = (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FpGroup;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)pGroup)).Value.Fmutex);
    if (reuseUnlikely != 0 || (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)pGroup)).Value.FnPurgeable > (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)pGroup)).Value.FnMaxPage) {
        _pcache1RemoveFromHash(tls, pPage, (int32)1);
    } else {
        ppFirst = pGroup + 24 + 40;
        (@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FpLruPrev = pGroup + 24;
        v1 = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppFirst));
        (@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)pPage)).Value.FpLruNext = v1;
        (@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)v1)).Value.FpLruPrev = pPage;
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppFirst))).Value = pPage;
        (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnRecyclable = (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnRecyclable + 1;
    }
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)(@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FpGroup)).Value.Fmutex);
}

internal static uintptr /*r*/ _pcacheMergeDirtyList(ref libc.TLS tls, uintptr pA, uintptr pB) {
    @unsafe.Pointer pTail = default;
    ref var result = ref heap(new TPgHdr(), out var Ꮡresult);
    _ = pTail;
    _ = result;
    pTail = @unsafe.Pointer.FromPinnedBox(Ꮡresult);
    for (;;) {
        if ((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pA)).Value.Fpgno < (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pB)).Value.Fpgno) {
            (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pTail, 32))).Value = pA;
            pTail = (@unsafe.Pointer)pA;
            pA = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pA)).Value.FpDirty;
            if (pA == (uintptr)0) {
                (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pTail, 32))).Value = pB;
                break;
            }
        } else {
            (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pTail, 32))).Value = pB;
            pTail = (@unsafe.Pointer)pB;
            pB = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pB)).Value.FpDirty;
            if (pB == (uintptr)0) {
                (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pTail, 32))).Value = pA;
                break;
            }
        }
        goto _1;
_1:;
    }
    return result.FpDirty;
}

internal static void _printfFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        uintptr db = default;
        uintptr zFormat = default;
        uintptr v1 = default;
        bool v2 = default;
        TStrAccum _ᴛ1 = default;
        TPrintfArguments _ᴛ2 = default;
        _ = db;
        _ = zFormat;
        _ = v1;
        _ = v2;
        db = Xsqlite3_context_db_handle(tls, context);
        v2 = argc >= (int32)1; if (v2) {
            v1 = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
            zFormat = v1;
        }
        if (v2 && v1 != (uintptr)0) {
            (@unsafe.Pointer.To<TPrintfArguments>(__ccgo_ptr(bp))).Value.FnArg = argc - (int32)1;
            (@unsafe.Pointer.To<TPrintfArguments>(__ccgo_ptr(bp))).Value.FnUsed = 0;
            (@unsafe.Pointer.To<TPrintfArguments>(__ccgo_ptr(bp))).Value.FapArg = argv + (uintptr)1 * 8;
            _sqlite3StrAccumInit(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 16), db, (uintptr)0, 0, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(db + 136)));
            (@unsafe.Pointer.To<TStrAccum>(__ccgo_ptr(bp + 16))).Value.FprintfFlags = (uint8)SQLITE_PRINTF_SQLFUNC;
            Xsqlite3_str_appendf(tls, bp + 16, zFormat, libc.VaList(bp + 56, bp));
            Xsqlite3_result_str(tls, context, bp + 16, (int32)SQLITE_XFER);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _quoteFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr db = default;
        Tsqlite3_str _ᴛ1 = default;
        _ = db;
        db = Xsqlite3_context_db_handle(tls, context);
        _ = argc;
        _sqlite3StrAccumInit(ref (tls).DerefOrNull(), __ccgo_ptr(bp), db, (uintptr)0, 0, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(db + 136)));
        _sqlite3QuoteValue(tls, bp, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)), (int32)(int64)Xsqlite3_user_data(tls, context));
        Xsqlite3_result_str(tls, context, bp, (int32)SQLITE_XFER);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _rbuMPrintfExec(ж<libc.TLS> tls, uintptr p, uintptr db, uintptr zFmt, uintptr va) {
    Tva_list ap = default;
    uintptr zSql = default;
    _ = ap;
    _ = zSql;
    ap = va;
    zSql = Xsqlite3_vmprintf(tls, zFmt, ap);
    if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        if (zSql == (uintptr)0) {
            (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_NOMEM;
        } else {
            (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = _sqlite3_exec__ccgo_gp(tls, db, zSql, (uintptr)0, (uintptr)0, (@unsafe.Pointer)(p + 64));
        }
    }
    Xsqlite3_free(tls, zSql);
    _ = ap;
    return (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc;
}

internal static void _rbuMainlistRemove(ж<libc.TLS> tls, uintptr p) {
    uintptr pp = default;
    _ = pp;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Trbu_vfs>((@unsafe.Pointer)(@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpRbuVfs)).Value.Fmutex);
    pp = (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpRbuVfs + 192;
    for (;;) {
        if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != 0 && ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != p)) {
            break;
        }
        goto _1;
_1:;
        pp = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) + 88;
    }
    if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != 0) {
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpMainNext;
    }
    (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpMainNext = (uintptr)0;
    pp = (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpRbuVfs + 200;
    for (;;) {
        if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != 0 && ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != p)) {
            break;
        }
        goto _2;
_2:;
        pp = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) + 96;
    }
    if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != 0) {
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpMainRbuNext;
    }
    (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpMainRbuNext = (uintptr)0;
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Trbu_vfs>((@unsafe.Pointer)(@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpRbuVfs)).Value.Fmutex);
}

internal static void _rbuObjIterFreeCols(ж<libc.TLS> tls, uintptr pIter) {
    int32 i = default;
    _ = i;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FnTblCol)) {
            break;
        }
        Xsqlite3_free(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FazTblCol + (uintptr)i * 8)));
        Xsqlite3_free(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FazTblType + (uintptr)i * 8)));
        goto _1;
_1:;
        i = i + 1;
    }
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FazTblCol);
    (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FazTblCol = (uintptr)0;
    (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FazTblType = (uintptr)0;
    (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FaiSrcOrder = (uintptr)0;
    (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FabTblPk = (uintptr)0;
    (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FabNotNull = (uintptr)0;
    (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FnTblCol = 0;
    (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FeType = 0;
}

internal static void _rbuTmpInsertFunc(ж<libc.TLS> tls, uintptr pCtx, int32 nVal, uintptr apVal) {
    int32 i = default;
    int32 rc = default;
    uintptr p = default;
    _ = i;
    _ = p;
    _ = rc;
    p = Xsqlite3_user_data(tls, pCtx);
    rc = SQLITE_OK;
    if (Xsqlite3_value_int(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apVal))) != 0) {
        (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(p + 312))).Value += (int64)(@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Fobjiter.FnIndex;
    }
    i = 0;
    for (;;) {
        if (!(rc == SQLITE_OK && i < nVal)) {
            break;
        }
        rc = Xsqlite3_bind_value(tls, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Fobjiter.FpTmpInsert, i + (int32)1, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apVal + (uintptr)i * 8)));
        goto _1;
_1:;
        i = i + 1;
    }
    if (rc == SQLITE_OK) {
        Xsqlite3_step(tls, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Fobjiter.FpTmpInsert);
        rc = Xsqlite3_reset(tls, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Fobjiter.FpTmpInsert);
    }
    if (rc != SQLITE_OK) {
        Xsqlite3_result_error_code(tls, pCtx, rc);
    }
}

internal static int32 /*r*/ _rbuUpdateTempSize(ref libc.TLS tls, uintptr pFd, Tsqlite3_int64 nNew) {
    Ti64 nDiff = default;
    uintptr pRbu = default;
    _ = nDiff;
    _ = pRbu;
    pRbu = (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)pFd)).Value.FpRbu;
    nDiff = nNew - (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)pFd)).Value.Fsz;
    (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(pRbu + 384))).Value += nDiff;
    (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)pFd)).Value.Fsz = nNew;
    if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu)).Value.FszTempLimit != 0 && (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu)).Value.FszTemp > (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu)).Value.FszTempLimit) {
        return (int32)SQLITE_FULL;
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _rbuVfsClose(ж<libc.TLS> tls, uintptr pFile) {
    int32 i = default;
    int32 rc = default;
    uintptr p = default;
    uintptr pMeth = default;
    _ = i;
    _ = p;
    _ = pMeth;
    _ = rc;
    p = pFile;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FnShm)) {
            break;
        }
        Xsqlite3_free(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FapShm + (uintptr)i * 8)));
        goto _1;
_1:;
        i = i + 1;
    }
    Xsqlite3_free(tls, (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FapShm);
    (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FapShm = (uintptr)0;
    Xsqlite3_free(tls, (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FzDel);
    if (((@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FopenFlags & (int32)SQLITE_OPEN_MAIN_DB) != 0) {
        pMeth = (@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpReal)).Value.FpMethods;
        _rbuMainlistRemove(tls, p);
        _rbuUnlockShm(tls, p);
        if ((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)pMeth)).Value.FiVersion > (int32)1 && (@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)pMeth)).Value.FxShmUnmap != 0) {
            GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)pMeth)).Value.FxShmUnmap)(tls, (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpReal, 0);
        }
    } else {
        if (((@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FopenFlags & (int32)SQLITE_OPEN_DELETEONCLOSE) != 0 && (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpRbu != 0) {
            _rbuUpdateTempSize(ref (tls).DerefOrNull(), p, 0);
        }
    }
    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpReal)).Value.FpMethods)).Value.FxClose)(tls, (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpReal);
    return rc;
}

internal static void _releaseAllSavepoints(ж<libc.TLS> tls, uintptr pPager) {
    int32 ii = default;
    _ = ii;
    ii = 0;
    for (;;) {
        if (!(ii < (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnSavepoint)) {
            break;
        }
        _sqlite3BitvecDestroy(tls, (@unsafe.Pointer.To<TPagerSavepoint>(__ccgo_ptr((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FaSavepoint + (uintptr)ii * 56))).Value.FpInSavepoint);
        goto _1;
_1:;
        ii = ii + 1;
    }
    if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FexclusiveMode != 0) || _sqlite3JournalIsInMemory(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fsjfd) != 0) {
        _sqlite3OsClose(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fsjfd);
    }
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FaSavepoint);
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FaSavepoint = (uintptr)0;
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnSavepoint = 0;
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnSubRec = (uint32)0;
}

internal static void _removeElement(ж<libc.TLS> tls, uintptr pH, uintptr elem) {
    uintptr pEntry = default;
    _ = pEntry;
    if ((@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fprev != 0) {
        (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)(@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fprev)).Value.Fnext = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fnext;
    } else {
        (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Ffirst = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fnext;
    }
    if ((@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fnext != 0) {
        (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)(@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fnext)).Value.Fprev = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fprev;
    }
    if ((@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fht != 0) {
        pEntry = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fht + (uintptr)((@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fh % (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fhtsize) * 16;
        if ((@unsafe.Pointer.To<T_ht>((@unsafe.Pointer)pEntry)).Value.Fchain == elem) {
            (@unsafe.Pointer.To<T_ht>((@unsafe.Pointer)pEntry)).Value.Fchain = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fnext;
        }
        (@unsafe.Pointer.To<T_ht>((@unsafe.Pointer)pEntry)).Value.Fcount = (@unsafe.Pointer.To<T_ht>((@unsafe.Pointer)pEntry)).Value.Fcount - 1;
    }
    Xsqlite3_free(tls, elem);
    (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fcount = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fcount - 1;
    if ((@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fcount == (uint32)0) {
        _sqlite3HashClear(tls, (@unsafe.Pointer)pH);
    }
}

internal static void _removeFromBlockedList(ref libc.TLS tls, uintptr db) {
    uintptr pp = default;
    _ = pp;
    pp = (uintptr)@unsafe.Pointer.FromBox(Ꮡ_sqlite3BlockedList);
    for (;;) {
        if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != 0)) {
            break;
        }
        if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) == db) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))))).Value.FpNextBlocked;
            break;
        }
        goto _1;
_1:;
        pp = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) + 864;
    }
}

internal static void _renameColumnIdlistNames(ж<libc.TLS> tls, @unsafe.Pointer pParse, @unsafe.Pointer pCtx, uintptr pIdList, uintptr zOld) {
    int32 i = default;
    uintptr zName = default;
    _ = i;
    _ = zName;
    if (pIdList != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TIdList>((@unsafe.Pointer)pIdList)).Value.FnId)) {
                break;
            }
            zName = (@unsafe.Pointer.To<TIdList_item>((@unsafe.Pointer)(pIdList + 8 + (uintptr)i * 8))).Value.FzName;
            if (0 == Xsqlite3_stricmp(tls, zName, zOld)) {
                _renameTokenFind(ref (tls).DerefOrNull(), pParse, pCtx, zName);
            }
            goto _1;
_1:;
            i = i + 1;
        }
    }
}

internal static uintptr /*r*/ _renameColumnTokenNext(ref libc.TLS tls, @unsafe.Pointer pCtx) {
    uintptr pBest = default;
    uintptr pToken = default;
    @unsafe.Pointer pp = default;
    _ = pBest;
    _ = pToken;
    _ = pp;
    pBest = ~@unsafe.Pointer.To<uintptr>(pCtx);
    pToken = (@unsafe.Pointer.To<TRenameToken>((@unsafe.Pointer)pBest)).Value.FpNext;
    for (;;) {
        if (!(pToken != 0)) {
            break;
        }
        if ((@unsafe.Pointer.To<TRenameToken>((@unsafe.Pointer)pToken)).Value.Ft.Fz > (@unsafe.Pointer.To<TRenameToken>((@unsafe.Pointer)pBest)).Value.Ft.Fz) {
            pBest = pToken;
        }
        goto _1;
_1:;
        pToken = (@unsafe.Pointer.To<TRenameToken>((@unsafe.Pointer)pToken)).Value.FpNext;
    }
    pp = pCtx;
    for (;;) {
        if (!(~@unsafe.Pointer.To<uintptr>(pp) != pBest)) {
            break;
        }
        goto _2;
_2:;
        pp = (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pp) + 24);
    }
    (@unsafe.Pointer.To<uintptr>(pp)).Value = (@unsafe.Pointer.To<TRenameToken>((@unsafe.Pointer)pBest)).Value.FpNext;
    return pBest;
}

internal static void _renameParseCleanup(ж<libc.TLS> tls, @unsafe.Pointer pParse) {
    uintptr db = default;
    uintptr pIdx = default;
    uintptr v1 = default;
    _ = db;
    _ = pIdx;
    _ = v1;
    db = ~@unsafe.Pointer.To<uintptr>(pParse);
    if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 16)) != 0) {
        _sqlite3VdbeFinalize(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 16)));
    }
    _sqlite3DeleteTable(tls, (@unsafe.Pointer)db, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 368)));
    for (;;) {
        v1 = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 376));
        pIdx = v1;
        if (!(v1 != (uintptr)0)) {
            break;
        }
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 376))).Value = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
        _sqlite3FreeIndex(tls, (@unsafe.Pointer)db, pIdx);
    }
    _sqlite3DeleteTrigger(tls, (@unsafe.Pointer)db, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 384)));
    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 8)));
    _renameTokenFree(tls, db, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 432)));
    _sqlite3ParseObjectReset(tls, pParse);
}

internal static uintptr /*r*/ _renameTokenFind(ref libc.TLS tls, @unsafe.Pointer pParse, @unsafe.Pointer pCtx, uintptr pPtr) {
    uintptr pToken = default;
    @unsafe.Pointer pp = default;
    _ = pToken;
    _ = pp;
    if (pPtr == (uintptr)0) {
        return (uintptr)0;
    }
    pp = @unsafe.Add(pParse, 432);
    for (;;) {
        if (!(~@unsafe.Pointer.To<uintptr>(pp) != 0)) {
            break;
        }
        if ((@unsafe.Pointer.To<TRenameToken>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pp)))).Value.Fp == pPtr) {
            pToken = ~@unsafe.Pointer.To<uintptr>(pp);
            if (pCtx != nil) {
                (@unsafe.Pointer.To<uintptr>(pp)).Value = (@unsafe.Pointer.To<TRenameToken>((@unsafe.Pointer)pToken)).Value.FpNext;
                (@unsafe.Pointer.To<TRenameToken>((@unsafe.Pointer)pToken)).Value.FpNext = ~@unsafe.Pointer.To<uintptr>(pCtx);
                (@unsafe.Pointer.To<uintptr>(pCtx)).Value = pToken;
                (@unsafe.Pointer.To<int32>(@unsafe.Add(pCtx, 8))).Value = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pCtx, 8)) + 1;
            }
            return pToken;
        }
        goto _1;
_1:;
        pp = (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pp) + 24);
    }
    return (uintptr)0;
}

internal static void _renumberCursorDoMapping(ref libc.TLS tls, uintptr pWalker, uintptr piCursor) {
    uintptr aCsrMap = default;
    int32 iCsr = default;
    _ = aCsrMap;
    _ = iCsr;
    aCsrMap = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWalker + 40));
    iCsr = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(piCursor));
    if (iCsr < ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aCsrMap)) && ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aCsrMap + (uintptr)(iCsr + (int32)1) * 4)) > 0) {
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(piCursor))).Value = (@unsafe.Pointer.To<int32>(__ccgo_ptr(aCsrMap + (uintptr)(iCsr + (int32)1) * 4))).Value;
    }
}

internal static uintptr /*r*/ _rtreeCheckPrepare(ж<libc.TLS> tls, @unsafe.Pointer pCheck, uintptr zFmt, uintptr va) {
    Tva_list ap = default;
    ref var pRet = ref heap(new uintptr(), out var ᏑpRet);
    uintptr z = default;
    _ = ap;
    _ = pRet;
    _ = z;
    pRet = (uintptr)0;
    ap = va;
    z = Xsqlite3_vmprintf(tls, zFmt, ap);
    if (~@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 64)) == SQLITE_OK) {
        if (z == (uintptr)0) {
            (@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 64))).Value = (int32)SQLITE_NOMEM;
        } else {
            (@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 64))).Value = _sqlite3_prepare_v2__ccgo_gp(tls, ~@unsafe.Pointer.To<uintptr>(pCheck), z, -(int32)1, @unsafe.Pointer.FromBox(ᏑpRet), nil);
        }
    }
    Xsqlite3_free(tls, z);
    _ = ap;
    return pRet;
}

internal static void _rtreeCheckReset(ж<libc.TLS> tls, @unsafe.Pointer pCheck, uintptr pStmt) {
    int32 rc = default;
    _ = rc;
    rc = Xsqlite3_reset(tls, pStmt);
    if (~@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 64)) == SQLITE_OK) {
        (@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 64))).Value = rc;
    }
}

internal static void _rtreeMatchArgFree(ж<libc.TLS> tls, uintptr pArg) {
    int32 i = default;
    uintptr p = default;
    _ = i;
    _ = p;
    p = pArg;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TRtreeMatchArg>((@unsafe.Pointer)p)).Value.FnParam)) {
            break;
        }
        Xsqlite3_value_free(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TRtreeMatchArg>((@unsafe.Pointer)p)).Value.FapSqlParam + (uintptr)i * 8)));
        goto _1;
_1:;
        i = i + 1;
    }
    Xsqlite3_free(tls, p);
}

internal static void _rtreeRelease(ж<libc.TLS> tls, uintptr pRtree) {
    int32 i = default;
    uintptr pNext = default;
    _ = i;
    _ = pNext;
    (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnBusy = (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnBusy - 1;
    if ((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnBusy == (uint32)0) {
        (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FinWrTrans = (uint8)0;
        _nodeBlobReset(tls, pRtree);
        if ((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnNodeRef != 0) {
            i = 0;
            for (;;) {
                if (!(i < (int32)HASHSIZE)) {
                    break;
                }
                while (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pRtree + 200 + (uintptr)i * 8)) != 0) {
                    pNext = (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pRtree + 200 + (uintptr)i * 8))))).Value.FpNext;
                    Xsqlite3_free(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pRtree + 200 + (uintptr)i * 8)));
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pRtree + 200 + (uintptr)i * 8))).Value = pNext;
                }
                goto _1;
_1:;
                i = i + 1;
            }
        }
        Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpWriteNode);
        Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpDeleteNode);
        Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpReadRowid);
        Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpWriteRowid);
        Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpDeleteRowid);
        Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpReadParent);
        Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpWriteParent);
        Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpDeleteParent);
        Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpWriteAux);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzReadAuxSql);
        Xsqlite3_free(tls, pRtree);
    }
}

internal static uintptr /*r*/ _rtreeSearchPointFirst(ref libc.TLS tls, uintptr pCur) {
    uintptr v1 = default;
    uintptr v2 = default;
    _ = v1;
    _ = v2;
    if ((@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCur)).Value.FbPoint != 0) {
        v1 = pCur + 64;
    } else {
        if ((@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCur)).Value.FnPoint != 0) {
            v2 = (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCur)).Value.FaPoint;
        } else {
            v2 = (uintptr)0;
        }
        v1 = v2;
    }
    return v1;
}

internal static void _rtreeSearchPointSwap(ж<libc.TLS> tls, uintptr p, int32 i, int32 j) {
    uintptr pTemp = default;
    TRtreeSearchPoint t = default;
    _ = pTemp;
    _ = t;
    t = ~@unsafe.Pointer.To<TRtreeSearchPoint>(__ccgo_ptr((@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)p)).Value.FaPoint + (uintptr)i * 24));
    (@unsafe.Pointer.To<TRtreeSearchPoint>(__ccgo_ptr((@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)p)).Value.FaPoint + (uintptr)i * 24))).Value = (@unsafe.Pointer.To<TRtreeSearchPoint>(__ccgo_ptr((@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)p)).Value.FaPoint + (uintptr)j * 24))).Value;
    (@unsafe.Pointer.To<TRtreeSearchPoint>(__ccgo_ptr((@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)p)).Value.FaPoint + (uintptr)j * 24))).Value = t;
    i = i + 1;
    j = j + 1;
    if (i < (int32)RTREE_CACHE_SZ) {
        if (j >= (int32)RTREE_CACHE_SZ) {
            _nodeRelease(tls, (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)p)).Value.Fbase.FpVtab, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 88 + (uintptr)i * 8)));
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 88 + (uintptr)i * 8))).Value = (uintptr)0;
        } else {
            pTemp = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 88 + (uintptr)i * 8));
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 88 + (uintptr)i * 8))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 88 + (uintptr)j * 8))).Value;
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 88 + (uintptr)j * 8))).Value = pTemp;
        }
    }
}

internal static int32 /*r*/ _rtreeShadowName(ж<libc.TLS> tls, uintptr zName) {
    uint32 i = default;
    _ = i;
    i = (uint32)0;
    for (;;) {
        if (!((uint64)i < libc.Uint64FromInt64(24) / libc.Uint64FromInt64(8))) {
            break;
        }
        if (Xsqlite3_stricmp(tls, zName, _azName1[i]) == 0) {
            return (int32)1;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return 0;
}

internal static void _sampleClear(ж<libc.TLS> tls, uintptr db, uintptr p) {
    if ((@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)p)).Value.FnRowid != 0) {
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 24)));
        (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)p)).Value.FnRowid = (uint32)0;
    }
}

internal static void _samplePushPrevious(ж<libc.TLS> tls, uintptr p, int32 iChng) {
    int32 i = default;
    int32 j = default;
    int32 j1 = default;
    uintptr pBest = default;
    _ = i;
    _ = j;
    _ = j1;
    _ = pBest;
    i = (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnCol - (int32)2;
    for (;;) {
        if (!(i >= iChng)) {
            break;
        }
        pBest = (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FaBest + (uintptr)i * 48;
        (@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pBest)).Value.FanEq + (uintptr)i * 8))).Value = (@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fcurrent.FanEq + (uintptr)i * 8))).Value;
        if ((@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnSample < (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FmxSample || _sampleIsBetter(ref (tls).DerefOrNull(), p, pBest, (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fa + (uintptr)(@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FiMin * 48) != 0) {
            _sampleInsert(tls, p, pBest, i);
        }
        goto _1;
_1:;
        i = i - 1;
    }
    i = (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnSample - (int32)1;
    for (;;) {
        if (!(i >= 0)) {
            break;
        }
        j = (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnMaxEqZero;
        for (;;) {
            if (!(j < (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnCol)) {
                break;
            }
            goto _3;
_3:;
            j = j + 1;
        }
        goto _2;
_2:;
        i = i - 1;
    }
    if (iChng < (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnMaxEqZero) {
        i = (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnSample - (int32)1;
        for (;;) {
            if (!(i >= 0)) {
                break;
            }
            j1 = iChng;
            for (;;) {
                if (!(j1 < (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnCol)) {
                    break;
                }
                if (~@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TStatSample>(__ccgo_ptr((@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fa + (uintptr)i * 48))).Value.FanEq + (uintptr)j1 * 8)) == (uint64)0) {
                    (@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TStatSample>(__ccgo_ptr((@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fa + (uintptr)i * 48))).Value.FanEq + (uintptr)j1 * 8))).Value = (@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fcurrent.FanEq + (uintptr)j1 * 8))).Value;
                }
                goto _5;
_5:;
                j1 = j1 + 1;
            }
            goto _4;
_4:;
            i = i - 1;
        }
        (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnMaxEqZero = iChng;
    }
}

internal static void _sampleSetRowidInt64(ж<libc.TLS> tls, uintptr db, uintptr p, Ti64 iRowid) {
    if ((@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)p)).Value.FnRowid != 0) {
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 24)));
    }
    (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)p)).Value.FnRowid = (uint32)0;
    (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)(p + 24))).Value = iRowid;
}

internal static uintptr /*r*/ _searchWith(ref libc.TLS tls, uintptr pWith, uintptr pItem, @unsafe.Pointer ppContext) {
    int32 i = default;
    uintptr p = default;
    uintptr zName = default;
    _ = i;
    _ = p;
    _ = zName;
    zName = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName;
    p = pWith;
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TWith>((@unsafe.Pointer)p)).Value.FnCte)) {
                break;
            }
            if (_sqlite3StrICmp(ref tls, zName, (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)(p + 16 + (uintptr)i * 48))).Value.FzName) == 0) {
                (@unsafe.Pointer.To<uintptr>(ppContext)).Value = p;
                return p + 16 + (uintptr)i * 48;
            }
            goto _2;
_2:;
            i = i + 1;
        }
        if ((@unsafe.Pointer.To<TWith>((@unsafe.Pointer)p)).Value.FbView != 0) {
            break;
        }
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TWith>((@unsafe.Pointer)p)).Value.FpOuter;
    }
    return (uintptr)0;
}

internal static void _selectRefLeave(ж<libc.TLS> tls, uintptr pWalker, uintptr pSelect) {
    uintptr p = default;
    uintptr pSrc = default;
    _ = p;
    _ = pSrc;
    p = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWalker + 40));
    pSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc;
    if ((@unsafe.Pointer.To<TRefSrcList>((@unsafe.Pointer)p)).Value.FnExclude != 0) {
        (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(p + 16))).Value -= (int64)(@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc;
    }
}

internal static int32 /*r*/ _selectWindowRewriteSelectCb(ж<libc.TLS> tls, uintptr pWalker, uintptr pSelect) {
    uintptr p = default;
    uintptr pSave = default;
    _ = p;
    _ = pSave;
    p = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWalker + 40));
    pSave = (@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpSubSelect;
    if (pSave == pSelect) {
        return WRC_Continue;
    } else {
        (@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpSubSelect = pSelect;
        _sqlite3WalkSelect(tls, pWalker, pSelect);
        (@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpSubSelect = pSave;
    }
    return (int32)WRC_Prune;
}

internal static void _sessionAppendByte(ж<libc.TLS> tls, @unsafe.Pointer p, Tu8 v, @unsafe.Pointer pRc) {
    int32 v1 = default;
    @unsafe.Pointer v2 = default;
    _ = v1;
    _ = v2;
    if (0 == _sessionBufferGrow(tls, p, (int64)1, pRc)) {
        v2 = @unsafe.Add(p, 8);
        v1 = ~@unsafe.Pointer.To<int32>(v2);
        (@unsafe.Pointer.To<int32>(v2)).Value = ~@unsafe.Pointer.To<int32>(v2) + 1;
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(p) + (uintptr)v1))).Value = v;
    }
}

internal static void _sessionAppendValue(ж<libc.TLS> tls, uintptr p, uintptr pVal, @unsafe.Pointer pRc) {
    ref var nByte = ref heap(new Tsqlite3_int64(), out var ᏑnByte);
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    uintptr v1 = default;
    _ = nByte;
    _ = rc;
    _ = v1;
    rc = ~@unsafe.Pointer.To<int32>(pRc);
    if (rc == SQLITE_OK) {
        nByte = 0;
        rc = _sessionSerializeValue(tls, (uintptr)0, pVal, @unsafe.Pointer.FromPinnedBox(ᏑnByte));
        _sessionBufferGrow(tls, (@unsafe.Pointer)p, nByte, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        if (rc == SQLITE_OK) {
            rc = _sessionSerializeValue(tls, (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)p)).Value.FaBuf + (uintptr)(@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)p)).Value.FnBuf, pVal, nil);
            v1 = p + 8;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = (int32)((int64)(~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)) + nByte);
        } else {
            (@unsafe.Pointer.To<int32>(pRc)).Value = rc;
        }
    }
}

internal static void _sessionAppendVarint(ж<libc.TLS> tls, @unsafe.Pointer p, int32 v, @unsafe.Pointer pRc) {
    if (0 == _sessionBufferGrow(tls, p, (int64)9, pRc)) {
        (@unsafe.Pointer.To<int32>(@unsafe.Add(p, 8))).Value += _sessionVarintPut(tls, (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(p) + (uintptr)(~@unsafe.Pointer.To<int32>(@unsafe.Add(p, 8)))), v);
    }
}

internal static int32 /*r*/ _sessionApplyOneOp(ж<libc.TLS> tls, uintptr pIter, uintptr p, uintptr __ccgo_fp_xConflict, uintptr pCtx, @unsafe.Pointer pbReplace, @unsafe.Pointer pbRetry) {
    uintptr abPK = default;
    uintptr pNew = default;
    uintptr pOld = default;
    ref var pUp = ref heap(new uintptr(), out var ᏑpUp);
    ref var zDummy = ref heap(new uintptr(), out var ᏑzDummy);
    uintptr v1 = default;
    int32 bPatchset = default;
    int32 i = default;
    ref var nCol = ref heap(new int32(), out var ᏑnCol);
    ref var op = ref heap(new int32(), out var Ꮡop);
    int32 rc = default;
    _ = abPK;
    _ = bPatchset;
    _ = i;
    _ = nCol;
    _ = op;
    _ = pNew;
    _ = pOld;
    _ = pUp;
    _ = rc;
    _ = zDummy;
    _ = v1;
    rc = SQLITE_OK;
    _sqlite3changeset_op__ccgo_gp(ref (tls).DerefOrNull(), pIter, @unsafe.Pointer.FromBox(ᏑzDummy), @unsafe.Pointer.FromPinnedBox(ᏑnCol), @unsafe.Pointer.FromPinnedBox(Ꮡop), nil);
    if (op == (int32)SQLITE_DELETE) {
        if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FbPatchset != 0) {
            v1 = (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FabPK;
        } else {
            v1 = (uintptr)0;
        }
        abPK = v1;
        rc = _sessionBindRow(tls, pIter, __ccgo_fp(Xsqlite3changeset_old), nCol, abPK, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FpDelete);
        if (rc == SQLITE_OK && Xsqlite3_bind_parameter_count(tls, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FpDelete) > nCol) {
            rc = Xsqlite3_bind_int(tls, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FpDelete, nCol + (int32)1, libc.BoolInt32(pbRetry == nil || abPK != 0));
        }
        if (rc != SQLITE_OK) {
            return rc;
        }
        Xsqlite3_step(tls, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FpDelete);
        rc = Xsqlite3_reset(tls, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FpDelete);
        if (rc == SQLITE_OK && Xsqlite3_changes(tls, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.Fdb) == 0) {
            rc = _sessionConflictHandler(tls, (int32)SQLITE_CHANGESET_DATA, p, pIter, __ccgo_fp_xConflict, pCtx, pbRetry);
        } else {
            if ((rc & (int32)0xff) == (int32)SQLITE_CONSTRAINT) {
                rc = _sessionConflictHandler(tls, (int32)SQLITE_CHANGESET_CONFLICT, p, pIter, __ccgo_fp_xConflict, pCtx, nil);
            }
        }
    } else {
        if (op == (int32)SQLITE_UPDATE) {
            pUp = (uintptr)0;
            bPatchset = libc.BoolInt32(pbRetry == nil || (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FbPatchset != 0);
            rc = _sessionUpdateFind(tls, pIter, p, bPatchset, @unsafe.Pointer.FromBox(ᏑpUp));
            i = 0;
            for (;;) {
                if (!(rc == SQLITE_OK && i < nCol)) {
                    break;
                }
                pOld = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FapValue + (uintptr)i * 8));
                pNew = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FapValue + (uintptr)((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FnCol + i) * 8));
                if (pOld != 0 && (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FabPK + (uintptr)i)) != 0 || bPatchset == 0)) {
                    rc = _sessionBindValue(tls, pUp, i * (int32)2 + (int32)2, pOld);
                }
                if (rc == SQLITE_OK && pNew != 0) {
                    rc = _sessionBindValue(tls, pUp, i * (int32)2 + (int32)1, pNew);
                }
                goto _2;
_2:;
                i = i + 1;
            }
            if (rc != SQLITE_OK) {
                return rc;
            }
            Xsqlite3_step(tls, pUp);
            rc = Xsqlite3_reset(tls, pUp);
            if (rc == SQLITE_OK && Xsqlite3_changes(tls, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.Fdb) == 0) {
                rc = _sessionConflictHandler(tls, (int32)SQLITE_CHANGESET_DATA, p, pIter, __ccgo_fp_xConflict, pCtx, pbRetry);
            } else {
                if ((rc & (int32)0xff) == (int32)SQLITE_CONSTRAINT) {
                    rc = _sessionConflictHandler(tls, (int32)SQLITE_CHANGESET_CONFLICT, p, pIter, __ccgo_fp_xConflict, pCtx, nil);
                }
            }
        } else {
            if ((@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FbStat1 != 0) {
                rc = _sessionSeekToRow(tls, pIter, p);
                if (rc == (int32)SQLITE_ROW) {
                    rc = (int32)SQLITE_CONSTRAINT;
                    Xsqlite3_reset(tls, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FpSelect);
                }
            }
            if (rc == SQLITE_OK) {
                rc = _sessionBindRow(tls, pIter, __ccgo_fp(Xsqlite3changeset_new), nCol, (uintptr)0, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FpInsert);
                if (rc != SQLITE_OK) {
                    return rc;
                }
                Xsqlite3_step(tls, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FpInsert);
                rc = Xsqlite3_reset(tls, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FpInsert);
            }
            if ((rc & (int32)0xff) == (int32)SQLITE_CONSTRAINT) {
                rc = _sessionConflictHandler(tls, (int32)SQLITE_CHANGESET_CONFLICT, p, pIter, __ccgo_fp_xConflict, pCtx, pbReplace);
            }
        }
    }
    return rc;
}

internal static int32 /*r*/ _sessionChangegroupOutput(ж<libc.TLS> tls, uintptr pGrp, uintptr __ccgo_fp_xOutput, uintptr pOut, uintptr pnOut, uintptr ppOut) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 i = default;
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        uintptr p = default;
        uintptr pTab = default;
        TSessionBuffer _ᴛ1 = default;
        _ = i;
        _ = p;
        _ = pTab;
        _ = rc;
        rc = SQLITE_OK;
        (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value = new TSessionBuffer();
        pTab = (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.FpList;
        for (;;) {
            if (!(rc == SQLITE_OK && pTab != 0)) {
                break;
            }
            if ((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnEntry == 0) {
                goto _1;
            }
            _sessionAppendTableHdr(tls, bp, (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.FbPatch, pTab, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnChange)) {
                    break;
                }
                p = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FapChange + (uintptr)i * 8));
                for (;;) {
                    if (!(p != 0)) {
                        break;
                    }
                    _sessionAppendByte(tls, __ccgo_ptr(bp), (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)p)).Value.Fop, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                    _sessionAppendByte(tls, __ccgo_ptr(bp), (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)p)).Value.FbIndirect, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                    _sessionAppendBlob(tls, bp, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)p)).Value.FaRecord, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)p)).Value.FnRecord, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                    if (rc == SQLITE_OK && __ccgo_fp_xOutput != 0 && (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf >= _sessions_strm_chunk_size) {
                        rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, int32>>(__ccgo_fp_xOutput)(tls, pOut, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf);
                        (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf = 0;
                    }
                    goto _3;
_3:;
                    p = (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)p)).Value.FpNext;
                }
                goto _2;
_2:;
                i = i + 1;
            }
            goto _1;
_1:;
            pTab = (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FpNext;
        }
        if (rc == SQLITE_OK) {
            if (__ccgo_fp_xOutput != 0) {
                if ((@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf > 0) {
                    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, int32>>(__ccgo_fp_xOutput)(tls, pOut, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf);
                }
            } else {
                if (ppOut != 0) {
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppOut))).Value = (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf;
                    if (pnOut != 0) {
                        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pnOut))).Value = (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf;
                    }
                    (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf = (uintptr)0;
                }
            }
        }
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _sessionDeleteTable(ж<libc.TLS> tls, uintptr pSession, uintptr pList) {
    int32 i = default;
    uintptr p = default;
    uintptr pNext = default;
    uintptr pNextChange = default;
    uintptr pTab = default;
    _ = i;
    _ = p;
    _ = pNext;
    _ = pNextChange;
    _ = pTab;
    pTab = pList;
    for (;;) {
        if (!(pTab != 0)) {
            break;
        }
        pNext = (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FpNext;
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnChange)) {
                break;
            }
            p = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FapChange + (uintptr)i * 8));
            for (;;) {
                if (!(p != 0)) {
                    break;
                }
                pNextChange = (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)p)).Value.FpNext;
                _sessionFree(tls, pSession, p);
                goto _3;
_3:;
                p = pNextChange;
            }
            goto _2;
_2:;
            i = i + 1;
        }
        Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FpDfltStmt);
        _sessionFree(tls, pSession, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FazCol);
        _sessionFree(tls, pSession, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FapChange);
        _sessionFree(tls, pSession, pTab);
        goto _1;
_1:;
        pTab = pNext;
    }
}

internal static void _sessionFree(ж<libc.TLS> tls, uintptr pSession, uintptr pFree) {
    uintptr v1 = default;
    _ = v1;
    if (pSession != 0) {
        v1 = pSession + 56;
        (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)v1)).Value = (Ti64)((uint64)(~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)v1)) - Xsqlite3_msize(tls, pFree));
    }
    Xsqlite3_free(tls, pFree);
}

internal static int32 /*r*/ _sessionInputBuffer(ж<libc.TLS> tls, uintptr pIn, int32 nByte) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        int32 _ᴛ1 = default;
        _ = rc;
        rc = SQLITE_OK;
        if ((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FxInput != 0) {
            while (!((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FbEof != 0) && (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiNext + nByte >= (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FnData && rc == SQLITE_OK) {
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = _sessions_strm_chunk_size;
                if ((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FbNoDiscard == 0) {
                    _sessionDiscardData(tls, pIn);
                }
                if (SQLITE_OK == _sessionBufferGrow(tls, (@unsafe.Pointer)(pIn + 32), (int64)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))), @unsafe.Pointer.FromPinnedBox(Ꮡrc))) {
                    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, uintptr, int32>>((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FxInput)(tls, (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FpIn, (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.Fbuf.FaBuf + (uintptr)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.Fbuf.FnBuf, bp);
                    if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) == 0) {
                        (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FbEof = (int32)1;
                    } else {
                        (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.Fbuf.FnBuf += ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp));
                    }
                }
                (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FaData = (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.Fbuf.FaBuf;
                (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FnData = (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.Fbuf.FnBuf;
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _sessionUpdateFree(ж<libc.TLS> tls, @unsafe.Pointer p) {
    uintptr pNext = default;
    uintptr pUp = default;
    _ = pNext;
    _ = pUp;
    pUp = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64));
    for (;;) {
        if (!(pUp != 0)) {
            break;
        }
        pNext = (@unsafe.Pointer.To<TSessionUpdate>((@unsafe.Pointer)pUp)).Value.FpNext;
        Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TSessionUpdate>((@unsafe.Pointer)pUp)).Value.FpStmt);
        Xsqlite3_free(tls, pUp);
        goto _1;
_1:;
        pUp = pNext;
    }
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = (uintptr)0;
    Xsqlite3_free(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56)));
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = (uintptr)0;
}

internal static void _setAllPagerFlags(ж<libc.TLS> tls, uintptr db) {
    int32 n = default;
    int32 v1 = default;
    uintptr pDb = default;
    _ = n;
    _ = pDb;
    _ = v1;
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FautoCommit != 0) {
        pDb = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb;
        n = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb;
        for (;;) {
            v1 = n;
            n = n - 1;
            if (!(v1 > 0)) {
                break;
            }
            if ((@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pDb)).Value.FpBt != 0) {
                _sqlite3BtreeSetPagerFlags(tls, (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pDb)).Value.FpBt, (uint32)((uint64)(@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pDb)).Value.Fsafety_level | (Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)PAGER_FLAGS_MASK)));
            }
            pDb += 32;
        }
    }
}

internal static void _setPageReferenced(ref libc.TLS tls, @unsafe.Pointer pCheck, TPgno iPg) {
    uintptr v1 = default;
    _ = v1;
    v1 = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pCheck, 16)) + (uintptr)(iPg / (uint32)8);
    (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(1).Lsh((uint64)((TPgno)(iPg & libc.Uint32FromInt32(0x07)))));
}

internal static void _sqlite3AuthContextPop(ref libc.TLS tls, @unsafe.Pointer pContext) {
    if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pContext, 8)) != 0) {
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pContext, 8))))).Value.FzAuthContext = ~@unsafe.Pointer.To<uintptr>(pContext);
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pContext, 8))).Value = (uintptr)0;
    }
}

internal static void _sqlite3AuthContextPush(ref libc.TLS tls, uintptr pParse, @unsafe.Pointer pContext, uintptr zContext) {
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pContext, 8))).Value = pParse;
    (@unsafe.Pointer.To<uintptr>(pContext)).Value = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FzAuthContext;
    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FzAuthContext = zContext;
}

internal static void _sqlite3BeginWriteOperation(ж<libc.TLS> tls, uintptr pParse, int32 setStatement, int32 iDb) {
    uintptr pToplevel = default;
    uintptr v1 = default;
    _ = pToplevel;
    _ = v1;
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel != 0) {
        v1 = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel;
    } else {
        v1 = pParse;
    }
    pToplevel = v1;
    _sqlite3CodeVerifySchemaAtToplevel(tls, pToplevel, iDb);
    (@unsafe.Pointer.To<TyDbMask>(__ccgo_ptr(pToplevel + 120))).Value |= libc.Uint32FromInt32(1).Lsh((int64)(iDb));
    v1 = pToplevel + 32;
    (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) | setStatement);
}

internal static void _sqlite3BitvecDestroy(ж<libc.TLS> tls, uintptr p) {
    uint32 i = default;
    _ = i;
    if (p == (uintptr)0) {
        return;
    }
    if ((@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiDivisor != 0) {
        i = (uint32)0;
        for (;;) {
            if (!(i < (uint32)((libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(8)))) {
                break;
            }
            _sqlite3BitvecDestroy(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 16 + (uintptr)i * 8)));
            goto _1;
_1:;
            i = i + 1;
        }
    }
    Xsqlite3_free(tls, p);
}

internal static int32 /*r*/ _sqlite3Checkpoint(ж<libc.TLS> tls, uintptr db, int32 iDb, int32 eMode, uintptr pnLog, uintptr pnCkpt) {
    int32 bBusy = default;
    int32 i = default;
    int32 rc = default;
    int32 v2 = default;
    _ = bBusy;
    _ = i;
    _ = rc;
    _ = v2;
    rc = SQLITE_OK;
    bBusy = 0;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb && rc == SQLITE_OK)) {
            break;
        }
        if (i == iDb || iDb == libc.Int32FromInt32(SQLITE_MAX_ATTACHED) + libc.Int32FromInt32(2)) {
            rc = _sqlite3BtreeCheckpoint(tls, (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)i * 32))).Value.FpBt, eMode, pnLog, pnCkpt);
            pnLog = (uintptr)0;
            pnCkpt = (uintptr)0;
            if (rc == (int32)SQLITE_BUSY) {
                bBusy = (int32)1;
                rc = SQLITE_OK;
            }
        }
        goto _1;
_1:;
        i = i + 1;
    }
    if (rc == SQLITE_OK && bBusy != 0) {
        v2 = (int32)SQLITE_BUSY;
    } else {
        v2 = rc;
    }
    return v2;
}

internal static void _sqlite3CloseExtensions(ж<libc.TLS> tls, uintptr db) {
    int32 i = default;
    _ = i;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnExtension)) {
            break;
        }
        _sqlite3OsDlClose(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpVfs, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaExtension + (uintptr)i * 8)));
        goto _1;
_1:;
        i = i + 1;
    }
    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaExtension);
}

internal static void _sqlite3CodeVerifyNamedSchema(ж<libc.TLS> tls, uintptr pParse, uintptr zDb) {
    uintptr db = default;
    uintptr pDb = default;
    int32 i = default;
    _ = db;
    _ = i;
    _ = pDb;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
            break;
        }
        pDb = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)i * 32;
        if ((@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pDb)).Value.FpBt != 0 && (!(zDb != 0) || 0 == _sqlite3StrICmp(ref (tls).DerefOrNull(), zDb, (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pDb)).Value.FzDbSName))) {
            _sqlite3CodeVerifySchema(tls, pParse, i);
        }
        goto _1;
_1:;
        i = i + 1;
    }
}

internal static void _sqlite3CodeVerifySchemaAtToplevel(ж<libc.TLS> tls, uintptr pToplevel, int32 iDb) {
    if (libc.BoolInt32((TyDbMask)((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pToplevel)).Value.FcookieMask & (libc.Uint32FromInt32(1).Lsh((int64)(iDb)))) != (uint32)0) == 0) {
        (@unsafe.Pointer.To<TyDbMask>(__ccgo_ptr(pToplevel + 124))).Value |= libc.Uint32FromInt32(1).Lsh((int64)(iDb));
        if (libc.Bool(!(libc.Int32FromInt32(OMIT_TEMPDB) != 0)) && iDb == (int32)1) {
            _sqlite3OpenTempDatabase(tls, (@unsafe.Pointer)pToplevel);
        }
    }
}

internal static uintptr /*r*/ _sqlite3CteNew(ж<libc.TLS> tls, uintptr pParse, uintptr pName, uintptr pArglist, uintptr pQuery, Tu8 eM10d) {
    uintptr db = default;
    uintptr pNew = default;
    _ = db;
    _ = pNew;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    pNew = _sqlite3DbMallocZero(tls, db, (uint64)48);
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
        _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, pArglist);
        _sqlite3SelectDelete(tls, (@unsafe.Pointer)db, pQuery);
    } else {
        (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pNew)).Value.FpSelect = pQuery;
        (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pNew)).Value.FpCols = pArglist;
        (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pNew)).Value.FzName = _sqlite3NameFromToken(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (@unsafe.Pointer)pName);
        (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pNew)).Value.FeM10d = eM10d;
    }
    return pNew;
}

internal static void _sqlite3DbFreeNN(ж<libc.TLS> tls, @unsafe.Pointer db, uintptr p) {
    uintptr pBuf = default;
    uintptr pBuf1 = default;
    _ = pBuf;
    _ = pBuf1;
    if (db != nil) {
        if ((uint64)p < (uint64)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 528)))) {
            if ((uint64)p >= (uint64)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 512)))) {
                pBuf = p;
                (@unsafe.Pointer.To<TLookasideSlot>((@unsafe.Pointer)pBuf)).Value.FpNext = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 504));
                (@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 504))).Value = pBuf;
                return;
            }
            if ((uint64)p >= (uint64)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 520)))) {
                pBuf1 = p;
                (@unsafe.Pointer.To<TLookasideSlot>((@unsafe.Pointer)pBuf1)).Value.FpNext = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 488));
                (@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 488))).Value = pBuf1;
                return;
            }
        }
        if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 808)) != 0) {
            _measureAllocationSize(tls, db, p);
            return;
        }
    }
    Xsqlite3_free(tls, p);
}

internal static uintptr /*r*/ _sqlite3DbMallocRawNN(ж<libc.TLS> tls, uintptr db, Tu64 n) {
    uintptr pBuf = default;
    uintptr v1 = default;
    _ = pBuf;
    _ = v1;
    if (n > (uint64)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.Fsz) {
        if (!((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FbDisable != 0)) {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 448 + 16 + 1 * 4))).Value = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 448 + 16 + 1 * 4)) + 1;
        } else {
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                return (uintptr)0;
            }
        }
        return _dbMallocRawFinish(tls, db, n);
    }
    if (n <= (uint64)LOOKASIDE_SMALL) {
        v1 = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpSmallFree;
        pBuf = v1;
        if (v1 != (uintptr)0) {
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpSmallFree = (@unsafe.Pointer.To<TLookasideSlot>((@unsafe.Pointer)pBuf)).Value.FpNext;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 448 + 16))).Value = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 448 + 16)) + 1;
            return pBuf;
        } else {
            v1 = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpSmallInit;
            pBuf = v1;
            if (v1 != (uintptr)0) {
                (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpSmallInit = (@unsafe.Pointer.To<TLookasideSlot>((@unsafe.Pointer)pBuf)).Value.FpNext;
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 448 + 16))).Value = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 448 + 16)) + 1;
                return pBuf;
            }
        }
    }
    v1 = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpFree;
    pBuf = v1;
    if (v1 != (uintptr)0) {
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpFree = (@unsafe.Pointer.To<TLookasideSlot>((@unsafe.Pointer)pBuf)).Value.FpNext;
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 448 + 16))).Value = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 448 + 16)) + 1;
        return pBuf;
    } else {
        v1 = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpInit;
        pBuf = v1;
        if (v1 != (uintptr)0) {
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpInit = (@unsafe.Pointer.To<TLookasideSlot>((@unsafe.Pointer)pBuf)).Value.FpNext;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 448 + 16))).Value = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 448 + 16)) + 1;
            return pBuf;
        } else {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 448 + 16 + 2 * 4))).Value = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 448 + 16 + 2 * 4)) + 1;
        }
    }
    return _dbMallocRawFinish(tls, db, n);
}

internal static void _sqlite3DbNNFreeNN(ж<libc.TLS> tls, @unsafe.Pointer db, uintptr p) {
    uintptr pBuf = default;
    uintptr pBuf1 = default;
    _ = pBuf;
    _ = pBuf1;
    if ((uint64)p < (uint64)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 528)))) {
        if ((uint64)p >= (uint64)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 512)))) {
            pBuf = p;
            (@unsafe.Pointer.To<TLookasideSlot>((@unsafe.Pointer)pBuf)).Value.FpNext = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 504));
            (@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 504))).Value = pBuf;
            return;
        }
        if ((uint64)p >= (uint64)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 520)))) {
            pBuf1 = p;
            (@unsafe.Pointer.To<TLookasideSlot>((@unsafe.Pointer)pBuf1)).Value.FpNext = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 488));
            (@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 488))).Value = pBuf1;
            return;
        }
    }
    if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 808)) != 0) {
        _measureAllocationSize(tls, db, p);
        return;
    }
    Xsqlite3_free(tls, p);
}

internal static uintptr /*r*/ _sqlite3DbNameToBtree(ж<libc.TLS> tls, uintptr db, uintptr zDbName) {
    int32 iDb = default;
    int32 v1 = default;
    uintptr v2 = default;
    _ = iDb;
    _ = v1;
    _ = v2;
    if (zDbName != 0) {
        v1 = _sqlite3FindDbName(tls, db, zDbName);
    } else {
        v1 = 0;
    }
    iDb = v1;
    if (iDb < 0) {
        v2 = (uintptr)0;
    } else {
        v2 = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpBt;
    }
    return v2;
}

internal static uintptr /*r*/ _sqlite3DbRealloc(ж<libc.TLS> tls, uintptr db, uintptr p, Tu64 n) {
    if (p == (uintptr)0) {
        return _sqlite3DbMallocRawNN(tls, db, n);
    }
    if ((uint64)p < (uint64)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpEnd) {
        if ((uint64)p >= (uint64)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpMiddle) {
            if (n <= (uint64)LOOKASIDE_SMALL) {
                return p;
            }
        } else {
            if ((uint64)p >= (uint64)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpStart) {
                if (n <= (uint64)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FszTrue) {
                    return p;
                }
            }
        }
    }
    return _dbReallocFinish(tls, db, p, n);
}

internal static void _sqlite3DeleteIndexSamples(ж<libc.TLS> tls, @unsafe.Pointer db, uintptr pIdx) {
    int32 j = default;
    uintptr p = default;
    _ = j;
    _ = p;
    if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSample != 0) {
        j = 0;
        for (;;) {
            if (!(j < (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnSample)) {
                break;
            }
            p = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSample + (uintptr)j * 40;
            _sqlite3DbFree__ccgo_gp(tls, db, (@unsafe.Pointer.To<TIndexSample>((@unsafe.Pointer)p)).Value.Fp);
            goto _1;
_1:;
            j = j + 1;
        }
        _sqlite3DbFree__ccgo_gp(tls, db, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSample);
    }
    if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 808)) == (uintptr)0) {
        (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnSample = 0;
        (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSample = (uintptr)0;
    }
}

internal static void _sqlite3DeleteReturning(ж<libc.TLS> tls, uintptr db, uintptr pArg) {
    uintptr pHash = default;
    uintptr pRet = default;
    _ = pHash;
    _ = pRet;
    pRet = pArg;
    pHash = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + 1 * 32))).Value.FpSchema + 56;
    _sqlite3HashInsert(tls, pHash, pRet + 188, (uintptr)0);
    _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FpReturnEL);
    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, pRet);
}

internal static void _sqlite3DeleteTable(ж<libc.TLS> tls, @unsafe.Pointer db, uintptr pTable) {
    Tu32 v1 = default;
    uintptr v2 = default;
    bool v3 = default;
    _ = v1;
    _ = v2;
    _ = v3;
    if (!(pTable != 0)) {
        return;
    }
    v3 = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 808)) == (uintptr)0; if (v3) {
        v2 = pTable + 44;
        (@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v2) - 1;
        v1 = ~@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v2);
    }
    if (v3 && v1 > (uint32)0) {
        return;
    }
    _deleteTable(tls, db, pTable);
}

internal static void _sqlite3ErrorMsg(ж<libc.TLS> tls, @unsafe.Pointer pParse, uintptr zFormat, uintptr va) {
    Tva_list ap = default;
    uintptr db = default;
    uintptr zMsg = default;
    _ = ap;
    _ = db;
    _ = zMsg;
    db = ~@unsafe.Pointer.To<uintptr>(pParse);
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FerrByteOffset = -(int32)2;
    ap = va;
    zMsg = _sqlite3VMPrintf(tls, db, zFormat, ap);
    _ = ap;
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FerrByteOffset < -(int32)1) {
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FerrByteOffset = -(int32)1;
    }
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FsuppressErr != 0) {
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, zMsg);
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            (@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 52))).Value = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 52)) + 1;
            (@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 24))).Value = (int32)SQLITE_NOMEM;
        }
    } else {
        (@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 52))).Value = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 52)) + 1;
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 8)));
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 8))).Value = zMsg;
        (@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 24))).Value = (int32)SQLITE_ERROR;
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 424))).Value = (uintptr)0;
    }
}

internal static void _sqlite3ExprAnalyzeAggList(ж<libc.TLS> tls, uintptr pNC, uintptr pList) {
    int32 i = default;
    uintptr pItem = default;
    _ = i;
    _ = pItem;
    if (pList != 0) {
        pItem = pList + 8;
        i = libc.Int32FromInt32(0);
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr)) {
                break;
            }
            _sqlite3ExprAnalyzeAggregates(tls, pNC, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FpExpr);
            goto _1;
_1:;
            i = i + 1;
            pItem += 32;
        }
    }
}

internal static void _sqlite3ExprAnalyzeAggregates(ж<libc.TLS> tls, uintptr pNC, uintptr pExpr) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_analyzeAggregate);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_sqlite3WalkerDepthIncrease);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback2 = __ccgo_fp(_sqlite3WalkerDepthDecrease);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FwalkerDepth = 0;
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0))).Value = pNC;
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FpParse = (uintptr)0;
        _sqlite3WalkExpr(tls, bp, pExpr);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sqlite3ExprCheckIN(ж<libc.TLS> tls, uintptr pParse, uintptr pIn) {
    int32 nVector = default;
    _ = nVector;
    nVector = _sqlite3ExprVectorSize(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pIn)).Value.FpLeft);
    if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pIn)).Value.Fflags & (uint32)EP_xIsSelect) != (uint32)0 && !((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FmallocFailed != 0)) {
        if (nVector != (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pIn + 32))))).Value.FpEList)).Value.FnExpr) {
            _sqlite3SubselectError(tls, pParse, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pIn + 32))))).Value.FpEList)).Value.FnExpr, nVector);
            return (int32)1;
        }
    } else {
        if (nVector != (int32)1) {
            _sqlite3VectorErrorMsg(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pIn)).Value.FpLeft);
            return (int32)1;
        }
    }
    return 0;
}

internal static void _sqlite3ExprCodeLoadIndexColumn(ж<libc.TLS> tls, uintptr pParse, uintptr pIdx, int32 iTabCur, int32 iIdxCol, int32 regOut) {
    Ti16 iTabCol = default;
    _ = iTabCol;
    iTabCol = ~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)iIdxCol * 2));
    if ((int32)iTabCol == -(int32)2) {
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiSelfTab = iTabCur + (int32)1;
        _sqlite3ExprCodeCopy(tls, pParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaColExpr + 8 + (uintptr)iIdxCol * 32))).Value.FpExpr, regOut);
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiSelfTab = 0;
    } else {
        _sqlite3ExprCodeGetColumnOfTable(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable, iTabCur, (int32)iTabCol, regOut);
    }
}

internal static uintptr /*r*/ _sqlite3ExprListAppend(ж<libc.TLS> tls, uintptr pParse, uintptr pList, uintptr pExpr) {
    uintptr pItem = default;
    uintptr v2 = default;
    int32 v1 = default;
    _ = pItem;
    _ = v1;
    _ = v2;
    if (pList == (uintptr)0) {
        return _sqlite3ExprListAppendNew(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pExpr);
    }
    if ((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnAlloc < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr + (int32)1) {
        return _sqlite3ExprListAppendGrow(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pList, pExpr);
    }
    v2 = pList;
    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
    pItem = pList + 8 + (uintptr)v1 * 32;
    (@unsafe.Pointer.To<TExprList_item>(__ccgo_ptr(pItem))).Value = _zeroItem;
    (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FpExpr = pExpr;
    return pList;
}

internal static Tu32 /*r*/ _sqlite3ExprListFlags(ref libc.TLS tls, uintptr pList) {
    int32 i = default;
    Tu32 m = default;
    uintptr pExpr = default;
    _ = i;
    _ = m;
    _ = pExpr;
    m = (uint32)0;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr)) {
            break;
        }
        pExpr = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32))).Value.FpExpr;
        m = (Tu32)(m | (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags);
        goto _1;
_1:;
        i = i + 1;
    }
    return m;
}

internal static int32 /*r*/ _sqlite3ExprListIsConstant(ж<libc.TLS> tls, uintptr pParse, uintptr pList, int32 bNoIs) {
    int32 iInit = default;
    int32 ii = default;
    int32 v1 = default;
    _ = iInit;
    _ = ii;
    _ = v1;
    if (bNoIs != 0) {
        v1 = (int32)6;
    } else {
        v1 = (int32)1;
    }
    iInit = v1;
    ii = 0;
    for (;;) {
        if (!(ii < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr)) {
            break;
        }
        if (0 == _exprIsConst(tls, pParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)ii * 32))).Value.FpExpr, iInit)) {
            return 0;
        }
        goto _2;
_2:;
        ii = ii + 1;
    }
    return (int32)1;
}

internal static uintptr /*r*/ _sqlite3FindIndex(ref libc.TLS tls, uintptr db, uintptr zName, uintptr zDb) {
    int32 i = default;
    int32 j = default;
    int32 v2 = default;
    uintptr p = default;
    uintptr pSchema = default;
    _ = i;
    _ = j;
    _ = p;
    _ = pSchema;
    _ = v2;
    p = (uintptr)0;
    i = OMIT_TEMPDB;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
            break;
        }
        if (i < (int32)2) {
            v2 = i ^ (int32)1;
        } else {
            v2 = i;
        }
        j = v2;
        pSchema = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)j * 32))).Value.FpSchema;
        if (zDb != 0 && _sqlite3DbIsNamed(ref tls, db, j, zDb) == 0) {
            goto _1;
        }
        p = _sqlite3HashFind(ref tls, (@unsafe.Pointer)(pSchema + 32), zName);
        if (p != 0) {
            break;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return p;
}

internal static int32 /*r*/ _sqlite3FirstAvailableRegister(ref libc.TLS tls, uintptr pParse, int32 iMin) {
    int32 i = default;
    uintptr pList = default;
    _ = i;
    _ = pList;
    pList = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpConstExpr;
    if (pList != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr)) {
                break;
            }
            if (~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32 + 24)) >= iMin) {
                iMin = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32 + 24)) + (int32)1;
            }
            goto _1;
_1:;
            i = i + 1;
        }
    }
    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTempReg = (uint8)0;
    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnRangeReg = 0;
    return iMin;
}

internal static int32 /*r*/ _sqlite3FixExpr(ж<libc.TLS> tls, uintptr pFix, uintptr pExpr) {
    return _sqlite3WalkExpr(tls, pFix + 8, pExpr);
}

internal static void _sqlite3FixInit(ref libc.TLS tls, uintptr pFix, uintptr pParse, int32 iDb, uintptr zType, uintptr pName) {
    uintptr db = default;
    _ = db;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.FpParse = pParse;
    (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.FzDb = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName;
    (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.FpSchema = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpSchema;
    (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.FzType = zType;
    (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.FpName = pName;
    (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.FbTemp = libc.BoolUint8(iDb == libc.Int32FromInt32(1));
    (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.Fw.FpParse = pParse;
    (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.Fw.FxExprCallback = __ccgo_fp(_fixExprCb);
    (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.Fw.FxSelectCallback = __ccgo_fp(_fixSelectCb);
    (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.Fw.FxSelectCallback2 = __ccgo_fp(_sqlite3WalkWinDefnDummyCallback);
    (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.Fw.FwalkerDepth = 0;
    (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.Fw.FeCode = (uint16)0;
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFix + 8 + 40))).Value = pFix;
}

internal static int32 /*r*/ _sqlite3FixSelect(ж<libc.TLS> tls, uintptr pFix, uintptr pSelect) {
    return _sqlite3WalkSelect(tls, pFix + 8, pSelect);
}

internal static int32 /*r*/ _sqlite3FixTriggerStep(ж<libc.TLS> tls, uintptr pFix, uintptr pStep) {
    uintptr pUp = default;
    _ = pUp;
    while (pStep != 0) {
        if (_sqlite3WalkSelect(tls, pFix + 8, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSelect) != 0 || _sqlite3WalkExpr(tls, pFix + 8, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpWhere) != 0 || _sqlite3WalkExprList(tls, pFix + 8, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpExprList) != 0 || _sqlite3FixSrcList(tls, pFix, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSrc) != 0) {
            return (int32)1;
        }
        pUp = (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpUpsert;
        for (;;) {
            if (!(pUp != 0)) {
                break;
            }
            if (_sqlite3WalkExprList(tls, pFix + 8, (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUp)).Value.FpUpsertTarget) != 0 || _sqlite3WalkExpr(tls, pFix + 8, (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUp)).Value.FpUpsertTargetWhere) != 0 || _sqlite3WalkExprList(tls, pFix + 8, (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUp)).Value.FpUpsertSet) != 0 || _sqlite3WalkExpr(tls, pFix + 8, (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUp)).Value.FpUpsertWhere) != 0) {
                return (int32)1;
            }
            goto _1;
_1:;
            pUp = (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUp)).Value.FpNextUpsert;
        }
        pStep = (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpNext;
    }
    return 0;
}

internal static void _sqlite3FkDelete(ж<libc.TLS> tls, @unsafe.Pointer db, uintptr pTab) {
    uintptr pFKey = default;
    uintptr pNext = default;
    uintptr z = default;
    uintptr v2 = default;
    _ = pFKey;
    _ = pNext;
    _ = z;
    _ = v2;
    pFKey = (@unsafe.Pointer.To<TTableᴛ1_Fu_Ftab>((@unsafe.Pointer)(pTab + 64))).Value.FpFKey;
    for (;;) {
        if (!(pFKey != 0)) {
            break;
        }
        if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 808)) == (uintptr)0) {
            if ((@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpPrevTo != 0) {
                (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)(@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpPrevTo)).Value.FpNextTo = (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpNextTo;
            } else {
                if ((@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpNextTo != 0) {
                    v2 = (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)(@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpNextTo)).Value.FzTo;
                } else {
                    v2 = (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FzTo;
                }
                z = v2;
                _sqlite3HashInsert(tls, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema + 80, z, (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpNextTo);
            }
            if ((@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpNextTo != 0) {
                (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)(@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpNextTo)).Value.FpPrevTo = (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpPrevTo;
            }
        }
        _fkTriggerDelete(tls, db, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pFKey + 48)));
        _fkTriggerDelete(tls, db, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pFKey + 48 + 1 * 8)));
        pNext = (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpNextFrom;
        _sqlite3DbFree__ccgo_gp(tls, db, pFKey);
        goto _1;
_1:;
        pFKey = pNext;
    }
}

internal static uintptr /*r*/ _sqlite3FkReferences(ref libc.TLS tls, uintptr pTab) {
    return _sqlite3HashFind(ref tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema + 80), (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName);
}

internal static void _sqlite3ForceNotReadOnly(ж<libc.TLS> tls, uintptr pParse) {
    int32 iReg = default;
    int32 v1 = default;
    uintptr v = default;
    uintptr v2 = default;
    _ = iReg;
    _ = v;
    _ = v1;
    _ = v2;
    v2 = pParse + 60;
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
    iReg = v1;
    v = _sqlite3GetVdbe(tls, pParse);
    if (v != 0) {
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_JournalMode, 0, iReg, -(int32)1);
        _sqlite3VdbeUsesBtree(ref (tls).DerefOrNull(), v, 0);
    }
}

internal static void _sqlite3Fts5BufferZero(ref libc.TLS tls, @unsafe.Pointer pBuf) {
    (@unsafe.Pointer.To<int32>(@unsafe.Add(pBuf, 8))).Value = 0;
}

internal static void _sqlite3Fts5ConfigFree(ж<libc.TLS> tls, uintptr pConfig) {
    int32 i = default;
    _ = i;
    if (pConfig != 0) {
        if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Ft.FpTok != 0) {
            if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Ft.FpApi1 != 0) {
                GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>((@unsafe.Pointer.To<Tfts5_tokenizer>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Ft.FpApi1)).Value.FxDelete)(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Ft.FpTok);
            } else {
                GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>((@unsafe.Pointer.To<Tfts5_tokenizer_v2>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Ft.FpApi2)).Value.FxDelete)(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Ft.FpTok);
            }
        }
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Ft.FazArg);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzDb);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzName);
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnCol)) {
                break;
            }
            Xsqlite3_free(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FazCol + (uintptr)i * 8)));
            goto _1;
_1:;
            i = i + 1;
        }
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FazCol);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FaPrefix);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzRank);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzRankArgs);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzContent);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzContentRowid);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzContentExprlist);
        Xsqlite3_free(tls, pConfig);
    }
}

internal static void _sqlite3Fts5ExprClearTokens(ref libc.TLS tls, uintptr pExpr) {
    int32 ii = default;
    uintptr pT = default;
    _ = ii;
    _ = pT;
    ii = 0;
    for (;;) {
        if (!(ii < (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FnPhrase)) {
            break;
        }
        pT = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FapExprPhrase + (uintptr)ii * 8)) + 32;
        for (;;) {
            if (!(pT != 0)) {
                break;
            }
            _sqlite3Fts5IndexIterClearTokendata(ref tls, (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pT)).Value.FpIter);
            goto _2;
_2:;
            pT = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pT)).Value.FpSynonym;
        }
        goto _1;
_1:;
        ii = ii + 1;
    }
}

internal static int32 /*r*/ _sqlite3Fts5ExprInstToken(ж<libc.TLS> tls, uintptr pExpr, Ti64 iRowid, int32 iPhrase, int32 iCol, int32 iOff, int32 iToken, uintptr ppOut, uintptr pnOut) {
    uintptr pPhrase = default;
    uintptr pTerm = default;
    int32 rc = default;
    _ = pPhrase;
    _ = pTerm;
    _ = rc;
    pPhrase = (uintptr)0;
    pTerm = (uintptr)0;
    rc = SQLITE_OK;
    if (iPhrase < 0 || iPhrase >= (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FnPhrase) {
        return (int32)SQLITE_RANGE;
    }
    pPhrase = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FapExprPhrase + (uintptr)iPhrase * 8));
    if (iToken < 0 || iToken >= (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm) {
        return (int32)SQLITE_RANGE;
    }
    pTerm = pPhrase + 32 + (uintptr)iToken * 40;
    if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FpConfig)).Value.FbTokendata != 0 || (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FbPrefix != 0) {
        rc = _sqlite3Fts5IterToken(tls, (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FpIter, (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FpTerm, (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FnQueryTerm, iRowid, iCol, iOff + iToken, ppOut, pnOut);
    } else {
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppOut))).Value = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FpTerm;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pnOut))).Value = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FnFullTerm;
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3Fts5ExprPhraseCollist(ж<libc.TLS> tls, uintptr pExpr, int32 iPhrase, @unsafe.Pointer ppCollist, @unsafe.Pointer pnCollist) {
    uintptr pBuf = default;
    uintptr pNode = default;
    uintptr pPhrase = default;
    uintptr pTerm = default;
    int32 rc = default;
    _ = pBuf;
    _ = pNode;
    _ = pPhrase;
    _ = pTerm;
    _ = rc;
    pPhrase = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FapExprPhrase + (uintptr)iPhrase * 8));
    pNode = (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FpNode;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbEof == 0 && (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FiRowid == (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FpRoot)).Value.FiRowid && (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.Fposlist.Fn > 0) {
        pTerm = pPhrase + 32;
        if ((@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FpSynonym != 0) {
            pBuf = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FpSynonym + 1 * 40;
            rc = _fts5ExprSynonymList(tls, pTerm, (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FiRowid, (@unsafe.Pointer)pBuf, ppCollist, pnCollist);
        } else {
            (@unsafe.Pointer.To<uintptr>(ppCollist)).Value = (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pPhrase + 32))).Value.FpIter)).Value.FpData;
            (@unsafe.Pointer.To<int32>(pnCollist)).Value = (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pPhrase + 32))).Value.FpIter)).Value.FnData;
        }
    } else {
        (@unsafe.Pointer.To<uintptr>(ppCollist)).Value = (uintptr)0;
        (@unsafe.Pointer.To<int32>(pnCollist)).Value = 0;
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3Fts5ExprPhraseSize(ref libc.TLS tls, uintptr pExpr, int32 iPhrase) {
    if (iPhrase < 0 || iPhrase >= (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FnPhrase) {
        return 0;
    }
    return (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FapExprPhrase + (uintptr)iPhrase * 8))))).Value.FnTerm;
}

internal static int32 /*r*/ _sqlite3Fts5ExprPopulatePoslists(ж<libc.TLS> tls, uintptr pConfig, uintptr pExpr, uintptr aPopulator, int32 iCol, uintptr z, int32 n) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 i = default;
        uintptr pColset = default;
        uintptr pNode = default;
        TFts5ExprCtx _ᴛ1 = default;
        _ = i;
        _ = pColset;
        _ = pNode;
        (@unsafe.Pointer.To<TFts5ExprCtx>(__ccgo_ptr(bp))).Value.FpExpr = pExpr;
        (@unsafe.Pointer.To<TFts5ExprCtx>(__ccgo_ptr(bp))).Value.FaPopulator = aPopulator;
        (@unsafe.Pointer.To<TFts5ExprCtx>(__ccgo_ptr(bp))).Value.FiOff = ((int64)iCol).Lsh((int64)(libc.Int32FromInt32(32))) - (int64)1;
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FnPhrase)) {
                break;
            }
            pNode = (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FapExprPhrase + (uintptr)i * 8))))).Value.FpNode;
            pColset = (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FpNear)).Value.FpColset;
            if (pColset != 0 && 0 == _fts5ExprColsetTest(ref (tls).DerefOrNull(), pColset, iCol) || (@unsafe.Pointer.To<TFts5PoslistPopulator>(__ccgo_ptr(aPopulator + (uintptr)i * 16))).Value.FbMiss != 0) {
                (@unsafe.Pointer.To<TFts5PoslistPopulator>(__ccgo_ptr(aPopulator + (uintptr)i * 16))).Value.FbOk = 0;
            } else {
                (@unsafe.Pointer.To<TFts5PoslistPopulator>(__ccgo_ptr(aPopulator + (uintptr)i * 16))).Value.FbOk = (int32)1;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        r = _sqlite3Fts5Tokenize(tls, pConfig, (int32)FTS5_TOKENIZE_DOCUMENT, z, n, bp, __ccgo_fp(_fts5ExprPopulatePoslistsCb));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3Fts5ExprPoslist(ref libc.TLS tls, uintptr pExpr, int32 iPhrase, @unsafe.Pointer pa) {
    int32 nRet = default;
    uintptr pNode = default;
    uintptr pPhrase = default;
    _ = nRet;
    _ = pNode;
    _ = pPhrase;
    pPhrase = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FapExprPhrase + (uintptr)iPhrase * 8));
    pNode = (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FpNode;
    if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbEof == 0 && (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FiRowid == (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FpRoot)).Value.FiRowid) {
        (@unsafe.Pointer.To<uintptr>(pa)).Value = (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.Fposlist.Fp;
        nRet = (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.Fposlist.Fn;
    } else {
        (@unsafe.Pointer.To<uintptr>(pa)).Value = (uintptr)0;
        nRet = 0;
    }
    return nRet;
}

internal static int32 /*r*/ _sqlite3Fts5ExprQueryToken(ref libc.TLS tls, uintptr pExpr, int32 iPhrase, int32 iToken, uintptr ppOut, uintptr pnOut) {
    uintptr pPhrase = default;
    _ = pPhrase;
    pPhrase = (uintptr)0;
    if (iPhrase < 0 || iPhrase >= (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FnPhrase) {
        return (int32)SQLITE_RANGE;
    }
    pPhrase = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FapExprPhrase + (uintptr)iPhrase * 8));
    if (iToken < 0 || iToken >= (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm) {
        return (int32)SQLITE_RANGE;
    }
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppOut))).Value = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pPhrase + 32 + (uintptr)iToken * 40))).Value.FpTerm;
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pnOut))).Value = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pPhrase + 32 + (uintptr)iToken * 40))).Value.FnFullTerm;
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3Fts5HashScanInit(ж<libc.TLS> tls, uintptr p, uintptr pTerm, int32 nTerm) {
    return _fts5HashEntrySort(tls, p, pTerm, nTerm, p + 24);
}

internal static int32 /*r*/ _sqlite3Fts5IndexBeginWrite(ж<libc.TLS> tls, uintptr p, int32 bDelete, Ti64 iRowid) {
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpHash == (uintptr)0) {
        (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc = _sqlite3Fts5HashNew(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig, p + 24, p + 32);
    }
    if (iRowid < (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FiWriteRowid || iRowid == (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FiWriteRowid && (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FbDelete == 0 || (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FnPendingData > (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FnHashSize) {
        _fts5IndexFlush(tls, p);
    }
    (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FiWriteRowid = iRowid;
    (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FbDelete = bDelete;
    if (bDelete == 0) {
        (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FnPendingRow = (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FnPendingRow + 1;
    }
    return _fts5IndexReturn(ref (tls).DerefOrNull(), p);
}

internal static int32 /*r*/ _sqlite3Fts5IndexOptimize(ж<libc.TLS> tls, uintptr p) {
    int32 iLvl = default;
    ref var nRem = ref heap(new int32(), out var ᏑnRem);
    ref var pNew = ref heap(new uintptr(), out var ᏑpNew);
    uintptr pStruct = default;
    _ = iLvl;
    _ = nRem;
    _ = pNew;
    _ = pStruct;
    pNew = (uintptr)0;
    _fts5IndexFlush(tls, p);
    pStruct = _fts5StructureRead(tls, p);
    _fts5StructureInvalidate(tls, p);
    if (pStruct != 0) {
        pNew = _fts5IndexOptimizeStruct(tls, p, pStruct);
    }
    _fts5StructureRelease(tls, pStruct);
    if (pNew != 0) {
        iLvl = 0;
        for (;;) {
            if (!((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pNew + 32 + (uintptr)iLvl * 16))).Value.FnSeg == 0)) {
                break;
            }
            goto _1;
_1:;
            iLvl = iLvl + 1;
        }
        while ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pNew + 32 + (uintptr)iLvl * 16))).Value.FnSeg > 0) {
            nRem = (int32)FTS5_OPT_WORK_UNIT;
            _fts5IndexMergeLevel(tls, p, @unsafe.Pointer.FromBox(ᏑpNew), iLvl, @unsafe.Pointer.FromPinnedBox(ᏑnRem));
        }
        _fts5StructureWrite(tls, p, (@unsafe.Pointer)pNew);
        _fts5StructureRelease(tls, pNew);
    }
    return _fts5IndexReturn(ref (tls).DerefOrNull(), p);
}

internal static int32 /*r*/ _sqlite3Fts5IterToken(ж<libc.TLS> tls, uintptr pIndexIter, uintptr pToken, int32 nToken, Ti64 iRowid, int32 iCol, int32 iOff, uintptr ppOut, uintptr pnOut) {
    uintptr aMap = default;
    uintptr p = default;
    uintptr pIter = default;
    uintptr pMap = default;
    uintptr pT = default;
    int32 i1 = default;
    int32 i2 = default;
    int32 iTest = default;
    int32 rc = default;
    Ti64 iPos = default;
    _ = aMap;
    _ = i1;
    _ = i2;
    _ = iPos;
    _ = iTest;
    _ = p;
    _ = pIter;
    _ = pMap;
    _ = pT;
    _ = rc;
    pIter = pIndexIter;
    pT = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FpTokenDataIter;
    iPos = ((int64)iCol).Lsh((int64)(libc.Int32FromInt32(32))) + (int64)iOff;
    aMap = (uintptr)0;
    i1 = 0;
    i2 = 0;
    iTest = 0;
    if (pT == (uintptr)0) {
        rc = _fts5SetupPrefixIterTokendata(tls, pIter, pToken, nToken);
        if (rc != SQLITE_OK) {
            return rc;
        }
        pT = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FpTokenDataIter;
    }
    i2 = (int32)(@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pT)).Value.FnMap;
    aMap = (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pT)).Value.FaMap;
    while (i2 > i1) {
        iTest = (i1 + i2) / (int32)2;
        if ((@unsafe.Pointer.To<TFts5TokenDataMap>(__ccgo_ptr(aMap + (uintptr)iTest * 24))).Value.FiRowid < iRowid) {
            i1 = iTest + (int32)1;
        } else {
            if ((@unsafe.Pointer.To<TFts5TokenDataMap>(__ccgo_ptr(aMap + (uintptr)iTest * 24))).Value.FiRowid > iRowid) {
                i2 = iTest;
            } else {
                if ((@unsafe.Pointer.To<TFts5TokenDataMap>(__ccgo_ptr(aMap + (uintptr)iTest * 24))).Value.FiPos < iPos) {
                    if ((@unsafe.Pointer.To<TFts5TokenDataMap>(__ccgo_ptr(aMap + (uintptr)iTest * 24))).Value.FiPos < 0) {
                        break;
                    }
                    i1 = iTest + (int32)1;
                } else {
                    if ((@unsafe.Pointer.To<TFts5TokenDataMap>(__ccgo_ptr(aMap + (uintptr)iTest * 24))).Value.FiPos > iPos) {
                        i2 = iTest;
                    } else {
                        break;
                    }
                }
            }
        }
    }
    if (i2 > i1) {
        if ((@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FnSeg == 0) {
            pMap = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pT + 72 + (uintptr)(@unsafe.Pointer.To<TFts5TokenDataMap>(__ccgo_ptr(aMap + (uintptr)iTest * 24))).Value.FiIter * 8));
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppOut))).Value = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)(pMap + 104))).Value.Fterm.Fp + (uintptr)1;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pnOut))).Value = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)(pMap + 104))).Value.Fterm.Fn - (int32)1;
        } else {
            p = aMap + (uintptr)iTest * 24;
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppOut))).Value = (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pT)).Value.Fterms.Fp + (uintptr)(@unsafe.Pointer.To<TFts5TokenDataMap>((@unsafe.Pointer)p)).Value.FiIter;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pnOut))).Value = (@unsafe.Pointer.To<TFts5TokenDataMap>(__ccgo_ptr(aMap + (uintptr)iTest * 24))).Value.FnByte;
        }
    }
    return SQLITE_OK;
}

internal static void _sqlite3Fts5ParseError(ж<libc.TLS> tls, @unsafe.Pointer pParse, uintptr zFmt, uintptr va) {
    Tva_list ap = default;
    _ = ap;
    ap = va;
    if (~@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 16)) == SQLITE_OK) {
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 8))).Value = Xsqlite3_vmprintf(tls, zFmt, ap);
        (@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 16))).Value = (int32)SQLITE_ERROR;
    }
    _ = ap;
}

internal static void _sqlite3Fts5ParseNearsetFree(ж<libc.TLS> tls, uintptr pNear) {
    int32 i = default;
    _ = i;
    if (pNear != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FnPhrase)) {
                break;
            }
            _fts5ExprPhraseFree(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNear + 24 + (uintptr)i * 8)));
            goto _1;
_1:;
            i = i + 1;
        }
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FpColset);
        Xsqlite3_free(tls, pNear);
    }
}

internal static void _sqlite3Fts5ParseNodeFree(ж<libc.TLS> tls, uintptr p) {
    int32 i = default;
    _ = i;
    if (p != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p)).Value.FnChild)) {
                break;
            }
            _sqlite3Fts5ParseNodeFree(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 48 + (uintptr)i * 8)));
            goto _1;
_1:;
            i = i + 1;
        }
        _sqlite3Fts5ParseNearsetFree(tls, (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p)).Value.FpNear);
        Xsqlite3_free(tls, p);
    }
}

internal static void _sqlite3Fts5ParseSetCaret(ref libc.TLS tls, uintptr pPhrase) {
    if (pPhrase != 0 && (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm != 0) {
        (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pPhrase + 32))).Value.FbFirst = (uint8)1;
    }
}

internal static uintptr /*r*/ _sqlite3Fts5ParserAlloc(ж<libc.TLS> tls, uintptr __ccgo_fp_mallocProc) {
    uintptr fts5yypParser = default;
    _ = fts5yypParser;
    fts5yypParser = GoFuncCookie.Func<Func<ж<libc.TLS>, Tu64, uintptr>>(__ccgo_fp_mallocProc)(tls, libc.Uint64FromInt64(2432));
    if (fts5yypParser != 0) {
        _sqlite3Fts5ParserInit(ref (tls).DerefOrNull(), fts5yypParser);
    }
    return fts5yypParser;
}

internal static void _sqlite3Fts5ParserInit(ref libc.TLS tls, uintptr fts5yypRawParser) {
    uintptr fts5yypParser = default;
    _ = fts5yypParser;
    fts5yypParser = fts5yypRawParser;
    (@unsafe.Pointer.To<Tfts5yyParser>((@unsafe.Pointer)fts5yypParser)).Value.Ffts5yystack = fts5yypParser + 32;
    (@unsafe.Pointer.To<Tfts5yyParser>((@unsafe.Pointer)fts5yypParser)).Value.Ffts5yystackEnd = (@unsafe.Pointer.To<Tfts5yyParser>((@unsafe.Pointer)fts5yypParser)).Value.Ffts5yystack + (uintptr)(libc.Int32FromInt32(fts5YYSTACKDEPTH) - libc.Int32FromInt32(1)) * 24;
    (@unsafe.Pointer.To<Tfts5yyParser>((@unsafe.Pointer)fts5yypParser)).Value.Ffts5yytos = (@unsafe.Pointer.To<Tfts5yyParser>((@unsafe.Pointer)fts5yypParser)).Value.Ffts5yystack;
    (@unsafe.Pointer.To<Tfts5yyStackEntry>(__ccgo_ptr((@unsafe.Pointer.To<Tfts5yyParser>((@unsafe.Pointer)fts5yypParser)).Value.Ffts5yystack))).Value.Fstateno = (uint8)0;
    (@unsafe.Pointer.To<Tfts5yyStackEntry>(__ccgo_ptr((@unsafe.Pointer.To<Tfts5yyParser>((@unsafe.Pointer)fts5yypParser)).Value.Ffts5yystack))).Value.Fmajor = (uint8)0;
}

internal static void _sqlite3Fts5SetLocale(ref libc.TLS tls, uintptr pConfig, uintptr zLocale, int32 nLocale) {
    uintptr pT = default;
    _ = pT;
    pT = pConfig + 128;
    (@unsafe.Pointer.To<TFts5TokenizerConfig>((@unsafe.Pointer)pT)).Value.FpLocale = zLocale;
    (@unsafe.Pointer.To<TFts5TokenizerConfig>((@unsafe.Pointer)pT)).Value.FnLocale = nLocale;
}

internal static int32 /*r*/ _sqlite3Fts5StorageStmt(ж<libc.TLS> tls, uintptr p, int32 eStmt, uintptr pp, uintptr pzErrMsg) {
    int32 rc = default;
    _ = rc;
    rc = _fts5StorageGetStmt(tls, p, eStmt, (@unsafe.Pointer)pp, pzErrMsg);
    if (rc == SQLITE_OK) {
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 48 + (uintptr)eStmt * 8))).Value = (uintptr)0;
    }
    return rc;
}

internal static void _sqlite3Fts5StorageStmtRelease(ж<libc.TLS> tls, uintptr p, int32 eStmt, uintptr pStmt) {
    if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 48 + (uintptr)eStmt * 8)) == (uintptr)0) {
        Xsqlite3_reset(tls, pStmt);
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 48 + (uintptr)eStmt * 8))).Value = pStmt;
    } else {
        Xsqlite3_finalize(tls, pStmt);
    }
}

internal static int32 /*r*/ _sqlite3Fts5TermsetNew(ж<libc.TLS> tls, @unsafe.Pointer pp) {
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    _ = rc;
    rc = SQLITE_OK;
    (@unsafe.Pointer.To<uintptr>(pp)).Value = _sqlite3Fts5MallocZero(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (int64)4096);
    return rc;
}

internal static uintptr /*r*/ _sqlite3FunctionSearch(ref libc.TLS tls, int32 h, uintptr zFunc) {
    uintptr p = default;
    _ = p;
    p = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((uintptr)Ꮡ_sqlite3BuiltinFunctions + (uintptr)h * 8));
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        if (_sqlite3StrICmp(ref tls, (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FzName, zFunc) == 0) {
            return p;
        }
        goto _1;
_1:;
        p = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 64));
    }
    return (uintptr)0;
}

internal static uintptr /*r*/ _sqlite3GetFuncCollSeq(ref libc.TLS tls, uintptr context) {
    uintptr pOp = default;
    _ = pOp;
    pOp = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)context)).Value.FpVdbe)).Value.FaOp + (uintptr)((@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)context)).Value.FiOp - (int32)1) * 24;
    return ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOp + 16));
}

internal static int32 /*r*/ _sqlite3GetTempRange(ref libc.TLS tls, uintptr pParse, int32 nReg) {
    int32 i = default;
    int32 n = default;
    _ = i;
    _ = n;
    if (nReg == (int32)1) {
        return _sqlite3GetTempReg(ref tls, pParse);
    }
    i = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiRangeReg;
    n = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnRangeReg;
    if (nReg <= n) {
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 48))).Value += nReg;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 44))).Value -= nReg;
    } else {
        i = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += nReg;
    }
    return i;
}

internal static uintptr /*r*/ _sqlite3GetVTable(ref libc.TLS tls, uintptr db, uintptr pTab) {
    uintptr pVtab = default;
    _ = pVtab;
    pVtab = (@unsafe.Pointer.To<TTableᴛ1_Fu_Fvtab>((@unsafe.Pointer)(pTab + 64))).Value.Fp;
    for (;;) {
        if (!(pVtab != 0 && (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVtab)).Value.Fdb != db)) {
            break;
        }
        goto _1;
_1:;
        pVtab = (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVtab)).Value.FpNext;
    }
    return pVtab;
}

internal static void _sqlite3HashClear(ж<libc.TLS> tls, @unsafe.Pointer pH) {
    uintptr elem = default;
    uintptr next_elem = default;
    _ = elem;
    _ = next_elem;
    elem = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pH, 8));
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pH, 8))).Value = (uintptr)0;
    Xsqlite3_free(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pH, 16)));
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pH, 16))).Value = (uintptr)0;
    (@unsafe.Pointer.To<uint32>(pH)).Value = (uint32)0;
    while (elem != 0) {
        next_elem = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fnext;
        Xsqlite3_free(tls, elem);
        elem = next_elem;
    }
    (@unsafe.Pointer.To<uint32>(@unsafe.Add(pH, 4))).Value = (uint32)0;
}

internal static void _sqlite3HashInit(ref libc.TLS tls, @unsafe.Pointer pNew) {
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pNew, 8))).Value = (uintptr)0;
    (@unsafe.Pointer.To<uint32>(@unsafe.Add(pNew, 4))).Value = (uint32)0;
    (@unsafe.Pointer.To<uint32>(pNew)).Value = (uint32)0;
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pNew, 16))).Value = (uintptr)0;
}

internal static uintptr /*r*/ _sqlite3HashInsert(ж<libc.TLS> tls, uintptr pH, uintptr pKey, uintptr data) {
    uintptr elem = default;
    uintptr new_elem = default;
    uintptr old_data = default;
    uintptr v1 = default;
    ref var h = ref heap(new uint32(), out var Ꮡh);
    _ = elem;
    _ = h;
    _ = new_elem;
    _ = old_data;
    _ = v1;
    elem = _findElementWithHash(ref (tls).DerefOrNull(), (@unsafe.Pointer)pH, pKey, @unsafe.Pointer.FromPinnedBox(Ꮡh));
    if ((@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fdata != 0) {
        old_data = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fdata;
        if (data == (uintptr)0) {
            _removeElement(tls, pH, elem);
        } else {
            (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fdata = data;
            (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.FpKey = pKey;
        }
        return old_data;
    }
    if (data == (uintptr)0) {
        return (uintptr)0;
    }
    new_elem = _sqlite3Malloc(tls, (uint64)40);
    if (new_elem == (uintptr)0) {
        return data;
    }
    (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)new_elem)).Value.FpKey = pKey;
    (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)new_elem)).Value.Fh = h;
    (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)new_elem)).Value.Fdata = data;
    (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fcount = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fcount + 1;
    if ((@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fcount >= (uint32)5 && (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fcount > (uint32)2 * (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fhtsize) {
        _rehash(tls, pH, (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fcount * (uint32)3);
    }
    if ((@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fht != 0) {
        v1 = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fht + (uintptr)((@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)new_elem)).Value.Fh % (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fhtsize) * 16;
    } else {
        v1 = (uintptr)0;
    }
    _insertElement(ref (tls).DerefOrNull(), pH, v1, new_elem);
    return (uintptr)0;
}

internal static void _sqlite3IdListDelete(ж<libc.TLS> tls, @unsafe.Pointer db, uintptr pList) {
    int32 i = default;
    _ = i;
    if (pList == (uintptr)0) {
        return;
    }
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TIdList>((@unsafe.Pointer)pList)).Value.FnId)) {
            break;
        }
        _sqlite3DbFree__ccgo_gp(tls, db, (@unsafe.Pointer.To<TIdList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 8))).Value.FzName);
        goto _1;
_1:;
        i = i + 1;
    }
    _sqlite3DbNNFreeNN(tls, db, pList);
}

internal static int32 /*r*/ _sqlite3IdListIndex(ref libc.TLS tls, uintptr pList, uintptr zName) {
    int32 i = default;
    _ = i;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TIdList>((@unsafe.Pointer)pList)).Value.FnId)) {
            break;
        }
        if (_sqlite3StrICmp(ref tls, (@unsafe.Pointer.To<TIdList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 8))).Value.FzName, zName) == 0) {
            return i;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return -(int32)1;
}

internal static int32 /*r*/ _sqlite3JournalSize(ref libc.TLS tls, uintptr pVfs) {
    int32 v1 = default;
    _ = v1;
    if ((@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)pVfs)).Value.FszOsFile > libc.Int32FromInt64(80)) {
        v1 = (@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)pVfs)).Value.FszOsFile;
    } else {
        v1 = libc.Int32FromInt64(80);
    }
    return v1;
}

internal static uintptr /*r*/ _sqlite3JsonVtabRegister(ж<libc.TLS> tls, uintptr db, uintptr zName) {
    uint32 i = default;
    _ = i;
    i = (uint32)0;
    for (;;) {
        if (!((uint64)i < libc.Uint64FromInt64(32) / libc.Uint64FromInt64(8))) {
            break;
        }
        if (_sqlite3StrICmp(ref (tls).DerefOrNull(), _azModule[i], zName) == 0) {
            return _sqlite3VtabCreateModule(tls, db, _azModule[i], (uintptr)Ꮡ_jsonEachModule, (uintptr)0, (uintptr)0);
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return (uintptr)0;
}

internal static uintptr /*r*/ _sqlite3KeyInfoFromExprList(ж<libc.TLS> tls, uintptr pParse, uintptr pList, int32 iStart, int32 nExtra) {
    uintptr db = default;
    uintptr pInfo = default;
    uintptr pItem = default;
    int32 i = default;
    int32 nExpr = default;
    _ = db;
    _ = i;
    _ = nExpr;
    _ = pInfo;
    _ = pItem;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    nExpr = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr;
    pInfo = _sqlite3KeyInfoAlloc(tls, db, nExpr - iStart, nExtra + (int32)1);
    if (pInfo != 0) {
        i = iStart;
        pItem = pList + 8 + (uintptr)iStart * 32;
        for (;;) {
            if (!(i < nExpr)) {
                break;
            }
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pInfo + 32 + (uintptr)(i - iStart) * 8))).Value = _sqlite3ExprNNCollSeq(tls, pParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FpExpr);
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)pInfo)).Value.FaSortFlags + (uintptr)(i - iStart)))).Value = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.Ffg.FsortFlags;
            goto _1;
_1:;
            i = i + 1;
            pItem += 32;
        }
    }
    return pInfo;
}

internal static void _sqlite3MaterializeView(ж<libc.TLS> tls, uintptr pParse, uintptr pView, uintptr pWhere, uintptr pOrderBy, uintptr pLimit, int32 iCur) {
    uintptr db = default;
    uintptr pFrom = default;
    uintptr pSel = default;
    ref var dest = ref heap(new TSelectDest(), out var Ꮡdest);
    int32 iDb = default;
    _ = db;
    _ = dest;
    _ = iDb;
    _ = pFrom;
    _ = pSel;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pView)).Value.FpSchema);
    pWhere = _sqlite3ExprDup(tls, db, pWhere, 0);
    pFrom = _sqlite3SrcListAppend(tls, pParse, (uintptr)0, (uintptr)0, (uintptr)0);
    if (pFrom != 0) {
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pFrom + 8))).Value.FzName = _sqlite3DbStrDup(tls, db, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pView)).Value.FzName);
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 8 + 72))).Value = _sqlite3DbStrDup(tls, db, (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName);
    }
    pSel = _sqlite3SelectNew(tls, pParse, (uintptr)0, pFrom, pWhere, (uintptr)0, (uintptr)0, pOrderBy, (uint32)SF_IncludeHidden, pLimit);
    _sqlite3SelectDestInit(ref (tls).DerefOrNull(), @unsafe.Pointer.FromPinnedBox(Ꮡdest), (int32)SRT_EphemTab, iCur);
    _sqlite3Select(tls, pParse, pSel, @unsafe.Pointer.FromPinnedBox(Ꮡdest));
    _sqlite3SelectDelete(tls, (@unsafe.Pointer)db, pSel);
}

internal static void _sqlite3MemSetArrayInt64(ж<libc.TLS> tls, uintptr aMem, int32 iIdx, Ti64 val) {
    _sqlite3VdbeMemSetInt64(tls, aMem + (uintptr)iIdx * 56, val);
}

internal static uintptr /*r*/ _sqlite3NameFromToken(ж<libc.TLS> tls, uintptr db, @unsafe.Pointer pName) {
    uintptr zName = default;
    _ = zName;
    if (pName != nil) {
        zName = _sqlite3DbStrNDup(tls, db, ~@unsafe.Pointer.To<uintptr>(pName), (uint64)(~@unsafe.Pointer.To<uint32>(@unsafe.Add(pName, 8))));
        _sqlite3Dequote(ref (tls).DerefOrNull(), zName);
    } else {
        zName = (uintptr)0;
    }
    return zName;
}

internal static void _sqlite3PCacheBufferSetup(ref libc.TLS tls, uintptr pBuf, int32 sz, int32 n) {
    uintptr p = default;
    int32 v1 = default;
    _ = p;
    _ = v1;
    if (libc.AtomicLoadPInt32((uintptr)Ꮡ_pcache1_g + 80) != 0) {
        if (pBuf == (uintptr)0) {
            v1 = libc.Int32FromInt32(0);
            n = v1;
            sz = v1;
        }
        if (n == 0) {
            sz = 0;
        }
        sz = sz & ~libc.Int32FromInt32(7);
        _pcache1_g.FszSlot = sz;
        v1 = n;
        _pcache1_g.FnFreeSlot = v1;
        _pcache1_g.FnSlot = v1;
        if (n > (int32)90) {
            v1 = (int32)10;
        } else {
            v1 = n / (int32)10 + (int32)1;
        }
        _pcache1_g.FnReserve = v1;
        _pcache1_g.FpStart = pBuf;
        _pcache1_g.FpFree = (uintptr)0;
        libc.AtomicStoreNInt32((uintptr)Ꮡ_pcache1_g + 140, libc.Int32FromInt32(0), libc.Int32FromInt32(__ATOMIC_RELAXED));
        for (;;) {
            v1 = n;
            n = n - 1;
            if (!(v1 != 0)) {
                break;
            }
            p = pBuf;
            (@unsafe.Pointer.To<TPgFreeslot>((@unsafe.Pointer)p)).Value.FpNext = _pcache1_g.FpFree;
            _pcache1_g.FpFree = p;
            pBuf = pBuf + (uintptr)sz;
        }
        _pcache1_g.FpEnd = pBuf;
    }
}

internal static uintptr /*r*/ _sqlite3PagerBackupPtr(ref libc.TLS tls, uintptr pPager) {
    return pPager + 112;
}

internal static void _sqlite3PagerCacheStat(ref libc.TLS tls, uintptr pPager, int32 eStat, int32 reset, @unsafe.Pointer pnVal) {
    eStat = eStat - (int32)SQLITE_DBSTATUS_CACHE_HIT;
    (@unsafe.Pointer.To<Tu64>(pnVal)).Value += (uint64)(~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pPager + 248 + (uintptr)eStat * 4)));
    if (reset != 0) {
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pPager + 248 + (uintptr)eStat * 4))).Value = (uint32)0;
    }
}

internal static void _sqlite3PagerSetBusyHandler(ж<libc.TLS> tls, uintptr pPager, uintptr __ccgo_fp_xBusyHandler, uintptr pBusyHandlerArg) {
    uintptr ap = default;
    _ = ap;
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FxBusyHandler = __ccgo_fp_xBusyHandler;
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpBusyHandlerArg = pBusyHandlerArg;
    ap = pPager + 232;
    _sqlite3OsFileControlHint(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, (int32)SQLITE_FCNTL_BUSYHANDLER, ap);
}

internal static uintptr /*r*/ _sqlite3ParserAddCleanup(ж<libc.TLS> tls, uintptr pParse, uintptr __ccgo_fp_xCleanup, uintptr pPtr) {
    uintptr pCleanup = default;
    _ = pCleanup;
    if (_sqlite3FaultSim(tls, (int32)300) != 0) {
        pCleanup = (uintptr)0;
        _sqlite3OomFault(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb);
    } else {
        pCleanup = _sqlite3DbMallocRaw(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (uint64)24);
    }
    if (pCleanup != 0) {
        (@unsafe.Pointer.To<TParseCleanup>((@unsafe.Pointer)pCleanup)).Value.FpNext = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpCleanup;
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpCleanup = pCleanup;
        (@unsafe.Pointer.To<TParseCleanup>((@unsafe.Pointer)pCleanup)).Value.FpPtr = pPtr;
        (@unsafe.Pointer.To<TParseCleanup>((@unsafe.Pointer)pCleanup)).Value.FxCleanup = __ccgo_fp_xCleanup;
    } else {
        GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr>>(__ccgo_fp_xCleanup)(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pPtr);
        pPtr = (uintptr)0;
    }
    return pPtr;
}

internal static void _sqlite3ParserInit(ref libc.TLS tls, uintptr yypRawParser, uintptr pParse) {
    uintptr yypParser = default;
    _ = yypParser;
    yypParser = yypRawParser;
    (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)yypParser)).Value.FpParse = pParse;
    (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)yypParser)).Value.Fyystack = yypParser + 32;
    (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)yypParser)).Value.FyystackEnd = (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)yypParser)).Value.Fyystack + (uintptr)(libc.Int32FromInt32(YYSTACKDEPTH) - libc.Int32FromInt32(1)) * 24;
    (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)yypParser)).Value.Fyytos = (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)yypParser)).Value.Fyystack;
    (@unsafe.Pointer.To<TyyStackEntry>(__ccgo_ptr((@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)yypParser)).Value.Fyystack))).Value.Fstateno = (uint16)0;
    (@unsafe.Pointer.To<TyyStackEntry>(__ccgo_ptr((@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)yypParser)).Value.Fyystack))).Value.Fmajor = (uint16)0;
}

internal static void _sqlite3PcacheClearSyncFlags(ref libc.TLS tls, uintptr pCache) {
    uintptr p = default;
    uintptr v2 = default;
    _ = p;
    _ = v2;
    p = (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpDirty;
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        v2 = p + 52;
        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v2)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v2)) & ~libc.Int32FromInt32(PGHDR_NEED_SYNC));
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpDirtyNext;
    }
    (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpSynced = (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpDirtyTail;
}

internal static void _sqlite3PcacheClearWritable(ref libc.TLS tls, uintptr pCache) {
    uintptr p = default;
    uintptr v2 = default;
    _ = p;
    _ = v2;
    p = (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpDirty;
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        v2 = p + 52;
        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v2)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v2)) & ~(libc.Int32FromInt32(PGHDR_NEED_SYNC) | libc.Int32FromInt32(PGHDR_WRITEABLE)));
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpDirtyNext;
    }
    (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpSynced = (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpDirtyTail;
}

internal static void _sqlite3PcacheMakeClean(ж<libc.TLS> tls, uintptr p) {
    uintptr v1 = default;
    _ = v1;
    _pcacheManageDirtyList(ref (tls).DerefOrNull(), p, (uint8)PCACHE_DIRTYLIST_REMOVE);
    v1 = p + 52;
    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(PGHDR_DIRTY) | libc.Int32FromInt32(PGHDR_NEED_SYNC) | libc.Int32FromInt32(PGHDR_WRITEABLE)));
    v1 = p + 52;
    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(PGHDR_CLEAN));
    if ((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FnRef == 0) {
        _pcacheUnpin(tls, p);
    }
}

internal static int32 /*r*/ _sqlite3PcacheSize(ref libc.TLS tls) {
    return (int32)80;
}

internal static void _sqlite3ProgressCheck(ж<libc.TLS> tls, uintptr p) {
    uintptr db = default;
    uintptr v2 = default;
    Tu32 v1 = default;
    _ = db;
    _ = v1;
    _ = v2;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)p)).Value.Fdb;
    if (libc.AtomicLoadNInt32(db + 440, libc.Int32FromInt32(__ATOMIC_RELAXED)) != 0) {
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)p)).Value.FnErr = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)p)).Value.FnErr + 1;
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_INTERRUPT;
    }
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxProgress != 0) {
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)p)).Value.Frc == (int32)SQLITE_INTERRUPT) {
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)p)).Value.FnProgressSteps = (uint32)0;
        } else {
            v2 = p + 136;
            (@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v2) + 1;
            v1 = ~@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v2);
            if (v1 >= (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnProgressOps) {
                if (GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxProgress)(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpProgressArg) != 0) {
                    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)p)).Value.FnErr = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)p)).Value.FnErr + 1;
                    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_INTERRUPT;
                }
                (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)p)).Value.FnProgressSteps = (uint32)0;
            }
        }
    }
}

internal static int32 /*r*/ _sqlite3ReadSchema(ж<libc.TLS> tls, uintptr pParse) {
    uintptr db = default;
    int32 rc = default;
    _ = db;
    _ = rc;
    rc = SQLITE_OK;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    if (!((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy != 0)) {
        rc = _sqlite3Init(tls, db, pParse + 8);
        if (rc != SQLITE_OK) {
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc = rc;
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr + 1;
        } else {
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnoSharedCache != 0) {
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 44))).Value |= (uint32)DBFLAG_SchemaKnownOk;
            }
        }
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3RealSameAsInt(ж<libc.TLS> tls, float64 _r1, Tsqlite3_int64 i) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = _r1;
        float64 _ᴛ1 = default;
        (@unsafe.Pointer.To<float64>(__ccgo_ptr(bp + 8))).Value = (float64)i;
        r = libc.BoolInt32(~@unsafe.Pointer.To<float64>(__ccgo_ptr(bp)) == (float64)0D || libc.Xmemcmp(tls, bp, bp + 8, (uint64)8) == 0 && i >= -(int64)2251799813685248L && i < (int64)2251799813685248L);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _sqlite3RecordErrorByteOffset(ж<libc.TLS> tls, uintptr db, uintptr z) {
    uintptr pParse = default;
    uintptr zEnd = default;
    uintptr zText = default;
    _ = pParse;
    _ = zEnd;
    _ = zText;
    if (db == (uintptr)0) {
        return;
    }
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FerrByteOffset != -(int32)2) {
        return;
    }
    pParse = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpParse;
    if (pParse == (uintptr)0) {
        return;
    }
    zText = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FzTail;
    if (zText == (uintptr)0) {
        return;
    }
    zEnd = zText + (uintptr)libc.Xstrlen(tls, zText);
    if ((uint64)z >= (uint64)zText && (uint64)z < (uint64)zEnd) {
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FerrByteOffset = (int32)((int64)z - (int64)zText);
    }
}

internal static void _sqlite3ResolveSelectNames(ж<libc.TLS> tls, uintptr pParse, uintptr p, uintptr pOuterNC) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_resolveExprStep);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_resolveSelectStep);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback2 = (uintptr)0;
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0))).Value = pOuterNC;
        _sqlite3WalkSelect(tls, bp, p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sqlite3ReturningSubqueryCorrelated(ж<libc.TLS> tls, uintptr pWalker, uintptr pSelect) {
    int32 i = default;
    uintptr pSrc = default;
    _ = i;
    _ = pSrc;
    pSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc)) {
            break;
        }
        if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pSrc + 8 + (uintptr)i * 80))).Value.FpSTab == ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWalker + 40))) {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pSelect + 4))).Value |= (uint32)SF_Correlated;
            (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FeCode = (uint16)1;
            break;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return WRC_Continue;
}

internal static void _sqlite3RootPageMoved(ref libc.TLS tls, uintptr db, int32 iDb, TPgno iFrom, TPgno iTo) {
    uintptr pDb = default;
    uintptr pElem = default;
    uintptr pHash = default;
    uintptr pIdx = default;
    uintptr pTab = default;
    _ = pDb;
    _ = pElem;
    _ = pHash;
    _ = pIdx;
    _ = pTab;
    pDb = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32;
    pHash = (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pDb)).Value.FpSchema + 8;
    pElem = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pHash)).Value.Ffirst;
    for (;;) {
        if (!(pElem != 0)) {
            break;
        }
        pTab = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)pElem)).Value.Fdata;
        if ((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.Ftnum == iFrom) {
            (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.Ftnum = iTo;
        }
        goto _1;
_1:;
        pElem = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)pElem)).Value.Fnext;
    }
    pHash = (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pDb)).Value.FpSchema + 32;
    pElem = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pHash)).Value.Ffirst;
    for (;;) {
        if (!(pElem != 0)) {
            break;
        }
        pIdx = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)pElem)).Value.Fdata;
        if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.Ftnum == iFrom) {
            (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.Ftnum = iTo;
        }
        goto _2;
_2:;
        pElem = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)pElem)).Value.Fnext;
    }
}

internal static void _sqlite3RowSetInsert(ж<libc.TLS> tls, uintptr p, Ti64 rowid) {
    uintptr pEntry = default;
    uintptr pLast = default;
    uintptr v1 = default;
    _ = pEntry;
    _ = pLast;
    _ = v1;
    pEntry = _rowSetEntryAlloc(tls, p);
    if (pEntry == (uintptr)0) {
        return;
    }
    (@unsafe.Pointer.To<TRowSetEntry>((@unsafe.Pointer)pEntry)).Value.Fv = rowid;
    (@unsafe.Pointer.To<TRowSetEntry>((@unsafe.Pointer)pEntry)).Value.FpRight = (uintptr)0;
    pLast = (@unsafe.Pointer.To<TRowSet>((@unsafe.Pointer)p)).Value.FpLast;
    if (pLast != 0) {
        if (rowid <= (@unsafe.Pointer.To<TRowSetEntry>((@unsafe.Pointer)pLast)).Value.Fv) {
            v1 = p + 50;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(ROWSET_SORTED));
        }
        (@unsafe.Pointer.To<TRowSetEntry>((@unsafe.Pointer)pLast)).Value.FpRight = pEntry;
    } else {
        (@unsafe.Pointer.To<TRowSet>((@unsafe.Pointer)p)).Value.FpEntry = pEntry;
    }
    (@unsafe.Pointer.To<TRowSet>((@unsafe.Pointer)p)).Value.FpLast = pEntry;
}

internal static int32 /*r*/ _sqlite3SchemaToIndex(ref libc.TLS tls, uintptr db, uintptr pSchema) {
    int32 i = default;
    _ = i;
    i = -(int32)32768;
    if (pSchema != 0) {
        i = 0;
        for (;;) {
            if (!((int32)1 != 0)) {
                break;
            }
            if ((@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)i * 32))).Value.FpSchema == pSchema) {
                break;
            }
            goto _1;
_1:;
            i = i + 1;
        }
    }
    return i;
}

internal static void _sqlite3SelectAddTypeInfo(ж<libc.TLS> tls, uintptr pParse, uintptr pSelect) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_sqlite3SelectWalkNoop);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback2 = __ccgo_fp(_selectAddSubqueryTypeInfo);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_sqlite3ExprWalkNoop);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        _sqlite3WalkSelect(tls, bp, pSelect);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3SetJoinExpr(ref libc.TLS tls, uintptr p, int32 iTable, Tu32 joinFlag) {
    int32 i = default;
    _ = i;
    while (p != 0) {
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value |= joinFlag;
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)(p + 52))).Value = iTable;
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fflags & (uint32)EP_xIsSelect) == (uint32)0) {
            if (~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 32)) != 0) {
                i = 0;
                for (;;) {
                    if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 32))))).Value.FnExpr)) {
                        break;
                    }
                    _sqlite3SetJoinExpr(ref tls, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 32)) + 8 + (uintptr)i * 32))).Value.FpExpr, iTable, joinFlag);
                    goto _1;
_1:;
                    i = i + 1;
                }
            }
        }
        _sqlite3SetJoinExpr(ref tls, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft, iTable, joinFlag);
        p = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpRight;
    }
}

internal static void _sqlite3StatusDown(ref libc.TLS tls, int32 op, int32 N) {
    (@unsafe.Pointer.To<Tsqlite3StatValueType>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Stat + (uintptr)op * 8))).Value -= (int64)N;
}

internal static void _sqlite3StatusHighwater(ref libc.TLS tls, int32 op, int32 X) {
    Tsqlite3StatValueType newValue = default;
    _ = newValue;
    newValue = (int64)X;
    if (newValue > ~@unsafe.Pointer.To<Tsqlite3StatValueType>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Stat + 80 + (uintptr)op * 8))) {
        (@unsafe.Pointer.To<Tsqlite3StatValueType>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Stat + 80 + (uintptr)op * 8))).Value = newValue;
    }
}

internal static void _sqlite3StatusUp(ref libc.TLS tls, int32 op, int32 N) {
    (@unsafe.Pointer.To<Tsqlite3StatValueType>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Stat + (uintptr)op * 8))).Value += (int64)N;
    if (~@unsafe.Pointer.To<Tsqlite3StatValueType>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Stat + (uintptr)op * 8)) > ~@unsafe.Pointer.To<Tsqlite3StatValueType>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Stat + 80 + (uintptr)op * 8))) {
        (@unsafe.Pointer.To<Tsqlite3StatValueType>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Stat + 80 + (uintptr)op * 8))).Value = (@unsafe.Pointer.To<Tsqlite3StatValueType>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Stat + (uintptr)op * 8))).Value;
    }
}

internal static Tsqlite3_int64 /*r*/ _sqlite3StatusValue(ref libc.TLS tls, int32 op) {
    return ~@unsafe.Pointer.To<Tsqlite3StatValueType>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Stat + (uintptr)op * 8));
}

internal static Tsqlite3_int64 /*r*/ _sqlite3StmtCurrentTime(ж<libc.TLS> tls, uintptr p) {
    Tsqlite3_int64 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr piTime = default;
        uintptr v1 = default;
        int32 rc = default;
        Tsqlite3_int64 _ᴛ1 = default;
        _ = piTime;
        _ = rc;
        _ = v1;
        (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp))).Value = 0;
        if ((@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)p)).Value.FpVdbe != (uintptr)0) {
            v1 = (@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)p)).Value.FpVdbe + 72;
        } else {
            v1 = bp;
        }
        piTime = v1;
        if (~@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(piTime)) == 0) {
            rc = _sqlite3OsCurrentTimeInt64(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)p)).Value.FpOut)).Value.Fdb)).Value.FpVfs, piTime);
            if (rc != 0) {
                (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(piTime))).Value = 0;
            }
        }
        r = ~@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(piTime));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _sqlite3SystemError(ж<libc.TLS> tls, uintptr db, int32 rc) {
    int32 iErr = default;
    int32 ii = default;
    _ = iErr;
    _ = ii;
    if (rc == (libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(12).Lsh((int64)(libc.Int32FromInt32(8))))) {
        return;
    }
    if (rc == (libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(34).Lsh((int64)(libc.Int32FromInt32(8))))) {
        _sqlite3BtreeEnterAll(tls, db);
        ii = 0;
        for (;;) {
            if (!(ii < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
                break;
            }
            if ((@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)ii * 32))).Value.FpBt != 0) {
                iErr = _sqlite3PagerWalSystemErrno(ref (tls).DerefOrNull(), _sqlite3BtreePager(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)ii * 32))).Value.FpBt));
                if (iErr != 0) {
                    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FiSysErrno = iErr;
                }
            }
            goto _1;
_1:;
            ii = ii + 1;
        }
        _sqlite3BtreeLeaveAll(tls, db);
        return;
    }
    rc = rc & (int32)0xff;
    if (rc == (int32)SQLITE_CANTOPEN || rc == (int32)SQLITE_IOERR) {
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FiSysErrno = _sqlite3OsGetLastError(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpVfs);
    }
}

internal static void _sqlite3TableLock(ж<libc.TLS> tls, uintptr pParse, int32 iDb, TPgno iTab, Tu8 isWriteLock, uintptr zName) {
    if (iDb == (int32)1) {
        return;
    }
    if (!(_sqlite3BtreeSharable(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FaDb + (uintptr)iDb * 32))).Value.FpBt) != 0)) {
        return;
    }
    _lockTable(tls, pParse, iDb, iTab, isWriteLock, zName);
}

internal static int32 /*r*/ _sqlite3TransferBindings(ж<libc.TLS> tls, uintptr pFromStmt, uintptr pToStmt) {
    int32 i = default;
    uintptr pFrom = default;
    uintptr pTo = default;
    _ = i;
    _ = pFrom;
    _ = pTo;
    pFrom = pFromStmt;
    pTo = pToStmt;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pTo)).Value.Fdb)).Value.Fmutex);
    i = 0;
    for (;;) {
        if (!(i < (int32)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pFrom)).Value.FnVar)) {
            break;
        }
        _sqlite3VdbeMemMove(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pTo)).Value.FaVar + (uintptr)i * 56, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pFrom)).Value.FaVar + (uintptr)i * 56);
        goto _1;
_1:;
        i = i + 1;
    }
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pTo)).Value.Fdb)).Value.Fmutex);
    return SQLITE_OK;
}

internal static uintptr /*r*/ _sqlite3TriggerSelectStep(ж<libc.TLS> tls, uintptr db, uintptr pSelect, uintptr zStart, uintptr zEnd) {
    uintptr pTriggerStep = default;
    _ = pTriggerStep;
    pTriggerStep = _sqlite3DbMallocZero(tls, db, (uint64)88);
    if (pTriggerStep == (uintptr)0) {
        _sqlite3SelectDelete(tls, (@unsafe.Pointer)db, pSelect);
        return (uintptr)0;
    }
    (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.Fop = (uint8)TK_SELECT;
    (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.FpSelect = pSelect;
    (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.Forconf = (uint8)OE_Default;
    (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.FzSpan = _triggerSpanDup(tls, db, zStart, zEnd);
    return pTriggerStep;
}

internal static void _sqlite3UnlinkAndDeleteIndex(ж<libc.TLS> tls, uintptr db, int32 iDb, uintptr zIdxName) {
    uintptr p = default;
    uintptr pHash = default;
    uintptr pIndex = default;
    _ = p;
    _ = pHash;
    _ = pIndex;
    pHash = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpSchema + 32;
    pIndex = _sqlite3HashInsert(tls, pHash, zIdxName, (uintptr)0);
    if (pIndex != 0) {
        if ((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FpTable)).Value.FpIndex == pIndex) {
            (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FpTable)).Value.FpIndex = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FpNext;
        } else {
            p = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FpTable)).Value.FpIndex;
            while (p != 0 && (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FpNext != pIndex) {
                p = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FpNext;
            }
            if (p != 0 && (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FpNext == pIndex) {
                (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FpNext = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FpNext;
            }
        }
        _sqlite3FreeIndex(tls, (@unsafe.Pointer)db, pIndex);
    }
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 44))).Value |= (uint32)DBFLAG_SchemaChange;
}

internal static void _sqlite3UnlinkAndDeleteTable(ж<libc.TLS> tls, uintptr db, int32 iDb, uintptr zTabName) {
    uintptr p = default;
    uintptr pDb = default;
    _ = p;
    _ = pDb;
    pDb = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32;
    p = _sqlite3HashInsert(tls, (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pDb)).Value.FpSchema + 8, zTabName, (uintptr)0);
    _sqlite3DeleteTable(tls, (@unsafe.Pointer)db, p);
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 44))).Value |= (uint32)DBFLAG_SchemaChange;
}

internal static void _sqlite3UnlinkAndDeleteTrigger(ж<libc.TLS> tls, uintptr db, int32 iDb, uintptr zName) {
    uintptr pHash = default;
    uintptr pTab = default;
    uintptr pTrigger = default;
    uintptr pp = default;
    _ = pHash;
    _ = pTab;
    _ = pTrigger;
    _ = pp;
    pHash = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpSchema + 56;
    pTrigger = _sqlite3HashInsert(tls, pHash, zName, (uintptr)0);
    if (pTrigger != 0) {
        if ((@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.FpSchema == (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.FpTabSchema) {
            pTab = _tableOfTrigger(ref (tls).DerefOrNull(), pTrigger);
            if (pTab != 0) {
                pp = pTab + 88;
                for (;;) {
                    if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != 0)) {
                        break;
                    }
                    if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) == pTrigger) {
                        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))))).Value.FpNext;
                        break;
                    }
                    goto _1;
_1:;
                    pp = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) + 64;
                }
            }
        }
        _sqlite3DeleteTrigger(tls, (@unsafe.Pointer)db, pTrigger);
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 44))).Value |= (uint32)DBFLAG_SchemaChange;
    }
}

internal static uintptr /*r*/ _sqlite3UpsertNew(ж<libc.TLS> tls, uintptr db, uintptr pTarget, uintptr pTargetWhere, uintptr pSet, uintptr pWhere, uintptr pNext) {
    uintptr pNew = default;
    _ = pNew;
    pNew = _sqlite3DbMallocZero(tls, db, (uint64)88);
    if (pNew == (uintptr)0) {
        _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, pTarget);
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pTargetWhere);
        _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, pSet);
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pWhere);
        _sqlite3UpsertDelete(tls, (@unsafe.Pointer)db, pNext);
        return (uintptr)0;
    } else {
        (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pNew)).Value.FpUpsertTarget = pTarget;
        (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pNew)).Value.FpUpsertTargetWhere = pTargetWhere;
        (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pNew)).Value.FpUpsertSet = pSet;
        (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pNew)).Value.FpUpsertWhere = pWhere;
        (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pNew)).Value.FisDoUpdate = libc.BoolUint8(pSet != (uintptr)0);
        (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pNew)).Value.FpNextUpsert = pNext;
    }
    return pNew;
}

internal static void _sqlite3Vacuum(ж<libc.TLS> tls, uintptr pParse, uintptr pNmʗp, uintptr pInto) {
    ref var pNm = ref heap(pNmʗp, out var ᏑpNm);

    int32 iDb = default;
    int32 iIntoReg = default;
    int32 v1 = default;
    uintptr v = default;
    uintptr v2 = default;
    _ = iDb;
    _ = iIntoReg;
    _ = v;
    _ = v1;
    _ = v2;
    v = _sqlite3GetVdbe(tls, pParse);
    iDb = 0;
    if (v == (uintptr)0) {
        goto build_vacuum_end;
    }
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
        goto build_vacuum_end;
    }
    if (pNm != 0) {
        iDb = _sqlite3TwoPartName(tls, pParse, pNm, pNm, @unsafe.Pointer.FromBox(ᏑpNm));
        if (iDb < 0) {
            goto build_vacuum_end;
        }
    }
    if (iDb != (int32)1) {
        iIntoReg = 0;
        if (pInto != 0 && _sqlite3ResolveSelfReference(tls, pParse, (uintptr)0, 0, pInto, (uintptr)0) == 0) {
            v2 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
            iIntoReg = v1;
            _sqlite3ExprCode(tls, pParse, pInto, iIntoReg);
        }
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Vacuum, iDb, iIntoReg);
        _sqlite3VdbeUsesBtree(ref (tls).DerefOrNull(), v, iDb);
    }
    goto build_vacuum_end;
build_vacuum_end:;
    _sqlite3ExprDelete(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pInto);
    return;
}

internal static uintptr /*r*/ _sqlite3ValueNew(ж<libc.TLS> tls, uintptr db) {
    uintptr p = default;
    _ = p;
    p = _sqlite3DbMallocZero(tls, db, (uint64)56);
    if (p != 0) {
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fflags = (uint16)MEM_Null;
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fdb = db;
    }
    return p;
}

internal static void _sqlite3VdbeAppendP4(ж<libc.TLS> tls, uintptr p, uintptr pP4, int32 n) {
    uintptr pOp = default;
    _ = pOp;
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.FmallocFailed != 0) {
        _freeP4(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb, n, pP4);
    } else {
        pOp = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaOp + (uintptr)((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnOp - (int32)1) * 24;
        (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp4type = (int8)n;
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOp + 16))).Value = pP4;
    }
}

internal static void _sqlite3VdbeChangeP4(ж<libc.TLS> tls, uintptr p, int32 addr, uintptr _zP4, int32 n) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)bp)).Value = _zP4;
        uintptr db = default;
        uintptr pOp = default;
        _ = db;
        _ = pOp;
        db = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb;
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            if (n != -(int32)12) {
                _freeP4(tls, db, n, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
            }
            return;
        }
        if (addr < 0) {
            addr = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnOp - (int32)1;
        }
        pOp = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaOp + (uintptr)addr * 24;
        if (n >= 0 || (@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp4type != 0) {
            _vdbeChangeP4Full(tls, p, pOp, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)), n);
            return;
        }
        if (n == -(int32)3) {
            (@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp4.Fi = (int32)(int64)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
            (@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp4type = (int8)(-libc.Int32FromInt32(3));
        } else {
            if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) != (uintptr)0) {
                (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOp + 16))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value;
                (@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp4type = (int8)n;
                if (n == -(int32)12) {
                    _sqlite3VtabLock(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3VdbeChangeP5(ref libc.TLS tls, uintptr p, Tu16 p5) {
    if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnOp > 0) {
        (@unsafe.Pointer.To<TOp>(__ccgo_ptr((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaOp + (uintptr)((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnOp - (int32)1) * 24))).Value.Fp5 = p5;
    }
}

internal static int32 /*r*/ _sqlite3VdbeChangeToNoop(ж<libc.TLS> tls, uintptr p, int32 addr) {
    uintptr pOp = default;
    _ = pOp;
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.FmallocFailed != 0) {
        return 0;
    }
    pOp = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaOp + (uintptr)addr * 24;
    _freeP4(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb, (int32)(@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp4type, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOp + 16)));
    (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp4type = P4_NOTUSED;
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOp + 16))).Value = (uintptr)0;
    (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode = (uint8)OP_Noop;
    return (int32)1;
}

internal static int32 /*r*/ _sqlite3VdbeCursorRestore(ж<libc.TLS> tls, uintptr p) {
    if (_sqlite3BtreeCursorHasMoved(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 48))) != 0) {
        return _sqlite3VdbeHandleMovedCursor(tls, p);
    }
    return SQLITE_OK;
}

internal static void _sqlite3VdbeEnter(ж<libc.TLS> tls, uintptr p) {
    uintptr aDb = default;
    uintptr db = default;
    int32 i = default;
    int32 nDb = default;
    _ = aDb;
    _ = db;
    _ = i;
    _ = nDb;
    if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FlockMask == (uint32)0) {
        return;
    }
    db = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb;
    aDb = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb;
    nDb = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb;
    i = 0;
    for (;;) {
        if (!(i < nDb)) {
            break;
        }
        if (i != (int32)1 && (TyDbMask)((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FlockMask & (libc.Uint32FromInt32(1).Lsh((int64)(i)))) != (uint32)0 && (@unsafe.Pointer.To<TDb>(__ccgo_ptr(aDb + (uintptr)i * 32))).Value.FpBt != (uintptr)0) {
            _sqlite3BtreeEnter(tls, (@unsafe.Pointer.To<TDb>(__ccgo_ptr(aDb + (uintptr)i * 32))).Value.FpBt);
        }
        goto _1;
_1:;
        i = i + 1;
    }
}

internal static int32 /*r*/ _sqlite3VdbeFinishMoveto(ж<libc.TLS> tls, uintptr p) {
    int32 rc = default;
    ref var res = ref heap(new int32(), out var Ꮡres);
    _ = rc;
    _ = res;
    rc = _sqlite3BtreeTableMoveto(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 48)), (@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)p)).Value.FmovetoTarget, 0, @unsafe.Pointer.FromPinnedBox(Ꮡres));
    if (rc != 0) {
        return rc;
    }
    if (res != 0) {
        return _sqlite3CorruptError(tls, (int32)91715);
    }
    (@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)p)).Value.FdeferredMoveto = (uint8)0;
    (@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)p)).Value.FcacheStatus = (uint32)CACHE_STALE;
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3VdbeFrameRestore(ж<libc.TLS> tls, uintptr pFrame) {
    uintptr v = default;
    _ = v;
    v = (@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pFrame)).Value.Fv;
    _closeCursorsInFrame(tls, v);
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FaOp = (@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pFrame)).Value.FaOp;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FnOp = (@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pFrame)).Value.FnOp;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FaMem = (@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pFrame)).Value.FaMem;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FnMem = (@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pFrame)).Value.FnMem;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FapCsr = (@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pFrame)).Value.FapCsr;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FnCursor = (@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pFrame)).Value.FnCursor;
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.Fdb)).Value.FlastRowid = (@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pFrame)).Value.FlastRowid;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FnChange = (@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pFrame)).Value.FnChange;
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.Fdb)).Value.FnChange = (@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pFrame)).Value.FnDbChange;
    _sqlite3VdbeDeleteAuxData(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.Fdb, v + 304, -(int32)1, 0);
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FpAuxData = (@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pFrame)).Value.FpAuxData;
    (@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pFrame)).Value.FpAuxData = (uintptr)0;
    return (@unsafe.Pointer.To<TVdbeFrame>((@unsafe.Pointer)pFrame)).Value.Fpc;
}

internal static uintptr /*r*/ _sqlite3VdbeGetOp(ref libc.TLS tls, uintptr p, int32 addr) {
    uintptr r = default;

    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.FmallocFailed != 0) {
        return (uintptr)Ꮡ_dummy;
    } else {
        return (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaOp + (uintptr)addr * 24;
    }
    return r;
}

internal static int32 /*r*/ _sqlite3VdbeHandleMovedCursor(ж<libc.TLS> tls, uintptr p) {
    ref var isDifferentRow = ref heap(new int32(), out var ᏑisDifferentRow);
    int32 rc = default;
    _ = isDifferentRow;
    _ = rc;
    rc = _sqlite3BtreeCursorRestore(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 48)), @unsafe.Pointer.FromPinnedBox(ᏑisDifferentRow));
    (@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)p)).Value.FcacheStatus = (uint32)CACHE_STALE;
    if (isDifferentRow != 0) {
        (@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)p)).Value.FnullRow = (uint8)1;
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3VdbeMakeLabel(ref libc.TLS tls, @unsafe.Pointer pParse) {
    int32 v1 = default;
    @unsafe.Pointer v2 = default;
    _ = v1;
    _ = v2;
    v2 = @unsafe.Add(pParse, 76);
    (@unsafe.Pointer.To<int32>(v2)).Value = ~@unsafe.Pointer.To<int32>(v2) - 1;
    v1 = ~@unsafe.Pointer.To<int32>(v2);
    return v1;
}

internal static int32 /*r*/ _sqlite3VdbeMemClearAndResize(ж<libc.TLS> tls, uintptr pMem, int32 szNew) {
    uintptr v1 = default;
    _ = v1;
    if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FszMalloc < szNew) {
        return _sqlite3VdbeMemGrow(tls, pMem, szNew, 0);
    }
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz = (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc;
    v1 = pMem + 20;
    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & (libc.Int32FromInt32(MEM_Null) | libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_Real) | libc.Int32FromInt32(MEM_IntReal)));
    return SQLITE_OK;
}

internal static void _sqlite3VdbeMemInit(ref libc.TLS tls, @unsafe.Pointer pMem, uintptr db, Tu16 flags) {
    (@unsafe.Pointer.To<Tu16>(@unsafe.Add(pMem, 20))).Value = flags;
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pMem, 24))).Value = db;
    (@unsafe.Pointer.To<int32>(@unsafe.Add(pMem, 32))).Value = 0;
}

internal static void _sqlite3VdbeReprepareOnBind(ref libc.TLS tls, uintptr v, int32 iVar, int32 bSmallint) {
    Tu32 m = default;
    uint32 v1 = default;
    _ = m;
    _ = v1;
    if (iVar >= (int32)32) {
        v1 = (uint32)0x80000000U;
    } else {
        v1 = libc.Uint32FromInt32(1).Lsh((int64)(iVar - libc.Int32FromInt32(1)));
    }
    m = v1;
    if (bSmallint != 0) {
        if ((Tu32)((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.Fexpmask & m) == (uint32)0) {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(v + 288))).Value |= m;
        }
    } else {
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(v + 288))).Value &= ~m;
    }
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(v + 284))).Value |= m;
}

internal static void _sqlite3VdbeSetChanges(ref libc.TLS tls, uintptr db, Ti64 nChange) {
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnChange = nChange;
    (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(db + 128))).Value += nChange;
}

internal static int32 /*r*/ _sqlite3VdbeSorterNext(ж<libc.TLS> tls, uintptr db, uintptr pCsr) {
    uintptr pFree = default;
    uintptr pSorter = default;
    int32 rc = default;
    ref var res = ref heap(new int32(), out var Ꮡres);
    int32 v1 = default;
    _ = pFree;
    _ = pSorter;
    _ = rc;
    _ = res;
    _ = v1;
    pSorter = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pCsr + 48));
    if ((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FbUsePMA != 0) {
        if ((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FbUseThreads != 0) {
            rc = _vdbePmaReaderNext(tls, (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FpReader);
            if (rc == SQLITE_OK && (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FpReader)).Value.FpFd == (uintptr)0) {
                rc = (int32)SQLITE_DONE;
            }
        } else {
            res = 0;
            rc = _vdbeMergeEngineStep(tls, (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FpMerger, @unsafe.Pointer.FromPinnedBox(Ꮡres));
            if (rc == SQLITE_OK && res != 0) {
                rc = (int32)SQLITE_DONE;
            }
        }
    } else {
        pFree = (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FpList;
        (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FpList = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFree + 8));
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFree + 8))).Value = (uintptr)0;
        if ((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FaMemory == (uintptr)0) {
            _vdbeSorterRecordFree(tls, db, pFree);
        }
        if ((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FpList != 0) {
            v1 = SQLITE_OK;
        } else {
            v1 = (int32)SQLITE_DONE;
        }
        rc = v1;
    }
    return rc;
}

internal static void _sqlite3VdbeUsesBtree(ref libc.TLS tls, uintptr p, int32 i) {
    (@unsafe.Pointer.To<TyDbMask>(__ccgo_ptr(p + 204))).Value |= libc.Uint32FromInt32(1).Lsh((int64)(i));
    if (i != (int32)1 && _sqlite3BtreeSharable(ref tls, (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.FaDb + (uintptr)i * 32))).Value.FpBt) != 0) {
        (@unsafe.Pointer.To<TyDbMask>(__ccgo_ptr(p + 208))).Value |= libc.Uint32FromInt32(1).Lsh((int64)(i));
    }
}

internal static int32 /*r*/ _sqlite3VtabBegin(ж<libc.TLS> tls, uintptr db, uintptr pVTab) {
    int32 i = default;
    int32 iSvpt = default;
    int32 rc = default;
    uintptr pModule = default;
    _ = i;
    _ = iSvpt;
    _ = pModule;
    _ = rc;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVTrans > 0 && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaVTrans == (uintptr)0) {
        return (int32)SQLITE_LOCKED;
    }
    if (!(pVTab != 0)) {
        return SQLITE_OK;
    }
    pModule = (@unsafe.Pointer.To<Tsqlite3_vtab>((@unsafe.Pointer)(@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVTab)).Value.FpVtab)).Value.FpModule;
    if ((@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)pModule)).Value.FxBegin != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVTrans)) {
                break;
            }
            if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaVTrans + (uintptr)i * 8)) == pVTab) {
                return SQLITE_OK;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        rc = _growVTrans(tls, db);
        if (rc == SQLITE_OK) {
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)pModule)).Value.FxBegin)(tls, (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVTab)).Value.FpVtab);
            if (rc == SQLITE_OK) {
                iSvpt = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnStatement + (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnSavepoint;
                _addToVTrans(ref (tls).DerefOrNull(), db, pVTab);
                if (iSvpt != 0 && (@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)pModule)).Value.FxSavepoint != 0) {
                    (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVTab)).Value.FiSavepoint = iSvpt;
                    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32>>((@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)pModule)).Value.FxSavepoint)(tls, (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVTab)).Value.FpVtab, iSvpt - (int32)1);
                }
            }
        }
    }
    return rc;
}

internal static void _sqlite3VtabClear(ж<libc.TLS> tls, @unsafe.Pointer db, uintptr p) {
    int32 i = default;
    _ = i;
    if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(db, 808)) == (uintptr)0) {
        _vtabDisconnectAll(ref (tls).DerefOrNull(), (uintptr)0, p);
    }
    if ((@unsafe.Pointer.To<TTableᴛ1_Fu_Fvtab>((@unsafe.Pointer)(p + 64))).Value.FazArg != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TTableᴛ1_Fu_Fvtab>((@unsafe.Pointer)(p + 64))).Value.FnArg)) {
                break;
            }
            if (i != (int32)1) {
                _sqlite3DbFree__ccgo_gp(tls, db, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TTableᴛ1_Fu_Fvtab>((@unsafe.Pointer)(p + 64))).Value.FazArg + (uintptr)i * 8)));
            }
            goto _1;
_1:;
            i = i + 1;
        }
        _sqlite3DbFree__ccgo_gp(tls, db, (@unsafe.Pointer.To<TTableᴛ1_Fu_Fvtab>((@unsafe.Pointer)(p + 64))).Value.FazArg);
    }
}

internal static void _sqlite3VtabDisconnect(ж<libc.TLS> tls, uintptr db, uintptr p) {
    uintptr pVTab = default;
    uintptr ppVTab = default;
    _ = pVTab;
    _ = ppVTab;
    ppVTab = p + 64 + 16;
    for (;;) {
        if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppVTab)) != 0)) {
            break;
        }
        if ((@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppVTab))))).Value.Fdb == db) {
            pVTab = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppVTab));
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppVTab))).Value = (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVTab)).Value.FpNext;
            _sqlite3VtabUnlock(tls, pVTab);
            break;
        }
        goto _1;
_1:;
        ppVTab = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppVTab)) + 40;
    }
}

internal static void _sqlite3VtabEponymousTableClear(ж<libc.TLS> tls, uintptr db, uintptr pMod) {
    uintptr pTab = default;
    _ = pTab;
    pTab = (@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FpEpoTab;
    if (pTab != (uintptr)0) {
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pTab + 48))).Value |= (uint32)TF_Ephemeral;
        _sqlite3DeleteTable(tls, (@unsafe.Pointer)db, pTab);
        (@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FpEpoTab = (uintptr)0;
    }
}

internal static void _sqlite3VtabEponymousTableClearAll(ж<libc.TLS> tls, uintptr db) {
    uintptr pElem = default;
    _ = pElem;
    pElem = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)(db + 584))).Value.Ffirst;
    for (;;) {
        if (!(pElem != 0)) {
            break;
        }
        _sqlite3VtabEponymousTableClear(tls, db, (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)pElem)).Value.Fdata);
        goto _1;
_1:;
        pElem = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)pElem)).Value.Fnext;
    }
}

internal static int32 /*r*/ _sqlite3VtabSavepoint(ж<libc.TLS> tls, uintptr db, int32 op, int32 iSavepoint) {
    int32 i = default;
    int32 rc = default;
    uintptr pMod = default;
    uintptr pVTab = default;
    ref var xMethod = ref heap(new uintptr(), out var ᏑxMethod);
    Tu64 savedFlags = default;
    _ = i;
    _ = pMod;
    _ = pVTab;
    _ = rc;
    _ = savedFlags;
    _ = xMethod;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaVTrans != 0) {
        i = 0;
        for (;;) {
            if (!(rc == SQLITE_OK && i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVTrans)) {
                break;
            }
            pVTab = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaVTrans + (uintptr)i * 8));
            pMod = (@unsafe.Pointer.To<TModule>((@unsafe.Pointer)(@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVTab)).Value.FpMod)).Value.FpModule;
            if ((@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVTab)).Value.FpVtab != 0 && (@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)pMod)).Value.FiVersion >= (int32)2) {
                _sqlite3VtabLock(ref (tls).DerefOrNull(), pVTab);
                switch (op) {
                case var ᴛ when ᴛ == SAVEPOINT_BEGIN: {
                    xMethod = (@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)pMod)).Value.FxSavepoint;
                    (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVTab)).Value.FiSavepoint = iSavepoint + (int32)1;
                    break;
                }
                case var ᴛ when ᴛ == (int32)SAVEPOINT_ROLLBACK: {
                    xMethod = (@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)pMod)).Value.FxRollbackTo;
                    break;
                }
                default: {
                    xMethod = (@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)pMod)).Value.FxRelease;
                    break;
                    break;
                }}

                if (xMethod != 0 && (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVTab)).Value.FiSavepoint > iSavepoint) {
                    savedFlags = (Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & libc.Uint64FromInt32(SQLITE_Defensive));
                    (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(db + 48))).Value &= ~libc.Uint64FromInt32(SQLITE_Defensive);
                    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32>>(xMethod)(tls, (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVTab)).Value.FpVtab, iSavepoint);
                    (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(db + 48))).Value |= savedFlags;
                }
                _sqlite3VtabUnlock(tls, pVTab);
            }
            goto _1;
_1:;
            i = i + 1;
        }
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3VtabSync(ж<libc.TLS> tls, uintptr db, uintptr p) {
    uintptr aVTrans = default;
    uintptr pVtab = default;
    ref var x = ref heap(new uintptr(), out var Ꮡx);
    uintptr v2 = default;
    int32 i = default;
    int32 rc = default;
    bool v3 = default;
    _ = aVTrans;
    _ = i;
    _ = pVtab;
    _ = rc;
    _ = x;
    _ = v2;
    _ = v3;
    rc = SQLITE_OK;
    aVTrans = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaVTrans;
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaVTrans = (uintptr)0;
    i = 0;
    for (;;) {
        if (!(rc == SQLITE_OK && i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVTrans)) {
            break;
        }
        pVtab = (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aVTrans + (uintptr)i * 8))))).Value.FpVtab;
        v3 = pVtab != 0; if (v3) {
            v2 = (@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_vtab>((@unsafe.Pointer)pVtab)).Value.FpModule)).Value.FxSync;
            x = v2;
        }
        if (v3 && v2 != (uintptr)0) {
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>(x)(tls, pVtab);
            _sqlite3VtabImportErrmsg(tls, p, pVtab);
        }
        goto _1;
_1:;
        i = i + 1;
    }
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaVTrans = aVTrans;
    return rc;
}

internal static int32 /*r*/ _sqlite3WalDefaultHook(ж<libc.TLS> tls, uintptr pClientData, uintptr db, uintptr zDb, int32 nFrame) {
    if (nFrame >= (int32)(int64)pClientData) {
        _sqlite3BeginBenignMalloc(tls);
        Xsqlite3_wal_checkpoint(tls, db, zDb);
        _sqlite3EndBenignMalloc(tls);
    }
    return SQLITE_OK;
}

internal static void _sqlite3WalSavepoint(ref libc.TLS tls, uintptr pWal, uintptr aWalData) {
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aWalData))).Value = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame;
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aWalData + 1 * 4))).Value = (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24))).Value;
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aWalData + 2 * 4))).Value = (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24 + 1 * 4))).Value;
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aWalData + 3 * 4))).Value = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FnCkpt;
}

internal static int32 /*r*/ _sqlite3WalSavepointUndo(ж<libc.TLS> tls, uintptr pWal, uintptr aWalData) {
    int32 rc = default;
    _ = rc;
    rc = SQLITE_OK;
    if (~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aWalData + 3 * 4)) != (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FnCkpt) {
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aWalData))).Value = (uint32)0;
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aWalData + 3 * 4))).Value = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FnCkpt;
    }
    if (~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aWalData)) < (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame) {
        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aWalData));
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24))).Value = (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aWalData + 1 * 4))).Value;
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24 + 1 * 4))).Value = (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aWalData + 2 * 4))).Value;
        rc = _walSehTry(tls, pWal, __ccgo_fp(_walSehCleanupHash), (uintptr)0, (uintptr)0);
        if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiReCksum > (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame) {
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiReCksum = (uint32)0;
        }
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3WalUndo(ж<libc.TLS> tls, uintptr pWal, uintptr __ccgo_fp_xUndo, uintptr pUndoCtx) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 rc = default;
        TWalSehUndo _ᴛ1 = default;
        _ = rc;
        rc = SQLITE_OK;
        if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FwriteLock != 0) {
            (@unsafe.Pointer.To<TWalSehUndo>(__ccgo_ptr(bp))).Value.FxUndo = __ccgo_fp_xUndo;
            (@unsafe.Pointer.To<TWalSehUndo>(__ccgo_ptr(bp))).Value.FpUndoCtx = pUndoCtx;
            (@unsafe.Pointer.To<TWalSehUndo>(__ccgo_ptr(bp))).Value.FiMax = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame;
            rc = _walSehTry(tls, pWal, __ccgo_fp(_walSehUndo), bp, (uintptr)0);
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiReCksum = (uint32)0;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3WalkExprList(ж<libc.TLS> tls, uintptr pWalker, uintptr p) {
    int32 i = default;
    uintptr pItem = default;
    _ = i;
    _ = pItem;
    if (p != 0) {
        i = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)p)).Value.FnExpr;
        pItem = p + 8;
        for (;;) {
            if (!(i > 0)) {
                break;
            }
            if (_sqlite3WalkExpr(tls, pWalker, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FpExpr) != 0) {
                return (int32)WRC_Abort;
            }
            goto _1;
_1:;
            i = i - 1;
            pItem += 32;
        }
    }
    return WRC_Continue;
}

internal static TBitmask /*r*/ _sqlite3WhereExprListUsage(ref libc.TLS tls, uintptr pMaskSet, uintptr pList) {
    int32 i = default;
    TBitmask mask = default;
    _ = i;
    _ = mask;
    mask = (uint64)0;
    if (pList != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr)) {
                break;
            }
            mask = (TBitmask)(mask | _sqlite3WhereExprUsage(ref tls, pMaskSet, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32))).Value.FpExpr));
            goto _1;
_1:;
            i = i + 1;
        }
    }
    return mask;
}

internal static uintptr /*r*/ _sqlite3WindowDup(ж<libc.TLS> tls, uintptr db, uintptr pOwner, uintptr p) {
    uintptr pNew = default;
    _ = pNew;
    pNew = (uintptr)0;
    if (p != 0) {
        pNew = _sqlite3DbMallocZero(tls, db, (uint64)144);
        if (pNew != 0) {
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FzName = _sqlite3DbStrDup(tls, db, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FzName);
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FzBase = _sqlite3DbStrDup(tls, db, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FzBase);
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FpFilter = _sqlite3ExprDup(tls, db, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FpFilter, 0);
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FpWFunc = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FpWFunc;
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FpPartition = _sqlite3ExprListDup(tls, db, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FpPartition, 0);
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FpOrderBy = _sqlite3ExprListDup(tls, db, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FpOrderBy, 0);
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FeFrmType = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FeFrmType;
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FeEnd = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FeEnd;
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FeStart = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FeStart;
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FeExclude = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FeExclude;
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FregResult = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FregResult;
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FregAccum = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FregAccum;
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FiArgCol = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FiArgCol;
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FiEphCsr = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FiEphCsr;
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FbExprArgs = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FbExprArgs;
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FpStart = _sqlite3ExprDup(tls, db, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FpStart, 0);
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FpEnd = _sqlite3ExprDup(tls, db, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FpEnd, 0);
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FpOwner = pOwner;
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pNew)).Value.FbImplicitFrame = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FbImplicitFrame;
        }
    }
    return pNew;
}

internal static void _sqlite3WindowLink(ж<libc.TLS> tls, uintptr pSel, uintptr pWin) {
    if (pSel != 0) {
        if ((uintptr)0 == (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.FpWin || 0 == _sqlite3WindowCompare(tls, (uintptr)0, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.FpWin, pWin, 0)) {
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpNextWin = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.FpWin;
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.FpWin != 0) {
                (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.FpWin)).Value.FppThis = pWin + 64;
            }
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.FpWin = pWin;
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FppThis = pSel + 104;
        } else {
            if (_sqlite3ExprListCompare(tls, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpPartition, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.FpWin)).Value.FpPartition, -(int32)1) != 0) {
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pSel + 4))).Value |= (uint32)SF_MultiPart;
            }
        }
    }
}

internal static uintptr /*r*/ _sqlite3WindowListDup(ж<libc.TLS> tls, uintptr db, uintptr p) {
    ref var pRet = ref heap(new uintptr(), out var ᏑpRet);
    uintptr pWin = default;
    @unsafe.Pointer pp = default;
    _ = pRet;
    _ = pWin;
    _ = pp;
    pRet = (uintptr)0;
    pp = @unsafe.Pointer.FromBox(ᏑpRet);
    pWin = p;
    for (;;) {
        if (!(pWin != 0)) {
            break;
        }
        (@unsafe.Pointer.To<uintptr>(pp)).Value = _sqlite3WindowDup(tls, db, (uintptr)0, pWin);
        if (~@unsafe.Pointer.To<uintptr>(pp) == (uintptr)0) {
            break;
        }
        pp = (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pp) + 64);
        goto _1;
_1:;
        pWin = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpNextWin;
    }
    return pRet;
}

internal static void _sqlite3WithDelete(ж<libc.TLS> tls, @unsafe.Pointer db, uintptr pWith) {
    int32 i = default;
    _ = i;
    if (pWith != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TWith>((@unsafe.Pointer)pWith)).Value.FnCte)) {
                break;
            }
            _cteClear(tls, db, pWith + 16 + (uintptr)i * 48);
            goto _1;
_1:;
            i = i + 1;
        }
        _sqlite3DbFree__ccgo_gp(tls, db, pWith);
    }
}

internal static int32 /*r*/ _sqlite3changeset_new__ccgo_gp(ref libc.TLS tls, uintptr pIter, int32 iVal, @unsafe.Pointer ppValue) {
    if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.Fop != (int32)SQLITE_UPDATE && (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.Fop != (int32)SQLITE_INSERT) {
        return (int32)SQLITE_MISUSE;
    }
    if (iVal < 0 || iVal >= (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FnCol) {
        return (int32)SQLITE_RANGE;
    }
    (@unsafe.Pointer.To<uintptr>(ppValue)).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FapValue + (uintptr)((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FnCol + iVal) * 8))).Value;
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3changeset_old__ccgo_gp(ref libc.TLS tls, uintptr pIter, int32 iVal, @unsafe.Pointer ppValue) {
    if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.Fop != (int32)SQLITE_UPDATE && (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.Fop != (int32)SQLITE_DELETE) {
        return (int32)SQLITE_MISUSE;
    }
    if (iVal < 0 || iVal >= (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FnCol) {
        return (int32)SQLITE_RANGE;
    }
    (@unsafe.Pointer.To<uintptr>(ppValue)).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FapValue + (uintptr)iVal * 8))).Value;
    return SQLITE_OK;
}

internal static void _statAccumDestructor(ж<libc.TLS> tls, uintptr pOld) {
    int32 i = default;
    uintptr p = default;
    _ = i;
    _ = p;
    p = pOld;
    if ((@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FmxSample != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnCol)) {
                break;
            }
            _sampleClear(tls, (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fdb, (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FaBest + (uintptr)i * 48);
            goto _1;
_1:;
            i = i + 1;
        }
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FmxSample)) {
                break;
            }
            _sampleClear(tls, (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fdb, (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fa + (uintptr)i * 48);
            goto _2;
_2:;
            i = i + 1;
        }
        _sampleClear(tls, (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fdb, p + 40);
    }
    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fdb, p);
}

internal static void _statClearCells(ж<libc.TLS> tls, uintptr p) {
    int32 i = default;
    _ = i;
    if ((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FaCell != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FnCell)) {
                break;
            }
            Xsqlite3_free(tls, (@unsafe.Pointer.To<TStatCell>(__ccgo_ptr((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FaCell + (uintptr)i * 32))).Value.FaOvfl);
            goto _1;
_1:;
            i = i + 1;
        }
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FaCell);
    }
    (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FnCell = 0;
    (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FaCell = (uintptr)0;
}

internal static int32 /*r*/ _subjRequiresPage(ref libc.TLS tls, uintptr pPg) {
    int32 i = default;
    uintptr p = default;
    uintptr pPager = default;
    TPgno pgno = default;
    _ = i;
    _ = p;
    _ = pPager;
    _ = pgno;
    pPager = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpPager;
    pgno = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.Fpgno;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnSavepoint)) {
            break;
        }
        p = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FaSavepoint + (uintptr)i * 56;
        if ((@unsafe.Pointer.To<TPagerSavepoint>((@unsafe.Pointer)p)).Value.FnOrig >= pgno && 0 == _sqlite3BitvecTestNotNull(ref tls, (@unsafe.Pointer.To<TPagerSavepoint>((@unsafe.Pointer)p)).Value.FpInSavepoint, pgno)) {
            i = i + (int32)1;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnSavepoint)) {
                    break;
                }
                (@unsafe.Pointer.To<TPagerSavepoint>(__ccgo_ptr((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FaSavepoint + (uintptr)i * 56))).Value.FbTruncateOnRelease = 0;
                goto _2;
_2:;
                i = i + 1;
            }
            return (int32)1;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return 0;
}

internal static void _substExprList(ж<libc.TLS> tls, @unsafe.Pointer pSubst, uintptr pList) {
    int32 i = default;
    _ = i;
    if (pList == (uintptr)0) {
        return;
    }
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr)) {
            break;
        }
        (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32))).Value.FpExpr = _substExpr(tls, pSubst, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32))).Value.FpExpr);
        goto _1;
_1:;
        i = i + 1;
    }
}

internal static int32 /*r*/ _tempTriggersExist(ref libc.TLS tls, uintptr db) {
    if ((@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + 1 * 32))).Value.FpSchema == (uintptr)0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<THash>((@unsafe.Pointer)((@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + 1 * 32))).Value.FpSchema + 56))).Value.Ffirst == (uintptr)0) {
        return 0;
    }
    return (int32)1;
}

internal static uintptr /*r*/ _termFromWhereClause(ref libc.TLS tls, uintptr pWC, int32 iTerm) {
    uintptr p = default;
    _ = p;
    p = pWC;
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        if (iTerm < (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)p)).Value.FnTerm) {
            return (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)p)).Value.Fa + (uintptr)iTerm * 56;
        }
        iTerm = iTerm - (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)p)).Value.FnTerm;
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)p)).Value.FpOuter;
    }
    return (uintptr)0;
}

internal static void _transferParseError(ж<libc.TLS> tls, uintptr pTo, @unsafe.Pointer pFrom) {
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pTo)).Value.FnErr == 0) {
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pTo)).Value.FzErrMsg = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pFrom, 8));
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pTo)).Value.FnErr = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pFrom, 52));
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pTo)).Value.Frc = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pFrom, 24));
    } else {
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pFrom)), ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pFrom, 8)));
    }
}

internal static void _unmapColumnIdlistNames(ref libc.TLS tls, uintptr pParse, uintptr pIdList) {
    int32 ii = default;
    _ = ii;
    ii = 0;
    for (;;) {
        if (!(ii < (@unsafe.Pointer.To<TIdList>((@unsafe.Pointer)pIdList)).Value.FnId)) {
            break;
        }
        _sqlite3RenameTokenRemap(ref tls, pParse, (uintptr)0, (@unsafe.Pointer.To<TIdList_item>((@unsafe.Pointer)(pIdList + 8 + (uintptr)ii * 8))).Value.FzName);
        goto _1;
_1:;
        ii = ii + 1;
    }
}

internal static int32 /*r*/ _vdbeCloseStatement(ж<libc.TLS> tls, uintptr p, int32 eOp) {
    uintptr db = default;
    uintptr pBt = default;
    int32 i = default;
    int32 iSavepoint = default;
    int32 rc = default;
    int32 rc2 = default;
    _ = db;
    _ = i;
    _ = iSavepoint;
    _ = pBt;
    _ = rc;
    _ = rc2;
    db = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb;
    rc = SQLITE_OK;
    iSavepoint = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FiStatement - (int32)1;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
            break;
        }
        rc2 = SQLITE_OK;
        pBt = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)i * 32))).Value.FpBt;
        if (pBt != 0) {
            if (eOp == (int32)SAVEPOINT_ROLLBACK) {
                rc2 = _sqlite3BtreeSavepoint(tls, pBt, (int32)SAVEPOINT_ROLLBACK, iSavepoint);
            }
            if (rc2 == SQLITE_OK) {
                rc2 = _sqlite3BtreeSavepoint(tls, pBt, (int32)SAVEPOINT_RELEASE, iSavepoint);
            }
            if (rc == SQLITE_OK) {
                rc = rc2;
            }
        }
        goto _1;
_1:;
        i = i + 1;
    }
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnStatement = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnStatement - 1;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FiStatement = 0;
    if (rc == SQLITE_OK) {
        if (eOp == (int32)SAVEPOINT_ROLLBACK) {
            rc = _sqlite3VtabSavepoint(tls, db, (int32)SAVEPOINT_ROLLBACK, iSavepoint);
        }
        if (rc == SQLITE_OK) {
            rc = _sqlite3VtabSavepoint(tls, db, (int32)SAVEPOINT_RELEASE, iSavepoint);
        }
    }
    if (eOp == (int32)SAVEPOINT_ROLLBACK) {
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDeferredCons = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnStmtDefCons;
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDeferredImmCons = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnStmtDefImmCons;
    }
    return rc;
}

internal static int32 /*r*/ _vdbeCompareMemStringWithEncodingChange(ж<libc.TLS> tls, uintptr pMem1, uintptr pMem2, uintptr pColl, @unsafe.Pointer prcErr) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(112);
        defer(tls.Free, (nint)112, ref ᒐ);
        int32 rc = default;
        uintptr v1 = default;
        uintptr v2 = default;
        TMem _ᴛ1 = new();
        TMem _ᴛ2 = new();
        _ = rc;
        _ = v1;
        _ = v2;
        _sqlite3VdbeMemInit(ref (tls).DerefOrNull(), __ccgo_ptr(bp), (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem1)).Value.Fdb, (uint16)MEM_Null);
        _sqlite3VdbeMemInit(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 56), (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem1)).Value.Fdb, (uint16)MEM_Null);
        _sqlite3VdbeMemShallowCopy(tls, bp, pMem1, (int32)MEM_Ephem);
        _sqlite3VdbeMemShallowCopy(tls, bp + 56, pMem2, (int32)MEM_Ephem);
        v1 = _sqlite3ValueText(tls, bp, (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.Fenc);
        v2 = _sqlite3ValueText(tls, bp + 56, (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.Fenc);
        if (v1 == (uintptr)0 || v2 == (uintptr)0) {
            if (prcErr != nil) {
                (@unsafe.Pointer.To<Tu8>(prcErr)).Value = (uint8)SQLITE_NOMEM;
            }
            rc = 0;
        } else {
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32, uintptr, int32>>((@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.FxCmp)(tls, (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.FpUser, (@unsafe.Pointer.To<TMem>(__ccgo_ptr(bp))).Value.Fn, v1, (@unsafe.Pointer.To<TMem>(__ccgo_ptr(bp + 56))).Value.Fn, v2);
        }
        _sqlite3VdbeMemReleaseMalloc(tls, bp);
        _sqlite3VdbeMemReleaseMalloc(tls, bp + 56);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _vdbeFreeOpArray(ж<libc.TLS> tls, uintptr db, uintptr aOp, int32 nOp) {
    uintptr pOp = default;
    _ = pOp;
    if (aOp != 0) {
        pOp = aOp + (uintptr)(nOp - (int32)1) * 24;
        while ((int32)1 != 0) {
            if ((int32)(@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp4type <= -(int32)7) {
                _freeP4(tls, db, (int32)(@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp4type, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOp + 16)));
            }
            if (pOp == aOp) {
                break;
            }
            pOp -= 24;
        }
        _sqlite3DbNNFreeNN(tls, (@unsafe.Pointer)db, aOp);
    }
}

internal static void _vdbeFreeUnpacked(ж<libc.TLS> tls, uintptr db, int32 nField, uintptr p) {
    int32 i = default;
    uintptr pMem = default;
    _ = i;
    _ = pMem;
    if (p != 0) {
        i = 0;
        for (;;) {
            if (!(i < nField)) {
                break;
            }
            pMem = (@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)p)).Value.FaMem + (uintptr)i * 56;
            if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc != 0) {
                _sqlite3VdbeMemReleaseMalloc(tls, pMem);
            }
            goto _1;
_1:;
            i = i + 1;
        }
        _sqlite3DbNNFreeNN(tls, (@unsafe.Pointer)db, p);
    }
}

internal static void _vdbeIncrFree(ж<libc.TLS> tls, uintptr pIncr) {
    if (pIncr != 0) {
        if ((@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FbUseThread != 0) {
            _vdbeSorterJoinThread(tls, (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FpTask);
            if ((@unsafe.Pointer.To<TSorterFile>(__ccgo_ptr(pIncr + 40))).Value.FpFd != 0) {
                _sqlite3OsCloseFree(tls, (@unsafe.Pointer.To<TSorterFile>(__ccgo_ptr(pIncr + 40))).Value.FpFd);
            }
            if ((@unsafe.Pointer.To<TSorterFile>(__ccgo_ptr(pIncr + 40 + 1 * 16))).Value.FpFd != 0) {
                _sqlite3OsCloseFree(tls, (@unsafe.Pointer.To<TSorterFile>(__ccgo_ptr(pIncr + 40 + 1 * 16))).Value.FpFd);
            }
        }
        _vdbeMergeEngineFree(tls, (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FpMerger);
        Xsqlite3_free(tls, pIncr);
    }
}

internal static int32 /*r*/ _vdbeIncrMergerNew(ж<libc.TLS> tls, uintptr pTask, uintptr pMerger, @unsafe.Pointer ppOut) {
    uintptr pIncr = default;
    uintptr v1 = default;
    uintptr v2 = default;
    int32 rc = default;
    int32 v3 = default;
    _ = pIncr;
    _ = rc;
    _ = v1;
    _ = v2;
    _ = v3;
    rc = SQLITE_OK;
    if (_sqlite3FaultSim(tls, (int32)100) != 0) {
        v2 = (uintptr)0;
    } else {
        v2 = _sqlite3MallocZero(tls, (uint64)72);
    }
    v1 = v2;
    (@unsafe.Pointer.To<uintptr>(ppOut)).Value = v1;
    pIncr = v1;
    if (pIncr != 0) {
        (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FpMerger = pMerger;
        (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FpTask = pTask;
        if ((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)(@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpSorter)).Value.FmxKeysize + (int32)9 > (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)(@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpSorter)).Value.FmxPmaSize / (int32)2) {
            v3 = (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)(@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpSorter)).Value.FmxKeysize + (int32)9;
        } else {
            v3 = (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)(@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpSorter)).Value.FmxPmaSize / (int32)2;
        }
        (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FmxSz = v3;
        (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile2.FiEof += (int64)(@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FmxSz;
    } else {
        _vdbeMergeEngineFree(tls, pMerger);
        rc = (int32)SQLITE_NOMEM;
    }
    return rc;
}

internal static uintptr /*r*/ _vdbeIncrPopulateThread(ж<libc.TLS> tls, uintptr pCtx) {
    uintptr pIncr = default;
    uintptr pRet = default;
    _ = pIncr;
    _ = pRet;
    pIncr = pCtx;
    pRet = (uintptr)(int64)_vdbeIncrPopulate(tls, pIncr);
    (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)(@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FpTask)).Value.FbDone = (int32)1;
    return pRet;
}

internal static int32 /*r*/ _vdbeIncrSwap(ж<libc.TLS> tls, uintptr pIncr) {
    TSorterFile f0 = default;
    int32 rc = default;
    _ = f0;
    _ = rc;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FbUseThread != 0) {
        rc = _vdbeSorterJoinThread(tls, (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FpTask);
        if (rc == SQLITE_OK) {
            f0 = ~@unsafe.Pointer.To<TSorterFile>(__ccgo_ptr(pIncr + 40));
            (@unsafe.Pointer.To<TSorterFile>(__ccgo_ptr(pIncr + 40))).Value = (@unsafe.Pointer.To<TSorterFile>(__ccgo_ptr(pIncr + 40 + 1 * 16))).Value;
            (@unsafe.Pointer.To<TSorterFile>(__ccgo_ptr(pIncr + 40 + 1 * 16))).Value = f0;
        }
        if (rc == SQLITE_OK) {
            if ((@unsafe.Pointer.To<TSorterFile>(__ccgo_ptr(pIncr + 40))).Value.FiEof == (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FiStartOff) {
                (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FbEof = (int32)1;
            } else {
                rc = _vdbeIncrBgPopulate(tls, pIncr);
            }
        }
    } else {
        rc = _vdbeIncrPopulate(tls, pIncr);
        (@unsafe.Pointer.To<TSorterFile>(__ccgo_ptr(pIncr + 40))).Value = (@unsafe.Pointer.To<TSorterFile>(__ccgo_ptr(pIncr + 40 + 1 * 16))).Value;
        if ((@unsafe.Pointer.To<TSorterFile>(__ccgo_ptr(pIncr + 40))).Value.FiEof == (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FiStartOff) {
            (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FbEof = (int32)1;
        }
    }
    return rc;
}

internal static void _vdbeLeave(ж<libc.TLS> tls, uintptr p) {
    uintptr aDb = default;
    uintptr db = default;
    int32 i = default;
    int32 nDb = default;
    _ = aDb;
    _ = db;
    _ = i;
    _ = nDb;
    db = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb;
    aDb = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb;
    nDb = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb;
    i = 0;
    for (;;) {
        if (!(i < nDb)) {
            break;
        }
        if (i != (int32)1 && (TyDbMask)((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FlockMask & (libc.Uint32FromInt32(1).Lsh((int64)(i)))) != (uint32)0 && (@unsafe.Pointer.To<TDb>(__ccgo_ptr(aDb + (uintptr)i * 32))).Value.FpBt != (uintptr)0) {
            _sqlite3BtreeLeave(tls, (@unsafe.Pointer.To<TDb>(__ccgo_ptr(aDb + (uintptr)i * 32))).Value.FpBt);
        }
        goto _1;
_1:;
        i = i + 1;
    }
}

internal static void _vdbeMergeEngineCompare(ж<libc.TLS> tls, uintptr pMerger, int32 iOut) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 i1 = default;
        int32 i2 = default;
        int32 iRes = default;
        int32 res = default;
        uintptr p1 = default;
        uintptr p2 = default;
        uintptr pTask = default;
        int32 _ᴛ1 = default;
        _ = i1;
        _ = i2;
        _ = iRes;
        _ = p1;
        _ = p2;
        _ = pTask;
        _ = res;
        if (iOut >= (@unsafe.Pointer.To<TMergeEngine>((@unsafe.Pointer)pMerger)).Value.FnTree / (int32)2) {
            i1 = (iOut - (@unsafe.Pointer.To<TMergeEngine>((@unsafe.Pointer)pMerger)).Value.FnTree / (int32)2) * (int32)2;
            i2 = i1 + (int32)1;
        } else {
            i1 = ~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TMergeEngine>((@unsafe.Pointer)pMerger)).Value.FaTree + (uintptr)(iOut * (int32)2) * 4));
            i2 = ~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TMergeEngine>((@unsafe.Pointer)pMerger)).Value.FaTree + (uintptr)(iOut * (int32)2 + (int32)1) * 4));
        }
        p1 = (@unsafe.Pointer.To<TMergeEngine>((@unsafe.Pointer)pMerger)).Value.FaReadr + (uintptr)i1 * 80;
        p2 = (@unsafe.Pointer.To<TMergeEngine>((@unsafe.Pointer)pMerger)).Value.FaReadr + (uintptr)i2 * 80;
        if ((@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p1)).Value.FpFd == (uintptr)0) {
            iRes = i2;
        } else {
            if ((@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p2)).Value.FpFd == (uintptr)0) {
                iRes = i1;
            } else {
                pTask = (@unsafe.Pointer.To<TMergeEngine>((@unsafe.Pointer)pMerger)).Value.FpTask;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = 0;
                res = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, uintptr, int32, uintptr, int32, int32>>((@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FxCompare)(tls, pTask, bp, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p1)).Value.FaKey, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p1)).Value.FnKey, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p2)).Value.FaKey, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p2)).Value.FnKey);
                if (res <= 0) {
                    iRes = i1;
                } else {
                    iRes = i2;
                }
            }
        }
        (@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TMergeEngine>((@unsafe.Pointer)pMerger)).Value.FaTree + (uintptr)iOut * 4))).Value = iRes;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _vdbeMergeEngineFree(ж<libc.TLS> tls, uintptr pMerger) {
    int32 i = default;
    _ = i;
    if (pMerger != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TMergeEngine>((@unsafe.Pointer)pMerger)).Value.FnTree)) {
                break;
            }
            _vdbePmaReaderClear(tls, (@unsafe.Pointer.To<TMergeEngine>((@unsafe.Pointer)pMerger)).Value.FaReadr + (uintptr)i * 80);
            goto _1;
_1:;
            i = i + 1;
        }
    }
    Xsqlite3_free(tls, pMerger);
}

internal static int32 /*r*/ _vdbeMergeEngineLevel0(ж<libc.TLS> tls, uintptr pTask, int32 nPMA, @unsafe.Pointer piOffset, @unsafe.Pointer ppOut) {
    int32 i = default;
    int32 rc = default;
    Ti64 iOff = default;
    ref var nDummy = ref heap(new Ti64(), out var ᏑnDummy);
    uintptr pNew = default;
    uintptr pReadr = default;
    uintptr v1 = default;
    _ = i;
    _ = iOff;
    _ = nDummy;
    _ = pNew;
    _ = pReadr;
    _ = rc;
    _ = v1;
    iOff = ~@unsafe.Pointer.To<Ti64>(piOffset);
    rc = SQLITE_OK;
    v1 = _vdbeMergeEngineNew(tls, nPMA);
    pNew = v1;
    (@unsafe.Pointer.To<uintptr>(ppOut)).Value = v1;
    if (pNew == (uintptr)0) {
        rc = (int32)SQLITE_NOMEM;
    }
    i = 0;
    for (;;) {
        if (!(i < nPMA && rc == SQLITE_OK)) {
            break;
        }
        nDummy = 0;
        pReadr = (@unsafe.Pointer.To<TMergeEngine>((@unsafe.Pointer)pNew)).Value.FaReadr + (uintptr)i * 80;
        rc = _vdbePmaReaderInit(tls, pTask, pTask + 64, iOff, pReadr, @unsafe.Pointer.FromPinnedBox(ᏑnDummy));
        iOff = (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)pReadr)).Value.FiEof;
        goto _2;
_2:;
        i = i + 1;
    }
    if (rc != SQLITE_OK) {
        _vdbeMergeEngineFree(tls, pNew);
        (@unsafe.Pointer.To<uintptr>(ppOut)).Value = (uintptr)0;
    }
    (@unsafe.Pointer.To<Ti64>(piOffset)).Value = iOff;
    return rc;
}

internal static uintptr /*r*/ _vdbePmaReaderBgIncrInit(ж<libc.TLS> tls, uintptr pCtx) {
    uintptr pReader = default;
    uintptr pRet = default;
    _ = pReader;
    _ = pRet;
    pReader = pCtx;
    pRet = (uintptr)(int64)_vdbePmaReaderIncrMergeInit(tls, pReader, (int32)INCRINIT_TASK);
    (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)(@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)(@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)pReader)).Value.FpIncr)).Value.FpTask)).Value.FbDone = (int32)1;
    return pRet;
}

internal static int32 /*r*/ _vdbePmaReaderIncrMergeInit(ж<libc.TLS> tls, uintptr pReadr, int32 eMode) {
    uintptr db = default;
    uintptr pIncr = default;
    uintptr pTask = default;
    int32 mxSz = default;
    int32 rc = default;
    _ = db;
    _ = mxSz;
    _ = pIncr;
    _ = pTask;
    _ = rc;
    rc = SQLITE_OK;
    pIncr = (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)pReadr)).Value.FpIncr;
    pTask = (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FpTask;
    db = (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)(@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpSorter)).Value.Fdb;
    rc = _vdbeMergeEngineInit(tls, pTask, (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FpMerger, eMode);
    if (rc == SQLITE_OK) {
        mxSz = (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FmxSz;
        if ((@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FbUseThread != 0) {
            rc = _vdbeSorterOpenTempFile(tls, db, (int64)mxSz, pIncr + 40);
            if (rc == SQLITE_OK) {
                rc = _vdbeSorterOpenTempFile(tls, db, (int64)mxSz, pIncr + 40 + 1 * 16);
            }
        } else {
            if ((@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile2.FpFd == (uintptr)0) {
                rc = _vdbeSorterOpenTempFile(tls, db, (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile2.FiEof, pTask + 80);
                (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile2.FiEof = 0;
            }
            if (rc == SQLITE_OK) {
                (@unsafe.Pointer.To<TSorterFile>(__ccgo_ptr(pIncr + 40 + 1 * 16))).Value.FpFd = (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile2.FpFd;
                (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FiStartOff = (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile2.FiEof;
                (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile2.FiEof += (int64)mxSz;
            }
        }
    }
    if (rc == SQLITE_OK && (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)pIncr)).Value.FbUseThread != 0) {
        rc = _vdbeIncrPopulate(tls, pIncr);
    }
    if (rc == SQLITE_OK && (libc.Bool(false) || eMode != (int32)INCRINIT_TASK)) {
        rc = _vdbePmaReaderNext(tls, pReadr);
    }
    return rc;
}

internal static int32 /*r*/ _vdbeSorterAddToTree(ж<libc.TLS> tls, uintptr pTask, int32 nDepth, int32 iSeq, uintptr pRoot, uintptr pLeaf) {
    int32 i = default;
    int32 iIter = default;
    int32 nDiv = default;
    int32 rc = default;
    uintptr p = default;
    ref var pIncr = ref heap(new uintptr(), out var ᏑpIncr);
    uintptr pNew = default;
    uintptr pReadr = default;
    _ = i;
    _ = iIter;
    _ = nDiv;
    _ = p;
    _ = pIncr;
    _ = pNew;
    _ = pReadr;
    _ = rc;
    rc = SQLITE_OK;
    nDiv = (int32)1;
    p = pRoot;
    rc = _vdbeIncrMergerNew(tls, pTask, pLeaf, @unsafe.Pointer.FromBox(ᏑpIncr));
    i = (int32)1;
    for (;;) {
        if (!(i < nDepth)) {
            break;
        }
        nDiv = nDiv * (int32)SORTER_MAX_MERGE_COUNT;
        goto _1;
_1:;
        i = i + 1;
    }
    i = (int32)1;
    for (;;) {
        if (!(i < nDepth && rc == SQLITE_OK)) {
            break;
        }
        iIter = quo(iSeq, nDiv) % (int32)SORTER_MAX_MERGE_COUNT;
        pReadr = (@unsafe.Pointer.To<TMergeEngine>((@unsafe.Pointer)p)).Value.FaReadr + (uintptr)iIter * 80;
        if ((@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)pReadr)).Value.FpIncr == (uintptr)0) {
            pNew = _vdbeMergeEngineNew(tls, (int32)SORTER_MAX_MERGE_COUNT);
            if (pNew == (uintptr)0) {
                rc = (int32)SQLITE_NOMEM;
            } else {
                rc = _vdbeIncrMergerNew(tls, pTask, pNew, (@unsafe.Pointer)(pReadr + 72));
            }
        }
        if (rc == SQLITE_OK) {
            p = (@unsafe.Pointer.To<TIncrMerger>((@unsafe.Pointer)(@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)pReadr)).Value.FpIncr)).Value.FpMerger;
            nDiv = nDiv / (int32)SORTER_MAX_MERGE_COUNT;
        }
        goto _2;
_2:;
        i = i + 1;
    }
    if (rc == SQLITE_OK) {
        (@unsafe.Pointer.To<TPmaReader>(__ccgo_ptr((@unsafe.Pointer.To<TMergeEngine>((@unsafe.Pointer)p)).Value.FaReadr + (uintptr)(iSeq % (int32)SORTER_MAX_MERGE_COUNT) * 80))).Value.FpIncr = pIncr;
    } else {
        _vdbeIncrFree(tls, pIncr);
    }
    return rc;
}

internal static uintptr /*r*/ _vdbeSorterFlushThread(ж<libc.TLS> tls, uintptr pCtx) {
    uintptr pTask = default;
    int32 rc = default;
    _ = pTask;
    _ = rc;
    pTask = pCtx;
    rc = _vdbeSorterListToPMA(tls, pTask, pTask + 32);
    (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FbDone = (int32)1;
    return (uintptr)(int64)rc;
}

internal static uintptr /*r*/ _vdbeSorterMerge(ж<libc.TLS> tls, uintptr pTask, uintptr p1, uintptr p2) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        ref var pFinal = ref heap(new uintptr(), out var ᏑpFinal);
        @unsafe.Pointer pp = default;
        int32 res = default;
        int32 _ᴛ1 = default;
        _ = pFinal;
        _ = pp;
        _ = res;
        pFinal = (uintptr)0;
        pp = @unsafe.Pointer.FromBox(ᏑpFinal);
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = 0;
        for (;;) {
            res = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, uintptr, int32, uintptr, int32, int32>>((@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FxCompare)(tls, pTask, bp, p1 + libc.UintptrFromInt32(1) * 16, (@unsafe.Pointer.To<TSorterRecord>((@unsafe.Pointer)p1)).Value.FnVal, p2 + libc.UintptrFromInt32(1) * 16, (@unsafe.Pointer.To<TSorterRecord>((@unsafe.Pointer)p2)).Value.FnVal);
            if (res <= 0) {
                (@unsafe.Pointer.To<uintptr>(pp)).Value = p1;
                pp = (@unsafe.Pointer)(p1 + 8);
                p1 = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p1 + 8));
                if (p1 == (uintptr)0) {
                    (@unsafe.Pointer.To<uintptr>(pp)).Value = p2;
                    break;
                }
            } else {
                (@unsafe.Pointer.To<uintptr>(pp)).Value = p2;
                pp = (@unsafe.Pointer)(p2 + 8);
                p2 = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p2 + 8));
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = 0;
                if (p2 == (uintptr)0) {
                    (@unsafe.Pointer.To<uintptr>(pp)).Value = p1;
                    break;
                }
            }
            goto _1;
_1:;
        }
        r = pFinal;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _vdbeSorterRecordFree(ж<libc.TLS> tls, uintptr db, uintptr pRecord) {
    uintptr p = default;
    uintptr pNext = default;
    _ = p;
    _ = pNext;
    p = pRecord;
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        pNext = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8));
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, p);
        goto _1;
_1:;
        p = pNext;
    }
}

internal static uintptr /*r*/ _vdbeSorterRowkey(ref libc.TLS tls, uintptr pSorter, @unsafe.Pointer pnKey) {
    uintptr pKey = default;
    uintptr pReader = default;
    _ = pKey;
    _ = pReader;
    if ((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FbUsePMA != 0) {
        if ((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FbUseThreads != 0) {
            pReader = (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FpReader;
        } else {
            pReader = (@unsafe.Pointer.To<TMergeEngine>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FpMerger)).Value.FaReadr + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TMergeEngine>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FpMerger)).Value.FaTree + 1 * 4))) * 80;
        }
        (@unsafe.Pointer.To<int32>(pnKey)).Value = (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)pReader)).Value.FnKey;
        pKey = (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)pReader)).Value.FaKey;
    } else {
        (@unsafe.Pointer.To<int32>(pnKey)).Value = (@unsafe.Pointer.To<TSorterRecord>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FpList)).Value.FnVal;
        pKey = (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FpList + libc.UintptrFromInt32(1) * 16;
    }
    return pKey;
}

internal static uintptr /*r*/ _vtabDisconnectAll(ref libc.TLS tls, uintptr db, uintptr p) {
    uintptr db2 = default;
    uintptr pNext = default;
    uintptr pRet = default;
    uintptr pVTable = default;
    _ = db2;
    _ = pNext;
    _ = pRet;
    _ = pVTable;
    pRet = (uintptr)0;
    pVTable = (@unsafe.Pointer.To<TTableᴛ1_Fu_Fvtab>((@unsafe.Pointer)(p + 64))).Value.Fp;
    (@unsafe.Pointer.To<TTableᴛ1_Fu_Fvtab>((@unsafe.Pointer)(p + 64))).Value.Fp = (uintptr)0;
    while (pVTable != 0) {
        db2 = (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVTable)).Value.Fdb;
        pNext = (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVTable)).Value.FpNext;
        if (db2 == db) {
            pRet = pVTable;
            (@unsafe.Pointer.To<TTableᴛ1_Fu_Fvtab>((@unsafe.Pointer)(p + 64))).Value.Fp = pRet;
            (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pRet)).Value.FpNext = (uintptr)0;
        } else {
            (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)pVTable)).Value.FpNext = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db2)).Value.FpDisconnect;
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db2)).Value.FpDisconnect = pVTable;
        }
        pVTable = pNext;
    }
    return pRet;
}

internal static uintptr /*r*/ _walCkptInfo(ref libc.TLS tls, uintptr pWal) {
    _modernc_seh_inject(ref tls, pWal);
    return ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData)) + (uintptr)(libc.Uint64FromInt64(48) / libc.Uint64FromInt32(2)) * 4;
}

internal static Tu32 /*r*/ _walFramePgno(ref libc.TLS tls, uintptr pWal, Tu32 iFrame) {
    int32 iHash = default;
    _ = iHash;
    iHash = _walFramePage(ref tls, iFrame);
    _modernc_seh_inject(ref tls, pWal);
    if (iHash == 0) {
        return ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData)) + (uintptr)((libc.Uint64FromInt64(48) * libc.Uint64FromInt32(2) + libc.Uint64FromInt64(40)) / libc.Uint64FromInt64(4) + (uint64)iFrame - (uint64)1) * 4));
    }
    return ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData + (uintptr)iHash * 8)) + (uintptr)(((uint64)(iFrame - (uint32)1) - (libc.Uint64FromInt32(HASHTABLE_NPAGE) - (libc.Uint64FromInt64(48) * libc.Uint64FromInt32(2) + libc.Uint64FromInt64(40)) / libc.Uint64FromInt64(4))) % (uint64)HASHTABLE_NPAGE) * 4));
}

internal static int32 /*r*/ _walIndexPage(ж<libc.TLS> tls, uintptr pWal, int32 iPage, uintptr ppPage) {
    uintptr v1 = default;
    bool v2 = default;
    _ = v1;
    _ = v2;
    _modernc_seh_inject(ref (tls).DerefOrNull(), pWal);
    v2 = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FnWiData <= iPage; if (!v2) {
        v1 = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData + (uintptr)iPage * 8));
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppPage))).Value = v1;
    }
    if (v2 || v1 == (uintptr)0) {
        return _walIndexPageRealloc(tls, pWal, iPage, ppPage);
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _walSehBeginWriteCheck(ж<libc.TLS> tls, uintptr pWal, uintptr pNotUsed) {
    _ = pNotUsed;
    if (libc.Xmemcmp(tls, pWal + 72, _walIndexHdr(ref (tls).DerefOrNull(), pWal), (uint64)48) != 0) {
        return libc.Int32FromInt32(SQLITE_BUSY) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8)));
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _walSehSnapshotCheck(ж<libc.TLS> tls, uintptr pWal, uintptr p) {
    uintptr pNew = default;
    int32 rc = default;
    _ = pNew;
    _ = rc;
    rc = _walLockShared(tls, pWal, (int32)WAL_CKPT_LOCK);
    if (rc == SQLITE_OK) {
        pNew = p;
        if (libc.Xmemcmp(tls, pNew + 32, pWal + 72 + 32, (uint64)8) != 0 || (@unsafe.Pointer.To<TWalIndexHdr>((@unsafe.Pointer)pNew)).Value.FmxFrame < (@unsafe.Pointer.To<TWalCkptInfo>((@unsafe.Pointer)_walCkptInfo(ref (tls).DerefOrNull(), pWal))).Value.FnBackfillAttempted) {
            rc = libc.Int32FromInt32(SQLITE_ERROR) | libc.Int32FromInt32(3).Lsh((int64)(libc.Int32FromInt32(8)));
            _walUnlockShared(tls, pWal, (int32)WAL_CKPT_LOCK);
        }
    }
    return rc;
}

internal static int32 /*r*/ _walSehSnapshotRecover(ж<libc.TLS> tls, uintptr pWal, uintptr p) {
    uintptr aArg = default;
    _ = aArg;
    aArg = p;
    return _walSnapshotRecover(tls, pWal, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aArg)), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aArg + 1 * 8)));
}

internal static int32 /*r*/ _walWriteToLog(ж<libc.TLS> tls, @unsafe.Pointer p, uintptr pContent, int32 iAmt, Tsqlite3_int64 iOffset) {
    int32 iFirstAmt = default;
    int32 rc = default;
    _ = iFirstAmt;
    _ = rc;
    if (iOffset < ~@unsafe.Pointer.To<Tsqlite3_int64>(@unsafe.Add(p, 16)) && iOffset + (int64)iAmt >= ~@unsafe.Pointer.To<Tsqlite3_int64>(@unsafe.Add(p, 16))) {
        iFirstAmt = (int32)(~@unsafe.Pointer.To<Tsqlite3_int64>(@unsafe.Add(p, 16)) - iOffset);
        rc = _sqlite3OsWrite(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8)), pContent, iFirstAmt, iOffset);
        if (rc != 0) {
            return rc;
        }
        iOffset = iOffset + (int64)iFirstAmt;
        iAmt = iAmt - iFirstAmt;
        pContent = (uintptr)iFirstAmt + pContent;
        rc = _sqlite3OsSync(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8)), ~@unsafe.Pointer.To<int32>(@unsafe.Add(p, 24)) & (int32)0x03);
        if (iAmt == 0 || rc != 0) {
            return rc;
        }
    }
    rc = _sqlite3OsWrite(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8)), pContent, iAmt, iOffset);
    return rc;
}

internal static void _whereInfoFree(ж<libc.TLS> tls, uintptr db, uintptr pWInfo) {
    uintptr p = default;
    uintptr pNext = default;
    _ = p;
    _ = pNext;
    _sqlite3WhereClauseClear(tls, pWInfo + 104);
    while ((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpLoops != 0) {
        p = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpLoops;
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpLoops = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FpNextLoop;
        _whereLoopDelete(tls, db, p);
    }
    while ((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpMemToFree != 0) {
        pNext = (@unsafe.Pointer.To<TWhereMemBlock>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpMemToFree)).Value.FpNext;
        _sqlite3DbNNFreeNN(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpMemToFree);
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpMemToFree = pNext;
    }
    _sqlite3DbNNFreeNN(tls, (@unsafe.Pointer)db, pWInfo);
}

internal static int32 /*r*/ _windowArgCount(ref libc.TLS tls, uintptr pWin) {
    uintptr pList = default;
    int32 v1 = default;
    _ = pList;
    _ = v1;
    pList = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpOwner + 32));
    if (pList != 0) {
        v1 = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr;
    } else {
        v1 = 0;
    }
    return v1;
}

internal static int32 /*r*/ _windowInitAccum(ж<libc.TLS> tls, uintptr pParse, uintptr pMWin) {
    int32 nArg = default;
    int32 regArg = default;
    int32 v2 = default;
    uintptr pFunc = default;
    uintptr pWin = default;
    uintptr v = default;
    _ = nArg;
    _ = pFunc;
    _ = pWin;
    _ = regArg;
    _ = v;
    _ = v2;
    v = _sqlite3GetVdbe(tls, pParse);
    nArg = 0;
    pWin = pMWin;
    for (;;) {
        if (!(pWin != 0)) {
            break;
        }
        pFunc = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpWFunc;
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregAccum);
        if (nArg > _windowArgCount(ref (tls).DerefOrNull(), pWin)) {
            v2 = nArg;
        } else {
            v2 = _windowArgCount(ref (tls).DerefOrNull(), pWin);
        }
        nArg = v2;
        if ((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregStartRowid == 0) {
            if ((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pFunc)).Value.FzName == (uintptr)Ꮡ_nth_valueName || (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pFunc)).Value.FzName == (uintptr)Ꮡ_first_valueName) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregApp);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregApp + (int32)1);
            }
            if ((Tu32)((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pFunc)).Value.FfuncFlags & (uint32)SQLITE_FUNC_MINMAX) != 0 && (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FcsrApp != 0) {
                _sqlite3VdbeAddOp1(tls, v, (int32)OP_ResetSorter, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FcsrApp);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregApp + (int32)1);
            }
        }
        goto _1;
_1:;
        pWin = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpNextWin;
    }
    regArg = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += nArg;
    return regArg;
}

internal static void _windowReadPeerValues(ж<libc.TLS> tls, @unsafe.Pointer p, int32 csr, int32 reg) {
    int32 i = default;
    int32 iColOff = default;
    int32 v1 = default;
    uintptr pMWin = default;
    uintptr pOrderBy = default;
    uintptr pPart = default;
    uintptr v = default;
    _ = i;
    _ = iColOff;
    _ = pMWin;
    _ = pOrderBy;
    _ = pPart;
    _ = v;
    _ = v1;
    pMWin = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8));
    pOrderBy = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpOrderBy;
    if (pOrderBy != 0) {
        v = _sqlite3GetVdbe(tls, ~@unsafe.Pointer.To<uintptr>(p));
        pPart = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpPartition;
        if (pPart != 0) {
            v1 = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pPart)).Value.FnExpr;
        } else {
            v1 = 0;
        }
        iColOff = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FnBufferCol + v1;
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr)) {
                break;
            }
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, csr, iColOff + i, reg + i);
            goto _2;
_2:;
            i = i + 1;
        }
    }
}

internal static void _windowReturnOneRow(ж<libc.TLS> tls, @unsafe.Pointer p) {
    int32 csr = default;
    int32 csr1 = default;
    int32 iEph = default;
    int32 lbl = default;
    int32 lbl1 = default;
    int32 nArg = default;
    int32 op = default;
    int32 tmpReg = default;
    int32 tmpReg1 = default;
    int32 tmpReg2 = default;
    int32 val = default;
    int32 v2 = default;
    uintptr pFunc = default;
    uintptr pMWin = default;
    uintptr pParse = default;
    uintptr pWin = default;
    uintptr v = default;
    _ = csr;
    _ = csr1;
    _ = iEph;
    _ = lbl;
    _ = lbl1;
    _ = nArg;
    _ = op;
    _ = pFunc;
    _ = pMWin;
    _ = pParse;
    _ = pWin;
    _ = tmpReg;
    _ = tmpReg1;
    _ = tmpReg2;
    _ = v;
    _ = val;
    _ = v2;
    pMWin = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8));
    v = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16));
    if ((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregStartRowid != 0) {
        _windowFullScan(tls, p);
    } else {
        pParse = ~@unsafe.Pointer.To<uintptr>(p);
        pWin = pMWin;
        for (;;) {
            if (!(pWin != 0)) {
                break;
            }
            pFunc = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpWFunc;
            if ((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pFunc)).Value.FzName == (uintptr)Ꮡ_nth_valueName || (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pFunc)).Value.FzName == (uintptr)Ꮡ_first_valueName) {
                csr = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FcsrApp;
                lbl = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                tmpReg = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregResult);
                if ((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pFunc)).Value.FzName == (uintptr)Ꮡ_nth_valueName) {
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FiEphCsr, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FiArgCol + (int32)1, tmpReg);
                    _windowCheckValue(tls, pParse, tmpReg, (int32)2);
                } else {
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, (int32)1, tmpReg);
                }
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Add, tmpReg, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregApp, tmpReg);
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Gt, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregApp + (int32)1, lbl, tmpReg);
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_SeekRowid, csr, 0, tmpReg);
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, csr, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FiArgCol, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregResult);
                _sqlite3VdbeResolveLabel(tls, v, lbl);
                _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, tmpReg);
            } else {
                if ((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pFunc)).Value.FzName == (uintptr)Ꮡ_leadName || (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pFunc)).Value.FzName == (uintptr)Ꮡ_lagName) {
                    nArg = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpOwner + 32))))).Value.FnExpr;
                    csr1 = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FcsrApp;
                    lbl1 = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                    tmpReg1 = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
                    iEph = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FiEphCsr;
                    if (nArg < (int32)3) {
                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregResult);
                    } else {
                        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, iEph, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FiArgCol + (int32)2, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregResult);
                    }
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rowid, iEph, tmpReg1);
                    if (nArg < (int32)2) {
                        if ((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pFunc)).Value.FzName == (uintptr)Ꮡ_leadName) {
                            v2 = (int32)1;
                        } else {
                            v2 = -(int32)1;
                        }
                        val = v2;
                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_AddImm, tmpReg1, val);
                    } else {
                        if ((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pFunc)).Value.FzName == (uintptr)Ꮡ_leadName) {
                            v2 = (int32)OP_Add;
                        } else {
                            v2 = (int32)OP_Subtract;
                        }
                        op = v2;
                        tmpReg2 = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
                        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, iEph, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FiArgCol + (int32)1, tmpReg2);
                        _sqlite3VdbeAddOp3(tls, v, op, tmpReg2, tmpReg1, tmpReg1);
                        _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, tmpReg2);
                    }
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_SeekRowid, csr1, lbl1, tmpReg1);
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, csr1, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FiArgCol, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregResult);
                    _sqlite3VdbeResolveLabel(tls, v, lbl1);
                    _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, tmpReg1);
                }
            }
            goto _1;
_1:;
            pWin = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpNextWin;
        }
    }
    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Gosub, ~@unsafe.Pointer.To<int32>(@unsafe.Add(p, 28)), ~@unsafe.Pointer.To<int32>(@unsafe.Add(p, 24)));
}

internal static void _yy_pop_parser_stack(ж<libc.TLS> tls, uintptr pParser) {
    uintptr yytos = default;
    uintptr v1 = default;
    uintptr v2 = default;
    _ = yytos;
    _ = v1;
    _ = v2;
    v2 = pParser;
    v1 = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)v2);
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)v2)).Value -= 24;
    yytos = v1;
    _yy_destructor(tls, pParser, (@unsafe.Pointer.To<TyyStackEntry>((@unsafe.Pointer)yytos)).Value.Fmajor, (@unsafe.Pointer)(yytos + 8));
}

[GoInit] internal static void initΔ1() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_defaultMethods);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 0))).Value = __ccgo_fp(_sqlite3MemMalloc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(_sqlite3MemFree);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_sqlite3MemRealloc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_sqlite3MemSize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_sqlite3MemRoundup);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_sqlite3MemInit);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_sqlite3MemShutdown);
}

[GoInit] internal static void initΔ2() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_statPushFuncdef);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_statPush);
}

[GoInit] internal static void initΔ3() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_sFts5Api);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(_fts5ApiUserData);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_fts5ApiColumnCount);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_fts5ApiRowCount);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_fts5ApiColumnTotalSize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_fts5ApiTokenize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_fts5ApiPhraseCount);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_fts5ApiPhraseSize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_fts5ApiInstCount);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_fts5ApiInst);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_fts5ApiRowid);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(_fts5ApiColumnText);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_fts5ApiColumnSize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 104))).Value = __ccgo_fp(_fts5ApiQueryPhrase);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 112))).Value = __ccgo_fp(_fts5ApiSetAuxdata);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 120))).Value = __ccgo_fp(_fts5ApiGetAuxdata);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 128))).Value = __ccgo_fp(_fts5ApiPhraseFirst);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 136))).Value = __ccgo_fp(_fts5ApiPhraseNext);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 144))).Value = __ccgo_fp(_fts5ApiPhraseFirstColumn);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 152))).Value = __ccgo_fp(_fts5ApiPhraseNextColumn);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 160))).Value = __ccgo_fp(_fts5ApiQueryToken);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 168))).Value = __ccgo_fp(_fts5ApiInstToken);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 176))).Value = __ccgo_fp(_fts5ApiColumnLocale);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 184))).Value = __ccgo_fp(_fts5ApiTokenize_v2);
}

[GoInit] internal static void initΔ4() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_aAgg);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 0))).Value = __ccgo_fp(_geopolyBBoxStep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(_geopolyBBoxFinal);
}

[GoInit] internal static void initΔ5() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_aFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 0))).Value = __ccgo_fp(_geopolyAreaFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_geopolyBlobFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_geopolyJsonFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_geopolySvgFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_geopolyWithinFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 120))).Value = __ccgo_fp(_geopolyContainsPointFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 144))).Value = __ccgo_fp(_geopolyOverlapFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 168))).Value = __ccgo_fp(_geopolyDebugFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 192))).Value = __ccgo_fp(_geopolyBBoxFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 216))).Value = __ccgo_fp(_geopolyXformFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 240))).Value = __ccgo_fp(_geopolyRegularFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 264))).Value = __ccgo_fp(_geopolyCcwFunc);
}

[GoInit] internal static void initΔ6() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_attach_func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_attachFunc);
}

[GoInit] internal static void initΔ7() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_sMutex);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 0))).Value = __ccgo_fp(_noopMutexInit);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(_noopMutexEnd);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_noopMutexAlloc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_noopMutexFree);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_noopMutexEnter);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_noopMutexTry);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_noopMutexLeave);
}

[GoInit] internal static void initΔ8() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_aAlterTableFuncs);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_renameColumnFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_renameTableFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 168))).Value = __ccgo_fp(_renameTableTest);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 240))).Value = __ccgo_fp(_dropColumnFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 312))).Value = __ccgo_fp(_renameQuotefixFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 384))).Value = __ccgo_fp(_dropConstraintFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 456))).Value = __ccgo_fp(_failConstraintFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 528))).Value = __ccgo_fp(_addConstraintFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 600))).Value = __ccgo_fp(_findConstraintFunc);
}

[GoInit] internal static void initΔ9() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_detach_func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_detachFunc);
}

[GoInit] internal static void initΔ10() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_sqlite3Apis);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 0))).Value = __ccgo_fp(Xsqlite3_aggregate_context);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(Xsqlite3_aggregate_count);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(Xsqlite3_bind_blob);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(Xsqlite3_bind_double);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(Xsqlite3_bind_int);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(Xsqlite3_bind_int64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(Xsqlite3_bind_null);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(Xsqlite3_bind_parameter_count);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(Xsqlite3_bind_parameter_index);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(Xsqlite3_bind_parameter_name);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(Xsqlite3_bind_text);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(Xsqlite3_bind_text16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(Xsqlite3_bind_value);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 104))).Value = __ccgo_fp(Xsqlite3_busy_handler);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 112))).Value = __ccgo_fp(Xsqlite3_busy_timeout);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 120))).Value = __ccgo_fp(Xsqlite3_changes);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 128))).Value = __ccgo_fp(Xsqlite3_close);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 136))).Value = __ccgo_fp(Xsqlite3_collation_needed);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 144))).Value = __ccgo_fp(Xsqlite3_collation_needed16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 152))).Value = __ccgo_fp(Xsqlite3_column_blob);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 160))).Value = __ccgo_fp(Xsqlite3_column_bytes);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 168))).Value = __ccgo_fp(Xsqlite3_column_bytes16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 176))).Value = __ccgo_fp(Xsqlite3_column_count);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 184))).Value = __ccgo_fp(Xsqlite3_column_database_name);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 192))).Value = __ccgo_fp(Xsqlite3_column_database_name16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 200))).Value = __ccgo_fp(Xsqlite3_column_decltype);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 208))).Value = __ccgo_fp(Xsqlite3_column_decltype16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 216))).Value = __ccgo_fp(Xsqlite3_column_double);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 224))).Value = __ccgo_fp(Xsqlite3_column_int);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 232))).Value = __ccgo_fp(Xsqlite3_column_int64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 240))).Value = __ccgo_fp(Xsqlite3_column_name);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 248))).Value = __ccgo_fp(Xsqlite3_column_name16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 256))).Value = __ccgo_fp(Xsqlite3_column_origin_name);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 264))).Value = __ccgo_fp(Xsqlite3_column_origin_name16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 272))).Value = __ccgo_fp(Xsqlite3_column_table_name);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 280))).Value = __ccgo_fp(Xsqlite3_column_table_name16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 288))).Value = __ccgo_fp(Xsqlite3_column_text);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 296))).Value = __ccgo_fp(Xsqlite3_column_text16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 304))).Value = __ccgo_fp(Xsqlite3_column_type);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 312))).Value = __ccgo_fp(Xsqlite3_column_value);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 320))).Value = __ccgo_fp(Xsqlite3_commit_hook);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 328))).Value = __ccgo_fp(Xsqlite3_complete);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 336))).Value = __ccgo_fp(Xsqlite3_complete16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 344))).Value = __ccgo_fp(Xsqlite3_create_collation);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 352))).Value = __ccgo_fp(Xsqlite3_create_collation16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 360))).Value = __ccgo_fp(Xsqlite3_create_function);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 368))).Value = __ccgo_fp(Xsqlite3_create_function16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 376))).Value = __ccgo_fp(Xsqlite3_create_module);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 384))).Value = __ccgo_fp(Xsqlite3_data_count);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 392))).Value = __ccgo_fp(Xsqlite3_db_handle);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 400))).Value = __ccgo_fp(Xsqlite3_declare_vtab);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 408))).Value = __ccgo_fp(Xsqlite3_enable_shared_cache);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 416))).Value = __ccgo_fp(Xsqlite3_errcode);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 424))).Value = __ccgo_fp(Xsqlite3_errmsg);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 432))).Value = __ccgo_fp(Xsqlite3_errmsg16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 440))).Value = __ccgo_fp(Xsqlite3_exec);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 448))).Value = __ccgo_fp(Xsqlite3_expired);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 456))).Value = __ccgo_fp(Xsqlite3_finalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 464))).Value = __ccgo_fp(Xsqlite3_free);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 472))).Value = __ccgo_fp(Xsqlite3_free_table);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 480))).Value = __ccgo_fp(Xsqlite3_get_autocommit);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 488))).Value = __ccgo_fp(Xsqlite3_get_auxdata);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 496))).Value = __ccgo_fp(Xsqlite3_get_table);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 512))).Value = __ccgo_fp(Xsqlite3_interrupt);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 520))).Value = __ccgo_fp(Xsqlite3_last_insert_rowid);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 528))).Value = __ccgo_fp(Xsqlite3_libversion);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 536))).Value = __ccgo_fp(Xsqlite3_libversion_number);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 544))).Value = __ccgo_fp(Xsqlite3_malloc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 552))).Value = __ccgo_fp(Xsqlite3_mprintf);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 560))).Value = __ccgo_fp(Xsqlite3_open);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 568))).Value = __ccgo_fp(Xsqlite3_open16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 576))).Value = __ccgo_fp(Xsqlite3_prepare);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 584))).Value = __ccgo_fp(Xsqlite3_prepare16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 592))).Value = __ccgo_fp(Xsqlite3_profile);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 600))).Value = __ccgo_fp(Xsqlite3_progress_handler);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 608))).Value = __ccgo_fp(Xsqlite3_realloc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 616))).Value = __ccgo_fp(Xsqlite3_reset);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 624))).Value = __ccgo_fp(Xsqlite3_result_blob);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 632))).Value = __ccgo_fp(Xsqlite3_result_double);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 640))).Value = __ccgo_fp(Xsqlite3_result_error);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 648))).Value = __ccgo_fp(Xsqlite3_result_error16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 656))).Value = __ccgo_fp(Xsqlite3_result_int);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 664))).Value = __ccgo_fp(Xsqlite3_result_int64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 672))).Value = __ccgo_fp(Xsqlite3_result_null);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 680))).Value = __ccgo_fp(Xsqlite3_result_text);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 688))).Value = __ccgo_fp(Xsqlite3_result_text16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 696))).Value = __ccgo_fp(Xsqlite3_result_text16be);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 704))).Value = __ccgo_fp(Xsqlite3_result_text16le);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 712))).Value = __ccgo_fp(Xsqlite3_result_value);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 720))).Value = __ccgo_fp(Xsqlite3_rollback_hook);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 728))).Value = __ccgo_fp(Xsqlite3_set_authorizer);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 736))).Value = __ccgo_fp(Xsqlite3_set_auxdata);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 744))).Value = __ccgo_fp(Xsqlite3_snprintf);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 752))).Value = __ccgo_fp(Xsqlite3_step);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 760))).Value = __ccgo_fp(Xsqlite3_table_column_metadata);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 768))).Value = __ccgo_fp(Xsqlite3_thread_cleanup);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 776))).Value = __ccgo_fp(Xsqlite3_total_changes);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 784))).Value = __ccgo_fp(Xsqlite3_trace);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 792))).Value = __ccgo_fp(Xsqlite3_transfer_bindings);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 800))).Value = __ccgo_fp(Xsqlite3_update_hook);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 808))).Value = __ccgo_fp(Xsqlite3_user_data);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 816))).Value = __ccgo_fp(Xsqlite3_value_blob);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 824))).Value = __ccgo_fp(Xsqlite3_value_bytes);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 832))).Value = __ccgo_fp(Xsqlite3_value_bytes16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 840))).Value = __ccgo_fp(Xsqlite3_value_double);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 848))).Value = __ccgo_fp(Xsqlite3_value_int);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 856))).Value = __ccgo_fp(Xsqlite3_value_int64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 864))).Value = __ccgo_fp(Xsqlite3_value_numeric_type);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 872))).Value = __ccgo_fp(Xsqlite3_value_text);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 880))).Value = __ccgo_fp(Xsqlite3_value_text16);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 888))).Value = __ccgo_fp(Xsqlite3_value_text16be);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 896))).Value = __ccgo_fp(Xsqlite3_value_text16le);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 904))).Value = __ccgo_fp(Xsqlite3_value_type);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 912))).Value = __ccgo_fp(Xsqlite3_vmprintf);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 920))).Value = __ccgo_fp(Xsqlite3_overload_function);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 928))).Value = __ccgo_fp(Xsqlite3_prepare_v2);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 936))).Value = __ccgo_fp(Xsqlite3_prepare16_v2);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 944))).Value = __ccgo_fp(Xsqlite3_clear_bindings);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 952))).Value = __ccgo_fp(Xsqlite3_create_module_v2);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 960))).Value = __ccgo_fp(Xsqlite3_bind_zeroblob);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 968))).Value = __ccgo_fp(Xsqlite3_blob_bytes);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 976))).Value = __ccgo_fp(Xsqlite3_blob_close);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 984))).Value = __ccgo_fp(Xsqlite3_blob_open);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 992))).Value = __ccgo_fp(Xsqlite3_blob_read);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1000))).Value = __ccgo_fp(Xsqlite3_blob_write);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1008))).Value = __ccgo_fp(Xsqlite3_create_collation_v2);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1016))).Value = __ccgo_fp(Xsqlite3_file_control);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1024))).Value = __ccgo_fp(Xsqlite3_memory_highwater);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1032))).Value = __ccgo_fp(Xsqlite3_memory_used);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1040))).Value = __ccgo_fp(Xsqlite3_mutex_alloc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1048))).Value = __ccgo_fp(Xsqlite3_mutex_enter);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1056))).Value = __ccgo_fp(Xsqlite3_mutex_free);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1064))).Value = __ccgo_fp(Xsqlite3_mutex_leave);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1072))).Value = __ccgo_fp(Xsqlite3_mutex_try);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1080))).Value = __ccgo_fp(Xsqlite3_open_v2);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1088))).Value = __ccgo_fp(Xsqlite3_release_memory);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1096))).Value = __ccgo_fp(Xsqlite3_result_error_nomem);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1104))).Value = __ccgo_fp(Xsqlite3_result_error_toobig);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1112))).Value = __ccgo_fp(Xsqlite3_sleep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1120))).Value = __ccgo_fp(Xsqlite3_soft_heap_limit);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1128))).Value = __ccgo_fp(Xsqlite3_vfs_find);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1136))).Value = __ccgo_fp(Xsqlite3_vfs_register);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1144))).Value = __ccgo_fp(Xsqlite3_vfs_unregister);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1152))).Value = __ccgo_fp(Xsqlite3_threadsafe);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1160))).Value = __ccgo_fp(Xsqlite3_result_zeroblob);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1168))).Value = __ccgo_fp(Xsqlite3_result_error_code);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1176))).Value = __ccgo_fp(Xsqlite3_test_control);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1184))).Value = __ccgo_fp(Xsqlite3_randomness);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1192))).Value = __ccgo_fp(Xsqlite3_context_db_handle);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1200))).Value = __ccgo_fp(Xsqlite3_extended_result_codes);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1208))).Value = __ccgo_fp(Xsqlite3_limit);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1216))).Value = __ccgo_fp(Xsqlite3_next_stmt);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1224))).Value = __ccgo_fp(Xsqlite3_sql);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1232))).Value = __ccgo_fp(Xsqlite3_status);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1240))).Value = __ccgo_fp(Xsqlite3_backup_finish);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1248))).Value = __ccgo_fp(Xsqlite3_backup_init);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1256))).Value = __ccgo_fp(Xsqlite3_backup_pagecount);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1264))).Value = __ccgo_fp(Xsqlite3_backup_remaining);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1272))).Value = __ccgo_fp(Xsqlite3_backup_step);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1280))).Value = __ccgo_fp(Xsqlite3_compileoption_get);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1288))).Value = __ccgo_fp(Xsqlite3_compileoption_used);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1296))).Value = __ccgo_fp(Xsqlite3_create_function_v2);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1304))).Value = __ccgo_fp(Xsqlite3_db_config);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1312))).Value = __ccgo_fp(Xsqlite3_db_mutex);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1320))).Value = __ccgo_fp(Xsqlite3_db_status);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1328))).Value = __ccgo_fp(Xsqlite3_extended_errcode);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1336))).Value = __ccgo_fp(Xsqlite3_log);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1344))).Value = __ccgo_fp(Xsqlite3_soft_heap_limit64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1352))).Value = __ccgo_fp(Xsqlite3_sourceid);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1360))).Value = __ccgo_fp(Xsqlite3_stmt_status);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1368))).Value = __ccgo_fp(Xsqlite3_strnicmp);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1376))).Value = __ccgo_fp(Xsqlite3_unlock_notify);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1384))).Value = __ccgo_fp(Xsqlite3_wal_autocheckpoint);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1392))).Value = __ccgo_fp(Xsqlite3_wal_checkpoint);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1400))).Value = __ccgo_fp(Xsqlite3_wal_hook);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1408))).Value = __ccgo_fp(Xsqlite3_blob_reopen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1416))).Value = __ccgo_fp(Xsqlite3_vtab_config);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1424))).Value = __ccgo_fp(Xsqlite3_vtab_on_conflict);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1432))).Value = __ccgo_fp(Xsqlite3_close_v2);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1440))).Value = __ccgo_fp(Xsqlite3_db_filename);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1448))).Value = __ccgo_fp(Xsqlite3_db_readonly);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1456))).Value = __ccgo_fp(Xsqlite3_db_release_memory);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1464))).Value = __ccgo_fp(Xsqlite3_errstr);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1472))).Value = __ccgo_fp(Xsqlite3_stmt_busy);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1480))).Value = __ccgo_fp(Xsqlite3_stmt_readonly);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1488))).Value = __ccgo_fp(Xsqlite3_stricmp);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1496))).Value = __ccgo_fp(Xsqlite3_uri_boolean);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1504))).Value = __ccgo_fp(Xsqlite3_uri_int64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1512))).Value = __ccgo_fp(Xsqlite3_uri_parameter);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1520))).Value = __ccgo_fp(Xsqlite3_vsnprintf);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1528))).Value = __ccgo_fp(Xsqlite3_wal_checkpoint_v2);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1536))).Value = __ccgo_fp(Xsqlite3_auto_extension);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1544))).Value = __ccgo_fp(Xsqlite3_bind_blob64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1552))).Value = __ccgo_fp(Xsqlite3_bind_text64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1560))).Value = __ccgo_fp(Xsqlite3_cancel_auto_extension);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1568))).Value = __ccgo_fp(Xsqlite3_load_extension);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1576))).Value = __ccgo_fp(Xsqlite3_malloc64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1584))).Value = __ccgo_fp(Xsqlite3_msize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1592))).Value = __ccgo_fp(Xsqlite3_realloc64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1600))).Value = __ccgo_fp(Xsqlite3_reset_auto_extension);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1608))).Value = __ccgo_fp(Xsqlite3_result_blob64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1616))).Value = __ccgo_fp(Xsqlite3_result_text64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1624))).Value = __ccgo_fp(Xsqlite3_strglob);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1632))).Value = __ccgo_fp(Xsqlite3_value_dup);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1640))).Value = __ccgo_fp(Xsqlite3_value_free);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1648))).Value = __ccgo_fp(Xsqlite3_result_zeroblob64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1656))).Value = __ccgo_fp(Xsqlite3_bind_zeroblob64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1664))).Value = __ccgo_fp(Xsqlite3_value_subtype);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1672))).Value = __ccgo_fp(Xsqlite3_result_subtype);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1680))).Value = __ccgo_fp(Xsqlite3_status64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1688))).Value = __ccgo_fp(Xsqlite3_strlike);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1696))).Value = __ccgo_fp(Xsqlite3_db_cacheflush);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1704))).Value = __ccgo_fp(Xsqlite3_system_errno);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1712))).Value = __ccgo_fp(Xsqlite3_trace_v2);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1720))).Value = __ccgo_fp(Xsqlite3_expanded_sql);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1728))).Value = __ccgo_fp(Xsqlite3_set_last_insert_rowid);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1736))).Value = __ccgo_fp(Xsqlite3_prepare_v3);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1744))).Value = __ccgo_fp(Xsqlite3_prepare16_v3);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1752))).Value = __ccgo_fp(Xsqlite3_bind_pointer);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1760))).Value = __ccgo_fp(Xsqlite3_result_pointer);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1768))).Value = __ccgo_fp(Xsqlite3_value_pointer);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1776))).Value = __ccgo_fp(Xsqlite3_vtab_nochange);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1784))).Value = __ccgo_fp(Xsqlite3_value_nochange);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1792))).Value = __ccgo_fp(Xsqlite3_vtab_collation);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1800))).Value = __ccgo_fp(Xsqlite3_keyword_count);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1808))).Value = __ccgo_fp(Xsqlite3_keyword_name);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1816))).Value = __ccgo_fp(Xsqlite3_keyword_check);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1824))).Value = __ccgo_fp(Xsqlite3_str_new);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1832))).Value = __ccgo_fp(Xsqlite3_str_finish);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1840))).Value = __ccgo_fp(Xsqlite3_str_appendf);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1848))).Value = __ccgo_fp(Xsqlite3_str_vappendf);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1856))).Value = __ccgo_fp(Xsqlite3_str_append);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1864))).Value = __ccgo_fp(Xsqlite3_str_appendall);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1872))).Value = __ccgo_fp(Xsqlite3_str_appendchar);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1880))).Value = __ccgo_fp(Xsqlite3_str_reset);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1888))).Value = __ccgo_fp(Xsqlite3_str_errcode);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1896))).Value = __ccgo_fp(Xsqlite3_str_length);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1904))).Value = __ccgo_fp(Xsqlite3_str_value);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1912))).Value = __ccgo_fp(Xsqlite3_create_window_function);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1928))).Value = __ccgo_fp(Xsqlite3_stmt_isexplain);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1936))).Value = __ccgo_fp(Xsqlite3_value_frombind);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1944))).Value = __ccgo_fp(Xsqlite3_drop_modules);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1952))).Value = __ccgo_fp(Xsqlite3_hard_heap_limit64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1960))).Value = __ccgo_fp(Xsqlite3_uri_key);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1968))).Value = __ccgo_fp(Xsqlite3_filename_database);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1976))).Value = __ccgo_fp(Xsqlite3_filename_journal);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1984))).Value = __ccgo_fp(Xsqlite3_filename_wal);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1992))).Value = __ccgo_fp(Xsqlite3_create_filename);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2000))).Value = __ccgo_fp(Xsqlite3_free_filename);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2008))).Value = __ccgo_fp(Xsqlite3_database_file_object);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2016))).Value = __ccgo_fp(Xsqlite3_txn_state);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2024))).Value = __ccgo_fp(Xsqlite3_changes64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2032))).Value = __ccgo_fp(Xsqlite3_total_changes64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2040))).Value = __ccgo_fp(Xsqlite3_autovacuum_pages);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2048))).Value = __ccgo_fp(Xsqlite3_error_offset);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2056))).Value = __ccgo_fp(Xsqlite3_vtab_rhs_value);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2064))).Value = __ccgo_fp(Xsqlite3_vtab_distinct);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2072))).Value = __ccgo_fp(Xsqlite3_vtab_in);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2080))).Value = __ccgo_fp(Xsqlite3_vtab_in_first);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2088))).Value = __ccgo_fp(Xsqlite3_vtab_in_next);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2096))).Value = __ccgo_fp(Xsqlite3_deserialize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2104))).Value = __ccgo_fp(Xsqlite3_serialize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2112))).Value = __ccgo_fp(Xsqlite3_db_name);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2120))).Value = __ccgo_fp(Xsqlite3_value_encoding);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2128))).Value = __ccgo_fp(Xsqlite3_is_interrupted);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2136))).Value = __ccgo_fp(Xsqlite3_stmt_explain);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2144))).Value = __ccgo_fp(Xsqlite3_get_clientdata);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2152))).Value = __ccgo_fp(Xsqlite3_set_clientdata);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2160))).Value = __ccgo_fp(Xsqlite3_setlk_timeout);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2168))).Value = __ccgo_fp(Xsqlite3_set_errmsg);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2176))).Value = __ccgo_fp(Xsqlite3_db_status64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2184))).Value = __ccgo_fp(Xsqlite3_str_truncate);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2192))).Value = __ccgo_fp(Xsqlite3_str_free);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2216))).Value = __ccgo_fp(Xsqlite3_incomplete);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2224))).Value = __ccgo_fp(Xsqlite3_result_str);
}

[GoInit] internal static void initΔ11() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_vfs_template);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_rbuVfsOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_rbuVfsDelete);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_rbuVfsAccess);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_rbuVfsFullPathname);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_rbuVfsDlOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_rbuVfsDlError);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(_rbuVfsDlSym);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_rbuVfsDlClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 104))).Value = __ccgo_fp(_rbuVfsRandomness);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 112))).Value = __ccgo_fp(_rbuVfsSleep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 120))).Value = __ccgo_fp(_rbuVfsCurrentTime);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 128))).Value = __ccgo_fp(_rbuVfsGetLastError);
}

[GoInit] internal static void initΔ12() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_statGetFuncdef);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_statGet);
}

[GoInit] internal static void initΔ13() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_jsonEachModule);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_jsonEachConnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_jsonEachBestIndex);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_jsonEachDisconnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_jsonEachOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_jsonEachClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_jsonEachFilter);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_jsonEachNext);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_jsonEachEof);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(_jsonEachColumn);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_jsonEachRowid);
}

[GoInit] internal static void initΔ14() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_memdb_vfs);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_memdbOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_memdbAccess);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_memdbFullPathname);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_memdbDlOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_memdbDlError);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(_memdbDlSym);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_memdbDlClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 104))).Value = __ccgo_fp(_memdbRandomness);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 112))).Value = __ccgo_fp(_memdbSleep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 128))).Value = __ccgo_fp(_memdbGetLastError);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 136))).Value = __ccgo_fp(_memdbCurrentTimeInt64);
}

[GoInit] internal static void initΔ15() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_aBuiltinFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_versionFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_versionFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 168))).Value = __ccgo_fp(_versionFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 240))).Value = __ccgo_fp(_versionFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 312))).Value = __ccgo_fp(_soundexFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 384))).Value = __ccgo_fp(_loadExt);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 456))).Value = __ccgo_fp(_loadExt);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 528))).Value = __ccgo_fp(_compileoptionusedFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 600))).Value = __ccgo_fp(_compileoptiongetFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 672))).Value = __ccgo_fp(_versionFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 744))).Value = __ccgo_fp(_versionFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 816))).Value = __ccgo_fp(_versionFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 888))).Value = __ccgo_fp(_versionFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 960))).Value = __ccgo_fp(_trimFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1032))).Value = __ccgo_fp(_trimFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1104))).Value = __ccgo_fp(_trimFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1176))).Value = __ccgo_fp(_trimFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1248))).Value = __ccgo_fp(_trimFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1320))).Value = __ccgo_fp(_trimFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1392))).Value = __ccgo_fp(_minmaxFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1464))).Value = __ccgo_fp(_minmaxStep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1472))).Value = __ccgo_fp(_minMaxFinalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1480))).Value = __ccgo_fp(_minMaxValue);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1536))).Value = __ccgo_fp(_minmaxFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1608))).Value = __ccgo_fp(_minmaxStep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1616))).Value = __ccgo_fp(_minMaxFinalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1624))).Value = __ccgo_fp(_minMaxValue);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1680))).Value = __ccgo_fp(_typeofFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1752))).Value = __ccgo_fp(_subtypeFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1824))).Value = __ccgo_fp(_lengthFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1896))).Value = __ccgo_fp(_bytelengthFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1968))).Value = __ccgo_fp(_instrFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2040))).Value = __ccgo_fp(_printfFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2112))).Value = __ccgo_fp(_printfFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2184))).Value = __ccgo_fp(_unicodeFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2256))).Value = __ccgo_fp(_charFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2328))).Value = __ccgo_fp(_absFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2400))).Value = __ccgo_fp(_roundFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2472))).Value = __ccgo_fp(_roundFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2544))).Value = __ccgo_fp(_upperFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2616))).Value = __ccgo_fp(_lowerFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2688))).Value = __ccgo_fp(_hexFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2760))).Value = __ccgo_fp(_unhexFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2832))).Value = __ccgo_fp(_unhexFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2904))).Value = __ccgo_fp(_concatFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2976))).Value = __ccgo_fp(_concatwsFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 3048))).Value = __ccgo_fp(_versionFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 3120))).Value = __ccgo_fp(_randomFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 3192))).Value = __ccgo_fp(_randomBlob);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 3264))).Value = __ccgo_fp(_nullifFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 3336))).Value = __ccgo_fp(_versionFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 3408))).Value = __ccgo_fp(_sourceidFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 3480))).Value = __ccgo_fp(_errlogFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 3552))).Value = __ccgo_fp(_unistrFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 3624))).Value = __ccgo_fp(_quoteFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 3696))).Value = __ccgo_fp(_quoteFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 3768))).Value = __ccgo_fp(_last_insert_rowid);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 3840))).Value = __ccgo_fp(_changes);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 3912))).Value = __ccgo_fp(_total_changes);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 3984))).Value = __ccgo_fp(_replaceFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4056))).Value = __ccgo_fp(_zeroblobFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4128))).Value = __ccgo_fp(_substrFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4200))).Value = __ccgo_fp(_substrFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4272))).Value = __ccgo_fp(_substrFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4344))).Value = __ccgo_fp(_substrFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4416))).Value = __ccgo_fp(_sumStep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4424))).Value = __ccgo_fp(_sumFinalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4432))).Value = __ccgo_fp(_sumFinalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4440))).Value = __ccgo_fp(_sumInverse);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4488))).Value = __ccgo_fp(_sumStep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4496))).Value = __ccgo_fp(_totalFinalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4504))).Value = __ccgo_fp(_totalFinalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4512))).Value = __ccgo_fp(_sumInverse);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4560))).Value = __ccgo_fp(_sumStep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4568))).Value = __ccgo_fp(_avgFinalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4576))).Value = __ccgo_fp(_avgFinalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4584))).Value = __ccgo_fp(_sumInverse);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4632))).Value = __ccgo_fp(_countStep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4640))).Value = __ccgo_fp(_countFinalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4648))).Value = __ccgo_fp(_countFinalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4656))).Value = __ccgo_fp(_countInverse);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4704))).Value = __ccgo_fp(_countStep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4712))).Value = __ccgo_fp(_countFinalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4720))).Value = __ccgo_fp(_countFinalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4728))).Value = __ccgo_fp(_countInverse);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4776))).Value = __ccgo_fp(_groupConcatStep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4784))).Value = __ccgo_fp(_groupConcatFinalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4792))).Value = __ccgo_fp(_groupConcatValue);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4800))).Value = __ccgo_fp(_groupConcatInverse);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4848))).Value = __ccgo_fp(_groupConcatStep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4856))).Value = __ccgo_fp(_groupConcatFinalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4864))).Value = __ccgo_fp(_groupConcatValue);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4872))).Value = __ccgo_fp(_groupConcatInverse);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4920))).Value = __ccgo_fp(_groupConcatStep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4928))).Value = __ccgo_fp(_groupConcatFinalize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4936))).Value = __ccgo_fp(_groupConcatValue);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4944))).Value = __ccgo_fp(_groupConcatInverse);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 4992))).Value = __ccgo_fp(_likeFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5064))).Value = __ccgo_fp(_likeFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5136))).Value = __ccgo_fp(_likeFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5192))).Value = __ccgo_fp(_xCeil);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5208))).Value = __ccgo_fp(_ceilingFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5264))).Value = __ccgo_fp(_xCeil);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5280))).Value = __ccgo_fp(_ceilingFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5336))).Value = __ccgo_fp(_xFloor);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5352))).Value = __ccgo_fp(_ceilingFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5408))).Value = __ccgo_fp(libc.Xtrunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5424))).Value = __ccgo_fp(_ceilingFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5496))).Value = __ccgo_fp(_logFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5568))).Value = __ccgo_fp(_logFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5640))).Value = __ccgo_fp(_logFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5712))).Value = __ccgo_fp(_logFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5784))).Value = __ccgo_fp(_logFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5840))).Value = __ccgo_fp(libc.Xexp);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5856))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5912))).Value = __ccgo_fp(libc.Xpow);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5928))).Value = __ccgo_fp(_math2Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 5984))).Value = __ccgo_fp(libc.Xpow);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6000))).Value = __ccgo_fp(_math2Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6056))).Value = __ccgo_fp(libc.Xfmod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6072))).Value = __ccgo_fp(_math2Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6128))).Value = __ccgo_fp(libc.Xacos);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6144))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6200))).Value = __ccgo_fp(libc.Xasin);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6216))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6272))).Value = __ccgo_fp(libc.Xatan);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6288))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6344))).Value = __ccgo_fp(libc.Xatan2);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6360))).Value = __ccgo_fp(_math2Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6416))).Value = __ccgo_fp(libc.Xcos);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6432))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6488))).Value = __ccgo_fp(libc.Xsin);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6504))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6560))).Value = __ccgo_fp(libc.Xtan);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6576))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6632))).Value = __ccgo_fp(libc.Xcosh);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6648))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6704))).Value = __ccgo_fp(libc.Xsinh);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6720))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6776))).Value = __ccgo_fp(libc.Xtanh);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6792))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6848))).Value = __ccgo_fp(libc.Xacosh);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6864))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6920))).Value = __ccgo_fp(libc.Xasinh);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6936))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 6992))).Value = __ccgo_fp(libc.Xatanh);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 7008))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 7064))).Value = __ccgo_fp(libc.Xsqrt);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 7080))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 7136))).Value = __ccgo_fp(_degToRad);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 7152))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 7208))).Value = __ccgo_fp(_radToDeg);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 7224))).Value = __ccgo_fp(_math1Func);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 7296))).Value = __ccgo_fp(_piFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 7368))).Value = __ccgo_fp(_signFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 7440))).Value = __ccgo_fp(_versionFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 7512))).Value = __ccgo_fp(_versionFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 7584))).Value = __ccgo_fp(_versionFunc);
}

[GoInit] internal static void initΔ16() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_fts5Mod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(_fts5CreateMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_fts5ConnectMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_fts5BestIndexMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_fts5DisconnectMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_fts5DestroyMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_fts5OpenMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_fts5CloseMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_fts5FilterMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_fts5NextMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_fts5EofMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(_fts5ColumnMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_fts5RowidMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 104))).Value = __ccgo_fp(_fts5UpdateMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 112))).Value = __ccgo_fp(_fts5BeginMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 120))).Value = __ccgo_fp(_fts5SyncMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 128))).Value = __ccgo_fp(_fts5CommitMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 136))).Value = __ccgo_fp(_fts5RollbackMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 144))).Value = __ccgo_fp(_fts5FindFunctionMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 152))).Value = __ccgo_fp(_fts5RenameMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 160))).Value = __ccgo_fp(_fts5SavepointMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 168))).Value = __ccgo_fp(_fts5ReleaseMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 176))).Value = __ccgo_fp(_fts5RollbackToMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 184))).Value = __ccgo_fp(_fts5ShadowName);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 192))).Value = __ccgo_fp(_fts5IntegrityMethod);
}

[GoInit] internal static void initΔ17() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_rbuvfs_io_methods1);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(_rbuVfsClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_rbuVfsRead);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_rbuVfsWrite);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_rbuVfsTruncate);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_rbuVfsSync);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_rbuVfsFileSize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_rbuVfsLock);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_rbuVfsUnlock);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_rbuVfsCheckReservedLock);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_rbuVfsFileControl);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(_rbuVfsSectorSize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_rbuVfsDeviceCharacteristics);
}

[GoInit] internal static void initΔ18() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_rbuvfs_io_methods);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(_rbuVfsClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_rbuVfsRead);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_rbuVfsWrite);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_rbuVfsTruncate);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_rbuVfsSync);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_rbuVfsFileSize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_rbuVfsLock);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_rbuVfsUnlock);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_rbuVfsCheckReservedLock);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_rbuVfsFileControl);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(_rbuVfsSectorSize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_rbuVfsDeviceCharacteristics);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 104))).Value = __ccgo_fp(_rbuVfsShmMap);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 112))).Value = __ccgo_fp(_rbuVfsShmLock);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 120))).Value = __ccgo_fp(_rbuVfsShmBarrier);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 128))).Value = __ccgo_fp(_rbuVfsShmUnmap);
}

[GoInit] internal static void initΔ19() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_aWindowFuncs);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_row_numberStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_row_numberValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_row_numberValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_dense_rankStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 104))).Value = __ccgo_fp(_dense_rankValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 112))).Value = __ccgo_fp(_dense_rankValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 120))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 168))).Value = __ccgo_fp(_rankStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 176))).Value = __ccgo_fp(_rankValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 184))).Value = __ccgo_fp(_rankValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 192))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 240))).Value = __ccgo_fp(_percent_rankStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 248))).Value = __ccgo_fp(_percent_rankValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 256))).Value = __ccgo_fp(_percent_rankValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 264))).Value = __ccgo_fp(_percent_rankInvFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 312))).Value = __ccgo_fp(_cume_distStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 320))).Value = __ccgo_fp(_cume_distValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 328))).Value = __ccgo_fp(_cume_distValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 336))).Value = __ccgo_fp(_cume_distInvFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 384))).Value = __ccgo_fp(_ntileStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 392))).Value = __ccgo_fp(_ntileValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 400))).Value = __ccgo_fp(_ntileValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 408))).Value = __ccgo_fp(_ntileInvFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 456))).Value = __ccgo_fp(_last_valueStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 464))).Value = __ccgo_fp(_last_valueFinalizeFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 472))).Value = __ccgo_fp(_last_valueValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 480))).Value = __ccgo_fp(_last_valueInvFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 528))).Value = __ccgo_fp(_nth_valueStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 536))).Value = __ccgo_fp(_nth_valueFinalizeFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 544))).Value = __ccgo_fp(_noopValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 552))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 600))).Value = __ccgo_fp(_first_valueStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 608))).Value = __ccgo_fp(_first_valueFinalizeFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 616))).Value = __ccgo_fp(_noopValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 624))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 672))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 680))).Value = __ccgo_fp(_noopValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 688))).Value = __ccgo_fp(_noopValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 696))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 744))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 752))).Value = __ccgo_fp(_noopValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 760))).Value = __ccgo_fp(_noopValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 768))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 816))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 824))).Value = __ccgo_fp(_noopValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 832))).Value = __ccgo_fp(_noopValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 840))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 888))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 896))).Value = __ccgo_fp(_noopValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 904))).Value = __ccgo_fp(_noopValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 912))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 960))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 968))).Value = __ccgo_fp(_noopValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 976))).Value = __ccgo_fp(_noopValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 984))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1032))).Value = __ccgo_fp(_noopStepFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1040))).Value = __ccgo_fp(_noopValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1048))).Value = __ccgo_fp(_noopValueFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1056))).Value = __ccgo_fp(_noopStepFunc);
}

[GoInit] internal static void initΔ20() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_rtreeModule);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(_rtreeCreate);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_rtreeConnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_rtreeBestIndex);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_rtreeDisconnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_rtreeDestroy);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_rtreeOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_rtreeClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_rtreeFilter);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_rtreeNext);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_rtreeEof);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(_rtreeColumn);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_rtreeRowid);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 104))).Value = __ccgo_fp(_rtreeUpdate);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 112))).Value = __ccgo_fp(_rtreeBeginTransaction);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 120))).Value = __ccgo_fp(_rtreeEndTransaction);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 128))).Value = __ccgo_fp(_rtreeEndTransaction);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 136))).Value = __ccgo_fp(_rtreeRollback);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 152))).Value = __ccgo_fp(_rtreeRename);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 160))).Value = __ccgo_fp(_rtreeSavepoint);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 184))).Value = __ccgo_fp(_rtreeShadowName);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 192))).Value = __ccgo_fp(_rtreeIntegrity);
}

[GoInit] internal static void initΔ21() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_MemJournalMethods);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(_memjrnlClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_memjrnlRead);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_memjrnlWrite);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_memjrnlTruncate);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_memjrnlSync);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_memjrnlFileSize);
}

[GoInit] internal static void initΔ22() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_defaultMethods1);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_pcache1Init);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_pcache1Shutdown);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_pcache1Create);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_pcache1Cachesize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_pcache1Pagecount);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_pcache1Fetch);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_pcache1Unpin);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_pcache1Rekey);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_pcache1Truncate);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(_pcache1Destroy);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_pcache1Shrink);
}

[GoInit] internal static void initΔ23() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_dbstat_module);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(_statConnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_statConnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_statBestIndex);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_statDisconnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_statDisconnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_statOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_statClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_statFilter);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_statNext);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_statEof);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(_statColumn);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_statRowid);
}

[GoInit] internal static void initΔ24() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_aDateTimeFuncs);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_juliandayFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_unixepochFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 168))).Value = __ccgo_fp(_dateFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 240))).Value = __ccgo_fp(_timeFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 312))).Value = __ccgo_fp(_datetimeFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 384))).Value = __ccgo_fp(_strftimeFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 456))).Value = __ccgo_fp(_timediffFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 528))).Value = __ccgo_fp(_ctimeFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 600))).Value = __ccgo_fp(_ctimestampFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 672))).Value = __ccgo_fp(_cdateFunc);
}

[GoInit] internal static void initΔ25() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_pragmaVtabModule);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_pragmaVtabConnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_pragmaVtabBestIndex);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_pragmaVtabDisconnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_pragmaVtabOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_pragmaVtabClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_pragmaVtabFilter);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_pragmaVtabNext);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_pragmaVtabEof);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(_pragmaVtabColumn);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_pragmaVtabRowid);
}

[GoInit] internal static void initΔ26() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_geopolyModule);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(_geopolyCreate);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_geopolyConnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_geopolyBestIndex);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_rtreeDisconnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_rtreeDestroy);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_rtreeOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_rtreeClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_geopolyFilter);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_rtreeNext);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_rtreeEof);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(_geopolyColumn);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_rtreeRowid);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 104))).Value = __ccgo_fp(_geopolyUpdate);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 112))).Value = __ccgo_fp(_rtreeBeginTransaction);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 120))).Value = __ccgo_fp(_rtreeEndTransaction);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 128))).Value = __ccgo_fp(_rtreeEndTransaction);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 136))).Value = __ccgo_fp(_rtreeEndTransaction);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 144))).Value = __ccgo_fp(_geopolyFindFunction);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 152))).Value = __ccgo_fp(_rtreeRename);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 160))).Value = __ccgo_fp(_rtreeSavepoint);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 184))).Value = __ccgo_fp(_rtreeShadowName);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 192))).Value = __ccgo_fp(_rtreeIntegrity);
}

[GoInit] internal static void initΔ27() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_dbpage_module);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(_dbpageConnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_dbpageConnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_dbpageBestIndex);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_dbpageDisconnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_dbpageDisconnect);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_dbpageOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_dbpageClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_dbpageFilter);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_dbpageNext);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_dbpageEof);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(_dbpageColumn);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_dbpageRowid);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 104))).Value = __ccgo_fp(_dbpageUpdate);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 112))).Value = __ccgo_fp(_dbpageBegin);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 120))).Value = __ccgo_fp(_dbpageSync);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 176))).Value = __ccgo_fp(_dbpageRollbackTo);
}

[GoInit] internal static void initΔ28() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_aJsonFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_jsonRemoveFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_jsonRemoveFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 168))).Value = __ccgo_fp(_jsonArrayFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 240))).Value = __ccgo_fp(_jsonArrayFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 312))).Value = __ccgo_fp(_jsonSetFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 384))).Value = __ccgo_fp(_jsonSetFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 456))).Value = __ccgo_fp(_jsonArrayLengthFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 528))).Value = __ccgo_fp(_jsonArrayLengthFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 600))).Value = __ccgo_fp(_jsonErrorFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 672))).Value = __ccgo_fp(_jsonExtractFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 744))).Value = __ccgo_fp(_jsonExtractFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 816))).Value = __ccgo_fp(_jsonExtractFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 888))).Value = __ccgo_fp(_jsonExtractFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 960))).Value = __ccgo_fp(_jsonSetFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1032))).Value = __ccgo_fp(_jsonSetFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1104))).Value = __ccgo_fp(_jsonObjectFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1176))).Value = __ccgo_fp(_jsonObjectFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1248))).Value = __ccgo_fp(_jsonPatchFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1320))).Value = __ccgo_fp(_jsonPatchFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1392))).Value = __ccgo_fp(_jsonPrettyFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1464))).Value = __ccgo_fp(_jsonPrettyFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1536))).Value = __ccgo_fp(_jsonQuoteFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1608))).Value = __ccgo_fp(_jsonRemoveFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1680))).Value = __ccgo_fp(_jsonRemoveFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1752))).Value = __ccgo_fp(_jsonReplaceFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1824))).Value = __ccgo_fp(_jsonReplaceFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1896))).Value = __ccgo_fp(_jsonSetFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 1968))).Value = __ccgo_fp(_jsonSetFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2040))).Value = __ccgo_fp(_jsonTypeFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2112))).Value = __ccgo_fp(_jsonTypeFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2184))).Value = __ccgo_fp(_jsonValidFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2256))).Value = __ccgo_fp(_jsonValidFunc);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2328))).Value = __ccgo_fp(_jsonArrayStep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2336))).Value = __ccgo_fp(_jsonArrayFinal);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2344))).Value = __ccgo_fp(_jsonArrayValue);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2352))).Value = __ccgo_fp(_jsonGroupInverse);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2400))).Value = __ccgo_fp(_jsonArrayStep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2408))).Value = __ccgo_fp(_jsonArrayFinal);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2416))).Value = __ccgo_fp(_jsonArrayValue);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2424))).Value = __ccgo_fp(_jsonGroupInverse);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2472))).Value = __ccgo_fp(_jsonObjectStep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2480))).Value = __ccgo_fp(_jsonObjectFinal);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2488))).Value = __ccgo_fp(_jsonObjectValue);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2496))).Value = __ccgo_fp(_jsonGroupInverse);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2544))).Value = __ccgo_fp(_jsonObjectStep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2552))).Value = __ccgo_fp(_jsonObjectFinal);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2560))).Value = __ccgo_fp(_jsonObjectValue);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 2568))).Value = __ccgo_fp(_jsonGroupInverse);
}

[GoInit] internal static void initΔ29() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_statInitFuncdef);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_statInit);
}

[GoInit] internal static void initΔ30() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_memdb_io_methods);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(_memdbClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_memdbRead);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_memdbWrite);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_memdbTruncate);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_memdbSync);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_memdbFileSize);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_memdbLock);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_memdbUnlock);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_memdbFileControl);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_memdbDeviceCharacteristics);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 136))).Value = __ccgo_fp(_memdbFetch);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 144))).Value = __ccgo_fp(_memdbUnfetch);
}

[GoInit] internal static void initΔ31() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_fts5Vocab);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(_fts5VocabCreateMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_fts5VocabConnectMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_fts5VocabBestIndexMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_fts5VocabDisconnectMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_fts5VocabDestroyMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_fts5VocabOpenMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_fts5VocabCloseMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_fts5VocabFilterMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_fts5VocabNextMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_fts5VocabEofMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(_fts5VocabColumnMethod);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_fts5VocabRowidMethod);
}

[GoInit] internal static void initΔ32() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_sqlite3BuiltinExtensions);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 0))).Value = __ccgo_fp(_sqlite3Fts5Init);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = __ccgo_fp(_sqlite3RtreeInit);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 16))).Value = __ccgo_fp(_sqlite3DbpageRegister);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 24))).Value = __ccgo_fp(_sqlite3DbstatRegister);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = __ccgo_fp(_sqlite3TestExtInit);
}

} // end sqlite3_package
