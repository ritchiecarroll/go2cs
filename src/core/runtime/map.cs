// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using maps = @internal.runtime.maps_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// TODO: remove? These are used by tests but not the actual map
internal static UntypedInt loadFactorNum => 7;
internal static UntypedInt loadFactorDen => 8;

//go:linkname maps_errNilAssign internal/runtime/maps.errNilAssign
internal static error maps_errNilAssign = ((plainError)(@string)"assignment to entry in nil map"u8);

internal static ж<mapsꓸMap> makemap64(ж<abiꓸMapType> Ꮡt, int64 hint, ж<mapsꓸMap> Ꮡm) {
    if ((int64)(nint)hint != hint) {
        hint = 0;
    }
    return makemap(Ꮡt, (nint)hint, Ꮡm);
}

// makemap_small implements Go map creation for make(map[k]v) and
// make(map[k]v, hint) when hint is known to be at most abi.MapGroupSlots
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
internal static ж<mapsꓸMap> makemap_small() {
    return maps.NewEmptyMap();
}

// makemap implements Go map creation for make(map[k]v, hint).
// If the compiler has determined that the map or the first group
// can be created on the stack, m and optionally m.dirPtr may be non-nil.
// If m != nil, the map can be created directly in m.
// If m.dirPtr != nil, it points to a group usable for a small map.
//
// makemap should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname makemap
internal static ж<mapsꓸMap> makemap(ж<abiꓸMapType> Ꮡt, nint hint, ж<mapsꓸMap> Ꮡm) {
    if (hint < 0) {
        hint = 0;
    }

    return maps.NewMap(Ꮡt, (uintptr)hint, Ꮡm, maxAlloc);
}

// mapaccess1 returns a pointer to h[key].  Never returns nil, instead
// it will return a reference to the zero object for the elem type if
// the key is not in the map.
// NOTE: The returned pointer may keep the whole map live, so don't
// hold onto it for very long.
//
// mapaccess1 is pushed from internal/runtime/maps. We could just call it, but
// we want to avoid one layer of call.
//
//go:linkname mapaccess1
internal static partial @unsafe.Pointer mapaccess1(ж<abiꓸMapType> t, ж<mapsꓸMap> m, @unsafe.Pointer key);

// mapaccess2 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapaccess2
internal static partial (@unsafe.Pointer, bool) mapaccess2(ж<abiꓸMapType> t, ж<mapsꓸMap> m, @unsafe.Pointer key);

internal static @unsafe.Pointer mapaccess1_fat(ж<abiꓸMapType> Ꮡt, ж<mapsꓸMap> Ꮡm, @unsafe.Pointer key, @unsafe.Pointer zero) {
    @unsafe.Pointer e = (uintptr)mapaccess1(Ꮡt, Ꮡm, key);
    if (e == @unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0))) {
        return zero;
    }
    return e;
}

internal static (@unsafe.Pointer, bool) mapaccess2_fat(ж<abiꓸMapType> Ꮡt, ж<mapsꓸMap> Ꮡm, @unsafe.Pointer key, @unsafe.Pointer zero) {
    @unsafe.Pointer e = (uintptr)mapaccess1(Ꮡt, Ꮡm, key);
    if (e == @unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0))) {
        return (zero, false);
    }
    return (e, true);
}

// mapassign is pushed from internal/runtime/maps. We could just call it, but
// we want to avoid one layer of call.
//
// mapassign should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//   - github.com/RomiChan/protobuf
//   - github.com/segmentio/encoding
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapassign
internal static partial @unsafe.Pointer mapassign(ж<abiꓸMapType> t, ж<mapsꓸMap> m, @unsafe.Pointer key);

// mapdelete should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapdelete
internal static void mapdelete(ж<abiꓸMapType> Ꮡt, ж<mapsꓸMap> Ꮡm, @unsafe.Pointer key) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (raceenabled && Ꮡm != nil) {
        var callerpc = sys.GetCallerPC();
        var pc = abi.FuncPCABIInternal(mapdelete);
        racewritepc(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, pc);
        raceReadObjectPC(ref (t.Key).DerefOrNull(), key, callerpc, pc);
    }
    if (msanenabled && Ꮡm != nil) {
        msanread(key, (~t.Key).Size_);
    }
    if (asanenabled && Ꮡm != nil) {
        asanread(key, (~t.Key).Size_);
    }

    Ꮡm.Delete(Ꮡt, key);
}

// mapIterStart initializes the Iter struct used for ranging over maps and
// performs the first step of iteration. The Iter struct pointed to by 'it' is
// allocated on the stack by the compilers order pass or on the heap by
// reflect. Both need to have zeroed it since the struct contains pointers.
internal static void mapIterStart(ж<abiꓸMapType> Ꮡt, ж<mapsꓸMap> Ꮡm, ж<maps.Iter> Ꮡit) {
    ref var it = ref Ꮡit.DerefOrNull();

    if (raceenabled && Ꮡm != nil) {
        var callerpc = sys.GetCallerPC();
        racereadpc(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, abi.FuncPCABIInternal(mapIterStart));
    }

    it.Init(Ꮡt, Ꮡm);
    it.Next();
}

// mapIterNext performs the next step of iteration. Afterwards, the next
// key/elem are in it.Key()/it.Elem().
internal static void mapIterNext(ж<maps.Iter> Ꮡit) {
    ref var it = ref Ꮡit.DerefOrNull();

    if (raceenabled) {
        var callerpc = sys.GetCallerPC();
        racereadpc(@unsafe.Pointer.FromPinnedBox(it.Map()), callerpc, abi.FuncPCABIInternal(mapIterNext));
    }

    it.Next();
}

// mapclear deletes all keys from a map.
internal static void mapclear(ж<abiꓸMapType> Ꮡt, ж<mapsꓸMap> Ꮡm) {
    if (raceenabled && Ꮡm != nil) {
        var callerpc = sys.GetCallerPC();
        var pc = abi.FuncPCABIInternal(mapclear);
        racewritepc(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, pc);
    }

    Ꮡm.Clear(Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeReflectMakemapˢ = "runtime.reflect_makemap: unsupported map key type"u8;

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
internal static ж<mapsꓸMap> reflect_makemap(ж<abiꓸMapType> Ꮡt, nint cap) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Check invariants and reflects math.
    if ((~t.Key).Equal == default!) {
        @throw(runtimeReflectMakemapˢ);
    }
    // TODO: other checks

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
internal static @unsafe.Pointer reflect_mapaccess(ж<abiꓸMapType> Ꮡt, ж<mapsꓸMap> Ꮡm, @unsafe.Pointer key) {
    var (elem, ok) = mapaccess2(Ꮡt, Ꮡm, key);
    if (!ok) {
        // reflect wants nil for a missing element
        elem = default!;
    }
    return elem;
}

//go:linkname reflect_mapaccess_faststr reflect.mapaccess_faststr
internal static @unsafe.Pointer reflect_mapaccess_faststr(ж<abiꓸMapType> Ꮡt, ж<mapsꓸMap> Ꮡm, @string key) {
    var (elem, ok) = mapaccess2_faststr(Ꮡt, Ꮡm, key);
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
internal static void reflect_mapassign(ж<abiꓸMapType> Ꮡt, ж<mapsꓸMap> Ꮡm, @unsafe.Pointer key, @unsafe.Pointer elem) {
    ref var t = ref Ꮡt.DerefOrNull();

    @unsafe.Pointer Δp = (uintptr)mapassign(Ꮡt, Ꮡm, key);
    typedmemmove(t.Elem, Δp, elem);
}

//go:linkname reflect_mapassign_faststr reflect.mapassign_faststr0
internal static void reflect_mapassign_faststr(ж<abiꓸMapType> Ꮡt, ж<mapsꓸMap> Ꮡm, @string key, @unsafe.Pointer elem) {
    ref var t = ref Ꮡt.DerefOrNull();

    @unsafe.Pointer Δp = (uintptr)mapassign_faststr(Ꮡt, Ꮡm, key);
    typedmemmove(t.Elem, Δp, elem);
}

//go:linkname reflect_mapdelete reflect.mapdelete
internal static void reflect_mapdelete(ж<abiꓸMapType> Ꮡt, ж<mapsꓸMap> Ꮡm, @unsafe.Pointer key) {
    mapdelete(Ꮡt, Ꮡm, key);
}

//go:linkname reflect_mapdelete_faststr reflect.mapdelete_faststr
internal static void reflect_mapdelete_faststr(ж<abiꓸMapType> Ꮡt, ж<mapsꓸMap> Ꮡm, @string key) {
    mapdelete_faststr(Ꮡt, Ꮡm, key);
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
internal static nint reflect_maplen(ж<mapsꓸMap> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (Ꮡm == nil) {
        return 0;
    }
    if (raceenabled) {
        var callerpc = sys.GetCallerPC();
        racereadpc(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, abi.FuncPCABIInternal(reflect_maplen));
    }
    return (nint)m.Used();
}

//go:linkname reflect_mapclear reflect.mapclear
internal static void reflect_mapclear(ж<abiꓸMapType> Ꮡt, ж<mapsꓸMap> Ꮡm) {
    mapclear(Ꮡt, Ꮡm);
}

//go:linkname reflectlite_maplen internal/reflectlite.maplen
internal static nint reflectlite_maplen(ж<mapsꓸMap> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (Ꮡm == nil) {
        return 0;
    }
    if (raceenabled) {
        var callerpc = sys.GetCallerPC();
        racereadpc(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, abi.FuncPCABIInternal(reflect_maplen));
    }
    return (nint)m.Used();
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
    var typ = (~e)._type.Reinterpret<_type, abiꓸMapType>();
    var map_ = (ж<mapsꓸMap>)(uintptr)((~e).data);
    map_ = map_.Clone(typ);
    e.Value.data = @unsafe.Pointer.FromPinnedBox(map_);
    return m;
}

} // end runtime_package
