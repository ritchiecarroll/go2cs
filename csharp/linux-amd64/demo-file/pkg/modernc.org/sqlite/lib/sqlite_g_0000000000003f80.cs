namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static int32 /*r*/ Xsqlite3_open16(ж<libc.TLS> tls, uintptr zFilename, uintptr ppDb) {
    uintptr pVal = default;
    uintptr zFilename8 = default;
    int32 rc = default;
    Tu8 v1 = default;
    _ = pVal;
    _ = rc;
    _ = zFilename8;
    _ = v1;
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppDb))).Value = (uintptr)0;
    rc = Xsqlite3_initialize(tls);
    if (rc != 0) {
        return rc;
    }
    if (zFilename == (uintptr)0) {
        zFilename = __ccgo_ts + 26434;
    }
    pVal = _sqlite3ValueNew(tls, (uintptr)0);
    _sqlite3ValueSetStr(tls, pVal, -(int32)1, zFilename, (uint8)SQLITE_UTF16LE, libc.UintptrFromInt32(0));
    zFilename8 = _sqlite3ValueText(tls, pVal, (uint8)SQLITE_UTF8);
    if (zFilename8 != 0) {
        rc = _openDatabase(tls, zFilename8, (@unsafe.Pointer)ppDb, libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_OPEN_READWRITE) | libc.Int32FromInt32(SQLITE_OPEN_CREATE)), (uintptr)0);
        if (rc == SQLITE_OK && !((libc.Int32FromUint16((@unsafe.Pointer.To<TSchema>((@unsafe.Pointer)(@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppDb))))).Value.FaDb))).Value.FpSchema)).Value.FschemaFlags) & libc.Int32FromInt32(DB_SchemaLoaded)) == libc.Int32FromInt32(DB_SchemaLoaded))) {
            v1 = libc.Uint8FromInt32(SQLITE_UTF16LE);
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppDb))))).Value.Fenc = v1;
            (@unsafe.Pointer.To<TSchema>((@unsafe.Pointer)(@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppDb))))).Value.FaDb))).Value.FpSchema)).Value.Fenc = v1;
        }
    } else {
        rc = (int32)SQLITE_NOMEM;
    }
    _sqlite3ValueFree(tls, pVal);
    return rc & (int32)0xff;
}

internal static UntypedInt __BYTE_ORDER => 1234;

internal static void _checkList(ж<libc.TLS> tls, uintptr pCheck, int32 isFreeList, TPgno iPage, Tu32 N) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        Tu32 expected = default;
        Tu32 n = default;
        int32 i = default;
        int32 nErrAtStart = default;
        TPgno iFreePage = default;
        uintptr pOvflData = default;
        uintptr v2 = default;
        uintptr _ᴛ1 = default;
        _ = expected;
        _ = i;
        _ = iFreePage;
        _ = n;
        _ = nErrAtStart;
        _ = pOvflData;
        _ = v2;
        expected = N;
        nErrAtStart = (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FnErr;
        while (iPage != (uint32)0 && (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FmxErr != 0) {
            if (_checkRef(tls, pCheck, iPage) != 0) {
                break;
            }
            N = N - 1;
            if (_sqlite3PagerGet(tls, (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FpPager, iPage, bp, 0) != 0) {
                _checkAppendMsg(tls, pCheck, __ccgo_ts + 4527, libc.VaList(bp + 16, iPage));
                break;
            }
            pOvflData = _sqlite3PagerGetData(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
            if (isFreeList != 0) {
                n = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pOvflData + 4));
                if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FpBt)).Value.FautoVacuum != 0) {
                    _checkPtrmap(tls, pCheck, iPage, (uint8)PTRMAP_FREEPAGE, (uint32)0);
                }
                if (n > (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FpBt)).Value.FusableSize / (uint32)4 - (uint32)2) {
                    _checkAppendMsg(tls, pCheck, __ccgo_ts + 4549, libc.VaList(bp + 16, iPage));
                    N = N - 1;
                } else {
                    i = 0;
                    for (;;) {
                        if (!(i < libc.Int32FromUint32(n))) {
                            break;
                        }
                        iFreePage = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pOvflData + (uintptr)((int32)8 + i * (int32)4)));
                        if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FpBt)).Value.FautoVacuum != 0) {
                            _checkPtrmap(tls, pCheck, iFreePage, (uint8)PTRMAP_FREEPAGE, (uint32)0);
                        }
                        _checkRef(tls, pCheck, iFreePage);
                        goto _1;
_1:;
                        i = i + 1;
                    }
                    N = N - n;
                }
            } else {
                if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FpBt)).Value.FautoVacuum != 0 && N > (uint32)0) {
                    i = libc.Int32FromUint32(_sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)pOvflData));
                    _checkPtrmap(tls, pCheck, libc.Uint32FromInt32(i), (uint8)PTRMAP_OVERFLOW2, iPage);
                }
            }
            iPage = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)pOvflData);
            _sqlite3PagerUnref(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
        }
        if (N != 0 && nErrAtStart == (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FnErr) {
            if (isFreeList != 0) {
                v2 = __ccgo_ts + 4588;
            } else {
                v2 = __ccgo_ts + 4593;
            }
            _checkAppendMsg(tls, pCheck, __ccgo_ts + 4614, libc.VaList(bp + 16, v2, expected - N, expected));
        }
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
            if (_sqlite3StrICmp(ref (tls).DerefOrNull(), zDate, __ccgo_ts + 1248) == 0 && _sqlite3NotPureFunc(tls, context) != 0) {
                return _setDateTimeToCurrent(tls, context, p);
            } else {
                if (_sqlite3AtoF(tls, zDate, @unsafe.Pointer.FromPinnedBox(Ꮡr)) > 0) {
                    _setRawDateNumber(ref (tls).DerefOrNull(), p, r);
                    return 0;
                } else {
                    if ((_sqlite3StrICmp(ref (tls).DerefOrNull(), zDate, __ccgo_ts + 1252) == 0 || _sqlite3StrICmp(ref (tls).DerefOrNull(), zDate, __ccgo_ts + 1259) == 0) && _sqlite3NotPureFunc(tls, context) != 0) {
                        libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(1), 2, 0x4);
                        return _setDateTimeToCurrent(tls, context, p);
                    }
                }
            }
        }
    }
    return (int32)1;
}

} // end sqlite3_package
