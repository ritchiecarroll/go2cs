// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

internal static @unsafe.Pointer mapaccess1_faststr(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @string kyʗp) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var h = ref Ꮡh.DerefOrNull();

    ref var ky = ref heap(kyʗp, out var Ꮡky);
    if (raceenabled && Ꮡh != nil) {
        var callerpc = getcallerpc();
        racereadpc(@unsafe.Pointer.FromPinnedBox(Ꮡh), callerpc, abi.FuncPCABIInternal(mapaccess1_faststr));
    }
    if (Ꮡh == nil || h.count == 0) {
        return @unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0));
    }
    if ((uint8)(h.flags & (uint8)hashWriting) != 0) {
        fatal(concurrentMapReadAndMapˢ);
    }
    var key = stringStructOf(Ꮡky);
    if (h.B == 0) {
        // One-bucket table.
        var bΔ1 = (ж<bmap>)(uintptr)(h.buckets);
        if ((~key).len < 32) {
            // short key, doing lots of comparisons is ok
            for ((var i, @unsafe.Pointer kptr) = ((uintptr)0, (uintptr)bΔ1.keys()); i < abi.MapBucketCount; (i, kptr) = (i + 1, (uintptr)add(kptr, 2 * goarch.PtrSize))) {
                var k = (ж<stringStruct>)(uintptr)(kptr);
                if ((~k).len != (~key).len || isEmpty((~bΔ1).tophash[(nint)(i)])) {
                    if ((~bΔ1).tophash[(nint)(i)] == emptyRest) {
                        break;
                    }
                    continue;
                }
                if ((~k).str == (~key).str || memequal((~k).str, (~key).str, (uintptr)(~key).len)) {
                    return (uintptr)add(@unsafe.Pointer.FromPinnedBox(bΔ1), (uintptr)(dataOffset + (uintptr)(abi.MapBucketCount * 2 * goarch.PtrSize)) + i * (uintptr)t.ValueSize);
                }
            }
            return @unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0));
        }
        // long key, try not to do more comparisons than necessary
        var keymaybe = (uintptr)abi.MapBucketCount;
        for ((var i, @unsafe.Pointer kptr) = ((uintptr)0, (uintptr)bΔ1.keys()); i < abi.MapBucketCount; (i, kptr) = (i + 1, (uintptr)add(kptr, 2 * goarch.PtrSize))) {
            var k = (ж<stringStruct>)(uintptr)(kptr);
            if ((~k).len != (~key).len || isEmpty((~bΔ1).tophash[(nint)(i)])) {
                if ((~bΔ1).tophash[(nint)(i)] == emptyRest) {
                    break;
                }
                continue;
            }
            if ((~k).str == (~key).str) {
                return (uintptr)add(@unsafe.Pointer.FromPinnedBox(bΔ1), (uintptr)(dataOffset + (uintptr)(abi.MapBucketCount * 2 * goarch.PtrSize)) + i * (uintptr)t.ValueSize);
            }
            // check first 4 bytes
            if (((ж<array<byte>>)(uintptr)((~key).str)).Value != ((ж<array<byte>>)(uintptr)((~k).str)).Value) {
                continue;
            }
            // check last 4 bytes
            if (((ж<array<byte>>)(uintptr)(add((~key).str, (uintptr)(~key).len - 4))).Value != ((ж<array<byte>>)(uintptr)(add((~k).str, (uintptr)(~key).len - 4))).Value) {
                continue;
            }
            if (keymaybe != abi.MapBucketCount) {
                // Two keys are potential matches. Use hash to distinguish them.
                goto dohash;
            }
            keymaybe = i;
        }
        if (keymaybe != abi.MapBucketCount) {
            var k = (ж<stringStruct>)(uintptr)(add(@unsafe.Pointer.FromPinnedBox(bΔ1), dataOffset + keymaybe * 2 * (uintptr)goarch.PtrSize));
            if (memequal((~k).str, (~key).str, (uintptr)(~key).len)) {
                return (uintptr)add(@unsafe.Pointer.FromPinnedBox(bΔ1), (uintptr)(dataOffset + (uintptr)(abi.MapBucketCount * 2 * goarch.PtrSize)) + keymaybe * (uintptr)t.ValueSize);
            }
        }
        return @unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0));
    }
dohash:
    var hash = t.Hasher((uintptr)noescape(@unsafe.Pointer.FromPinnedBox(Ꮡky)), (uintptr)h.hash0);
    System.GC.KeepAlive(Ꮡky);
    var m = bucketMask(h.B);
    var b = (ж<bmap>)(uintptr)(add(h.buckets, ((uintptr)(hash & m)) * (uintptr)t.BucketSize));
    {
        @unsafe.Pointer c = h.oldbuckets; if (c != nil) {
            if (!h.sameSizeGrow()) {
                // There used to be half as many buckets; mask down one more power of two.
                m >>= (int)(1);
            }
            var oldb = (ж<bmap>)(uintptr)(add(c, ((uintptr)(hash & m)) * (uintptr)t.BucketSize));
            if (!evacuated(ref (oldb).DerefOrNull())) {
                b = oldb;
            }
        }
    }
    var top = tophash(hash);
    for (; b != nil; b = b.overflow(Ꮡt)) {
        for ((var i, @unsafe.Pointer kptr) = ((uintptr)0, (uintptr)b.keys()); i < abi.MapBucketCount; (i, kptr) = (i + 1, (uintptr)add(kptr, 2 * goarch.PtrSize))) {
            var k = (ж<stringStruct>)(uintptr)(kptr);
            if ((~k).len != (~key).len || (~b).tophash[(nint)(i)] != top) {
                continue;
            }
            if ((~k).str == (~key).str || memequal((~k).str, (~key).str, (uintptr)(~key).len)) {
                return (uintptr)add(@unsafe.Pointer.FromPinnedBox(b), (uintptr)(dataOffset + (uintptr)(abi.MapBucketCount * 2 * goarch.PtrSize)) + i * (uintptr)t.ValueSize);
            }
        }
    }
    return @unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0));
}

// mapaccess2_faststr should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapaccess2_faststr
internal static (@unsafe.Pointer, bool) mapaccess2_faststr(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @string kyʗp) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var h = ref Ꮡh.DerefOrNull();

    ref var ky = ref heap(kyʗp, out var Ꮡky);
    if (raceenabled && Ꮡh != nil) {
        var callerpc = getcallerpc();
        racereadpc(@unsafe.Pointer.FromPinnedBox(Ꮡh), callerpc, abi.FuncPCABIInternal(mapaccess2_faststr));
    }
    if (Ꮡh == nil || h.count == 0) {
        return (@unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0)), false);
    }
    if ((uint8)(h.flags & (uint8)hashWriting) != 0) {
        fatal(concurrentMapReadAndMapˢ);
    }
    var key = stringStructOf(Ꮡky);
    if (h.B == 0) {
        // One-bucket table.
        var bΔ1 = (ж<bmap>)(uintptr)(h.buckets);
        if ((~key).len < 32) {
            // short key, doing lots of comparisons is ok
            for ((var i, @unsafe.Pointer kptr) = ((uintptr)0, (uintptr)bΔ1.keys()); i < abi.MapBucketCount; (i, kptr) = (i + 1, (uintptr)add(kptr, 2 * goarch.PtrSize))) {
                var k = (ж<stringStruct>)(uintptr)(kptr);
                if ((~k).len != (~key).len || isEmpty((~bΔ1).tophash[(nint)(i)])) {
                    if ((~bΔ1).tophash[(nint)(i)] == emptyRest) {
                        break;
                    }
                    continue;
                }
                if ((~k).str == (~key).str || memequal((~k).str, (~key).str, (uintptr)(~key).len)) {
                    return ((uintptr)add(@unsafe.Pointer.FromPinnedBox(bΔ1), (uintptr)(dataOffset + (uintptr)(abi.MapBucketCount * 2 * goarch.PtrSize)) + i * (uintptr)t.ValueSize), true);
                }
            }
            return (@unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0)), false);
        }
        // long key, try not to do more comparisons than necessary
        var keymaybe = (uintptr)abi.MapBucketCount;
        for ((var i, @unsafe.Pointer kptr) = ((uintptr)0, (uintptr)bΔ1.keys()); i < abi.MapBucketCount; (i, kptr) = (i + 1, (uintptr)add(kptr, 2 * goarch.PtrSize))) {
            var k = (ж<stringStruct>)(uintptr)(kptr);
            if ((~k).len != (~key).len || isEmpty((~bΔ1).tophash[(nint)(i)])) {
                if ((~bΔ1).tophash[(nint)(i)] == emptyRest) {
                    break;
                }
                continue;
            }
            if ((~k).str == (~key).str) {
                return ((uintptr)add(@unsafe.Pointer.FromPinnedBox(bΔ1), (uintptr)(dataOffset + (uintptr)(abi.MapBucketCount * 2 * goarch.PtrSize)) + i * (uintptr)t.ValueSize), true);
            }
            // check first 4 bytes
            if (((ж<array<byte>>)(uintptr)((~key).str)).Value != ((ж<array<byte>>)(uintptr)((~k).str)).Value) {
                continue;
            }
            // check last 4 bytes
            if (((ж<array<byte>>)(uintptr)(add((~key).str, (uintptr)(~key).len - 4))).Value != ((ж<array<byte>>)(uintptr)(add((~k).str, (uintptr)(~key).len - 4))).Value) {
                continue;
            }
            if (keymaybe != abi.MapBucketCount) {
                // Two keys are potential matches. Use hash to distinguish them.
                goto dohash;
            }
            keymaybe = i;
        }
        if (keymaybe != abi.MapBucketCount) {
            var k = (ж<stringStruct>)(uintptr)(add(@unsafe.Pointer.FromPinnedBox(bΔ1), dataOffset + keymaybe * 2 * (uintptr)goarch.PtrSize));
            if (memequal((~k).str, (~key).str, (uintptr)(~key).len)) {
                return ((uintptr)add(@unsafe.Pointer.FromPinnedBox(bΔ1), (uintptr)(dataOffset + (uintptr)(abi.MapBucketCount * 2 * goarch.PtrSize)) + keymaybe * (uintptr)t.ValueSize), true);
            }
        }
        return (@unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0)), false);
    }
dohash:
    var hash = t.Hasher((uintptr)noescape(@unsafe.Pointer.FromPinnedBox(Ꮡky)), (uintptr)h.hash0);
    System.GC.KeepAlive(Ꮡky);
    var m = bucketMask(h.B);
    var b = (ж<bmap>)(uintptr)(add(h.buckets, ((uintptr)(hash & m)) * (uintptr)t.BucketSize));
    {
        @unsafe.Pointer c = h.oldbuckets; if (c != nil) {
            if (!h.sameSizeGrow()) {
                // There used to be half as many buckets; mask down one more power of two.
                m >>= (int)(1);
            }
            var oldb = (ж<bmap>)(uintptr)(add(c, ((uintptr)(hash & m)) * (uintptr)t.BucketSize));
            if (!evacuated(ref (oldb).DerefOrNull())) {
                b = oldb;
            }
        }
    }
    var top = tophash(hash);
    for (; b != nil; b = b.overflow(Ꮡt)) {
        for ((var i, @unsafe.Pointer kptr) = ((uintptr)0, (uintptr)b.keys()); i < abi.MapBucketCount; (i, kptr) = (i + 1, (uintptr)add(kptr, 2 * goarch.PtrSize))) {
            var k = (ж<stringStruct>)(uintptr)(kptr);
            if ((~k).len != (~key).len || (~b).tophash[(nint)(i)] != top) {
                continue;
            }
            if ((~k).str == (~key).str || memequal((~k).str, (~key).str, (uintptr)(~key).len)) {
                return ((uintptr)add(@unsafe.Pointer.FromPinnedBox(b), (uintptr)(dataOffset + (uintptr)(abi.MapBucketCount * 2 * goarch.PtrSize)) + i * (uintptr)t.ValueSize), true);
            }
        }
    }
    return (@unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0)), false);
}

// mapassign_faststr should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//   - github.com/cloudwego/frugal
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapassign_faststr
internal static @unsafe.Pointer mapassign_faststr(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @string sʗp) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var h = ref Ꮡh.DerefOrNull();

    ref var s = ref heap(sʗp, out var Ꮡs);
    if (Ꮡh == nil) {
        throw panic(((plainError)(@string)assignmentToEntryInNilˢ));
    }
    if (raceenabled) {
        var callerpc = getcallerpc();
        racewritepc(@unsafe.Pointer.FromPinnedBox(Ꮡh), callerpc, abi.FuncPCABIInternal(mapassign_faststr));
    }
    if ((uint8)(h.flags & (uint8)hashWriting) != 0) {
        fatal(concurrentMapWritesˢ);
    }
    var key = stringStructOf(Ꮡs);
    var hash = t.Hasher((uintptr)noescape(@unsafe.Pointer.FromPinnedBox(Ꮡs)), (uintptr)h.hash0);
    System.GC.KeepAlive(Ꮡs);
    // Set hashWriting after calling t.hasher for consistency with mapassign.
    h.flags ^= (uint8)(hashWriting);
    if (h.buckets == nil) {
        h.buckets = (uintptr)newobject(t.Bucket); // newarray(t.bucket, 1)
    }
again:
    var bucket = (uintptr)(hash & bucketMask(h.B));
    if (h.growing()) {
        growWork_faststr(Ꮡt, Ꮡh, bucket);
    }
    var b = (ж<bmap>)(uintptr)(add(h.buckets, bucket * (uintptr)t.BucketSize));
    var top = tophash(hash);
    ж<bmap> insertb = default!;
    uintptr inserti = default!;
    @unsafe.Pointer insertk = default!;
bucketloop:
    while (ᐧ) {
        for (var i = (uintptr)0; i < abi.MapBucketCount; i++) {
            if ((~b).tophash[(nint)(i)] != top) {
                if (isEmpty((~b).tophash[(nint)(i)]) && insertb == nil) {
                    insertb = b;
                    inserti = i;
                }
                if ((~b).tophash[(nint)(i)] == emptyRest) {
                    goto break_bucketloop;
                }
                continue;
            }
            var k = (ж<stringStruct>)(uintptr)(add(@unsafe.Pointer.FromPinnedBox(b), dataOffset + i * 2 * (uintptr)goarch.PtrSize));
            if ((~k).len != (~key).len) {
                continue;
            }
            if ((~k).str != (~key).str && !memequal((~k).str, (~key).str, (uintptr)(~key).len)) {
                continue;
            }
            // already have a mapping for key. Update it.
            inserti = i;
            insertb = b;
            // Overwrite existing key, so it can be garbage collected.
            // The size is already guaranteed to be set correctly.
            k.Value.str = key.Value.str;
            goto done;
        }
        var ovf = b.overflow(Ꮡt);
        if (ovf == nil) {
            break;
        }
        b = ovf;
continue_bucketloop:;
    }
break_bucketloop:;
    // Did not find mapping for key. Allocate new cell & add entry.
    // If we hit the max load factor or we have too many overflow buckets,
    // and we're not already in the middle of growing, start growing.
    if (!h.growing() && (overLoadFactor(h.count + 1, h.B) || tooManyOverflowBuckets(h.noverflow, h.B))) {
        hashGrow(Ꮡt, ref (Ꮡh).DerefOrNull());
        goto again; // Growing the table invalidates everything, so try again
    }
    if (insertb == nil) {
        // The current bucket and all the overflow buckets connected to it are full, allocate a new one.
        insertb = h.newoverflow(Ꮡt, b);
        inserti = 0; // not necessary, but avoids needlessly spilling inserti
    }
    insertb.Value.tophash[(nint)((uintptr)(inserti & (uintptr)(abi.MapBucketCount - 1)))] = top; // mask inserti to avoid bounds checks
    insertk = (uintptr)add(@unsafe.Pointer.FromPinnedBox(insertb), dataOffset + inserti * 2 * (uintptr)goarch.PtrSize);
    // store new key at insert position
    ((ж<stringStruct>)(uintptr)(insertk)).Value = key.Value;
    h.count++;
done:
    @unsafe.Pointer elem = (uintptr)add(@unsafe.Pointer.FromPinnedBox(insertb), (uintptr)(dataOffset + (uintptr)(abi.MapBucketCount * 2 * goarch.PtrSize)) + inserti * (uintptr)t.ValueSize);
    if ((uint8)(h.flags & (uint8)hashWriting) == 0) {
        fatal(concurrentMapWritesˢ);
    }
    h.flags &= unchecked((uint8)~(uint8)(hashWriting));
    return elem;
}

internal static void mapdelete_faststr(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @string kyʗp) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var h = ref Ꮡh.DerefOrNull();

    ref var ky = ref heap(kyʗp, out var Ꮡky);
    if (raceenabled && Ꮡh != nil) {
        var callerpc = getcallerpc();
        racewritepc(@unsafe.Pointer.FromPinnedBox(Ꮡh), callerpc, abi.FuncPCABIInternal(mapdelete_faststr));
    }
    if (Ꮡh == nil || h.count == 0) {
        return;
    }
    if ((uint8)(h.flags & (uint8)hashWriting) != 0) {
        fatal(concurrentMapWritesˢ);
    }
    var key = stringStructOf(Ꮡky);
    var hash = t.Hasher((uintptr)noescape(@unsafe.Pointer.FromPinnedBox(Ꮡky)), (uintptr)h.hash0);
    System.GC.KeepAlive(Ꮡky);
    // Set hashWriting after calling t.hasher for consistency with mapdelete
    h.flags ^= (uint8)(hashWriting);
    var bucket = (uintptr)(hash & bucketMask(h.B));
    if (h.growing()) {
        growWork_faststr(Ꮡt, Ꮡh, bucket);
    }
    var b = (ж<bmap>)(uintptr)(add(h.buckets, bucket * (uintptr)t.BucketSize));
    var bOrig = b;
    var top = tophash(hash);
search:
    for (; b != nil; b = b.overflow(Ꮡt)) {
        for ((var i, @unsafe.Pointer kptr) = ((uintptr)0, (uintptr)b.keys()); i < abi.MapBucketCount; (i, kptr) = (i + 1, (uintptr)add(kptr, 2 * goarch.PtrSize))) {
            var k = (ж<stringStruct>)(uintptr)(kptr);
            if ((~k).len != (~key).len || (~b).tophash[(nint)(i)] != top) {
                continue;
            }
            if ((~k).str != (~key).str && !memequal((~k).str, (~key).str, (uintptr)(~key).len)) {
                continue;
            }
            // Clear key's pointer.
            k.Value.str = default!;
            @unsafe.Pointer e = (uintptr)add(@unsafe.Pointer.FromPinnedBox(b), (uintptr)(dataOffset + (uintptr)(abi.MapBucketCount * 2 * goarch.PtrSize)) + i * (uintptr)t.ValueSize);
            if (t.Elem.Pointers()){
                memclrHasPointers(e, (~t.Elem).Size_);
            } else {
                memclrNoHeapPointers(e, (~t.Elem).Size_);
            }
            b.Value.tophash[(nint)(i)] = emptyOne;
            // If the bucket now ends in a bunch of emptyOne states,
            // change those to emptyRest states.
            if (i == (uintptr)(abi.MapBucketCount - 1)){
                if (b.overflow(Ꮡt) != nil && (~b.overflow(Ꮡt)).tophash[0] != emptyRest) {
                    goto notLast;
                }
            } else {
                if ((~b).tophash[(nint)(i + 1)] != emptyRest) {
                    goto notLast;
                }
            }
            while (ᐧ) {
                b.Value.tophash[(nint)(i)] = emptyRest;
                if (i == 0){
                    if (b == bOrig) {
                        break; // beginning of initial bucket, we're done.
                    }
                    // Find previous bucket, continue at its last entry.
                    var c = b;
                    for (b = bOrig; b.overflow(Ꮡt) != c; b = b.overflow(Ꮡt)) {
                    }
                    i = abi.MapBucketCount - 1;
                } else {
                    i--;
                }
                if ((~b).tophash[(nint)(i)] != emptyOne) {
                    break;
                }
            }
notLast:
            h.count--;
            // Reset the hash seed to make it more difficult for attackers to
            // repeatedly trigger hash collisions. See issue 25237.
            if (h.count == 0) {
                h.hash0 = (uint32)rand();
            }
            goto break_search;
        }
continue_search:;
    }
break_search:;
    if ((uint8)(h.flags & (uint8)hashWriting) == 0) {
        fatal(concurrentMapWritesˢ);
    }
    h.flags &= unchecked((uint8)~(uint8)(hashWriting));
}

internal static void growWork_faststr(ж<maptype> Ꮡt, ж<hmap> Ꮡh, uintptr bucket) {
    ref var h = ref Ꮡh.DerefOrNull();

    // make sure we evacuate the oldbucket corresponding
    // to the bucket we're about to use
    evacuate_faststr(Ꮡt, Ꮡh, (uintptr)(bucket & h.oldbucketmask()));
    // evacuate one more oldbucket to make progress on growing
    if (h.growing()) {
        evacuate_faststr(Ꮡt, Ꮡh, h.nevacuate);
    }
}

internal static void evacuate_faststr(ж<maptype> Ꮡt, ж<hmap> Ꮡh, uintptr oldbucket) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var h = ref Ꮡh.DerefOrNull();

    var b = (ж<bmap>)(uintptr)(add(h.oldbuckets, oldbucket * (uintptr)t.BucketSize));
    var newbit = h.noldbuckets();
    if (!evacuated(ref (b).DerefOrNull())) {
        // TODO: reuse overflow buckets instead of using new ones, if there
        // is no iterator using the old buckets.  (If !oldIterator.)
        // xy contains the x and y (low and high) evacuation destinations.
        ref var xy = ref heap(new array<evacDst>(2), out var Ꮡxy);
        var x = Ꮡxy.at<evacDst>(0);
        x.Value.b = (ж<bmap>)(uintptr)(add(h.buckets, oldbucket * (uintptr)t.BucketSize));
        x.Value.k = (uintptr)add(@unsafe.Pointer.FromPinnedBox((~x).b), dataOffset);
        x.Value.e = (uintptr)add((~x).k, abi.MapBucketCount * 2 * goarch.PtrSize);
        if (!h.sameSizeGrow()) {
            // Only calculate y pointers if we're growing bigger.
            // Otherwise GC can see bad pointers.
            var y = Ꮡxy.at<evacDst>(1);
            y.Value.b = (ж<bmap>)(uintptr)(add(h.buckets, (oldbucket + newbit) * (uintptr)t.BucketSize));
            y.Value.k = (uintptr)add(@unsafe.Pointer.FromPinnedBox((~y).b), dataOffset);
            y.Value.e = (uintptr)add((~y).k, abi.MapBucketCount * 2 * goarch.PtrSize);
        }
        for (; b != nil; b = b.overflow(Ꮡt)) {
            @unsafe.Pointer k = (uintptr)add(@unsafe.Pointer.FromPinnedBox(b), dataOffset);
            @unsafe.Pointer e = (uintptr)add(k, abi.MapBucketCount * 2 * goarch.PtrSize);
            for (nint i = 0; i < abi.MapBucketCount; (i, k, e) = (i + 1, (uintptr)add(k, 2 * goarch.PtrSize), (uintptr)add(e, (uintptr)t.ValueSize))) {
                var top = (~b).tophash[i];
                if (isEmpty(top)) {
                    b.Value.tophash[i] = evacuatedEmpty;
                    continue;
                }
                if (top < minTopHash) {
                    @throw(badMapStateˢ);
                }
                uint8 useY = default!;
                if (!h.sameSizeGrow()) {
                    // Compute hash to make our evacuation decision (whether we need
                    // to send this key/elem to bucket x or bucket y).
                    var hash = t.Hasher(k, (uintptr)h.hash0);
                    if ((uintptr)(hash & newbit) != 0) {
                        useY = 1;
                    }
                }
                b.Value.tophash[i] = (uint8)((uint8)evacuatedX + useY); // evacuatedX + 1 == evacuatedY, enforced in makemap
                var dst = Ꮡxy.at<evacDst>((nint)(useY)); // evacuation destination
                if ((~dst).i == abi.MapBucketCount) {
                    dst.Value.b = h.newoverflow(Ꮡt, (~dst).b);
                    dst.Value.i = 0;
                    dst.Value.k = (uintptr)add(@unsafe.Pointer.FromPinnedBox((~dst).b), dataOffset);
                    dst.Value.e = (uintptr)add((~dst).k, abi.MapBucketCount * 2 * goarch.PtrSize);
                }
                dst.Value.b.Value.tophash[(nint)((~dst).i & (nint)(abi.MapBucketCount - 1))] = top; // mask dst.i as an optimization, to avoid a bounds check
                // Copy key.
                ((ж<@string>)(uintptr)((~dst).k)).Value = ((ж<@string>)(uintptr)(k)).Value;
                typedmemmove(t.Elem, (~dst).e, e);
                dst.Value.i++;
                // These updates might push these pointers past the end of the
                // key or elem arrays.  That's ok, as we have the overflow pointer
                // at the end of the bucket to protect against pointing past the
                // end of the bucket.
                dst.Value.k = (uintptr)add((~dst).k, 2 * goarch.PtrSize);
                dst.Value.e = (uintptr)add((~dst).e, (uintptr)t.ValueSize);
            }
        }
        // Unlink the overflow buckets & clear key/elem to help GC.
        if ((uint8)(h.flags & (uint8)oldIterator) == 0 && t.Bucket.Pointers()) {
            @unsafe.Pointer bΔ1 = (uintptr)add(h.oldbuckets, oldbucket * (uintptr)t.BucketSize);
            // Preserve b.tophash because the evacuation
            // state is maintained there.
            @unsafe.Pointer ptr = (uintptr)add(bΔ1, dataOffset);
            var n = (uintptr)t.BucketSize - dataOffset;
            memclrHasPointers(ptr, n);
        }
    }
    if (oldbucket == h.nevacuate) {
        advanceEvacuationMark(ref (Ꮡh).DerefOrNull(), ref (Ꮡt).DerefOrNull(), newbit);
    }
}

} // end runtime_package
