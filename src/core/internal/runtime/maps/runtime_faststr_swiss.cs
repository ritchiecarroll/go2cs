// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build goexperiment.swissmap
namespace go.@internal.runtime;

using abi = go.@internal.abi_package;
using goarch = go.@internal.goarch_package;
using race = go.@internal.race_package;
using sys = go.@internal.runtime.sys_package;
using @unsafe = unsafe_package;
using go.@internal;
using go.@internal.runtime;

partial class maps_package {

internal static @unsafe.Pointer getWithoutKeySmallFastStr(this ref ΔMap m, ж<abi.SwissMapType> Ꮡtyp, @string keyʗp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    ref var key = ref heap(keyʗp, out var Ꮡkey);
    var g = new groupReference(
        data: m.dirPtr
    );
    var ctrls = g.ctrls().Value;
    @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, 0);
    var slotSize = typ.SlotSize;
    // The 64 threshold was chosen based on performance of BenchmarkMapStringKeysEight,
    // where there are 8 keys to check, all of which don't quick-match the lookup key.
    // In that case, we can save hashing the lookup key. That savings is worth this extra code
    // for strings that are long enough that hashing is expensive.
    if (len(key) > 64) {
        // String hashing and equality might be expensive. Do a quick check first.
        nint j = abi.SwissMapGroupSlots;
        foreach (var i in range(abi.SwissMapGroupSlots)) {
            if ((ctrlGroup)(ctrls & ((ctrlGroup)((uint64)1 << (int)(7)))) == 0 && longStringQuickEqualityTest(key, ~(ж<@string>)(uintptr)(slotKey))) {
                if (j < abi.SwissMapGroupSlots) {
                    // 2 strings both passed the quick equality test.
                    // Break out of this loop and do it the slow way.
                    goto dohash;
                }
                j = i;
            }
            slotKey = (@unsafe.Pointer)((uintptr)slotKey + slotSize);
            ctrls >>= (int)(8);
        }
        if (j == abi.SwissMapGroupSlots) {
            // No slot passed the quick test.
            return default!;
        }
        // There's exactly one slot that passed the quick test. Do the single expensive comparison.
        slotKey = (uintptr)g.key(Ꮡtyp, (uintptr)j);
        if (key == ~(ж<@string>)(uintptr)(slotKey)) {
            return (@unsafe.Pointer)((uintptr)slotKey + (uintptr)(2 * goarch.PtrSize));
        }
        return default!;
    }
dohash:
    var hash = typ.Hasher((uintptr)abi.NoEscape(@unsafe.Pointer.FromPinnedBox(Ꮡkey)), // This path will cost 1 hash and 1+ε comparisons.
 m.seed);
    System.GC.KeepAlive(Ꮡkey);
    var h2Δ1 = (uint8)h2(hash);
    ctrls = g.ctrls().Value;
    slotKey = (uintptr)g.key(Ꮡtyp, 0);
    foreach (var _ᴛ1 in range(abi.SwissMapGroupSlots)) {
        if ((uint8)(uint64)ctrls == h2Δ1 && key == ~(ж<@string>)(uintptr)(slotKey)) {
            return (@unsafe.Pointer)((uintptr)slotKey + (uintptr)(2 * goarch.PtrSize));
        }
        slotKey = (@unsafe.Pointer)((uintptr)slotKey + slotSize);
        ctrls >>= (int)(8);
    }
    return default!;
}

// Returns true if a and b might be equal.
// Returns false if a and b are definitely not equal.
// Requires len(a)>=8.
internal static bool longStringQuickEqualityTest(@string a, @string b) {
    if (len(a) != len(b)) {
        return false;
    }
    @unsafe.Pointer x = (uintptr)stringPtr(a);
    @unsafe.Pointer y = (uintptr)stringPtr(b);
    // Check first 8 bytes.
    if (~(ж<array<byte>>)(uintptr)(x) != ~(ж<array<byte>>)(uintptr)(y)) {
        return false;
    }
    // Check last 8 bytes.
    x = (@unsafe.Pointer)((uintptr)x + (uintptr)len(a) - 8);
    y = (@unsafe.Pointer)((uintptr)y + (uintptr)len(a) - 8);
    if (~(ж<array<byte>>)(uintptr)(x) != ~(ж<array<byte>>)(uintptr)(y)) {
        return false;
    }
    return true;
}

internal partial struct stringPtr_stringStruct /*dyn*/ {
    internal @unsafe.Pointer ptr;
    internal nint len;
}

internal static @unsafe.Pointer stringPtr(@string sʗp) {
    ref var s = ref heap(sʗp, out var Ꮡs);

    return (Ꮡs.Reinterpret<@string, stringPtr_stringStruct>()).Value.ptr;
}

//go:linkname runtime_mapaccess1_faststr runtime.mapaccess1_faststr
internal static @unsafe.Pointer runtime_mapaccess1_faststr(ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm, @string key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    if (race.Enabled && Ꮡm != nil) {
        var callerpc = sys.GetCallerPC();
        var pc = abi.FuncPCABIInternal(runtime_mapaccess1_faststr);
        race.ReadPC(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, pc);
    }
    if (Ꮡm == nil || m.Used() == 0) {
        return @unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0));
    }
    if (m.writing != 0) {
        fatal(concurrentMapReadAndMapˢ);
        return default!;
    }
    if (m.dirLen <= 0) {
        @unsafe.Pointer elem = (uintptr)m.getWithoutKeySmallFastStr(Ꮡtyp, key);
        if (elem == nil) {
            return @unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0));
        }
        return elem;
    }
    ref var k = ref heap<@string>(out var Ꮡk);
    k = key;
    var hash = typ.Hasher((uintptr)abi.NoEscape(@unsafe.Pointer.FromPinnedBox(Ꮡk)), m.seed);
    System.GC.KeepAlive(Ꮡk);
    // Select table.
    var idx = m.directoryIndex(hash);
    var t = m.directoryAt(idx);
    // Probe table.
    var seq = makeProbeSeq(h1(hash), (~t).groups.lengthMask);
    for (; ᐧ ; seq = seq.next()) {
        var g = t.of(table.Ꮡgroups).group(Ꮡtyp, seq.offset);
        var match = (~g.ctrls()).matchH2(h2(hash));
        while (match != 0) {
            var i = match.first();
            @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, i);
            if (key == ~(ж<@string>)(uintptr)(slotKey)) {
                @unsafe.Pointer slotElem = (@unsafe.Pointer)((uintptr)slotKey + (uintptr)(2 * goarch.PtrSize));
                return slotElem;
            }
            match = match.removeFirst();
        }
        match = (~g.ctrls()).matchEmpty();
        if (match != 0) {
            // Finding an empty slot means we've reached the end of
            // the probe sequence.
            return @unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0));
        }
    }
}

//go:linkname runtime_mapaccess2_faststr runtime.mapaccess2_faststr
internal static (@unsafe.Pointer, bool) runtime_mapaccess2_faststr(ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm, @string key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    if (race.Enabled && Ꮡm != nil) {
        var callerpc = sys.GetCallerPC();
        var pc = abi.FuncPCABIInternal(runtime_mapaccess2_faststr);
        race.ReadPC(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, pc);
    }
    if (Ꮡm == nil || m.Used() == 0) {
        return (@unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0)), false);
    }
    if (m.writing != 0) {
        fatal(concurrentMapReadAndMapˢ);
        return (default!, false);
    }
    if (m.dirLen <= 0) {
        @unsafe.Pointer elem = (uintptr)m.getWithoutKeySmallFastStr(Ꮡtyp, key);
        if (elem == nil) {
            return (@unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0)), false);
        }
        return (elem, true);
    }
    ref var k = ref heap<@string>(out var Ꮡk);
    k = key;
    var hash = typ.Hasher((uintptr)abi.NoEscape(@unsafe.Pointer.FromPinnedBox(Ꮡk)), m.seed);
    System.GC.KeepAlive(Ꮡk);
    // Select table.
    var idx = m.directoryIndex(hash);
    var t = m.directoryAt(idx);
    // Probe table.
    var seq = makeProbeSeq(h1(hash), (~t).groups.lengthMask);
    for (; ᐧ ; seq = seq.next()) {
        var g = t.of(table.Ꮡgroups).group(Ꮡtyp, seq.offset);
        var match = (~g.ctrls()).matchH2(h2(hash));
        while (match != 0) {
            var i = match.first();
            @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, i);
            if (key == ~(ж<@string>)(uintptr)(slotKey)) {
                @unsafe.Pointer slotElem = (@unsafe.Pointer)((uintptr)slotKey + (uintptr)(2 * goarch.PtrSize));
                return (slotElem, true);
            }
            match = match.removeFirst();
        }
        match = (~g.ctrls()).matchEmpty();
        if (match != 0) {
            // Finding an empty slot means we've reached the end of
            // the probe sequence.
            return (@unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0)), false);
        }
    }
}

internal static @unsafe.Pointer putSlotSmallFastStr(this ref ΔMap m, ж<abi.SwissMapType> Ꮡtyp, uintptr hash, @string key) {
    var g = new groupReference(
        data: m.dirPtr
    );
    var match = (~g.ctrls()).matchH2(h2(hash));
    // Look for an existing slot containing this key.
    while (match != 0) {
        var iΔ1 = match.first();
        @unsafe.Pointer slotKeyΔ1 = (uintptr)g.key(Ꮡtyp, iΔ1);
        if (key == ~(ж<@string>)(uintptr)(slotKeyΔ1)) {
            // Key needs update, as the backing storage may differ.
            ((ж<@string>)(uintptr)(slotKeyΔ1)).Value = key;
            @unsafe.Pointer slotElemΔ1 = (uintptr)g.elem(Ꮡtyp, iΔ1);
            return slotElemΔ1;
        }
        match = match.removeFirst();
    }
    // There can't be deleted slots, small maps can't have them
    // (see deleteSmall). Use matchEmptyOrDeleted as it is a bit
    // more efficient than matchEmpty.
    match = (~g.ctrls()).matchEmptyOrDeleted();
    if (match == 0) {
        fatal(smallMapWithNoEmptySlotˢ);
    }
    var i = match.first();
    @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, i);
    ((ж<@string>)(uintptr)(slotKey)).Value = key;
    @unsafe.Pointer slotElem = (uintptr)g.elem(Ꮡtyp, i);
    g.ctrls().set(i, ((ctrl)(uint8)h2(hash)));
    m.used++;
    return slotElem;
}

//go:linkname runtime_mapassign_faststr runtime.mapassign_faststr
internal static @unsafe.Pointer runtime_mapassign_faststr(ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm, @string key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    if (Ꮡm == nil) {
        throw panic(errNilAssign);
    }
    if (race.Enabled) {
        var callerpc = sys.GetCallerPC();
        var pc = abi.FuncPCABIInternal(runtime_mapassign_faststr);
        race.WritePC(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, pc);
    }
    if (m.writing != 0) {
        fatal(concurrentMapWritesˢ);
    }
    ref var k = ref heap<@string>(out var Ꮡk);
    k = key;
    var hash = typ.Hasher((uintptr)abi.NoEscape(@unsafe.Pointer.FromPinnedBox(Ꮡk)), m.seed);
    System.GC.KeepAlive(Ꮡk);
    // Set writing after calling Hasher, since Hasher may panic, in which
    // case we have not actually done a write.
    m.writing ^= (uint8)(1); // toggle, see comment on writing
    if (m.dirPtr == nil) {
        m.growToSmall(Ꮡtyp);
    }
    if (m.dirLen == 0) {
        if (m.used < abi.SwissMapGroupSlots) {
            @unsafe.Pointer elem = (uintptr)m.putSlotSmallFastStr(Ꮡtyp, hash, key);
            if (m.writing == 0) {
                fatal(concurrentMapWritesˢ);
            }
            m.writing ^= (uint8)(1);
            return elem;
        }
        // Can't fit another entry, grow to full size map.
        m.growToTable(Ꮡtyp);
    }
    @unsafe.Pointer slotElem = default!;
outer:
    while (ᐧ) {
        // Select table.
        var idx = m.directoryIndex(hash);
        var t = m.directoryAt(idx);
        var seq = makeProbeSeq(h1(hash), (~t).groups.lengthMask);
        // As we look for a match, keep track of the first deleted slot
        // we find, which we'll use to insert the new entry if
        // necessary.
        groupReference firstDeletedGroup = default!;
        uintptr firstDeletedSlot = default!;
        for (; ᐧ ; seq = seq.next()) {
            var g = t.of(table.Ꮡgroups).group(Ꮡtyp, seq.offset);
            var match = (~g.ctrls()).matchH2(h2(hash));
            // Look for an existing slot containing this key.
            while (match != 0) {
                var iΔ1 = match.first();
                @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, iΔ1);
                if (key == ~(ж<@string>)(uintptr)(slotKey)) {
                    // Key needs update, as the backing
                    // storage may differ.
                    ((ж<@string>)(uintptr)(slotKey)).Value = key;
                    slotElem = (uintptr)g.elem(Ꮡtyp, iΔ1);
                    t.checkInvariants(Ꮡtyp, Ꮡm);
                    goto break_outer;
                }
                match = match.removeFirst();
            }
            // No existing slot for this key in this group. Is this the end
            // of the probe sequence?
            match = (~g.ctrls()).matchEmptyOrDeleted();
            if (match == 0) {
                continue; // nothing but filled slots. Keep probing.
            }
            var i = match.first();
            if (g.ctrls().get(i) == ctrlDeleted) {
                // There are some deleted slots. Remember
                // the first one, and keep probing.
                if (firstDeletedGroup.data == nil) {
                    firstDeletedGroup = g;
                    firstDeletedSlot = i;
                }
                continue;
            }
            // We've found an empty slot, which means we've reached the end of
            // the probe sequence.
            // If we found a deleted slot along the way, we can
            // replace it without consuming growthLeft.
            if (firstDeletedGroup.data != nil) {
                g = firstDeletedGroup;
                i = firstDeletedSlot;
                t.Value.growthLeft++; // will be decremented below to become a no-op.
            }
            // If there is room left to grow, just insert the new entry.
            if ((~t).growthLeft > 0) {
                @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, i);
                ((ж<@string>)(uintptr)(slotKey)).Value = key;
                slotElem = (uintptr)g.elem(Ꮡtyp, i);
                g.ctrls().set(i, ((ctrl)(uint8)h2(hash)));
                t.Value.growthLeft--;
                t.Value.used++;
                m.used++;
                t.checkInvariants(Ꮡtyp, Ꮡm);
                goto break_outer;
            }
            t.rehash(Ꮡtyp, Ꮡm);
            goto continue_outer;
        }
continue_outer:;
    }
break_outer:;
    if (m.writing == 0) {
        fatal(concurrentMapWritesˢ);
    }
    m.writing ^= (uint8)(1);
    return slotElem;
}

//go:linkname runtime_mapdelete_faststr runtime.mapdelete_faststr
internal static void runtime_mapdelete_faststr(ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm, @string keyʗp) {
    ref var m = ref Ꮡm.DerefOrNull();

    ref var key = ref heap(keyʗp, out var Ꮡkey);
    if (race.Enabled) {
        var callerpc = sys.GetCallerPC();
        var pc = abi.FuncPCABIInternal(runtime_mapdelete_faststr);
        race.WritePC(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, pc);
    }
    if (Ꮡm == nil || m.Used() == 0) {
        return;
    }
    Ꮡm.Delete(Ꮡtyp, (uintptr)abi.NoEscape(@unsafe.Pointer.FromPinnedBox(Ꮡkey)));
    System.GC.KeepAlive(Ꮡkey);
}

} // end maps_package
