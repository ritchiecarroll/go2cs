namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static uintptr /*r*/ _fts5ConfigSkipLiteral(ж<libc.TLS> tls, uintptr pIn) {
    uintptr p = default;
    _ = p;
    p = pIn;
    var exprᴛ1 = (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p)));
    var matchᴛ1 = false;
    if (exprᴛ1 == (int32)'n') { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)'N') { matchᴛ1 = true;
        if (Xsqlite3_strnicmp(tls, __ccgo_ts + 1716, p, (int32)4) == 0) {
            p = p + 4;
        } else {
            p = (uintptr)0;
        }
    }
    else if (exprᴛ1 == (int32)'x') { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)'X') { matchᴛ1 = true;
        p = p + 1;
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) == (int32)'\'') {
            p = p + 1;
            while ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) >= (int32)'a' && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) <= (int32)'f' || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) >= (int32)'A' && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) <= (int32)'F' || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) >= (int32)'0' && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) <= (int32)'9') {
                p = p + 1;
            }
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) == (int32)'\'' && 0 == ((int64)p - (int64)pIn) % (int64)2) {
                p = p + 1;
            } else {
                p = (uintptr)0;
            }
        } else {
            p = (uintptr)0;
        }
    }
    else if (exprᴛ1 == (int32)'\'') {
        p = p + 1;
        while (p != 0) {
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) == (int32)'\'') {
                p = p + 1;
                if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) != (int32)'\'') {
                    break;
                }
            }
            p = p + 1;
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) == 0) {
                p = (uintptr)0;
            }
        }
    }
    else if (!matchᴛ1) { /* default: */
        do {
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) == (int32)'+' || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) == (int32)'-') {
                p = p + 1;
            }
            while (_fts5_isdigit(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) != 0) {
                p = p + 1;
            }
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) == (int32)'.' && _fts5_isdigit(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<int8>(__ccgo_ptr(p + 1))) != 0) {
                p = p + (uintptr)2;
                while (_fts5_isdigit(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) != 0) {
                    p = p + 1;
                }
            }
            if (p == pIn) {
                p = (uintptr)0;
            }
            break;
        } while (false);
    }

    return p;
}

internal static void _timediffFunc(ж<libc.TLS> tls, uintptr context, int32 NotUsed1, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(192);
        defer(tls.Free, (nint)192, ref ᒐ);
        int32 M = default;
        int32 Y = default;
        int8 sign = default;
        uintptr v1 = default;
        TDateTime _ᴛ1 = default;
        TDateTime _ᴛ2 = default;
        Tsqlite3_str _ᴛ3 = default;
        _ = M;
        _ = Y;
        _ = sign;
        _ = v1;
        _ = NotUsed1;
        if (_isDate(tls, context, (int32)1, argv, bp) != 0) {
            return;
        }
        if (_isDate(tls, context, (int32)1, argv + 1 * 8, bp + 48) != 0) {
            return;
        }
        _computeYMD_HMS(tls, bp);
        _computeYMD_HMS(tls, bp + 48);
        if ((@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FiJD >= (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FiJD) {
            sign = (int8)'+';
            Y = (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FY - (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FY;
            if (Y != 0) {
                (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FY = (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FY;
                (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FvalidJD = 0;
                _computeJD(tls, bp + 48);
            }
            M = (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FM - (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FM;
            if (M < 0) {
                Y = Y - 1;
                M = M + (int32)12;
            }
            if (M != 0) {
                (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FM = (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FM;
                (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FvalidJD = 0;
                _computeJD(tls, bp + 48);
            }
            while ((@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FiJD < (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FiJD) {
                M = M - 1;
                if (M < 0) {
                    M = (int32)11;
                    Y = Y - 1;
                }
                (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FM = (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FM - 1;
                if ((@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FM < (int32)1) {
                    (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FM = (int32)12;
                    (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FY = (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FY - 1;
                }
                (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FvalidJD = 0;
                _computeJD(tls, bp + 48);
            }
            (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FiJD -= (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FiJD;
            v1 = bp;
            (@unsafe.Pointer.To<Tsqlite3_int64>((@unsafe.Pointer)v1)).Value = (Tsqlite3_int64)((uint64)(~@unsafe.Pointer.To<Tsqlite3_int64>((@unsafe.Pointer)v1)) + libc.Uint64FromInt32(1486995408) * libc.Uint64FromInt32(100000));
        } else {
            sign = (int8)'-';
            Y = (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FY - (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FY;
            if (Y != 0) {
                (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FY = (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FY;
                (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FvalidJD = 0;
                _computeJD(tls, bp + 48);
            }
            M = (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FM - (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FM;
            if (M < 0) {
                Y = Y - 1;
                M = M + (int32)12;
            }
            if (M != 0) {
                (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FM = (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FM;
                (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FvalidJD = 0;
                _computeJD(tls, bp + 48);
            }
            while ((@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FiJD > (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FiJD) {
                M = M - 1;
                if (M < 0) {
                    M = (int32)11;
                    Y = Y - 1;
                }
                (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FM = (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FM + 1;
                if ((@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FM > (int32)12) {
                    (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FM = (int32)1;
                    (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FY = (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FY + 1;
                }
                (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FvalidJD = 0;
                _computeJD(tls, bp + 48);
            }
            (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FiJD = (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp + 48))).Value.FiJD - (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FiJD;
            v1 = bp;
            (@unsafe.Pointer.To<Tsqlite3_int64>((@unsafe.Pointer)v1)).Value = (Tsqlite3_int64)((uint64)(~@unsafe.Pointer.To<Tsqlite3_int64>((@unsafe.Pointer)v1)) + libc.Uint64FromInt32(1486995408) * libc.Uint64FromInt32(100000));
        }
        _clearYMD_HMS_TZ(ref (tls).DerefOrNull(), __ccgo_ptr(bp));
        _computeYMD_HMS(tls, bp);
        _sqlite3StrAccumInit(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 96), (uintptr)0, (uintptr)0, 0, (int32)100);
        Xsqlite3_str_appendf(tls, bp + 96, __ccgo_ts + 1511, libc.VaList(bp + 136, (int32)sign, Y, M, (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FD - (int32)1, (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.Fh, (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.Fm, (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.Fs));
        Xsqlite3_result_str(tls, context, bp + 96, (int32)SQLITE_XFER);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end sqlite3_package
