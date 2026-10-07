// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build goexperiment.swissmap
global using mapType = go.@internal.abi_package.SwissMapType;
global using hiter = go.@internal.runtime.maps_package.Iter;

namespace go;

using abi = @internal.abi_package;
using race = @internal.race_package;
using maps = @internal.runtime.maps_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using System.Runtime.CompilerServices;

partial class reflect_package {

// go2cs generated this placeholder — func Key is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func MapOf is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static (ΔType, ΔType) groupAndSlotOf(ΔType ktyp, ΔType etyp) {
    // type group struct {
    //     ctrl uint64
    //     slots [abi.SwissMapGroupSlots]struct {
    //         key  keyType
    //         elem elemType
    //     }
    // }
    if (ktyp.Size() > abi.SwissMapMaxKeyBytes) {
        ktyp = PointerTo(ktyp);
    }
    if (etyp.Size() > abi.SwissMapMaxElemBytes) {
        etyp = PointerTo(etyp);
    }
    var fields = new StructField[]{
        new(
            Name: "Key"u8,
            Type: ktyp
        ),
        new(
            Name: "Elem"u8,
            Type: etyp
        )
    }.slice();
    var slot = StructOf(fields);
    fields = new StructField[]{
        new(
            Name: "Ctrl"u8,
            Type: TypeFor<uint64>()
        ),
        new(
            Name: "Slots"u8,
            Type: ArrayOf(abi.SwissMapGroupSlots, slot)
        )
    }.slice();
    var group = StructOf(fields);
    return (group, slot);
}

internal static ж<abi.Type> stringType = rtypeOf((@string)""u8);

// go2cs generated this placeholder — func MapIndex is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Equivalent to runtime.mapIterStart.
//
//go:noinline
internal static partial void mapIterStart(ж<abi.SwissMapType> Ꮡt, ж<mapsꓸMap> Ꮡm, ж<maps.Iter> Ꮡit) {
    ref var it = ref Ꮡit.DerefOrNull();

    if (race.Enabled && Ꮡm != nil) {
        var callerpc = sys.GetCallerPC();
        race.ReadPC(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, abi.FuncPCABIInternal(mapIterStart));
    }
    it.Init(Ꮡt, Ꮡm);
    it.Next();
}

// Equivalent to runtime.mapIterNext.
//
//go:noinline
internal static partial void mapIterNext(ж<maps.Iter> Ꮡit) {
    ref var it = ref Ꮡit.DerefOrNull();

    if (race.Enabled) {
        var callerpc = sys.GetCallerPC();
        race.ReadPC(@unsafe.Pointer.FromPinnedBox(it.Map()), callerpc, abi.FuncPCABIInternal(mapIterNext));
    }
    it.Next();
}

// go2cs generated this placeholder — func MapKeys is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// A MapIter is an iterator for ranging over a map.
// See [Value.MapRange].
[GoType] partial struct MapIter {
    internal ΔValue m;
    internal maps.Iter hiter;
}

// go2cs generated this placeholder — func Key is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func SetIterKey is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Value is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func SetIterValue is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Next is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Reset is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func MapRange is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func SetMapIndex is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Force slow panicking path not inlined, so it won't add to the
// inlining budget of the caller.
// TODO: undo when the inliner is no longer bottom-up only.
//
//go:noinline
internal static partial void panicNotMap(this flag f) {
    f.mustBe(Map);
}

} // end reflect_package
