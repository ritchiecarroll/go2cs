// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package maps implements Go's builtin map type.
namespace go.@internal.runtime;

using abi = go.@internal.abi_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class maps_package {

internal const bool debugLog = false;

[GoRecv] internal static void checkInvariants(this ref table t, ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    if (!debugLog) {
        return;
    }
    // For every non-empty slot, verify we can retrieve the key using Get.
    // Count the number of used and deleted slots.
    uint16 used = default!;
    uint16 deleted = default!;
    uint16 empty = default!;
    for (var i = (uint64)0; i <= t.groups.lengthMask; i++) {
        var g = t.groups.group(Ꮡtyp, i);
        for (var j = (uintptr)0; j < abi.SwissMapGroupSlots; j++) {
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
                @unsafe.Pointer key = (uintptr)g.key(Ꮡtyp, j);
                if (typ.IndirectKey()) {
                    key = ((ж<@unsafe.Pointer>)(uintptr)(key)).Value;
                }
                if (!(~typ.Key).Equal(key, // Can't lookup keys that don't compare equal
 // to themselves (e.g., NaN).
 key)) {
                    continue;
                }
                {
                    var (_, ok) = t.Get(Ꮡtyp, Ꮡm, key); if (!ok) {
                        var hash = typ.Hasher(key, m.seed);
                        print((@string)"invariant failed: slot("u8, i, (@string)"/"u8, j, (@string)"): key "u8);
                        dump(key, (~typ.Key).Size_);
                        print((@string)" not found [hash="u8, hash, (@string)", h2="u8, h2(hash), (@string)" h1="u8, h1(hash), (@string)"]\n"u8);
                        t.Print(Ꮡtyp, Ꮡm);
                        throw panic("invariant failed: slot: key not found");
                    }
                }
                break;
            }}

        }
    }
    if (used != t.used) {
        print((@string)"invariant failed: found "u8, used, (@string)" used slots, but used count is "u8, t.used, (@string)"\n"u8);
        t.Print(Ꮡtyp, Ꮡm);
        throw panic("invariant failed: found mismatched used slot count");
    }
    var growthLeft = (uint16)((uint16)(t.capacity * (uint16)maxAvgGroupLoad) / (uint16)abi.SwissMapGroupSlots - t.used - deleted);
    if (growthLeft != t.growthLeft) {
        print((@string)"invariant failed: found "u8, t.growthLeft, (@string)" growthLeft, but expected "u8, growthLeft, (@string)"\n"u8);
        t.Print(Ꮡtyp, Ꮡm);
        throw panic("invariant failed: found mismatched growthLeft");
    }
    if (deleted != t.tombstones()) {
        print((@string)"invariant failed: found "u8, deleted, (@string)" tombstones, but expected "u8, t.tombstones(), (@string)"\n"u8);
        t.Print(Ꮡtyp, Ꮡm);
        throw panic("invariant failed: found mismatched tombstones");
    }
    if (empty == 0) {
        print((@string)"invariant failed: found no empty slots (violates probe invariant)\n"u8);
        t.Print(Ꮡtyp, Ꮡm);
        throw panic("invariant failed: found no empty slots (violates probe invariant)");
    }
}

[GoRecv] public static void Print(this ref table t, ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    print((@string)"""
table{
	index: 
"""u8, t.index, (@string)"""

	localDepth: 
"""u8, t.localDepth, (@string)"""

	capacity: 
"""u8, t.capacity, (@string)"""

	used: 
"""u8, t.used, (@string)"""

	growthLeft: 
"""u8, t.growthLeft, (@string)"""

	groups:

"""u8);
    for (var i = (uint64)0; i <= t.groups.lengthMask; i++) {
        print((@string)"\t\tgroup "u8, i, (@string)"\n"u8);
        var g = t.groups.group(Ꮡtyp, i);
        var ctrls = g.ctrls();
        for (var j = (uintptr)0; j < abi.SwissMapGroupSlots; j++) {
            print((@string)"\t\t\tslot "u8, j, (@string)"\n"u8);
            var c = ctrls.get(j);
            print((@string)"\t\t\t\tctrl "u8, c);
            var exprᴛ1 = c;
            if (exprᴛ1 == ctrlEmpty) {
                print((@string)" (empty)\n"u8);
            }
            else if (exprᴛ1 == ctrlDeleted) {
                print((@string)" (deleted)\n"u8);
            }
            else { /* default: */
                print((@string)"\n"u8);
            }

            print((@string)"\t\t\t\tkey  "u8);
            dump((uintptr)g.key(Ꮡtyp, j), (~typ.Key).Size_);
            println((@string)""u8);
            print((@string)"\t\t\t\telem "u8);
            dump((uintptr)g.elem(Ꮡtyp, j), (~typ.Elem).Size_);
            println((@string)""u8);
        }
    }
}

// TODO(prattmic): not in hex because print doesn't have a way to print in hex
// outside the runtime.
internal static void dump(@unsafe.Pointer ptr, uintptr size) {
    while (size > 0) {
        print(~(ж<byte>)(uintptr)(ptr), (@string)" "u8);
        ptr = (@unsafe.Pointer)((uintptr)ptr + 1);
        size--;
    }
}

} // end maps_package
