// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using CtrlGroup = global::go.@internal.runtime.maps_package.ctrlGroup;

namespace go.@internal.runtime;

using abi = go.@internal.abi_package;
using @unsafe = unsafe_package;
using go.@internal;
using static go.@internal.runtime.maps_package;

partial class maps_internal_test_package {

public const bool DebugLog = /* debugLog */ false;

public static Func<uint64, (uint64, bool)> AlignUpPow2 = alignUpPow2;

public static UntypedInt MaxTableCapacity => /* maxTableCapacity */ 1024;

public static UntypedInt MaxAvgGroupLoad => /* maxAvgGroupLoad */ 7;

// This isn't equivalent to runtime.maxAlloc. It is fine for basic testing but
// we can't properly test hint alloc overflows with this.
internal static UntypedInt maxAllocTest => /* 1 << 30 */ 1073741824;

public static (ж<global::go.@internal.runtime.maps_package.ΔMap>, ж<abi.SwissMapType>) NewTestMap<K, V>(uintptr hint) {
    var mt = newTestMapType<K, V>();
    return (NewMap(mt, hint, nil, maxAllocTest), mt);
}

internal static nint TableCount(this ref global::go.@internal.runtime.maps_package.ΔMap m) {
    if (m.dirLen <= 0) {
        return 0;
    }
    return m.dirLen;
}

// Total group count, summed across all tables.
internal static uint64 GroupCount(this ref global::go.@internal.runtime.maps_package.ΔMap m) {
    if (m.dirLen <= 0) {
        if (m.dirPtr == nil) {
            return 0;
        }
        return 1;
    }
    uint64 n = default!;
    ж<global::go.@internal.runtime.maps_package.table> lastTab = default!;
    foreach (var i in range(m.dirLen)) {
        var t = m.directoryAt((uintptr)i);
        if (t == lastTab) {
            continue;
        }
        lastTab = t;
        n += (~t).groups.lengthMask + 1;
    }
    return n;
}

// Return a key from a group containing no empty slots.
//
// Returns nil if there are no full groups.
// Returns nil if a group is full but contains entirely deleted slots.
// Returns nil if the map is small.
internal static @unsafe.Pointer KeyFromFullGroup(this ref global::go.@internal.runtime.maps_package.ΔMap m, ж<abi.SwissMapType> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (m.dirLen <= 0) {
        return default!;
    }
    ж<global::go.@internal.runtime.maps_package.table> lastTab = default!;
    foreach (var i in range(m.dirLen)) {
        var t = m.directoryAt((uintptr)i);
        if (t == lastTab) {
            continue;
        }
        lastTab = t;
        for (var iΔ1 = (uint64)0; iΔ1 <= (~t).groups.lengthMask; iΔ1++) {
            var g = t.of(global::go.@internal.runtime.maps_package.table.Ꮡgroups).group(Ꮡtyp, iΔ1);
            var match = (~g.ctrls()).matchEmpty();
            if (match != 0) {
                continue;
            }
            // All full or deleted slots.
            for (var j = (uintptr)0; j < abi.SwissMapGroupSlots; j++) {
                if (g.ctrls().get(j) == ctrlDeleted) {
                    continue;
                }
                @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, j);
                if (typ.IndirectKey()) {
                    slotKey = ((ж<@unsafe.Pointer>)(uintptr)(slotKey)).Value;
                }
                return slotKey;
            }
        }
    }
    return default!;
}

// Returns nil if the map is small.
internal static ж<global::go.@internal.runtime.maps_package.table> TableFor(this ref global::go.@internal.runtime.maps_package.ΔMap m, ж<abi.SwissMapType> Ꮡtyp, @unsafe.Pointer key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (m.dirLen <= 0) {
        return default!;
    }
    var hash = typ.Hasher(key, m.seed);
    var idx = m.directoryIndex(hash);
    return m.directoryAt(idx);
}

internal static uint64 GrowthLeft(this ref global::go.@internal.runtime.maps_package.table t) {
    return (uint64)t.growthLeft;
}

// Returns the start address of the groups array.
internal static @unsafe.Pointer GroupsStart(this ref global::go.@internal.runtime.maps_package.table t) {
    return t.groups.data;
}

// Returns the length of the groups array.
internal static uintptr GroupsLength(this ref global::go.@internal.runtime.maps_package.table t) {
    return (uintptr)(t.groups.lengthMask + 1);
}

} // end maps_internal_test_package
