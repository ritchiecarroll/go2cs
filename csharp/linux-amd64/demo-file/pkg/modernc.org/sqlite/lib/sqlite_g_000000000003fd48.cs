namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static void _fts5Bm25Function(ж<libc.TLS> tls, uintptr pApi, uintptr pFts, uintptr pCtx, int32 nVal, uintptr apVal) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        float64 D = default;
        float64 b = default;
        float64 k1 = default;
        float64 score = default;
        float64 w = default;
        float64 v2 = default;
        uintptr aFreq = default;
        ref var pData = ref heap(new uintptr(), out var ᏑpData);
        int32 i = default;
        int32 rc = default;
        int32 _ᴛ1 = default;
        int32 _ᴛ2 = default;
        int32 _ᴛ3 = default;
        int32 _ᴛ4 = default;
        int32 _ᴛ5 = default;
        _ = D;
        _ = aFreq;
        _ = b;
        _ = i;
        _ = k1;
        _ = pData;
        _ = rc;
        _ = score;
        _ = w;
        _ = v2;
        k1 = (float64)1.2D;
        b = (float64)0.75D;
        score = (float64)0D;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = 0;
        D = (float64)0D;
        aFreq = (uintptr)0;
        rc = _fts5Bm25GetData(tls, pApi, pFts, @unsafe.Pointer.FromBox(ᏑpData));
        if (rc == SQLITE_OK) {
            aFreq = (@unsafe.Pointer.To<TFts5Bm25Data>((@unsafe.Pointer)pData)).Value.FaFreq;
            libc.Xmemset(tls, aFreq, 0, (uint64)8 * libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5Bm25Data>((@unsafe.Pointer)pData)).Value.FnPhrase));
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32>>((@unsafe.Pointer.To<TFts5ExtensionApi>((@unsafe.Pointer)pApi)).Value.FxInstCount)(tls, pFts, bp);
        }
        i = 0;
        for (;;) {
            if (!(rc == SQLITE_OK && i < ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)))) {
                break;
            }
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, uintptr, uintptr, int32>>((@unsafe.Pointer.To<TFts5ExtensionApi>((@unsafe.Pointer)pApi)).Value.FxInst)(tls, pFts, i, bp + 4, bp + 8, bp + 12);
            if (rc == SQLITE_OK) {
                if (nVal > ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8))) {
                    v2 = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apVal + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8))) * 8)));
                } else {
                    v2 = (float64)1D;
                }
                w = v2;
                (@unsafe.Pointer.To<float64>(__ccgo_ptr(aFreq + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 4))) * 8))).Value += w;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        if (rc == SQLITE_OK) {
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32>>((@unsafe.Pointer.To<TFts5ExtensionApi>((@unsafe.Pointer)pApi)).Value.FxColumnSize)(tls, pFts, -(int32)1, bp + 16);
            D = (float64)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 16)));
        }
        if (rc == SQLITE_OK) {
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TFts5Bm25Data>((@unsafe.Pointer)pData)).Value.FnPhrase)) {
                    break;
                }
                score = score + (float64)(~@unsafe.Pointer.To<float64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Bm25Data>((@unsafe.Pointer)pData)).Value.FaIDF + (uintptr)i * 8)) * ((float64)(~@unsafe.Pointer.To<float64>(__ccgo_ptr(aFreq + (uintptr)i * 8)) * (k1 + libc.Float64FromFloat64(1D))) / (~@unsafe.Pointer.To<float64>(__ccgo_ptr(aFreq + (uintptr)i * 8)) + (float64)(k1 * (libc.Float64FromInt32(1) - b + (float64)(b * D) / (@unsafe.Pointer.To<TFts5Bm25Data>((@unsafe.Pointer)pData)).Value.Favgdl)))));
                goto _3;
_3:;
                i = i + 1;
            }
            Xsqlite3_result_double(tls, pCtx, (float64)(-libc.Float64FromFloat64(1D) * score));
        } else {
            Xsqlite3_result_error_code(tls, pCtx, rc);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _geopolyOverlap(ж<libc.TLS> tls, uintptr p1, uintptr p2) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 iMask = default;
        int32 needSort = default;
        int32 rc = default;
        int32 v1 = default;
        Tsqlite3_int64 nByte = default;
        Tsqlite3_int64 nVertex = default;
        uintptr p = default;
        uintptr pActive = default;
        uintptr pPrev = default;
        uintptr pSeg = default;
        uintptr pThisEvent = default;
        uintptr v5 = default;
        float64 rX = default;
        float64 y = default;
        float64 v2 = default;
        array<uint8> _ᴛ1 = new(4);
        _ = iMask;
        _ = nByte;
        _ = nVertex;
        _ = needSort;
        _ = p;
        _ = pActive;
        _ = pPrev;
        _ = pSeg;
        _ = pThisEvent;
        _ = rX;
        _ = rc;
        _ = y;
        _ = v1;
        _ = v2;
        _ = v5;
        nVertex = (int64)((@unsafe.Pointer.To<TGeoPoly>((@unsafe.Pointer)p1)).Value.FnVertex + (@unsafe.Pointer.To<TGeoPoly>((@unsafe.Pointer)p2)).Value.FnVertex + (int32)2);
        rc = 0;
        needSort = 0;
        pActive = (uintptr)0;
        nByte = libc.Int64FromUint64((uint64)32 * libc.Uint64FromInt64(nVertex) * (uint64)2 + (uint64)48 * libc.Uint64FromInt64(nVertex) + (uint64)24);
        p = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte));
        if (p == (uintptr)0) {
            r = -(int32)1; goto ᒐdone;
        }
        (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FaEvent = p + 1 * 24;
        (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FaSegment = (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FaEvent + (uintptr)(nVertex * (int64)2) * 32;
        v1 = libc.Int32FromInt32(0);
        (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FnSegment = v1;
        (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FnEvent = v1;
        _geopolyAddSegments(ref (tls).DerefOrNull(), p, p1, (uint8)1);
        _geopolyAddSegments(ref (tls).DerefOrNull(), p, p2, (uint8)2);
        pThisEvent = _geopolySortEventsByX(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FaEvent, (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FnEvent);
        if (pThisEvent != 0 && (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pThisEvent)).Value.Fx == (float64)0D) {
            v2 = -libc.Float64FromFloat64(1D);
        } else {
            v2 = (float64)0D;
        }
        rX = v2;
        libc.Xmemset(tls, bp, 0, (uint64)4);
        while (pThisEvent != 0) {
            if ((@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pThisEvent)).Value.Fx != rX) {
                pPrev = (uintptr)0;
                iMask = 0;
                rX = (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pThisEvent)).Value.Fx;
                if (needSort != 0) {
                    pActive = _geopolySortSegmentsByYAndC(ref (tls).DerefOrNull(), pActive);
                    needSort = 0;
                }
                pSeg = pActive;
                for (;;) {
                    if (!(pSeg != 0)) {
                        break;
                    }
                    if (pPrev != 0) {
                        if ((@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pPrev)).Value.Fy != (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.Fy) {
                            @unsafe.Pointer.ElementRef<uint8>(__ccgo_ptr(bp), iMask, 4) = (uint8)1;
                        }
                    }
                    iMask = iMask ^ libc.Int32FromUint8((@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.Fside);
                    pPrev = pSeg;
                    goto _3;
_3:;
                    pSeg = (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.FpNext;
                }
                pPrev = (uintptr)0;
                pSeg = pActive;
                for (;;) {
                    if (!(pSeg != 0)) {
                        break;
                    }
                    y = (float64)((@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.FC * rX) + (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.FB;
                    (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.Fy = y;
                    if (pPrev != 0) {
                        if ((@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pPrev)).Value.Fy > (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.Fy && libc.Int32FromUint8((@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pPrev)).Value.Fside) != libc.Int32FromUint8((@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.Fside)) {
                            rc = (int32)1;
                            goto geopolyOverlapDone;
                        } else {
                            if ((@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pPrev)).Value.Fy != (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.Fy) {
                                @unsafe.Pointer.ElementRef<uint8>(__ccgo_ptr(bp), iMask, 4) = (uint8)1;
                            }
                        }
                    }
                    iMask = iMask ^ libc.Int32FromUint8((@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.Fside);
                    pPrev = pSeg;
                    goto _4;
_4:;
                    pSeg = (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.FpNext;
                }
            }
            if ((@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pThisEvent)).Value.FeType == 0) {
                pSeg = (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pThisEvent)).Value.FpSeg;
                (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.Fy = (float64)(@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.Fy0;
                (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.FpNext = pActive;
                pActive = pSeg;
                needSort = (int32)1;
            } else {
                if (pActive == (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pThisEvent)).Value.FpSeg) {
                    if (pActive != 0) {
                        v5 = (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pActive)).Value.FpNext;
                    } else {
                        v5 = (uintptr)0;
                    }
                    pActive = v5;
                } else {
                    pSeg = pActive;
                    for (;;) {
                        if (!(pSeg != 0)) {
                            break;
                        }
                        if ((@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.FpNext == (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pThisEvent)).Value.FpSeg) {
                            if ((@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.FpNext != 0) {
                                v5 = (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)(@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.FpNext)).Value.FpNext;
                            } else {
                                v5 = (uintptr)0;
                            }
                            (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.FpNext = v5;
                            break;
                        }
                        goto _6;
_6:;
                        pSeg = (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.FpNext;
                    }
                }
            }
            pThisEvent = (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pThisEvent)).Value.FpNext;
        }
        if (libc.Int32FromUint8(@unsafe.Pointer.ElementRef<uint8>(__ccgo_ptr(bp), (int32)3, 4)) == 0) {
            rc = 0;
        } else {
            if (libc.Int32FromUint8(@unsafe.Pointer.ElementRef<uint8>(__ccgo_ptr(bp), (int32)1, 4)) != 0 && libc.Int32FromUint8(@unsafe.Pointer.ElementRef<uint8>(__ccgo_ptr(bp), (int32)2, 4)) == 0) {
                rc = (int32)3;
            } else {
                if (libc.Int32FromUint8(@unsafe.Pointer.ElementRef<uint8>(__ccgo_ptr(bp), (int32)1, 4)) == 0 && libc.Int32FromUint8(@unsafe.Pointer.ElementRef<uint8>(__ccgo_ptr(bp), (int32)2, 4)) != 0) {
                    rc = (int32)2;
                } else {
                    if (libc.Int32FromUint8(@unsafe.Pointer.ElementRef<uint8>(__ccgo_ptr(bp), (int32)1, 4)) == 0 && libc.Int32FromUint8(@unsafe.Pointer.ElementRef<uint8>(__ccgo_ptr(bp), (int32)2, 4)) == 0) {
                        rc = (int32)4;
                    } else {
                        rc = (int32)1;
                    }
                }
            }
        }
        goto geopolyOverlapDone;
geopolyOverlapDone:;
        Xsqlite3_free(tls, p);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

} // end sqlite3_package
