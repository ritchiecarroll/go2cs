// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using abi = go.@internal.abi_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class maps_package {

internal const bool debugLog = false;

internal static void checkInvariants(this ref table t, ж<abiꓸMapType> Ꮡtyp, ж<ΔMap> Ꮡm) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    if (!debugLog) {
        return;
    }

    // For every non-empty slot, verify we can retrieve the key using Get.
    // Count the number of used and deleted slots.
    uint16 used = default;
    uint16 deleted = default;
    uint16 empty = default;
    for (var i = (uint64)0; i <= t.groups.lengthMask; i++) {
        var g = t.groups.group(Ꮡtyp, i);
        for (var j = (uintptr)0; j < abi.MapGroupSlots; j++) {
            var c = g.ctrls().get(j);
            switch (ᐧ) {
            case {} when c == ctrlDeleted: {
                deleted++;
                break;
            }
            case {} when c == ctrlEmpty: {
                empty++;
                break;
            }
            default: {
                used++;

                @unsafe.Pointer key = g.key(Ꮡtyp, j);
                if (typ.IndirectKey()) {
                    key = ((ж<@unsafe.Pointer>)(uintptr)(key)).Value;
                }

                // Can't lookup keys that don't compare equal
                // to themselves (e.g., NaN).
                if (!typ.Key.Equal(key, key)) {
                    continue;
                }

                var (_, ok) = t.Get(Ꮡtyp, Ꮡm, key); if (!ok) {
                    var hash = typ.Hasher(key, m.seed);
                    print((@string)"invariant failed: slot(", i, (@string)"/", j, (@string)"): key ");
                    dump(key, typ.Key.Size_);
                    print((@string)" not found [hash=", hash, (@string)", h2=", h2(hash), (@string)" h1=", h1(hash), (@string)"]\n");
                    t.Print(Ꮡtyp, Ꮡm);
                    throw panic("invariant failed: slot: key not found");
                }
                break;
            }}
        }
    }

    if (used != t.used) {
        print((@string)"invariant failed: found ", used, (@string)" used slots, but used count is ", t.used, (@string)"\n");
        t.Print(Ꮡtyp, Ꮡm);
        throw panic("invariant failed: found mismatched used slot count");
    }

    var growthLeft = (uint16)((uint16)(t.capacity * (uint16)maxAvgGroupLoad) / (uint16)abi.MapGroupSlots - t.used - deleted);
    if (growthLeft != t.growthLeft) {
        print((@string)"invariant failed: found ", t.growthLeft, (@string)" growthLeft, but expected ", growthLeft, (@string)"\n");
        t.Print(Ꮡtyp, Ꮡm);
        throw panic("invariant failed: found mismatched growthLeft");
    }
    if (deleted != t.tombstones()) {
        print((@string)"invariant failed: found ", deleted, (@string)" tombstones, but expected ", t.tombstones(), (@string)"\n");
        t.Print(Ꮡtyp, Ꮡm);
        throw panic("invariant failed: found mismatched tombstones");
    }

    if (empty == 0) {
        print((@string)"invariant failed: found no empty slots (violates probe invariant)\n");
        t.Print(Ꮡtyp, Ꮡm);
        throw panic("invariant failed: found no empty slots (violates probe invariant)");
    }
}
public static void Print(this ref table t, ж<abiꓸMapType> Ꮡtyp, ж<ΔMap> m) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    print((@string)"""
table{
	index: 
"""u8.Raw(), t.index, (@string)"""

	localDepth: 
"""u8.Raw(), t.localDepth, (@string)"""

	capacity: 
"""u8.Raw(), t.capacity, (@string)"""

	used: 
"""u8.Raw(), t.used, (@string)"""

	growthLeft: 
"""u8.Raw(), t.growthLeft, (@string)"""

	groups:

"""u8.Raw());

    for (var i = (uint64)0; i <= t.groups.lengthMask; i++) {
        print((@string)"\t\tgroup ", i, (@string)"\n");

        var g = t.groups.group(Ꮡtyp, i);
        var ctrls = g.ctrls();
        for (var j = (uintptr)0; j < abi.MapGroupSlots; j++) {
            print((@string)"\t\t\tslot ", j, (@string)"\n");

            var c = ctrls.get(j);
            print((@string)"\t\t\t\tctrl ", c);
            switch (c) {
            case var ᴛ when ᴛ == ctrlEmpty: {
                print((@string)" (empty)\n");
                break;
            }
            case var ᴛ when ᴛ == ctrlDeleted: {
                print((@string)" (deleted)\n");
                break;
            }
            default: {
                print((@string)"\n");
                break;
            }}

            print((@string)"\t\t\t\tkey  ");
            dump(g.key(Ꮡtyp, j), typ.Key.Size_);
            println((@string)""u8);
            print((@string)"\t\t\t\telem ");
            dump(g.elem(Ꮡtyp, j), typ.Elem.Size_);
            println((@string)""u8);
        }
    }
}

// TODO(prattmic): not in hex because print doesn't have a way to print in hex
// outside the runtime.
internal static void dump(@unsafe.Pointer ptr, uintptr size) {
    while (size > 0) {
        print(~(ж<byte>)(uintptr)(ptr), (@string)" ");
        ptr = (@unsafe.Pointer)((uintptr)ptr + 1);
        size--;
    }
}

} // end maps_package
