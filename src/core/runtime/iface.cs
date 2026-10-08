// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

internal static UntypedInt itabInitSize => 512;

internal static ж<mutex> ᏑitabLock = new StandardBox<mutex>(new mutex(nil));
internal static ref mutex itabLock => ref ᏑitabLock.Value;                         // lock for accessing itab table
internal static ж<ж<itabTableType>> ᏑitabTable = new StandardBox<ж<itabTableType>>(default(ж<itabTableType>));
internal static ref ж<itabTableType> itabTable => ref ᏑitabTable.ValueSlot;
internal static void initᴛitabTable() { itabTable = ᏑitabTableInit; }                    // pointer to current table
internal static ж<itabTableType> ᏑitabTableInit = new StandardBox<itabTableType>(new itabTableType(size: itabInitSize));
internal static ref itabTableType itabTableInit => ref ᏑitabTableInit.Value; // starter table

// Note: change the formula in the mallocgc call in itabAdd if you change these fields.
partial struct itabTableType {
    internal uintptr size;             // length of entries array. Always a power of 2.
    internal uintptr count;             // current number of filled entries.
    internal array<ж<itab>> entries = new(itabInitSize); // really [size] large
}

internal static uintptr itabHashFunc(ref abiꓸInterfaceType inter, ref _type typ) {
    // compiler has provided some good hash codes for us.
    return (uintptr)((uint32)(inter.Type.Hash ^ typ.Hash));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string internalErrorMisuseOfˢ = "internal error - misuse of itab"u8;

// getitab should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname getitab
internal static ж<itab> getitab(ж<interfacetype> Ꮡinter, ж<_type> Ꮡtyp, bool canfail) {
    ref var inter = ref Ꮡinter.DerefOrNull();
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (len(inter.Methods) == 0) {
        @throw(internalErrorMisuseOfˢ);
    }
    // easy case
    if ((abi.TFlag)(typ.TFlag & abi.TFlagUncommon) == 0) {
        if (canfail) {
            return default!;
        }
        var name = toRType(Ꮡinter.of(interfacetype.ᏑType)).nameOff(inter.Methods[0].Name);
        throw panic(Ꮡ(new TypeAssertionError(nil, Ꮡtyp, Ꮡinter.of(interfacetype.ᏑType), name.Name())));
    }
    ж<itab> m = default!;
    // First, look in the existing table to see if we can find the itab we need.
    // This is by far the most common case, so do it without locks.
    // Use atomic to ensure we see any previous writes done by the thread
    // that updates the itabTable field (with atomic.Storep in itabAdd).
    var t = (ж<itabTableType>)(uintptr)(atomic.Loadp(@unsafe.Pointer.FromBox(ᏑitabTable)));
    {
        m = t.find(Ꮡinter, Ꮡtyp); if (m != nil) {
            goto finish;
        }
    }
    // Not found.  Grab the lock and try again.
    @lock(ᏑitabLock);
    {
        m = itabTable.find(Ꮡinter, Ꮡtyp); if (m != nil) {
            unlock(ᏑitabLock);
            goto finish;
        }
    }
    // Entry doesn't exist yet. Make a new entry & add it.
    m = (ж<itab>)(uintptr)(persistentalloc(/* unsafe.Sizeof(itab{}) */ (uintptr)32 + (uintptr)(len(inter.Methods) - 1) * (uintptr)goarch.PtrSize, 0, Ꮡmemstats.of(mstats.Ꮡother_sys)));
    m.Value.Inter = Ꮡinter;
    m.Value.Type = Ꮡtyp;
    // The hash is used in type switches. However, compiler statically generates itab's
    // for all interface/type pairs used in switches (which are added to itabTable
    // in itabsinit). The dynamically-generated itab's never participate in type switches,
    // and thus the hash is irrelevant.
    // Note: m.Hash is _not_ the hash used for the runtime itabTable hash table.
    m.Value.Hash = 0;
    itabInit(m, true);
    itabAdd(m);
    unlock(ᏑitabLock);
finish:
    if ((~m).Fun[0] != 0) {
        return m;
    }
    if (canfail) {
        return default!;
    }
    // this can only happen if the conversion
    // was already done once using the , ok form
    // and we have a cached negative result.
    // The cached result doesn't record which
    // interface function was missing, so initialize
    // the itab again to get the missing function name.
    throw panic(Ꮡ(new TypeAssertionError(concrete: Ꮡtyp, asserted: Ꮡinter.of(interfacetype.ᏑType), missingMethod: itabInit(m, false))));
}

// find finds the given interface/type pair in t.
// Returns nil if the given interface/type pair isn't present.
internal static ж<itab> find(this ж<itabTableType> Ꮡt, ж<interfacetype> Ꮡinter, ж<_type> Ꮡtyp) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Implemented using quadratic probing.
    // Probe sequence is h(i) = h0 + i*(i+1)/2 mod 2^k.
    // We're guaranteed to hit all table entries using this probe sequence.
    var mask = t.size - 1;
    var h = (uintptr)(itabHashFunc(ref (Ꮡinter).DerefOrNull(), ref (Ꮡtyp).DerefOrNull()) & mask);
    for (var i = (uintptr)1; ᐧ ; i++) {
        var Δp = (ж<ж<itab>>)(uintptr)(add(@unsafe.Pointer.FromPinnedBox(Ꮡt.of(itabTableType.Ꮡentries)), h * (uintptr)goarch.PtrSize));
        // Use atomic read here so if we see m != nil, we also see
        // the initializations of the fields of m.
        // m := *p
        var m = (ж<itab>)(uintptr)(atomic.Loadp(@unsafe.Pointer.FromBox(Δp)));
        if (m == nil) {
            return default!;
        }
        if ((~m).Inter == Ꮡinter && (~m).Type == Ꮡtyp) {
            return m;
        }
        h += i;
        h &= (uintptr)(mask);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mismatchedCountDuringˢ = "mismatched count during itab table copy"u8;

// itabAdd adds the given itab to the itab hash table.
// itabLock must be held.
internal static void itabAdd(ж<itab> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    // Bugs can lead to calling this while mallocing is set,
    // typically because this is called while panicking.
    // Crash reliably, rather than only when we need to grow
    // the hash table.
    if ((~(~getg()).m).mallocing != 0) {
        @throw(mallocDeadlockˢ);
    }
    var t = itabTable;
    if ((~t).count >= 3 * ((~t).size / 4)) {
        // 75% load factor
        // Grow hash table.
        // t2 = new(itabTableType) + some additional entries
        // We lie and tell malloc we want pointer-free memory because
        // all the pointed-to values are not in the heap.
        var t2 = (ж<itabTableType>)(uintptr)(mallocgc((2 + 2 * (~t).size) * (uintptr)goarch.PtrSize, nil, true));
        t2.Value.size = (~t).size * 2;
        // Copy over entries.
        // Note: while copying, other threads may look for an itab and
        // fail to find it. That's ok, they will then try to get the itab lock
        // and as a consequence wait until this copying is complete.
        iterate_itabs(t2.add);
        if ((~t2).count != (~t).count) {
            @throw(mismatchedCountDuringˢ);
        }
        // Publish new hash table. Use an atomic write: see comment in getitab.
        atomicstorep(@unsafe.Pointer.FromBox(ᏑitabTable), @unsafe.Pointer.FromPinnedBox(t2));
        // Adopt the new table as our own.
        t = itabTable;
    }
    // Note: the old table can be GC'ed here.
    t.add(Ꮡm);
}

// add adds the given itab to itab table t.
// itabLock must be held.
internal static void add(this ж<itabTableType> Ꮡt, ж<itab> Ꮡm) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    // See comment in find about the probe sequence.
    // Insert new itab in the first empty spot in the probe sequence.
    var mask = t.size - 1;
    var h = (uintptr)(itabHashFunc(ref (m.Inter).DerefOrNull(), ref (m.Type).DerefOrNull()) & mask);
    for (var i = (uintptr)1; ᐧ ; i++) {
        var Δp = (ж<ж<itab>>)(uintptr)(add(@unsafe.Pointer.FromPinnedBox(Ꮡt.of(itabTableType.Ꮡentries)), h * (uintptr)goarch.PtrSize));
        var m2 = Δp.ValueSlot;
        if (m2 == Ꮡm) {
            // A given itab may be used in more than one module
            // and thanks to the way global symbol resolution works, the
            // pointed-to itab may already have been inserted into the
            // global 'hash'.
            return;
        }
        if (m2 == nil) {
            // Use atomic write here so if a reader sees m, it also
            // sees the correctly initialized fields of m.
            // NoWB is ok because m is not in heap memory.
            // *p = m
            atomic.StorepNoWB(@unsafe.Pointer.FromBox(Δp), @unsafe.Pointer.FromPinnedBox(Ꮡm));
            t.count++;
            return;
        }
        h += i;
        h &= (uintptr)(mask);
    }
}

// itabInit fills in the m.Fun array with all the code pointers for
// the m.Inter/m.Type pair. If the type does not implement the interface,
// it sets m.Fun[0] to 0 and returns the name of an interface function that is missing.
// If !firstTime, itabInit will not write anything to m.Fun (see issue 65962).
// It is ok to call this multiple times on the same m, even concurrently
// (although it will only be called once with firstTime==true).
internal static unsafe @string itabInit(ж<itab> Ꮡm, bool firstTime) {
    ref var m = ref Ꮡm.DerefOrNull();

    var inter = m.Inter;
    var typ = m.Type;
    var x = typ.Uncommon();
    // both inter and typ have method sorted by name,
    // and interface names are unique,
    // so can iterate over both in lock step;
    // the loop is O(ni+nt) not O(ni*nt).
    nint ni = len((~inter).Methods);
    nint nt = (nint)(~x).Mcount;
    var xmhdr = new slice<abi.Method>(new ReadOnlySpan<abi.Method>((abi.Method*)(uintptr)(add(@unsafe.Pointer.FromPinnedBox(x), (uintptr)(~x).Moff)), (int)(nt)));
    nint j = 0;
    var methods = (false ? default(slice<@unsafe.Pointer>) : throw panic("go2cs: a slice view (*[N]unsafe.Pointer)(p)[...] over a managed element has no managed aliasing pair (iface.go:217)"));
    @unsafe.Pointer fun0 = default!;
imethods:
    for (nint k = 0; k < ni; k++) {
        var i = Ꮡ((~inter).Methods, k);
        var itype = toRType(inter.of(abiꓸInterfaceType.ᏑType)).typeOff((~i).Typ);
        var name = toRType(inter.of(abiꓸInterfaceType.ᏑType)).nameOff((~i).Name);
        @string iname = name.Name();
        @string ipkg = pkgPath(name);
        if (ipkg == ""u8) {
            ipkg = (~inter).PkgPath.Name();
        }
        for (; j < nt; j++) {
            var t = Ꮡ(xmhdr, j);
            var rtyp = toRType(typ);
            var tname = rtyp.nameOff((~t).Name);
            if (rtyp.typeOff((~t).Mtyp) == itype && tname.Name() == iname) {
                @string pkgPathΔ1 = pkgPath(tname);
                if (pkgPathΔ1 == ""u8) {
                    pkgPathΔ1 = rtyp.nameOff((~x).PkgPath).Name();
                }
                if (tname.IsExported() || pkgPathΔ1 == ipkg) {
                    @unsafe.Pointer ifn = (uintptr)rtyp.textOff((~t).Ifn);
                    if (k == 0){
                        fun0 = ifn; // we'll set m.Fun[0] at the end
                    } else 
                    if (firstTime) {
                        methods[k] = ifn;
                    }
                    goto continue_imethods;
                }
            }
        }
        // didn't find method
        // Leaves m.Fun[0] set to 0.
        return iname;
continue_imethods:;
    }
break_imethods:;
    if (firstTime) {
        m.Fun[0] = (uintptr)fun0;
    }
    return ""u8;
}

internal static void itabsinit() {
    lockInit(ᏑitabLock, lockRankItab);
    @lock(ᏑitabLock);
    foreach (var (_, md) in activeModules()) {
        foreach (var (_, i) in (~md).itablinks) {
            itabAdd(i);
        }
    }
    unlock(ᏑitabLock);
}

// panicdottypeE is called when doing an e.(T) conversion and the conversion fails.
// have = the dynamic type we have.
// want = the static type we're trying to convert to.
// iface = the static type we're converting from.
internal static void panicdottypeE(ж<_type> Ꮡhave, ж<_type> Ꮡwant, ж<_type> Ꮡiface) {
    throw panic(Ꮡ(new TypeAssertionError(Ꮡiface, Ꮡhave, Ꮡwant, ""u8)));
}

// panicdottypeI is called when doing an i.(T) conversion and the conversion fails.
// Same args as panicdottypeE, but "have" is the dynamic itab we have.
internal static void panicdottypeI(ж<itab> Ꮡhave, ж<_type> Ꮡwant, ж<_type> Ꮡiface) {
    ref var have = ref Ꮡhave.DerefOrNull();

    ж<_type> t = default!;
    if (Ꮡhave != nil) {
        t = have.Type;
    }
    panicdottypeE(t, Ꮡwant, Ꮡiface);
}

// panicnildottype is called when doing an i.(T) conversion and the interface i is nil.
// want = the static type we're trying to convert to.
internal static void panicnildottype(ж<_type> Ꮡwant) {
    throw panic(Ꮡ(new TypeAssertionError(nil, nil, Ꮡwant, ""u8)));
}

partial struct uint16InterfacePtr /*num:uint16*/;

partial struct uint32InterfacePtr /*num:uint32*/;

partial struct uint64InterfacePtr /*num:uint64*/;

partial struct stringInterfacePtr /*@string*/;

partial struct sliceInterfacePtr /*[]byte*/;

// TODO: Add the static type we're converting from as well.
// It might generate a better error message.
// Just to match other nil conversion errors, we don't for now.
internal static ж<any> Ꮡuint16Eface = new StandardBox<any>(((uint16InterfacePtr)0));
internal static ref any uint16Eface => ref Ꮡuint16Eface.ValueSlot;
internal static ж<any> Ꮡuint32Eface = new StandardBox<any>(((uint32InterfacePtr)0));
internal static ref any uint32Eface => ref Ꮡuint32Eface.ValueSlot;
internal static ж<any> Ꮡuint64Eface = new StandardBox<any>(((uint64InterfacePtr)0));
internal static ref any uint64Eface => ref Ꮡuint64Eface.ValueSlot;
internal static ж<any> ᏑstringEface = new StandardBox<any>(((stringInterfacePtr)(@string)""u8));
internal static ref any stringEface => ref ᏑstringEface.ValueSlot;
internal static ж<any> ᏑsliceEface = new StandardBox<any>(((sliceInterfacePtr)default!));
internal static ref any sliceEface => ref ᏑsliceEface.ValueSlot;
internal static ж<_type> uint16Type = (~efaceOf(Ꮡuint16Eface))._type;
internal static ж<_type> uint32Type = (~efaceOf(Ꮡuint32Eface))._type;
internal static ж<_type> uint64Type = (~efaceOf(Ꮡuint64Eface))._type;
internal static ж<_type> stringType = (~efaceOf(ᏑstringEface))._type;
internal static ж<_type> sliceType = (~efaceOf(ᏑsliceEface))._type;

// The conv and assert functions below do very similar things.
// The convXXX functions are guaranteed by the compiler to succeed.
// The assertXXX functions may fail (either panicking or returning false,
// depending on whether they are 1-result or 2-result).
// The convXXX functions succeed on a nil input, whereas the assertXXX
// functions fail on a nil input.

// convT converts a value of type t, which is pointed to by v, to a pointer that can
// be used as the second word of an interface value.
internal static @unsafe.Pointer convT(ж<_type> Ꮡt, @unsafe.Pointer v) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (raceenabled) {
        raceReadObjectPC(ref (Ꮡt).DerefOrNull(), v, sys.GetCallerPC(), abi.FuncPCABIInternal(convT));
    }
    if (msanenabled) {
        msanread(v, t.Size_);
    }
    if (asanenabled) {
        asanread(v, t.Size_);
    }
    @unsafe.Pointer x = (uintptr)mallocgc(t.Size_, Ꮡt, true);
    typedmemmove(Ꮡt, x, v);
    return x;
}

internal static @unsafe.Pointer convTnoptr(ж<_type> Ꮡt, @unsafe.Pointer v) {
    ref var t = ref Ꮡt.DerefOrNull();

    // TODO: maybe take size instead of type?
    if (raceenabled) {
        raceReadObjectPC(ref (Ꮡt).DerefOrNull(), v, sys.GetCallerPC(), abi.FuncPCABIInternal(convTnoptr));
    }
    if (msanenabled) {
        msanread(v, t.Size_);
    }
    if (asanenabled) {
        asanread(v, t.Size_);
    }
    @unsafe.Pointer x = (uintptr)mallocgc(t.Size_, Ꮡt, false);
    memmove(x, v, t.Size_);
    return x;
}

internal static @unsafe.Pointer /*x*/ convT16(uint16 val) {
    @unsafe.Pointer x = default!;

    if (val < (uint16)len(staticuint64s)){
        x = @unsafe.Pointer.FromPinnedBox(Ꮡstaticuint64s.at<uint64>((nint)(val)));
        if (goarch.BigEndian) {
            x = (uintptr)add(x, 6);
        }
    } else {
        x = (uintptr)mallocgc(2, uint16Type, false);
        ((ж<uint16>)(uintptr)(x)).Value = val;
    }
    return x;
}

internal static @unsafe.Pointer /*x*/ convT32(uint32 val) {
    @unsafe.Pointer x = default!;

    if (val < (uint32)len(staticuint64s)){
        x = @unsafe.Pointer.FromPinnedBox(Ꮡstaticuint64s.at<uint64>((ulong)(val)));
        if (goarch.BigEndian) {
            x = (uintptr)add(x, 4);
        }
    } else {
        x = (uintptr)mallocgc(4, uint32Type, false);
        ((ж<uint32>)(uintptr)(x)).Value = val;
    }
    return x;
}

// convT64 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname convT64
internal static @unsafe.Pointer /*x*/ convT64(uint64 val) {
    @unsafe.Pointer x = default!;

    if (val < (uint64)len(staticuint64s)){
        x = @unsafe.Pointer.FromPinnedBox(Ꮡstaticuint64s.at<uint64>((ulong)(val)));
    } else {
        x = (uintptr)mallocgc(8, uint64Type, false);
        ((ж<uint64>)(uintptr)(x)).Value = val;
    }
    return x;
}

// convTstring should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname convTstring
internal static @unsafe.Pointer /*x*/ convTstring(@string val) {
    @unsafe.Pointer x = default!;

    if (val == ""u8){
        x = @unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0));
    } else {
        x = (uintptr)mallocgc(/* unsafe.Sizeof(val) */ (uintptr)16, stringType, true);
        ((ж<@string>)(uintptr)(x)).Value = val;
    }
    return x;
}

// convTslice should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname convTslice
internal static @unsafe.Pointer /*x*/ convTslice(slice<byte> valʗp) {
    @unsafe.Pointer x = default!;

    ref var val = ref heap(valʗp, out var Ꮡval);
    // Note: this must work for any element type, not just byte.
    if ((Ꮡval.Reinterpret<slice<byte>, Δsliceᴛ>()).Value.Δarray == nil){
        x = @unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0));
    } else {
        x = (uintptr)mallocgc(/* unsafe.Sizeof(val) */ (uintptr)24, sliceType, true);
        ((ж<slice<byte>>)(uintptr)(x)).ValueSlot = val;
    }
    return x;
}

internal static ж<itab> assertE2I(ж<interfacetype> Ꮡinter, ж<_type> Ꮡt) {
    if (Ꮡt == nil) {
        // explicit conversions require non-nil interface value.
        throw panic(Ꮡ(new TypeAssertionError(nil, nil, Ꮡinter.of(interfacetype.ᏑType), ""u8)));
    }
    return getitab(Ꮡinter, Ꮡt, false);
}

internal static ж<itab> assertE2I2(ж<interfacetype> Ꮡinter, ж<_type> Ꮡt) {
    if (Ꮡt == nil) {
        return default!;
    }
    return getitab(Ꮡinter, Ꮡt, true);
}

// typeAssert builds an itab for the concrete type t and the
// interface type s.Inter. If the conversion is not possible it
// panics if s.CanFail is false and returns nil if s.CanFail is true.
internal static ж<itab> typeAssert(ж<abi.TypeAssert> Ꮡs, ж<_type> Ꮡt) {
    ref var s = ref Ꮡs.DerefOrNull();
    ref var t = ref Ꮡt.DerefOrNull();

    ж<itab> tab = default!;
    if (Ꮡt == nil){
        if (!s.CanFail) {
            throw panic(Ꮡ(new TypeAssertionError(nil, nil, s.Inter.of(abiꓸInterfaceType.ᏑType), ""u8)));
        }
    } else {
        tab = getitab(s.Inter, Ꮡt, s.CanFail);
    }
    if (!abi.UseInterfaceSwitchCache(GOARCH)) {
        return tab;
    }
    // Maybe update the cache, so the next time the generated code
    // doesn't need to call into the runtime.
    if ((uint32)(cheaprand() & 1023) != 0) {
        // Only bother updating the cache ~1 in 1000 times.
        return tab;
    }
    // Load the current cache.
    var oldC = (ж<abi.TypeAssertCache>)(uintptr)(atomic.Loadp(@unsafe.Pointer.FromBox(Ꮡs.of(abi.TypeAssert.ᏑCache))));
    if ((uint32)(cheaprand() & (uint32)(~oldC).Mask) != 0) {
        // As cache gets larger, choose to update it less often
        // so we can amortize the cost of building a new cache.
        return tab;
    }
    // Make a new cache.
    var newC = buildTypeAssertCache(oldC, Ꮡt, tab);
    // Update cache. Use compare-and-swap so if multiple threads
    // are fighting to update the cache, at least one of their
    // updates will stick.
    atomic_casPointer(Ꮡ(@unsafe.Pointer.FromPinnedBox(s.Cache)), @unsafe.Pointer.FromPinnedBox(oldC), @unsafe.Pointer.FromPinnedBox(newC));
    return tab;
}

internal static ж<abi.TypeAssertCache> buildTypeAssertCache(ж<abi.TypeAssertCache> ᏑoldC, ж<_type> Ꮡtyp, ж<itab> Ꮡtab) {
    ref var oldC = ref ᏑoldC.DerefOrNull();

    var oldEntries = @unsafe.Slice(ᏑoldC.at(abi.TypeAssertCache.ᏑEntries, 0), oldC.Mask + 1);
    // Count the number of entries we need.
    nint n = 1;
    foreach (var (_, e) in oldEntries) {
        if (e.Typ != 0) {
            n++;
        }
    }
    // Figure out how big a table we need.
    // We need at least one more slot than the number of entries
    // so that we are guaranteed an empty slot (for termination).
    nint newN = n * 2; // make it at most 50% full
    newN = ((nint)1).Lsh((int64)(sys.Len64((uint64)(newN - 1)))); // round up to a power of 2
    // Allocate the new table.
    var newSize = /* unsafe.Sizeof(abi.TypeAssertCache{}) */ (uintptr)24 + (uintptr)(newN - 1) * /* unsafe.Sizeof(abi.TypeAssertCacheEntry{}) */ (uintptr)16;
    var newC = (ж<abi.TypeAssertCache>)(uintptr)(mallocgc(newSize, nil, true));
    newC.Value.Mask = (uintptr)(newN - 1);
    var newEntries = @unsafe.Slice(newC.at(abi.TypeAssertCache.ᏑEntries, 0), newN);
    // Fill the new table.
    var newEntriesʗ1 = newEntries;
    void addEntry(ж<_type> typΔ1, ж<itab> tabΔ1) {
        nint h = (nint)((nint)(~typΔ1).Hash & (newN - 1));
        while (ᐧ) {
            if (newEntriesʗ1[h].Typ == 0) {
                newEntriesʗ1[h].Typ = (uintptr)typΔ1;
                newEntriesʗ1[h].Itab = (uintptr)tabΔ1;
                return;
            }
            h = (nint)((h + 1) & (newN - 1));
        }
    }
    foreach (var (_, e) in oldEntries) {
        if (e.Typ != 0) {
            addEntry((ж<_type>)(uintptr)((@unsafe.Pointer)e.Typ), (ж<itab>)(uintptr)((@unsafe.Pointer)e.Itab));
        }
    }
    addEntry(Ꮡtyp, Ꮡtab);
    return newC;
}

// Empty type assert cache. Contains one entry with a nil Typ (which
// causes a cache lookup to fail immediately.)
internal static abi.TypeAssertCache emptyTypeAssertCache = new abi.TypeAssertCache(Mask: 0);

// interfaceSwitch compares t against the list of cases in s.
// If t matches case i, interfaceSwitch returns the case index i and
// an itab for the pair <t, s.Cases[i]>.
// If there is no match, return N,nil, where N is the number
// of cases.
internal static (nint, ж<itab>) interfaceSwitch(ж<abi.InterfaceSwitch> Ꮡs, ж<_type> Ꮡt) {
    ref var s = ref Ꮡs.DerefOrNull();
    ref var t = ref Ꮡt.DerefOrNull();

    var cases = @unsafe.Slice(Ꮡs.at(abi.InterfaceSwitch.ᏑCases, 0), s.NCases);
    // Results if we don't find a match.
    nint case_ = len(cases);
    ж<itab> tab = default!;
    // Look through each case in order.
    foreach (var (i, c) in cases) {
        tab = getitab(c, Ꮡt, true);
        if (tab != nil) {
            case_ = i;
            break;
        }
    }
    if (!abi.UseInterfaceSwitchCache(GOARCH)) {
        return (case_, tab);
    }
    // Maybe update the cache, so the next time the generated code
    // doesn't need to call into the runtime.
    if ((uint32)(cheaprand() & 1023) != 0) {
        // Only bother updating the cache ~1 in 1000 times.
        // This ensures we don't waste memory on switches, or
        // switch arguments, that only happen a few times.
        return (case_, tab);
    }
    // Load the current cache.
    var oldC = (ж<abi.InterfaceSwitchCache>)(uintptr)(atomic.Loadp(@unsafe.Pointer.FromBox(Ꮡs.of(abi.InterfaceSwitch.ᏑCache))));
    if ((uint32)(cheaprand() & (uint32)(~oldC).Mask) != 0) {
        // As cache gets larger, choose to update it less often
        // so we can amortize the cost of building a new cache
        // (that cost is linear in oldc.Mask).
        return (case_, tab);
    }
    // Make a new cache.
    var newC = buildInterfaceSwitchCache(oldC, Ꮡt, case_, tab);
    // Update cache. Use compare-and-swap so if multiple threads
    // are fighting to update the cache, at least one of their
    // updates will stick.
    atomic_casPointer(Ꮡ(@unsafe.Pointer.FromPinnedBox(s.Cache)), @unsafe.Pointer.FromPinnedBox(oldC), @unsafe.Pointer.FromPinnedBox(newC));
    return (case_, tab);
}

// buildInterfaceSwitchCache constructs an interface switch cache
// containing all the entries from oldC plus the new entry
// (typ,case_,tab).
internal static ж<abi.InterfaceSwitchCache> buildInterfaceSwitchCache(ж<abi.InterfaceSwitchCache> ᏑoldC, ж<_type> Ꮡtyp, nint case_, ж<itab> Ꮡtab) {
    ref var oldC = ref ᏑoldC.DerefOrNull();

    var oldEntries = @unsafe.Slice(ᏑoldC.at(abi.InterfaceSwitchCache.ᏑEntries, 0), oldC.Mask + 1);
    // Count the number of entries we need.
    nint n = 1;
    foreach (var (_, e) in oldEntries) {
        if (e.Typ != 0) {
            n++;
        }
    }
    // Figure out how big a table we need.
    // We need at least one more slot than the number of entries
    // so that we are guaranteed an empty slot (for termination).
    nint newN = n * 2; // make it at most 50% full
    newN = ((nint)1).Lsh((int64)(sys.Len64((uint64)(newN - 1)))); // round up to a power of 2
    // Allocate the new table.
    var newSize = /* unsafe.Sizeof(abi.InterfaceSwitchCache{}) */ (uintptr)32 + (uintptr)(newN - 1) * /* unsafe.Sizeof(abi.InterfaceSwitchCacheEntry{}) */ (uintptr)24;
    var newC = (ж<abi.InterfaceSwitchCache>)(uintptr)(mallocgc(newSize, nil, true));
    newC.Value.Mask = (uintptr)(newN - 1);
    var newEntries = @unsafe.Slice(newC.at(abi.InterfaceSwitchCache.ᏑEntries, 0), newN);
    // Fill the new table.
    var newEntriesʗ1 = newEntries;
    void addEntry(ж<_type> typΔ1, nint case_Δ1, ж<itab> tabΔ1) {
        nint h = (nint)((nint)(~typΔ1).Hash & (newN - 1));
        while (ᐧ) {
            if (newEntriesʗ1[h].Typ == 0) {
                newEntriesʗ1[h].Typ = (uintptr)typΔ1;
                newEntriesʗ1[h].Case = case_Δ1;
                newEntriesʗ1[h].Itab = (uintptr)tabΔ1;
                return;
            }
            h = (nint)((h + 1) & (newN - 1));
        }
    }
    foreach (var (_, e) in oldEntries) {
        if (e.Typ != 0) {
            addEntry((ж<_type>)(uintptr)((@unsafe.Pointer)e.Typ), e.Case, (ж<itab>)(uintptr)((@unsafe.Pointer)e.Itab));
        }
    }
    addEntry(Ꮡtyp, case_, Ꮡtab);
    return newC;
}

// Empty interface switch cache. Contains one entry with a nil Typ (which
// causes a cache lookup to fail immediately.)
internal static abi.InterfaceSwitchCache emptyInterfaceSwitchCache = new abi.InterfaceSwitchCache(Mask: 0);

// reflect_ifaceE2I is for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//   - github.com/modern-go/reflect2
//   - github.com/v2pro/plz
//
// Do not remove or change the type signature.
//
//go:linkname reflect_ifaceE2I reflect.ifaceE2I
internal static void reflect_ifaceE2I(ж<interfacetype> Ꮡinter, eface e, ref iface dst) {
    dst = new iface(assertE2I(Ꮡinter, e._type), e.data);
}

//go:linkname reflectlite_ifaceE2I internal/reflectlite.ifaceE2I
internal static void reflectlite_ifaceE2I(ж<interfacetype> Ꮡinter, eface e, ref iface dst) {
    dst = new iface(assertE2I(Ꮡinter, e._type), e.data);
}

internal static void iterate_itabs(Action<ж<itab>> fn) {
    // Note: only runs during stop the world or with itabLock held,
    // so no other locks/atomics needed.
    var t = itabTable;
    for (var i = (uintptr)0; i < (~t).size; i++) {
        var m = ~(ж<ж<itab>>)(uintptr)(add(@unsafe.Pointer.FromPinnedBox(t.of(itabTableType.Ꮡentries)), i * (uintptr)goarch.PtrSize));
        if (m != nil) {
            fn(m);
        }
    }
}

// staticuint64s is used to avoid allocating in convTx for small integer values.
// staticuint64s[0] == 0, staticuint64s[1] == 1, and so forth.
// It is defined in assembler code so that it is read-only.
internal static ж<array<uint64>> Ꮡstaticuint64s = new StandardBox<array<uint64>>(new array<uint64>(256));
internal static ref array<uint64> staticuint64s => ref Ꮡstaticuint64s.Value;

// getStaticuint64s is called by the reflect package to get a pointer
// to the read-only array.
//
//go:linkname getStaticuint64s
internal static ж<array<uint64>> getStaticuint64s() {
    return Ꮡstaticuint64s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unreachableMethodCalledˢ = "unreachable method called. linker bug?"u8;

// The linker redirects a reference of a method that it determined
// unreachable to a reference to this function, so it will throw if
// ever called.
internal static void unreachableMethod() {
    @throw(unreachableMethodCalledˢ);
}

} // end runtime_package
