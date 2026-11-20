namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static int32 /*r*/ Xsqlite3changeset_invert(ж<libc.TLS> tls, int32 nChangeset, uintptr pChangeset, uintptr pnInverted, uintptr ppInverted) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        TSessionInput _ᴛ1 = default;
        libc.Xmemset(tls, bp, 0, (uint64)72);
        (@unsafe.Pointer.To<TSessionInput>(__ccgo_ptr(bp))).Value.FnData = nChangeset;
        (@unsafe.Pointer.To<TSessionInput>(__ccgo_ptr(bp))).Value.FaData = pChangeset;
        r = _sessionChangesetInvert(tls, bp, (uintptr)0, (uintptr)0, pnInverted, ppInverted);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 /*r*/ Xsqlite3changeset_invert_strm(ж<libc.TLS> tls, uintptr __ccgo_fp_xInput, uintptr pIn, uintptr __ccgo_fp_xOutput, uintptr pOut) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        int32 rc = default;
        TSessionInput _ᴛ1 = default;
        _ = rc;
        libc.Xmemset(tls, bp, 0, (uint64)72);
        (@unsafe.Pointer.To<TSessionInput>(__ccgo_ptr(bp))).Value.FxInput = __ccgo_fp_xInput;
        (@unsafe.Pointer.To<TSessionInput>(__ccgo_ptr(bp))).Value.FpIn = pIn;
        rc = _sessionChangesetInvert(tls, bp, __ccgo_fp_xOutput, pOut, (uintptr)0, (uintptr)0);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TSessionInput>(__ccgo_ptr(bp))).Value.Fbuf.FaBuf);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 /*r*/ Xsqlite3rebaser_create(ж<libc.TLS> tls, uintptr ppNew) {
    uintptr pNew = default;
    int32 rc = default;
    _ = pNew;
    _ = rc;
    rc = SQLITE_OK;
    pNew = Xsqlite3_malloc(tls, (int32)96);
    if (pNew == (uintptr)0) {
        rc = (int32)SQLITE_NOMEM;
    } else {
        libc.Xmemset(tls, pNew, 0, (uint64)96);
    }
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppNew))).Value = pNew;
    return rc;
}

internal static void _aggregateConvertIndexedExprRefToColumn(ж<libc.TLS> tls, uintptr pAggInfo) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        int32 i = default;
        TWalker _ᴛ1 = new();
        _ = i;
        libc.Xmemset(tls, bp, 0, (uint64)48);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_aggregateIdxEprRefToColCallback);
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnFunc)) {
                break;
            }
            _sqlite3WalkExpr(tls, bp, (@unsafe.Pointer.To<TAggInfo_func>(__ccgo_ptr((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaFunc + (uintptr)i * 32))).Value.FpFExpr);
            goto _1;
_1:;
            i = i + 1;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static uintptr /*r*/ _appendText(ж<libc.TLS> tls, uintptr p, uintptr z) {
    Tsize_t n = default;
    _ = n;
    n = libc.Xstrlen(tls, z);
    libc.Xmemcpy(tls, p, z, n + (uint64)1);
    return p + (uintptr)n + (uintptr)1;
}

internal static void _chacha_block(ж<libc.TLS> tls, uintptr @out, uintptr @in) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        int32 i = default;
        array<Tu32> _ᴛ1 = new(16);
        _ = i;
        libc.Xmemcpy(tls, bp, @in, (uint64)64);
        i = 0;
        for (;;) {
            if (!(i < (int32)10)) {
                break;
            }
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 12 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), 0, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16) << (int)(libc.Int32FromInt32(16)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(16)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 8 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 4 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)8, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16) << (int)(libc.Int32FromInt32(12)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(12)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 12 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), 0, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16) << (int)(libc.Int32FromInt32(8)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(8)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 8 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 4 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)8, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16) << (int)(libc.Int32FromInt32(7)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(7)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 1 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 13 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)1, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16) << (int)(libc.Int32FromInt32(16)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(16)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 9 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 5 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)9, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16) << (int)(libc.Int32FromInt32(12)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(12)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 1 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 13 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)1, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16) << (int)(libc.Int32FromInt32(8)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(8)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 9 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 5 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)9, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16) << (int)(libc.Int32FromInt32(7)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(7)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 2 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 14 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)2, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16) << (int)(libc.Int32FromInt32(16)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(16)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 10 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 6 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)10, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16) << (int)(libc.Int32FromInt32(12)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(12)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 2 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 14 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)2, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16) << (int)(libc.Int32FromInt32(8)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(8)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 10 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 6 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)10, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16) << (int)(libc.Int32FromInt32(7)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(7)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 3 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 15 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)3, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16) << (int)(libc.Int32FromInt32(16)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(16)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 11 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 7 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)11, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16) << (int)(libc.Int32FromInt32(12)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(12)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 3 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 15 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)3, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16) << (int)(libc.Int32FromInt32(8)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(8)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 11 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 7 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)11, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16) << (int)(libc.Int32FromInt32(7)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(7)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 15 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), 0, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16) << (int)(libc.Int32FromInt32(16)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(16)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 10 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 5 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)10, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16) << (int)(libc.Int32FromInt32(12)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(12)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 15 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), 0, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16) << (int)(libc.Int32FromInt32(8)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(8)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 10 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)15, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 5 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)10, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16) << (int)(libc.Int32FromInt32(7)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)5, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(7)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 1 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 12 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)1, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16) << (int)(libc.Int32FromInt32(16)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(16)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 11 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 6 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)11, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16) << (int)(libc.Int32FromInt32(12)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(12)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 1 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 12 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)1, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16) << (int)(libc.Int32FromInt32(8)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(8)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 11 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)12, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 6 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)11, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16) << (int)(libc.Int32FromInt32(7)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)6, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(7)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 2 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 13 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)2, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16) << (int)(libc.Int32FromInt32(16)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(16)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 8 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 7 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)8, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16) << (int)(libc.Int32FromInt32(12)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(12)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 2 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 13 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)2, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16) << (int)(libc.Int32FromInt32(8)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(8)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 8 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)13, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 7 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)8, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16) << (int)(libc.Int32FromInt32(7)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)7, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(7)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 3 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 14 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)3, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16) << (int)(libc.Int32FromInt32(16)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(16)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 9 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 4 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)9, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16) << (int)(libc.Int32FromInt32(12)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(12)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 3 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 14 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)3, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16) << (int)(libc.Int32FromInt32(8)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(8)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 9 * 4))).Value += @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)14, 16);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 4 * 4))).Value ^= @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)9, 16);
            @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16) = (Tu32)(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16) << (int)(libc.Int32FromInt32(7)) | @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), (int32)4, 16) >> (int)(libc.Int32FromInt32(32) - libc.Int32FromInt32(7)));
            goto _1;
_1:;
            i = i + 1;
        }
        i = 0;
        for (;;) {
            if (!(i < (int32)16)) {
                break;
            }
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(@out + (uintptr)i * 4))).Value = @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), i, 16) + ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(@in + (uintptr)i * 4));
            goto _2;
_2:;
            i = i + 1;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _dbpageOpen(ж<libc.TLS> tls, uintptr pVTab, uintptr ppCursor) {
    uintptr pCsr = default;
    _ = pCsr;
    pCsr = Xsqlite3_malloc64(tls, (uint64)40);
    if (pCsr == (uintptr)0) {
        return (int32)SQLITE_NOMEM;
    } else {
        libc.Xmemset(tls, pCsr, 0, (uint64)40);
        (@unsafe.Pointer.To<TDbpageCursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab = pVTab;
        (@unsafe.Pointer.To<TDbpageCursor>((@unsafe.Pointer)pCsr)).Value.Fpgno = (uint32)0;
    }
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppCursor))).Value = pCsr;
    return SQLITE_OK;
}

internal static void _disableLookaside(ж<libc.TLS> tls, uintptr pParse) {
    uintptr db = default;
    _ = db;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FdisableLookaside = (Tu8)((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FdisableLookaside + 1);
    libc.Xmemset(tls, pParse + 240, 0, (uint64)32);
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FbDisable = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FbDisable + 1;
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.Fsz = (uint16)0;
}

internal static int32 /*r*/ _fts5AllocateSegid(ж<libc.TLS> tls, uintptr p, uintptr pStruct) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(256);
        defer(tls.Free, (nint)256, ref ᒐ);
        int32 i = default;
        int32 iId = default;
        int32 iLvl = default;
        int32 iSeg = default;
        int32 iSegid = default;
        Tu32 mask = default;
        array<Tu32> _ᴛ1 = new(63);
        _ = i;
        _ = iId;
        _ = iLvl;
        _ = iSeg;
        _ = iSegid;
        _ = mask;
        iSegid = 0;
        if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
            if ((@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnSegment >= (int32)FTS5_MAX_SEGMENT) {
                (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_FULL;
            } else {
                libc.Xmemset(tls, bp, 0, (uint64)252);
                iLvl = 0;
                for (;;) {
                    if (!(iLvl < (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnLevel)) {
                        break;
                    }
                    iSeg = 0;
                    for (;;) {
                        if (!(iSeg < (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pStruct + 32 + (uintptr)iLvl * 16))).Value.FnSeg)) {
                            break;
                        }
                        iId = (@unsafe.Pointer.To<TFts5StructureSegment>(__ccgo_ptr((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pStruct + 32 + (uintptr)iLvl * 16))).Value.FaSeg + (uintptr)iSeg * 56))).Value.FiSegid;
                        if (iId <= (int32)FTS5_MAX_SEGMENT && iId > 0) {
                            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + (uintptr)((iId - (int32)1) / (int32)32) * 4))).Value |= libc.Uint32FromInt32(1).Lsh((int64)((iId - (int32)1) % (int32)32));
                        }
                        goto _2;
_2:;
                        iSeg = iSeg + 1;
                    }
                    goto _1;
_1:;
                    iLvl = iLvl + 1;
                }
                i = 0;
                for (;;) {
                    if (!(@unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), i, 63) == (uint32)0xFFFFFFFFU)) {
                        break;
                    }
                    goto _3;
_3:;
                    i = i + 1;
                }
                mask = @unsafe.Pointer.ElementRef<Tu32>(__ccgo_ptr(bp), i, 63);
                iSegid = 0;
                for (;;) {
                    if (!((Tu32)(mask & (libc.Uint32FromInt32(1).Lsh((int64)(iSegid)))) != 0)) {
                        break;
                    }
                    goto _4;
_4:;
                    iSegid = iSegid + 1;
                }
                iSegid = iSegid + ((int32)1 + i * (int32)32);
            }
        }
        r = iSegid;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _fts5CInstIterInit(ж<libc.TLS> tls, uintptr pApi, uintptr pFts, int32 iCol, uintptr pIter) {
    int32 rc = default;
    _ = rc;
    libc.Xmemset(tls, pIter, 0, (uint64)40);
    (@unsafe.Pointer.To<TCInstIter>((@unsafe.Pointer)pIter)).Value.FpApi = pApi;
    (@unsafe.Pointer.To<TCInstIter>((@unsafe.Pointer)pIter)).Value.FpFts = pFts;
    (@unsafe.Pointer.To<TCInstIter>((@unsafe.Pointer)pIter)).Value.FiCol = iCol;
    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32>>((@unsafe.Pointer.To<TFts5ExtensionApi>((@unsafe.Pointer)pApi)).Value.FxInstCount)(tls, pFts, pIter + 24);
    if (rc == SQLITE_OK) {
        rc = _fts5CInstIterNext(tls, (@unsafe.Pointer)pIter);
    }
    return rc;
}

internal static void _fts5DlidxIterLast(ж<libc.TLS> tls, uintptr p, uintptr pIter) {
    int32 i = default;
    uintptr pChild = default;
    uintptr pLvl = default;
    _ = i;
    _ = pChild;
    _ = pLvl;
    i = (@unsafe.Pointer.To<TFts5DlidxIter>((@unsafe.Pointer)pIter)).Value.FnLvl - (int32)1;
    for (;;) {
        if (!((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && i >= 0)) {
            break;
        }
        pLvl = pIter + 8 + (uintptr)i * 32;
        while (_fts5DlidxLvlNext(ref (tls).DerefOrNull(), pLvl) == 0) {
        }
        (@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)pLvl)).Value.FbEof = 0;
        if (i > 0) {
            pChild = pLvl + (uintptr)(-libc.Int32FromInt32(1)) * 32;
            _fts5DataRelease(tls, (@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)pChild)).Value.FpData);
            libc.Xmemset(tls, pChild, 0, (uint64)32);
            (@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)pChild)).Value.FpData = _fts5DataRead(tls, p, ((int64)(@unsafe.Pointer.To<TFts5DlidxIter>((@unsafe.Pointer)pIter)).Value.FiSegid).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B) + libc.Int32FromInt32(FTS5_DATA_DLI_B))) + ((int64)libc.Int32FromInt32(1)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B))) + ((int64)(i - libc.Int32FromInt32(1))).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B))) + (int64)(@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)pLvl)).Value.FiLeafPgno);
        }
        goto _1;
_1:;
        i = i - 1;
    }
}

internal static int32 /*r*/ _fts5DlidxIterNextR(ж<libc.TLS> tls, uintptr p, uintptr pIter, int32 iLvl) {
    uintptr pLvl = default;
    _ = pLvl;
    pLvl = pIter + 8 + (uintptr)iLvl * 32;
    if (_fts5DlidxLvlNext(ref (tls).DerefOrNull(), pLvl) != 0) {
        if (iLvl + (int32)1 < (@unsafe.Pointer.To<TFts5DlidxIter>((@unsafe.Pointer)pIter)).Value.FnLvl) {
            _fts5DlidxIterNextR(tls, p, pIter, iLvl + (int32)1);
            if ((@unsafe.Pointer.To<TFts5DlidxLvl>(__ccgo_ptr(pLvl + 1 * 32))).Value.FbEof == 0) {
                _fts5DataRelease(tls, (@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)pLvl)).Value.FpData);
                libc.Xmemset(tls, pLvl, 0, (uint64)32);
                (@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)pLvl)).Value.FpData = _fts5DataRead(tls, p, ((int64)(@unsafe.Pointer.To<TFts5DlidxIter>((@unsafe.Pointer)pIter)).Value.FiSegid).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B) + libc.Int32FromInt32(FTS5_DATA_DLI_B))) + ((int64)libc.Int32FromInt32(1)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B))) + ((int64)iLvl).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B))) + (int64)(@unsafe.Pointer.To<TFts5DlidxLvl>(__ccgo_ptr(pLvl + 1 * 32))).Value.FiLeafPgno);
                if ((@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)pLvl)).Value.FpData != 0) {
                    _fts5DlidxLvlNext(ref (tls).DerefOrNull(), pLvl);
                }
            }
        }
    }
    return (@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)(pIter + 8))).Value.FbEof;
}

internal static int32 /*r*/ _fts5DlidxIterPrevR(ж<libc.TLS> tls, uintptr p, uintptr pIter, int32 iLvl) {
    uintptr pLvl = default;
    _ = pLvl;
    pLvl = pIter + 8 + (uintptr)iLvl * 32;
    if (_fts5DlidxLvlPrev(ref (tls).DerefOrNull(), pLvl) != 0) {
        if (iLvl + (int32)1 < (@unsafe.Pointer.To<TFts5DlidxIter>((@unsafe.Pointer)pIter)).Value.FnLvl) {
            _fts5DlidxIterPrevR(tls, p, pIter, iLvl + (int32)1);
            if ((@unsafe.Pointer.To<TFts5DlidxLvl>(__ccgo_ptr(pLvl + 1 * 32))).Value.FbEof == 0) {
                _fts5DataRelease(tls, (@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)pLvl)).Value.FpData);
                libc.Xmemset(tls, pLvl, 0, (uint64)32);
                (@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)pLvl)).Value.FpData = _fts5DataRead(tls, p, ((int64)(@unsafe.Pointer.To<TFts5DlidxIter>((@unsafe.Pointer)pIter)).Value.FiSegid).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B) + libc.Int32FromInt32(FTS5_DATA_DLI_B))) + ((int64)libc.Int32FromInt32(1)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B))) + ((int64)iLvl).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B))) + (int64)(@unsafe.Pointer.To<TFts5DlidxLvl>(__ccgo_ptr(pLvl + 1 * 32))).Value.FiLeafPgno);
                if ((@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)pLvl)).Value.FpData != 0) {
                    while (_fts5DlidxLvlNext(ref (tls).DerefOrNull(), pLvl) == 0) {
                    }
                    (@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)pLvl)).Value.FbEof = 0;
                }
            }
        }
    }
    return (@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)(pIter + 8))).Value.FbEof;
}

internal static void _fts5DoclistIterInit(ж<libc.TLS> tls, uintptr pBuf, uintptr pIter) {
    libc.Xmemset(tls, pIter, 0, (uint64)32);
    if ((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn > 0) {
        (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FaPoslist = (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp;
        (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FaEof = (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn;
        _fts5DoclistIterNext(ref (tls).DerefOrNull(), pIter);
    }
}

internal static int32 /*r*/ _fts5FindTokenizer(ж<libc.TLS> tls, uintptr pApi, uintptr zName, uintptr ppUserData, uintptr pTokenizer) {
    uintptr pMod = default;
    int32 rc = default;
    _ = pMod;
    _ = rc;
    rc = SQLITE_OK;
    pMod = _fts5LocateTokenizer(tls, pApi, zName);
    if (pMod != 0) {
        if ((@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pMod)).Value.FbV2Native == 0) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppUserData))).Value = (@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pMod)).Value.FpUserData;
        } else {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppUserData))).Value = pMod;
        }
        (@unsafe.Pointer.To<Tfts5_tokenizer>(__ccgo_ptr(pTokenizer))).Value = (@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pMod)).Value.Fx1;
    } else {
        libc.Xmemset(tls, pTokenizer, 0, (uint64)24);
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppUserData))).Value = (uintptr)0;
        rc = (int32)SQLITE_ERROR;
    }
    return rc;
}

internal static int32 /*r*/ _fts5LookaheadReaderInit(ж<libc.TLS> tls, uintptr a, int32 n, uintptr p) {
    libc.Xmemset(tls, p, 0, (uint64)32);
    (@unsafe.Pointer.To<TFts5LookaheadReader>((@unsafe.Pointer)p)).Value.Fa = a;
    (@unsafe.Pointer.To<TFts5LookaheadReader>((@unsafe.Pointer)p)).Value.Fn = n;
    _fts5LookaheadReaderNext(ref (tls).DerefOrNull(), p);
    return _fts5LookaheadReaderNext(ref (tls).DerefOrNull(), p);
}

internal static void _fts5SegIterClear(ж<libc.TLS> tls, uintptr pIter) {
    _sqlite3Fts5BufferFree(tls, pIter + 96);
    _fts5DataRelease(tls, (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf);
    _fts5DataRelease(tls, (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpNextLeaf);
    _fts5TombstoneArrayDelete(tls, (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpTombArray);
    _fts5DlidxIterFree(tls, (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpDlidx);
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FaRowidOffset);
    libc.Xmemset(tls, pIter, 0, (uint64)128);
}

internal static void _fts5SegIterInit(ж<libc.TLS> tls, uintptr p, uintptr pSeg, uintptr pIter) {
    if ((@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FpgnoFirst == 0) {
        return;
    }
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        libc.Xmemset(tls, pIter, 0, (uint64)128);
        _fts5SegIterSetNext(ref (tls).DerefOrNull(), p, pIter);
        (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpSeg = pSeg;
        (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiLeafPgno = (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FpgnoFirst - (int32)1;
        for (var cond = true; cond; cond = (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf != 0 && (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf)).Value.Fnn == (int32)4) {
            _fts5SegIterNextPage(tls, p, pIter);
        }
    }
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf != 0) {
        (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiLeafOffset = (int64)4;
        (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiPgidxOff = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf)).Value.FszLeaf + (int32)1;
        _fts5SegIterLoadTerm(tls, p, pIter, 0);
        _fts5SegIterLoadNPos(ref (tls).DerefOrNull(), p, pIter);
        _fts5SegIterAllocTombstone(tls, p, pIter);
    }
}

internal static void _fts5SegIterNextInit(ж<libc.TLS> tls, uintptr p, uintptr pTerm, int32 nTerm, uintptr pSeg, uintptr pIter) {
    uintptr a = default;
    uintptr pSel = default;
    int32 bDlidx = default;
    int32 iPg = default;
    ref var iTermOff = ref heap(new int32(), out var ᏑiTermOff);
    Ti64 val = default;
    _ = a;
    _ = bDlidx;
    _ = iPg;
    _ = iTermOff;
    _ = pSel;
    _ = val;
    iPg = -(int32)1;
    bDlidx = 0;
    pSel = (uintptr)0;
    pSel = _fts5IdxNextStmt(tls, p);
    if (pSel != 0) {
        Xsqlite3_bind_int(tls, pSel, (int32)1, (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FiSegid);
        Xsqlite3_bind_blob(tls, pSel, (int32)2, pTerm, nTerm, libc.UintptrFromInt32(0));
        if (Xsqlite3_step(tls, pSel) == (int32)SQLITE_ROW) {
            val = Xsqlite3_column_int64(tls, pSel, 0);
            iPg = (int32)(val >> (int)(libc.Int32FromInt32(1)));
            bDlidx = (int32)((Ti64)(val & libc.Int64FromInt32(0x0001)));
        }
        (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc = Xsqlite3_reset(tls, pSel);
        Xsqlite3_bind_null(tls, pSel, (int32)2);
        if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc != 0) {
            return;
        }
    }
    libc.Xmemset(tls, pIter, 0, (uint64)128);
    (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpSeg = pSeg;
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pIter + 8))).Value |= (int32)FTS5_SEGITER_ONETERM;
    if (iPg >= 0) {
        (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiLeafPgno = iPg - (int32)1;
        _fts5SegIterNextPage(tls, p, pIter);
        _fts5SegIterSetNext(ref (tls).DerefOrNull(), p, pIter);
    }
    if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf != 0) {
        a = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf)).Value.Fp;
        iTermOff = 0;
        (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiPgidxOff = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf)).Value.FszLeaf;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pIter + 64))).Value += _sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), a + (uintptr)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiPgidxOff, @unsafe.Pointer.FromPinnedBox(ᏑiTermOff));
        if (iTermOff > (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf)).Value.FszLeaf) {
            (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc = libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
            return;
        }
        (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiLeafOffset = (int64)iTermOff;
        _fts5SegIterLoadTerm(tls, p, pIter, 0);
        _fts5SegIterLoadNPos(ref (tls).DerefOrNull(), p, pIter);
        if (bDlidx != 0) {
            _fts5SegIterLoadDlidx(tls, p, pIter);
        }
    }
}

internal static void _fts5SegIterSeekInit(ж<libc.TLS> tls, uintptr p, uintptr pTerm, int32 nTerm, int32 flags, uintptr pSeg, uintptr pIter) {
    int32 bDlidx = default;
    int32 bGe = default;
    int32 iPg = default;
    uintptr pIdxSelect = default;
    Ti64 val = default;
    _ = bDlidx;
    _ = bGe;
    _ = iPg;
    _ = pIdxSelect;
    _ = val;
    iPg = (int32)1;
    bGe = flags & (int32)FTS5INDEX_QUERY_SCAN;
    bDlidx = 0;
    pIdxSelect = (uintptr)0;
    libc.Xmemset(tls, pIter, 0, (uint64)128);
    (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpSeg = pSeg;
    pIdxSelect = _fts5IdxSelectStmt(tls, p);
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc != 0) {
        return;
    }
    Xsqlite3_bind_int(tls, pIdxSelect, (int32)1, (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FiSegid);
    Xsqlite3_bind_blob(tls, pIdxSelect, (int32)2, pTerm, nTerm, libc.UintptrFromInt32(0));
    if ((int32)SQLITE_ROW == Xsqlite3_step(tls, pIdxSelect)) {
        val = (int64)Xsqlite3_column_int(tls, pIdxSelect, 0);
        iPg = (int32)(val >> (int)(libc.Int32FromInt32(1)));
        bDlidx = (int32)((Ti64)(val & libc.Int64FromInt32(0x0001)));
    }
    (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc = Xsqlite3_reset(tls, pIdxSelect);
    Xsqlite3_bind_null(tls, pIdxSelect, (int32)2);
    if (iPg < (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FpgnoFirst) {
        iPg = (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FpgnoFirst;
        bDlidx = 0;
    }
    (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiLeafPgno = iPg - (int32)1;
    _fts5SegIterNextPage(tls, p, pIter);
    if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf != 0) {
        _fts5LeafSeek(tls, p, bGe, pIter, pTerm, nTerm);
    }
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && (bGe == 0 || (flags & (int32)FTS5INDEX_QUERY_SCANONETERM) != 0)) {
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pIter + 8))).Value |= (int32)FTS5_SEGITER_ONETERM;
        if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf != 0) {
            if ((flags & (int32)FTS5INDEX_QUERY_DESC) != 0) {
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pIter + 8))).Value |= (int32)FTS5_SEGITER_REVERSE;
            }
            if (bDlidx != 0) {
                _fts5SegIterLoadDlidx(tls, p, pIter);
            }
            if ((flags & (int32)FTS5INDEX_QUERY_DESC) != 0) {
                _fts5SegIterReverse(tls, p, pIter);
            }
        }
    }
    _fts5SegIterSetNext(ref (tls).DerefOrNull(), p, pIter);
    if (0 == (flags & (int32)FTS5INDEX_QUERY_SCANONETERM)) {
        _fts5SegIterAllocTombstone(tls, p, pIter);
    }
}

internal static int32 /*r*/ _fts5StorageSaveTotals(ж<libc.TLS> tls, uintptr p) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 i = default;
        int32 nCol = default;
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        TFts5Buffer _ᴛ1 = default;
        _ = i;
        _ = nCol;
        _ = rc;
        nCol = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FnCol;
        rc = SQLITE_OK;
        libc.Xmemset(tls, bp, 0, (uint64)16);
        _sqlite3Fts5BufferAppendVarint(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), __ccgo_ptr(bp), (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FnTotalRow);
        i = 0;
        for (;;) {
            if (!(i < nCol)) {
                break;
            }
            _sqlite3Fts5BufferAppendVarint(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), __ccgo_ptr(bp), ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FaTotalSize + (uintptr)i * 8)));
            goto _1;
_1:;
            i = i + 1;
        }
        if (rc == SQLITE_OK) {
            rc = _sqlite3Fts5IndexSetAverages(tls, (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpIndex, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fp, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fn);
        }
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fp);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _fts5StructurePromoteTo(ж<libc.TLS> tls, uintptr p, int32 iPromote, int32 szPromote, uintptr pStruct) {
    int32 il = default;
    int32 @is = default;
    int32 sz = default;
    uintptr pLvl = default;
    uintptr pOut = default;
    _ = il;
    _ = @is;
    _ = pLvl;
    _ = pOut;
    _ = sz;
    pOut = pStruct + 32 + (uintptr)iPromote * 16;
    if ((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pOut)).Value.FnMerge == 0) {
        il = iPromote + (int32)1;
        for (;;) {
            if (!(il < (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnLevel)) {
                break;
            }
            pLvl = pStruct + 32 + (uintptr)il * 16;
            if ((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnMerge != 0) {
                return;
            }
            @is = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnSeg - (int32)1;
            for (;;) {
                if (!(@is >= 0)) {
                    break;
                }
                sz = _fts5SegmentSize(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FaSeg + (uintptr)@is * 56);
                if (sz > szPromote) {
                    return;
                }
                _fts5StructureExtendLevel(tls, p + 60, pStruct, iPromote, (int32)1, (int32)1);
                if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc != 0) {
                    return;
                }
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pOut)).Value.FaSeg, (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FaSeg + (uintptr)@is * 56, (uint64)56);
                (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pOut)).Value.FnSeg = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pOut)).Value.FnSeg + 1;
                (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnSeg = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnSeg - 1;
                goto _2;
_2:;
                @is = @is - 1;
            }
            goto _1;
_1:;
            il = il + 1;
        }
    }
}

internal static void _fts5TokendataMerge(ж<libc.TLS> tls, uintptr a1, int32 n1, uintptr a2, int32 n2, uintptr aOut) {
    int32 i1 = default;
    int32 i2 = default;
    uintptr pOut = default;
    _ = i1;
    _ = i2;
    _ = pOut;
    i1 = 0;
    i2 = 0;
    while (i1 < n1 || i2 < n2) {
        pOut = aOut + (uintptr)(i1 + i2) * 24;
        if (i2 >= n2 || i1 < n1 && ((@unsafe.Pointer.To<TFts5TokenDataMap>(__ccgo_ptr(a1 + (uintptr)i1 * 24))).Value.FiRowid < (@unsafe.Pointer.To<TFts5TokenDataMap>(__ccgo_ptr(a2 + (uintptr)i2 * 24))).Value.FiRowid || (@unsafe.Pointer.To<TFts5TokenDataMap>(__ccgo_ptr(a1 + (uintptr)i1 * 24))).Value.FiRowid == (@unsafe.Pointer.To<TFts5TokenDataMap>(__ccgo_ptr(a2 + (uintptr)i2 * 24))).Value.FiRowid && (@unsafe.Pointer.To<TFts5TokenDataMap>(__ccgo_ptr(a1 + (uintptr)i1 * 24))).Value.FiPos <= (@unsafe.Pointer.To<TFts5TokenDataMap>(__ccgo_ptr(a2 + (uintptr)i2 * 24))).Value.FiPos)) {
            libc.Xmemcpy(tls, pOut, a1 + (uintptr)i1 * 24, (uint64)24);
            i1 = i1 + 1;
        } else {
            libc.Xmemcpy(tls, pOut, a2 + (uintptr)i2 * 24, (uint64)24);
            i2 = i2 + 1;
        }
    }
}

internal static void _geopolyBBoxStep(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr pBBox = default;
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        array<TRtreeCoord> _ᴛ1 = new(4, () => new());
        _ = pBBox;
        _ = rc;
        rc = SQLITE_OK;
        _ = argc;
        _geopolyBBox(tls, context, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)), bp, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        if (rc == SQLITE_OK) {
            pBBox = Xsqlite3_aggregate_context(tls, context, (int32)20);
            if (pBBox == (uintptr)0) {
                return;
            }
            if (libc.AtomicLoadPInt32(pBBox) == 0) {
                libc.AtomicStorePInt32(pBBox, (int32)1);
                libc.Xmemcpy(tls, pBBox + 4, bp, libc.Uint64FromInt64(4) * libc.Uint64FromInt32(4));
            } else {
                if (~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)bp) < ~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(pBBox + 4))) {
                    (@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(pBBox + 4))).Value = @unsafe.Pointer.ElementRef<TRtreeCoord>(__ccgo_ptr(bp), 0, 4);
                }
                if (~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(bp + 1 * 4)) > ~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(pBBox + 4 + 1 * 4))) {
                    (@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(pBBox + 4 + 1 * 4))).Value = @unsafe.Pointer.ElementRef<TRtreeCoord>(__ccgo_ptr(bp), (int32)1, 4);
                }
                if (~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(bp + 2 * 4)) < ~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(pBBox + 4 + 2 * 4))) {
                    (@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(pBBox + 4 + 2 * 4))).Value = @unsafe.Pointer.ElementRef<TRtreeCoord>(__ccgo_ptr(bp), (int32)2, 4);
                }
                if (~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(bp + 3 * 4)) > ~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(pBBox + 4 + 3 * 4))) {
                    (@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(pBBox + 4 + 3 * 4))).Value = @unsafe.Pointer.ElementRef<TRtreeCoord>(__ccgo_ptr(bp), (int32)3, 4);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _havingToWhere(ж<libc.TLS> tls, uintptr pParse, uintptr p) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        libc.Xmemset(tls, bp, 0, (uint64)48);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_havingToWhereExprCb);
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0))).Value = p;
        _sqlite3WalkExpr(tls, bp, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpHaving);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _incrAggFunctionDepth(ж<libc.TLS> tls, uintptr pExpr, int32 N) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        if (N > 0) {
            libc.Xmemset(tls, bp, 0, (uint64)48);
            (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_incrAggDepth);
            (@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0))).Value = N;
            _sqlite3WalkExpr(tls, bp, pExpr);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _jsonAppendRaw(ж<libc.TLS> tls, uintptr p, uintptr zIn, Tu32 N) {
    if (N == (uint32)0) {
        return;
    }
    if ((uint64)N + (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed >= (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnAlloc) {
        _jsonStringExpandAndAppend(tls, p, zIn, N);
    } else {
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf + (uintptr)(@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed, zIn, (uint64)N);
        (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(p + 24))).Value += (uint64)N;
    }
}

internal static void _jsonAppendRawNZ(ж<libc.TLS> tls, uintptr p, uintptr zIn, Tu32 N) {
    if ((uint64)N + (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed >= (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnAlloc) {
        _jsonStringExpandAndAppend(tls, p, zIn, N);
    } else {
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf + (uintptr)(@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed, zIn, (uint64)N);
        (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(p + 24))).Value += (uint64)N;
    }
}

internal static int32 /*r*/ _jsonBlobMakeEditable(ж<libc.TLS> tls, uintptr pParse, Tu32 nExtra) {
    uintptr aOld = default;
    Tu64 nSize = default;
    _ = aOld;
    _ = nSize;
    if ((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.Foom != 0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlobAlloc > (uint32)0) {
        return (int32)1;
    }
    aOld = (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob;
    nSize = (uint64)((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob + nExtra);
    (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob = (uintptr)0;
    if (_jsonBlobExpand(tls, pParse, nSize) != 0) {
        return 0;
    }
    libc.Xmemcpy(tls, (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob, aOld, (uint64)(@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob);
    return (int32)1;
}

internal static void _jsonReturnTextJsonFromBlob(ж<libc.TLS> tls, uintptr ctx, uintptr aBlob, Tu32 nBlob) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(208);
        defer(tls.Free, (nint)208, ref ᒐ);
        TJsonString _ᴛ1 = new();
        TJsonParse _ᴛ2 = default;
        if (aBlob == (uintptr)0) {
            return;
        }
        libc.Xmemset(tls, bp, 0, (uint64)72);
        (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FaBlob = aBlob;
        (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FnBlob = nBlob;
        _jsonStringInit(ref (tls).DerefOrNull(), bp + 72, ctx);
        _jsonTranslateBlobToText(tls, bp, (uint32)0, bp + 72);
        _jsonReturnString(tls, bp + 72, (uintptr)0, (uintptr)0);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _jsonStringExpandAndAppend(ж<libc.TLS> tls, uintptr p, uintptr zIn, Tu32 N) {
    if (_jsonStringGrow(tls, p, N) != 0) {
        return;
    }
    libc.Xmemcpy(tls, (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf + (uintptr)(@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed, zIn, (uint64)N);
    (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(p + 24))).Value += (uint64)N;
}

internal static int32 /*r*/ _jsonStringGrow(ж<libc.TLS> tls, uintptr p, Tu32 N) {
    Tu64 nTotal = default;
    uintptr zNew = default;
    uintptr v2 = default;
    uint64 v1 = default;
    _ = nTotal;
    _ = zNew;
    _ = v1;
    _ = v2;
    if ((uint64)N < (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnAlloc) {
        v1 = (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnAlloc * (uint64)2;
    } else {
        v1 = (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnAlloc + (uint64)N + (uint64)10;
    }
    nTotal = v1;
    if ((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FbStatic != 0) {
        if ((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FeErr != 0) {
            return (int32)1;
        }
        zNew = _sqlite3RCStrNew(tls, nTotal);
        if (zNew == (uintptr)0) {
            _jsonStringOom(tls, p);
            return (int32)SQLITE_NOMEM;
        }
        libc.Xmemcpy(tls, zNew, (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf, (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed);
        (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf = zNew;
        (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FbStatic = (uint8)0;
    } else {
        (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf = _sqlite3RCStrResize(tls, (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf, nTotal);
        if ((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf == (uintptr)0) {
            v2 = p + 33;
            (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v2)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v2)) | libc.Int32FromInt32(JSTRING_OOM));
            _jsonStringZero(ref (tls).DerefOrNull(), p);
            return (int32)SQLITE_NOMEM;
        }
    }
    (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnAlloc = nTotal;
    return SQLITE_OK;
}

internal static int32 /*r*/ _memjrnlCreateFile(ж<libc.TLS> tls, uintptr p) {
    TMemJournal copy1 = default;
    Ti64 iOff = default;
    int32 nChunk = default;
    int32 rc = default;
    uintptr pIter = default;
    uintptr pReal = default;
    _ = copy1;
    _ = iOff;
    _ = nChunk;
    _ = pIter;
    _ = pReal;
    _ = rc;
    pReal = p;
    copy1 = ~@unsafe.Pointer.To<TMemJournal>(__ccgo_ptr(p));
    libc.Xmemset(tls, p, 0, (uint64)80);
    rc = _sqlite3OsOpen(tls, copy1.FpVfs, copy1.FzJournal, pReal, copy1.Fflags, (uintptr)0);
    if (rc == SQLITE_OK) {
        nChunk = copy1.FnChunkSize;
        iOff = 0;
        pIter = copy1.FpFirst;
        for (;;) {
            if (!(pIter != 0)) {
                break;
            }
            if (iOff + (int64)nChunk > copy1.Fendpoint.FiOffset) {
                nChunk = (int32)(copy1.Fendpoint.FiOffset - iOff);
            }
            rc = _sqlite3OsWrite(tls, pReal, pIter + 8, nChunk, iOff);
            if (rc != 0) {
                break;
            }
            iOff = iOff + (int64)nChunk;
            goto _1;
_1:;
            pIter = (@unsafe.Pointer.To<TFileChunk>((@unsafe.Pointer)pIter)).Value.FpNext;
        }
        if (rc == SQLITE_OK) {
            _memjrnlFreeChunks(tls, copy1.FpFirst);
        }
    }
    if (rc != SQLITE_OK) {
        _sqlite3OsClose(tls, pReal);
        (@unsafe.Pointer.To<TMemJournal>(__ccgo_ptr(p))).Value = copy1;
    }
    return rc;
}

internal static int32 /*r*/ _pager_incr_changecounter(ж<libc.TLS> tls, uintptr pPager, int32 isDirectMode) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr pCopy = default;
        uintptr zBuf = default;
        int32 rc = default;
        uintptr _ᴛ1 = default;
        _ = pCopy;
        _ = rc;
        _ = zBuf;
        rc = SQLITE_OK;
        _ = isDirectMode;
        if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FchangeCountDone != 0) && (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize > (uint32)0) {
            rc = _sqlite3PagerGet(tls, pPager, (uint32)1, bp, 0);
            if (libc.Bool(!(libc.Int32FromInt32(DIRECT_MODE) != 0)) && rc == SQLITE_OK) {
                rc = _sqlite3PagerWrite(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
            }
            if (rc == SQLITE_OK) {
                _pager_write_changecounter(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                if (DIRECT_MODE != 0) {
                    zBuf = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))))).Value.FpData;
                    if (rc == SQLITE_OK) {
                        rc = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, zBuf, (int32)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, 0);
                        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pPager + 248 + 2 * 4))).Value = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pPager + 248 + 2 * 4)) + 1;
                    }
                    if (rc == SQLITE_OK) {
                        pCopy = zBuf + 24;
                        libc.Xmemcpy(tls, pPager + 136, pCopy, (uint64)16);
                        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FchangeCountDone = (uint8)1;
                    }
                } else {
                    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FchangeCountDone = (uint8)1;
                }
            }
            _sqlite3PagerUnref(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _pcache1Init(ж<libc.TLS> tls, uintptr NotUsed) {
    _ = NotUsed;
    libc.Xmemset(tls, (uintptr)Ꮡ_pcache1_g, 0, (uint64)144);
    _pcache1_g.FseparateCache = 0;
    if (_sqlite3Config.FbCoreMutex != 0) {
        _pcache1_g.Fgrp.Fmutex = _sqlite3MutexAlloc(tls, (int32)SQLITE_MUTEX_STATIC_LRU);
        _pcache1_g.Fmutex = _sqlite3MutexAlloc(tls, (int32)SQLITE_MUTEX_STATIC_PMEM);
    }
    if (_pcache1_g.FseparateCache != 0 && _sqlite3Config.FnPage != 0 && _sqlite3Config.FpPage == (uintptr)0) {
        _pcache1_g.FnInitPage = _sqlite3Config.FnPage;
    } else {
        _pcache1_g.FnInitPage = 0;
    }
    _pcache1_g.Fgrp.FmxPinned = (uint32)10;
    libc.AtomicStorePInt32((uintptr)Ꮡ_pcache1_g + 80, (int32)1);
    return SQLITE_OK;
}

internal static void _pcache1Shutdown(ж<libc.TLS> tls, uintptr NotUsed) {
    _ = NotUsed;
    libc.Xmemset(tls, (uintptr)Ꮡ_pcache1_g, 0, (uint64)144);
}

internal static uintptr /*r*/ _pcacheFetchFinishWithInit(ж<libc.TLS> tls, uintptr pCache, TPgno pgno, uintptr pPage) {
    uintptr pPgHdr = default;
    _ = pPgHdr;
    pPgHdr = (@unsafe.Pointer.To<Tsqlite3_pcache_page>((@unsafe.Pointer)pPage)).Value.FpExtra;
    libc.Xmemset(tls, pPgHdr + 32, 0, libc.Uint64FromInt64(80) - (uint64)(libc.UintptrFromInt32(0) + 32));
    (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPgHdr)).Value.FpPage = pPage;
    (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPgHdr)).Value.FpData = (@unsafe.Pointer.To<Tsqlite3_pcache_page>((@unsafe.Pointer)pPage)).Value.FpBuf;
    (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPgHdr)).Value.FpExtra = pPgHdr + 1 * 80;
    libc.Xmemset(tls, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPgHdr)).Value.FpExtra, 0, (uint64)8);
    (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPgHdr)).Value.FpCache = pCache;
    (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPgHdr)).Value.Fpgno = pgno;
    (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPgHdr)).Value.Fflags = (uint16)PGHDR_CLEAN;
    return _sqlite3PcacheFetchFinish(tls, pCache, pgno, pPage);
}

internal static uintptr /*r*/ _pcacheSortDirtyList(ж<libc.TLS> tls, uintptr pIn) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(256);
        defer(tls.Free, (nint)256, ref ᒐ);
        int32 i = default;
        uintptr p = default;
        uintptr v3 = default;
        array<uintptr> _ᴛ1 = new(32);
        _ = i;
        _ = p;
        _ = v3;
        libc.Xmemset(tls, bp, 0, (uint64)256);
        while (pIn != 0) {
            p = pIn;
            pIn = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpDirty;
            (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpDirty = (uintptr)0;
            i = 0;
            for (;;) {
                if (!(i < libc.Int32FromInt32(N_SORT_BUCKET) - libc.Int32FromInt32(1))) {
                    break;
                }
                if (@unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 32) == (uintptr)0) {
                    @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 32) = p;
                    break;
                } else {
                    p = _pcacheMergeDirtyList(ref (tls).DerefOrNull(), @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 32), p);
                    @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 32) = (uintptr)0;
                }
                goto _1;
_1:;
                i = i + 1;
            }
            if (i == libc.Int32FromInt32(N_SORT_BUCKET) - libc.Int32FromInt32(1)) {
                @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 32) = _pcacheMergeDirtyList(ref (tls).DerefOrNull(), @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 32), p);
            }
        }
        p = @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), 0, 32);
        i = (int32)1;
        for (;;) {
            if (!(i < (int32)N_SORT_BUCKET)) {
                break;
            }
            if (@unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 32) == (uintptr)0) {
                goto _2;
            }
            if (p != 0) {
                v3 = _pcacheMergeDirtyList(ref (tls).DerefOrNull(), p, @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 32));
            } else {
                v3 = @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 32);
            }
            p = v3;
            goto _2;
_2:;
            i = i + 1;
        }
        r = p;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _pragmaVtabOpen(ж<libc.TLS> tls, uintptr pVtab, uintptr ppCursor) {
    uintptr pCsr = default;
    _ = pCsr;
    pCsr = Xsqlite3_malloc(tls, (int32)40);
    if (pCsr == (uintptr)0) {
        return (int32)SQLITE_NOMEM;
    }
    libc.Xmemset(tls, pCsr, 0, (uint64)40);
    (@unsafe.Pointer.To<TPragmaVtabCursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab = pVtab;
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppCursor))).Value = pCsr;
    return SQLITE_OK;
}

internal static uintptr /*r*/ _rbuMisuseError(ж<libc.TLS> tls) {
    uintptr pRet = default;
    _ = pRet;
    pRet = Xsqlite3_malloc64(tls, (uint64)416);
    if (pRet != 0) {
        libc.Xmemset(tls, pRet, 0, (uint64)416);
        (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRet)).Value.Frc = (int32)SQLITE_MISUSE;
    }
    return pRet;
}

internal static void _rbuObjIterFinalize(ж<libc.TLS> tls, uintptr pIter) {
    _rbuObjIterClearStatements(tls, pIter);
    Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FpTblIter);
    Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FpIdxIter);
    _rbuObjIterFreeCols(tls, pIter);
    libc.Xmemset(tls, pIter, 0, (uint64)192);
}

internal static uintptr /*r*/ _rbuStrndup(ж<libc.TLS> tls, uintptr zStr, @unsafe.Pointer pRc) {
    Tsize_t nCopy = default;
    uintptr zRet = default;
    _ = nCopy;
    _ = zRet;
    zRet = (uintptr)0;
    if (~@unsafe.Pointer.To<int32>(pRc) == SQLITE_OK) {
        if (zStr != 0) {
            nCopy = libc.Xstrlen(tls, zStr) + (uint64)1;
            zRet = Xsqlite3_malloc64(tls, nCopy);
            if (zRet != 0) {
                libc.Xmemcpy(tls, zRet, zStr, nCopy);
            } else {
                (@unsafe.Pointer.To<int32>(pRc)).Value = (int32)SQLITE_NOMEM;
            }
        }
    }
    return zRet;
}

internal static void _recomputeColumnsUsed(ж<libc.TLS> tls, uintptr pSelect, uintptr pSrcItem) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrcItem)).Value.FpSTab == (uintptr)0) {
            return;
        }
        libc.Xmemset(tls, bp, 0, (uint64)48);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_recomputeColumnsUsedExpr);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_sqlite3SelectWalkNoop);
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0))).Value = pSrcItem;
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrcItem)).Value.FcolUsed = (uint64)0;
        _sqlite3WalkSelect(tls, bp, pSelect);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _renameWalkWith(ж<libc.TLS> tls, uintptr pWalker, uintptr pSelect) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        int32 i = default;
        uintptr p = default;
        uintptr pCopy = default;
        uintptr pParse = default;
        uintptr pWith = default;
        TNameContext _ᴛ1 = new();
        _ = i;
        _ = p;
        _ = pCopy;
        _ = pParse;
        _ = pWith;
        pWith = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpWith;
        if (pWith != 0) {
            pParse = (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FpParse;
            pCopy = (uintptr)0;
            if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TCte>((@unsafe.Pointer)(pWith + 16))).Value.FpSelect)).Value.FselFlags & (uint32)SF_Expanded) == (uint32)0) {
                pCopy = _sqlite3WithDup(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pWith);
                pCopy = _sqlite3WithPush(tls, pParse, pCopy, (uint8)1);
            }
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TWith>((@unsafe.Pointer)pWith)).Value.FnCte)) {
                    break;
                }
                p = (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)(pWith + 16 + (uintptr)i * 48))).Value.FpSelect;
                libc.Xmemset(tls, bp, 0, (uint64)56);
                (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpParse = pParse;
                if (pCopy != 0) {
                    _sqlite3SelectPrep(tls, (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpParse, p, bp);
                }
                if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpParse)).Value.Fdb)).Value.FmallocFailed != 0) {
                    return;
                }
                _sqlite3WalkSelect(tls, pWalker, p);
                _sqlite3RenameExprlistUnmap(tls, pParse, (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)(pWith + 16 + (uintptr)i * 48))).Value.FpCols);
                goto _1;
_1:;
                i = i + 1;
            }
            if (pCopy != 0 && (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpWith == pCopy) {
                (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpWith = (@unsafe.Pointer.To<TWith>((@unsafe.Pointer)pCopy)).Value.FpOuter;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _renumberCursors(ж<libc.TLS> tls, uintptr pParse, uintptr p, int32 iExcept, uintptr aCsrMap) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        _srclistRenumberCursors(ref (tls).DerefOrNull(), pParse, aCsrMap, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc, iExcept);
        libc.Xmemset(tls, bp, 0, (uint64)48);
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0))).Value = aCsrMap;
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_renumberCursorsCb);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_sqlite3SelectWalkNoop);
        _sqlite3WalkSelect(tls, bp, p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _resetCursor(ж<libc.TLS> tls, uintptr pCsr) {
    int32 i = default;
    int32 ii = default;
    uintptr pInfo = default;
    uintptr pRtree = default;
    uintptr pStmt = default;
    _ = i;
    _ = ii;
    _ = pInfo;
    _ = pRtree;
    _ = pStmt;
    pRtree = (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab;
    if ((@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FaConstraint != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FnConstraint)) {
                break;
            }
            pInfo = (@unsafe.Pointer.To<TRtreeConstraint>(__ccgo_ptr((@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FaConstraint + (uintptr)i * 24))).Value.FpInfo;
            if (pInfo != 0) {
                if ((@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)pInfo)).Value.FxDelUser != 0) {
                    GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>((@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)pInfo)).Value.FxDelUser)(tls, (@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)pInfo)).Value.FpUser);
                }
                Xsqlite3_free(tls, pInfo);
            }
            goto _1;
_1:;
            i = i + 1;
        }
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FaConstraint);
        (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FaConstraint = (uintptr)0;
    }
    ii = 0;
    for (;;) {
        if (!(ii < (int32)RTREE_CACHE_SZ)) {
            break;
        }
        _nodeRelease(tls, pRtree, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pCsr + 88 + (uintptr)ii * 8)));
        goto _2;
_2:;
        ii = ii + 1;
    }
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FaPoint);
    pStmt = (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FpReadAux;
    libc.Xmemset(tls, pCsr, 0, (uint64)296);
    (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab = pRtree;
    (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FpReadAux = pStmt;
    Xsqlite3_reset(tls, pStmt);
}

internal static int32 /*r*/ _resolveOrderByTermToExprList(ж<libc.TLS> tls, uintptr pParse, uintptr pSelect, uintptr pE) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        uintptr db = default;
        uintptr pEList = default;
        int32 i = default;
        int32 rc = default;
        Tu8 savedSuppErr = default;
        TNameContext _ᴛ1 = new();
        _ = db;
        _ = i;
        _ = pEList;
        _ = rc;
        _ = savedSuppErr;
        pEList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpEList;
        libc.Xmemset(tls, bp, 0, (uint64)56);
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpSrcList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc;
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp)).of(TNameContext.ᏑFuNC)), 0))).Value = pEList;
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FncFlags = libc.Int32FromInt32(NC_AllowAgg) | libc.Int32FromInt32(NC_UEList) | libc.Int32FromInt32(NC_NoSelect);
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FnNcErr = 0;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        savedSuppErr = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FsuppressErr;
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FsuppressErr = (uint8)1;
        rc = _sqlite3ResolveExprNames(tls, bp, pE);
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FsuppressErr = savedSuppErr;
        if (rc != 0) {
            r = 0; goto ᒐdone;
        }
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr)) {
                break;
            }
            if (_sqlite3ExprCompare(tls, (uintptr)0, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)i * 32))).Value.FpExpr, pE, -(int32)1) < (int32)2) {
                r = i + (int32)1; goto ᒐdone;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static uintptr /*r*/ _rowSetEntrySort(ж<libc.TLS> tls, uintptr pIn) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(320);
        defer(tls.Free, (nint)320, ref ᒐ);
        uint32 i = default;
        uintptr pNext = default;
        uintptr v3 = default;
        array<uintptr> _ᴛ1 = new(40);
        _ = i;
        _ = pNext;
        _ = v3;
        libc.Xmemset(tls, bp, 0, (uint64)320);
        while (pIn != 0) {
            pNext = (@unsafe.Pointer.To<TRowSetEntry>((@unsafe.Pointer)pIn)).Value.FpRight;
            (@unsafe.Pointer.To<TRowSetEntry>((@unsafe.Pointer)pIn)).Value.FpRight = (uintptr)0;
            i = (uint32)0;
            for (;;) {
                if (!(@unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), (nint)(i), 40) != 0)) {
                    break;
                }
                pIn = _rowSetEntryMerge(ref (tls).DerefOrNull(), @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), (nint)(i), 40), pIn);
                @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), (nint)(i), 40) = (uintptr)0;
                goto _1;
_1:;
                i = i + 1;
            }
            @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), (nint)(i), 40) = pIn;
            pIn = pNext;
        }
        pIn = @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), 0, 40);
        i = (uint32)1;
        for (;;) {
            if (!((uint64)i < libc.Uint64FromInt64(320) / libc.Uint64FromInt64(8))) {
                break;
            }
            if (@unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), (nint)(i), 40) == (uintptr)0) {
                goto _2;
            }
            if (pIn != 0) {
                v3 = _rowSetEntryMerge(ref (tls).DerefOrNull(), pIn, @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), (nint)(i), 40));
            } else {
                v3 = @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), (nint)(i), 40);
            }
            pIn = v3;
            goto _2;
_2:;
            i = i + 1;
        }
        r = pIn;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _rtreeOpen(ж<libc.TLS> tls, uintptr pVTab, uintptr ppCursor) {
    uintptr pCsr = default;
    uintptr pRtree = default;
    int32 rc = default;
    _ = pCsr;
    _ = pRtree;
    _ = rc;
    rc = (int32)SQLITE_NOMEM;
    pRtree = pVTab;
    pCsr = Xsqlite3_malloc64(tls, (uint64)296);
    if (pCsr != 0) {
        libc.Xmemset(tls, pCsr, 0, (uint64)296);
        (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab = pVTab;
        rc = SQLITE_OK;
        (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnCursor = (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnCursor + 1;
    }
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppCursor))).Value = pCsr;
    return rc;
}

internal static TRtreeValue /*r*/ _rtreeValueDown(ж<libc.TLS> tls, uintptr v) {
    TRtreeValue r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        float64 d = default;
        uint32 v1 = default;
        float32 _ᴛ1 = default;
        uint32 _ᴛ2 = default;
        _ = d;
        _ = v1;
        d = Xsqlite3_value_double(tls, v);
        (@unsafe.Pointer.To<float32>(__ccgo_ptr(bp))).Value = (float32)d;
        if ((float64)(~@unsafe.Pointer.To<float32>(__ccgo_ptr(bp))) > d) {
            libc.Xmemcpy(tls, bp + 4, bp, (uint64)4);
            if ((~@unsafe.Pointer.To<uint32>(__ccgo_ptr(bp + 4)) & (uint32)0x80000000U) != (uint32)0) {
                v1 = ~@unsafe.Pointer.To<uint32>(__ccgo_ptr(bp + 4)) + (uint32)1;
            } else {
                v1 = ~@unsafe.Pointer.To<uint32>(__ccgo_ptr(bp + 4)) - (uint32)1;
            }
            (@unsafe.Pointer.To<uint32>(__ccgo_ptr(bp + 4))).Value = v1;
            libc.Xmemcpy(tls, bp, bp + 4, (uint64)4);
        }
        r = ~@unsafe.Pointer.To<float32>(__ccgo_ptr(bp));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static TRtreeValue /*r*/ _rtreeValueUp(ж<libc.TLS> tls, uintptr v) {
    TRtreeValue r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        float64 d = default;
        uint32 v1 = default;
        float32 _ᴛ1 = default;
        uint32 _ᴛ2 = default;
        _ = d;
        _ = v1;
        d = Xsqlite3_value_double(tls, v);
        (@unsafe.Pointer.To<float32>(__ccgo_ptr(bp))).Value = (float32)d;
        if ((float64)(~@unsafe.Pointer.To<float32>(__ccgo_ptr(bp))) < d) {
            libc.Xmemcpy(tls, bp + 4, bp, (uint64)4);
            if ((~@unsafe.Pointer.To<uint32>(__ccgo_ptr(bp + 4)) & (uint32)0x80000000U) != (uint32)0) {
                v1 = ~@unsafe.Pointer.To<uint32>(__ccgo_ptr(bp + 4)) - (uint32)1;
            } else {
                v1 = ~@unsafe.Pointer.To<uint32>(__ccgo_ptr(bp + 4)) + (uint32)1;
            }
            (@unsafe.Pointer.To<uint32>(__ccgo_ptr(bp + 4))).Value = v1;
            libc.Xmemcpy(tls, bp, bp + 4, (uint64)4);
        }
        r = ~@unsafe.Pointer.To<float32>(__ccgo_ptr(bp));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _selectWindowRewriteEList(ж<libc.TLS> tls, uintptr pParse, uintptr pWin, uintptr pSrc, uintptr pEList, uintptr pTab, @unsafe.Pointer ppSub) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(96);
        defer(tls.Free, (nint)96, ref ᒐ);
        TWindowRewrite _ᴛ1 = default;
        TWalker _ᴛ2 = new();
        libc.Xmemset(tls, bp, 0, (uint64)48);
        libc.Xmemset(tls, bp + 48, 0, (uint64)40);
        (@unsafe.Pointer.To<TWindowRewrite>(__ccgo_ptr(bp + 48))).Value.FpSub = ~@unsafe.Pointer.To<uintptr>(ppSub);
        (@unsafe.Pointer.To<TWindowRewrite>(__ccgo_ptr(bp + 48))).Value.FpWin = pWin;
        (@unsafe.Pointer.To<TWindowRewrite>(__ccgo_ptr(bp + 48))).Value.FpSrc = pSrc;
        (@unsafe.Pointer.To<TWindowRewrite>(__ccgo_ptr(bp + 48))).Value.FpTab = pTab;
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_selectWindowRewriteExprCb);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_selectWindowRewriteSelectCb);
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0))).Value = bp + 48;
        _sqlite3WalkExprList(tls, bp, pEList);
        (@unsafe.Pointer.To<uintptr>(ppSub)).Value = (@unsafe.Pointer.To<TWindowRewrite>(__ccgo_ptr(bp + 48))).Value.FpSub;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sessionChangesetStart(ж<libc.TLS> tls, @unsafe.Pointer pp, uintptr __ccgo_fp_xInput, uintptr pIn, int32 nChangeset, uintptr pChangeset, int32 bInvert, int32 bSkipEmpty) {
    int32 nByte = default;
    int32 v1 = default;
    uintptr pRet = default;
    _ = nByte;
    _ = pRet;
    _ = v1;
    (@unsafe.Pointer.To<uintptr>(pp)).Value = (uintptr)0;
    nByte = (int32)152;
    pRet = Xsqlite3_malloc(tls, nByte);
    if (!(pRet != 0)) {
        return (int32)SQLITE_NOMEM;
    }
    libc.Xmemset(tls, pRet, 0, (uint64)152);
    (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pRet)).Value.Fin.FaData = pChangeset;
    (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pRet)).Value.Fin.FnData = nChangeset;
    (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pRet)).Value.Fin.FxInput = __ccgo_fp_xInput;
    (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pRet)).Value.Fin.FpIn = pIn;
    if (__ccgo_fp_xInput != 0) {
        v1 = 0;
    } else {
        v1 = (int32)1;
    }
    (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pRet)).Value.Fin.FbEof = v1;
    (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pRet)).Value.FbInvert = bInvert;
    (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pRet)).Value.FbSkipEmpty = bSkipEmpty;
    (@unsafe.Pointer.To<uintptr>(pp)).Value = pRet;
    return SQLITE_OK;
}

internal static void _sqlite3AggInfoPersistWalkerInit(ж<libc.TLS> tls, uintptr pWalker, uintptr pParse) {
    libc.Xmemset(tls, pWalker, 0, (uint64)48);
    (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FpParse = pParse;
    (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FxExprCallback = __ccgo_fp(_agginfoPersistExprCb);
    (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FxSelectCallback = __ccgo_fp(_sqlite3SelectWalkNoop);
}

internal static void _sqlite3BtreeCursorZero(ж<libc.TLS> tls, uintptr p) {
    libc.Xmemset(tls, p, 0, (uint64)(libc.UintptrFromInt32(0) + 32));
}

internal static void _sqlite3CollapseDatabaseArray(ж<libc.TLS> tls, uintptr db) {
    int32 i = default;
    int32 j = default;
    int32 v2 = default;
    uintptr pDb = default;
    _ = i;
    _ = j;
    _ = pDb;
    _ = v2;
    v2 = libc.Int32FromInt32(2);
    j = v2;
    i = v2;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
            break;
        }
        pDb = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)i * 32;
        if ((@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pDb)).Value.FpBt == (uintptr)0) {
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pDb)).Value.FzDbSName);
            (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pDb)).Value.FzDbSName = (uintptr)0;
            goto _1;
        }
        if (j < i) {
            (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)j * 32))).Value = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)i * 32))).Value;
        }
        j = j + 1;
        goto _1;
_1:;
        i = i + 1;
    }
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb = j;
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb <= (int32)2 && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb != db + 704) {
        libc.Xmemcpy(tls, db + 704, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb, libc.Uint64FromInt32(2) * libc.Uint64FromInt64(32));
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb);
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb = db + 704;
    }
}

internal static uintptr /*r*/ _sqlite3DbMallocZero(ж<libc.TLS> tls, uintptr db, Tu64 n) {
    uintptr p = default;
    _ = p;
    p = _sqlite3DbMallocRaw(tls, db, n);
    if (p != 0) {
        libc.Xmemset(tls, p, 0, n);
    }
    return p;
}

internal static uintptr /*r*/ _sqlite3DbStrDup(ж<libc.TLS> tls, uintptr db, uintptr z) {
    Tsize_t n = default;
    uintptr zNew = default;
    _ = n;
    _ = zNew;
    if (z == (uintptr)0) {
        return (uintptr)0;
    }
    n = libc.Xstrlen(tls, z) + (uint64)1;
    zNew = _sqlite3DbMallocRaw(tls, db, n);
    if (zNew != 0) {
        libc.Xmemcpy(tls, zNew, z, n);
    }
    return zNew;
}

internal static int32 /*r*/ _sqlite3ExprCoveredByIndex(ж<libc.TLS> tls, uintptr pExpr, int32 iCur, uintptr pIdx) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        TWalker _ᴛ1 = new();
        TIdxCover _ᴛ2 = default;
        libc.Xmemset(tls, bp, 0, (uint64)48);
        (@unsafe.Pointer.To<TIdxCover>(__ccgo_ptr(bp + 48))).Value.FiCur = iCur;
        (@unsafe.Pointer.To<TIdxCover>(__ccgo_ptr(bp + 48))).Value.FpIdx = pIdx;
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_exprIdxCover);
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0))).Value = bp + 48;
        _sqlite3WalkExpr(tls, bp, pExpr);
        r = libc.BoolInt32(!((@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FeCode != 0));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3FixSrcList(ж<libc.TLS> tls, uintptr pFix, uintptr pList) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(128);
        defer(tls.Free, (nint)128, ref ᒐ);
        int32 res = default;
        TSelect _ᴛ1 = default;
        _ = res;
        res = 0;
        if (pList != 0) {
            libc.Xmemset(tls, bp, 0, (uint64)120);
            (@unsafe.Pointer.To<TSelect>(__ccgo_ptr(bp))).Value.FpSrc = pList;
            res = _sqlite3WalkSelect(tls, pFix + 8, bp);
        }
        r = res;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _sqlite3Fts5BufferFree(ж<libc.TLS> tls, uintptr pBuf) {
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp);
    libc.Xmemset(tls, pBuf, 0, (uint64)16);
}

internal static void _sqlite3FuncAuth(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        libc.Xmemset(tls, bp, 0, (uint64)48);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_authFunctions);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        _sqlite3WalkExpr(tls, bp, pExpr);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sqlite3IsNaN(ж<libc.TLS> tls, float64 _x) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = _x;
        int32 rc = default;
        Tu64 _ᴛ1 = default;
        _ = rc;
        libc.Xmemcpy(tls, bp + 8, bp, (uint64)8);
        rc = libc.BoolInt32((Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp + 8)) & (libc.Uint64FromInt32(0x7ff).Lsh((int64)(libc.Int32FromInt32(52))))) == libc.Uint64FromInt32(0x7ff).Lsh((int64)(libc.Int32FromInt32(52))) && (Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp + 8)) & (libc.Uint64FromInt32(1).Lsh((int64)(libc.Int32FromInt32(52))) - libc.Uint64FromInt32(1))) != (uint64)0);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3IsOverflow(ж<libc.TLS> tls, float64 _x) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = _x;
        int32 rc = default;
        Tu64 _ᴛ1 = default;
        _ = rc;
        libc.Xmemcpy(tls, bp + 8, bp, (uint64)8);
        rc = libc.BoolInt32((Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp + 8)) & (libc.Uint64FromInt32(0x7ff).Lsh((int64)(libc.Int32FromInt32(52))))) == libc.Uint64FromInt32(0x7ff).Lsh((int64)(libc.Int32FromInt32(52))));
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _sqlite3MallocEnd(ж<libc.TLS> tls) {
    if (_sqlite3Config.Fm.FxShutdown != 0) {
        GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>(_sqlite3Config.Fm.FxShutdown)(tls, _sqlite3Config.Fm.FpAppData);
    }
    libc.Xmemset(tls, (uintptr)Ꮡ_mem0, 0, (uint64)32);
}

internal static int32 /*r*/ _sqlite3MallocInit(ж<libc.TLS> tls) {
    int32 rc = default;
    _ = rc;
    if (_sqlite3Config.Fm.FxMalloc == (uintptr)0) {
        _sqlite3MemSetDefault(tls);
    }
    _mem0.Fmutex = _sqlite3MutexAlloc(tls, (int32)SQLITE_MUTEX_STATIC_MEM);
    if (_sqlite3Config.FpPage == (uintptr)0 || _sqlite3Config.FszPage < (int32)512 || _sqlite3Config.FnPage <= 0) {
        _sqlite3Config.FpPage = (uintptr)0;
        _sqlite3Config.FszPage = 0;
    }
    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>(_sqlite3Config.Fm.FxInit)(tls, _sqlite3Config.Fm.FpAppData);
    if (rc != SQLITE_OK) {
        libc.Xmemset(tls, (uintptr)Ꮡ_mem0, 0, (uint64)32);
    }
    return rc;
}

internal static uintptr /*r*/ _sqlite3MallocZero(ж<libc.TLS> tls, Tu64 n) {
    uintptr p = default;
    _ = p;
    p = _sqlite3Malloc(tls, n);
    if (p != 0) {
        libc.Xmemset(tls, p, 0, n);
    }
    return p;
}

internal static void _sqlite3PrngRestoreState(ж<libc.TLS> tls) {
    libc.Xmemcpy(tls, (uintptr)Ꮡ_sqlite3Prng, (uintptr)Ꮡ_sqlite3SavedPrng, (uint64)132);
}

internal static void _sqlite3PrngSaveState(ж<libc.TLS> tls) {
    libc.Xmemcpy(tls, (uintptr)Ꮡ_sqlite3SavedPrng, (uintptr)Ꮡ_sqlite3Prng, (uint64)132);
}

internal static void _sqlite3ProcessReturningSubqueries(ж<libc.TLS> tls, uintptr pEList, uintptr pTab) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        libc.Xmemset(tls, bp, 0, (uint64)48);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_sqlite3ExprWalkNoop);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_sqlite3ReturningSubqueryCorrelated);
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0))).Value = pTab;
        _sqlite3WalkExprList(tls, bp, pEList);
        if ((@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FeCode != 0) {
            (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_sqlite3ReturningSubqueryVarSelect);
            (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_sqlite3SelectWalkNoop);
            _sqlite3WalkExprList(tls, bp, pEList);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3RenameExprUnmap(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        Tu8 eMode = default;
        TWalker _ᴛ1 = new();
        _ = eMode;
        eMode = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode;
        libc.Xmemset(tls, bp, 0, (uint64)48);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_renameUnmapExprCb);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_renameUnmapSelectCb);
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode = (uint8)PARSE_MODE_UNMAP;
        _sqlite3WalkExpr(tls, bp, pExpr);
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode = eMode;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct _sqlite3ResolveSelfReference__ᴛ2 /*dyn*/ {
    public /*[0][88]*/ array<array<Tu8>> FsrcSpace = new(0, () => new(88));
    public TSrcList FsSrc;
    public array<byte> F__ccgo_pad2 = new(80);
}

internal static int32 /*r*/ _sqlite3ResolveSelfReference(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, int32 type1, uintptr pExpr, uintptr pList) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(144);
        defer(tls.Free, (nint)144, ref ᒐ);
        uintptr pSrc = default;
        int32 rc = default;
        int32 v1 = default;
        TNameContext _ᴛ1 = new();
        _sqlite3ResolveSelfReference__ᴛ2 _ᴛ2 = new();
        _ = pSrc;
        _ = rc;
        _ = v1;
        libc.Xmemset(tls, bp, 0, (uint64)56);
        libc.Xmemset(tls, bp + 56, 0, (uint64)88);
        pSrc = bp + 56;
        if (pTab != 0) {
            (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc = (int32)1;
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pSrc + 8))).Value.FzName = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName;
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pSrc + 8))).Value.FpSTab = pTab;
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pSrc + 8))).Value.FiCursor = -(int32)1;
            if ((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema != (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FaDb + 1 * 32))).Value.FpSchema) {
                type1 = type1 | (int32)NC_FromDDL;
            }
        }
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpSrcList = pSrc;
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FncFlags = type1 | (int32)NC_IsDDL;
        v1 = _sqlite3ResolveExprNames(tls, bp, pExpr);
        rc = v1;
        if (v1 != SQLITE_OK) {
            r = rc; goto ᒐdone;
        }
        if (pList != 0) {
            rc = _sqlite3ResolveExprListNames(tls, bp, pList);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static uintptr /*r*/ _sqlite3Utf16to8(ж<libc.TLS> tls, uintptr db, uintptr z, int32 nByte, Tu8 enc) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        TMem _ᴛ1 = new();
        libc.Xmemset(tls, bp, 0, (uint64)56);
        (@unsafe.Pointer.To<TMem>(__ccgo_ptr(bp))).Value.Fdb = db;
        _sqlite3VdbeMemSetStr(tls, bp, z, (int64)nByte, enc, libc.UintptrFromInt32(0));
        _sqlite3VdbeChangeEncoding(tls, bp, (int32)SQLITE_UTF8);
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            _sqlite3VdbeMemRelease(tls, bp);
            (@unsafe.Pointer.To<TMem>(__ccgo_ptr(bp))).Value.Fz = (uintptr)0;
        }
        r = (@unsafe.Pointer.To<TMem>(__ccgo_ptr(bp))).Value.Fz;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static uintptr /*r*/ _sqlite3VdbeCreate(ж<libc.TLS> tls, uintptr pParse) {
    uintptr db = default;
    uintptr p = default;
    _ = db;
    _ = p;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    p = _sqlite3DbMallocRawNN(tls, db, (uint64)312);
    if (p == (uintptr)0) {
        return (uintptr)0;
    }
    libc.Xmemset(tls, p + 136, 0, libc.Uint64FromInt64(312) - (uint64)(libc.UintptrFromInt32(0) + 136));
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb = db;
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpVdbe != 0) {
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpVdbe)).Value.FppVPrev = p + 16;
    }
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpVNext = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpVdbe;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FppVPrev = db + 8;
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpVdbe = p;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpParse = pParse;
    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe = p;
    _sqlite3VdbeAddOp2(tls, p, (int32)OP_Init, 0, (int32)1);
    return p;
}

internal static int32 /*r*/ _sqlite3VdbeMemAggValue(ж<libc.TLS> tls, uintptr pAccum, uintptr pOut, uintptr pFunc) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        Tsqlite3_context _ᴛ1 = default;
        libc.Xmemset(tls, bp, 0, (uint64)48);
        _sqlite3VdbeMemSetNull(tls, pOut);
        (@unsafe.Pointer.To<Tsqlite3_context>(__ccgo_ptr(bp))).Value.FpOut = pOut;
        (@unsafe.Pointer.To<Tsqlite3_context>(__ccgo_ptr(bp))).Value.FpMem = pAccum;
        (@unsafe.Pointer.To<Tsqlite3_context>(__ccgo_ptr(bp))).Value.FpFunc = pFunc;
        (@unsafe.Pointer.To<Tsqlite3_context>(__ccgo_ptr(bp))).Value.Fenc = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pAccum)).Value.Fdb)).Value.Fenc;
        GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pFunc)).Value.FxValue)(tls, bp);
        r = (@unsafe.Pointer.To<Tsqlite3_context>(__ccgo_ptr(bp))).Value.FisError;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3VdbeMemFinalize(ж<libc.TLS> tls, uintptr pMem, uintptr pFunc) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(112);
        defer(tls.Free, (nint)112, ref ᒐ);
        Tsqlite3_context _ᴛ1 = default;
        TMem _ᴛ2 = new();
        libc.Xmemset(tls, bp, 0, (uint64)48);
        libc.Xmemset(tls, bp + 48, 0, (uint64)56);
        (@unsafe.Pointer.To<TMem>(__ccgo_ptr(bp + 48))).Value.Fflags = (uint16)MEM_Null;
        (@unsafe.Pointer.To<TMem>(__ccgo_ptr(bp + 48))).Value.Fdb = (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb;
        (@unsafe.Pointer.To<Tsqlite3_context>(__ccgo_ptr(bp))).Value.FpOut = bp + 48;
        (@unsafe.Pointer.To<Tsqlite3_context>(__ccgo_ptr(bp))).Value.FpMem = pMem;
        (@unsafe.Pointer.To<Tsqlite3_context>(__ccgo_ptr(bp))).Value.FpFunc = pFunc;
        (@unsafe.Pointer.To<Tsqlite3_context>(__ccgo_ptr(bp))).Value.Fenc = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TMem>(__ccgo_ptr(bp + 48))).Value.Fdb)).Value.Fenc;
        GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pFunc)).Value.FxFinalize)(tls, bp);
        if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FszMalloc > 0) {
            _sqlite3DbFreeNN(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc);
        }
        libc.Xmemcpy(tls, pMem, bp + 48, (uint64)56);
        r = (@unsafe.Pointer.To<Tsqlite3_context>(__ccgo_ptr(bp))).Value.FisError;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _sqlite3VdbeMemMove(ж<libc.TLS> tls, uintptr pTo, uintptr pFrom) {
    _sqlite3VdbeMemRelease(tls, pTo);
    libc.Xmemcpy(tls, pTo, pFrom, (uint64)56);
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pFrom)).Value.Fflags = (uint16)MEM_Null;
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pFrom)).Value.FszMalloc = 0;
}

internal static void _sqlite3VdbeSwap(ж<libc.TLS> tls, uintptr pA, uintptr pB) {
    uintptr pTmp = default;
    uintptr ppTmp = default;
    uintptr zTmp = default;
    TVdbe tmp = new();
    _ = pTmp;
    _ = ppTmp;
    _ = tmp;
    _ = zTmp;
    tmp = ~@unsafe.Pointer.To<TVdbe>(__ccgo_ptr(pA));
    (@unsafe.Pointer.To<TVdbe>(__ccgo_ptr(pA))).Value = (@unsafe.Pointer.To<TVdbe>(__ccgo_ptr(pB))).Value;
    (@unsafe.Pointer.To<TVdbe>(__ccgo_ptr(pB))).Value = tmp;
    pTmp = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pA)).Value.FpVNext;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pA)).Value.FpVNext = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pB)).Value.FpVNext;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pB)).Value.FpVNext = pTmp;
    ppTmp = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pA)).Value.FppVPrev;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pA)).Value.FppVPrev = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pB)).Value.FppVPrev;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pB)).Value.FppVPrev = ppTmp;
    zTmp = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pA)).Value.FzSql;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pA)).Value.FzSql = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pB)).Value.FzSql;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pB)).Value.FzSql = zTmp;
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pB + 284))).Value |= (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pA)).Value.Fexpmask;
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pB + 288))).Value |= (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pA)).Value.Fsmimask;
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pB)).Value.FprepFlags = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pA)).Value.FprepFlags;
    libc.Xmemcpy(tls, pB + 212, pA + 212, (uint64)36);
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pB + 212 + 5 * 4))).Value = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pB + 212 + 5 * 4)) + 1;
}

internal static int32 /*r*/ _sqlite3WalCheckpoint(ж<libc.TLS> tls, uintptr pWal, uintptr db, int32 eMode, uintptr __ccgo_fp_xBusy, uintptr pBusyArg, int32 sync_flags, int32 nBuf, uintptr zBuf, uintptr pnLog, uintptr pnCkpt) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(96);
        defer(tls.Free, (nint)96, ref ᒐ);
        int32 eMode2 = default;
        int32 rc = default;
        int32 v1 = default;
        uintptr xBusy2 = default;
        TWalSehCheckpoint _ᴛ1 = default;
        int32 _ᴛ2 = default;
        _ = eMode2;
        _ = rc;
        _ = xBusy2;
        _ = v1;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = 0;
        eMode2 = eMode;
        xBusy2 = __ccgo_fp_xBusy;
        if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FreadOnly != 0) {
            r = (int32)SQLITE_READONLY; goto ᒐdone;
        }
        if (xBusy2 != 0) {
        }
        if (eMode != -(int32)1) {
            rc = _walLockExclusive(tls, pWal, (int32)WAL_CKPT_LOCK, (int32)1);
            if (rc == SQLITE_OK) {
                (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FckptLock = (uint8)1;
                if (eMode != SQLITE_CHECKPOINT_PASSIVE) {
                    rc = _walBusyLock(tls, pWal, xBusy2, pBusyArg, WAL_WRITE_LOCK, (int32)1);
                    if (rc == SQLITE_OK) {
                        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FwriteLock = (uint8)1;
                    } else {
                        if (rc == (int32)SQLITE_BUSY) {
                            eMode2 = SQLITE_CHECKPOINT_PASSIVE;
                            xBusy2 = (uintptr)0;
                            rc = SQLITE_OK;
                        }
                    }
                }
            }
        } else {
            rc = SQLITE_OK;
        }
        (@unsafe.Pointer.To<TWalSehCheckpoint>(__ccgo_ptr(bp + 8))).Value.Frc = rc;
        (@unsafe.Pointer.To<TWalSehCheckpoint>(__ccgo_ptr(bp + 8))).Value.FpisChanged = bp;
        (@unsafe.Pointer.To<TWalSehCheckpoint>(__ccgo_ptr(bp + 8))).Value.FeMode2 = eMode2;
        (@unsafe.Pointer.To<TWalSehCheckpoint>(__ccgo_ptr(bp + 8))).Value.FxBusy2 = xBusy2;
        (@unsafe.Pointer.To<TWalSehCheckpoint>(__ccgo_ptr(bp + 8))).Value.FpBusyArg = pBusyArg;
        (@unsafe.Pointer.To<TWalSehCheckpoint>(__ccgo_ptr(bp + 8))).Value.Fdb = db;
        (@unsafe.Pointer.To<TWalSehCheckpoint>(__ccgo_ptr(bp + 8))).Value.Fsync_flags = sync_flags;
        (@unsafe.Pointer.To<TWalSehCheckpoint>(__ccgo_ptr(bp + 8))).Value.FnBuf = nBuf;
        (@unsafe.Pointer.To<TWalSehCheckpoint>(__ccgo_ptr(bp + 8))).Value.FzBuf = zBuf;
        (@unsafe.Pointer.To<TWalSehCheckpoint>(__ccgo_ptr(bp + 8))).Value.FpnLog = pnLog;
        (@unsafe.Pointer.To<TWalSehCheckpoint>(__ccgo_ptr(bp + 8))).Value.FpnCkpt = pnCkpt;
        rc = _walSehTry(tls, pWal, __ccgo_fp(_walSehCheckpoint), bp + 8, __ccgo_fp(_walHandleException));
        if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) != 0) {
            libc.Xmemset(tls, pWal + 72, 0, (uint64)48);
        }
        _sqlite3WalEndWriteTransaction(tls, pWal);
        if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FckptLock != 0) {
            _walUnlockExclusive(tls, pWal, (int32)WAL_CKPT_LOCK, (int32)1);
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FckptLock = (uint8)0;
        }
        if (rc == SQLITE_OK && eMode != eMode2) {
            v1 = (int32)SQLITE_BUSY;
        } else {
            v1 = rc;
        }
        r = v1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _sqlite3WalSnapshotGet(ж<libc.TLS> tls, uintptr pWal, uintptr ppSnapshot) {
    uintptr pRet = default;
    int32 rc = default;
    _ = pRet;
    _ = rc;
    rc = SQLITE_OK;
    if (libc.Xmemcmp(tls, pWal + 72 + 24, (uintptr)Ꮡ_aZero, (uint64)16) == 0) {
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppSnapshot))).Value = (uintptr)0;
        return (int32)SQLITE_ERROR;
    }
    pRet = Xsqlite3_malloc(tls, (int32)48);
    if (pRet == (uintptr)0) {
        rc = (int32)SQLITE_NOMEM;
    } else {
        libc.Xmemcpy(tls, pRet, pWal + 72, (uint64)48);
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppSnapshot))).Value = pRet;
    }
    return rc;
}

internal static uintptr /*r*/ _sqlite3WhereRealloc(ж<libc.TLS> tls, uintptr pWInfo, uintptr pOld, Tu64 nByte) {
    uintptr pNew = default;
    uintptr pOldBlk = default;
    _ = pNew;
    _ = pOldBlk;
    pNew = _sqlite3WhereMalloc(tls, pWInfo, nByte);
    if (pNew != 0 && pOld != 0) {
        pOldBlk = pOld;
        pOldBlk -= 16;
        libc.Xmemcpy(tls, pNew, pOld, (@unsafe.Pointer.To<TWhereMemBlock>((@unsafe.Pointer)pOldBlk)).Value.Fsz);
    }
    return pNew;
}

internal static int32 /*r*/ _sqlite3changegroup_new__ccgo_gp(ж<libc.TLS> tls, @unsafe.Pointer pp) {
    uintptr p = default;
    int32 rc = default;
    _ = p;
    _ = rc;
    rc = SQLITE_OK;
    p = Xsqlite3_malloc(tls, (int32)96);
    if (p == (uintptr)0) {
        rc = (int32)SQLITE_NOMEM;
    } else {
        libc.Xmemset(tls, p, 0, (uint64)96);
    }
    (@unsafe.Pointer.To<uintptr>(pp)).Value = p;
    return rc;
}

internal static void _statClearPage(ж<libc.TLS> tls, uintptr p) {
    uintptr aPg = default;
    _ = aPg;
    aPg = (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FaPg;
    _statClearCells(tls, p);
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FzPath);
    libc.Xmemset(tls, p, 0, (uint64)64);
    (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FaPg = aPg;
}

internal static int32 /*r*/ _statOpen(ж<libc.TLS> tls, uintptr pVTab, uintptr ppCursor) {
    uintptr pCsr = default;
    uintptr pTab = default;
    _ = pCsr;
    _ = pTab;
    pTab = pVTab;
    pCsr = Xsqlite3_malloc64(tls, (uint64)2152);
    if (pCsr == (uintptr)0) {
        return (int32)SQLITE_NOMEM;
    } else {
        libc.Xmemset(tls, pCsr, 0, (uint64)2152);
        (@unsafe.Pointer.To<TStatCursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab = pVTab;
        (@unsafe.Pointer.To<TStatCursor>((@unsafe.Pointer)pCsr)).Value.FiDb = (@unsafe.Pointer.To<TStatTable>((@unsafe.Pointer)pTab)).Value.FiDb;
    }
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppCursor))).Value = pCsr;
    return SQLITE_OK;
}

internal static uintptr /*r*/ _strAccumFinishRealloc(ж<libc.TLS> tls, @unsafe.Pointer p) {
    uintptr zText = default;
    @unsafe.Pointer v1 = default;
    _ = zText;
    _ = v1;
    zText = _sqlite3DbMallocRaw(tls, ~@unsafe.Pointer.To<uintptr>(p), (uint64)1 + (uint64)(~@unsafe.Pointer.To<Tu32>(@unsafe.Add(p, 24))));
    if (zText != 0) {
        libc.Xmemcpy(tls, zText, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8)), (uint64)(~@unsafe.Pointer.To<Tu32>(@unsafe.Add(p, 24)) + (uint32)1));
        v1 = @unsafe.Add(p, 29);
        (@unsafe.Pointer.To<Tu8>(v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>(v1)) | libc.Int32FromInt32(SQLITE_PRINTF_MALLOCED));
    } else {
        _sqlite3StrAccumSetError(tls, p, (uint8)SQLITE_NOMEM);
    }
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8))).Value = zText;
    return zText;
}

internal static int32 /*r*/ _synthCollSeq(ж<libc.TLS> tls, uintptr db, uintptr pColl) {
    int32 i = default;
    uintptr pColl2 = default;
    uintptr z = default;
    _ = i;
    _ = pColl2;
    _ = z;
    z = (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.FzName;
    i = 0;
    for (;;) {
        if (!(i < (int32)3)) {
            break;
        }
        pColl2 = _sqlite3FindCollSeq(tls, db, _aEnc[i], z, 0);
        if ((@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl2)).Value.FxCmp != (uintptr)0) {
            libc.Xmemcpy(tls, pColl, pColl2, (uint64)40);
            (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.FxDel = (uintptr)0;
            return SQLITE_OK;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return (int32)SQLITE_ERROR;
}

internal static void _vdbePmaReaderClear(ж<libc.TLS> tls, uintptr pReadr) {
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)pReadr)).Value.FaAlloc);
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)pReadr)).Value.FaBuffer);
    if ((@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)pReadr)).Value.FaMap != 0) {
        _sqlite3OsUnfetch(tls, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)pReadr)).Value.FpFd, 0, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)pReadr)).Value.FaMap);
    }
    _vdbeIncrFree(tls, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)pReadr)).Value.FpIncr);
    libc.Xmemset(tls, pReadr, 0, (uint64)80);
}

internal static void _vdbeSortSubtaskCleanup(ж<libc.TLS> tls, uintptr db, uintptr pTask) {
    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpUnpacked);
    if ((@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Flist.FaMemory != 0) {
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Flist.FaMemory);
    } else {
        _vdbeSorterRecordFree(tls, (uintptr)0, (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Flist.FpList);
    }
    if ((@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile.FpFd != 0) {
        _sqlite3OsCloseFree(tls, (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile.FpFd);
    }
    if ((@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile2.FpFd != 0) {
        _sqlite3OsCloseFree(tls, (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile2.FpFd);
    }
    libc.Xmemset(tls, pTask, 0, (uint64)104);
}

internal static int32 /*r*/ _walBeginReadTransaction(ж<libc.TLS> tls, uintptr pWal, uintptr pChanged) {
    int32 bChanged = default;
    int32 ckptLock = default;
    ref var cnt = ref heap(new int32(), out var Ꮡcnt);
    int32 rc = default;
    uintptr pInfo = default;
    uintptr pSnapshot = default;
    _ = bChanged;
    _ = ckptLock;
    _ = cnt;
    _ = pInfo;
    _ = pSnapshot;
    _ = rc;
    cnt = 0;
    ckptLock = 0;
    bChanged = 0;
    pSnapshot = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpSnapshot;
    if (pSnapshot != 0) {
        if (libc.Xmemcmp(tls, pSnapshot, pWal + 72, (uint64)48) != 0) {
            bChanged = (int32)1;
        }
        rc = _walLockShared(tls, pWal, (int32)WAL_CKPT_LOCK);
        if (rc != SQLITE_OK) {
            return rc;
        }
        ckptLock = (int32)1;
    }
    for (var cond = true; cond; cond = rc == -(int32)1) {
        rc = _walTryBeginRead(tls, pWal, (@unsafe.Pointer)pChanged, 0, @unsafe.Pointer.FromPinnedBox(Ꮡcnt));
    }
    if (rc == SQLITE_OK) {
        if (pSnapshot != 0 && libc.Xmemcmp(tls, pSnapshot, pWal + 72, (uint64)48) != 0) {
            pInfo = _walCkptInfo(ref (tls).DerefOrNull(), pWal);
            if (!(libc.Xmemcmp(tls, pSnapshot + 32, pWal + 72 + 32, (uint64)8) != 0) && (@unsafe.Pointer.To<TWalIndexHdr>((@unsafe.Pointer)pSnapshot)).Value.FmxFrame >= (@unsafe.Pointer.To<TWalCkptInfo>((@unsafe.Pointer)pInfo)).Value.FnBackfillAttempted) {
                libc.Xmemcpy(tls, pWal + 72, pSnapshot, (uint64)48);
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pChanged))).Value = bChanged;
            } else {
                rc = libc.Int32FromInt32(SQLITE_ERROR) | libc.Int32FromInt32(3).Lsh((int64)(libc.Int32FromInt32(8)));
            }
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FminFrame = (uint32)1;
            if (rc != SQLITE_OK) {
                _sqlite3WalEndReadTransaction(tls, pWal);
            }
        }
    }
    if (ckptLock != 0) {
        _walUnlockShared(tls, pWal, (int32)WAL_CKPT_LOCK);
    }
    return rc;
}

internal static int32 /*r*/ _walSehUndo(ж<libc.TLS> tls, uintptr pWal, uintptr p) {
    uintptr a = default;
    TPgno iFrame = default;
    int32 rc = default;
    _ = a;
    _ = iFrame;
    _ = rc;
    a = p;
    rc = SQLITE_OK;
    libc.Xmemcpy(tls, pWal + 72, _walIndexHdr(ref (tls).DerefOrNull(), pWal), (uint64)48);
    iFrame = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame + (uint32)1;
    for (;;) {
        if (!(rc == SQLITE_OK && iFrame <= (@unsafe.Pointer.To<TWalSehUndo>((@unsafe.Pointer)a)).Value.FiMax)) {
            break;
        }
        rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, TPgno, int32>>((@unsafe.Pointer.To<TWalSehUndo>((@unsafe.Pointer)a)).Value.FxUndo)(tls, (@unsafe.Pointer.To<TWalSehUndo>((@unsafe.Pointer)a)).Value.FpUndoCtx, _walFramePgno(ref (tls).DerefOrNull(), pWal, iFrame));
        goto _1;
_1:;
        iFrame = iFrame + 1;
    }
    if ((@unsafe.Pointer.To<TWalSehUndo>((@unsafe.Pointer)a)).Value.FiMax != (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame) {
        _walCleanupHash(tls, pWal);
    }
    return rc;
}

internal static void _whereOrMove(ж<libc.TLS> tls, uintptr pDest, uintptr pSrc) {
    (@unsafe.Pointer.To<TWhereOrSet>((@unsafe.Pointer)pDest)).Value.Fn = (@unsafe.Pointer.To<TWhereOrSet>((@unsafe.Pointer)pSrc)).Value.Fn;
    libc.Xmemcpy(tls, pDest + 8, pSrc + 8, (uint64)(@unsafe.Pointer.To<TWhereOrSet>((@unsafe.Pointer)pDest)).Value.Fn * (uint64)16);
}

internal static void _windowRemoveExprFromSelect(ж<libc.TLS> tls, uintptr pSelect, uintptr pExpr) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpWin != 0) {
            libc.Xmemset(tls, bp, 0, (uint64)48);
            (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_resolveRemoveWindowsCb);
            (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0))).Value = pSelect;
            _sqlite3WalkExpr(tls, bp, pExpr);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _xferCompatibleCheck(ж<libc.TLS> tls, uintptr pDest, uintptr pSrc) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        if (_sqlite3ExprListCompare(tls, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FpCheck, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FpCheck, -(int32)1) != 0) {
            r = 0; goto ᒐdone;
        }
        if ((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FiPKey < 0) {
            libc.Xmemset(tls, bp, 0, (uint64)48);
            (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_xferCheckRowid);
            _sqlite3WalkExprList(tls, bp, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FpCheck);
            if ((@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FeCode != 0) {
                r = 0; goto ᒐdone;
            }
        }
        r = (int32)1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

} // end sqlite3_package
