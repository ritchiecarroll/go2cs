global using TDblquoteStr = go.modernc.org.sqlite.lib.sqlite3_package.TDblquoteStrᴛ1;
global using TFpDecode = go.modernc.org.sqlite.lib.sqlite3_package.TFpDecodeᴛ1;
global using TNanInfName = go.modernc.org.sqlite.lib.sqlite3_package.TNanInfNameᴛ1;
global using TPorterTokenizer = go.modernc.org.sqlite.lib.sqlite3_package.TPorterTokenizerᴛ1;
global using TReturning = go.modernc.org.sqlite.lib.sqlite3_package.TReturningᴛ1;
global using TWhereScan = go.modernc.org.sqlite.lib.sqlite3_package.TWhereScanᴛ1;
global using Tet_info = go.modernc.org.sqlite.lib.sqlite3_package.Tet_infoᴛ1;

namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using System.Runtime.InteropServices;
using go.arrays;
using go.golib;
using go.modernc.org;

partial class sqlite3_package {

partial struct TDblquoteStrᴛ1 /*dyn*/ {
    public uintptr FpNextStr;
    public array<int8> Fz = new(8);
}

[global::go.golib.GoInlineLayout(Size = 40, Align = 8), StructLayout(LayoutKind.Explicit, Size = 40)] partial struct TFpDecodeᴛ1 /*dyn*/ {
    [FieldOffset(0)] public int32 Fn;
    [FieldOffset(4)] public int32 FiDP;
    [FieldOffset(8)] public uintptr Fz;
    [FieldOffset(16)] public array21<int8> FzBuf;
    [FieldOffset(37)] public int8 Fsign;
    [FieldOffset(38)] public int8 FisSpecial;
}

partial struct TNanInfNameᴛ1 /*dyn*/ {
    public int8 Fc1;
    public int8 Fc2;
    public int8 Fn;
    public int8 FeType;
    public int8 FnRepl;
    public uintptr FzMatch;
    public uintptr FzRepl;
}

[global::go.golib.GoInlineLayout(Size = 168, Align = 8), StructLayout(LayoutKind.Explicit, Size = 168)] partial struct TPorterTokenizerᴛ1 /*dyn*/ {
    [FieldOffset(0)] public Tfts5_tokenizer_v2 Ftokenizer_v2;
    [FieldOffset(32)] public uintptr FpTokenizer;
    [FieldOffset(40)] public array128<int8> FaBuf;
}

[global::go.golib.GoInlineLayout(Size = 232, Align = 8), StructLayout(LayoutKind.Explicit, Size = 232)] partial struct TReturningᴛ1 /*dyn*/ {
    [FieldOffset(0)] public uintptr FpParse;
    [FieldOffset(8)] public uintptr FpReturnEL;
    [FieldOffset(16)] public TTrigger FretTrig;
    [FieldOffset(88)] public TTriggerStep FretTStep;
    [FieldOffset(176)] public int32 FiRetCur;
    [FieldOffset(180)] public int32 FnRetCol;
    [FieldOffset(184)] public int32 FiRetReg;
    [FieldOffset(188)] public array40<int8> FzName;
}

[global::go.golib.GoInlineLayout(Size = 112, Align = 8), StructLayout(LayoutKind.Explicit, Size = 112)] partial struct TWhereScanᴛ1 /*dyn*/ {
    [FieldOffset(0)] public uintptr FpOrigWC;
    [FieldOffset(8)] public uintptr FpWC;
    [FieldOffset(16)] public uintptr FzCollName;
    [FieldOffset(24)] public uintptr FpIdxExpr;
    [FieldOffset(32)] public int32 Fk;
    [FieldOffset(36)] public Tu32 FopMask;
    [FieldOffset(40)] public int8 Fidxaff;
    [FieldOffset(41)] public uint8 FiEquiv;
    [FieldOffset(42)] public uint8 FnEquiv;
    [FieldOffset(44)] public array11<int32> FaiCur;
    [FieldOffset(88)] public array11<Ti16> FaiColumn;
}

partial struct Tet_infoᴛ1 /*dyn*/ {
    public int8 Ffmttype;
    public TetByte Fbase;
    public TetByte Fflags;
    public TetByte Ftype1;
    public TetByte Fcharset;
    public TetByte Fprefix;
    public int8 FiNxt;
}

public static uintptr /*r*/ Xsqlite3_filename_journal(ж<libc.TLS> tls, uintptr zFilename) {
    if (zFilename == (uintptr)0) {
        return (uintptr)0;
    }
    zFilename = _databaseName(ref (tls).DerefOrNull(), zFilename);
    zFilename = zFilename + (uintptr)(_sqlite3Strlen30(tls, zFilename) + (int32)1);
    while (zFilename != 0 && ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zFilename)) != 0) {
        zFilename = zFilename + (uintptr)(_sqlite3Strlen30(tls, zFilename) + (int32)1);
        zFilename = zFilename + (uintptr)(_sqlite3Strlen30(tls, zFilename) + (int32)1);
    }
    return zFilename + (uintptr)1;
}

public static uintptr /*r*/ Xsqlite3_snprintf(ж<libc.TLS> tls, int32 n, uintptr zBuf, uintptr zFormat, uintptr va) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        Tva_list ap = default;
        TStrAccum _ᴛ1 = default;
        _ = ap;
        if (n <= 0) {
            r = zBuf; goto ᒐdone;
        }
        _sqlite3StrAccumInit(ref (tls).DerefOrNull(), __ccgo_ptr(bp), (uintptr)0, zBuf, n, 0);
        ap = va;
        Xsqlite3_str_vappendf(tls, bp, zFormat, ap);
        _ = ap;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zBuf + (uintptr)(@unsafe.Pointer.To<TStrAccum>(__ccgo_ptr(bp))).Value.FnChar))).Value = 0;
        r = zBuf;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static uintptr /*r*/ Xsqlite3_str_value(ж<libc.TLS> tls, uintptr p) {
    if (p == (uintptr)0 || (@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnChar == (uint32)0) {
        return (uintptr)0;
    }
    (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FzText + (uintptr)(@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnChar))).Value = 0;
    return (@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FzText;
}

public static uintptr /*r*/ Xsqlite3_uri_key(ж<libc.TLS> tls, uintptr zFilename, int32 N) {
    int32 v1 = default;
    bool v2 = default;
    uintptr v3 = default;
    _ = v1;
    _ = v2;
    _ = v3;
    if (zFilename == (uintptr)0 || N < 0) {
        return (uintptr)0;
    }
    zFilename = _databaseName(ref (tls).DerefOrNull(), zFilename);
    zFilename = zFilename + (uintptr)(_sqlite3Strlen30(tls, zFilename) + (int32)1);
    for (;;) {
        v2 = zFilename != 0 && ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zFilename)) != 0; if (v2) {
            v1 = N;
            N = N - 1;
        }
        if (!(v2 && v1 > 0)) {
            break;
        }
        zFilename = zFilename + (uintptr)(_sqlite3Strlen30(tls, zFilename) + (int32)1);
        zFilename = zFilename + (uintptr)(_sqlite3Strlen30(tls, zFilename) + (int32)1);
    }
    if (~@unsafe.Pointer.To<int8>(__ccgo_ptr(zFilename)) != 0) {
        v3 = zFilename;
    } else {
        v3 = (uintptr)0;
    }
    return v3;
}

public static readonly ж<array<int8>> ᏑXsqlite3_version = new StaticBox<array<int8>>(new int8[]{(int8)'3', (int8)'.', (int8)'5', (int8)'4', (int8)'.', (int8)'1'}.array(7));
public static ref array<int8> Xsqlite3_version => ref ((StaticBox<array<int8>>)ᏑXsqlite3_version).ValueSlot;

public static uintptr /*r*/ Xsqlite3_vsnprintf(ж<libc.TLS> tls, int32 n, uintptr zBuf, uintptr zFormat, Tva_list ap) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        TStrAccum _ᴛ1 = default;
        if (n <= 0) {
            r = zBuf; goto ᒐdone;
        }
        _sqlite3StrAccumInit(ref (tls).DerefOrNull(), __ccgo_ptr(bp), (uintptr)0, zBuf, n, 0);
        Xsqlite3_str_vappendf(tls, bp, zFormat, ap);
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zBuf + (uintptr)(@unsafe.Pointer.To<TStrAccum>(__ccgo_ptr(bp))).Value.FnChar))).Value = 0;
        r = zBuf;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

[GoLazy] partial class lazyᴛ_aDigits { internal static readonly ж<array<int8>> Ꮡ_aDigits = new StaticBox<array<int8>>(new int8[]{(int8)'0', (int8)'1', (int8)'2', (int8)'3', (int8)'4', (int8)'5', (int8)'6', (int8)'7', (int8)'8', (int8)'9', (int8)'A', (int8)'B', (int8)'C', (int8)'D', (int8)'E', (int8)'F', (int8)'0', (int8)'1', (int8)'2', (int8)'3', (int8)'4', (int8)'5', (int8)'6', (int8)'7', (int8)'8', (int8)'9', (int8)'a', (int8)'b', (int8)'c', (int8)'d', (int8)'e', (int8)'f'}.array(33)); }
internal static ref array<int8> _aDigits => ref ((StaticBox<array<int8>>)Ꮡ_aDigits).ValueSlot;

[GoLazy] partial class lazyᴛ_aHex { internal static array<int8> _aHex = new int8[]{(int8)'0', (int8)'1', (int8)'2', (int8)'3', (int8)'4', (int8)'5', (int8)'6', (int8)'7', (int8)'8', (int8)'9', (int8)'a', (int8)'b', (int8)'c', (int8)'d', (int8)'e', (int8)'f'}.array(17); }

internal static readonly ж<array<int8>> Ꮡ_aPrefix = new StaticBox<array<int8>>(new int8[]{(int8)'-', (int8)'x', (int8)'0', 0, (int8)'X', (int8)'0'}.array(7));
internal static ref array<int8> _aPrefix => ref ((StaticBox<array<int8>>)Ꮡ_aPrefix).ValueSlot;

[GoLazy] partial class lazyᴛ_aSpecial { internal static array<int8> _aSpecial = new array<int8>(32){
    [8] = (int8)'b',
    [9] = (int8)'t',
    [10] = (int8)'n',
    [12] = (int8)'f',
    [13] = (int8)'r'
}; }


[global::go.golib.GoInlineLayout(Size = 16, Align = 4), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct _aXformTypeᴛ1 /*dyn*/ {
    [FieldOffset(0)] public Tu8 FnName;
    [FieldOffset(1)] public array7<int8> FzName;
    [FieldOffset(8)] public float32 FrLimit;
    [FieldOffset(12)] public float32 FrXform;
}
[GoLazy] partial class lazyᴛ_aXformType { internal static readonly ж<array<_aXformTypeᴛ1>> Ꮡ_aXformType = new StaticBox<array<_aXformTypeᴛ1>>(new array<_aXformTypeᴛ1>(6, () => new()){
    [0] = new(
        FnName: (uint8)6,
        FzName: new array7<int8>((int8)'s', (int8)'e', (int8)'c', (int8)'o', (int8)'n', (int8)'d'),
        FrLimit: (float32)4.6427e+14F,
        FrXform: (float32)1F
    ),
    [1] = new(
        FnName: (uint8)6,
        FzName: new array7<int8>((int8)'m', (int8)'i', (int8)'n', (int8)'u', (int8)'t', (int8)'e'),
        FrLimit: (float32)7.7379e+12F,
        FrXform: (float32)60F
    ),
    [2] = new(
        FnName: (uint8)4,
        FzName: new array7<int8>((int8)'h', (int8)'o', (int8)'u', (int8)'r'),
        FrLimit: (float32)1.2897e+11F,
        FrXform: (float32)3600F
    ),
    [3] = new(
        FnName: (uint8)3,
        FzName: new array7<int8>((int8)'d', (int8)'a', (int8)'y'),
        FrLimit: (float32)5.373485e+06F,
        FrXform: (float32)86400F
    ),
    [4] = new(
        FnName: (uint8)5,
        FzName: new array7<int8>((int8)'m', (int8)'o', (int8)'n', (int8)'t', (int8)'h'),
        FrLimit: (float32)176546F,
        FrXform: (float32)2.592e+06F
    ),
    [5] = new(
        FnName: (uint8)4,
        FzName: new array7<int8>((int8)'y', (int8)'e', (int8)'a', (int8)'r'),
        FrLimit: (float32)14713F,
        FrXform: (float32)3.1536e+07F
    )
}); }
internal static ref array<_aXformTypeᴛ1> _aXformType => ref ((StaticBox<array<_aXformTypeᴛ1>>)Ꮡ_aXformType).ValueSlot;

internal static void _asciiFold(ref libc.TLS tls, uintptr aOut, uintptr aIn, int32 nByte) {
    int8 c = default;
    int32 i = default;
    _ = c;
    _ = i;
    i = 0;
    for (;;) {
        if (!(i < nByte)) {
            break;
        }
        c = ~@unsafe.Pointer.To<int8>(__ccgo_ptr(aIn + (uintptr)i));
        if ((int32)c >= (int32)'A' && (int32)c <= (int32)'Z') {
            c = (int8)((int32)c + libc.Int32FromInt32(32));
        }
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(aOut + (uintptr)i))).Value = c;
        goto _1;
_1:;
        i = i + 1;
    }
}

internal static void _autoAdjustDate(ref libc.TLS tls, uintptr p) {
    float64 r = default;
    _ = r;
    if (!((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(p + 44)) & 0x1) >> 0) != 0) || (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidJD != 0) {
        libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(0), 0, 0x1);
    } else {
        if ((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fs >= (float64)((int64)(-libc.Int32FromInt32(21086676)) * libc.Int64FromInt32(10000)) && (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fs <= (float64)(libc.Int64FromInt32(25340230) * libc.Int64FromInt32(10000) + libc.Int64FromInt32(799))) {
            r = (float64)((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fs * (float64)1000D) + (float64)2.1086676e+14D;
            _clearYMD_HMS_TZ(ref tls, (@unsafe.Pointer)p);
            (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FiJD = (int64)(r + libc.Float64FromFloat64(0.5D));
            (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidJD = (int8)1;
            libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(0), 0, 0x1);
        }
    }
}

internal static void _clearYMD_HMS_TZ(ref libc.TLS tls, @unsafe.Pointer p) {
    (@unsafe.Pointer.To<int8>(@unsafe.Add(p, 41))).Value = 0;
    (@unsafe.Pointer.To<int8>(@unsafe.Add(p, 42))).Value = 0;
    (@unsafe.Pointer.To<int32>(@unsafe.Add(p, 28))).Value = 0;
}

internal static void _codeApplyAffinity(ж<libc.TLS> tls, uintptr pParse, int32 @base, int32 n, uintptr zAff) {
    uintptr v = default;
    _ = v;
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    if (zAff == (uintptr)0) {
        return;
    }
    while (n > 0 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zAff))) <= (int32)SQLITE_AFF_BLOB) {
        n = n - 1;
        @base = @base + 1;
        zAff = zAff + 1;
    }
    while (n > (int32)1 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zAff + (uintptr)(n - (int32)1)))) <= (int32)SQLITE_AFF_BLOB) {
        n = n - 1;
    }
    if (n > 0) {
        _sqlite3VdbeAddOp4(tls, v, (int32)OP_Affinity, @base, n, 0, zAff, n);
    }
}

internal static void _computeFloor(ref libc.TLS tls, uintptr p) {
    int32 mxD = default;
    int32 v1 = default;
    _ = mxD;
    _ = v1;
    mxD = _daysOfMonth(ref tls, (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FY, (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FM);
    if ((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FD > mxD) {
        v1 = (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FD - mxD;
    } else {
        v1 = 0;
    }
    (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FnFloor = (int8)v1;
}

internal static void _computeHMS(ж<libc.TLS> tls, uintptr p) {
    int32 day_min = default;
    int32 day_ms = default;
    _ = day_min;
    _ = day_ms;
    if ((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidHMS != 0) {
        return;
    }
    _computeJD(tls, p);
    day_ms = (int32)(rem(((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FiJD + libc.Int64FromInt32(43200000)), libc.Int64FromInt32(86400000)));
    (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fs = (float64)(rem(day_ms, libc.Int32FromInt32(60000))) / (float64)1000D;
    day_min = day_ms / (int32)60000;
    (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fm = day_min % (int32)60;
    (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fh = day_min / (int32)60;
    libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(0), 0, 0x1);
    (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidHMS = (int8)1;
}

internal static void _computeJD(ж<libc.TLS> tls, uintptr p) {
    int32 A = default;
    int32 B = default;
    int32 D = default;
    int32 M = default;
    int32 X1 = default;
    int32 X2 = default;
    int32 Y = default;
    _ = A;
    _ = B;
    _ = D;
    _ = M;
    _ = X1;
    _ = X2;
    _ = Y;
    if ((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidJD != 0) {
        return;
    }
    if ((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidYMD != 0) {
        Y = (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FY;
        M = (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FM;
        D = (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FD;
    } else {
        Y = (int32)2000;
        M = (int32)1;
        D = (int32)1;
        if ((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidHMS != 0 && (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fh == (int32)24) {
            (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fh = 0;
            D = (int32)2;
        }
    }
    if (Y < -(int32)4713 || Y > (int32)9999 || (int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(p + 44)) & 0x1) >> 0) != 0) {
        _datetimeError(tls, p);
        return;
    }
    if (M <= (int32)2) {
        Y = Y - 1;
        M = M + (int32)12;
    }
    A = (Y + (int32)4800) / (int32)100;
    B = (int32)38 - A + A / (int32)4;
    X1 = (int32)36525 * (Y + (int32)4716) / (int32)100;
    X2 = (int32)306001 * (M + (int32)1) / (int32)10000;
    (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FiJD = (int64)(float64)(((float64)(X1 + X2 + D + B) - libc.Float64FromFloat64(1524.5D)) * libc.Float64FromInt32(86400000));
    (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidJD = (int8)1;
    if ((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidHMS != 0) {
        (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(p))).Value += (int64)((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fh * (int32)3600000 + (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fm * (int32)60000) + (int64)((float64)((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fs * libc.Float64FromInt32(1000)) + libc.Float64FromFloat64(0.5D));
        if ((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Ftz != 0) {
            (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(p))).Value -= (int64)((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Ftz * (int32)60000);
            (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidYMD = 0;
            (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidHMS = 0;
            (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Ftz = 0;
            libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(1), 3, 0x8);
            libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(0), 4, 0x10);
        }
    }
}

internal static void _computeYMD(ж<libc.TLS> tls, uintptr p) {
    int32 A = default;
    int32 B = default;
    int32 C = default;
    int32 D = default;
    int32 E = default;
    int32 X1 = default;
    int32 Z = default;
    int32 alpha = default;
    int32 v1 = default;
    _ = A;
    _ = B;
    _ = C;
    _ = D;
    _ = E;
    _ = X1;
    _ = Z;
    _ = alpha;
    _ = v1;
    if ((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidYMD != 0) {
        return;
    }
    if (!((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidJD != 0)) {
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FY = (int32)2000;
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FM = (int32)1;
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FD = (int32)1;
    } else {
        if (!(_validJulianDay(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FiJD) != 0)) {
            _datetimeError(tls, p);
            return;
        } else {
            Z = (int32)(quo(((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FiJD + libc.Int64FromInt32(43200000)), libc.Int64FromInt32(86400000)));
            alpha = (int32)(((float64)Z + libc.Float64FromFloat64(32044.75D)) / libc.Float64FromFloat64(36524.25D)) - (int32)52;
            A = Z + (int32)1 + alpha - (alpha + (int32)100) / (int32)4 + (int32)25;
            B = A + (int32)1524;
            C = (int32)(((float64)B - libc.Float64FromFloat64(122.1D)) / libc.Float64FromFloat64(365.25D));
            D = (int32)36525 * (C & (int32)32767) / (int32)100;
            E = (int32)((float64)(B - D) / libc.Float64FromFloat64(30.6001D));
            X1 = (int32)(float64)(libc.Float64FromFloat64(30.6001D) * (float64)E);
            (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FD = B - D - X1;
            if (E < (int32)14) {
                v1 = E - (int32)1;
            } else {
                v1 = E - (int32)13;
            }
            (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FM = v1;
            if ((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FM > (int32)2) {
                v1 = C - (int32)4716;
            } else {
                v1 = C - (int32)4715;
            }
            (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FY = v1;
        }
    }
    (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidYMD = (int8)1;
}

internal static readonly ж<array<int8>> Ꮡ_cume_distName = new StaticBox<array<int8>>(new int8[]{(int8)'c', (int8)'u', (int8)'m', (int8)'e', (int8)'_', (int8)'d', (int8)'i', (int8)'s', (int8)'t'}.array(10));
internal static ref array<int8> _cume_distName => ref ((StaticBox<array<int8>>)Ꮡ_cume_distName).ValueSlot;

internal static uintptr /*r*/ _databaseName(ref libc.TLS tls, uintptr zName) {
    while ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zName + (uintptr)(-libc.Int32FromInt32(1))))) != 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zName + (uintptr)(-libc.Int32FromInt32(2))))) != 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zName + (uintptr)(-libc.Int32FromInt32(3))))) != 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zName + (uintptr)(-libc.Int32FromInt32(4))))) != 0) {
        zName = zName - 1;
    }
    return zName;
}

internal static void _dateFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        int32 Y = default;
        TDateTime _ᴛ1 = default;
        array<int8> _ᴛ2 = new(16);
        _ = Y;
        if (_isDate(tls, context, argc, argv, bp) == 0) {
            _computeYMD(tls, bp);
            Y = (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FY;
            if (Y < 0) {
                Y = -Y;
            }
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)1, 16) = (int8)((int32)'0' + Y / (int32)1000 % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)2, 16) = (int8)((int32)'0' + Y / (int32)100 % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)3, 16) = (int8)((int32)'0' + Y / (int32)10 % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)4, 16) = (int8)((int32)'0' + Y % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)5, 16) = (int8)'-';
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)6, 16) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FM / (int32)10 % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)7, 16) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FM % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)8, 16) = (int8)'-';
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)9, 16) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FD / (int32)10 % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)10, 16) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FD % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)11, 16) = 0;
            if ((@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FY < 0) {
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), 0, 16) = (int8)'-';
                Xsqlite3_result_text(tls, context, bp + 48, (int32)11, (uintptr)(-libc.Int32FromInt32(1)));
            } else {
                Xsqlite3_result_text(tls, context, bp + 48 + 1, (int32)10, (uintptr)(-libc.Int32FromInt32(1)));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _datetimeFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        int32 Y = default;
        int32 n = default;
        int32 s = default;
        TDateTime _ᴛ1 = default;
        array<int8> _ᴛ2 = new(32);
        _ = Y;
        _ = n;
        _ = s;
        if (_isDate(tls, context, argc, argv, bp) == 0) {
            _computeYMD_HMS(tls, bp);
            Y = (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FY;
            if (Y < 0) {
                Y = -Y;
            }
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)1, 32) = (int8)((int32)'0' + Y / (int32)1000 % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)2, 32) = (int8)((int32)'0' + Y / (int32)100 % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)3, 32) = (int8)((int32)'0' + Y / (int32)10 % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)4, 32) = (int8)((int32)'0' + Y % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)5, 32) = (int8)'-';
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)6, 32) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FM / (int32)10 % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)7, 32) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FM % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)8, 32) = (int8)'-';
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)9, 32) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FD / (int32)10 % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)10, 32) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FD % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)11, 32) = (int8)' ';
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)12, 32) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.Fh / (int32)10 % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)13, 32) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.Fh % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)14, 32) = (int8)':';
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)15, 32) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.Fm / (int32)10 % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)16, 32) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.Fm % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)17, 32) = (int8)':';
            if ((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(bp + 44)) & 0x4) >> 2) != 0) {
                s = (int32)((float64)(libc.Float64FromFloat64(1000D) * (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.Fs) + libc.Float64FromFloat64(0.5D));
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)18, 32) = (int8)((int32)'0' + s / (int32)10000 % (int32)10);
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)19, 32) = (int8)((int32)'0' + s / (int32)1000 % (int32)10);
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)20, 32) = (int8)'.';
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)21, 32) = (int8)((int32)'0' + s / (int32)100 % (int32)10);
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)22, 32) = (int8)((int32)'0' + s / (int32)10 % (int32)10);
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)23, 32) = (int8)((int32)'0' + s % (int32)10);
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)24, 32) = 0;
                n = (int32)24;
            } else {
                s = (int32)(@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.Fs;
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)18, 32) = (int8)((int32)'0' + s / (int32)10 % (int32)10);
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)19, 32) = (int8)((int32)'0' + s % (int32)10);
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)20, 32) = 0;
                n = (int32)20;
            }
            if ((@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FY < 0) {
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), 0, 32) = (int8)'-';
                Xsqlite3_result_text(tls, context, bp + 48, n, (uintptr)(-libc.Int32FromInt32(1)));
            } else {
                Xsqlite3_result_text(tls, context, bp + 48 + 1, n - (int32)1, (uintptr)(-libc.Int32FromInt32(1)));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _daysAfterJan01(ж<libc.TLS> tls, @unsafe.Pointer pDate) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TDateTime _ᴛ1 = default;
        (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value = (@unsafe.Pointer.To<TDateTime>(pDate)).Value;
        (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FvalidJD = 0;
        (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FM = (int32)1;
        (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FD = (int32)1;
        _computeJD(tls, bp);
        r = (int32)(quo((~@unsafe.Pointer.To<Tsqlite3_int64>(pDate) - (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FiJD + libc.Int64FromInt32(43200000)), libc.Int64FromInt32(86400000)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static readonly ж<array<int8>> Ꮡ_dense_rankName = new StaticBox<array<int8>>(new int8[]{(int8)'d', (int8)'e', (int8)'n', (int8)'s', (int8)'e', (int8)'_', (int8)'r', (int8)'a', (int8)'n', (int8)'k'}.array(11));
internal static ref array<int8> _dense_rankName => ref ((StaticBox<array<int8>>)Ꮡ_dense_rankName).ValueSlot;

internal static Ti64 /*r*/ _findNextHostParameter(ref libc.TLS tls, uintptr zSql, @unsafe.Pointer pnToken) {
    Ti64 n = default;
    Ti64 nTotal = default;
    ref var tokenType = ref heap(new int32(), out var ᏑtokenType);
    _ = n;
    _ = nTotal;
    _ = tokenType;
    nTotal = 0;
    (@unsafe.Pointer.To<Ti64>(pnToken)).Value = 0;
    while (~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql)) != 0) {
        n = _sqlite3GetToken(ref tls, zSql, @unsafe.Pointer.FromPinnedBox(ᏑtokenType));
        if (tokenType == (int32)TK_VARIABLE) {
            (@unsafe.Pointer.To<Ti64>(pnToken)).Value = n;
            break;
        }
        nTotal = nTotal + n;
        zSql = zSql + (uintptr)n;
    }
    return nTotal;
}

internal static readonly ж<array<int8>> Ꮡ_first_valueName = new StaticBox<array<int8>>(new int8[]{(int8)'f', (int8)'i', (int8)'r', (int8)'s', (int8)'t', (int8)'_', (int8)'v', (int8)'a', (int8)'l', (int8)'u', (int8)'e'}.array(12));
internal static ref array<int8> _first_valueName => ref ((StaticBox<array<int8>>)Ꮡ_first_valueName).ValueSlot;

[GoLazy] partial class lazyᴛ_fmtinfo { internal static readonly ж<array<Tet_info>> Ꮡ_fmtinfo = new StaticBox<array<Tet_info>>(new array<Tet_info>(25){
    [0] = new(
        Ffmttype: (int8)'d',
        Fbase: (uint8)10,
        Fflags: (uint8)1,
        Ftype1: (uint8)etDECIMAL
    ),
    [1] = new(
        Ffmttype: (int8)'e',
        Fflags: (uint8)1,
        Ftype1: (uint8)etEXP,
        Fcharset: (uint8)30
    ),
    [2] = new(
        Ffmttype: (int8)'f',
        Fflags: (uint8)1,
        Ftype1: (uint8)etFLOAT
    ),
    [3] = new(
        Ffmttype: (int8)'g',
        Fflags: (uint8)1,
        Ftype1: (uint8)etGENERIC,
        Fcharset: (uint8)30
    ),
    [4] = new(
        Ffmttype: (int8)'j',
        Ftype1: (uint8)etESCAPE_j
    ),
    [5] = new(
        Ffmttype: (int8)'i',
        Fbase: (uint8)10,
        Fflags: (uint8)1,
        Ftype1: (uint8)etDECIMAL
    ),
    [6] = new(
        Ffmttype: (int8)'Q',
        Fflags: (uint8)4,
        Ftype1: (uint8)etESCAPE_Q,
        FiNxt: (int8)4
    ),
    [7] = new(
        Ffmttype: (int8)'p',
        Fbase: (uint8)16,
        Ftype1: (uint8)etPOINTER,
        Fprefix: (uint8)1
    ),
    [8] = new(
        Ffmttype: (int8)'S',
        Ftype1: (uint8)etSRCITEM
    ),
    [9] = new(
        Ffmttype: (int8)'T',
        Ftype1: (uint8)etTOKEN
    ),
    [10] = new(
        Ffmttype: (int8)'n',
        Ftype1: (uint8)etSIZE
    ),
    [11] = new(
        Ffmttype: (int8)'o',
        Fbase: (uint8)8,
        Fprefix: (uint8)2
    ),
    [12] = new(
        Ffmttype: (int8)'%',
        Ftype1: (uint8)etPERCENT,
        FiNxt: (int8)7
    ),
    [13] = new(
        Ffmttype: (int8)'q',
        Fflags: (uint8)4,
        Ftype1: (uint8)etESCAPE_q,
        FiNxt: (int8)16
    ),
    [14] = new(
        Ffmttype: (int8)'r',
        Fbase: (uint8)10,
        Fflags: (uint8)1,
        Ftype1: (uint8)etORDINAL
    ),
    [15] = new(
        Ffmttype: (int8)'s',
        Fflags: (uint8)4,
        Ftype1: (uint8)etSTRING
    ),
    [16] = new(
        Ffmttype: (int8)'X',
        Fbase: (uint8)16,
        Fprefix: (uint8)4
    ),
    [17] = new(
        Ffmttype: (int8)'u',
        Fbase: (uint8)10,
        Ftype1: (uint8)etDECIMAL
    ),
    [18] = new(
        Ffmttype: (int8)'w',
        Fflags: (uint8)4,
        Ftype1: (uint8)etESCAPE_w
    ),
    [19] = new(
        Ffmttype: (int8)'E',
        Fflags: (uint8)1,
        Ftype1: (uint8)etEXP,
        Fcharset: (uint8)14,
        FiNxt: (int8)18
    ),
    [20] = new(
        Ffmttype: (int8)'x',
        Fbase: (uint8)16,
        Fcharset: (uint8)16,
        Fprefix: (uint8)1
    ),
    [21] = new(
        Ffmttype: (int8)'G',
        Fflags: (uint8)1,
        Ftype1: (uint8)etGENERIC,
        Fcharset: (uint8)14
    ),
    [22] = new(
        Ffmttype: (int8)'z',
        Fflags: (uint8)4,
        Ftype1: (uint8)etDYNSTRING
    ),
    [23] = new(
        Ffmttype: (int8)'J',
        Ftype1: (uint8)etESCAPE_J
    ),
    [24] = new(
        Ffmttype: (int8)'c',
        Ftype1: (uint8)etCHARX,
        FiNxt: (int8)23
    )
}); }
internal static ref array<Tet_info> _fmtinfo => ref ((StaticBox<array<Tet_info>>)Ꮡ_fmtinfo).ValueSlot;

internal static uintptr /*r*/ _fts5ConfigSkipArgs(ж<libc.TLS> tls, uintptr pIn) {
    uintptr p = default;
    _ = p;
    p = pIn;
    while ((int32)1 != 0) {
        p = _fts5ConfigSkipWhitespace(ref (tls).DerefOrNull(), p);
        p = _fts5ConfigSkipLiteral(tls, p);
        p = _fts5ConfigSkipWhitespace(ref (tls).DerefOrNull(), p);
        if (p == (uintptr)0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) == (int32)')') {
            break;
        }
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) != (int32)',') {
            p = (uintptr)0;
            break;
        }
        p = p + 1;
    }
    return p;
}

internal static uintptr /*r*/ _fts5ConfigSkipBareword(ref libc.TLS tls, uintptr pIn) {
    uintptr p = default;
    _ = p;
    p = pIn;
    while (_sqlite3Fts5IsBareword(ref tls, ~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) != 0) {
        p = p + 1;
    }
    if (p == pIn) {
        p = (uintptr)0;
    }
    return p;
}

internal static uintptr /*r*/ _fts5ConfigSkipWhitespace(ref libc.TLS tls, uintptr pIn) {
    uintptr p = default;
    _ = p;
    p = pIn;
    if (p != 0) {
        while (_fts5_iswhitespace(ref tls, ~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) != 0) {
            p = p + 1;
        }
    }
    return p;
}

internal static int32 /*r*/ _fts5Dequote(ref libc.TLS tls, uintptr z) {
    int32 iIn = default;
    int32 iOut = default;
    int32 v1 = default;
    int32 v2 = default;
    int8 q = default;
    _ = iIn;
    _ = iOut;
    _ = q;
    _ = v1;
    _ = v2;
    iIn = (int32)1;
    iOut = 0;
    q = ~@unsafe.Pointer.To<int8>(__ccgo_ptr(z));
    if ((int32)q == (int32)'[') {
        q = (int8)']';
    }
    while (~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)iIn)) != 0) {
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)iIn))) == (int32)q) {
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(iIn + (int32)1)))) != (int32)q) {
                iIn = iIn + 1;
                break;
            } else {
                iIn = iIn + (int32)2;
                v1 = iOut;
                iOut = iOut + 1;
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)v1))).Value = q;
            }
        } else {
            v1 = iOut;
            iOut = iOut + 1;
            v2 = iIn;
            iIn = iIn + 1;
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)v1))).Value = (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)v2))).Value;
        }
    }
    (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)iOut))).Value = (int8)'\u0000';
    return iIn;
}

internal static int32 /*r*/ _fts5ExprCountChar(ref libc.TLS tls, uintptr z, int32 nByte) {
    int32 ii = default;
    int32 nRet = default;
    _ = ii;
    _ = nRet;
    nRet = 0;
    ii = 0;
    for (;;) {
        if (!(ii < nByte)) {
            break;
        }
        if (((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)ii))) & (int32)0xC0) != (int32)0x80) {
            nRet = nRet + 1;
        }
        goto _1;
_1:;
        ii = ii + 1;
    }
    return nRet;
}

internal static int32 /*r*/ _fts5ExprIsspace(ref libc.TLS tls, int8 t) {
    return libc.BoolInt32((int32)t == (int32)' ' || (int32)t == (int32)'\t' || (int32)t == (int32)'\n' || (int32)t == (int32)'\r');
}

internal static int32 /*r*/ _fts5PorterGobbleVC(ref libc.TLS tls, uintptr zStem, int32 nStem, int32 bPrevCons) {
    int32 bCons = default;
    int32 i = default;
    int32 v2 = default;
    _ = bCons;
    _ = i;
    _ = v2;
    bCons = bPrevCons;
    i = 0;
    for (;;) {
        if (!(i < nStem)) {
            break;
        }
        v2 = libc.BoolInt32(!(_fts5PorterIsVowel(ref tls, ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zStem + (uintptr)i)), bCons) != 0));
        bCons = v2;
        if (0 == v2) {
            break;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    i = i + 1;
    for (;;) {
        if (!(i < nStem)) {
            break;
        }
        v2 = libc.BoolInt32(!(_fts5PorterIsVowel(ref tls, ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zStem + (uintptr)i)), bCons) != 0));
        bCons = v2;
        if (v2 != 0) {
            return i + (int32)1;
        }
        goto _3;
_3:;
        i = i + 1;
    }
    return 0;
}

internal static int32 /*r*/ _fts5PorterIsVowel(ref libc.TLS tls, int8 c, int32 bYIsVowel) {
    return libc.BoolInt32((int32)c == (int32)'a' || (int32)c == (int32)'e' || (int32)c == (int32)'i' || (int32)c == (int32)'o' || (int32)c == (int32)'u' || bYIsVowel != 0 && (int32)c == (int32)'y');
}

internal static void _fts5PorterStep1A(ref libc.TLS tls, uintptr aBuf, @unsafe.Pointer pnBuf) {
    int32 nBuf = default;
    _ = nBuf;
    nBuf = ~@unsafe.Pointer.To<int32>(pnBuf);
    if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(aBuf + (uintptr)(nBuf - (int32)1)))) == (int32)'s') {
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(aBuf + (uintptr)(nBuf - (int32)2)))) == (int32)'e') {
            if (nBuf > (int32)4 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(aBuf + (uintptr)(nBuf - (int32)4)))) == (int32)'s' && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(aBuf + (uintptr)(nBuf - (int32)3)))) == (int32)'s' || nBuf > (int32)3 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(aBuf + (uintptr)(nBuf - (int32)3)))) == (int32)'i') {
                (@unsafe.Pointer.To<int32>(pnBuf)).Value = nBuf - (int32)2;
            } else {
                (@unsafe.Pointer.To<int32>(pnBuf)).Value = nBuf - (int32)1;
            }
        } else {
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(aBuf + (uintptr)(nBuf - (int32)2)))) != (int32)'s') {
                (@unsafe.Pointer.To<int32>(pnBuf)).Value = nBuf - (int32)1;
            }
        }
    }
}

internal static int32 /*r*/ _fts5Porter_MGt1_and_S_or_T(ref libc.TLS tls, uintptr zStem, int32 nStem) {
    return libc.BoolInt32(((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zStem + (uintptr)(nStem - (int32)1)))) == (int32)'s' || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zStem + (uintptr)(nStem - (int32)1)))) == (int32)'t') && _fts5Porter_MGt1(ref tls, zStem, nStem) != 0);
}

internal static int32 /*r*/ _fts5Porter_Ostar(ref libc.TLS tls, uintptr zStem, int32 nStem) {
    int32 r = default;

    int32 bCons = default;
    int32 i = default;
    int32 mask = default;
    _ = bCons;
    _ = i;
    _ = mask;
    if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zStem + (uintptr)(nStem - (int32)1)))) == (int32)'w' || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zStem + (uintptr)(nStem - (int32)1)))) == (int32)'x' || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zStem + (uintptr)(nStem - (int32)1)))) == (int32)'y') {
        return 0;
    } else {
        mask = 0;
        bCons = 0;
        i = 0;
        for (;;) {
            if (!(i < nStem)) {
                break;
            }
            bCons = libc.BoolInt32(!(_fts5PorterIsVowel(ref tls, ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zStem + (uintptr)i)), bCons) != 0));
            mask = ((mask << (int)((int32)1)) + bCons) & (int32)0x0007;
            goto _1;
_1:;
            i = i + 1;
        }
        return libc.BoolInt32(mask == (int32)0x0005);
    }
    return r;
}

internal static int32 /*r*/ _fts5Porter_Vowel(ref libc.TLS tls, uintptr zStem, int32 nStem) {
    int32 i = default;
    _ = i;
    i = 0;
    for (;;) {
        if (!(i < nStem)) {
            break;
        }
        if (_fts5PorterIsVowel(ref tls, ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zStem + (uintptr)i)), libc.BoolInt32(i > 0)) != 0) {
            return (int32)1;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return 0;
}

internal static int32 /*r*/ _fts5QueryTerm(ref libc.TLS tls, uintptr pToken, int32 nToken) {
    int32 ii = default;
    _ = ii;
    ii = 0;
    for (;;) {
        if (!(ii < nToken && ~@unsafe.Pointer.To<int8>(__ccgo_ptr(pToken + (uintptr)ii)) != 0)) {
            break;
        }
        goto _1;
_1:;
        ii = ii + 1;
    }
    return ii;
}

internal static int32 /*r*/ _fts5SentenceFinderCb(ж<libc.TLS> tls, uintptr pContext, int32 tflags, uintptr pToken, int32 nToken, int32 iStartOff, int32 iEndOff) {
    int8 c = default;
    int32 i = default;
    int32 rc = default;
    uintptr p = default;
    _ = c;
    _ = i;
    _ = p;
    _ = rc;
    rc = SQLITE_OK;
    _ = pToken;
    _ = nToken;
    _ = iEndOff;
    if ((tflags & (int32)FTS5_TOKEN_COLOCATED) == 0) {
        p = pContext;
        if ((@unsafe.Pointer.To<TFts5SFinder>((@unsafe.Pointer)p)).Value.FiPos > 0) {
            c = 0;
            i = iStartOff - (int32)1;
            for (;;) {
                if (!(i >= 0)) {
                    break;
                }
                c = ~@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5SFinder>((@unsafe.Pointer)p)).Value.FzDoc + (uintptr)i));
                if ((int32)c != (int32)' ' && (int32)c != (int32)'\t' && (int32)c != (int32)'\n' && (int32)c != (int32)'\r') {
                    break;
                }
                goto _1;
_1:;
                i = i - 1;
            }
            if (i != iStartOff - (int32)1 && ((int32)c == (int32)'.' || (int32)c == (int32)':')) {
                rc = _fts5SentenceFinderAdd(tls, p, (@unsafe.Pointer.To<TFts5SFinder>((@unsafe.Pointer)p)).Value.FiPos);
            }
        } else {
            rc = _fts5SentenceFinderAdd(tls, p, 0);
        }
        (@unsafe.Pointer.To<TFts5SFinder>((@unsafe.Pointer)p)).Value.FiPos = (@unsafe.Pointer.To<TFts5SFinder>((@unsafe.Pointer)p)).Value.FiPos + 1;
    }
    return rc;
}

internal static int32 /*r*/ _fts5_isdigit(ref libc.TLS tls, int8 a) {
    return libc.BoolInt32((int32)a >= (int32)'0' && (int32)a <= (int32)'9');
}

internal static int32 /*r*/ _fts5_isopenquote(ref libc.TLS tls, int8 x) {
    return libc.BoolInt32((int32)x == (int32)'"' || (int32)x == (int32)'\'' || (int32)x == (int32)'[' || (int32)x == (int32)'`');
}

internal static int32 /*r*/ _fts5_iswhitespace(ref libc.TLS tls, int8 x) {
    return libc.BoolInt32((int32)x == (int32)' ');
}

[GoLazy] partial class lazyᴛ_geopolyIsSpace { internal static array<int8> _geopolyIsSpace = new array<int8>(256){
    [9] = (int8)1,
    [10] = (int8)1,
    [13] = (int8)1,
    [32] = (int8)1
}; }

[GoLazy] partial class lazyᴛ_hexdigits { internal static array<int8> _hexdigits = new array<int8>(16){
    [0] = (int8)'0',
    [1] = (int8)'1',
    [2] = (int8)'2',
    [3] = (int8)'3',
    [4] = (int8)'4',
    [5] = (int8)'5',
    [6] = (int8)'6',
    [7] = (int8)'7',
    [8] = (int8)'8',
    [9] = (int8)'9',
    [10] = (int8)'A',
    [11] = (int8)'B',
    [12] = (int8)'C',
    [13] = (int8)'D',
    [14] = (int8)'E',
    [15] = (int8)'F'
}; }

internal static Ti64 /*r*/ _identLength(ref libc.TLS tls, uintptr z) {
    Ti64 n = default;
    _ = n;
    n = 0;
    for (;;) {
        if (!(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z)) != 0)) {
            break;
        }
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) == (int32)'"') {
            n = n + 1;
        }
        goto _1;
_1:;
        n = n + 1;
        z = z + 1;
    }
    return n + (int64)2;
}

internal static int32 /*r*/ _isAllZero(ref libc.TLS tls, uintptr z, int32 n) {
    int32 i = default;
    _ = i;
    i = 0;
    for (;;) {
        if (!(i < n)) {
            break;
        }
        if (~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i)) != 0) {
            return 0;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return (int32)1;
}

internal static void _jsonAppendSeparator(ж<libc.TLS> tls, uintptr p) {
    int8 c = default;
    _ = c;
    if ((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed == (uint64)0) {
        return;
    }
    c = ~@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf + (uintptr)((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed - (uint64)1)));
    if ((int32)c == (int32)'[' || (int32)c == (int32)'{') {
        return;
    }
    _jsonAppendChar(tls, p, (int8)',');
}

internal static int32 /*r*/ _jsonIs4HexB(ref libc.TLS tls, uintptr z, @unsafe.Pointer pOp) {
    if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) != (int32)'u') {
        return 0;
    }
    if (!(_jsonIs4Hex(ref tls, z + 1) != 0)) {
        return 0;
    }
    (@unsafe.Pointer.To<int32>(pOp)).Value = (int32)JSONB_TEXTJ;
    return (int32)1;
}

[GoLazy] partial class lazyᴛ_jsonIsOk { internal static array<int8> _jsonIsOk = new array<int8>(256){
    [32] = (int8)1,
    [33] = (int8)1,
    [35] = (int8)1,
    [36] = (int8)1,
    [37] = (int8)1,
    [38] = (int8)1,
    [40] = (int8)1,
    [41] = (int8)1,
    [42] = (int8)1,
    [43] = (int8)1,
    [44] = (int8)1,
    [45] = (int8)1,
    [46] = (int8)1,
    [47] = (int8)1,
    [48] = (int8)1,
    [49] = (int8)1,
    [50] = (int8)1,
    [51] = (int8)1,
    [52] = (int8)1,
    [53] = (int8)1,
    [54] = (int8)1,
    [55] = (int8)1,
    [56] = (int8)1,
    [57] = (int8)1,
    [58] = (int8)1,
    [59] = (int8)1,
    [60] = (int8)1,
    [61] = (int8)1,
    [62] = (int8)1,
    [63] = (int8)1,
    [64] = (int8)1,
    [65] = (int8)1,
    [66] = (int8)1,
    [67] = (int8)1,
    [68] = (int8)1,
    [69] = (int8)1,
    [70] = (int8)1,
    [71] = (int8)1,
    [72] = (int8)1,
    [73] = (int8)1,
    [74] = (int8)1,
    [75] = (int8)1,
    [76] = (int8)1,
    [77] = (int8)1,
    [78] = (int8)1,
    [79] = (int8)1,
    [80] = (int8)1,
    [81] = (int8)1,
    [82] = (int8)1,
    [83] = (int8)1,
    [84] = (int8)1,
    [85] = (int8)1,
    [86] = (int8)1,
    [87] = (int8)1,
    [88] = (int8)1,
    [89] = (int8)1,
    [90] = (int8)1,
    [91] = (int8)1,
    [93] = (int8)1,
    [94] = (int8)1,
    [95] = (int8)1,
    [96] = (int8)1,
    [97] = (int8)1,
    [98] = (int8)1,
    [99] = (int8)1,
    [100] = (int8)1,
    [101] = (int8)1,
    [102] = (int8)1,
    [103] = (int8)1,
    [104] = (int8)1,
    [105] = (int8)1,
    [106] = (int8)1,
    [107] = (int8)1,
    [108] = (int8)1,
    [109] = (int8)1,
    [110] = (int8)1,
    [111] = (int8)1,
    [112] = (int8)1,
    [113] = (int8)1,
    [114] = (int8)1,
    [115] = (int8)1,
    [116] = (int8)1,
    [117] = (int8)1,
    [118] = (int8)1,
    [119] = (int8)1,
    [120] = (int8)1,
    [121] = (int8)1,
    [122] = (int8)1,
    [123] = (int8)1,
    [124] = (int8)1,
    [125] = (int8)1,
    [126] = (int8)1,
    [127] = (int8)1,
    [128] = (int8)1,
    [129] = (int8)1,
    [130] = (int8)1,
    [131] = (int8)1,
    [132] = (int8)1,
    [133] = (int8)1,
    [134] = (int8)1,
    [135] = (int8)1,
    [136] = (int8)1,
    [137] = (int8)1,
    [138] = (int8)1,
    [139] = (int8)1,
    [140] = (int8)1,
    [141] = (int8)1,
    [142] = (int8)1,
    [143] = (int8)1,
    [144] = (int8)1,
    [145] = (int8)1,
    [146] = (int8)1,
    [147] = (int8)1,
    [148] = (int8)1,
    [149] = (int8)1,
    [150] = (int8)1,
    [151] = (int8)1,
    [152] = (int8)1,
    [153] = (int8)1,
    [154] = (int8)1,
    [155] = (int8)1,
    [156] = (int8)1,
    [157] = (int8)1,
    [158] = (int8)1,
    [159] = (int8)1,
    [160] = (int8)1,
    [161] = (int8)1,
    [162] = (int8)1,
    [163] = (int8)1,
    [164] = (int8)1,
    [165] = (int8)1,
    [166] = (int8)1,
    [167] = (int8)1,
    [168] = (int8)1,
    [169] = (int8)1,
    [170] = (int8)1,
    [171] = (int8)1,
    [172] = (int8)1,
    [173] = (int8)1,
    [174] = (int8)1,
    [175] = (int8)1,
    [176] = (int8)1,
    [177] = (int8)1,
    [178] = (int8)1,
    [179] = (int8)1,
    [180] = (int8)1,
    [181] = (int8)1,
    [182] = (int8)1,
    [183] = (int8)1,
    [184] = (int8)1,
    [185] = (int8)1,
    [186] = (int8)1,
    [187] = (int8)1,
    [188] = (int8)1,
    [189] = (int8)1,
    [190] = (int8)1,
    [191] = (int8)1,
    [192] = (int8)1,
    [193] = (int8)1,
    [194] = (int8)1,
    [195] = (int8)1,
    [196] = (int8)1,
    [197] = (int8)1,
    [198] = (int8)1,
    [199] = (int8)1,
    [200] = (int8)1,
    [201] = (int8)1,
    [202] = (int8)1,
    [203] = (int8)1,
    [204] = (int8)1,
    [205] = (int8)1,
    [206] = (int8)1,
    [207] = (int8)1,
    [208] = (int8)1,
    [209] = (int8)1,
    [210] = (int8)1,
    [211] = (int8)1,
    [212] = (int8)1,
    [213] = (int8)1,
    [214] = (int8)1,
    [215] = (int8)1,
    [216] = (int8)1,
    [217] = (int8)1,
    [218] = (int8)1,
    [219] = (int8)1,
    [220] = (int8)1,
    [221] = (int8)1,
    [222] = (int8)1,
    [223] = (int8)1,
    [224] = (int8)1,
    [225] = (int8)1,
    [226] = (int8)1,
    [227] = (int8)1,
    [228] = (int8)1,
    [229] = (int8)1,
    [230] = (int8)1,
    [231] = (int8)1,
    [232] = (int8)1,
    [233] = (int8)1,
    [234] = (int8)1,
    [235] = (int8)1,
    [236] = (int8)1,
    [237] = (int8)1,
    [238] = (int8)1,
    [239] = (int8)1,
    [240] = (int8)1,
    [241] = (int8)1,
    [242] = (int8)1,
    [243] = (int8)1,
    [244] = (int8)1,
    [245] = (int8)1,
    [246] = (int8)1,
    [247] = (int8)1,
    [248] = (int8)1,
    [249] = (int8)1,
    [250] = (int8)1,
    [251] = (int8)1,
    [252] = (int8)1,
    [253] = (int8)1,
    [254] = (int8)1,
    [255] = (int8)1
}; }

[GoLazy] partial class lazyᴛ_jsonIsSpace { internal static array<int8> _jsonIsSpace = new array<int8>(256){
    [9] = (int8)1,
    [10] = (int8)1,
    [13] = (int8)1,
    [32] = (int8)1
}; }

internal static readonly ж<array<int8>> Ꮡ_jsonSpaces = new StaticBox<array<int8>>(new int8[]{9, 10, 13, (int8)' '}.array(5));
internal static ref array<int8> _jsonSpaces => ref ((StaticBox<array<int8>>)Ꮡ_jsonSpaces).ValueSlot;

internal static readonly ж<array<int8>> Ꮡ_lagName = new StaticBox<array<int8>>(new int8[]{(int8)'l', (int8)'a', (int8)'g'}.array(4));
internal static ref array<int8> _lagName => ref ((StaticBox<array<int8>>)Ꮡ_lagName).ValueSlot;

internal static readonly ж<array<int8>> Ꮡ_last_valueName = new StaticBox<array<int8>>(new int8[]{(int8)'l', (int8)'a', (int8)'s', (int8)'t', (int8)'_', (int8)'v', (int8)'a', (int8)'l', (int8)'u', (int8)'e'}.array(11));
internal static ref array<int8> _last_valueName => ref ((StaticBox<array<int8>>)Ꮡ_last_valueName).ValueSlot;

internal static readonly ж<array<int8>> Ꮡ_leadName = new StaticBox<array<int8>>(new int8[]{(int8)'l', (int8)'e', (int8)'a', (int8)'d'}.array(5));
internal static ref array<int8> _leadName => ref ((StaticBox<array<int8>>)Ꮡ_leadName).ValueSlot;

internal static readonly ж<array<int8>> Ꮡ_nth_valueName = new StaticBox<array<int8>>(new int8[]{(int8)'n', (int8)'t', (int8)'h', (int8)'_', (int8)'v', (int8)'a', (int8)'l', (int8)'u', (int8)'e'}.array(10));
internal static ref array<int8> _nth_valueName => ref ((StaticBox<array<int8>>)Ꮡ_nth_valueName).ValueSlot;

internal static readonly ж<array<int8>> Ꮡ_ntileName = new StaticBox<array<int8>>(new int8[]{(int8)'n', (int8)'t', (int8)'i', (int8)'l', (int8)'e'}.array(6));
internal static ref array<int8> _ntileName => ref ((StaticBox<array<int8>>)Ꮡ_ntileName).ValueSlot;

internal static readonly ж<array<int8>> Ꮡ_nullRow = new StaticBox<array<int8>>(new array<int8>(6){
    [0] = (int8)6
});
internal static ref array<int8> _nullRow => ref ((StaticBox<array<int8>>)Ꮡ_nullRow).ValueSlot;

internal static readonly ж<array<int8>> Ꮡ_percent_rankName = new StaticBox<array<int8>>(new int8[]{(int8)'p', (int8)'e', (int8)'r', (int8)'c', (int8)'e', (int8)'n', (int8)'t', (int8)'_', (int8)'r', (int8)'a', (int8)'n', (int8)'k'}.array(13));
internal static ref array<int8> _percent_rankName => ref ((StaticBox<array<int8>>)Ꮡ_percent_rankName).ValueSlot;

internal static readonly ж<array<int8>> Ꮡ_rankName = new StaticBox<array<int8>>(new int8[]{(int8)'r', (int8)'a', (int8)'n', (int8)'k'}.array(5));
internal static ref array<int8> _rankName => ref ((StaticBox<array<int8>>)Ꮡ_rankName).ValueSlot;

internal static uintptr /*r*/ _rbuObjIterGetBindlist(ж<libc.TLS> tls, uintptr p, int32 nBind) {
    int32 i = default;
    int32 v2 = default;
    Tsqlite3_int64 nByte = default;
    uintptr zRet = default;
    _ = i;
    _ = nByte;
    _ = zRet;
    _ = v2;
    zRet = (uintptr)0;
    nByte = (int64)2 * (int64)nBind + (int64)1;
    zRet = _rbuMalloc(tls, p, nByte);
    if (zRet != 0) {
        i = 0;
        for (;;) {
            if (!(i < nBind)) {
                break;
            }
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(zRet + (uintptr)(i * (int32)2)))).Value = (int8)'?';
            if (i + (int32)1 == nBind) {
                v2 = (int32)'\u0000';
            } else {
                v2 = (int32)',';
            }
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(zRet + (uintptr)(i * (int32)2 + (int32)1)))).Value = (int8)v2;
            goto _1;
_1:;
            i = i + 1;
        }
    }
    return zRet;
}

internal static readonly ж<array<int8>> Ꮡ_row_numberName = new StaticBox<array<int8>>(new int8[]{(int8)'r', (int8)'o', (int8)'w', (int8)'_', (int8)'n', (int8)'u', (int8)'m', (int8)'b', (int8)'e', (int8)'r'}.array(11));
internal static ref array<int8> _row_numberName => ref ((StaticBox<array<int8>>)Ꮡ_row_numberName).ValueSlot;

internal static int32 /*r*/ _setDateTimeToCurrent(ж<libc.TLS> tls, uintptr context, uintptr p) {
    int32 r = default;

    (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FiJD = _sqlite3StmtCurrentTime(tls, context);
    if ((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FiJD > 0) {
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidJD = (int8)1;
        libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(1), 3, 0x8);
        libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(0), 4, 0x10);
        _clearYMD_HMS_TZ(ref (tls).DerefOrNull(), (@unsafe.Pointer)p);
        return 0;
    } else {
        return (int32)1;
    }
    return r;
}

internal static void _setRawDateNumber(ref libc.TLS tls, uintptr p, float64 r) {
    (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fs = r;
    libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(1), 0, 0x1);
    if (r >= (float64)0D && r < (float64)5.3734845e+06D) {
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FiJD = (int64)((float64)(r * libc.Float64FromFloat64(8.64e+07D)) + libc.Float64FromFloat64(0.5D));
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidJD = (int8)1;
    }
}

internal static int8 /*r*/ _sqlite3CompareAffinity(ж<libc.TLS> tls, uintptr pExpr, int8 aff2) {
    int8 r = default;

    int8 aff1 = default;
    int32 v1 = default;
    _ = aff1;
    _ = v1;
    aff1 = _sqlite3ExprAffinity(tls, pExpr);
    if ((int32)aff1 > (int32)SQLITE_AFF_NONE && (int32)aff2 > (int32)SQLITE_AFF_NONE) {
        if ((int32)aff1 >= (int32)SQLITE_AFF_NUMERIC || (int32)aff2 >= (int32)SQLITE_AFF_NUMERIC) {
            return (int8)SQLITE_AFF_NUMERIC;
        } else {
            return (int8)SQLITE_AFF_BLOB;
        }
    } else {
        if ((int32)aff1 <= (int32)SQLITE_AFF_NONE) {
            v1 = (int32)aff2;
        } else {
            v1 = (int32)aff1;
        }
        return (int8)(v1 | (int32)SQLITE_AFF_NONE);
    }
    return r;
}


    [global::go.golib.GoInlineLayout(Size = 202, Align = 2), StructLayout(LayoutKind.Explicit, Size = 202)] partial struct Δtype /*dyn*/ {
        [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<int16>))] public readonly short FforceAlignment;
        [FieldOffset(0)] public array201<int8> Fa;
        [FieldOffset(201)] public array1<byte> F__ccgo_pad2;
    }

[global::go.golib.GoInlineLayout(Size = 202, Align = 1), StructLayout(LayoutKind.Explicit, Size = 202)] partial struct Δtypeᴛ1 /*dyn*/ {
    [FieldOffset(0)] internal array201<int8> f;
    [FieldOffset(201)] internal array1<byte> _;
}
internal static readonly ж<Δtype> Ꮡ_sqlite3DigitPairs = new StaticBox<Δtype>((Ꮡ(new Δtypeᴛ1(f: new array201<int8>((int8)'0', (int8)'0', (int8)'0', (int8)'1', (int8)'0', (int8)'2', (int8)'0', (int8)'3', (int8)'0', (int8)'4', (int8)'0', (int8)'5', (int8)'0', (int8)'6', (int8)'0', (int8)'7', (int8)'0', (int8)'8', (int8)'0', (int8)'9', (int8)'1', (int8)'0', (int8)'1', (int8)'1', (int8)'1', (int8)'2', (int8)'1', (int8)'3', (int8)'1', (int8)'4', (int8)'1', (int8)'5', (int8)'1', (int8)'6', (int8)'1', (int8)'7', (int8)'1', (int8)'8', (int8)'1', (int8)'9', (int8)'2', (int8)'0', (int8)'2', (int8)'1', (int8)'2', (int8)'2', (int8)'2', (int8)'3', (int8)'2', (int8)'4', (int8)'2', (int8)'5', (int8)'2', (int8)'6', (int8)'2', (int8)'7', (int8)'2', (int8)'8', (int8)'2', (int8)'9', (int8)'3', (int8)'0', (int8)'3', (int8)'1', (int8)'3', (int8)'2', (int8)'3', (int8)'3', (int8)'3', (int8)'4', (int8)'3', (int8)'5', (int8)'3', (int8)'6', (int8)'3', (int8)'7', (int8)'3', (int8)'8', (int8)'3', (int8)'9', (int8)'4', (int8)'0', (int8)'4', (int8)'1', (int8)'4', (int8)'2', (int8)'4', (int8)'3', (int8)'4', (int8)'4', (int8)'4', (int8)'5', (int8)'4', (int8)'6', (int8)'4', (int8)'7', (int8)'4', (int8)'8', (int8)'4', (int8)'9', (int8)'5', (int8)'0', (int8)'5', (int8)'1', (int8)'5', (int8)'2', (int8)'5', (int8)'3', (int8)'5', (int8)'4', (int8)'5', (int8)'5', (int8)'5', (int8)'6', (int8)'5', (int8)'7', (int8)'5', (int8)'8', (int8)'5', (int8)'9', (int8)'6', (int8)'0', (int8)'6', (int8)'1', (int8)'6', (int8)'2', (int8)'6', (int8)'3', (int8)'6', (int8)'4', (int8)'6', (int8)'5', (int8)'6', (int8)'6', (int8)'6', (int8)'7', (int8)'6', (int8)'8', (int8)'6', (int8)'9', (int8)'7', (int8)'0', (int8)'7', (int8)'1', (int8)'7', (int8)'2', (int8)'7', (int8)'3', (int8)'7', (int8)'4', (int8)'7', (int8)'5', (int8)'7', (int8)'6', (int8)'7', (int8)'7', (int8)'7', (int8)'8', (int8)'7', (int8)'9', (int8)'8', (int8)'0', (int8)'8', (int8)'1', (int8)'8', (int8)'2', (int8)'8', (int8)'3', (int8)'8', (int8)'4', (int8)'8', (int8)'5', (int8)'8', (int8)'6', (int8)'8', (int8)'7', (int8)'8', (int8)'8', (int8)'8', (int8)'9', (int8)'9', (int8)'0', (int8)'9', (int8)'1', (int8)'9', (int8)'2', (int8)'9', (int8)'3', (int8)'9', (int8)'4', (int8)'9', (int8)'5', (int8)'9', (int8)'6', (int8)'9', (int8)'7', (int8)'9', (int8)'8', (int8)'9', (int8)'9'))).Reinterpret<Δtypeᴛ1, Δtype>()).Value);
internal static ref Δtype _sqlite3DigitPairs => ref ((StaticBox<Δtype>)Ꮡ_sqlite3DigitPairs).ValueSlot;

internal static int32 /*r*/ _sqlite3ExprTruthValue(ref libc.TLS tls, uintptr pExpr) {
    pExpr = _sqlite3ExprSkipCollateAndLikely(ref tls, pExpr);
    return libc.BoolInt32((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 8)) + 4))) == 0);
}

internal static int32 /*r*/ _sqlite3Fts5DecodeLocaleValue(ж<libc.TLS> tls, uintptr pVal, @unsafe.Pointer ppText, @unsafe.Pointer pnText, @unsafe.Pointer ppLoc, @unsafe.Pointer pnLoc) {
    int32 n = default;
    int32 nLoc = default;
    uintptr p = default;
    _ = n;
    _ = nLoc;
    _ = p;
    p = Xsqlite3_value_blob(tls, pVal);
    n = Xsqlite3_value_bytes(tls, pVal);
    nLoc = 0;
    nLoc = libc.Int32FromInt64(16);
    for (;;) {
        if (!(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p + (uintptr)nLoc)) != 0)) {
            break;
        }
        if (nLoc == n - (int32)1) {
            return (int32)SQLITE_MISMATCH;
        }
        goto _1;
_1:;
        nLoc = nLoc + 1;
    }
    (@unsafe.Pointer.To<uintptr>(ppLoc)).Value = p + (uintptr)libc.Int32FromInt64(16);
    (@unsafe.Pointer.To<int32>(pnLoc)).Value = nLoc - libc.Int32FromInt64(16);
    (@unsafe.Pointer.To<uintptr>(ppText)).Value = p + (uintptr)(nLoc + (int32)1);
    (@unsafe.Pointer.To<int32>(pnText)).Value = n - nLoc - (int32)1;
    return SQLITE_OK;
}

internal static void _sqlite3Fts5Dequote(ref libc.TLS tls, uintptr z) {
    int8 quote = default;
    _ = quote;
    quote = ~@unsafe.Pointer.To<int8>(__ccgo_ptr(z));
    if ((int32)quote == (int32)'[' || (int32)quote == (int32)'\'' || (int32)quote == (int32)'"' || (int32)quote == (int32)'`') {
        _fts5Dequote(ref tls, z);
    }
}

internal static int32 /*r*/ _sqlite3Fts5IndexWrite(ж<libc.TLS> tls, uintptr p, int32 iCol, int32 iPos, uintptr pToken, int32 nToken) {
    int32 i = default;
    int32 nByte = default;
    int32 nChar = default;
    int32 rc = default;
    uintptr pConfig = default;
    _ = i;
    _ = nByte;
    _ = nChar;
    _ = pConfig;
    _ = rc;
    rc = SQLITE_OK;
    pConfig = (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig;
    rc = _sqlite3Fts5HashWrite(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpHash, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FiWriteRowid, iCol, iPos, (int8)'0', pToken, nToken);
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnPrefix && rc == SQLITE_OK)) {
            break;
        }
        nChar = ~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FaPrefix + (uintptr)i * 4));
        nByte = _sqlite3Fts5IndexCharlenToBytelen(ref (tls).DerefOrNull(), pToken, nToken, nChar);
        if (nByte != 0) {
            rc = _sqlite3Fts5HashWrite(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpHash, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FiWriteRowid, iCol, iPos, (int8)(libc.Int32FromUint8((uint8)'0') + i + libc.Int32FromInt32(1)), pToken, nByte);
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3Fts5IsBareword(ref libc.TLS tls, int8 t) {
    array<Tu8> aBareword = new(128);
    _ = aBareword;
    aBareword.Assign(new array<Tu8>(128){
        [26] = (uint8)1,
        [48] = (uint8)1,
        [49] = (uint8)1,
        [50] = (uint8)1,
        [51] = (uint8)1,
        [52] = (uint8)1,
        [53] = (uint8)1,
        [54] = (uint8)1,
        [55] = (uint8)1,
        [56] = (uint8)1,
        [57] = (uint8)1,
        [65] = (uint8)1,
        [66] = (uint8)1,
        [67] = (uint8)1,
        [68] = (uint8)1,
        [69] = (uint8)1,
        [70] = (uint8)1,
        [71] = (uint8)1,
        [72] = (uint8)1,
        [73] = (uint8)1,
        [74] = (uint8)1,
        [75] = (uint8)1,
        [76] = (uint8)1,
        [77] = (uint8)1,
        [78] = (uint8)1,
        [79] = (uint8)1,
        [80] = (uint8)1,
        [81] = (uint8)1,
        [82] = (uint8)1,
        [83] = (uint8)1,
        [84] = (uint8)1,
        [85] = (uint8)1,
        [86] = (uint8)1,
        [87] = (uint8)1,
        [88] = (uint8)1,
        [89] = (uint8)1,
        [90] = (uint8)1,
        [95] = (uint8)1,
        [97] = (uint8)1,
        [98] = (uint8)1,
        [99] = (uint8)1,
        [100] = (uint8)1,
        [101] = (uint8)1,
        [102] = (uint8)1,
        [103] = (uint8)1,
        [104] = (uint8)1,
        [105] = (uint8)1,
        [106] = (uint8)1,
        [107] = (uint8)1,
        [108] = (uint8)1,
        [109] = (uint8)1,
        [110] = (uint8)1,
        [111] = (uint8)1,
        [112] = (uint8)1,
        [113] = (uint8)1,
        [114] = (uint8)1,
        [115] = (uint8)1,
        [116] = (uint8)1,
        [117] = (uint8)1,
        [118] = (uint8)1,
        [119] = (uint8)1,
        [120] = (uint8)1,
        [121] = (uint8)1,
        [122] = (uint8)1
    });
    return libc.BoolInt32(((int32)t & (int32)0x80) != 0 || aBareword[(int32)t] != 0);
}

internal static int32 /*r*/ _sqlite3Fts5UnicodeCatParse(ref libc.TLS tls, uintptr zCat, uintptr aArray) {
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray))).Value = (uint8)1;
    switch ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zCat)))) {
    case var ᴛ when ᴛ == (int32)'C': {
        switch ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zCat + 1)))) {
        case var ᴛ2 when ᴛ2 == (int32)'c': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 1))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'f': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 2))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'n': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 3))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'s': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 4))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'o': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 31))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'*': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 1))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 2))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 3))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 4))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 31))).Value = (uint8)1;
            break;
        }
        default: {
            return (int32)1;
        }}

        break;
    }
    case var ᴛ when ᴛ == (int32)'L': {
        switch ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zCat + 1)))) {
        case var ᴛ2 when ᴛ2 == (int32)'l': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 5))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'m': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 6))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'o': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 7))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'t': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 8))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'u': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 9))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'C': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 30))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'*': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 5))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 6))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 7))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 8))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 9))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 30))).Value = (uint8)1;
            break;
        }
        default: {
            return (int32)1;
        }}

        break;
    }
    case var ᴛ when ᴛ == (int32)'M': {
        switch ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zCat + 1)))) {
        case var ᴛ2 when ᴛ2 == (int32)'c': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 10))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'e': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 11))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'n': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 12))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'*': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 10))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 11))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 12))).Value = (uint8)1;
            break;
        }
        default: {
            return (int32)1;
        }}

        break;
    }
    case var ᴛ when ᴛ == (int32)'N': {
        switch ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zCat + 1)))) {
        case var ᴛ2 when ᴛ2 == (int32)'d': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 13))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'l': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 14))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'o': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 15))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'*': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 13))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 14))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 15))).Value = (uint8)1;
            break;
        }
        default: {
            return (int32)1;
        }}

        break;
    }
    case var ᴛ when ᴛ == (int32)'P': {
        switch ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zCat + 1)))) {
        case var ᴛ2 when ᴛ2 == (int32)'c': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 16))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'d': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 17))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'e': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 18))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'f': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 19))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'i': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 20))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'o': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 21))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'s': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 22))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'*': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 16))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 17))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 18))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 19))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 20))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 21))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 22))).Value = (uint8)1;
            break;
        }
        default: {
            return (int32)1;
        }}

        break;
    }
    case var ᴛ when ᴛ == (int32)'S': {
        switch ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zCat + 1)))) {
        case var ᴛ2 when ᴛ2 == (int32)'c': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 23))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'k': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 24))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'m': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 25))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'o': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 26))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'*': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 23))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 24))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 25))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 26))).Value = (uint8)1;
            break;
        }
        default: {
            return (int32)1;
        }}

        break;
    }
    case var ᴛ when ᴛ == (int32)'Z': {
        switch ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zCat + 1)))) {
        case var ᴛ2 when ᴛ2 == (int32)'l': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 27))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'p': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 28))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'s': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 29))).Value = (uint8)1;
            break;
        }
        case var ᴛ2 when ᴛ2 == (int32)'*': {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 27))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 28))).Value = (uint8)1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + 29))).Value = (uint8)1;
            break;
        }
        default: {
            return (int32)1;
        }}

        break;
    }
    default: {
        return (int32)1;
    }}

    return 0;
}

internal static int32 /*r*/ _sqlite3IndexAffinityOk(ж<libc.TLS> tls, @unsafe.Pointer pExpr, int8 idx_affinity) {
    int8 aff = default;
    _ = aff;
    aff = _comparisonAffinity(tls, pExpr);
    if ((int32)aff < (int32)SQLITE_AFF_TEXT) {
        return (int32)1;
    }
    if ((int32)aff == (int32)SQLITE_AFF_TEXT) {
        return libc.BoolInt32((int32)idx_affinity == (int32)SQLITE_AFF_TEXT);
    }
    return libc.BoolInt32((int32)idx_affinity >= (int32)SQLITE_AFF_NUMERIC);
}

internal static int8 /*r*/ _sqlite3IndexColumnAffinity(ж<libc.TLS> tls, uintptr db, uintptr pIdx, int32 iCol) {
    if (!((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FzColAff != 0)) {
        if (_sqlite3IndexAffinityStr(tls, db, pIdx) == (uintptr)0) {
            return (int8)SQLITE_AFF_BLOB;
        }
    }
    return ~@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FzColAff + (uintptr)iCol));
}

internal static int32 /*r*/ _sqlite3OsFullPathname(ж<libc.TLS> tls, uintptr pVfs, uintptr zPath, int32 nPathOut, uintptr zPathOut) {
    (@unsafe.Pointer.To<int8>(__ccgo_ptr(zPathOut))).Value = 0;
    return GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)pVfs)).Value.FxFullPathname)(tls, pVfs, zPath, nPathOut, zPathOut);
}

[GoLazy] partial class lazyᴛ_sqlite3StdTypeAffinity { internal static array<int8> _sqlite3StdTypeAffinity = new array<int8>(6){
    [0] = (int8)SQLITE_AFF_NUMERIC,
    [1] = (int8)SQLITE_AFF_BLOB,
    [2] = (int8)SQLITE_AFF_INTEGER,
    [3] = (int8)SQLITE_AFF_INTEGER,
    [4] = (int8)SQLITE_AFF_REAL,
    [5] = (int8)SQLITE_AFF_TEXT
}; }

internal static readonly ж<array<int8>> Ꮡ_sqlite3StrBINARY = new StaticBox<array<int8>>(new int8[]{(int8)'B', (int8)'I', (int8)'N', (int8)'A', (int8)'R', (int8)'Y'}.array(7));
internal static ref array<int8> _sqlite3StrBINARY => ref ((StaticBox<array<int8>>)Ꮡ_sqlite3StrBINARY).ValueSlot;

internal static void _sqlite3VdbeMultiLoad(ж<libc.TLS> tls, uintptr p, int32 iDest, uintptr zTypes, uintptr va) {
    ref var ap = ref heap(new Tva_list(), out var Ꮡap);
    int8 c = default;
    int8 v2 = default;
    int32 i = default;
    int32 v3 = default;
    uintptr z = default;
    _ = ap;
    _ = c;
    _ = i;
    _ = z;
    _ = v2;
    _ = v3;
    ap = va;
    i = 0;
    for (;;) {
        v2 = ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zTypes + (uintptr)i));
        c = v2;
        if (!((int32)v2 != 0)) {
            break;
        }
        if ((int32)c == (int32)'s') {
            z = libc.VaUintptr(Ꮡap);
            if (z == (uintptr)0) {
                v3 = (int32)OP_Null;
            } else {
                v3 = (int32)OP_String8;
            }
            _sqlite3VdbeAddOp4(tls, p, v3, 0, iDest + i, 0, z, 0);
        } else {
            if ((int32)c == (int32)'i') {
                _sqlite3VdbeAddOp2(tls, p, (int32)OP_Integer, libc.VaInt32(Ꮡap), iDest + i);
            } else {
                goto skip_op_resultrow;
            }
        }
        goto _1;
_1:;
        i = i + 1;
    }
    _sqlite3VdbeAddOp2(tls, p, (int32)OP_ResultRow, iDest, i);
    goto skip_op_resultrow;
skip_op_resultrow:;
    _ = ap;
}

internal static array<int8> _statMutex = new array<int8>(10){
    [1] = (int8)1,
    [2] = (int8)1,
    [7] = (int8)1
};

internal static void _timeFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        int32 n = default;
        int32 s = default;
        TDateTime _ᴛ1 = default;
        array<int8> _ᴛ2 = new(16);
        _ = n;
        _ = s;
        if (_isDate(tls, context, argc, argv, bp) == 0) {
            _computeHMS(tls, bp);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), 0, 16) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.Fh / (int32)10 % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)1, 16) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.Fh % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)2, 16) = (int8)':';
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)3, 16) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.Fm / (int32)10 % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)4, 16) = (int8)((int32)'0' + (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.Fm % (int32)10);
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)5, 16) = (int8)':';
            if ((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(bp + 44)) & 0x4) >> 2) != 0) {
                s = (int32)((float64)(libc.Float64FromFloat64(1000D) * (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.Fs) + libc.Float64FromFloat64(0.5D));
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)6, 16) = (int8)((int32)'0' + s / (int32)10000 % (int32)10);
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)7, 16) = (int8)((int32)'0' + s / (int32)1000 % (int32)10);
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)8, 16) = (int8)'.';
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)9, 16) = (int8)((int32)'0' + s / (int32)100 % (int32)10);
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)10, 16) = (int8)((int32)'0' + s / (int32)10 % (int32)10);
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)11, 16) = (int8)((int32)'0' + s % (int32)10);
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)12, 16) = 0;
                n = (int32)12;
            } else {
                s = (int32)(@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.Fs;
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)6, 16) = (int8)((int32)'0' + s / (int32)10 % (int32)10);
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)7, 16) = (int8)((int32)'0' + s % (int32)10);
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 48), (int32)8, 16) = 0;
                n = (int32)8;
            }
            Xsqlite3_result_text(tls, context, bp + 48, n, (uintptr)(-libc.Int32FromInt32(1)));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _updateRangeAffinityStr(ж<libc.TLS> tls, uintptr pRight, int32 n, uintptr zAff) {
    int32 i = default;
    uintptr p = default;
    _ = i;
    _ = p;
    i = 0;
    for (;;) {
        if (!(i < n)) {
            break;
        }
        p = _sqlite3VectorFieldSubexpr(ref (tls).DerefOrNull(), pRight, i);
        if ((int32)_sqlite3CompareAffinity(tls, p, ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zAff + (uintptr)i))) == (int32)SQLITE_AFF_BLOB || _sqlite3ExprNeedsNoAffinityChange(ref (tls).DerefOrNull(), p, ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zAff + (uintptr)i))) != 0) {
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(zAff + (uintptr)i))).Value = (int8)SQLITE_AFF_BLOB;
        }
        goto _1;
_1:;
        i = i + 1;
    }
}

internal static uintptr /*r*/ _uriParameter(ж<libc.TLS> tls, uintptr zFilename, uintptr zParam) {
    int32 x = default;
    _ = x;
    zFilename = zFilename + (uintptr)(_sqlite3Strlen30(tls, zFilename) + (int32)1);
    while (zFilename != (uintptr)0 && ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zFilename)) != 0) {
        x = libc.Xstrcmp(tls, zFilename, zParam);
        zFilename = zFilename + (uintptr)(_sqlite3Strlen30(tls, zFilename) + (int32)1);
        if (x == 0) {
            return zFilename;
        }
        zFilename = zFilename + (uintptr)(_sqlite3Strlen30(tls, zFilename) + (int32)1);
    }
    return (uintptr)0;
}

internal static int32 /*r*/ _write32bits(ж<libc.TLS> tls, uintptr fd, Ti64 offset, Tu32 val) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        array<int8> _ᴛ1 = new(4);
        _sqlite3Put4byte(ref (tls).DerefOrNull(), __ccgo_ptr(bp), val);
        r = _sqlite3OsWrite(tls, fd, bp, (int32)4, offset);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static readonly ж<array<int8>> Ꮡ_zAff = new StaticBox<array<int8>>(new int8[]{(int8)'B', 0, (int8)'C', 0, (int8)'D', 0, (int8)'E', 0, (int8)'F'}.array(10));
internal static ref array<int8> _zAff => ref ((StaticBox<array<int8>>)Ꮡ_zAff).ValueSlot;

[GoLazy] partial class lazyᴛ_zDbstatSchema { internal static readonly ж<array<int8>> Ꮡ_zDbstatSchema = new StaticBox<array<int8>>(new int8[]{(int8)'C', (int8)'R', (int8)'E', (int8)'A', (int8)'T', (int8)'E', (int8)' ', (int8)'T', (int8)'A', (int8)'B', (int8)'L', (int8)'E', (int8)' ', (int8)'x', (int8)'(', (int8)' ', (int8)'n', (int8)'a', (int8)'m', (int8)'e', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)'T', (int8)'E', (int8)'X', (int8)'T', (int8)',', (int8)' ', (int8)'p', (int8)'a', (int8)'t', (int8)'h', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)'T', (int8)'E', (int8)'X', (int8)'T', (int8)',', (int8)' ', (int8)'p', (int8)'a', (int8)'g', (int8)'e', (int8)'n', (int8)'o', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)'I', (int8)'N', (int8)'T', (int8)'E', (int8)'G', (int8)'E', (int8)'R', (int8)',', (int8)' ', (int8)'p', (int8)'a', (int8)'g', (int8)'e', (int8)'t', (int8)'y', (int8)'p', (int8)'e', (int8)' ', (int8)' ', (int8)' ', (int8)'T', (int8)'E', (int8)'X', (int8)'T', (int8)',', (int8)' ', (int8)'n', (int8)'c', (int8)'e', (int8)'l', (int8)'l', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)'I', (int8)'N', (int8)'T', (int8)'E', (int8)'G', (int8)'E', (int8)'R', (int8)',', (int8)' ', (int8)'p', (int8)'a', (int8)'y', (int8)'l', (int8)'o', (int8)'a', (int8)'d', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)'I', (int8)'N', (int8)'T', (int8)'E', (int8)'G', (int8)'E', (int8)'R', (int8)',', (int8)' ', (int8)'u', (int8)'n', (int8)'u', (int8)'s', (int8)'e', (int8)'d', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)'I', (int8)'N', (int8)'T', (int8)'E', (int8)'G', (int8)'E', (int8)'R', (int8)',', (int8)' ', (int8)'m', (int8)'x', (int8)'_', (int8)'p', (int8)'a', (int8)'y', (int8)'l', (int8)'o', (int8)'a', (int8)'d', (int8)' ', (int8)'I', (int8)'N', (int8)'T', (int8)'E', (int8)'G', (int8)'E', (int8)'R', (int8)',', (int8)' ', (int8)'p', (int8)'g', (int8)'o', (int8)'f', (int8)'f', (int8)'s', (int8)'e', (int8)'t', (int8)' ', (int8)' ', (int8)' ', (int8)'I', (int8)'N', (int8)'T', (int8)'E', (int8)'G', (int8)'E', (int8)'R', (int8)',', (int8)' ', (int8)'p', (int8)'g', (int8)'s', (int8)'i', (int8)'z', (int8)'e', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)'I', (int8)'N', (int8)'T', (int8)'E', (int8)'G', (int8)'E', (int8)'R', (int8)',', (int8)' ', (int8)'s', (int8)'c', (int8)'h', (int8)'e', (int8)'m', (int8)'a', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)' ', (int8)'T', (int8)'E', (int8)'X', (int8)'T', (int8)' ', (int8)'H', (int8)'I', (int8)'D', (int8)'D', (int8)'E', (int8)'N', (int8)',', (int8)' ', (int8)'a', (int8)'g', (int8)'g', (int8)'r', (int8)'e', (int8)'g', (int8)'a', (int8)'t', (int8)'e', (int8)' ', (int8)' ', (int8)'B', (int8)'O', (int8)'O', (int8)'L', (int8)'E', (int8)'A', (int8)'N', (int8)' ', (int8)'H', (int8)'I', (int8)'D', (int8)'D', (int8)'E', (int8)'N', (int8)')'}.array(258)); }
internal static ref array<int8> _zDbstatSchema => ref ((StaticBox<array<int8>>)Ꮡ_zDbstatSchema).ValueSlot;

internal static readonly ж<array<int8>> Ꮡ_zFake = new StaticBox<array<int8>>(new int8[]{}.array(8));
internal static ref array<int8> _zFake => ref ((StaticBox<array<int8>>)Ꮡ_zFake).ValueSlot;

[GoLazy] partial class lazyᴛ_zKWText { internal static readonly ж<array<int8>> Ꮡ_zKWText = new StaticBox<array<int8>>(new array<int8>(666){
    [0] = (int8)'R',
    [1] = (int8)'E',
    [2] = (int8)'I',
    [3] = (int8)'N',
    [4] = (int8)'D',
    [5] = (int8)'E',
    [6] = (int8)'X',
    [7] = (int8)'E',
    [8] = (int8)'D',
    [9] = (int8)'E',
    [10] = (int8)'S',
    [11] = (int8)'C',
    [12] = (int8)'A',
    [13] = (int8)'P',
    [14] = (int8)'E',
    [15] = (int8)'A',
    [16] = (int8)'C',
    [17] = (int8)'H',
    [18] = (int8)'E',
    [19] = (int8)'C',
    [20] = (int8)'K',
    [21] = (int8)'E',
    [22] = (int8)'Y',
    [23] = (int8)'B',
    [24] = (int8)'E',
    [25] = (int8)'F',
    [26] = (int8)'O',
    [27] = (int8)'R',
    [28] = (int8)'E',
    [29] = (int8)'I',
    [30] = (int8)'G',
    [31] = (int8)'N',
    [32] = (int8)'O',
    [33] = (int8)'R',
    [34] = (int8)'E',
    [35] = (int8)'G',
    [36] = (int8)'E',
    [37] = (int8)'X',
    [38] = (int8)'P',
    [39] = (int8)'L',
    [40] = (int8)'A',
    [41] = (int8)'I',
    [42] = (int8)'N',
    [43] = (int8)'S',
    [44] = (int8)'T',
    [45] = (int8)'E',
    [46] = (int8)'A',
    [47] = (int8)'D',
    [48] = (int8)'D',
    [49] = (int8)'A',
    [50] = (int8)'T',
    [51] = (int8)'A',
    [52] = (int8)'B',
    [53] = (int8)'A',
    [54] = (int8)'S',
    [55] = (int8)'E',
    [56] = (int8)'L',
    [57] = (int8)'E',
    [58] = (int8)'C',
    [59] = (int8)'T',
    [60] = (int8)'A',
    [61] = (int8)'B',
    [62] = (int8)'L',
    [63] = (int8)'E',
    [64] = (int8)'F',
    [65] = (int8)'T',
    [66] = (int8)'H',
    [67] = (int8)'E',
    [68] = (int8)'N',
    [69] = (int8)'D',
    [70] = (int8)'E',
    [71] = (int8)'F',
    [72] = (int8)'E',
    [73] = (int8)'R',
    [74] = (int8)'R',
    [75] = (int8)'A',
    [76] = (int8)'B',
    [77] = (int8)'L',
    [78] = (int8)'E',
    [79] = (int8)'L',
    [80] = (int8)'S',
    [81] = (int8)'E',
    [82] = (int8)'X',
    [83] = (int8)'C',
    [84] = (int8)'L',
    [85] = (int8)'U',
    [86] = (int8)'D',
    [87] = (int8)'E',
    [88] = (int8)'L',
    [89] = (int8)'E',
    [90] = (int8)'T',
    [91] = (int8)'E',
    [92] = (int8)'M',
    [93] = (int8)'P',
    [94] = (int8)'O',
    [95] = (int8)'R',
    [96] = (int8)'A',
    [97] = (int8)'R',
    [98] = (int8)'Y',
    [99] = (int8)'I',
    [100] = (int8)'S',
    [101] = (int8)'N',
    [102] = (int8)'U',
    [103] = (int8)'L',
    [104] = (int8)'L',
    [105] = (int8)'S',
    [106] = (int8)'A',
    [107] = (int8)'V',
    [108] = (int8)'E',
    [109] = (int8)'P',
    [110] = (int8)'O',
    [111] = (int8)'I',
    [112] = (int8)'N',
    [113] = (int8)'T',
    [114] = (int8)'E',
    [115] = (int8)'R',
    [116] = (int8)'S',
    [117] = (int8)'E',
    [118] = (int8)'C',
    [119] = (int8)'T',
    [120] = (int8)'I',
    [121] = (int8)'E',
    [122] = (int8)'S',
    [123] = (int8)'N',
    [124] = (int8)'O',
    [125] = (int8)'T',
    [126] = (int8)'N',
    [127] = (int8)'U',
    [128] = (int8)'L',
    [129] = (int8)'L',
    [130] = (int8)'I',
    [131] = (int8)'K',
    [132] = (int8)'E',
    [133] = (int8)'X',
    [134] = (int8)'C',
    [135] = (int8)'E',
    [136] = (int8)'P',
    [137] = (int8)'T',
    [138] = (int8)'R',
    [139] = (int8)'A',
    [140] = (int8)'N',
    [141] = (int8)'S',
    [142] = (int8)'A',
    [143] = (int8)'C',
    [144] = (int8)'T',
    [145] = (int8)'I',
    [146] = (int8)'O',
    [147] = (int8)'N',
    [148] = (int8)'A',
    [149] = (int8)'T',
    [150] = (int8)'U',
    [151] = (int8)'R',
    [152] = (int8)'A',
    [153] = (int8)'L',
    [154] = (int8)'T',
    [155] = (int8)'E',
    [156] = (int8)'R',
    [157] = (int8)'A',
    [158] = (int8)'I',
    [159] = (int8)'S',
    [160] = (int8)'E',
    [161] = (int8)'X',
    [162] = (int8)'C',
    [163] = (int8)'L',
    [164] = (int8)'U',
    [165] = (int8)'S',
    [166] = (int8)'I',
    [167] = (int8)'V',
    [168] = (int8)'E',
    [169] = (int8)'X',
    [170] = (int8)'I',
    [171] = (int8)'S',
    [172] = (int8)'T',
    [173] = (int8)'S',
    [174] = (int8)'C',
    [175] = (int8)'O',
    [176] = (int8)'N',
    [177] = (int8)'S',
    [178] = (int8)'T',
    [179] = (int8)'R',
    [180] = (int8)'A',
    [181] = (int8)'I',
    [182] = (int8)'N',
    [183] = (int8)'T',
    [184] = (int8)'O',
    [185] = (int8)'F',
    [186] = (int8)'F',
    [187] = (int8)'S',
    [188] = (int8)'E',
    [189] = (int8)'T',
    [190] = (int8)'R',
    [191] = (int8)'I',
    [192] = (int8)'G',
    [193] = (int8)'G',
    [194] = (int8)'E',
    [195] = (int8)'R',
    [196] = (int8)'A',
    [197] = (int8)'N',
    [198] = (int8)'G',
    [199] = (int8)'E',
    [200] = (int8)'N',
    [201] = (int8)'E',
    [202] = (int8)'R',
    [203] = (int8)'A',
    [204] = (int8)'T',
    [205] = (int8)'E',
    [206] = (int8)'D',
    [207] = (int8)'E',
    [208] = (int8)'T',
    [209] = (int8)'A',
    [210] = (int8)'C',
    [211] = (int8)'H',
    [212] = (int8)'A',
    [213] = (int8)'V',
    [214] = (int8)'I',
    [215] = (int8)'N',
    [216] = (int8)'G',
    [217] = (int8)'L',
    [218] = (int8)'O',
    [219] = (int8)'B',
    [220] = (int8)'E',
    [221] = (int8)'G',
    [222] = (int8)'I',
    [223] = (int8)'N',
    [224] = (int8)'N',
    [225] = (int8)'E',
    [226] = (int8)'R',
    [227] = (int8)'E',
    [228] = (int8)'F',
    [229] = (int8)'E',
    [230] = (int8)'R',
    [231] = (int8)'E',
    [232] = (int8)'N',
    [233] = (int8)'C',
    [234] = (int8)'E',
    [235] = (int8)'S',
    [236] = (int8)'U',
    [237] = (int8)'N',
    [238] = (int8)'I',
    [239] = (int8)'Q',
    [240] = (int8)'U',
    [241] = (int8)'E',
    [242] = (int8)'R',
    [243] = (int8)'Y',
    [244] = (int8)'W',
    [245] = (int8)'I',
    [246] = (int8)'T',
    [247] = (int8)'H',
    [248] = (int8)'O',
    [249] = (int8)'U',
    [250] = (int8)'T',
    [251] = (int8)'E',
    [252] = (int8)'R',
    [253] = (int8)'E',
    [254] = (int8)'L',
    [255] = (int8)'E',
    [256] = (int8)'A',
    [257] = (int8)'S',
    [258] = (int8)'E',
    [259] = (int8)'A',
    [260] = (int8)'T',
    [261] = (int8)'T',
    [262] = (int8)'A',
    [263] = (int8)'C',
    [264] = (int8)'H',
    [265] = (int8)'B',
    [266] = (int8)'E',
    [267] = (int8)'T',
    [268] = (int8)'W',
    [269] = (int8)'E',
    [270] = (int8)'E',
    [271] = (int8)'N',
    [272] = (int8)'O',
    [273] = (int8)'T',
    [274] = (int8)'H',
    [275] = (int8)'I',
    [276] = (int8)'N',
    [277] = (int8)'G',
    [278] = (int8)'R',
    [279] = (int8)'O',
    [280] = (int8)'U',
    [281] = (int8)'P',
    [282] = (int8)'S',
    [283] = (int8)'C',
    [284] = (int8)'A',
    [285] = (int8)'S',
    [286] = (int8)'C',
    [287] = (int8)'A',
    [288] = (int8)'D',
    [289] = (int8)'E',
    [290] = (int8)'F',
    [291] = (int8)'A',
    [292] = (int8)'U',
    [293] = (int8)'L',
    [294] = (int8)'T',
    [295] = (int8)'C',
    [296] = (int8)'A',
    [297] = (int8)'S',
    [298] = (int8)'E',
    [299] = (int8)'C',
    [300] = (int8)'O',
    [301] = (int8)'L',
    [302] = (int8)'L',
    [303] = (int8)'A',
    [304] = (int8)'T',
    [305] = (int8)'E',
    [306] = (int8)'C',
    [307] = (int8)'R',
    [308] = (int8)'E',
    [309] = (int8)'A',
    [310] = (int8)'T',
    [311] = (int8)'E',
    [312] = (int8)'C',
    [313] = (int8)'U',
    [314] = (int8)'R',
    [315] = (int8)'R',
    [316] = (int8)'E',
    [317] = (int8)'N',
    [318] = (int8)'T',
    [319] = (int8)'_',
    [320] = (int8)'D',
    [321] = (int8)'A',
    [322] = (int8)'T',
    [323] = (int8)'E',
    [324] = (int8)'I',
    [325] = (int8)'M',
    [326] = (int8)'M',
    [327] = (int8)'E',
    [328] = (int8)'D',
    [329] = (int8)'I',
    [330] = (int8)'A',
    [331] = (int8)'T',
    [332] = (int8)'E',
    [333] = (int8)'J',
    [334] = (int8)'O',
    [335] = (int8)'I',
    [336] = (int8)'N',
    [337] = (int8)'S',
    [338] = (int8)'E',
    [339] = (int8)'R',
    [340] = (int8)'T',
    [341] = (int8)'M',
    [342] = (int8)'A',
    [343] = (int8)'T',
    [344] = (int8)'C',
    [345] = (int8)'H',
    [346] = (int8)'P',
    [347] = (int8)'L',
    [348] = (int8)'A',
    [349] = (int8)'N',
    [350] = (int8)'A',
    [351] = (int8)'L',
    [352] = (int8)'Y',
    [353] = (int8)'Z',
    [354] = (int8)'E',
    [355] = (int8)'P',
    [356] = (int8)'R',
    [357] = (int8)'A',
    [358] = (int8)'G',
    [359] = (int8)'M',
    [360] = (int8)'A',
    [361] = (int8)'T',
    [362] = (int8)'E',
    [363] = (int8)'R',
    [364] = (int8)'I',
    [365] = (int8)'A',
    [366] = (int8)'L',
    [367] = (int8)'I',
    [368] = (int8)'Z',
    [369] = (int8)'E',
    [370] = (int8)'D',
    [371] = (int8)'E',
    [372] = (int8)'F',
    [373] = (int8)'E',
    [374] = (int8)'R',
    [375] = (int8)'R',
    [376] = (int8)'E',
    [377] = (int8)'D',
    [378] = (int8)'I',
    [379] = (int8)'S',
    [380] = (int8)'T',
    [381] = (int8)'I',
    [382] = (int8)'N',
    [383] = (int8)'C',
    [384] = (int8)'T',
    [385] = (int8)'U',
    [386] = (int8)'P',
    [387] = (int8)'D',
    [388] = (int8)'A',
    [389] = (int8)'T',
    [390] = (int8)'E',
    [391] = (int8)'V',
    [392] = (int8)'A',
    [393] = (int8)'L',
    [394] = (int8)'U',
    [395] = (int8)'E',
    [396] = (int8)'S',
    [397] = (int8)'V',
    [398] = (int8)'I',
    [399] = (int8)'R',
    [400] = (int8)'T',
    [401] = (int8)'U',
    [402] = (int8)'A',
    [403] = (int8)'L',
    [404] = (int8)'W',
    [405] = (int8)'A',
    [406] = (int8)'Y',
    [407] = (int8)'S',
    [408] = (int8)'W',
    [409] = (int8)'H',
    [410] = (int8)'E',
    [411] = (int8)'N',
    [412] = (int8)'W',
    [413] = (int8)'H',
    [414] = (int8)'E',
    [415] = (int8)'R',
    [416] = (int8)'E',
    [417] = (int8)'C',
    [418] = (int8)'U',
    [419] = (int8)'R',
    [420] = (int8)'S',
    [421] = (int8)'I',
    [422] = (int8)'V',
    [423] = (int8)'E',
    [424] = (int8)'A',
    [425] = (int8)'B',
    [426] = (int8)'O',
    [427] = (int8)'R',
    [428] = (int8)'T',
    [429] = (int8)'A',
    [430] = (int8)'F',
    [431] = (int8)'T',
    [432] = (int8)'E',
    [433] = (int8)'R',
    [434] = (int8)'E',
    [435] = (int8)'N',
    [436] = (int8)'A',
    [437] = (int8)'M',
    [438] = (int8)'E',
    [439] = (int8)'A',
    [440] = (int8)'N',
    [441] = (int8)'D',
    [442] = (int8)'R',
    [443] = (int8)'O',
    [444] = (int8)'P',
    [445] = (int8)'A',
    [446] = (int8)'R',
    [447] = (int8)'T',
    [448] = (int8)'I',
    [449] = (int8)'T',
    [450] = (int8)'I',
    [451] = (int8)'O',
    [452] = (int8)'N',
    [453] = (int8)'A',
    [454] = (int8)'U',
    [455] = (int8)'T',
    [456] = (int8)'O',
    [457] = (int8)'I',
    [458] = (int8)'N',
    [459] = (int8)'C',
    [460] = (int8)'R',
    [461] = (int8)'E',
    [462] = (int8)'M',
    [463] = (int8)'E',
    [464] = (int8)'N',
    [465] = (int8)'T',
    [466] = (int8)'C',
    [467] = (int8)'A',
    [468] = (int8)'S',
    [469] = (int8)'T',
    [470] = (int8)'C',
    [471] = (int8)'O',
    [472] = (int8)'L',
    [473] = (int8)'U',
    [474] = (int8)'M',
    [475] = (int8)'N',
    [476] = (int8)'C',
    [477] = (int8)'O',
    [478] = (int8)'M',
    [479] = (int8)'M',
    [480] = (int8)'I',
    [481] = (int8)'T',
    [482] = (int8)'C',
    [483] = (int8)'O',
    [484] = (int8)'N',
    [485] = (int8)'F',
    [486] = (int8)'L',
    [487] = (int8)'I',
    [488] = (int8)'C',
    [489] = (int8)'T',
    [490] = (int8)'C',
    [491] = (int8)'R',
    [492] = (int8)'O',
    [493] = (int8)'S',
    [494] = (int8)'S',
    [495] = (int8)'C',
    [496] = (int8)'U',
    [497] = (int8)'R',
    [498] = (int8)'R',
    [499] = (int8)'E',
    [500] = (int8)'N',
    [501] = (int8)'T',
    [502] = (int8)'_',
    [503] = (int8)'T',
    [504] = (int8)'I',
    [505] = (int8)'M',
    [506] = (int8)'E',
    [507] = (int8)'S',
    [508] = (int8)'T',
    [509] = (int8)'A',
    [510] = (int8)'M',
    [511] = (int8)'P',
    [512] = (int8)'R',
    [513] = (int8)'E',
    [514] = (int8)'C',
    [515] = (int8)'E',
    [516] = (int8)'D',
    [517] = (int8)'I',
    [518] = (int8)'N',
    [519] = (int8)'G',
    [520] = (int8)'F',
    [521] = (int8)'A',
    [522] = (int8)'I',
    [523] = (int8)'L',
    [524] = (int8)'A',
    [525] = (int8)'S',
    [526] = (int8)'T',
    [527] = (int8)'F',
    [528] = (int8)'I',
    [529] = (int8)'L',
    [530] = (int8)'T',
    [531] = (int8)'E',
    [532] = (int8)'R',
    [533] = (int8)'E',
    [534] = (int8)'P',
    [535] = (int8)'L',
    [536] = (int8)'A',
    [537] = (int8)'C',
    [538] = (int8)'E',
    [539] = (int8)'F',
    [540] = (int8)'I',
    [541] = (int8)'R',
    [542] = (int8)'S',
    [543] = (int8)'T',
    [544] = (int8)'F',
    [545] = (int8)'O',
    [546] = (int8)'L',
    [547] = (int8)'L',
    [548] = (int8)'O',
    [549] = (int8)'W',
    [550] = (int8)'I',
    [551] = (int8)'N',
    [552] = (int8)'G',
    [553] = (int8)'F',
    [554] = (int8)'R',
    [555] = (int8)'O',
    [556] = (int8)'M',
    [557] = (int8)'F',
    [558] = (int8)'U',
    [559] = (int8)'L',
    [560] = (int8)'L',
    [561] = (int8)'I',
    [562] = (int8)'M',
    [563] = (int8)'I',
    [564] = (int8)'T',
    [565] = (int8)'I',
    [566] = (int8)'F',
    [567] = (int8)'O',
    [568] = (int8)'R',
    [569] = (int8)'D',
    [570] = (int8)'E',
    [571] = (int8)'R',
    [572] = (int8)'E',
    [573] = (int8)'S',
    [574] = (int8)'T',
    [575] = (int8)'R',
    [576] = (int8)'I',
    [577] = (int8)'C',
    [578] = (int8)'T',
    [579] = (int8)'O',
    [580] = (int8)'T',
    [581] = (int8)'H',
    [582] = (int8)'E',
    [583] = (int8)'R',
    [584] = (int8)'S',
    [585] = (int8)'O',
    [586] = (int8)'V',
    [587] = (int8)'E',
    [588] = (int8)'R',
    [589] = (int8)'E',
    [590] = (int8)'T',
    [591] = (int8)'U',
    [592] = (int8)'R',
    [593] = (int8)'N',
    [594] = (int8)'I',
    [595] = (int8)'N',
    [596] = (int8)'G',
    [597] = (int8)'R',
    [598] = (int8)'I',
    [599] = (int8)'G',
    [600] = (int8)'H',
    [601] = (int8)'T',
    [602] = (int8)'R',
    [603] = (int8)'O',
    [604] = (int8)'L',
    [605] = (int8)'L',
    [606] = (int8)'B',
    [607] = (int8)'A',
    [608] = (int8)'C',
    [609] = (int8)'K',
    [610] = (int8)'R',
    [611] = (int8)'O',
    [612] = (int8)'W',
    [613] = (int8)'S',
    [614] = (int8)'U',
    [615] = (int8)'N',
    [616] = (int8)'B',
    [617] = (int8)'O',
    [618] = (int8)'U',
    [619] = (int8)'N',
    [620] = (int8)'D',
    [621] = (int8)'E',
    [622] = (int8)'D',
    [623] = (int8)'U',
    [624] = (int8)'N',
    [625] = (int8)'I',
    [626] = (int8)'O',
    [627] = (int8)'N',
    [628] = (int8)'U',
    [629] = (int8)'S',
    [630] = (int8)'I',
    [631] = (int8)'N',
    [632] = (int8)'G',
    [633] = (int8)'V',
    [634] = (int8)'A',
    [635] = (int8)'C',
    [636] = (int8)'U',
    [637] = (int8)'U',
    [638] = (int8)'M',
    [639] = (int8)'V',
    [640] = (int8)'I',
    [641] = (int8)'E',
    [642] = (int8)'W',
    [643] = (int8)'I',
    [644] = (int8)'N',
    [645] = (int8)'D',
    [646] = (int8)'O',
    [647] = (int8)'W',
    [648] = (int8)'B',
    [649] = (int8)'Y',
    [650] = (int8)'I',
    [651] = (int8)'N',
    [652] = (int8)'I',
    [653] = (int8)'T',
    [654] = (int8)'I',
    [655] = (int8)'A',
    [656] = (int8)'L',
    [657] = (int8)'L',
    [658] = (int8)'Y',
    [659] = (int8)'P',
    [660] = (int8)'R',
    [661] = (int8)'I',
    [662] = (int8)'M',
    [663] = (int8)'A',
    [664] = (int8)'R',
    [665] = (int8)'Y'
}); }
internal static ref array<int8> _zKWText => ref ((StaticBox<array<int8>>)Ꮡ_zKWText).ValueSlot;

[GoLazy] partial class lazyᴛ_zKeyText { internal static readonly ж<array<int8>> Ꮡ_zKeyText = new StaticBox<array<int8>>(new int8[]{(int8)'n', (int8)'a', (int8)'t', (int8)'u', (int8)'r', (int8)'a', (int8)'l', (int8)'e', (int8)'f', (int8)'t', (int8)'o', (int8)'u', (int8)'t', (int8)'e', (int8)'r', (int8)'i', (int8)'g', (int8)'h', (int8)'t', (int8)'f', (int8)'u', (int8)'l', (int8)'l', (int8)'i', (int8)'n', (int8)'n', (int8)'e', (int8)'r', (int8)'c', (int8)'r', (int8)'o', (int8)'s', (int8)'s'}.array(34)); }
internal static ref array<int8> _zKeyText => ref ((StaticBox<array<int8>>)Ꮡ_zKeyText).ValueSlot;

internal static readonly ж<array<int8>> Ꮡ_zMagicHeader = new StaticBox<array<int8>>(new int8[]{(int8)'S', (int8)'Q', (int8)'L', (int8)'i', (int8)'t', (int8)'e', (int8)' ', (int8)'f', (int8)'o', (int8)'r', (int8)'m', (int8)'a', (int8)'t', (int8)' ', (int8)'3'}.array(16));
internal static ref array<int8> _zMagicHeader => ref ((StaticBox<array<int8>>)Ꮡ_zMagicHeader).ValueSlot;

internal static array<int8> _zOrd = new int8[]{(int8)'t', (int8)'h', (int8)'s', (int8)'t', (int8)'n', (int8)'d', (int8)'r', (int8)'d'}.array(9);

[GoLazy] partial class lazyᴛ_zText { internal static readonly ж<array<int8>> Ꮡ_zText = new StaticBox<array<int8>>(new int8[]{(int8)'o', (int8)'n', (int8)'o', (int8)'f', (int8)'f', (int8)'a', (int8)'l', (int8)'s', (int8)'e', (int8)'y', (int8)'e', (int8)'s', (int8)'t', (int8)'r', (int8)'u', (int8)'e', (int8)'x', (int8)'t', (int8)'r', (int8)'a', (int8)'f', (int8)'u', (int8)'l', (int8)'l'}.array(25)); }
internal static ref array<int8> _zText => ref ((StaticBox<array<int8>>)Ꮡ_zText).ValueSlot;

internal static readonly ж<array<int8>> Ꮡ_zeroHdr = new StaticBox<array<int8>>(new int8[]{}.array(28));
internal static ref array<int8> _zeroHdr => ref ((StaticBox<array<int8>>)Ꮡ_zeroHdr).ValueSlot;

} // end sqlite3_package
