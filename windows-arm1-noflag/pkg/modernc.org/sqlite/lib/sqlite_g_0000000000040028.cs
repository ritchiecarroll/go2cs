namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal partial struct _sqlite3Fts5IndexReinit__ᴛ1 /*dyn*/ {
    public /*[0][48]*/ array<array<Tu8>> FtmpSpace = new(0, () => new(48));
    public TFts5Structure FsFts;
    public array<byte> F__ccgo_pad2 = new(16);
}

internal static int32 /*r*/ _sqlite3Fts5IndexReinit(ж<libc.TLS> tls, uintptr p) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        @unsafe.Pointer pTmp = default;
        _sqlite3Fts5IndexReinit__ᴛ1 _ᴛ1 = new();
        _ = pTmp;
        _fts5StructureInvalidate(tls, p);
        _fts5IndexDiscardData(tls, p);
        pTmp = __ccgo_ptr(bp);
        libc.Xmemset(tls, bp, 0, (uint64)48);
        if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FbContentlessDelete != 0) {
            (@unsafe.Pointer.To<Tu64>(@unsafe.Add(pTmp, 16))).Value = (uint64)1;
        }
        _fts5DataWrite(tls, p, (int64)FTS5_AVERAGES_ROWID, __ccgo_ts + 1730, 0);
        _fts5StructureWrite(tls, p, pTmp);
        r = _fts5IndexReturn(ref (tls).DerefOrNull(), p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _sqlite3ParseObjectInit(ж<libc.TLS> tls, uintptr pParse, uintptr db) {
    libc.Xmemset(tls, pParse + (uintptr)(uint64)(libc.UintptrFromInt32(0) + 8), 0, (uint64)(libc.UintptrFromInt32(0) + 176) - (uint64)(libc.UintptrFromInt32(0) + 8));
    libc.Xmemset(tls, pParse + (uintptr)(uint64)(libc.UintptrFromInt32(0) + 288), 0, libc.Uint64FromInt64(440) - (uint64)(libc.UintptrFromInt32(0) + 288));
    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpOuterParse = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpParse;
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpParse = pParse;
    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb = db;
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 1700, 0);
    }
}

} // end sqlite3_package
