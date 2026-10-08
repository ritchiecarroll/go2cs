// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build goexperiment.swissmap
namespace go.@internal.runtime;

using abi = go.@internal.abi_package;
using race = go.@internal.race_package;
using sys = go.@internal.runtime.sys_package;
using @unsafe = unsafe_package;
using go.@internal;
using go.@internal.runtime;

partial class maps_package {

//go:linkname runtime_mapaccess1_fast32 runtime.mapaccess1_fast32
internal static @unsafe.Pointer runtime_mapaccess1_fast32(ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm, uint32 key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    if (race.Enabled && Ꮡm != nil) {
        var callerpc = sys.GetCallerPC();
        var pc = abi.FuncPCABIInternal(runtime_mapaccess1_fast32);
        race.ReadPC(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, pc);
    }
    if (Ꮡm == nil || m.Used() == 0) {
        return @unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0));
    }
    if (m.writing != 0) {
        fatal(concurrentMapReadAndMapˢ);
        return default!;
    }
    if (m.dirLen == 0) {
        var g = new groupReference(
            data: m.dirPtr
        );
        var full = (~g.ctrls()).matchFull();
        @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, 0);
        var slotSize = typ.SlotSize;
        while (full != 0) {
            if (key == ~(ж<uint32>)(uintptr)(slotKey) && full.lowestSet()) {
                @unsafe.Pointer slotElem = (@unsafe.Pointer)((uintptr)slotKey + typ.ElemOff);
                return slotElem;
            }
            slotKey = (@unsafe.Pointer)((uintptr)slotKey + slotSize);
            full = full.shiftOutLowest();
        }
        return @unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0));
    }
    ref var k = ref heap<uint32>(out var Ꮡk);
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
            if (key == ~(ж<uint32>)(uintptr)(slotKey)) {
                @unsafe.Pointer slotElem = (@unsafe.Pointer)((uintptr)slotKey + typ.ElemOff);
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

//go:linkname runtime_mapaccess2_fast32 runtime.mapaccess2_fast32
internal static (@unsafe.Pointer, bool) runtime_mapaccess2_fast32(ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm, uint32 key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    if (race.Enabled && Ꮡm != nil) {
        var callerpc = sys.GetCallerPC();
        var pc = abi.FuncPCABIInternal(runtime_mapaccess2_fast32);
        race.ReadPC(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, pc);
    }
    if (Ꮡm == nil || m.Used() == 0) {
        return (@unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0)), false);
    }
    if (m.writing != 0) {
        fatal(concurrentMapReadAndMapˢ);
        return (default!, false);
    }
    if (m.dirLen == 0) {
        var g = new groupReference(
            data: m.dirPtr
        );
        var full = (~g.ctrls()).matchFull();
        @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, 0);
        var slotSize = typ.SlotSize;
        while (full != 0) {
            if (key == ~(ж<uint32>)(uintptr)(slotKey) && full.lowestSet()) {
                @unsafe.Pointer slotElem = (@unsafe.Pointer)((uintptr)slotKey + typ.ElemOff);
                return (slotElem, true);
            }
            slotKey = (@unsafe.Pointer)((uintptr)slotKey + slotSize);
            full = full.shiftOutLowest();
        }
        return (@unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0)), false);
    }
    ref var k = ref heap<uint32>(out var Ꮡk);
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
            if (key == ~(ж<uint32>)(uintptr)(slotKey)) {
                @unsafe.Pointer slotElem = (@unsafe.Pointer)((uintptr)slotKey + typ.ElemOff);
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

internal static @unsafe.Pointer putSlotSmallFast32(this ref ΔMap m, ж<abi.SwissMapType> Ꮡtyp, uintptr hash, uint32 key) {
    var g = new groupReference(
        data: m.dirPtr
    );
    var match = (~g.ctrls()).matchH2(h2(hash));
    // Look for an existing slot containing this key.
    while (match != 0) {
        var iΔ1 = match.first();
        @unsafe.Pointer slotKeyΔ1 = (uintptr)g.key(Ꮡtyp, iΔ1);
        if (key == ~(ж<uint32>)(uintptr)(slotKeyΔ1)) {
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
    ((ж<uint32>)(uintptr)(slotKey)).Value = key;
    @unsafe.Pointer slotElem = (uintptr)g.elem(Ꮡtyp, i);
    g.ctrls().set(i, ((ctrl)(uint8)h2(hash)));
    m.used++;
    return slotElem;
}

//go:linkname runtime_mapassign_fast32 runtime.mapassign_fast32
internal static @unsafe.Pointer runtime_mapassign_fast32(ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm, uint32 key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    if (Ꮡm == nil) {
        throw panic(errNilAssign);
    }
    if (race.Enabled) {
        var callerpc = sys.GetCallerPC();
        var pc = abi.FuncPCABIInternal(runtime_mapassign_fast32);
        race.WritePC(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, pc);
    }
    if (m.writing != 0) {
        fatal(concurrentMapWritesˢ);
    }
    ref var k = ref heap<uint32>(out var Ꮡk);
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
            @unsafe.Pointer elem = (uintptr)m.putSlotSmallFast32(Ꮡtyp, hash, key);
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
                if (key == ~(ж<uint32>)(uintptr)(slotKey)) {
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
                ((ж<uint32>)(uintptr)(slotKey)).Value = key;
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

// Key is a 32-bit pointer (only called on 32-bit GOARCH). This source is identical to fast64ptr.
//
// TODO(prattmic): With some compiler refactoring we could avoid duplication of this function.
//
//go:linkname runtime_mapassign_fast32ptr runtime.mapassign_fast32ptr
internal static @unsafe.Pointer runtime_mapassign_fast32ptr(ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm, @unsafe.Pointer key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    if (Ꮡm == nil) {
        throw panic(errNilAssign);
    }
    if (race.Enabled) {
        var callerpc = sys.GetCallerPC();
        var pc = abi.FuncPCABIInternal(runtime_mapassign_fast32ptr);
        race.WritePC(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, pc);
    }
    if (m.writing != 0) {
        fatal(concurrentMapWritesˢ);
    }
    ref var k = ref heap<@unsafe.Pointer>(out var Ꮡk);
    k = key;
    var hash = typ.Hasher((uintptr)abi.NoEscape(@unsafe.Pointer.FromBox(Ꮡk)), m.seed);
    System.GC.KeepAlive(Ꮡk);
    // Set writing after calling Hasher, since Hasher may panic, in which
    // case we have not actually done a write.
    m.writing ^= (uint8)(1); // toggle, see comment on writing
    if (m.dirPtr == nil) {
        m.growToSmall(Ꮡtyp);
    }
    if (m.dirLen == 0) {
        if (m.used < abi.SwissMapGroupSlots) {
            @unsafe.Pointer elem = (uintptr)m.putSlotSmallFastPtr(Ꮡtyp, hash, key);
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
        // As we look for a match, keep track of the first deleted slot we
        // find, which we'll use to insert the new entry if necessary.
        groupReference firstDeletedGroup = default!;
        uintptr firstDeletedSlot = default!;
        for (; ᐧ ; seq = seq.next()) {
            var g = t.of(table.Ꮡgroups).group(Ꮡtyp, seq.offset);
            var match = (~g.ctrls()).matchH2(h2(hash));
            // Look for an existing slot containing this key.
            while (match != 0) {
                var iΔ1 = match.first();
                @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, iΔ1);
                if (key == ~(ж<@unsafe.Pointer>)(uintptr)(slotKey)) {
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
                ((ж<@unsafe.Pointer>)(uintptr)(slotKey)).Value = key;
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

//go:linkname runtime_mapdelete_fast32 runtime.mapdelete_fast32
internal static void runtime_mapdelete_fast32(ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm, uint32 keyʗp) {
    ref var m = ref Ꮡm.DerefOrNull();

    ref var key = ref heap(keyʗp, out var Ꮡkey);
    if (race.Enabled) {
        var callerpc = sys.GetCallerPC();
        var pc = abi.FuncPCABIInternal(runtime_mapdelete_fast32);
        race.WritePC(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, pc);
    }
    if (Ꮡm == nil || m.Used() == 0) {
        return;
    }
    Ꮡm.Delete(Ꮡtyp, (uintptr)abi.NoEscape(@unsafe.Pointer.FromPinnedBox(Ꮡkey)));
    System.GC.KeepAlive(Ꮡkey);
}

} // end maps_package
