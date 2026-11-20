namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static void Xsqlite3_str_truncate(ж<libc.TLS> tls, uintptr p, int32 N) {
    if (p != (uintptr)0 && N >= 0 && libc.Uint32FromInt32(N) < (@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnChar) {
        (@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnChar = libc.Uint32FromInt32(N);
        (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FzText + (uintptr)(@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnChar))).Value = 0;
    }
}

internal static void _appendAllPathElements(ж<libc.TLS> tls, uintptr pPath, uintptr zPath) {
    int32 i = default;
    int32 j = default;
    int32 v1 = default;
    _ = i;
    _ = j;
    _ = v1;
    i = 0;
    j = 0;
    for (;;) {
        while (~@unsafe.Pointer.To<int8>(__ccgo_ptr(zPath + (uintptr)i)) != 0 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zPath + (uintptr)i))) != (int32)'/') {
            i = i + 1;
        }
        if (i > j) {
            _appendOnePathElement(tls, pPath, zPath + (uintptr)j, i - j);
        }
        j = i + (int32)1;
        goto _2;
_2:;
        v1 = i;
        i = i + 1;
        if (!(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zPath + (uintptr)v1)) != 0)) {
            break;
        }
    }
}

internal static Tu8 /*r*/ _binaryCompareP5(ж<libc.TLS> tls, uintptr pExpr1, uintptr pExpr2, int32 jumpIfNull) {
    Tu8 aff = default;
    _ = aff;
    aff = libc.Uint8FromInt8(_sqlite3ExprAffinity(tls, pExpr2));
    aff = libc.Uint8FromInt32(libc.Int32FromUint8(libc.Uint8FromInt8(_sqlite3CompareAffinity(tls, pExpr1, libc.Int8FromUint8(aff)))) | libc.Int32FromUint8(libc.Uint8FromInt32(jumpIfNull)));
    return aff;
}

internal static void _columnDefaultUncommonCase(ж<libc.TLS> tls, uintptr v, uintptr pTab, uintptr pCol) {
    Tu8 enc = default;
    uintptr pParse = default;
    ref var pValue = ref heap(new uintptr(), out var ᏑpValue);
    _ = enc;
    _ = pParse;
    _ = pValue;
    pValue = (uintptr)0;
    enc = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)_sqlite3VdbeDb(ref (tls).DerefOrNull(), v))).Value.Fenc;
    pParse = _sqlite3VdbeParser(ref (tls).DerefOrNull(), v);
    _sqlite3ValueFromExpr(tls, _sqlite3VdbeDb(ref (tls).DerefOrNull(), v), _sqlite3ColumnExprAuth(tls, pTab, pCol, pParse), enc, libc.Uint8FromInt8((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.Faffinity), @unsafe.Pointer.FromBox(ᏑpValue));
    if (pValue != 0) {
        _sqlite3VdbeAppendP4(tls, v, pValue, -(int32)11);
    }
}

internal static void _fts5AsciiAddExceptions(ref libc.TLS tls, uintptr p, uintptr zArg, int32 bTokenChars) {
    int32 i = default;
    _ = i;
    i = 0;
    for (;;) {
        if (!(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zArg + (uintptr)i)) != 0)) {
            break;
        }
        if (((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zArg + (uintptr)i))) & (int32)0x80) == 0) {
            (@unsafe.Pointer.To<uint8>(__ccgo_ptr(p + (uintptr)(int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zArg + (uintptr)i)))))).Value = libc.Uint8FromInt32(bTokenChars);
        }
        goto _1;
_1:;
        i = i + 1;
    }
}

internal static int32 /*r*/ _fts5AsciiTokenize(ж<libc.TLS> tls, uintptr pTokenizer, uintptr pCtx, int32 iUnused, uintptr pText, int32 nText, uintptr __ccgo_fp_xToken) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        uintptr a = default;
        uintptr p = default;
        uintptr pFold = default;
        int32 ie = default;
        int32 @is = default;
        int32 nByte = default;
        int32 nFold = default;
        int32 rc = default;
        array<int8> _ᴛ1 = new(64);
        _ = a;
        _ = ie;
        _ = @is;
        _ = nByte;
        _ = nFold;
        _ = p;
        _ = pFold;
        _ = rc;
        p = pTokenizer;
        rc = SQLITE_OK;
        @is = 0;
        nFold = (int32)64;
        pFold = bp;
        a = p;
        _ = iUnused;
        while (@is < nText && rc == SQLITE_OK) {
            while (@is < nText && (((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(pText + (uintptr)@is))) & (int32)0x80) == 0 && libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(a + (uintptr)(int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(pText + (uintptr)@is)))))) == 0)) {
                @is = @is + 1;
            }
            if (@is == nText) {
                break;
            }
            ie = @is + (int32)1;
            while (ie < nText && (((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(pText + (uintptr)ie))) & (int32)0x80) != 0 || ~@unsafe.Pointer.To<uint8>(__ccgo_ptr(a + (uintptr)(int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(pText + (uintptr)ie))))) != 0)) {
                ie = ie + 1;
            }
            nByte = ie - @is;
            if (nByte > nFold) {
                if (pFold != bp) {
                    Xsqlite3_free(tls, pFold);
                }
                pFold = Xsqlite3_malloc64(tls, libc.Uint64FromInt64((int64)nByte * (int64)2));
                if (pFold == (uintptr)0) {
                    rc = (int32)SQLITE_NOMEM;
                    break;
                }
                nFold = nByte * (int32)2;
            }
            _asciiFold(ref (tls).DerefOrNull(), pFold, pText + (uintptr)@is, nByte);
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32, int32, int32, int32>>(__ccgo_fp_xToken)(tls, pCtx, 0, pFold, nByte, @is, ie);
            @is = ie + (int32)1;
        }
        if (pFold != bp) {
            Xsqlite3_free(tls, pFold);
        }
        if (rc == (int32)SQLITE_DONE) {
            rc = SQLITE_OK;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _fts5IndexCharlen(ref libc.TLS tls, uintptr pIn, int32 nIn) {
    int32 i = default;
    int32 nChar = default;
    int32 v1 = default;
    _ = i;
    _ = nChar;
    _ = v1;
    nChar = 0;
    i = 0;
    while (i < nIn) {
        v1 = i;
        i = i + 1;
        if (libc.Int32FromUint8(libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(pIn + (uintptr)v1)))) >= (int32)0xc0) {
            while (i < nIn && ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(pIn + (uintptr)i))) & (int32)0xc0) == (int32)0x80) {
                i = i + 1;
            }
        }
        nChar = nChar + 1;
    }
    return nChar;
}

internal static int32 /*r1*/ _geopolyParseNumber(ж<libc.TLS> tls, @unsafe.Pointer p, uintptr pVal) {
    int8 c = default;
    int32 j = default;
    int32 seenDP = default;
    int32 seenE = default;
    int32 v2 = default;
    ref var r = ref heap(new float64(), out var Ꮡr);
    uintptr z = default;
    _ = c;
    _ = j;
    _ = r;
    _ = seenDP;
    _ = seenE;
    _ = z;
    _ = v2;
    c = _geopolySkipSpace(ref (tls).DerefOrNull(), p);
    z = ~@unsafe.Pointer.To<uintptr>(p);
    j = 0;
    seenDP = 0;
    seenE = 0;
    if ((int32)c == (int32)'-') {
        j = (int32)1;
        c = libc.Int8FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)j)));
    }
    if ((int32)c == (int32)'0' && libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)(j + (int32)1)))) >= (int32)'0' && libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)(j + (int32)1)))) <= (int32)'9') {
        return 0;
    }
    for (;;) {
        c = libc.Int8FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)j)));
        if ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(c)]) & (int32)0x04) != 0) {
            goto _1;
        }
        if ((int32)c == (int32)'.') {
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)(j - (int32)1)))) == (int32)'-') {
                return 0;
            }
            if (seenDP != 0) {
                return 0;
            }
            seenDP = (int32)1;
            goto _1;
        }
        if ((int32)c == (int32)'e' || (int32)c == (int32)'E') {
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)(j - (int32)1)))) < (int32)'0') {
                return 0;
            }
            if (seenE != 0) {
                return -(int32)1;
            }
            v2 = libc.Int32FromInt32(1);
            seenE = v2;
            seenDP = v2;
            c = libc.Int8FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)(j + (int32)1))));
            if ((int32)c == (int32)'+' || (int32)c == (int32)'-') {
                j = j + 1;
                c = libc.Int8FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)(j + (int32)1))));
            }
            if ((int32)c < (int32)'0' || (int32)c > (int32)'9') {
                return 0;
            }
            goto _1;
        }
        break;
        goto _1;
_1:;
        j = j + 1;
    }
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)(j - (int32)1)))) < (int32)'0') {
        return 0;
    }
    if (pVal != 0) {
        _sqlite3AtoF(tls, ~@unsafe.Pointer.To<uintptr>(p), @unsafe.Pointer.FromPinnedBox(Ꮡr));
        (@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(pVal))).Value = (float32)r;
    }
    (@unsafe.Pointer.To<uintptr>(p)).Value += (uintptr)j;
    return (int32)1;
}

internal static int8 /*r*/ _geopolySkipSpace(ref libc.TLS tls, @unsafe.Pointer p) {
    while (_geopolyIsSpace[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(p)))] != 0) {
        (@unsafe.Pointer.To<uintptr>(p)).Value = ~@unsafe.Pointer.To<uintptr>(p) + 1;
    }
    return libc.Int8FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(p))));
}

internal static int32 /*r*/ _getDigits(ref libc.TLS tls, uintptr zDate, uintptr zFormat, uintptr va) {
    int8 N = default;
    int8 min = default;
    int8 nextC = default;
    int8 v1 = default;
    ref var ap = ref heap(new Tva_list(), out var Ꮡap);
    int32 cnt = default;
    int32 val = default;
    Tu16 max = default;
    _ = N;
    _ = ap;
    _ = cnt;
    _ = max;
    _ = min;
    _ = nextC;
    _ = val;
    _ = v1;
    cnt = 0;
    ap = va;
    for (var cond = true; cond; cond = nextC != 0) {
        N = (int8)((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zFormat))) - (int32)'0');
        min = (int8)((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zFormat + 1))) - (int32)'0');
        val = 0;
        max = _aMx[(int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zFormat + 2))) - (int32)'a'];
        nextC = ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zFormat + 3));
        val = 0;
        for (;;) {
            v1 = N;
            N = (int8)(N - 1);
            if (!(v1 != 0)) {
                break;
            }
            if (!((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDate)))]) & libc.Int32FromInt32(0x04)) != 0)) {
                goto end_getDigits;
            }
            val = val * (int32)10 + (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDate))) - (int32)'0';
            zDate = zDate + 1;
        }
        if (val < (int32)min || val > libc.Int32FromUint16(max) || (int32)nextC != 0 && (int32)nextC != (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDate)))) {
            goto end_getDigits;
        }
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.VaUintptr(Ꮡap)))).Value = val;
        zDate = zDate + 1;
        cnt = cnt + 1;
        zFormat = zFormat + (uintptr)4;
    }
    goto end_getDigits;
end_getDigits:;
    _ = ap;
    return cnt;
}

internal static void _identPut(ref libc.TLS tls, uintptr z, @unsafe.Pointer pIdx, uintptr zSignedIdent) {
    int32 i = default;
    int32 j = default;
    int32 needQuote = default;
    int32 v2 = default;
    uintptr zIdent = default;
    _ = i;
    _ = j;
    _ = needQuote;
    _ = zIdent;
    _ = v2;
    zIdent = zSignedIdent;
    i = ~@unsafe.Pointer.To<int32>(pIdx);
    j = 0;
    for (;;) {
        if (!(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zIdent + (uintptr)j)) != 0)) {
            break;
        }
        if (!((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zIdent + (uintptr)j))]) & libc.Int32FromInt32(0x06)) != 0) && libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zIdent + (uintptr)j))) != (int32)'_') {
            break;
        }
        goto _1;
_1:;
        j = j + 1;
    }
    needQuote = libc.BoolInt32((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zIdent))]) & (int32)0x04) != 0 || _sqlite3KeywordCode(ref tls, zIdent, j) != (int32)TK_ID || libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zIdent + (uintptr)j))) != 0 || j == 0);
    if (needQuote != 0) {
        v2 = i;
        i = i + 1;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)v2))).Value = (int8)'"';
    }
    j = 0;
    for (;;) {
        if (!(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zIdent + (uintptr)j)) != 0)) {
            break;
        }
        v2 = i;
        i = i + 1;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)v2))).Value = libc.Int8FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zIdent + (uintptr)j)));
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zIdent + (uintptr)j))) == (int32)'"') {
            v2 = i;
            i = i + 1;
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)v2))).Value = (int8)'"';
        }
        goto _3;
_3:;
        j = j + 1;
    }
    if (needQuote != 0) {
        v2 = i;
        i = i + 1;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)v2))).Value = (int8)'"';
    }
    (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i))).Value = 0;
    (@unsafe.Pointer.To<int32>(pIdx)).Value = i;
}

internal static int32 /*r*/ _isNHex(ref libc.TLS tls, uintptr z, int32 N, @unsafe.Pointer pVal) {
    int32 i = default;
    Tu32 v = default;
    _ = i;
    _ = v;
    v = (uint32)0;
    i = 0;
    for (;;) {
        if (!(i < N)) {
            break;
        }
        if (!((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i)))]) & libc.Int32FromInt32(0x08)) != 0)) {
            return 0;
        }
        v = (v << (int)(libc.Int32FromInt32(4))) + (uint32)_sqlite3HexToInt(ref tls, (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i))));
        goto _1;
_1:;
        i = i + 1;
    }
    (@unsafe.Pointer.To<Tu32>(pVal)).Value = v;
    return (int32)1;
}

internal static int32 /*r*/ _json5Whitespace(ref libc.TLS tls, uintptr zIn) {
    int32 r = default;

    int8 c = default;
    int8 v3 = default;
    Tu8 c1 = default;
    int32 j = default;
    int32 j1 = default;
    int32 n = default;
    uintptr z = default;
    _ = c;
    _ = c1;
    _ = j;
    _ = j1;
    _ = n;
    _ = z;
    _ = v3;
    n = 0;
    z = zIn;
    while ((int32)1 != 0) {
        var exprᴛ1 = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)n)));
        var matchᴛ1 = false;
        if (exprᴛ1 == (int32)0x09) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)0x0a) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)0x0b) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)0x0c) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)0x0d) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)0x20) { matchᴛ1 = true;
            n = n + 1;
        }
        else if (exprᴛ1 == (int32)'/') { matchᴛ1 = true;
            do {
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(n + (int32)1)))) == (int32)'*' && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(n + (int32)2)))) != 0) {
                    j = n + (int32)3;
                    for (;;) {
                        if (!(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) != (int32)'/' || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(j - (int32)1)))) != (int32)'*')) {
                            break;
                        }
                        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) == 0) {
                            goto whitespace_done;
                        }
                        goto _1;
_1:;
                        j = j + 1;
                    }
                    n = j + (int32)1;
                    break;
                } else {
                    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(n + (int32)1)))) == (int32)'/') {
                        j1 = n + (int32)2;
                        for (;;) {
                            v3 = libc.Int8FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j1)));
                            c = v3;
                            if (!((int32)v3 != 0)) {
                                break;
                            }
                            if ((int32)c == (int32)'\n' || (int32)c == (int32)'\r') {
                                break;
                            }
                            if ((int32)0xe2 == libc.Int32FromUint8(libc.Uint8FromInt8(c)) && (int32)0x80 == libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(j1 + (int32)1)))) && ((int32)0xa8 == libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(j1 + (int32)2)))) || (int32)0xa9 == libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(j1 + (int32)2)))))) {
                                j1 = j1 + (int32)2;
                                break;
                            }
                            goto _2;
_2:;
                            j1 = j1 + 1;
                        }
                        n = j1;
                        if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)n)) != 0) {
                            n = n + 1;
                        }
                        break;
                    }
                }
                goto whitespace_done;
            } while (false);
        }
        else if (exprᴛ1 == (int32)0xc2) { matchᴛ1 = true;
            do {
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(n + (int32)1)))) == (int32)0xa0) {
                    n = n + (int32)2;
                    break;
                }
                goto whitespace_done;
            } while (false);
        }
        else if (exprᴛ1 == (int32)0xe1) { matchᴛ1 = true;
            do {
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(n + (int32)1)))) == (int32)0x9a && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(n + (int32)2)))) == (int32)0x80) {
                    n = n + (int32)3;
                    break;
                }
                goto whitespace_done;
            } while (false);
        }
        else if (exprᴛ1 == (int32)0xe2) { matchᴛ1 = true;
            do {
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(n + (int32)1)))) == (int32)0x80) {
                    c1 = ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(n + (int32)2)));
                    if (libc.Int32FromUint8(c1) < (int32)0x80) {
                        goto whitespace_done;
                    }
                    if (libc.Int32FromUint8(c1) <= (int32)0x8a || libc.Int32FromUint8(c1) == (int32)0xa8 || libc.Int32FromUint8(c1) == (int32)0xa9 || libc.Int32FromUint8(c1) == (int32)0xaf) {
                        n = n + (int32)3;
                        break;
                    }
                } else {
                    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(n + (int32)1)))) == (int32)0x81 && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(n + (int32)2)))) == (int32)0x9f) {
                        n = n + (int32)3;
                        break;
                    }
                }
                goto whitespace_done;
            } while (false);
        }
        else if (exprᴛ1 == (int32)0xe3) { matchᴛ1 = true;
            do {
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(n + (int32)1)))) == (int32)0x80 && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(n + (int32)2)))) == (int32)0x80) {
                    n = n + (int32)3;
                    break;
                }
                goto whitespace_done;
            } while (false);
        }
        else if (exprᴛ1 == (int32)0xef) {
            do {
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(n + (int32)1)))) == (int32)0xbb && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(n + (int32)2)))) == (int32)0xbf) {
                    n = n + (int32)3;
                    break;
                }
                goto whitespace_done;
            } while (false);
        }
        else if (!matchᴛ1) { /* default: */
            goto whitespace_done;
        }

    }
    goto whitespace_done;
whitespace_done:;
    return n;
    return r;
}

internal static int32 /*r*/ _jsonAllAlphanum(ref libc.TLS tls, uintptr z, int32 n) {
    int32 i = default;
    _ = i;
    i = 0;
    for (;;) {
        if (!(i < n && ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i)))]) & (int32)0x06) != 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i))) == (int32)'_'))) {
            break;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return libc.BoolInt32(i == n);
}

internal static Tu32 /*r*/ _jsonBytesToBypass(ref libc.TLS tls, uintptr z, Tu32 n) {
    Tu32 i = default;
    _ = i;
    i = (uint32)0;
    while (i + (uint32)1 < n) {
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i))) != (int32)'\\') {
            return i;
        }
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(i + (uint32)1)))) == (int32)'\n') {
            i = i + (uint32)2;
            continue;
        }
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(i + (uint32)1)))) == (int32)'\r') {
            if (i + (uint32)2 < n && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(i + (uint32)2)))) == (int32)'\n') {
                i = i + (uint32)3;
            } else {
                i = i + (uint32)2;
            }
            continue;
        }
        if ((int32)0xe2 == libc.Int32FromUint8(libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(i + (uint32)1))))) && i + (uint32)3 < n && (int32)0x80 == libc.Int32FromUint8(libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(i + (uint32)2))))) && ((int32)0xa8 == libc.Int32FromUint8(libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(i + (uint32)3))))) || (int32)0xa9 == libc.Int32FromUint8(libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(i + (uint32)3))))))) {
            i = i + (uint32)4;
            continue;
        }
        break;
    }
    return i;
}

internal static Tu32 /*r*/ _jsonHexToInt4(ref libc.TLS tls, uintptr z) {
    Tu32 v = default;
    _ = v;
    v = libc.Uint32FromInt32((libc.Int32FromUint8(_jsonHexToInt(ref tls, (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))))) << (int)((int32)12)) + (libc.Int32FromUint8(_jsonHexToInt(ref tls, (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + 1))))) << (int)((int32)8)) + (libc.Int32FromUint8(_jsonHexToInt(ref tls, (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + 2))))) << (int)((int32)4)) + libc.Int32FromUint8(_jsonHexToInt(ref tls, (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + 3))))));
    return v;
}

internal static int32 /*r*/ _jsonIs2Hex(ref libc.TLS tls, uintptr z) {
    return libc.BoolInt32((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z)))]) & (int32)0x08) != 0 && (libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + 1)))]) & (int32)0x08) != 0);
}

internal static int32 /*r*/ _jsonLabelCompareEscaped(ref libc.TLS tls, uintptr zLeft, Tu32 nLeft, int32 rawLeft, uintptr zRight, Tu32 nRight, int32 rawRight) {
    int32 r = default;

    ref var cLeft = ref heap(new Tu32(), out var ᏑcLeft);
    ref var cRight = ref heap(new Tu32(), out var ᏑcRight);
    Tu32 n = default;
    Tu32 n1 = default;
    int32 sz = default;
    int32 sz1 = default;
    _ = cLeft;
    _ = cRight;
    _ = n;
    _ = n1;
    _ = sz;
    _ = sz1;
    while ((int32)1 != 0) {
        if (nLeft == (uint32)0) {
            cLeft = (uint32)0;
        } else {
            if (rawLeft != 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zLeft))) != (int32)'\\') {
                cLeft = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zLeft)));
                if (cLeft >= (uint32)0xc0) {
                    sz = _sqlite3Utf8ReadLimited(ref tls, zLeft, libc.Int32FromUint32(nLeft), @unsafe.Pointer.FromPinnedBox(ᏑcLeft));
                    zLeft = zLeft + (uintptr)sz;
                    nLeft = nLeft - libc.Uint32FromInt32(sz);
                } else {
                    zLeft = zLeft + 1;
                    nLeft = nLeft - 1;
                }
            } else {
                n = _jsonUnescapeOneChar(ref tls, zLeft, nLeft, @unsafe.Pointer.FromPinnedBox(ᏑcLeft));
                zLeft = zLeft + (uintptr)n;
                nLeft = nLeft - n;
            }
        }
        if (nRight == (uint32)0) {
            cRight = (uint32)0;
        } else {
            if (rawRight != 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zRight))) != (int32)'\\') {
                cRight = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zRight)));
                if (cRight >= (uint32)0xc0) {
                    sz1 = _sqlite3Utf8ReadLimited(ref tls, zRight, libc.Int32FromUint32(nRight), @unsafe.Pointer.FromPinnedBox(ᏑcRight));
                    zRight = zRight + (uintptr)sz1;
                    nRight = nRight - libc.Uint32FromInt32(sz1);
                } else {
                    zRight = zRight + 1;
                    nRight = nRight - 1;
                }
            } else {
                n1 = _jsonUnescapeOneChar(ref tls, zRight, nRight, @unsafe.Pointer.FromPinnedBox(ᏑcRight));
                zRight = zRight + (uintptr)n1;
                nRight = nRight - n1;
            }
        }
        if (cLeft != cRight) {
            return 0;
        }
        if (cLeft == (uint32)0) {
            return (int32)1;
        }
    }
    return r;
}

internal static int32 /*r*/ _jsonStringTerminate(ж<libc.TLS> tls, uintptr p) {
    _jsonAppendChar(tls, p, 0);
    _jsonStringTrimOneChar(ref (tls).DerefOrNull(), p);
    return libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FeErr) == 0);
}

internal static Tu32 /*r*/ _jsonUnescapeOneChar(ref libc.TLS tls, uintptr z, Tu32 n, @unsafe.Pointer piOut) {
    Tu32 r = default;

    Tu32 nSkip = default;
    Tu32 v = default;
    Tu32 vlo = default;
    Tu32 v1 = default;
    int32 sz = default;
    int32 v3 = default;
    bool v2 = default;
    _ = nSkip;
    _ = sz;
    _ = v;
    _ = vlo;
    _ = v1;
    _ = v2;
    _ = v3;
    if (n < (uint32)2) {
        (@unsafe.Pointer.To<Tu32>(piOut)).Value = (uint32)JSON_INVALID_CHAR;
        return n;
    }
    var exprᴛ1 = libc.Int32FromUint8(libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + 1))));
    var matchᴛ1 = false;
    if (exprᴛ1 == (int32)'u') { matchᴛ1 = true;
        if (n < (uint32)6) {
            (@unsafe.Pointer.To<Tu32>(piOut)).Value = (uint32)JSON_INVALID_CHAR;
            return n;
        }
        v = _jsonHexToInt4(ref tls, z + 2);
        v2 = (Tu32)(v & (uint32)0xfc00) == (uint32)0xd800 && n >= (uint32)12 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + 6))) == (int32)'\\' && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + 7))) == (int32)'u'; if (v2) {
            v1 = _jsonHexToInt4(ref tls, z + 8);
            vlo = v1;
        }
        if (v2 && (Tu32)(v1 & (uint32)0xfc00) == (uint32)0xdc00) {
            (@unsafe.Pointer.To<Tu32>(piOut)).Value = ((Tu32)(v & (uint32)0x3ff) << (int)((int32)10)) + (Tu32)(vlo & (uint32)0x3ff) + (uint32)0x10000;
            return (uint32)12;
        } else {
            (@unsafe.Pointer.To<Tu32>(piOut)).Value = v;
            return (uint32)6;
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)'b') { matchᴛ1 = true;
        (@unsafe.Pointer.To<Tu32>(piOut)).Value = (uint32)'\b';
        return (uint32)2;
    }
    if (exprᴛ1 == (int32)'f') { matchᴛ1 = true;
        (@unsafe.Pointer.To<Tu32>(piOut)).Value = (uint32)'\f';
        return (uint32)2;
    }
    if (exprᴛ1 == (int32)'n') { matchᴛ1 = true;
        (@unsafe.Pointer.To<Tu32>(piOut)).Value = (uint32)'\n';
        return (uint32)2;
    }
    if (exprᴛ1 == (int32)'r') { matchᴛ1 = true;
        (@unsafe.Pointer.To<Tu32>(piOut)).Value = (uint32)'\r';
        return (uint32)2;
    }
    if (exprᴛ1 == (int32)'t') { matchᴛ1 = true;
        (@unsafe.Pointer.To<Tu32>(piOut)).Value = (uint32)'\t';
        return (uint32)2;
    }
    if (exprᴛ1 == (int32)'v') { matchᴛ1 = true;
        (@unsafe.Pointer.To<Tu32>(piOut)).Value = (uint32)'\v';
        return (uint32)2;
    }
    if (exprᴛ1 == (int32)'0') { matchᴛ1 = true;
        if (n > (uint32)2 && (libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + 2)))]) & (int32)0x04) != 0) {
            v3 = (int32)JSON_INVALID_CHAR;
        } else {
            v3 = 0;
        }
        (@unsafe.Pointer.To<Tu32>(piOut)).Value = libc.Uint32FromInt32(v3);
        return (uint32)2;
    }
    if (exprᴛ1 == (int32)'\'') { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)'"') { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)'/') { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)'\\') { matchᴛ1 = true;
        (@unsafe.Pointer.To<Tu32>(piOut)).Value = libc.Uint32FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + 1)));
        return (uint32)2;
    }
    if (exprᴛ1 == (int32)'x') { matchᴛ1 = true;
        if (n < (uint32)4) {
            (@unsafe.Pointer.To<Tu32>(piOut)).Value = (uint32)JSON_INVALID_CHAR;
            return n;
        }
        (@unsafe.Pointer.To<Tu32>(piOut)).Value = libc.Uint32FromInt32(libc.Int32FromUint8(_jsonHexToInt(ref tls, (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + 2))))) << (int)((int32)4) | libc.Int32FromUint8(_jsonHexToInt(ref tls, (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + 3))))));
        return (uint32)4;
    }
    if (exprᴛ1 == (int32)0xe2) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)'\r') { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)'\n') { matchᴛ1 = true;
        nSkip = _jsonBytesToBypass(ref tls, z, n);
        if (nSkip == (uint32)0) {
            (@unsafe.Pointer.To<Tu32>(piOut)).Value = (uint32)JSON_INVALID_CHAR;
            return n;
        } else {
            if (nSkip == n) {
                (@unsafe.Pointer.To<Tu32>(piOut)).Value = (uint32)0;
                return n;
            } else {
                if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)nSkip))) == (int32)'\\') {
                    return nSkip + _jsonUnescapeOneChar(ref tls, z + (uintptr)nSkip, n - nSkip, piOut);
                } else {
                    sz = _sqlite3Utf8ReadLimited(ref tls, z + (uintptr)nSkip, libc.Int32FromUint32(n - nSkip), piOut);
                    return nSkip + libc.Uint32FromInt32(sz);
                }
            }
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        (@unsafe.Pointer.To<Tu32>(piOut)).Value = (uint32)JSON_INVALID_CHAR;
        return (uint32)2;
    }
    return default;

    return r;
}

internal static Ti64 /*r*/ _keywordCode(ref libc.TLS tls, uintptr z, Ti64 n, @unsafe.Pointer pType) {
    Ti64 i = default;
    Ti64 j = default;
    uintptr zKW = default;
    _ = i;
    _ = j;
    _ = zKW;
    i = ((int64)(libc.Int32FromUint8(_sqlite3UpperToLower[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z)))]) * (int32)4 ^ libc.Int32FromUint8(_sqlite3UpperToLower[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(n - (int64)1))))]) * (int32)3) ^ n * (int64)1) % (int64)127;
    i = (int64)libc.Int32FromUint8(_aKWHash[(nint)(i)]);
    for (;;) {
        if (!(i > 0)) {
            break;
        }
        if (libc.Int64FromUint8(_aKWLen[(nint)(i)]) != n) {
            goto _1;
        }
        zKW = (uintptr)Ꮡ_zKWText + (uintptr)_aKWOffset[(nint)(i)];
        if (((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) & ~libc.Int32FromInt32(0x20)) != (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zKW)))) {
            goto _1;
        }
        if (((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + 1))) & ~libc.Int32FromInt32(0x20)) != (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zKW + 1)))) {
            goto _1;
        }
        j = (int64)2;
        while (j < n && ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)j))) & ~libc.Int32FromInt32(0x20)) == (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zKW + (uintptr)j)))) {
            j = j + 1;
        }
        if (j < n) {
            goto _1;
        }
        (@unsafe.Pointer.To<int32>(pType)).Value = libc.Int32FromUint8(_aKWCode[(nint)(i)]);
        break;
        goto _1;
_1:;
        i = libc.Int64FromUint8(_aKWNext[(nint)(i)]);
    }
    return n;
}

internal static void _lowerFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    int32 i = default;
    int32 n = default;
    uintptr z1 = default;
    uintptr z2 = default;
    _ = i;
    _ = n;
    _ = z1;
    _ = z2;
    _ = argc;
    z2 = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    n = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    if (z2 != 0) {
        z1 = _contextMalloc(tls, context, (int64)n + (int64)1);
        if (z1 != 0) {
            i = 0;
            for (;;) {
                if (!(i < n)) {
                    break;
                }
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(z1 + (uintptr)i))).Value = libc.Int8FromUint8(_sqlite3UpperToLower[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z2 + (uintptr)i)))]);
                goto _1;
_1:;
                i = i + 1;
            }
            Xsqlite3_result_text(tls, context, z1, n, __ccgo_fp(Xsqlite3_free));
        }
    }
}

internal static int32 /*r*/ _patternCompare(ж<libc.TLS> tls, uintptr zPatternʗp, uintptr zStringʗp, uintptr pInfo, Tu32 matchOther) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        ref var zPattern = ref heap(zPatternʗp, out var ᏑzPattern);
        ref var zString = ref heap(zStringʗp, out var ᏑzString);

        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 bMatch = default;
        int32 bMatch1 = default;
        int32 bMatch2 = default;
        int32 invert = default;
        int32 seen = default;
        int32 v13 = default;
        Tu32 c = default;
        Tu32 c2 = default;
        Tu32 matchAll = default;
        Tu32 matchOne = default;
        Tu32 prior_c = default;
        Tu32 v1 = default;
        Tu32 v4 = default;
        Tu8 noCase = default;
        uintptr zEscaped = default;
        uintptr v3 = default;
        uintptr v6 = default;
        uint32 v2 = default;
        uint32 v5 = default;
        array<int8> _ᴛ1 = new(3);
        _ = bMatch;
        _ = bMatch1;
        _ = bMatch2;
        _ = c;
        _ = c2;
        _ = invert;
        _ = matchAll;
        _ = matchOne;
        _ = noCase;
        _ = prior_c;
        _ = seen;
        _ = zEscaped;
        _ = v1;
        _ = v13;
        _ = v2;
        _ = v3;
        _ = v4;
        _ = v5;
        _ = v6;
        matchOne = (uint32)(@unsafe.Pointer.To<TcompareInfo>((@unsafe.Pointer)pInfo)).Value.FmatchOne;
        matchAll = (uint32)(@unsafe.Pointer.To<TcompareInfo>((@unsafe.Pointer)pInfo)).Value.FmatchAll;
        noCase = (@unsafe.Pointer.To<TcompareInfo>((@unsafe.Pointer)pInfo)).Value.FnoCase;
        zEscaped = (uintptr)0;
        for (;;) {
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zPattern))) < (int32)0x80) {
                v3 = zPattern;
                zPattern = zPattern + 1;
                v2 = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v3)));
            } else {
                v2 = _sqlite3Utf8Read(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzPattern));
            }
            v1 = v2;
            c = v1;
            if (!(v1 != (uint32)0)) {
                break;
            }
            if (c == matchAll) {
                for (;;) {
                    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zPattern))) < (int32)0x80) {
                        v6 = zPattern;
                        zPattern = zPattern + 1;
                        v5 = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v6)));
                    } else {
                        v5 = _sqlite3Utf8Read(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzPattern));
                    }
                    v4 = v5;
                    c = v4;
                    if (!(v4 == matchAll || c == matchOne && matchOne != (uint32)0)) {
                        break;
                    }
                    if (c == matchOne && _sqlite3Utf8Read(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzString)) == (uint32)0) {
                        r = (int32)SQLITE_NOWILDCARDMATCH; goto ᒐdone;
                    }
                }
                if (c == (uint32)0) {
                    r = SQLITE_MATCH; goto ᒐdone;
                } else {
                    if (c == matchOther) {
                        if (libc.Int32FromUint8((@unsafe.Pointer.To<TcompareInfo>((@unsafe.Pointer)pInfo)).Value.FmatchSet) == 0) {
                            c = _sqlite3Utf8Read(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzPattern));
                            if (c == (uint32)0) {
                                r = (int32)SQLITE_NOWILDCARDMATCH; goto ᒐdone;
                            }
                        } else {
                            while (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zString)) != 0) {
                                bMatch = _patternCompare(tls, zPattern + (uintptr)(-libc.Int32FromInt32(1)), zString, pInfo, matchOther);
                                if (bMatch != (int32)SQLITE_NOMATCH) {
                                    r = bMatch; goto ᒐdone;
                                }
                                v3 = zString;
                                zString = zString + 1;
                                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v3))) >= (int32)0xc0) {
                                    while ((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zString))) & (int32)0xc0) == (int32)0x80) {
                                        zString = zString + 1;
                                    }
                                }
                            }
                            r = (int32)SQLITE_NOWILDCARDMATCH; goto ᒐdone;
                        }
                    }
                }
                if (c < (uint32)0x80) {
                    if (noCase != 0) {
                        @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp), 0, 3) = libc.Int8FromUint32((Tu32)(c & libc.Uint32FromInt32(~(libc.Int32FromUint8(_sqlite3CtypeMap[(uint8)c]) & libc.Int32FromInt32(0x20)))));
                        @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp), (int32)1, 3) = libc.Int8FromUint8(_sqlite3UpperToLower[(uint8)c]);
                        @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp), (int32)2, 3) = 0;
                    } else {
                        @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp), 0, 3) = libc.Int8FromUint32(c);
                        @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp), (int32)1, 3) = 0;
                    }
                    while ((int32)1 != 0) {
                        zString = zString + (uintptr)libc.Xstrcspn(tls, zString, bp);
                        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zString))) == 0) {
                            break;
                        }
                        zString = zString + 1;
                        bMatch1 = _patternCompare(tls, zPattern, zString, pInfo, matchOther);
                        if (bMatch1 != (int32)SQLITE_NOMATCH) {
                            r = bMatch1; goto ᒐdone;
                        }
                    }
                } else {
                    for (;;) {
                        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zString))) < (int32)0x80) {
                            v3 = zString;
                            zString = zString + 1;
                            v2 = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v3)));
                        } else {
                            v2 = _sqlite3Utf8Read(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzString));
                        }
                        v1 = v2;
                        c2 = v1;
                        if (!(v1 != (uint32)0)) {
                            break;
                        }
                        if (c2 != c) {
                            continue;
                        }
                        bMatch2 = _patternCompare(tls, zPattern, zString, pInfo, matchOther);
                        if (bMatch2 != (int32)SQLITE_NOMATCH) {
                            r = bMatch2; goto ᒐdone;
                        }
                    }
                }
                r = (int32)SQLITE_NOWILDCARDMATCH; goto ᒐdone;
            }
            if (c == matchOther) {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TcompareInfo>((@unsafe.Pointer)pInfo)).Value.FmatchSet) == 0) {
                    c = _sqlite3Utf8Read(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzPattern));
                    if (c == (uint32)0) {
                        r = (int32)SQLITE_NOMATCH; goto ᒐdone;
                    }
                    zEscaped = zPattern;
                } else {
                    prior_c = (uint32)0;
                    seen = 0;
                    invert = 0;
                    c = _sqlite3Utf8Read(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzString));
                    if (c == (uint32)0) {
                        r = (int32)SQLITE_NOMATCH; goto ᒐdone;
                    }
                    c2 = _sqlite3Utf8Read(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzPattern));
                    if (c2 == (uint32)'^') {
                        invert = (int32)1;
                        c2 = _sqlite3Utf8Read(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzPattern));
                    }
                    if (c2 == (uint32)']') {
                        if (c == (uint32)']') {
                            seen = (int32)1;
                        }
                        c2 = _sqlite3Utf8Read(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzPattern));
                    }
                    while (c2 != 0 && c2 != (uint32)']') {
                        if (c2 == (uint32)'-' && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zPattern))) != (int32)']' && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zPattern))) != 0 && prior_c > (uint32)0) {
                            c2 = _sqlite3Utf8Read(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzPattern));
                            if (c >= prior_c && c <= c2) {
                                seen = (int32)1;
                            }
                            prior_c = (uint32)0;
                        } else {
                            if (c == c2) {
                                seen = (int32)1;
                            }
                            prior_c = c2;
                        }
                        c2 = _sqlite3Utf8Read(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzPattern));
                    }
                    if (c2 == (uint32)0 || (seen ^ invert) == 0) {
                        r = (int32)SQLITE_NOMATCH; goto ᒐdone;
                    }
                    continue;
                }
            }
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zString))) < (int32)0x80) {
                v3 = zString;
                zString = zString + 1;
                v2 = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v3)));
            } else {
                v2 = _sqlite3Utf8Read(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzString));
            }
            c2 = v2;
            if (c == c2) {
                continue;
            }
            if (noCase != 0 && libc.Int32FromUint8(_sqlite3UpperToLower[(uint8)c]) == libc.Int32FromUint8(_sqlite3UpperToLower[(uint8)c2]) && c < (uint32)0x80 && c2 < (uint32)0x80) {
                continue;
            }
            if (c == matchOne && zPattern != zEscaped && c2 != (uint32)0) {
                continue;
            }
            r = (int32)SQLITE_NOMATCH; goto ᒐdone;
        }
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zString))) == 0) {
            v13 = SQLITE_MATCH;
        } else {
            v13 = (int32)SQLITE_NOMATCH;
        }
        r = v13;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _ptrmapPut(ж<libc.TLS> tls, uintptr pBt, TPgno key, Tu8 eType, TPgno parent, @unsafe.Pointer pRC) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        TPgno iPtrmap = default;
        int32 offset = default;
        int32 rc = default;
        int32 v1 = default;
        uintptr pPtrmap = default;
        uintptr _ᴛ1 = default;
        _ = iPtrmap;
        _ = offset;
        _ = pPtrmap;
        _ = rc;
        _ = v1;
        if (~@unsafe.Pointer.To<int32>(pRC) != 0) {
            return;
        }
        if (key == (uint32)0) {
            (@unsafe.Pointer.To<int32>(pRC)).Value = _sqlite3CorruptError(tls, (int32)74248);
            return;
        }
        iPtrmap = _ptrmapPageno(ref (tls).DerefOrNull(), pBt, key);
        rc = _sqlite3PagerGet(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager, iPtrmap, bp, 0);
        if (rc != SQLITE_OK) {
            (@unsafe.Pointer.To<int32>(pRC)).Value = rc;
            return;
        }
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(_sqlite3PagerGetExtra(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)))))) != 0) {
            (@unsafe.Pointer.To<int32>(pRC)).Value = _sqlite3CorruptError(tls, (int32)74261);
            goto ptrmap_exit;
        }
        offset = libc.Int32FromUint32(libc.Uint32FromInt32(5) * (key - iPtrmap - libc.Uint32FromInt32(1)));
        if (offset < 0) {
            (@unsafe.Pointer.To<int32>(pRC)).Value = _sqlite3CorruptError(tls, (int32)74266);
            goto ptrmap_exit;
        }
        pPtrmap = _sqlite3PagerGetData(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
        if (libc.Int32FromUint8(eType) != libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pPtrmap + (uintptr)offset))) || _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pPtrmap + (uintptr)(offset + (int32)1))) != parent) {
            v1 = _sqlite3PagerWrite(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
            rc = v1;
            (@unsafe.Pointer.To<int32>(pRC)).Value = v1;
            if (rc == SQLITE_OK) {
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pPtrmap + (uintptr)offset))).Value = eType;
                _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pPtrmap + (uintptr)(offset + (int32)1)), parent);
            }
        }
        goto ptrmap_exit;
ptrmap_exit:;
        _sqlite3PagerUnref(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _rbuDeltaOutputSize(ref libc.TLS tls, uintptr zDeltaʗp, int32 lenDeltaʗp) {
    ref var zDelta = ref heap(zDeltaʗp, out var ᏑzDelta);
    ref var lenDelta = ref heap(lenDeltaʗp, out var ᏑlenDelta);

    int32 size = default;
    _ = size;
    size = libc.Int32FromUint32(_rbuDeltaGetInt(ref tls, @unsafe.Pointer.FromBox(ᏑzDelta), @unsafe.Pointer.FromPinnedBox(ᏑlenDelta)));
    if (lenDelta <= 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDelta))) != (int32)'\n') {
        return -(int32)1;
    }
    return size;
}

internal static int8 /*r*/ _sqlite3AffinityType(ж<libc.TLS> tls, uintptr zIn, uintptr pCol) {
    int8 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int8 aff = default;
        Tu32 h = default;
        Tu8 x = default;
        uintptr zChar = default;
        int32 _ᴛ1 = default;
        _ = aff;
        _ = h;
        _ = x;
        _ = zChar;
        h = (uint32)0;
        aff = (int8)SQLITE_AFF_NUMERIC;
        zChar = (uintptr)0;
        while (~@unsafe.Pointer.To<int8>(__ccgo_ptr(zIn)) != 0) {
            x = ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zIn));
            h = (h << (int)(libc.Int32FromInt32(8))) + (uint32)_sqlite3UpperToLower[x];
            zIn = zIn + 1;
            if (h == libc.Uint32FromInt32(libc.Int32FromUint8((uint8)'c').Lsh((int64)(libc.Int32FromInt32(24))) + libc.Int32FromUint8((uint8)'h').Lsh((int64)(libc.Int32FromInt32(16))) + libc.Int32FromUint8((uint8)'a').Lsh((int64)(libc.Int32FromInt32(8))) + libc.Int32FromUint8((uint8)'r'))) {
                aff = (int8)SQLITE_AFF_TEXT;
                zChar = zIn;
            } else {
                if (h == libc.Uint32FromInt32(libc.Int32FromUint8((uint8)'c').Lsh((int64)(libc.Int32FromInt32(24))) + libc.Int32FromUint8((uint8)'l').Lsh((int64)(libc.Int32FromInt32(16))) + libc.Int32FromUint8((uint8)'o').Lsh((int64)(libc.Int32FromInt32(8))) + libc.Int32FromUint8((uint8)'b'))) {
                    aff = (int8)SQLITE_AFF_TEXT;
                } else {
                    if (h == libc.Uint32FromInt32(libc.Int32FromUint8((uint8)'t').Lsh((int64)(libc.Int32FromInt32(24))) + libc.Int32FromUint8((uint8)'e').Lsh((int64)(libc.Int32FromInt32(16))) + libc.Int32FromUint8((uint8)'x').Lsh((int64)(libc.Int32FromInt32(8))) + libc.Int32FromUint8((uint8)'t'))) {
                        aff = (int8)SQLITE_AFF_TEXT;
                    } else {
                        if (h == libc.Uint32FromInt32(libc.Int32FromUint8((uint8)'b').Lsh((int64)(libc.Int32FromInt32(24))) + libc.Int32FromUint8((uint8)'l').Lsh((int64)(libc.Int32FromInt32(16))) + libc.Int32FromUint8((uint8)'o').Lsh((int64)(libc.Int32FromInt32(8))) + libc.Int32FromUint8((uint8)'b')) && ((int32)aff == (int32)SQLITE_AFF_NUMERIC || (int32)aff == (int32)SQLITE_AFF_REAL)) {
                            aff = (int8)SQLITE_AFF_BLOB;
                            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zIn))) == (int32)'(') {
                                zChar = zIn;
                            }
                        } else {
                            if (h == libc.Uint32FromInt32(libc.Int32FromUint8((uint8)'r').Lsh((int64)(libc.Int32FromInt32(24))) + libc.Int32FromUint8((uint8)'e').Lsh((int64)(libc.Int32FromInt32(16))) + libc.Int32FromUint8((uint8)'a').Lsh((int64)(libc.Int32FromInt32(8))) + libc.Int32FromUint8((uint8)'l')) && (int32)aff == (int32)SQLITE_AFF_NUMERIC) {
                                aff = (int8)SQLITE_AFF_REAL;
                            } else {
                                if (h == libc.Uint32FromInt32(libc.Int32FromUint8((uint8)'f').Lsh((int64)(libc.Int32FromInt32(24))) + libc.Int32FromUint8((uint8)'l').Lsh((int64)(libc.Int32FromInt32(16))) + libc.Int32FromUint8((uint8)'o').Lsh((int64)(libc.Int32FromInt32(8))) + libc.Int32FromUint8((uint8)'a')) && (int32)aff == (int32)SQLITE_AFF_NUMERIC) {
                                    aff = (int8)SQLITE_AFF_REAL;
                                } else {
                                    if (h == libc.Uint32FromInt32(libc.Int32FromUint8((uint8)'d').Lsh((int64)(libc.Int32FromInt32(24))) + libc.Int32FromUint8((uint8)'o').Lsh((int64)(libc.Int32FromInt32(16))) + libc.Int32FromUint8((uint8)'u').Lsh((int64)(libc.Int32FromInt32(8))) + libc.Int32FromUint8((uint8)'b')) && (int32)aff == (int32)SQLITE_AFF_NUMERIC) {
                                        aff = (int8)SQLITE_AFF_REAL;
                                    } else {
                                        if ((Tu32)(h & (uint32)0x00FFFFFF) == libc.Uint32FromInt32(libc.Int32FromUint8((uint8)'i').Lsh((int64)(libc.Int32FromInt32(16))) + libc.Int32FromUint8((uint8)'n').Lsh((int64)(libc.Int32FromInt32(8))) + libc.Int32FromUint8((uint8)'t'))) {
                                            aff = (int8)SQLITE_AFF_INTEGER;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        if (pCol != 0) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = 0;
            if ((int32)aff < (int32)SQLITE_AFF_NUMERIC) {
                if (zChar != 0) {
                    while (~@unsafe.Pointer.To<int8>(__ccgo_ptr(zChar)) != 0) {
                        if ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zChar)))]) & (int32)0x04) != 0) {
                            _sqlite3GetInt32(tls, zChar, bp);
                            break;
                        }
                        zChar = zChar + 1;
                    }
                } else {
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = (int32)16;
                }
            }
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) / (int32)4 + (int32)1;
            if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) > (int32)255) {
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = (int32)255;
            }
            (@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FszEst = libc.Uint8FromInt32(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)));
        }
        r = aff;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3AppendOneUtf8Character(ref libc.TLS tls, @unsafe.Pointer zOut, Tu32 v) {
    if (v < (uint32)0x00080) {
        (@unsafe.Pointer.To<int8>(zOut)).Value = libc.Int8FromUint8((uint8)((Tu32)(v & libc.Uint32FromInt32(0xff))));
        return (int32)1;
    }
    if (v < (uint32)0x00800) {
        (@unsafe.Pointer.To<int8>(zOut)).Value = (int8)((int32)0xc0 + libc.Int32FromUint8((uint8)((Tu32)(v >> (int)(libc.Int32FromInt32(6)) & libc.Uint32FromInt32(0x1f)))));
        (@unsafe.Pointer.To<int8>(@unsafe.Add(zOut, 1))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(v & libc.Uint32FromInt32(0x3f)))));
        return (int32)2;
    }
    if (v < (uint32)0x10000) {
        (@unsafe.Pointer.To<int8>(zOut)).Value = (int8)((int32)0xe0 + libc.Int32FromUint8((uint8)((Tu32)(v >> (int)(libc.Int32FromInt32(12)) & libc.Uint32FromInt32(0x0f)))));
        (@unsafe.Pointer.To<int8>(@unsafe.Add(zOut, 1))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(v >> (int)(libc.Int32FromInt32(6)) & libc.Uint32FromInt32(0x3f)))));
        (@unsafe.Pointer.To<int8>(@unsafe.Add(zOut, 2))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(v & libc.Uint32FromInt32(0x3f)))));
        return (int32)3;
    }
    (@unsafe.Pointer.To<int8>(zOut)).Value = (int8)((int32)0xf0 + libc.Int32FromUint8((uint8)((Tu32)(v >> (int)(libc.Int32FromInt32(18)) & libc.Uint32FromInt32(0x07)))));
    (@unsafe.Pointer.To<int8>(@unsafe.Add(zOut, 1))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(v >> (int)(libc.Int32FromInt32(12)) & libc.Uint32FromInt32(0x3f)))));
    (@unsafe.Pointer.To<int8>(@unsafe.Add(zOut, 2))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(v >> (int)(libc.Int32FromInt32(6)) & libc.Uint32FromInt32(0x3f)))));
    (@unsafe.Pointer.To<int8>(@unsafe.Add(zOut, 3))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(v & libc.Uint32FromInt32(0x3f)))));
    return (int32)4;
}

internal static int32 /*r*/ _sqlite3Atoi64(ref libc.TLS tls, uintptr zNum, @unsafe.Pointer pNum, int32 length, Tu8 enc) {
    int32 r = default;

    uint32 c = default;
    uint32 v3 = default;
    int32 i = default;
    int32 incr = default;
    int32 j = default;
    int32 jj = default;
    int32 neg = default;
    int32 nonNum = default;
    int32 rc = default;
    int32 v6 = default;
    Tu64 u = default;
    uintptr zEnd = default;
    uintptr zStart = default;
    bool v4 = default;
    int64 v5 = default;
    _ = c;
    _ = i;
    _ = incr;
    _ = j;
    _ = jj;
    _ = neg;
    _ = nonNum;
    _ = rc;
    _ = u;
    _ = zEnd;
    _ = zStart;
    _ = v3;
    _ = v4;
    _ = v5;
    _ = v6;
    u = (uint64)0;
    neg = 0;
    c = (uint32)0;
    nonNum = 0;
    zEnd = zNum + (uintptr)length;
    if (libc.Int32FromUint8(enc) == (int32)SQLITE_UTF8) {
        incr = (int32)1;
    } else {
        incr = (int32)2;
        length = length & ~libc.Int32FromInt32(1);
        i = (int32)3 - libc.Int32FromUint8(enc);
        for (;;) {
            if (!(i < length && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum + (uintptr)i))) == 0)) {
                break;
            }
            goto _1;
_1:;
            i = i + (int32)2;
        }
        nonNum = libc.BoolInt32(i < length);
        zEnd = zNum + (uintptr)(i ^ (int32)1);
        zNum = zNum + (uintptr)(libc.Int32FromUint8(enc) & libc.Int32FromInt32(1));
    }
    while (zNum < zEnd && (libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum)))]) & (int32)0x01) != 0) {
        zNum = zNum + (uintptr)incr;
    }
    if (zNum < zEnd) {
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum))) == (int32)'-') {
            neg = (int32)1;
            zNum = zNum + (uintptr)incr;
        } else {
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum))) == (int32)'+') {
                zNum = zNum + (uintptr)incr;
            }
        }
    }
    zStart = zNum;
    while (zNum < zEnd && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum))) == (int32)'0') {
        zNum = zNum + (uintptr)incr;
    }
    i = 0;
    for (;;) {
        v4 = zNum + (uintptr)i < zEnd; if (v4) {
            v3 = libc.Uint32FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum + (uintptr)i))) - libc.Uint32FromUint8((uint8)'0');
            c = v3;
        }
        if (!(v4 && v3 <= (uint32)9)) {
            break;
        }
        u = u * (uint64)10 + (uint64)c;
        goto _2;
_2:;
        i = i + incr;
    }
    if (u > libc.Uint64FromInt64(libc.Int64FromUint32(0xffffffffU) | libc.Int64FromInt32(0x7fffffff).Lsh((int64)(libc.Int32FromInt32(32))))) {
        if (neg != 0) {
            v5 = (int64)(-libc.Int32FromInt32(1)) - (libc.Int64FromUint32(0xffffffffU) | libc.Int64FromInt32(0x7fffffff).Lsh((int64)(libc.Int32FromInt32(32))));
        } else {
            v5 = libc.Int64FromUint32(0xffffffffU) | libc.Int64FromInt32(0x7fffffff).Lsh((int64)(libc.Int32FromInt32(32)));
        }
        (@unsafe.Pointer.To<Ti64>(pNum)).Value = v5;
    } else {
        if (neg != 0) {
            (@unsafe.Pointer.To<Ti64>(pNum)).Value = -libc.Int64FromUint64(u);
        } else {
            (@unsafe.Pointer.To<Ti64>(pNum)).Value = libc.Int64FromUint64(u);
        }
    }
    rc = 0;
    if (i == 0 && zStart == zNum) {
        rc = -(int32)1;
    } else {
        if (nonNum != 0) {
            rc = (int32)1;
        } else {
            if (zNum + (uintptr)i < zEnd) {
                jj = i;
                for (var cond = true; cond; cond = zNum + (uintptr)jj < zEnd) {
                    if (!((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum + (uintptr)jj)))]) & libc.Int32FromInt32(0x01)) != 0)) {
                        rc = (int32)1;
                        break;
                    }
                    jj = jj + incr;
                }
            }
        }
    }
    if (i < (int32)19 * incr) {
        return rc;
    } else {
        if (i > (int32)19 * incr) {
            v6 = (int32)1;
        } else {
            v6 = _compare2pow63(ref tls, zNum, incr);
        }
        j = v6;
        if (j < 0) {
            return rc;
        } else {
            if (neg != 0) {
                v5 = (int64)(-libc.Int32FromInt32(1)) - (libc.Int64FromUint32(0xffffffffU) | libc.Int64FromInt32(0x7fffffff).Lsh((int64)(libc.Int32FromInt32(32))));
            } else {
                v5 = libc.Int64FromUint32(0xffffffffU) | libc.Int64FromInt32(0x7fffffff).Lsh((int64)(libc.Int32FromInt32(32)));
            }
            (@unsafe.Pointer.To<Ti64>(pNum)).Value = v5;
            if (j > 0) {
                return (int32)2;
            } else {
                if (neg != 0) {
                    v6 = rc;
                } else {
                    v6 = (int32)3;
                }
                return v6;
            }
        }
    }
    return r;
}

internal static uintptr /*r*/ _sqlite3ColumnColl(ref libc.TLS tls, uintptr pCol) {
    uintptr z = default;
    _ = z;
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FcolFlags) & (int32)COLFLAG_HASCOLL) == 0) {
        return (uintptr)0;
    }
    z = (@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FzCnName;
    while (~@unsafe.Pointer.To<int8>(__ccgo_ptr(z)) != 0) {
        z = z + 1;
    }
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FcolFlags) & (int32)COLFLAG_HASTYPE) != 0) {
        for (var cond = true; cond; cond = ~@unsafe.Pointer.To<int8>(__ccgo_ptr(z)) != 0) {
            z = z + 1;
        }
    }
    return z + (uintptr)1;
}

internal static void _sqlite3Dequote(ref libc.TLS tls, uintptr z) {
    int32 i = default;
    int32 j = default;
    int32 v2 = default;
    int8 quote = default;
    _ = i;
    _ = j;
    _ = quote;
    _ = v2;
    if (z == (uintptr)0) {
        return;
    }
    quote = ~@unsafe.Pointer.To<int8>(__ccgo_ptr(z));
    if (!((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(quote)]) & libc.Int32FromInt32(0x80)) != 0)) {
        return;
    }
    if ((int32)quote == (int32)'[') {
        quote = (int8)']';
    }
    i = (int32)1;
    j = libc.Int32FromInt32(0);
    for (;;) {
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i))) == (int32)quote) {
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(i + (int32)1)))) == (int32)quote) {
                v2 = j;
                j = j + 1;
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)v2))).Value = quote;
                i = i + 1;
            } else {
                break;
            }
        } else {
            v2 = j;
            j = j + 1;
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)v2))).Value = (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i))).Value;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)j))).Value = 0;
}

internal static void _sqlite3DequoteExpr(ref libc.TLS tls, uintptr p) {
    int32 v1 = default;
    _ = v1;
    if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8))))) == (int32)'"') {
        v1 = libc.Int32FromInt32(EP_Quoted) | libc.Int32FromInt32(EP_DblQuoted);
    } else {
        v1 = (int32)EP_Quoted;
    }
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value |= libc.Uint32FromInt32(v1);
    _sqlite3Dequote(ref tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8)));
}

internal static int32 /*r*/ _sqlite3ExprNeedsNoAffinityChange(ref libc.TLS tls, uintptr p, int8 aff) {
    int32 r = default;

    Tu8 op = default;
    int32 unaryMinus = default;
    _ = op;
    _ = unaryMinus;
    unaryMinus = 0;
    if ((int32)aff == (int32)SQLITE_AFF_BLOB) {
        return (int32)1;
    }
    while (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_UPLUS || libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_UMINUS) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_UMINUS) {
            unaryMinus = (int32)1;
        }
        p = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft;
    }
    op = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop;
    if (libc.Int32FromUint8(op) == (int32)TK_REGISTER) {
        op = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop2;
    }
    switch (libc.Int32FromUint8(op)) {
    case var ᴛ when ᴛ == (int32)TK_INTEGER: {
        return libc.BoolInt32((int32)aff >= (int32)SQLITE_AFF_NUMERIC);
    }
    case var ᴛ when ᴛ == (int32)TK_FLOAT: {
        return libc.BoolInt32((int32)aff >= (int32)SQLITE_AFF_NUMERIC);
    }
    case var ᴛ when ᴛ == (int32)TK_STRING: {
        return libc.BoolInt32(!(unaryMinus != 0) && (int32)aff == (int32)SQLITE_AFF_TEXT);
    }
    case var ᴛ when ᴛ == (int32)TK_BLOB: {
        return libc.BoolInt32(!(unaryMinus != 0));
    }
    case var ᴛ when ᴛ == (int32)TK_COLUMN: {
        return libc.BoolInt32((int32)aff >= (int32)SQLITE_AFF_NUMERIC && (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FiColumn < 0);
    }
    default: {
        return 0;
    }}

    return r;
}

internal static int32 /*r*/ _sqlite3Fts5IndexCharlenToBytelen(ref libc.TLS tls, uintptr p, int32 nByte, int32 nChar) {
    int32 i = default;
    int32 n = default;
    int32 v2 = default;
    _ = i;
    _ = n;
    _ = v2;
    n = 0;
    i = 0;
    for (;;) {
        if (!(i < nChar)) {
            break;
        }
        if (n >= nByte) {
            return 0;
        }
        v2 = n;
        n = n + 1;
        if (libc.Int32FromUint8(libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p + (uintptr)v2)))) >= (int32)0xc0) {
            if (n >= nByte) {
                return 0;
            }
            while (((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p + (uintptr)n))) & (int32)0xc0) == (int32)0x80) {
                n = n + 1;
                if (n >= nByte) {
                    if (i + (int32)1 == nChar) {
                        break;
                    }
                    return 0;
                }
            }
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return n;
}

internal static Tu64 /*r*/ _sqlite3Fts5IndexEntryCksum(ref libc.TLS tls, Ti64 iRowid, int32 iCol, int32 iPos, int32 iIdx, uintptr pTerm, int32 nTerm) {
    int32 i = default;
    Tu64 ret = default;
    _ = i;
    _ = ret;
    ret = libc.Uint64FromInt64(iRowid);
    ret = ret + ((ret << (int)(libc.Int32FromInt32(3))) + libc.Uint64FromInt32(iCol));
    ret = ret + ((ret << (int)(libc.Int32FromInt32(3))) + libc.Uint64FromInt32(iPos));
    if (iIdx >= 0) {
        ret = ret + ((ret << (int)(libc.Int32FromInt32(3))) + libc.Uint64FromInt32(libc.Int32FromUint8((uint8)'0') + iIdx));
    }
    i = 0;
    for (;;) {
        if (!(i < nTerm)) {
            break;
        }
        ret = ret + ((ret << (int)(libc.Int32FromInt32(3))) + libc.Uint64FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(pTerm + (uintptr)i))));
        goto _1;
_1:;
        i = i + 1;
    }
    return ret;
}

internal static int32 /*r*/ _sqlite3GetUInt32(ref libc.TLS tls, uintptr z, uintptr pI) {
    int32 i = default;
    Tu64 v = default;
    _ = i;
    _ = v;
    v = (uint64)0;
    i = 0;
    for (;;) {
        if (!((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i)))]) & (int32)0x04) != 0)) {
            break;
        }
        v = v * (uint64)10 + libc.Uint64FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i))) - (uint64)'0';
        if (v > (uint64)4294967295U) {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pI))).Value = (uint32)0;
            return 0;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    if (i == 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i))) != 0) {
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pI))).Value = (uint32)0;
        return 0;
    }
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pI))).Value = (uint32)v;
    return (int32)1;
}

internal static uintptr /*r*/ _sqlite3HexToBlob(ж<libc.TLS> tls, uintptr db, uintptr z, int32 n) {
    int32 i = default;
    uintptr zBlob = default;
    _ = i;
    _ = zBlob;
    zBlob = _sqlite3DbMallocRawNN(tls, db, libc.Uint64FromInt32(n / (int32)2 + (int32)1));
    n = n - 1;
    if (zBlob != 0) {
        i = 0;
        for (;;) {
            if (!(i < n)) {
                break;
            }
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(zBlob + (uintptr)(i / (int32)2)))).Value = (int8)(libc.Int32FromUint8(_sqlite3HexToInt(ref (tls).DerefOrNull(), (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i))))) << (int)((int32)4) | libc.Int32FromUint8(_sqlite3HexToInt(ref (tls).DerefOrNull(), (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(i + (int32)1)))))));
            goto _1;
_1:;
            i = i + (int32)2;
        }
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zBlob + (uintptr)(i / (int32)2)))).Value = 0;
    }
    return zBlob;
}

internal static int32 /*r*/ _sqlite3IndexedExprLookup(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, int32 target) {
    int32 addr = default;
    int32 iDataCur = default;
    Tu8 exprAff = default;
    uintptr p = default;
    uintptr v = default;
    _ = addr;
    _ = exprAff;
    _ = iDataCur;
    _ = p;
    _ = v;
    p = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpIdxEpr;
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        iDataCur = (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiDataCur;
        if (iDataCur < 0) {
            goto _1;
        }
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiSelfTab != 0) {
            if ((@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiDataCur != (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiSelfTab - (int32)1) {
                goto _1;
            }
            iDataCur = -(int32)1;
        }
        if (_sqlite3ExprCompare(tls, (uintptr)0, pExpr, (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FpExpr, iDataCur) != 0) {
            goto _1;
        }
        exprAff = libc.Uint8FromInt8(_sqlite3ExprAffinity(tls, pExpr));
        if (libc.Int32FromUint8(exprAff) <= (int32)SQLITE_AFF_BLOB && libc.Int32FromUint8((@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.Faff) != (int32)SQLITE_AFF_BLOB || libc.Int32FromUint8(exprAff) == (int32)SQLITE_AFF_TEXT && libc.Int32FromUint8((@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.Faff) != (int32)SQLITE_AFF_TEXT || libc.Int32FromUint8(exprAff) >= (int32)SQLITE_AFF_NUMERIC && libc.Int32FromUint8((@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.Faff) != (int32)SQLITE_AFF_NUMERIC) {
            goto _1;
        }
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromUint32(EP_SubtArg)) != (uint32)0 && _sqlite3ExprCanReturnSubtype(tls, pParse, pExpr) != 0) {
            goto _1;
        }
        v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
        if ((@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FbMaybeNullRow != 0 || _sqlite3ExprIsConstant(tls, pParse, pExpr) != 0) {
            addr = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_IfNullRow, (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiIdxCur, addr + (int32)3, target);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiIdxCur, (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiIdxCol, target);
            _sqlite3VdbeGoto(tls, v, 0);
            p = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpIdxEpr;
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpIdxEpr = (uintptr)0;
            _sqlite3ExprCode(tls, pParse, pExpr, target);
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpIdxEpr = p;
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addr + (int32)2);
        } else {
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiIdxCur, (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiIdxCol, target);
        }
        return target;
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FpIENext;
    }
    return -(int32)1;
}

internal static int32 /*r*/ _sqlite3Stat4ProbeSetValue(ж<libc.TLS> tls, uintptr pParse, uintptr pIdx, uintptr ppRec, uintptr pExpr, int32 nElem, int32 iVal, @unsafe.Pointer pnExtract) {
    Tu8 aff = default;
    ref var alloc = ref heap(new TValueNewStat4Ctx(), out var Ꮡalloc);
    int32 i = default;
    int32 nExtract = default;
    int32 rc = default;
    uintptr pElem = default;
    ref var pVal = ref heap(new uintptr(), out var ᏑpVal);
    uintptr v2 = default;
    _ = aff;
    _ = alloc;
    _ = i;
    _ = nExtract;
    _ = pElem;
    _ = pVal;
    _ = rc;
    _ = v2;
    rc = SQLITE_OK;
    nExtract = 0;
    if (pExpr == (uintptr)0 || libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) != (int32)TK_SELECT) {
        alloc.FpParse = pParse;
        alloc.FpIdx = pIdx;
        alloc.FppRec = ppRec;
        i = 0;
        for (;;) {
            if (!(i < nElem)) {
                break;
            }
            pVal = (uintptr)0;
            if (pExpr != 0) {
                v2 = _sqlite3VectorFieldSubexpr(ref (tls).DerefOrNull(), pExpr, i);
            } else {
                v2 = (uintptr)0;
            }
            pElem = v2;
            aff = libc.Uint8FromInt8(_sqlite3IndexColumnAffinity(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pIdx, iVal + i));
            alloc.FiVal = iVal + i;
            rc = _stat4ValueFromExpr(tls, pParse, pElem, aff, @unsafe.Pointer.FromPinnedBox(Ꮡalloc), @unsafe.Pointer.FromBox(ᏑpVal));
            if (!(pVal != 0)) {
                break;
            }
            nExtract = nExtract + 1;
            goto _1;
_1:;
            i = i + 1;
        }
    }
    (@unsafe.Pointer.To<int32>(pnExtract)).Value = nExtract;
    return rc;
}

internal static Tu8 /*r*/ _sqlite3StrIHash(ref libc.TLS tls, uintptr z) {
    Tu8 h = default;
    _ = h;
    h = (uint8)0;
    if (z == (uintptr)0) {
        return (uint8)0;
    }
    while (~@unsafe.Pointer.To<int8>(__ccgo_ptr(z)) != 0) {
        h = libc.Uint8FromInt32((int32)h + libc.Int32FromUint8(_sqlite3UpperToLower[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z)))]));
        z = z + 1;
    }
    return h;
}

internal static void _sqlite3ValueApplyAffinity(ж<libc.TLS> tls, uintptr pVal, Tu8 affinity, Tu8 enc) {
    _applyAffinity(tls, pVal, libc.Int8FromUint8(affinity), enc);
}

internal static int32 /*r*/ _sqlite3VdbeMemFromBtree(ж<libc.TLS> tls, uintptr pCur, Tu32 offset, Tu32 amt, uintptr pMem) {
    int32 rc = default;
    int32 v1 = default;
    _ = rc;
    _ = v1;
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = (uint16)MEM_Null;
    if (amt >= (uint32)SQLITE_MAX_ALLOCATION_SIZE) {
        return (int32)SQLITE_NOMEM;
    }
    if ((uint64)amt + (uint64)offset > libc.Uint64FromInt64(_sqlite3BtreeMaxRecordSize(ref (tls).DerefOrNull(), pCur))) {
        return _sqlite3CorruptError(tls, (int32)87082);
    }
    v1 = _sqlite3VdbeMemClearAndResize(tls, pMem, libc.Int32FromUint32(amt + (uint32)1));
    rc = v1;
    if (SQLITE_OK == v1) {
        rc = _sqlite3BtreePayload(tls, pCur, offset, amt, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz);
        if (rc == SQLITE_OK) {
            (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz + (uintptr)amt))).Value = 0;
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = (uint16)MEM_Blob;
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn = libc.Int32FromUint32(amt);
        } else {
            _sqlite3VdbeMemRelease(tls, pMem);
        }
    }
    return rc;
}

internal static uint32 /*r*/ _strHash(ref libc.TLS tls, uintptr z) {
    uint32 h = default;
    uintptr v1 = default;
    _ = h;
    _ = v1;
    h = (uint32)0;
    while (~@unsafe.Pointer.To<int8>(__ccgo_ptr(z)) != 0) {
        v1 = z;
        z = z + 1;
        h = h + libc.Uint32FromInt32((int32)0xdf & libc.Int32FromUint8(libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(v1)))));
        h = h * (uint32)0x9e3779b1U;
    }
    return h;
}

internal static int32 /*r*/ _termIsEquivalence(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, uintptr pSrc) {
    int8 aff1 = default;
    int8 aff2 = default;
    _ = aff1;
    _ = aff2;
    if (!((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_Transitive))) == libc.Uint32FromInt32(0))) {
        return 0;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) != (int32)TK_EQ && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) != (int32)TK_IS) {
        return 0;
    }
    if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_OuterON) | libc.Int32FromInt32(EP_Collate))) != (uint32)0) {
        return 0;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_IS && (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc >= (int32)2 && (libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pSrc + 8))).Value.Ffg.Fjointype) & (int32)JT_LTORJ) != 0) {
        return 0;
    }
    aff1 = _sqlite3ExprAffinity(tls, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft);
    aff2 = _sqlite3ExprAffinity(tls, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight);
    if ((int32)aff1 != (int32)aff2 && (!((int32)aff1 >= libc.Int32FromInt32(SQLITE_AFF_NUMERIC)) || !((int32)aff2 >= libc.Int32FromInt32(SQLITE_AFF_NUMERIC)))) {
        return 0;
    }
    if (!(_sqlite3ExprCollSeqMatch(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight) != 0)) {
        return 0;
    }
    return (int32)1;
}

internal static uintptr /*r*/ _triggerSpanDup(ж<libc.TLS> tls, uintptr db, uintptr zStart, uintptr zEnd) {
    int32 i = default;
    uintptr z = default;
    _ = i;
    _ = z;
    z = _sqlite3DbSpanDup(tls, db, zStart, zEnd);
    if (z != 0) {
        i = 0;
        for (;;) {
            if (!(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i)) != 0)) {
                break;
            }
            if ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i)))]) & (int32)0x01) != 0) {
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i))).Value = (int8)' ';
            }
            goto _1;
_1:;
            i = i + 1;
        }
    }
    return z;
}

internal static void _upperFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    int32 i = default;
    int32 n = default;
    uintptr z1 = default;
    uintptr z2 = default;
    _ = i;
    _ = n;
    _ = z1;
    _ = z2;
    _ = argc;
    z2 = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    n = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    if (z2 != 0) {
        z1 = _contextMalloc(tls, context, (int64)n + (int64)1);
        if (z1 != 0) {
            i = 0;
            for (;;) {
                if (!(i < n)) {
                    break;
                }
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(z1 + (uintptr)i))).Value = (int8)((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z2 + (uintptr)i))) & ~(libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z2 + (uintptr)i)))]) & libc.Int32FromInt32(0x20)));
                goto _1;
_1:;
                i = i + 1;
            }
            Xsqlite3_result_text(tls, context, z1, n, __ccgo_fp(Xsqlite3_free));
        }
    }
}

} // end sqlite3_package
