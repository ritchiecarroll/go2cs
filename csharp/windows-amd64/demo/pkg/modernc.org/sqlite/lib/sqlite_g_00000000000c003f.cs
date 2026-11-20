global using Terrno_t = int;
global using Tmode_t = ushort;
global using errno_t = int;
global using rsize_t = ulong;

namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static array<uintptr> _azTypes;
internal static void initᴛ_azTypes() { _azTypes = new array<uintptr>(5){
    [0] = __ccgo_ts + 1192,
    [1] = __ccgo_ts + 1204,
    [2] = __ccgo_ts + 1209,
    [3] = __ccgo_ts + 1187,
    [4] = __ccgo_ts + 1744
}; }

internal static void _concatFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    _concatFuncCore(tls, context, argc, argv, 0, __ccgo_ts + 1730);
}

internal static uintptr /*r*/ _fts5ValueToText(ж<libc.TLS> tls, uintptr pVal) {
    uintptr zRet = default;
    uintptr v1 = default;
    _ = zRet;
    _ = v1;
    zRet = Xsqlite3_value_text(tls, pVal);
    if (zRet != 0) {
        v1 = zRet;
    } else {
        v1 = __ccgo_ts + 1730;
    }
    return v1;
}

internal static void _fts5WriteFlushBtree(ж<libc.TLS> tls, uintptr p, uintptr pWriter) {
    int32 bFlag = default;
    uintptr z = default;
    uintptr v1 = default;
    _ = bFlag;
    _ = z;
    _ = v1;
    if ((@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FiBtPage == 0) {
        return;
    }
    bFlag = _fts5WriteFlushDlidx(tls, p, pWriter);
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        if ((@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.Fbtterm.Fn > 0) {
            v1 = (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.Fbtterm.Fp;
        } else {
            v1 = __ccgo_ts + 1730;
        }
        z = v1;
        Xsqlite3_bind_blob(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpIdxWriter, (int32)2, z, (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.Fbtterm.Fn, libc.UintptrFromInt32(0));
        Xsqlite3_bind_int64(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpIdxWriter, (int32)3, (int64)bFlag + ((int64)(@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FiBtPage).Lsh((int64)(libc.Int32FromInt32(1))));
        Xsqlite3_step(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpIdxWriter);
        (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc = Xsqlite3_reset(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpIdxWriter);
        Xsqlite3_bind_null(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpIdxWriter, (int32)2);
    }
    (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FiBtPage = 0;
}

internal static void _logBadConnection(ж<libc.TLS> tls, uintptr zType) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Xsqlite3_log(tls, (int32)SQLITE_MISUSE, __ccgo_ts + 1874, libc.VaList(bp + 8, zType));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r1*/ _parseDateOrTime(ж<libc.TLS> tls, uintptr context, uintptr zDate, uintptr p) {
    ref var r = ref heap(new float64(), out var Ꮡr);
    _ = r;
    if (_parseYyyyMmDd(tls, zDate, p) == 0) {
        return 0;
    } else {
        if (_parseHhMmSs(tls, zDate, p) == 0) {
            return 0;
        } else {
            if (_sqlite3StrICmp(ref (tls).DerefOrNull(), zDate, __ccgo_ts + 1246) == 0 && _sqlite3NotPureFunc(tls, context) != 0) {
                return _setDateTimeToCurrent(tls, context, p);
            } else {
                if (_sqlite3AtoF(tls, zDate, @unsafe.Pointer.FromPinnedBox(Ꮡr)) > 0) {
                    _setRawDateNumber(ref (tls).DerefOrNull(), p, r);
                    return 0;
                } else {
                    if ((_sqlite3StrICmp(ref (tls).DerefOrNull(), zDate, __ccgo_ts + 1250) == 0 || _sqlite3StrICmp(ref (tls).DerefOrNull(), zDate, __ccgo_ts + 1257) == 0) && _sqlite3NotPureFunc(tls, context) != 0) {
                        libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(1), 2, 0x4);
                        return _setDateTimeToCurrent(tls, context, p);
                    }
                }
            }
        }
    }
    return (int32)1;
}

internal static readonly ж<array<uintptr>> Ꮡ_sqlite3StdType = new StaticBox<array<uintptr>>(default(array<uintptr>));
internal static ref array<uintptr> _sqlite3StdType => ref ((StaticBox<array<uintptr>>)Ꮡ_sqlite3StdType).ValueSlot;
internal static void initᴛ_sqlite3StdType() { _sqlite3StdType = new array<uintptr>(6){
    [0] = __ccgo_ts + 1183,
    [1] = __ccgo_ts + 1187,
    [2] = __ccgo_ts + 1192,
    [3] = __ccgo_ts + 1196,
    [4] = __ccgo_ts + 1204,
    [5] = __ccgo_ts + 1209
}; }

internal static readonly ж<array<uintptr>> Ꮡ_sqlite3azCompileOpt = new StaticBox<array<uintptr>>(default(array<uintptr>));
internal static ref array<uintptr> _sqlite3azCompileOpt => ref ((StaticBox<array<uintptr>>)Ꮡ_sqlite3azCompileOpt).ValueSlot;
internal static void initᴛ_sqlite3azCompileOpt() { _sqlite3azCompileOpt = new array<uintptr>(57){
    [0] = __ccgo_ts,
    [1] = __ccgo_ts + 20,
    [2] = __ccgo_ts + 42,
    [3] = __ccgo_ts + 61,
    [4] = __ccgo_ts + 86,
    [5] = __ccgo_ts + 108,
    [6] = __ccgo_ts + 138,
    [7] = __ccgo_ts + 158,
    [8] = __ccgo_ts + 178,
    [9] = __ccgo_ts + 201,
    [10] = __ccgo_ts + 226,
    [11] = __ccgo_ts + 253,
    [12] = __ccgo_ts + 278,
    [13] = __ccgo_ts + 300,
    [14] = __ccgo_ts + 332,
    [15] = __ccgo_ts + 358,
    [16] = __ccgo_ts + 383,
    [17] = __ccgo_ts + 404,
    [18] = __ccgo_ts + 422,
    [19] = __ccgo_ts + 445,
    [20] = __ccgo_ts + 464,
    [21] = __ccgo_ts + 483,
    [22] = __ccgo_ts + 495,
    [23] = __ccgo_ts + 510,
    [24] = __ccgo_ts + 532,
    [25] = __ccgo_ts + 557,
    [26] = __ccgo_ts + 580,
    [27] = __ccgo_ts + 602,
    [28] = __ccgo_ts + 613,
    [29] = __ccgo_ts + 626,
    [30] = __ccgo_ts + 641,
    [31] = __ccgo_ts + 657,
    [32] = __ccgo_ts + 670,
    [33] = __ccgo_ts + 691,
    [34] = __ccgo_ts + 715,
    [35] = __ccgo_ts + 738,
    [36] = __ccgo_ts + 754,
    [37] = __ccgo_ts + 770,
    [38] = __ccgo_ts + 794,
    [39] = __ccgo_ts + 821,
    [40] = __ccgo_ts + 841,
    [41] = __ccgo_ts + 863,
    [42] = __ccgo_ts + 885,
    [43] = __ccgo_ts + 915,
    [44] = __ccgo_ts + 940,
    [45] = __ccgo_ts + 966,
    [46] = __ccgo_ts + 986,
    [47] = __ccgo_ts + 1006,
    [48] = __ccgo_ts + 1032,
    [49] = __ccgo_ts + 1055,
    [50] = __ccgo_ts + 1081,
    [51] = __ccgo_ts + 1103,
    [52] = __ccgo_ts + 1124,
    [53] = __ccgo_ts + 1135,
    [54] = __ccgo_ts + 1143,
    [55] = __ccgo_ts + 1157,
    [56] = __ccgo_ts + 1170
}; }

} // end sqlite3_package
