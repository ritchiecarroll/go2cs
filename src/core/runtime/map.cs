// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

// This file contains the implementation of Go's map type.
//
// A map is just a hash table. The data is arranged
// into an array of buckets. Each bucket contains up to
// 8 key/elem pairs. The low-order bits of the hash are
// used to select a bucket. Each bucket contains a few
// high-order bits of each hash to distinguish the entries
// within a single bucket.
//
// If more than 8 keys hash to a bucket, we chain on
// extra buckets.
//
// When the hashtable grows, we allocate a new array
// of buckets twice as big. Buckets are incrementally
// copied from the old bucket array to the new bucket array.
//
// Map iterators walk through the array of buckets and
// return the keys in walk order (bucket #, then overflow
// chain order, then bucket index).  To maintain iteration
// semantics, we never move keys within their bucket (if
// we did, keys might be returned 0 or 2 times).  When
// growing the table, iterators remain iterating through the
// old table and must check the new table if the bucket
// they are iterating through has been moved ("evacuated")
// to the new table.
// Picking loadFactor: too large and we have lots of overflow
// buckets, too small and we waste a lot of space. I wrote
// a simple program to check some stats for different loads:
// (64-bit, 8 byte keys and elems)
//  loadFactor    %overflow  bytes/entry     hitprobe    missprobe
//        4.00         2.13        20.77         3.00         4.00
//        4.50         4.05        17.30         3.25         4.50
//        5.00         6.85        14.77         3.50         5.00
//        5.50        10.55        12.94         3.75         5.50
//        6.00        15.27        11.67         4.00         6.00
//        6.50        20.90        10.79         4.25         6.50
//        7.00        27.14        10.15         4.50         7.00
//        7.50        34.03         9.73         4.75         7.50
//        8.00        41.10         9.40         5.00         8.00
//
// %overflow   = percentage of buckets which have an overflow bucket
// bytes/entry = overhead bytes used per key/elem pair
// hitprobe    = # of entries to check when looking up a present key
// missprobe   = # of entries to check when looking up an absent key
//
// Keep in mind this data is for maximally loaded tables, i.e. just
// before the table grows. Typical tables will be somewhat less loaded.
using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using Δmath = runtime.@internal.math_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

internal static UntypedInt bucketCntBits => /* abi.MapBucketCountBits */ 3;
internal static UntypedInt loadFactorDen => 2;
internal static UntypedInt loadFactorNum => /* loadFactorDen * abi.MapBucketCount * 13 / 16 */ 13;
internal static uintptr dataOffset => /* unsafe.Offsetof(struct {
	b	bmap
	v	int64
}{}.v) */ 8;
internal static UntypedInt emptyRest => 0; // this cell is empty, and there are no more non-empty cells at higher indexes or overflows.
internal static UntypedInt emptyOne => 1; // this cell is empty
internal static UntypedInt evacuatedX => 2; // key/elem is valid.  Entry has been evacuated to first half of larger table.
internal static UntypedInt evacuatedY => 3; // same as above, but evacuated to second half of larger table.
internal static UntypedInt evacuatedEmpty => 4; // cell is empty, bucket is evacuated.
internal static UntypedInt minTopHash => 5; // minimum tophash for a normal filled cell.
internal static UntypedInt iterator => 1; // there may be an iterator using buckets
internal static UntypedInt oldIterator => 2; // there may be an iterator using oldbuckets
internal static UntypedInt hashWriting => 4; // a goroutine is writing to the map
internal static UntypedInt ΔsameSizeGrow => 8; // the current map growth is to a new map of the same size
internal static UntypedInt noCheck => /* 1<<(8*goarch.PtrSize) - 1 */ 18446744073709551615;

// isEmpty reports whether the given tophash array entry represents an empty bucket entry.
internal static bool isEmpty(uint8 x) {
    return x <= emptyOne;
}

// A header for a Go map.
[GoType] partial struct hmap {
    // Note: the format of the hmap is also encoded in cmd/compile/internal/reflectdata/reflect.go.
    // Make sure this stays in sync with the compiler's definition.
    internal nint count; // # live cells == size of map.  Must be first (used by len() builtin)
    internal uint8 flags;
    public uint8 B;  // log_2 of # of buckets (can hold up to loadFactor * 2^B items)
    internal uint16 noverflow; // approximate number of overflow buckets; see incrnoverflow for details
    internal uint32 hash0; // hash seed
    internal @unsafe.Pointer buckets; // array of 2^B Buckets. may be nil if count==0.
    internal @unsafe.Pointer oldbuckets; // previous bucket array of half the size, non-nil only when growing
    internal uintptr nevacuate;        // progress counter for evacuation (buckets less than this have been evacuated)
    internal ж<mapextra> extra; // optional fields
}

// mapextra holds fields that are not present on all maps.
[GoType] partial struct mapextra {
    // If both key and elem do not contain pointers and are inline, then we mark bucket
    // type as containing no pointers. This avoids scanning such maps.
    // However, bmap.overflow is a pointer. In order to keep overflow buckets
    // alive, we store pointers to all overflow buckets in hmap.extra.overflow and hmap.extra.oldoverflow.
    // overflow and oldoverflow are only used if key and elem do not contain pointers.
    // overflow contains overflow buckets for hmap.buckets.
    // oldoverflow contains overflow buckets for hmap.oldbuckets.
    // The indirection allows to store a pointer to the slice in hiter.
    internal ж<slice<ж<bmap>>> overflow;
    internal ж<slice<ж<bmap>>> oldoverflow;
    // nextOverflow holds a pointer to a free overflow bucket.
    internal ж<bmap> nextOverflow;
}

// A bucket for a Go map.
[GoType] partial struct bmap {
    // tophash generally contains the top byte of the hash value
    // for each key in this bucket. If tophash[0] < minTopHash,
    // tophash[0] is a bucket evacuation state instead.
    internal array<uint8> tophash = new(abi.MapBucketCount);
}

// Followed by bucketCnt keys and then bucketCnt elems.
// NOTE: packing all the keys together and then all the elems together makes the
// code a bit more complicated than alternating key/elem/key/elem/... but it allows
// us to eliminate padding which would be needed for, e.g., map[int64]int8.
// Followed by an overflow pointer.

// A hash iteration structure.
// If you modify hiter, also change cmd/compile/internal/reflectdata/reflect.go
// and reflect/value.go to match the layout of this structure.
[GoType] partial struct hiter {
    internal @unsafe.Pointer key; // Must be in first position.  Write nil to indicate iteration end (see cmd/compile/internal/walk/range.go).
    internal @unsafe.Pointer elem; // Must be in second position (see cmd/compile/internal/walk/range.go).
    internal ж<maptype> t;
    internal ж<hmap> h;
    internal @unsafe.Pointer buckets; // bucket ptr at hash_iter initialization time
    internal ж<bmap> bptr;       // current bucket
    internal ж<slice<ж<bmap>>> overflow; // keeps overflow buckets of hmap.buckets alive
    internal ж<slice<ж<bmap>>> oldoverflow; // keeps overflow buckets of hmap.oldbuckets alive
    internal uintptr startBucket;        // bucket iteration started at
    internal uint8 offset;          // intra-bucket offset to start from during iteration (should be big enough to hold bucketCnt-1)
    internal bool wrapped;           // already wrapped around from end of bucket array to beginning
    public uint8 B;
    internal uint8 i;
    internal uintptr bucket;
    internal uintptr checkBucket;
}

// bucketShift returns 1<<b, optimized for code generation.
internal static uintptr bucketShift(uint8 b) {
    // Masking the shift amount allows overflow checks to be elided.
    return ((uintptr)1 << (int)(((uint8)(b & (uint8)((goarch.PtrSize * 8 - 1))))));
}

// bucketMask returns 1<<b - 1, optimized for code generation.
internal static uintptr bucketMask(uint8 b) {
    return bucketShift(b) - 1;
}

// tophash calculates the tophash value for hash.
internal static uint8 tophash(uintptr hash) {
    var top = (uint8)((hash >> (int)((goarch.PtrSize * 8 - 8))));
    if (top < minTopHash) {
        top += minTopHash;
    }
    return top;
}

internal static bool evacuated(ref bmap b) {
    var h = b.tophash[0];
    return h > emptyOne && h < minTopHash;
}

internal static ж<bmap> overflow(this ж<bmap> Ꮡb, ж<maptype> Ꮡt) {
    ref var b = ref Ꮡb.DerefOrNull();
    ref var t = ref Ꮡt.DerefOrNull();

    return ~(ж<ж<bmap>>)(uintptr)(add((uintptr)@unsafe.Pointer.FromRef(ref b), (uintptr)t.BucketSize - (uintptr)goarch.PtrSize));
}

internal static void setoverflow(this ж<bmap> Ꮡb, ж<maptype> Ꮡt, ж<bmap> Ꮡovf) {
    ref var b = ref Ꮡb.DerefOrNull();
    ref var t = ref Ꮡt.DerefOrNull();

    ((ж<ж<bmap>>)(uintptr)(add((uintptr)@unsafe.Pointer.FromRef(ref b), (uintptr)t.BucketSize - (uintptr)goarch.PtrSize))).ValueSlot = Ꮡovf;
}

internal static @unsafe.Pointer keys(this ж<bmap> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    return (uintptr)add((uintptr)@unsafe.Pointer.FromRef(ref b), dataOffset);
}

// incrnoverflow increments h.noverflow.
// noverflow counts the number of overflow buckets.
// This is used to trigger same-size map growth.
// See also tooManyOverflowBuckets.
// To keep hmap small, noverflow is a uint16.
// When there are few buckets, noverflow is an exact count.
// When there are many buckets, noverflow is an approximate count.
[GoRecv] internal static void incrnoverflow(this ref hmap h) {
    // We trigger same-size map growth if there are
    // as many overflow buckets as buckets.
    // We need to be able to count to 1<<h.B.
    if (h.B < 16) {
        h.noverflow++;
        return;
    }
    // Increment with probability 1/(1<<(h.B-15)).
    // When we reach 1<<15 - 1, we will have approximately
    // as many overflow buckets as buckets.
    var mask = ((uint32)1).Lsh((uint64)((h.B - 15))) - 1;
    // Example: if h.B == 18, then mask == 7,
    // and rand() & 7 == 0 with probability 1/8.
    if ((uint32)((uint32)rand() & mask) == 0) {
        h.noverflow++;
    }
}

[GoRecv] internal static ж<bmap> newoverflow(this ref hmap h, ж<maptype> Ꮡt, ж<bmap> Ꮡb) {
    ref var t = ref Ꮡt.DerefOrNull();

    ж<bmap> ovf = default!;
    if (h.extra != nil && (~h.extra).nextOverflow != nil){
        // We have preallocated overflow buckets available.
        // See makeBucketArray for more details.
        ovf = h.extra.Value.nextOverflow;
        if (ovf.overflow(Ꮡt) == nil){
            // We're not at the end of the preallocated overflow buckets. Bump the pointer.
            h.extra.Value.nextOverflow = (ж<bmap>)(uintptr)(add(new @unsafe.Pointer(ovf), (uintptr)t.BucketSize));
        } else {
            // This is the last preallocated overflow bucket.
            // Reset the overflow pointer on this bucket,
            // which was set to a non-nil sentinel value.
            ovf.setoverflow(Ꮡt, nil);
            h.extra.Value.nextOverflow = default!;
        }
    } else {
        ovf = (ж<bmap>)(uintptr)(newobject(t.Bucket));
    }
    h.incrnoverflow();
    if (!t.Bucket.Pointers()) {
        h.createOverflow();
        (~h.extra).overflow.ValueSlot = append((~h.extra).overflow.ValueSlot, ovf);
    }
    Ꮡb.setoverflow(Ꮡt, ovf);
    return ovf;
}

[GoRecv] internal static void createOverflow(this ref hmap h) {
    if (h.extra == nil) {
        h.extra = @new<mapextra>();
    }
    if ((~h.extra).overflow == nil) {
        h.extra.Value.overflow = @new<slice<ж<bmap>>>();
    }
}

internal static ж<hmap> makemap64(ж<maptype> Ꮡt, int64 hint, ж<hmap> Ꮡh) {
    if ((int64)(nint)hint != hint) {
        hint = 0;
    }
    return makemap(Ꮡt, (nint)hint, Ꮡh);
}

// makemap_small implements Go map creation for make(map[k]v) and
// make(map[k]v, hint) when hint is known to be at most bucketCnt
// at compile time and the map needs to be allocated on the heap.
//
// makemap_small should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname makemap_small
internal static ж<hmap> makemap_small() {
    var h = @new<hmap>();
    h.Value.hash0 = (uint32)rand();
    return h;
}

// makemap implements Go map creation for make(map[k]v, hint).
// If the compiler has determined that the map or the first bucket
// can be created on the stack, h and/or bucket may be non-nil.
// If h != nil, the map can be created directly in h.
// If h.buckets != nil, bucket pointed to can be used as the first bucket.
//
// makemap should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/cloudwego/frugal
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname makemap
internal static ж<hmap> makemap(ж<maptype> Ꮡt, nint hint, ж<hmap> Ꮡh) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var h = ref Ꮡh.DerefOrNull();

    var (mem, overflow) = Δmath.MulUintptr((uintptr)hint, (~t.Bucket).Size_);
    if (overflow || mem > maxAlloc) {
        hint = 0;
    }
    // initialize Hmap
    if (Ꮡh == nil) {
        Ꮡh = @new<hmap>(); h = ref Ꮡh.DerefOrNull();
    }
    h.hash0 = (uint32)rand();
    // Find the size parameter B which will hold the requested # of elements.
    // For hint < 0 overLoadFactor returns false since hint < bucketCnt.
    var B = (uint8)0;
    while (overLoadFactor(hint, B)) {
        B++;
    }
    h.B = B;
    // allocate initial hash table
    // if B == 0, the buckets field is allocated lazily later (in mapassign)
    // If hint is large zeroing this memory could take a while.
    if (h.B != 0) {
        ж<bmap> nextOverflow = default!;
        (h.buckets, nextOverflow) = makeBucketArray(Ꮡt, h.B, nil);
        if (nextOverflow != nil) {
            h.extra = @new<mapextra>();
            h.extra.Value.nextOverflow = nextOverflow;
        }
    }
    return Ꮡh;
}

// makeBucketArray initializes a backing array for map buckets.
// 1<<b is the minimum number of buckets to allocate.
// dirtyalloc should either be nil or a bucket array previously
// allocated by makeBucketArray with the same t and b parameters.
// If dirtyalloc is nil a new backing array will be alloced and
// otherwise dirtyalloc will be cleared and reused as backing array.
internal static (@unsafe.Pointer buckets, ж<bmap> nextOverflow) makeBucketArray(ж<maptype> Ꮡt, uint8 b, @unsafe.Pointer dirtyalloc) {
    @unsafe.Pointer buckets = default!;
    ж<bmap> nextOverflow = default!;

    ref var t = ref Ꮡt.DerefOrNull();
    var @base = bucketShift(b);
    var nbuckets = @base;
    // For small b, overflow buckets are unlikely.
    // Avoid the overhead of the calculation.
    if (b >= 4) {
        // Add on the estimated number of overflow buckets
        // required to insert the median number of elements
        // used with this value of b.
        nbuckets += bucketShift((uint8)(b - 4));
        var sz = (~t.Bucket).Size_ * nbuckets;
        var up = roundupsize(sz, !t.Bucket.Pointers());
        if (up != sz) {
            nbuckets = up / (~t.Bucket).Size_;
        }
    }
    if (dirtyalloc == nil){
        buckets = (uintptr)newarray(t.Bucket, (nint)nbuckets);
    } else {
        // dirtyalloc was previously generated by
        // the above newarray(t.Bucket, int(nbuckets))
        // but may not be empty.
        buckets = dirtyalloc;
        var size = (~t.Bucket).Size_ * nbuckets;
        if (t.Bucket.Pointers()){
            memclrHasPointers(buckets, size);
        } else {
            memclrNoHeapPointers(buckets, size);
        }
    }
    if (@base != nbuckets) {
        // We preallocated some overflow buckets.
        // To keep the overhead of tracking these overflow buckets to a minimum,
        // we use the convention that if a preallocated overflow bucket's overflow
        // pointer is nil, then there are more available by bumping the pointer.
        // We need a safe non-nil pointer for the last overflow bucket; just use buckets.
        nextOverflow = (ж<bmap>)(uintptr)(add(buckets, @base * (uintptr)t.BucketSize));
        var last = (ж<bmap>)(uintptr)(add(buckets, (nbuckets - 1) * (uintptr)t.BucketSize));
        last.setoverflow(Ꮡt, (ж<bmap>)(uintptr)(buckets));
    }
    return (buckets, nextOverflow);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string concurrentMapReadAndMapˢ = "concurrent map read and map write"u8;

// mapaccess1 returns a pointer to h[key].  Never returns nil, instead
// it will return a reference to the zero object for the elem type if
// the key is not in the map.
// NOTE: The returned pointer may keep the whole map live, so don't
// hold onto it for very long.
internal static @unsafe.Pointer mapaccess1(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @unsafe.Pointer key) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var h = ref Ꮡh.DerefOrNull();

    if (raceenabled && Ꮡh != nil) {
        var callerpc = getcallerpc();
        var pc = abi.FuncPCABIInternal(mapaccess1);
        racereadpc(new @unsafe.Pointer(Ꮡh), callerpc, pc);
        raceReadObjectPC(ref (t.Key).DerefOrNull(), key, callerpc, pc);
    }
    if (msanenabled && Ꮡh != nil) {
        msanread(key, (~t.Key).Size_);
    }
    if (asanenabled && Ꮡh != nil) {
        asanread(key, (~t.Key).Size_);
    }
    if (Ꮡh == nil || h.count == 0) {
        {
            var err = mapKeyError(Ꮡt, key); if (err != default!) {
                throw panic(err); // see issue 23734
            }
        }
        return new @unsafe.Pointer(ᏑzeroVal.at<byte>(0));
    }
    if ((uint8)(h.flags & (uint8)hashWriting) != 0) {
        fatal(concurrentMapReadAndMapˢ);
    }
    var hash = t.Hasher(key, (uintptr)h.hash0);
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
bucketloop:
    for (; b != nil; b = b.overflow(Ꮡt)) {
        for (var i = (uintptr)0; i < abi.MapBucketCount; i++) {
            if ((~b).tophash[(nint)(i)] != top) {
                if ((~b).tophash[(nint)(i)] == emptyRest) {
                    goto break_bucketloop;
                }
                continue;
            }
            @unsafe.Pointer k = (uintptr)add(new @unsafe.Pointer(b), dataOffset + i * (uintptr)t.KeySize);
            if (t.IndirectKey()) {
                k = ((ж<@unsafe.Pointer>)(uintptr)(k)).Value;
            }
            if ((~t.Key).Equal(key, k)) {
                @unsafe.Pointer e = (uintptr)add(new @unsafe.Pointer(b), dataOffset + (uintptr)abi.MapBucketCount * (uintptr)t.KeySize + i * (uintptr)t.ValueSize);
                if (t.IndirectElem()) {
                    e = ((ж<@unsafe.Pointer>)(uintptr)(e)).Value;
                }
                return e;
            }
        }
continue_bucketloop:;
    }
break_bucketloop:;
    return new @unsafe.Pointer(ᏑzeroVal.at<byte>(0));
}

// mapaccess2 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapaccess2
internal static (@unsafe.Pointer, bool) mapaccess2(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @unsafe.Pointer key) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var h = ref Ꮡh.DerefOrNull();

    if (raceenabled && Ꮡh != nil) {
        var callerpc = getcallerpc();
        var pc = abi.FuncPCABIInternal(mapaccess2);
        racereadpc(new @unsafe.Pointer(Ꮡh), callerpc, pc);
        raceReadObjectPC(ref (t.Key).DerefOrNull(), key, callerpc, pc);
    }
    if (msanenabled && Ꮡh != nil) {
        msanread(key, (~t.Key).Size_);
    }
    if (asanenabled && Ꮡh != nil) {
        asanread(key, (~t.Key).Size_);
    }
    if (Ꮡh == nil || h.count == 0) {
        {
            var err = mapKeyError(Ꮡt, key); if (err != default!) {
                throw panic(err); // see issue 23734
            }
        }
        return (new @unsafe.Pointer(ᏑzeroVal.at<byte>(0)), false);
    }
    if ((uint8)(h.flags & (uint8)hashWriting) != 0) {
        fatal(concurrentMapReadAndMapˢ);
    }
    var hash = t.Hasher(key, (uintptr)h.hash0);
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
bucketloop:
    for (; b != nil; b = b.overflow(Ꮡt)) {
        for (var i = (uintptr)0; i < abi.MapBucketCount; i++) {
            if ((~b).tophash[(nint)(i)] != top) {
                if ((~b).tophash[(nint)(i)] == emptyRest) {
                    goto break_bucketloop;
                }
                continue;
            }
            @unsafe.Pointer k = (uintptr)add(new @unsafe.Pointer(b), dataOffset + i * (uintptr)t.KeySize);
            if (t.IndirectKey()) {
                k = ((ж<@unsafe.Pointer>)(uintptr)(k)).Value;
            }
            if ((~t.Key).Equal(key, k)) {
                @unsafe.Pointer e = (uintptr)add(new @unsafe.Pointer(b), dataOffset + (uintptr)abi.MapBucketCount * (uintptr)t.KeySize + i * (uintptr)t.ValueSize);
                if (t.IndirectElem()) {
                    e = ((ж<@unsafe.Pointer>)(uintptr)(e)).Value;
                }
                return (e, true);
            }
        }
continue_bucketloop:;
    }
break_bucketloop:;
    return (new @unsafe.Pointer(ᏑzeroVal.at<byte>(0)), false);
}

// returns both key and elem. Used by map iterator.
internal static (@unsafe.Pointer, @unsafe.Pointer) mapaccessK(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @unsafe.Pointer key) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var h = ref Ꮡh.DerefOrNull();

    if (Ꮡh == nil || h.count == 0) {
        return (default!, default!);
    }
    var hash = t.Hasher(key, (uintptr)h.hash0);
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
bucketloop:
    for (; b != nil; b = b.overflow(Ꮡt)) {
        for (var i = (uintptr)0; i < abi.MapBucketCount; i++) {
            if ((~b).tophash[(nint)(i)] != top) {
                if ((~b).tophash[(nint)(i)] == emptyRest) {
                    goto break_bucketloop;
                }
                continue;
            }
            @unsafe.Pointer k = (uintptr)add(new @unsafe.Pointer(b), dataOffset + i * (uintptr)t.KeySize);
            if (t.IndirectKey()) {
                k = ((ж<@unsafe.Pointer>)(uintptr)(k)).Value;
            }
            if ((~t.Key).Equal(key, k)) {
                @unsafe.Pointer e = (uintptr)add(new @unsafe.Pointer(b), dataOffset + (uintptr)abi.MapBucketCount * (uintptr)t.KeySize + i * (uintptr)t.ValueSize);
                if (t.IndirectElem()) {
                    e = ((ж<@unsafe.Pointer>)(uintptr)(e)).Value;
                }
                return (k, e);
            }
        }
continue_bucketloop:;
    }
break_bucketloop:;
    return (default!, default!);
}

internal static @unsafe.Pointer mapaccess1_fat(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @unsafe.Pointer key, @unsafe.Pointer zero) {
    @unsafe.Pointer e = (uintptr)mapaccess1(Ꮡt, Ꮡh, key);
    if (e == new @unsafe.Pointer(ᏑzeroVal.at<byte>(0))) {
        return zero;
    }
    return e;
}

internal static (@unsafe.Pointer, bool) mapaccess2_fat(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @unsafe.Pointer key, @unsafe.Pointer zero) {
    @unsafe.Pointer e = (uintptr)mapaccess1(Ꮡt, Ꮡh, key);
    if (e == new @unsafe.Pointer(ᏑzeroVal.at<byte>(0))) {
        return (zero, false);
    }
    return (e, true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string assignmentToEntryInNilˢ = "assignment to entry in nil map"u8;
internal static readonly @string concurrentMapWritesˢ = "concurrent map writes"u8;

// Like mapaccess, but allocates a slot for the key if it is not present in the map.
//
// mapassign should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//   - github.com/cloudwego/frugal
//   - github.com/RomiChan/protobuf
//   - github.com/segmentio/encoding
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapassign
internal static @unsafe.Pointer mapassign(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @unsafe.Pointer key) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var h = ref Ꮡh.DerefOrNull();

    if (Ꮡh == nil) {
        throw panic(((plainError)(@string)assignmentToEntryInNilˢ));
    }
    if (raceenabled) {
        var callerpc = getcallerpc();
        var pc = abi.FuncPCABIInternal(mapassign);
        racewritepc(new @unsafe.Pointer(Ꮡh), callerpc, pc);
        raceReadObjectPC(ref (t.Key).DerefOrNull(), key, callerpc, pc);
    }
    if (msanenabled) {
        msanread(key, (~t.Key).Size_);
    }
    if (asanenabled) {
        asanread(key, (~t.Key).Size_);
    }
    if ((uint8)(h.flags & (uint8)hashWriting) != 0) {
        fatal(concurrentMapWritesˢ);
    }
    var hash = t.Hasher(key, (uintptr)h.hash0);
    // Set hashWriting after calling t.hasher, since t.hasher may panic,
    // in which case we have not actually done a write.
    h.flags ^= (uint8)(hashWriting);
    if (h.buckets == nil) {
        h.buckets = (uintptr)newobject(t.Bucket); // newarray(t.Bucket, 1)
    }
again:
    var bucket = (uintptr)(hash & bucketMask(h.B));
    if (h.growing()) {
        growWork(Ꮡt, Ꮡh, bucket);
    }
    var b = (ж<bmap>)(uintptr)(add(h.buckets, bucket * (uintptr)t.BucketSize));
    var top = tophash(hash);
    ж<uint8> inserti = default!;
    @unsafe.Pointer insertk = default!;
    @unsafe.Pointer elem = default!;
bucketloop:
    while (ᐧ) {
        for (var i = (uintptr)0; i < abi.MapBucketCount; i++) {
            if ((~b).tophash[(nint)(i)] != top) {
                if (isEmpty((~b).tophash[(nint)(i)]) && inserti == nil) {
                    inserti = b.at(bmap.Ꮡtophash, (nint)(i));
                    insertk = (uintptr)add(new @unsafe.Pointer(b), dataOffset + i * (uintptr)t.KeySize);
                    elem = (uintptr)add(new @unsafe.Pointer(b), dataOffset + (uintptr)abi.MapBucketCount * (uintptr)t.KeySize + i * (uintptr)t.ValueSize);
                }
                if ((~b).tophash[(nint)(i)] == emptyRest) {
                    goto break_bucketloop;
                }
                continue;
            }
            @unsafe.Pointer k = (uintptr)add(new @unsafe.Pointer(b), dataOffset + i * (uintptr)t.KeySize);
            if (t.IndirectKey()) {
                k = ((ж<@unsafe.Pointer>)(uintptr)(k)).Value;
            }
            if (!(~t.Key).Equal(key, k)) {
                continue;
            }
            // already have a mapping for key. Update it.
            if (t.NeedKeyUpdate()) {
                typedmemmove(t.Key, k, key);
            }
            elem = (uintptr)add(new @unsafe.Pointer(b), dataOffset + (uintptr)abi.MapBucketCount * (uintptr)t.KeySize + i * (uintptr)t.ValueSize);
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
    if (inserti == nil) {
        // The current bucket and all the overflow buckets connected to it are full, allocate a new one.
        var newb = h.newoverflow(Ꮡt, b);
        inserti = newb.at(bmap.Ꮡtophash, 0);
        insertk = (uintptr)add(new @unsafe.Pointer(newb), dataOffset);
        elem = (uintptr)add(insertk, (uintptr)abi.MapBucketCount * (uintptr)t.KeySize);
    }
    // store new key/elem at insert position
    if (t.IndirectKey()) {
        @unsafe.Pointer kmem = (uintptr)newobject(t.Key);
        ((ж<@unsafe.Pointer>)(uintptr)(insertk)).Value = kmem;
        insertk = kmem;
    }
    if (t.IndirectElem()) {
        @unsafe.Pointer vmem = (uintptr)newobject(t.Elem);
        ((ж<@unsafe.Pointer>)(uintptr)(elem)).Value = vmem;
    }
    typedmemmove(t.Key, insertk, key);
    inserti.Value = top;
    h.count++;
done:
    if ((uint8)(h.flags & (uint8)hashWriting) == 0) {
        fatal(concurrentMapWritesˢ);
    }
    h.flags &= unchecked((uint8)~(uint8)(hashWriting));
    if (t.IndirectElem()) {
        elem = ((ж<@unsafe.Pointer>)(uintptr)(elem)).Value;
    }
    return elem;
}

// mapdelete should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapdelete
internal static void mapdelete(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @unsafe.Pointer key) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var h = ref Ꮡh.DerefOrNull();

    if (raceenabled && Ꮡh != nil) {
        var callerpc = getcallerpc();
        var pc = abi.FuncPCABIInternal(mapdelete);
        racewritepc(new @unsafe.Pointer(Ꮡh), callerpc, pc);
        raceReadObjectPC(ref (t.Key).DerefOrNull(), key, callerpc, pc);
    }
    if (msanenabled && Ꮡh != nil) {
        msanread(key, (~t.Key).Size_);
    }
    if (asanenabled && Ꮡh != nil) {
        asanread(key, (~t.Key).Size_);
    }
    if (Ꮡh == nil || h.count == 0) {
        {
            var err = mapKeyError(Ꮡt, key); if (err != default!) {
                throw panic(err); // see issue 23734
            }
        }
        return;
    }
    if ((uint8)(h.flags & (uint8)hashWriting) != 0) {
        fatal(concurrentMapWritesˢ);
    }
    var hash = t.Hasher(key, (uintptr)h.hash0);
    // Set hashWriting after calling t.hasher, since t.hasher may panic,
    // in which case we have not actually done a write (delete).
    h.flags ^= (uint8)(hashWriting);
    var bucket = (uintptr)(hash & bucketMask(h.B));
    if (h.growing()) {
        growWork(Ꮡt, Ꮡh, bucket);
    }
    var b = (ж<bmap>)(uintptr)(add(h.buckets, bucket * (uintptr)t.BucketSize));
    var bOrig = b;
    var top = tophash(hash);
search:
    for (; b != nil; b = b.overflow(Ꮡt)) {
        for (var i = (uintptr)0; i < abi.MapBucketCount; i++) {
            if ((~b).tophash[(nint)(i)] != top) {
                if ((~b).tophash[(nint)(i)] == emptyRest) {
                    goto break_search;
                }
                continue;
            }
            @unsafe.Pointer k = (uintptr)add(new @unsafe.Pointer(b), dataOffset + i * (uintptr)t.KeySize);
            @unsafe.Pointer k2 = k;
            if (t.IndirectKey()) {
                k2 = ((ж<@unsafe.Pointer>)(uintptr)(k2)).Value;
            }
            if (!(~t.Key).Equal(key, k2)) {
                continue;
            }
            // Only clear key if there are pointers in it.
            if (t.IndirectKey()){
                ((ж<@unsafe.Pointer>)(uintptr)(k)).Value = default!;
            } else 
            if (t.Key.Pointers()) {
                memclrHasPointers(k, (~t.Key).Size_);
            }
            @unsafe.Pointer e = (uintptr)add(new @unsafe.Pointer(b), dataOffset + (uintptr)abi.MapBucketCount * (uintptr)t.KeySize + i * (uintptr)t.ValueSize);
            if (t.IndirectElem()){
                ((ж<@unsafe.Pointer>)(uintptr)(e)).Value = default!;
            } else 
            if (t.Elem.Pointers()){
                memclrHasPointers(e, (~t.Elem).Size_);
            } else {
                memclrNoHeapPointers(e, (~t.Elem).Size_);
            }
            b.Value.tophash[(nint)(i)] = emptyOne;
            // If the bucket now ends in a bunch of emptyOne states,
            // change those to emptyRest states.
            // It would be nice to make this a separate function, but
            // for loops are not currently inlineable.
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hashIterSizeIncorrectˢ = "hash_iter size incorrect"u8;

// mapiterinit initializes the hiter struct used for ranging over maps.
// The hiter struct pointed to by 'it' is allocated on the stack
// by the compilers order pass or on the heap by reflect_mapiterinit.
// Both need to have zeroed hiter since the struct contains pointers.
//
// mapiterinit should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//   - github.com/cloudwego/frugal
//   - github.com/goccy/go-json
//   - github.com/RomiChan/protobuf
//   - github.com/segmentio/encoding
//   - github.com/ugorji/go/codec
//   - github.com/wI2L/jettison
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapiterinit
internal static void mapiterinit(ж<maptype> Ꮡt, ж<hmap> Ꮡh, ж<hiter> Ꮡit) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var h = ref Ꮡh.DerefOrNull();
    ref var it = ref Ꮡit.DerefOrNull();

    if (raceenabled && Ꮡh != nil) {
        var callerpc = getcallerpc();
        racereadpc(new @unsafe.Pointer(Ꮡh), callerpc, abi.FuncPCABIInternal(mapiterinit));
    }
    it.t = Ꮡt;
    if (Ꮡh == nil || h.count == 0) {
        return;
    }
    if ((uintptr)(/* unsafe.Sizeof(hiter{}) */ (uintptr)96 / (uintptr)goarch.PtrSize) != 12) {
        @throw(hashIterSizeIncorrectˢ); // see cmd/compile/internal/reflectdata/reflect.go
    }
    it.h = Ꮡh;
    // grab snapshot of bucket state
    it.B = h.B;
    it.buckets = h.buckets;
    if (!t.Bucket.Pointers()) {
        // Allocate the current slice and remember pointers to both current and old.
        // This preserves all relevant overflow buckets alive even if
        // the table grows and/or overflow buckets are added to the table
        // while we are iterating.
        h.createOverflow();
        it.overflow = h.extra.Value.overflow;
        it.oldoverflow = h.extra.Value.oldoverflow;
    }
    // decide where to start
    var r = (uintptr)rand();
    it.startBucket = (uintptr)(r & bucketMask(h.B));
    it.offset = (uint8)((uintptr)(r.Rsh((uint64)(h.B)) & (uintptr)(abi.MapBucketCount - 1)));
    // iterator state
    it.bucket = it.startBucket;
    // Remember we have an iterator.
    // Can run concurrently with another mapiterinit().
    {
        var old = h.flags; if ((uint8)(old & ((uint8)((uint8)iterator | (uint8)oldIterator))) != (uint8)((uint8)iterator | (uint8)oldIterator)) {
            atomic.Or8(Ꮡh.of(hmap.Ꮡflags), (uint8)((uint8)iterator | (uint8)oldIterator));
        }
    }
    mapiternext(Ꮡit);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string concurrentMapIterationˢ = "concurrent map iteration and map write"u8;

// mapiternext should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//   - github.com/cloudwego/frugal
//   - github.com/RomiChan/protobuf
//   - github.com/segmentio/encoding
//   - github.com/ugorji/go/codec
//   - gonum.org/v1/gonum
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapiternext
internal static void mapiternext(ж<hiter> Ꮡit) {
    ref var it = ref Ꮡit.DerefOrNull();

    var h = it.h;
    if (raceenabled) {
        var callerpc = getcallerpc();
        racereadpc(new @unsafe.Pointer(h), callerpc, abi.FuncPCABIInternal(mapiternext));
    }
    if ((uint8)((~h).flags & (uint8)hashWriting) != 0) {
        fatal(concurrentMapIterationˢ);
    }
    var t = it.t;
    var bucket = it.bucket;
    var b = it.bptr;
    var i = it.i;
    var checkBucket = it.checkBucket;
next:
    if (b == nil) {
        if (bucket == it.startBucket && it.wrapped) {
            // end of iteration
            it.key = default!;
            it.elem = default!;
            return;
        }
        if (h.growing() && it.B == (~h).B){
            // Iterator was started in the middle of a grow, and the grow isn't done yet.
            // If the bucket we're looking at hasn't been filled in yet (i.e. the old
            // bucket hasn't been evacuated) then we need to iterate through the old
            // bucket and only return the ones that will be migrated to this bucket.
            var oldbucket = (uintptr)(bucket & it.h.oldbucketmask());
            b = (ж<bmap>)(uintptr)(add((~h).oldbuckets, oldbucket * (uintptr)(~t).BucketSize));
            if (!evacuated(ref (b).DerefOrNull())){
                checkBucket = bucket;
            } else {
                b = (ж<bmap>)(uintptr)(add(it.buckets, bucket * (uintptr)(~t).BucketSize));
                checkBucket = noCheck;
            }
        } else {
            b = (ж<bmap>)(uintptr)(add(it.buckets, bucket * (uintptr)(~t).BucketSize));
            checkBucket = noCheck;
        }
        bucket++;
        if (bucket == bucketShift(it.B)) {
            bucket = 0;
            it.wrapped = true;
        }
        i = 0;
    }
    for (; i < abi.MapBucketCount; i++) {
        var offi = (uint8)((i + it.offset) & (uint8)((abi.MapBucketCount - 1)));
        if (isEmpty((~b).tophash[offi]) || (~b).tophash[offi] == evacuatedEmpty) {
            // TODO: emptyRest is hard to use here, as we start iterating
            // in the middle of a bucket. It's feasible, just tricky.
            continue;
        }
        @unsafe.Pointer k = (uintptr)add(new @unsafe.Pointer(b), dataOffset + (uintptr)offi * (uintptr)(~t).KeySize);
        if (t.IndirectKey()) {
            k = ((ж<@unsafe.Pointer>)(uintptr)(k)).Value;
        }
        @unsafe.Pointer e = (uintptr)add(new @unsafe.Pointer(b), dataOffset + (uintptr)abi.MapBucketCount * (uintptr)(~t).KeySize + (uintptr)offi * (uintptr)(~t).ValueSize);
        if (checkBucket != noCheck && !h.sameSizeGrow()) {
            // Special case: iterator was started during a grow to a larger size
            // and the grow is not done yet. We're working on a bucket whose
            // oldbucket has not been evacuated yet. Or at least, it wasn't
            // evacuated when we started the bucket. So we're iterating
            // through the oldbucket, skipping any keys that will go
            // to the other new bucket (each oldbucket expands to two
            // buckets during a grow).
            if (t.ReflexiveKey() || (~(~t).Key).Equal(k, k)){
                // If the item in the oldbucket is not destined for
                // the current new bucket in the iteration, skip it.
                var hash = (~t).Hasher(k, (uintptr)(~h).hash0);
                if ((uintptr)(hash & bucketMask(it.B)) != checkBucket) {
                    continue;
                }
            } else {
                // Hash isn't repeatable if k != k (NaNs).  We need a
                // repeatable and randomish choice of which direction
                // to send NaNs during evacuation. We'll use the low
                // bit of tophash to decide which way NaNs go.
                // NOTE: this case is why we need two evacuate tophash
                // values, evacuatedX and evacuatedY, that differ in
                // their low bit.
                if (checkBucket.Rsh((uint64)((it.B - 1))) != (uintptr)((uint8)((~b).tophash[offi] & 1))) {
                    continue;
                }
            }
        }
        if (((~b).tophash[offi] != evacuatedX && (~b).tophash[offi] != evacuatedY) || !(t.ReflexiveKey() || (~(~t).Key).Equal(k, k))){
            // This is the golden data, we can return it.
            // OR
            // key!=key, so the entry can't be deleted or updated, so we can just return it.
            // That's lucky for us because when key!=key we can't look it up successfully.
            it.key = k;
            if (t.IndirectElem()) {
                e = ((ж<@unsafe.Pointer>)(uintptr)(e)).Value;
            }
            it.elem = e;
        } else {
            // The hash table has grown since the iterator was started.
            // The golden data for this key is now somewhere else.
            // Check the current hash table for the data.
            // This code handles the case where the key
            // has been deleted, updated, or deleted and reinserted.
            // NOTE: we need to regrab the key as it has potentially been
            // updated to an equal() but not identical key (e.g. +0.0 vs -0.0).
            var (rk, re) = mapaccessK(t, h, k);
            if (rk == nil) {
                continue; // key has been deleted
            }
            it.key = rk;
            it.elem = re;
        }
        it.bucket = bucket;
        if (it.bptr != b) {
            // avoid unnecessary write barrier; see issue 14921
            it.bptr = b;
        }
        it.i = (uint8)(i + 1);
        it.checkBucket = checkBucket;
        return;
    }
    b = b.overflow(t);
    i = 0;
    goto next;
}

// mapclear deletes all keys from a map.
// It is called by the compiler.
//
// mapclear should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/cloudwego/frugal
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapclear
internal static void mapclear(ж<maptype> Ꮡt, ж<hmap> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    if (raceenabled && Ꮡh != nil) {
        var callerpc = getcallerpc();
        var pc = abi.FuncPCABIInternal(mapclear);
        racewritepc(new @unsafe.Pointer(Ꮡh), callerpc, pc);
    }
    if (Ꮡh == nil || h.count == 0) {
        return;
    }
    if ((uint8)(h.flags & (uint8)hashWriting) != 0) {
        fatal(concurrentMapWritesˢ);
    }
    h.flags ^= (uint8)(hashWriting);
    // Mark buckets empty, so existing iterators can be terminated, see issue #59411.
    void markBucketsEmpty(@unsafe.Pointer bucket, uintptr mask) {
        for (var i = (uintptr)0; i <= mask; i++) {
            var b = (ж<bmap>)(uintptr)(add(bucket, i * (uintptr)Ꮡt.Value.BucketSize));
            for (; b != nil; b = b.overflow(Ꮡt)) {
                for (var iΔ1 = (uintptr)0; iΔ1 < abi.MapBucketCount; iΔ1++) {
                    b.Value.tophash[(nint)(iΔ1)] = emptyRest;
                }
            }
        }
    }
    markBucketsEmpty(h.buckets, bucketMask(h.B));
    {
        @unsafe.Pointer oldBuckets = h.oldbuckets; if (oldBuckets != nil) {
            markBucketsEmpty(oldBuckets, h.oldbucketmask());
        }
    }
    h.flags &= unchecked((uint8)~(uint8)(ΔsameSizeGrow));
    h.oldbuckets = default!;
    h.nevacuate = 0;
    h.noverflow = 0;
    h.count = 0;
    // Reset the hash seed to make it more difficult for attackers to
    // repeatedly trigger hash collisions. See issue 25237.
    h.hash0 = (uint32)rand();
    // Keep the mapextra allocation but clear any extra information.
    if (h.extra != nil) {
        h.extra.Value = new mapextra(nil);
    }
    // makeBucketArray clears the memory pointed to by h.buckets
    // and recovers any overflow buckets by generating them
    // as if h.buckets was newly alloced.
    var (_, nextOverflow) = makeBucketArray(Ꮡt, h.B, h.buckets);
    if (nextOverflow != nil) {
        // If overflow buckets are created then h.extra
        // will have been allocated during initial bucket creation.
        h.extra.Value.nextOverflow = nextOverflow;
    }
    if ((uint8)(h.flags & (uint8)hashWriting) == 0) {
        fatal(concurrentMapWritesˢ);
    }
    h.flags &= unchecked((uint8)~(uint8)(hashWriting));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string oldoverflowIsNotNilˢ = "oldoverflow is not nil"u8;

internal static void hashGrow(ж<maptype> Ꮡt, ref hmap h) {
    // If we've hit the load factor, get bigger.
    // Otherwise, there are too many overflow buckets,
    // so keep the same number of buckets and "grow" laterally.
    var bigger = (uint8)1;
    if (!overLoadFactor(h.count + 1, h.B)) {
        bigger = 0;
        h.flags |= (uint8)(ΔsameSizeGrow);
    }
    @unsafe.Pointer oldbuckets = h.buckets;
    var (newbuckets, nextOverflow) = makeBucketArray(Ꮡt, (uint8)(h.B + bigger), nil);
    var flags = (uint8)(h.flags & ~((uint8)((uint8)iterator | (uint8)oldIterator)));
    if ((uint8)(h.flags & (uint8)iterator) != 0) {
        flags |= (uint8)(oldIterator);
    }
    // commit the grow (atomic wrt gc)
    h.B += bigger;
    h.flags = flags;
    h.oldbuckets = oldbuckets;
    h.buckets = newbuckets;
    h.nevacuate = 0;
    h.noverflow = 0;
    if (h.extra != nil && (~h.extra).overflow != nil) {
        // Promote current overflow buckets to the old generation.
        if ((~h.extra).oldoverflow != nil) {
            @throw(oldoverflowIsNotNilˢ);
        }
        h.extra.Value.oldoverflow = h.extra.Value.overflow;
        h.extra.Value.overflow = default!;
    }
    if (nextOverflow != nil) {
        if (h.extra == nil) {
            h.extra = @new<mapextra>();
        }
        h.extra.Value.nextOverflow = nextOverflow;
    }
}

// the actual copying of the hash table data is done incrementally
// by growWork() and evacuate().

// overLoadFactor reports whether count items placed in 1<<B buckets is over loadFactor.
internal static bool overLoadFactor(nint count, uint8 B) {
    return count > abi.MapBucketCount && (uintptr)count > (uintptr)loadFactorNum * (bucketShift(B) / (uintptr)loadFactorDen);
}

// tooManyOverflowBuckets reports whether noverflow buckets is too many for a map with 1<<B buckets.
// Note that most of these overflow buckets must be in sparse use;
// if use was dense, then we'd have already triggered regular map growth.
internal static bool tooManyOverflowBuckets(uint16 noverflow, uint8 B) {
    // If the threshold is too low, we do extraneous work.
    // If the threshold is too high, maps that grow and shrink can hold on to lots of unused memory.
    // "too many" means (approximately) as many overflow buckets as regular buckets.
    // See incrnoverflow for more details.
    if (B > 15) {
        B = 15;
    }
    // The compiler doesn't see here that B < 16; mask B to generate shorter shift code.
    return noverflow >= (uint16)((uint16)1 << (int)(((uint8)(B & 15))));
}

// growing reports whether h is growing. The growth may be to the same size or bigger.
[GoRecv] internal static bool growing(this ref hmap h) {
    return h.oldbuckets != nil;
}

// sameSizeGrow reports whether the current growth is to a map of the same size.
[GoRecv] internal static bool sameSizeGrow(this ref hmap h) {
    return (uint8)(h.flags & (uint8)ΔsameSizeGrow) != 0;
}

//go:linkname sameSizeGrowForIssue69110Test
internal static bool sameSizeGrowForIssue69110Test(ж<hmap> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    return h.sameSizeGrow();
}

// noldbuckets calculates the number of buckets prior to the current map growth.
[GoRecv] internal static uintptr noldbuckets(this ref hmap h) {
    var oldB = h.B;
    if (!h.sameSizeGrow()) {
        oldB--;
    }
    return bucketShift(oldB);
}

// oldbucketmask provides a mask that can be applied to calculate n % noldbuckets().
[GoRecv] internal static uintptr oldbucketmask(this ref hmap h) {
    return h.noldbuckets() - 1;
}

internal static void growWork(ж<maptype> Ꮡt, ж<hmap> Ꮡh, uintptr bucket) {
    ref var h = ref Ꮡh.DerefOrNull();

    // make sure we evacuate the oldbucket corresponding
    // to the bucket we're about to use
    evacuate(Ꮡt, Ꮡh, (uintptr)(bucket & h.oldbucketmask()));
    // evacuate one more oldbucket to make progress on growing
    if (h.growing()) {
        evacuate(Ꮡt, Ꮡh, h.nevacuate);
    }
}

internal static bool bucketEvacuated(ref abiꓸMapType t, ref hmap h, uintptr bucket) {
    var b = (ж<bmap>)(uintptr)(add(h.oldbuckets, bucket * (uintptr)t.BucketSize));
    return evacuated(ref (b).DerefOrNull());
}

// evacDst is an evacuation destination.
[GoType] partial struct evacDst {
    internal ж<bmap> b;       // current destination bucket
    internal nint i;           // key/elem index into b
    internal @unsafe.Pointer k; // pointer to current key storage
    internal @unsafe.Pointer e; // pointer to current elem storage
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badMapStateˢ = "bad map state"u8;
internal static readonly @string badEvacuatedNˢ = "bad evacuatedN"u8;

internal static void evacuate(ж<maptype> Ꮡt, ж<hmap> Ꮡh, uintptr oldbucket) {
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
        x.Value.k = (uintptr)add(new @unsafe.Pointer((~x).b), dataOffset);
        x.Value.e = (uintptr)add((~x).k, (uintptr)abi.MapBucketCount * (uintptr)t.KeySize);
        if (!h.sameSizeGrow()) {
            // Only calculate y pointers if we're growing bigger.
            // Otherwise GC can see bad pointers.
            var y = Ꮡxy.at<evacDst>(1);
            y.Value.b = (ж<bmap>)(uintptr)(add(h.buckets, (oldbucket + newbit) * (uintptr)t.BucketSize));
            y.Value.k = (uintptr)add(new @unsafe.Pointer((~y).b), dataOffset);
            y.Value.e = (uintptr)add((~y).k, (uintptr)abi.MapBucketCount * (uintptr)t.KeySize);
        }
        for (; b != nil; b = b.overflow(Ꮡt)) {
            @unsafe.Pointer k = (uintptr)add(new @unsafe.Pointer(b), dataOffset);
            @unsafe.Pointer e = (uintptr)add(k, (uintptr)abi.MapBucketCount * (uintptr)t.KeySize);
            for (nint i = 0; i < abi.MapBucketCount; (i, k, e) = (i + 1, (uintptr)add(k, (uintptr)t.KeySize), (uintptr)add(e, (uintptr)t.ValueSize))) {
                var top = (~b).tophash[i];
                if (isEmpty(top)) {
                    b.Value.tophash[i] = evacuatedEmpty;
                    continue;
                }
                if (top < minTopHash) {
                    @throw(badMapStateˢ);
                }
                @unsafe.Pointer k2 = k;
                if (t.IndirectKey()) {
                    k2 = ((ж<@unsafe.Pointer>)(uintptr)(k2)).Value;
                }
                uint8 useY = default!;
                if (!h.sameSizeGrow()) {
                    // Compute hash to make our evacuation decision (whether we need
                    // to send this key/elem to bucket x or bucket y).
                    var hash = t.Hasher(k2, (uintptr)h.hash0);
                    if ((uint8)(h.flags & (uint8)iterator) != 0 && !t.ReflexiveKey() && !(~t.Key).Equal(k2, k2)){
                        // If key != key (NaNs), then the hash could be (and probably
                        // will be) entirely different from the old hash. Moreover,
                        // it isn't reproducible. Reproducibility is required in the
                        // presence of iterators, as our evacuation decision must
                        // match whatever decision the iterator made.
                        // Fortunately, we have the freedom to send these keys either
                        // way. Also, tophash is meaningless for these kinds of keys.
                        // We let the low bit of tophash drive the evacuation decision.
                        // We recompute a new random tophash for the next level so
                        // these keys will get evenly distributed across all buckets
                        // after multiple grows.
                        useY = (uint8)(top & 1);
                        top = tophash(hash);
                    } else {
                        if ((uintptr)(hash & newbit) != 0) {
                            useY = 1;
                        }
                    }
                }
                if (evacuatedX + 1 != evacuatedY || (UntypedInt)(evacuatedX ^ 1) != evacuatedY) {
                    @throw(badEvacuatedNˢ);
                }
                b.Value.tophash[i] = (uint8)((uint8)evacuatedX + useY); // evacuatedX + 1 == evacuatedY
                var dst = Ꮡxy.at<evacDst>((nint)(useY)); // evacuation destination
                if ((~dst).i == abi.MapBucketCount) {
                    dst.Value.b = h.newoverflow(Ꮡt, (~dst).b);
                    dst.Value.i = 0;
                    dst.Value.k = (uintptr)add(new @unsafe.Pointer((~dst).b), dataOffset);
                    dst.Value.e = (uintptr)add((~dst).k, (uintptr)abi.MapBucketCount * (uintptr)t.KeySize);
                }
                dst.Value.b.Value.tophash[(nint)((~dst).i & (nint)(abi.MapBucketCount - 1))] = top; // mask dst.i as an optimization, to avoid a bounds check
                if (t.IndirectKey()){
                    ((ж<@unsafe.Pointer>)(uintptr)((~dst).k)).Value = k2; // copy pointer
                } else {
                    typedmemmove(t.Key, (~dst).k, k); // copy elem
                }
                if (t.IndirectElem()){
                    ((ж<@unsafe.Pointer>)(uintptr)((~dst).e)).Value = ((ж<@unsafe.Pointer>)(uintptr)(e)).Value;
                } else {
                    typedmemmove(t.Elem, (~dst).e, e);
                }
                dst.Value.i++;
                // These updates might push these pointers past the end of the
                // key or elem arrays.  That's ok, as we have the overflow pointer
                // at the end of the bucket to protect against pointing past the
                // end of the bucket.
                dst.Value.k = (uintptr)add((~dst).k, (uintptr)t.KeySize);
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

internal static void advanceEvacuationMark(ref hmap h, ref abiꓸMapType t, uintptr newbit) {
    h.nevacuate++;
    // Experiments suggest that 1024 is overkill by at least an order of magnitude.
    // Put it in there as a safeguard anyway, to ensure O(1) behavior.
    var stop = h.nevacuate + 1024;
    if (stop > newbit) {
        stop = newbit;
    }
    while (h.nevacuate != stop && bucketEvacuated(ref t, ref h, h.nevacuate)) {
        h.nevacuate++;
    }
    if (h.nevacuate == newbit) {
        // newbit == # of oldbuckets
        // Growing is all done. Free old main bucket array.
        h.oldbuckets = default!;
        // Can discard old overflow buckets as well.
        // If they are still referenced by an iterator,
        // then the iterator holds a pointers to the slice.
        if (h.extra != nil) {
            h.extra.Value.oldoverflow = default!;
        }
        h.flags &= unchecked((uint8)~(uint8)(ΔsameSizeGrow));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeReflectMakemapˢ = "runtime.reflect_makemap: unsupported map key type"u8;
internal static readonly @string keySizeWrongˢ = "key size wrong"u8;
internal static readonly @string elemSizeWrongˢ = "elem size wrong"u8;
internal static readonly @string keyAlignTooBigˢ = "key align too big"u8;
internal static readonly @string elemAlignTooBigˢ = "elem align too big"u8;
internal static readonly @string keySizeNotAMultipleOfKeyˢ = "key size not a multiple of key align"u8;
internal static readonly @string elemSizeNotAMultipleOfˢ = "elem size not a multiple of elem align"u8;
internal static readonly @string bucketsizeTooSmallForˢ = "bucketsize too small for proper alignment"u8;
internal static readonly @string needPaddingInBucketKeyˢ = "need padding in bucket (key)"u8;
internal static readonly @string needPaddingInBucketElemˢ = "need padding in bucket (elem)"u8;

// Reflect stubs. Called from ../reflect/asm_*.s

// reflect_makemap is for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//   - github.com/modern-go/reflect2
//   - github.com/goccy/go-json
//   - github.com/RomiChan/protobuf
//   - github.com/segmentio/encoding
//   - github.com/v2pro/plz
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_makemap reflect.makemap
internal static ж<hmap> reflect_makemap(ж<maptype> Ꮡt, nint cap) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Check invariants and reflects math.
    if ((~t.Key).Equal == default!) {
        @throw(runtimeReflectMakemapˢ);
    }
    if ((~t.Key).Size_ > abi.MapMaxKeyBytes && (!t.IndirectKey() || t.KeySize != (uint8)goarch.PtrSize) || (~t.Key).Size_ <= abi.MapMaxKeyBytes && (t.IndirectKey() || t.KeySize != (uint8)(~t.Key).Size_)) {
        @throw(keySizeWrongˢ);
    }
    if ((~t.Elem).Size_ > abi.MapMaxElemBytes && (!t.IndirectElem() || t.ValueSize != (uint8)goarch.PtrSize) || (~t.Elem).Size_ <= abi.MapMaxElemBytes && (t.IndirectElem() || t.ValueSize != (uint8)(~t.Elem).Size_)) {
        @throw(elemSizeWrongˢ);
    }
    if ((~t.Key).Align_ > abi.MapBucketCount) {
        @throw(keyAlignTooBigˢ);
    }
    if ((~t.Elem).Align_ > abi.MapBucketCount) {
        @throw(elemAlignTooBigˢ);
    }
    if ((~t.Key).Size_ % (uintptr)(~t.Key).Align_ != 0) {
        @throw(keySizeNotAMultipleOfKeyˢ);
    }
    if ((~t.Elem).Size_ % (uintptr)(~t.Elem).Align_ != 0) {
        @throw(elemSizeNotAMultipleOfˢ);
    }
    if (abi.MapBucketCount < 8) {
        @throw(bucketsizeTooSmallForˢ);
    }
    if (dataOffset % (uintptr)(~t.Key).Align_ != 0) {
        @throw(needPaddingInBucketKeyˢ);
    }
    if (dataOffset % (uintptr)(~t.Elem).Align_ != 0) {
        @throw(needPaddingInBucketElemˢ);
    }
    return makemap(Ꮡt, cap, nil);
}

// reflect_mapaccess is for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//   - github.com/modern-go/reflect2
//   - github.com/v2pro/plz
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_mapaccess reflect.mapaccess
internal static @unsafe.Pointer reflect_mapaccess(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @unsafe.Pointer key) {
    var (elem, ok) = mapaccess2(Ꮡt, Ꮡh, key);
    if (!ok) {
        // reflect wants nil for a missing element
        elem = default!;
    }
    return elem;
}

//go:linkname reflect_mapaccess_faststr reflect.mapaccess_faststr
internal static @unsafe.Pointer reflect_mapaccess_faststr(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @string key) {
    var (elem, ok) = mapaccess2_faststr(Ꮡt, Ꮡh, key);
    if (!ok) {
        // reflect wants nil for a missing element
        elem = default!;
    }
    return elem;
}

// reflect_mapassign is for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//   - github.com/v2pro/plz
//
// Do not remove or change the type signature.
//
//go:linkname reflect_mapassign reflect.mapassign0
internal static void reflect_mapassign(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @unsafe.Pointer key, @unsafe.Pointer elem) {
    ref var t = ref Ꮡt.DerefOrNull();

    @unsafe.Pointer Δp = (uintptr)mapassign(Ꮡt, Ꮡh, key);
    typedmemmove(t.Elem, Δp, elem);
}

//go:linkname reflect_mapassign_faststr reflect.mapassign_faststr0
internal static void reflect_mapassign_faststr(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @string key, @unsafe.Pointer elem) {
    ref var t = ref Ꮡt.DerefOrNull();

    @unsafe.Pointer Δp = (uintptr)mapassign_faststr(Ꮡt, Ꮡh, key);
    typedmemmove(t.Elem, Δp, elem);
}

//go:linkname reflect_mapdelete reflect.mapdelete
internal static void reflect_mapdelete(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @unsafe.Pointer key) {
    mapdelete(Ꮡt, Ꮡh, key);
}

//go:linkname reflect_mapdelete_faststr reflect.mapdelete_faststr
internal static void reflect_mapdelete_faststr(ж<maptype> Ꮡt, ж<hmap> Ꮡh, @string key) {
    mapdelete_faststr(Ꮡt, Ꮡh, key);
}

// reflect_mapiterinit is for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/modern-go/reflect2
//   - gitee.com/quant1x/gox
//   - github.com/v2pro/plz
//   - github.com/wI2L/jettison
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_mapiterinit reflect.mapiterinit
internal static void reflect_mapiterinit(ж<maptype> Ꮡt, ж<hmap> Ꮡh, ж<hiter> Ꮡit) {
    mapiterinit(Ꮡt, Ꮡh, Ꮡit);
}

// reflect_mapiternext is for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//   - github.com/modern-go/reflect2
//   - github.com/goccy/go-json
//   - github.com/v2pro/plz
//   - github.com/wI2L/jettison
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_mapiternext reflect.mapiternext
internal static void reflect_mapiternext(ж<hiter> Ꮡit) {
    mapiternext(Ꮡit);
}

// reflect_mapiterkey is for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/goccy/go-json
//   - gonum.org/v1/gonum
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_mapiterkey reflect.mapiterkey
internal static @unsafe.Pointer reflect_mapiterkey(ref hiter it) {
    return it.key;
}

// reflect_mapiterelem is for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/goccy/go-json
//   - gonum.org/v1/gonum
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_mapiterelem reflect.mapiterelem
internal static @unsafe.Pointer reflect_mapiterelem(ref hiter it) {
    return it.elem;
}

// reflect_maplen is for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/goccy/go-json
//   - github.com/wI2L/jettison
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_maplen reflect.maplen
internal static nint reflect_maplen(ж<hmap> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    if (Ꮡh == nil) {
        return 0;
    }
    if (raceenabled) {
        var callerpc = getcallerpc();
        racereadpc(new @unsafe.Pointer(Ꮡh), callerpc, abi.FuncPCABIInternal(reflect_maplen));
    }
    return h.count;
}

//go:linkname reflect_mapclear reflect.mapclear
internal static void reflect_mapclear(ж<maptype> Ꮡt, ж<hmap> Ꮡh) {
    mapclear(Ꮡt, Ꮡh);
}

//go:linkname reflectlite_maplen internal/reflectlite.maplen
internal static nint reflectlite_maplen(ж<hmap> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    if (Ꮡh == nil) {
        return 0;
    }
    if (raceenabled) {
        var callerpc = getcallerpc();
        racereadpc(new @unsafe.Pointer(Ꮡh), callerpc, abi.FuncPCABIInternal(reflect_maplen));
    }
    return h.count;
}

// mapinitnoop is a no-op function known the Go linker; if a given global
// map (of the right size) is determined to be dead, the linker will
// rewrite the relocation (from the package init func) from the outlined
// map init function to this symbol. Defined in assembly so as to avoid
// complications with instrumentation (coverage, etc).
internal static partial void mapinitnoop();

// mapclone for implementing maps.Clone
//
//go:linkname mapclone maps.clone
internal static any mapclone(any mʗp) {
    ref var m = ref heap(mʗp, out var Ꮡm);

    var e = efaceOf(Ꮡm);
    e.Value.data = new @unsafe.Pointer(mapclone2((~e)._type.Reinterpret<_type, maptype>(), (ж<hmap>)(uintptr)((~e).data)));
    return m;
}

// moveToBmap moves a bucket from src to dst. It returns the destination bucket or new destination bucket if it overflows
// and the pos that the next key/value will be written, if pos == bucketCnt means needs to written in overflow bucket.
internal static (ж<bmap>, nint) moveToBmap(ж<maptype> Ꮡt, ж<hmap> Ꮡh, ж<bmap> Ꮡdst, nint pos, ж<bmap> Ꮡsrc) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var h = ref Ꮡh.DerefOrNull();
    ref var dst = ref Ꮡdst.DerefOrNull();
    ref var src = ref Ꮡsrc.DerefOrNull();

    for (nint i = 0; i < abi.MapBucketCount; i++) {
        if (isEmpty(src.tophash[i])) {
            continue;
        }
        for (; pos < abi.MapBucketCount; pos++) {
            if (isEmpty(dst.tophash[pos])) {
                break;
            }
        }
        if (pos == abi.MapBucketCount) {
            Ꮡdst = h.newoverflow(Ꮡt, Ꮡdst); dst = ref Ꮡdst.DerefOrNull();
            pos = 0;
        }
        @unsafe.Pointer srcK = (uintptr)add(new @unsafe.Pointer(Ꮡsrc), dataOffset + (uintptr)i * (uintptr)t.KeySize);
        @unsafe.Pointer srcEle = (uintptr)add(new @unsafe.Pointer(Ꮡsrc), dataOffset + (uintptr)abi.MapBucketCount * (uintptr)t.KeySize + (uintptr)i * (uintptr)t.ValueSize);
        @unsafe.Pointer dstK = (uintptr)add(new @unsafe.Pointer(Ꮡdst), dataOffset + (uintptr)pos * (uintptr)t.KeySize);
        @unsafe.Pointer dstEle = (uintptr)add(new @unsafe.Pointer(Ꮡdst), dataOffset + (uintptr)abi.MapBucketCount * (uintptr)t.KeySize + (uintptr)pos * (uintptr)t.ValueSize);
        dst.tophash[pos] = src.tophash[i];
        if (t.IndirectKey()){
            srcK = ~(ж<@unsafe.Pointer>)(uintptr)(srcK);
            if (t.NeedKeyUpdate()) {
                @unsafe.Pointer kStore = (uintptr)newobject(t.Key);
                typedmemmove(t.Key, kStore, srcK);
                srcK = kStore;
            }
            // Note: if NeedKeyUpdate is false, then the memory
            // used to store the key is immutable, so we can share
            // it between the original map and its clone.
            ((ж<@unsafe.Pointer>)(uintptr)(dstK)).Value = srcK;
        } else {
            typedmemmove(t.Key, dstK, srcK);
        }
        if (t.IndirectElem()){
            srcEle = ~(ж<@unsafe.Pointer>)(uintptr)(srcEle);
            @unsafe.Pointer eStore = (uintptr)newobject(t.Elem);
            typedmemmove(t.Elem, eStore, srcEle);
            ((ж<@unsafe.Pointer>)(uintptr)(dstEle)).Value = eStore;
        } else {
            typedmemmove(t.Elem, dstEle, srcEle);
        }
        pos++;
        h.count++;
    }
    return (Ꮡdst, pos);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string concurrentMapCloneAndMapˢ = "concurrent map clone and map write"u8;

internal static ж<hmap> mapclone2(ж<maptype> Ꮡt, ж<hmap> Ꮡsrc) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var src = ref Ꮡsrc.DerefOrNull();

    nint hint = src.count;
    if (overLoadFactor(hint, src.B)) {
        // Note: in rare cases (e.g. during a same-sized grow) the map
        // can be overloaded. Make sure we don't allocate a destination
        // bucket array larger than the source bucket array.
        // This will cause the cloned map to be overloaded also,
        // but that's better than crashing. See issue 69110.
        hint = (nint)((uintptr)loadFactorNum * (bucketShift(src.B) / (uintptr)loadFactorDen));
    }
    var dst = makemap(Ꮡt, hint, nil);
    dst.Value.hash0 = src.hash0;
    dst.Value.nevacuate = 0;
    // flags do not need to be copied here, just like a new map has no flags.
    if (src.count == 0) {
        return dst;
    }
    if ((uint8)(src.flags & (uint8)hashWriting) != 0) {
        fatal(concurrentMapCloneAndMapˢ);
    }
    if (src.B == 0 && !(t.IndirectKey() && t.NeedKeyUpdate()) && !t.IndirectElem()) {
        // Quick copy for small maps.
        dst.Value.buckets = (uintptr)newobject(t.Bucket);
        dst.Value.count = src.count;
        typedmemmove(t.Bucket, (~dst).buckets, src.buckets);
        return dst;
    }
    if ((~dst).B == 0) {
        dst.Value.buckets = (uintptr)newobject(t.Bucket);
    }
    nint dstArraySize = (nint)bucketShift((~dst).B);
    nint srcArraySize = (nint)bucketShift(src.B);
    for (nint i = 0; i < dstArraySize; i++) {
        var dstBmap = (ж<bmap>)(uintptr)(add((~dst).buckets, (uintptr)(i * (nint)t.BucketSize)));
        nint pos = 0;
        for (nint j = 0; j < srcArraySize; j += dstArraySize) {
            var srcBmap = (ж<bmap>)(uintptr)(add(src.buckets, (uintptr)((i + j) * (nint)t.BucketSize)));
            while (srcBmap != nil) {
                (dstBmap, pos) = moveToBmap(Ꮡt, dst, dstBmap, pos, srcBmap);
                srcBmap = srcBmap.overflow(Ꮡt);
            }
        }
    }
    if (src.oldbuckets == nil) {
        return dst;
    }
    var oldB = src.B;
    @unsafe.Pointer srcOldbuckets = src.oldbuckets;
    if (!src.sameSizeGrow()) {
        oldB--;
    }
    nint oldSrcArraySize = (nint)bucketShift(oldB);
    for (nint i = 0; i < oldSrcArraySize; i++) {
        var srcBmap = (ж<bmap>)(uintptr)(add(srcOldbuckets, (uintptr)(i * (nint)t.BucketSize)));
        if (evacuated(ref (srcBmap).DerefOrNull())) {
            continue;
        }
        if (oldB >= (~dst).B) {
            // main bucket bits in dst is less than oldB bits in src
            var dstBmap = (ж<bmap>)(uintptr)(add((~dst).buckets, ((uintptr)((uintptr)i & bucketMask((~dst).B))) * (uintptr)t.BucketSize));
            while (dstBmap.overflow(Ꮡt) != nil) {
                dstBmap = dstBmap.overflow(Ꮡt);
            }
            nint pos = 0;
            while (srcBmap != nil) {
                (dstBmap, pos) = moveToBmap(Ꮡt, dst, dstBmap, pos, srcBmap);
                srcBmap = srcBmap.overflow(Ꮡt);
            }
            continue;
        }
        // oldB < dst.B, so a single source bucket may go to multiple destination buckets.
        // Process entries one at a time.
        while (srcBmap != nil) {
            // move from oldBlucket to new bucket
            for (var iΔ1 = (uintptr)0; iΔ1 < abi.MapBucketCount; iΔ1++) {
                if (isEmpty((~srcBmap).tophash[(nint)(iΔ1)])) {
                    continue;
                }
                if ((uint8)(src.flags & (uint8)hashWriting) != 0) {
                    fatal(concurrentMapCloneAndMapˢ);
                }
                @unsafe.Pointer srcK = (uintptr)add(new @unsafe.Pointer(srcBmap), dataOffset + iΔ1 * (uintptr)t.KeySize);
                if (t.IndirectKey()) {
                    srcK = ((ж<@unsafe.Pointer>)(uintptr)(srcK)).Value;
                }
                @unsafe.Pointer srcEle = (uintptr)add(new @unsafe.Pointer(srcBmap), dataOffset + (uintptr)abi.MapBucketCount * (uintptr)t.KeySize + iΔ1 * (uintptr)t.ValueSize);
                if (t.IndirectElem()) {
                    srcEle = ((ж<@unsafe.Pointer>)(uintptr)(srcEle)).Value;
                }
                @unsafe.Pointer dstEle = (uintptr)mapassign(Ꮡt, dst, srcK);
                typedmemmove(t.Elem, dstEle, srcEle);
            }
            srcBmap = srcBmap.overflow(Ꮡt);
        }
    }
    return dst;
}

// keys for implementing maps.keys
//
//go:linkname keys maps.keys
internal static void keys(any mʗp, @unsafe.Pointer Δp) {
    ref var m = ref heap(mʗp, out var Ꮡm);

    var e = efaceOf(Ꮡm);
    var t = (~e)._type.Reinterpret<_type, maptype>();
    var h = (ж<hmap>)(uintptr)((~e).data);
    if (h == nil || (~h).count == 0) {
        return;
    }
    var s = (ж<Δsliceᴛ>)(uintptr)(Δp);
    nint r = (nint)rand();
    var offset = (uint8)((nint)(r.Rsh((uint64)((~h).B)) & (nint)(abi.MapBucketCount - 1)));
    if ((~h).B == 0) {
        copyKeys(t, ref (h).DerefOrNull(), (ж<bmap>)(uintptr)((~h).buckets), ref (s).DerefOrNull(), offset);
        return;
    }
    nint arraySize = (nint)bucketShift((~h).B);
    @unsafe.Pointer buckets = h.Value.buckets;
    for (nint i = 0; i < arraySize; i++) {
        nint bucket = (nint)((i + r) & (arraySize - 1));
        var b = (ж<bmap>)(uintptr)(add(buckets, (uintptr)bucket * (uintptr)(~t).BucketSize));
        copyKeys(t, ref (h).DerefOrNull(), b, ref (s).DerefOrNull(), offset);
    }
    if (h.growing()) {
        nint oldArraySize = (nint)h.noldbuckets();
        for (nint i = 0; i < oldArraySize; i++) {
            nint bucket = (nint)((i + r) & (oldArraySize - 1));
            var b = (ж<bmap>)(uintptr)(add((~h).oldbuckets, (uintptr)bucket * (uintptr)(~t).BucketSize));
            if (evacuated(ref (b).DerefOrNull())) {
                continue;
            }
            copyKeys(t, ref (h).DerefOrNull(), b, ref (s).DerefOrNull(), offset);
        }
    }
    return;
}

internal static void copyKeys(ж<maptype> Ꮡt, ref hmap h, ж<bmap> Ꮡb, ref Δsliceᴛ s, uint8 offset) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    while (Ꮡb != nil) {
        for (var i = (uintptr)0; i < abi.MapBucketCount; i++) {
            var offi = (uintptr)((i + (uintptr)offset) & (uintptr)(abi.MapBucketCount - 1));
            if (isEmpty(b.tophash[(nint)(offi)])) {
                continue;
            }
            if ((uint8)(h.flags & (uint8)hashWriting) != 0) {
                fatal(concurrentMapReadAndMapˢ);
            }
            @unsafe.Pointer k = (uintptr)add(new @unsafe.Pointer(Ꮡb), dataOffset + offi * (uintptr)t.KeySize);
            if (t.IndirectKey()) {
                k = ((ж<@unsafe.Pointer>)(uintptr)(k)).Value;
            }
            if (s.len >= s.cap) {
                fatal(concurrentMapReadAndMapˢ);
            }
            typedmemmove(t.Key, (uintptr)add(s.Δarray, (uintptr)s.len * (uintptr)t.Key.Size()), k);
            s.len++;
        }
        Ꮡb = Ꮡb.overflow(Ꮡt); b = ref Ꮡb.DerefOrNull();
    }
}

// values for implementing maps.values
//
//go:linkname values maps.values
internal static void values(any mʗp, @unsafe.Pointer Δp) {
    ref var m = ref heap(mʗp, out var Ꮡm);

    var e = efaceOf(Ꮡm);
    var t = (~e)._type.Reinterpret<_type, maptype>();
    var h = (ж<hmap>)(uintptr)((~e).data);
    if (h == nil || (~h).count == 0) {
        return;
    }
    var s = (ж<Δsliceᴛ>)(uintptr)(Δp);
    nint r = (nint)rand();
    var offset = (uint8)((nint)(r.Rsh((uint64)((~h).B)) & (nint)(abi.MapBucketCount - 1)));
    if ((~h).B == 0) {
        copyValues(t, ref (h).DerefOrNull(), (ж<bmap>)(uintptr)((~h).buckets), ref (s).DerefOrNull(), offset);
        return;
    }
    nint arraySize = (nint)bucketShift((~h).B);
    @unsafe.Pointer buckets = h.Value.buckets;
    for (nint i = 0; i < arraySize; i++) {
        nint bucket = (nint)((i + r) & (arraySize - 1));
        var b = (ж<bmap>)(uintptr)(add(buckets, (uintptr)bucket * (uintptr)(~t).BucketSize));
        copyValues(t, ref (h).DerefOrNull(), b, ref (s).DerefOrNull(), offset);
    }
    if (h.growing()) {
        nint oldArraySize = (nint)h.noldbuckets();
        for (nint i = 0; i < oldArraySize; i++) {
            nint bucket = (nint)((i + r) & (oldArraySize - 1));
            var b = (ж<bmap>)(uintptr)(add((~h).oldbuckets, (uintptr)bucket * (uintptr)(~t).BucketSize));
            if (evacuated(ref (b).DerefOrNull())) {
                continue;
            }
            copyValues(t, ref (h).DerefOrNull(), b, ref (s).DerefOrNull(), offset);
        }
    }
    return;
}

internal static void copyValues(ж<maptype> Ꮡt, ref hmap h, ж<bmap> Ꮡb, ref Δsliceᴛ s, uint8 offset) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    while (Ꮡb != nil) {
        for (var i = (uintptr)0; i < abi.MapBucketCount; i++) {
            var offi = (uintptr)((i + (uintptr)offset) & (uintptr)(abi.MapBucketCount - 1));
            if (isEmpty(b.tophash[(nint)(offi)])) {
                continue;
            }
            if ((uint8)(h.flags & (uint8)hashWriting) != 0) {
                fatal(concurrentMapReadAndMapˢ);
            }
            @unsafe.Pointer ele = (uintptr)add(new @unsafe.Pointer(Ꮡb), dataOffset + (uintptr)abi.MapBucketCount * (uintptr)t.KeySize + offi * (uintptr)t.ValueSize);
            if (t.IndirectElem()) {
                ele = ((ж<@unsafe.Pointer>)(uintptr)(ele)).Value;
            }
            if (s.len >= s.cap) {
                fatal(concurrentMapReadAndMapˢ);
            }
            typedmemmove(t.Elem, (uintptr)add(s.Δarray, (uintptr)s.len * (uintptr)t.Elem.Size()), ele);
            s.len++;
        }
        Ꮡb = Ꮡb.overflow(Ꮡt); b = ref Ꮡb.DerefOrNull();
    }
}

} // end runtime_package
