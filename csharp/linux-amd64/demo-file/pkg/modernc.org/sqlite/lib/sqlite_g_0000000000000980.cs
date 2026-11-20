global using Tfpos_t = go.modernc.org.libc_package.Tfpos_tᴛ1;

namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static Tsqlite3_int64 /*r*/ Xsqlite3_incomplete(ж<libc.TLS> tls, uintptr zSql) {
    int32 c = default;
    int32 nId = default;
    int32 nParen = default;
    Tu8 pending = default;
    Tu8 state = default;
    Tu8 token = default;
    _ = c;
    _ = nId;
    _ = nParen;
    _ = pending;
    _ = state;
    _ = token;
    state = (uint8)0;
    pending = (uint8)0;
    nParen = 0;
    while (~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql)) != 0) {
        var exprᴛ1 = (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql)));
        var matchᴛ1 = false;
        if (exprᴛ1 == (int32)';') { matchᴛ1 = true;
            token = (uint8)tkSEMI;
        }
        else if (exprᴛ1 == (int32)' ') { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)'\r') { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)'\t') { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)'\n') { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)'\f') { matchᴛ1 = true;
            token = (uint8)tkWS;
        }
        else if (exprᴛ1 == (int32)'/') { matchᴛ1 = true;
            do {
                if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql + 1))) != (int32)'*') {
                    token = (uint8)tkOTHER;
                    break;
                }
                zSql = zSql + (uintptr)2;
                while (~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql)) != 0 && ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql))) != (int32)'*' || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql + 1))) != (int32)'/')) {
                    zSql = zSql + 1;
                }
                if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql))) == 0) {
                    pending = (uint8)'/';
                    goto incomplete_finish;
                }
                zSql = zSql + 1;
                token = (uint8)tkWS;
            } while (false);
        }
        else if (exprᴛ1 == (int32)'-') { matchᴛ1 = true;
            do {
                if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql + 1))) != (int32)'-') {
                    token = (uint8)tkOTHER;
                    break;
                }
                while (~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql)) != 0 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql))) != (int32)'\n') {
                    zSql = zSql + 1;
                }
                if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql))) == 0) {
                    pending = (uint8)'-';
                    goto incomplete_finish;
                }
                token = (uint8)tkWS;
            } while (false);
        }
        else if (exprᴛ1 == (int32)'[') { matchᴛ1 = true;
            zSql = zSql + 1;
            while (~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql)) != 0 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql))) != (int32)']') {
                zSql = zSql + 1;
            }
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql))) == 0) {
                pending = (uint8)']';
                goto incomplete_finish;
            }
            token = (uint8)tkOTHER;
        }
        else if (exprᴛ1 == (int32)'`') { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)'"') { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)'\'') { matchᴛ1 = true;
            c = (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql)));
            zSql = zSql + 1;
            while (~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql)) != 0 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql))) != c) {
                zSql = zSql + 1;
            }
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql))) == 0) {
                pending = libc.Uint8FromInt32(c);
                goto incomplete_finish;
            }
            token = (uint8)tkOTHER;
        }
        else if (exprᴛ1 == (int32)'(') { matchᴛ1 = true;
            nParen = nParen + 1;
            token = (uint8)tkOTHER;
        }
        else if (exprᴛ1 == (int32)')') {
            nParen = nParen - 1;
            token = (uint8)tkOTHER;
        }
        else if (!matchᴛ1) { /* default: */
            do {
                if ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql)))]) & (int32)0x46) != 0) {
                    nId = (int32)1;
                    for (;;) {
                        if (!((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql + (uintptr)nId)))]) & (int32)0x46) != 0)) {
                            break;
                        }
                        goto _1;
_1:;
                        nId = nId + 1;
                    }
                    var exprᴛ2 = (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSql)));
                    var matchᴛ2 = false;
                    if (exprᴛ2 == (int32)'c') { matchᴛ2 = true;
                        fallthrough = true;
                    }
                    if (fallthrough || !matchᴛ2 && exprᴛ2 == (int32)'C') { matchᴛ2 = true;
                        if (nId == (int32)6 && Xsqlite3_strnicmp(tls, zSql, __ccgo_ts + 25423, (int32)6) == 0) {
                            token = (uint8)tkCREATE;
                        } else {
                            token = (uint8)tkOTHER;
                        }
                    }
                    else if (exprᴛ2 == (int32)'t') { matchᴛ2 = true;
                        fallthrough = true;
                    }
                    if (fallthrough || !matchᴛ2 && exprᴛ2 == (int32)'T') { matchᴛ2 = true;
                        if (nId == (int32)7 && Xsqlite3_strnicmp(tls, zSql, __ccgo_ts + 22019, (int32)7) == 0) {
                            token = (uint8)tkTRIGGER;
                        } else {
                            if (nId == (int32)4 && Xsqlite3_strnicmp(tls, zSql, __ccgo_ts + 25430, (int32)4) == 0) {
                                token = (uint8)tkTEMP;
                            } else {
                                if (nId == (int32)9 && Xsqlite3_strnicmp(tls, zSql, __ccgo_ts + 25435, (int32)9) == 0) {
                                    token = (uint8)tkTEMP;
                                } else {
                                    token = (uint8)tkOTHER;
                                }
                            }
                        }
                    }
                    else if (exprᴛ2 == (int32)'e') { matchᴛ2 = true;
                        fallthrough = true;
                    }
                    if (fallthrough || !matchᴛ2 && exprᴛ2 == (int32)'E') {
                        if (nId == (int32)3 && Xsqlite3_strnicmp(tls, zSql, __ccgo_ts + 25445, (int32)3) == 0) {
                            token = (uint8)tkEND;
                        } else {
                            if (nId == (int32)7 && Xsqlite3_strnicmp(tls, zSql, __ccgo_ts + 25449, (int32)7) == 0) {
                                token = (uint8)tkEXPLAIN;
                            } else {
                                token = (uint8)tkOTHER;
                            }
                        }
                    }
                    else if (!matchᴛ2) { /* default: */
                        do {
                            token = (uint8)tkOTHER;
                            break;
                        } while (false);
                    }

                    zSql = zSql + (uintptr)(nId - (int32)1);
                } else {
                    token = (uint8)tkOTHER;
                }
                break;
            } while (false);
        }

        state = ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((uintptr)Ꮡ_trans + (uintptr)state * 8 + (uintptr)token));
        zSql = zSql + 1;
    }
    goto incomplete_finish;
incomplete_finish:;
    if (libc.Int32FromUint8(state) == 0 && libc.Int32FromUint8(pending) == 0) {
        return (int64)SQLITE_EMPTY;
    }
    if (libc.Int32FromUint8(state) == (int32)1) {
        nParen = 0;
    }
    return libc.Int64FromUint64(libc.Uint64FromInt32(nParen).Lsh((int64)(libc.Int32FromInt32(32))) | ((uint64)pending).Lsh((int64)(libc.Int32FromInt32(16))) | ((uint64)_statemap[state]).Lsh((int64)(libc.Int32FromInt32(8))) | libc.BoolUint64(libc.Int32FromUint8(state) > (int32)1 || libc.Int32FromUint8(pending) != 0));
}

internal static array<TNanInfName> _aNanInfName;
internal static void initᴛ_aNanInfName() { _aNanInfName = new array<TNanInfName>(5){
    [0] = new(
        Fc1: (int8)'i',
        Fc2: (int8)'I',
        Fn: (int8)3,
        FeType: (int8)JSONB_FLOAT,
        FnRepl: (int8)7,
        FzMatch: __ccgo_ts + 26660,
        FzRepl: __ccgo_ts + 26664
    ),
    [1] = new(
        Fc1: (int8)'i',
        Fc2: (int8)'I',
        Fn: (int8)8,
        FeType: (int8)JSONB_FLOAT,
        FnRepl: (int8)7,
        FzMatch: __ccgo_ts + 26672,
        FzRepl: __ccgo_ts + 26664
    ),
    [2] = new(
        Fc1: (int8)'n',
        Fc2: (int8)'N',
        Fn: (int8)3,
        FnRepl: (int8)4,
        FzMatch: __ccgo_ts + 1723,
        FzRepl: __ccgo_ts + 1718
    ),
    [3] = new(
        Fc1: (int8)'q',
        Fc2: (int8)'Q',
        Fn: (int8)4,
        FnRepl: (int8)4,
        FzMatch: __ccgo_ts + 26681,
        FzRepl: __ccgo_ts + 1718
    ),
    [4] = new(
        Fc1: (int8)'s',
        Fc2: (int8)'S',
        Fn: (int8)4,
        FnRepl: (int8)4,
        FzMatch: __ccgo_ts + 26686,
        FzRepl: __ccgo_ts + 1718
    )
}; }

internal static int32 /*r*/ _compare2pow63(ref libc.TLS tls, uintptr zNum, int32 incr) {
    int32 c = default;
    int32 i = default;
    uintptr pow63 = default;
    _ = c;
    _ = i;
    _ = pow63;
    c = 0;
    pow63 = __ccgo_ts + 1839;
    i = 0;
    for (;;) {
        if (!(c == 0 && i < (int32)18)) {
            break;
        }
        c = ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum + (uintptr)(i * incr)))) - (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(pow63 + (uintptr)i)))) * (int32)10;
        goto _1;
_1:;
        i = i + 1;
    }
    if (c == 0) {
        c = (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum + (uintptr)((int32)18 * incr)))) - (int32)'8';
    }
    return c;
}

internal static int32 /*r*/ _fts5SpecialMatch(ж<libc.TLS> tls, uintptr pTab, uintptr pCsr, uintptr zQuery) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 n = default;
        int32 rc = default;
        uintptr z = default;
        _ = n;
        _ = rc;
        _ = z;
        rc = SQLITE_OK;
        z = zQuery;
        while ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) == (int32)' ') {
            z = z + 1;
        }
        n = 0;
        for (;;) {
            if (!(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)n)) != 0 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)n))) != (int32)' ')) {
                break;
            }
            goto _1;
_1:;
            n = n + 1;
        }
        (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FePlan = (int32)FTS5_PLAN_SPECIAL;
        if (n == (int32)5 && 0 == Xsqlite3_strnicmp(tls, __ccgo_ts + 40110, z, n)) {
            (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FiSpecial = (int64)_sqlite3Fts5IndexReads(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpIndex);
        } else {
            if (n == (int32)2 && 0 == Xsqlite3_strnicmp(tls, __ccgo_ts + 5734, z, n)) {
                (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FiSpecial = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FiCsrId;
            } else {
                (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.Fbase.FzErrMsg = Xsqlite3_mprintf(tls, __ccgo_ts + 40116, libc.VaList(bp + 8, n, z));
                rc = (int32)SQLITE_ERROR;
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _getTempStore(ref libc.TLS tls, uintptr z) {
    int32 r = default;

    if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) >= (int32)'0' && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) <= (int32)'2') {
        return (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) - (int32)'0';
    } else {
        if (_sqlite3StrICmp(ref tls, z, __ccgo_ts + 18105) == 0) {
            return (int32)1;
        } else {
            if (_sqlite3StrICmp(ref tls, z, __ccgo_ts + 19146) == 0) {
                return (int32)2;
            } else {
                return 0;
            }
        }
    }
    return r;
}

internal static int32 /*r*/ _jsonConvertTextToBlob(ж<libc.TLS> tls, uintptr pParse, uintptr pCtx) {
    int32 i = default;
    uintptr zJson = default;
    _ = i;
    _ = zJson;
    zJson = (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FzJson;
    i = _jsonTranslateTextToBlob(tls, pParse, (uint32)0);
    if ((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.Foom != 0) {
        i = -(int32)1;
    }
    if (i > 0) {
        while (_jsonIsSpace[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zJson + (uintptr)i)))] != 0) {
            i = i + 1;
        }
        if (~@unsafe.Pointer.To<int8>(__ccgo_ptr(zJson + (uintptr)i)) != 0) {
            i = i + _json5Whitespace(ref (tls).DerefOrNull(), zJson + (uintptr)i);
            if (~@unsafe.Pointer.To<int8>(__ccgo_ptr(zJson + (uintptr)i)) != 0) {
                if (pCtx != 0) {
                    Xsqlite3_result_error(tls, pCtx, __ccgo_ts + 26645, -(int32)1);
                }
                _jsonParseReset(tls, (@unsafe.Pointer)pParse);
                return (int32)1;
            }
            (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FhasNonstd = (uint8)1;
        }
    }
    if (i <= 0) {
        if (pCtx != (uintptr)0) {
            if ((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.Foom != 0) {
                Xsqlite3_result_error_nomem(tls, pCtx);
            } else {
                Xsqlite3_result_error(tls, pCtx, __ccgo_ts + 26645, -(int32)1);
            }
        }
        _jsonParseReset(tls, (@unsafe.Pointer)pParse);
        return (int32)1;
    }
    return 0;
}

internal static void _ofdInitOnce(ж<libc.TLS> tls) {
    uintptr z = default;
    _ = z;
    if (!(libc.AtomicLoadPInt32((uintptr)Ꮡ_ofdDecided) != 0)) {
        z = libc.Xgetenv(tls, __ccgo_ts + 3819);
        libc.AtomicStorePInt32((uintptr)Ꮡ_ofdEnabled, libc.BoolInt32(z != (uintptr)0 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) != 0 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) != (int32)'0'));
        libc.AtomicStorePInt32((uintptr)Ꮡ_ofdDecided, (int32)1);
    }
}

internal static int32 /*r*/ _parseHhMmSs(ж<libc.TLS> tls, uintptr zDate, uintptr p) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        float64 ms = default;
        float64 rScale = default;
        int32 _ᴛ1 = default;
        int32 _ᴛ2 = default;
        int32 _ᴛ3 = default;
        _ = ms;
        _ = rScale;
        ms = (float64)0D;
        if (_getDigits(ref (tls).DerefOrNull(), zDate, __ccgo_ts + 1224, libc.VaList(bp + 24, bp, bp + 4)) != (int32)2) {
            r = (int32)1; goto ᒐdone;
        }
        zDate = zDate + (uintptr)5;
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDate))) == (int32)':') {
            zDate = zDate + 1;
            if (_getDigits(ref (tls).DerefOrNull(), zDate, __ccgo_ts + 1232, libc.VaList(bp + 24, bp + 8)) != (int32)1) {
                r = (int32)1; goto ᒐdone;
            }
            zDate = zDate + (uintptr)2;
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDate))) == (int32)'.' && (libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDate + 1)))]) & (int32)0x04) != 0) {
                rScale = (float64)1D;
                zDate = zDate + 1;
                while ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDate)))]) & (int32)0x04) != 0) {
                    ms = (float64)(ms * (float64)10D) + (float64)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDate))) - libc.Float64FromUint8((uint8)'0');
                    rScale = (float64)(rScale * (float64)10D);
                    zDate = zDate + 1;
                }
                ms = ms / rScale;
                if (ms > (float64)0.999D) {
                    ms = (float64)0.999D;
                }
            }
        } else {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8))).Value = 0;
        }
        if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) == (int32)24 && (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 4)) != 0 || ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8)) != 0 || ms != (float64)0D)) {
            r = (int32)1; goto ᒐdone;
        }
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidJD = 0;
        libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(0), 0, 0x1);
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidHMS = (int8)1;
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fh = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp));
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fm = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 4));
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fs = (float64)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8))) + ms;
        if (_parseTimezone(tls, zDate, p) != 0) {
            r = (int32)1; goto ᒐdone;
        }
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _parseTimezone(ж<libc.TLS> tls, uintptr zDate, uintptr p) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 c = default;
        int32 sgn = default;
        int32 _ᴛ1 = default;
        int32 _ᴛ2 = default;
        _ = c;
        _ = sgn;
        sgn = 0;
        while ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDate)))]) & (int32)0x01) != 0) {
            zDate = zDate + 1;
        }
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Ftz = 0;
        c = (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDate)));
        if (c == (int32)'-') {
            sgn = -(int32)1;
        } else {
            if (c == (int32)'+') {
                sgn = +libc.Int32FromInt32(1);
            } else {
                if (c == (int32)'Z' || c == (int32)'z') {
                    zDate = zDate + 1;
                    libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(0), 4, 0x10);
                    libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(1), 3, 0x8);
                    goto zulu_time;
                } else {
                    r = libc.BoolInt32(c != 0); goto ᒐdone;
                }
            }
        }
        zDate = zDate + 1;
        if (_getDigits(ref (tls).DerefOrNull(), zDate, __ccgo_ts + 1216, libc.VaList(bp + 16, bp, bp + 4)) != (int32)2) {
            r = (int32)1; goto ᒐdone;
        }
        zDate = zDate + (uintptr)5;
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Ftz = sgn * (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 4)) + ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) * (int32)60);
        if ((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Ftz == 0) {
            libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(0), 4, 0x10);
            libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(1), 3, 0x8);
        }
        goto zulu_time;
zulu_time:;
        while ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDate)))]) & (int32)0x01) != 0) {
            zDate = zDate + 1;
        }
        r = libc.BoolInt32((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDate))) != 0);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _parseYyyyMmDd(ж<libc.TLS> tls, uintptr zDate, uintptr p) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        int32 mxD = default;
        int32 neg = default;
        int32 v1 = default;
        int32 _ᴛ1 = default;
        int32 _ᴛ2 = default;
        int32 _ᴛ3 = default;
        _ = mxD;
        _ = neg;
        _ = v1;
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDate))) == (int32)'-') {
            zDate = zDate + 1;
            neg = (int32)1;
        } else {
            neg = 0;
        }
        if (_getDigits(ref (tls).DerefOrNull(), zDate, __ccgo_ts + 1236, libc.VaList(bp + 24, bp, bp + 4, bp + 8)) != (int32)3) {
            r = (int32)1; goto ᒐdone;
        }
        zDate = zDate + (uintptr)10;
        while ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDate)))]) & (int32)0x01) != 0 || (int32)'T' == libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zDate)))) {
            zDate = zDate + 1;
        }
        if (_parseHhMmSs(tls, zDate, p) == 0) {
            if ((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fh == (int32)24) {
                mxD = _daysOfMonth(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)), ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 4)));
                (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Fh = 0;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8))).Value = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8)) + 1;
                if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8)) > mxD) {
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8))).Value = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8)) - mxD;
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 4))).Value = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 4)) + 1;
                    if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 4)) > (int32)12) {
                        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 4))).Value = (int32)1;
                        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) + 1;
                    }
                }
            }
        } else {
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDate))) == 0) {
                (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidHMS = 0;
            } else {
                r = (int32)1; goto ᒐdone;
            }
        }
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidJD = 0;
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidYMD = (int8)1;
        if (neg != 0) {
            v1 = -~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp));
        } else {
            v1 = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp));
        }
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FY = v1;
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FM = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 4));
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FD = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8));
        _computeFloor(ref (tls).DerefOrNull(), p);
        if ((@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.Ftz != 0) {
            _computeJD(tls, p);
        }
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _rbuCreateVfs(ж<libc.TLS> tls, uintptr p) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(96);
        defer(tls.Free, (nint)96, ref ᒐ);
        uintptr pVfs = default;
        int32 _ᴛ1 = default;
        array<int8> _ᴛ2 = new(64);
        _ = pVfs;
        Xsqlite3_randomness(tls, (int32)4, bp);
        Xsqlite3_snprintf(tls, (int32)64, bp + 4, __ccgo_ts + 34551, libc.VaList(bp + 80, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))));
        (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = Xsqlite3rbu_create_vfs(tls, bp + 4, (uintptr)0);
        if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
            pVfs = Xsqlite3_vfs_find(tls, bp + 4);
            (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FzVfsName = (@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)pVfs)).Value.FzName;
            (@unsafe.Pointer.To<Trbu_vfs>((@unsafe.Pointer)pVfs)).Value.FpRbu = p;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _soundexFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 code = default;
        int32 i = default;
        int32 j = default;
        int32 v3 = default;
        Tu8 prevcode = default;
        uintptr zIn = default;
        array<int8> _ᴛ1 = new(8);
        _ = code;
        _ = i;
        _ = j;
        _ = prevcode;
        _ = zIn;
        _ = v3;
        zIn = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        if (zIn == (uintptr)0) {
            zIn = __ccgo_ts + 1732;
        }
        i = 0;
        for (;;) {
            if (!(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zIn + (uintptr)i)) != 0 && !((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zIn + (uintptr)i))]) & libc.Int32FromInt32(0x02)) != 0))) {
                break;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zIn + (uintptr)i)) != 0) {
            prevcode = _iCode[(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zIn + (uintptr)i))) & (int32)0x7f)];
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp), 0, 8) = (int8)(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zIn + (uintptr)i))) & ~(libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zIn + (uintptr)i))]) & libc.Int32FromInt32(0x20)));
            j = (int32)1;
            for (;;) {
                if (!(j < (int32)4 && ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zIn + (uintptr)i)) != 0)) {
                    break;
                }
                code = libc.Int32FromUint8(_iCode[(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zIn + (uintptr)i))) & (int32)0x7f)]);
                if (code > 0) {
                    if (code != libc.Int32FromUint8(prevcode)) {
                        prevcode = libc.Uint8FromInt32(code);
                        v3 = j;
                        j = j + 1;
                        @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp), v3, 8) = (int8)(code + (int32)'0');
                    }
                } else {
                    prevcode = (uint8)0;
                }
                goto _2;
_2:;
                i = i + 1;
            }
            while (j < (int32)4) {
                v3 = j;
                j = j + 1;
                @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp), v3, 8) = (int8)'0';
            }
            @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp), j, 8) = 0;
            Xsqlite3_result_text(tls, context, bp, (int32)4, (uintptr)(-libc.Int32FromInt32(1)));
        } else {
            Xsqlite3_result_text(tls, context, __ccgo_ts + 16707, (int32)4, libc.UintptrFromInt32(0));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3Fts5ParseSetDistance(ж<libc.TLS> tls, uintptr pParse, uintptr pNear, uintptr p) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int8 c = default;
        int32 i = default;
        int32 nNear = default;
        _ = c;
        _ = i;
        _ = nNear;
        if (pNear != 0) {
            nNear = 0;
            if ((@unsafe.Pointer.To<TFts5Token>((@unsafe.Pointer)p)).Value.Fn != 0) {
                i = 0;
                for (;;) {
                    if (!(i < (@unsafe.Pointer.To<TFts5Token>((@unsafe.Pointer)p)).Value.Fn)) {
                        break;
                    }
                    c = ~@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Token>((@unsafe.Pointer)p)).Value.Fp + (uintptr)i));
                    if ((int32)c < (int32)'0' || (int32)c > (int32)'9') {
                        _sqlite3Fts5ParseError(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 38901, libc.VaList(bp + 8, (@unsafe.Pointer.To<TFts5Token>((@unsafe.Pointer)p)).Value.Fn, (@unsafe.Pointer.To<TFts5Token>((@unsafe.Pointer)p)).Value.Fp));
                        return;
                    }
                    if (nNear < (int32)214748363) {
                        nNear = nNear * (int32)10 + ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Token>((@unsafe.Pointer)p)).Value.Fp + (uintptr)i))) - (int32)'0');
                    }
                    goto _1;
_1:;
                    i = i + 1;
                }
            } else {
                nNear = (int32)FTS5_DEFAULT_NEARDIST;
            }
            (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FnNear = nNear;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3QuoteValue(ж<libc.TLS> tls, uintptr pStr, uintptr pValue, int32 bEscape) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 i = default;
        Ti64 nBlob = default;
        uintptr zArg = default;
        uintptr zBlob = default;
        uintptr zText = default;
        uintptr v2 = default;
        _ = i;
        _ = nBlob;
        _ = zArg;
        _ = zBlob;
        _ = zText;
        _ = v2;
        switch (Xsqlite3_value_type(tls, pValue)) {
        case var ᴛ when ᴛ == (int32)SQLITE_FLOAT: {
            Xsqlite3_str_appendf(tls, pStr, __ccgo_ts + 16672, libc.VaList(bp + 8, Xsqlite3_value_double(tls, pValue)));
            break;
        }
        case var ᴛ when ᴛ == (int32)SQLITE_INTEGER: {
            Xsqlite3_str_appendf(tls, pStr, __ccgo_ts + 1493, libc.VaList(bp + 8, Xsqlite3_value_int64(tls, pValue)));
            break;
        }
        case var ᴛ when ᴛ == (int32)SQLITE_BLOB: {
            zBlob = Xsqlite3_value_blob(tls, pValue);
            nBlob = (int64)Xsqlite3_value_bytes(tls, pValue);
            _sqlite3StrAccumEnlarge(tls, pStr, nBlob * (int64)2 + (int64)4);
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)pStr)).Value.FaccError) == 0) {
                zText = (@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)pStr)).Value.FzText;
                i = 0;
                for (;;) {
                    if (!((int64)i < nBlob)) {
                        break;
                    }
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(zText + (uintptr)(i * (int32)2 + (int32)2)))).Value = _hexdigits[((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zBlob + (uintptr)i))) >> (int)((int32)4) & (int32)0x0F)];
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(zText + (uintptr)(i * (int32)2 + (int32)3)))).Value = _hexdigits[((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zBlob + (uintptr)i))) & (int32)0x0F)];
                    goto _1;
_1:;
                    i = i + 1;
                }
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(zText + (uintptr)(nBlob * (int64)2 + (int64)2)))).Value = (int8)'\'';
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(zText + (uintptr)(nBlob * (int64)2 + (int64)3)))).Value = (int8)'\u0000';
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(zText))).Value = (int8)'X';
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(zText + 1))).Value = (int8)'\'';
                (@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)pStr)).Value.FnChar = libc.Uint32FromInt64(nBlob * (int64)2 + (int64)3);
            }
            break;
        }
        case var ᴛ when ᴛ == (int32)SQLITE_TEXT: {
            zArg = Xsqlite3_value_text(tls, pValue);
            if (bEscape != 0) {
                v2 = __ccgo_ts + 16680;
            } else {
                v2 = __ccgo_ts + 12043;
            }
            Xsqlite3_str_appendf(tls, pStr, v2, libc.VaList(bp + 8, zArg));
            break;
        }
        default: {
            Xsqlite3_str_append(tls, pStr, __ccgo_ts + 1746, (int32)4);
            break;
            break;
        }}

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _unixGetTempname(ж<libc.TLS> tls, int32 nBuf, uintptr zBuf) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        int32 iLimit = default;
        int32 rc = default;
        int32 v1 = default;
        uintptr zDir = default;
        bool v2 = default;
        Tu64 _ᴛ1 = default;
        _ = iLimit;
        _ = rc;
        _ = zDir;
        _ = v1;
        _ = v2;
        iLimit = 0;
        rc = SQLITE_OK;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zBuf))).Value = 0;
        Xsqlite3_mutex_enter(tls, _sqlite3MutexAlloc(tls, (int32)SQLITE_MUTEX_STATIC_VFS1));
        zDir = _unixTempFileDir(tls);
        if (zDir == (uintptr)0) {
            rc = libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(25).Lsh((int64)(libc.Int32FromInt32(8)));
        } else {
            for (var cond = true; cond; cond = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32>>(_aSyscall[(int32)2].FpCurrent)(tls, zBuf, 0) == 0) {
                Xsqlite3_randomness(tls, (int32)8, bp);
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(zBuf + (uintptr)(nBuf - (int32)2)))).Value = 0;
                Xsqlite3_snprintf(tls, nBuf, zBuf, __ccgo_ts + 4083, libc.VaList(bp + 16, zDir, ~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)), (nint)0));
                v2 = (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zBuf + (uintptr)(nBuf - (int32)2)))) != 0; if (!v2) {
                    v1 = iLimit;
                    iLimit = iLimit + 1;
                }
                if (v2 || v1 > (int32)10) {
                    rc = (int32)SQLITE_ERROR;
                    break;
                }
            }
        }
        Xsqlite3_mutex_leave(tls, _sqlite3MutexAlloc(tls, (int32)SQLITE_MUTEX_STATIC_VFS1));
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _yy_syntax_error(ж<libc.TLS> tls, uintptr yypParser, int32 yymajor, TToken _yyminor) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        (@unsafe.Pointer.To<TToken>((@unsafe.Pointer)bp)).Value = _yyminor;
        uintptr pParse = default;
        _ = pParse;
        pParse = (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)yypParser)).Value.FpParse;
        _ = yymajor;
        if (~@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TToken>(__ccgo_ptr(bp))).Value.Fz)) != 0) {
            _parserSyntaxError(tls, pParse, bp);
        } else {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 25370, 0);
        }
        (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)yypParser)).Value.FpParse = pParse;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end sqlite3_package
