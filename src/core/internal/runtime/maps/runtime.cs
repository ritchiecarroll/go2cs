// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using abi = go.@internal.abi_package;
using asan = go.@internal.asan_package;
using msan = go.@internal.msan_package;
using race = go.@internal.race_package;
using sys = go.@internal.runtime.sys_package;
using @unsafe = unsafe_package;
using go.@internal;
using go.@internal.runtime;

partial class maps_package {

// Functions below pushed from runtime.

//go:linkname fatal
internal static partial void fatal(@string s);

//go:linkname rand
internal static partial uint64 rand();

//go:linkname typedmemmove
internal static partial void typedmemmove(ж<abi.Type> typ, @unsafe.Pointer dst, @unsafe.Pointer src);

//go:linkname typedmemclr
internal static partial void typedmemclr(ж<abi.Type> typ, @unsafe.Pointer ptr);

//go:linkname newarray
internal static partial @unsafe.Pointer newarray(ж<abi.Type> typ, nint n);

//go:linkname newobject
internal static partial @unsafe.Pointer newobject(ж<abi.Type> typ);

// Pushed from runtime in order to use runtime.plainError
//
//go:linkname errNilAssign
public static error errNilAssign;

// Pull from runtime. It is important that is this the exact same copy as the
// runtime because runtime.mapaccess1_fat compares the returned pointer with
// &runtime.zeroVal[0].
// TODO: move zeroVal to internal/abi?
//
//go:linkname zeroVal runtime.zeroVal
internal static readonly ж<array<byte>> ᏑzeroVal = new StaticBox<array<byte>>(new array<byte>(1024));
internal static ref array<byte> zeroVal => ref ((StaticBox<array<byte>>)ᏑzeroVal).ValueSlot;

// mapaccess1 returns a pointer to h[key].  Never returns nil, instead
// it will return a reference to the zero object for the elem type if
// the key is not in the map.
// NOTE: The returned pointer may keep the whole map live, so don't
// hold onto it for very long.
//
//go:linkname runtime_mapaccess1 runtime.mapaccess1
internal static @unsafe.Pointer runtime_mapaccess1(ж<abiꓸMapType> Ꮡtyp, ж<ΔMap> Ꮡm, @unsafe.Pointer key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    if (race.Enabled && Ꮡm != nil) {
        var callerpc = sys.GetCallerPC();
        var pc = abi.FuncPCABIInternal(runtime_mapaccess1);
        race.ReadPC(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, pc);
        race.ReadObjectPC(typ.Key, key, callerpc, pc);
    }
    if (msan.Enabled && Ꮡm != nil) {
        msan.Read(key, typ.Key.Size_);
    }
    if (asan.Enabled && Ꮡm != nil) {
        asan.Read(key, typ.Key.Size_);
    }

    if (Ꮡm == nil || m.Used() == 0) {
        {
            var err = mapKeyError(Ꮡtyp, key); if (err != nil) {
                throw panic(err); // see issue 23734
            }
        }
        return @unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0));
    }

    if (m.writing != 0) {
        fatal(concurrentMapReadAndMapˢ);
    }

    var hash = typ.Hasher(key, m.seed);

    if (m.dirLen <= 0) {
        var (_, elem, ok) = m.getWithKeySmall(Ꮡtyp, hash, key);
        if (!ok) {
            return @unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0));
        }
        return elem;
    }

    // Select table.
    var idx = m.directoryIndex(hash);
    var t = m.directoryAt(idx);

    // Probe table.
    var seq = makeProbeSeq(h1(hash), t.groups.lengthMask);
    var h2Hash = h2(hash);
    for (; ; seq = seq.next()) {
        var g = t.of(table.Ꮡgroups).group(Ꮡtyp, seq.offset);

        var match = (~g.ctrls()).matchH2(h2Hash);

        while (match != 0) {
            var i = match.first();

            @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, i);
            @unsafe.Pointer slotKeyOrig = slotKey;
            if (typ.IndirectKey()) {
                slotKey = ((ж<@unsafe.Pointer>)(uintptr)(slotKey)).Value;
            }
            if (typ.Key.Equal(key, slotKey)) {
                @unsafe.Pointer slotElem = (@unsafe.Pointer)((uintptr)slotKeyOrig + typ.ElemOff);
                if (typ.IndirectElem()) {
                    slotElem = ((ж<@unsafe.Pointer>)(uintptr)(slotElem)).Value;
                }
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

//go:linkname runtime_mapaccess2 runtime.mapaccess2
internal static (@unsafe.Pointer, bool) runtime_mapaccess2(ж<abiꓸMapType> Ꮡtyp, ж<ΔMap> Ꮡm, @unsafe.Pointer key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    if (race.Enabled && Ꮡm != nil) {
        var callerpc = sys.GetCallerPC();
        var pc = abi.FuncPCABIInternal(runtime_mapaccess1);
        race.ReadPC(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, pc);
        race.ReadObjectPC(typ.Key, key, callerpc, pc);
    }
    if (msan.Enabled && Ꮡm != nil) {
        msan.Read(key, typ.Key.Size_);
    }
    if (asan.Enabled && Ꮡm != nil) {
        asan.Read(key, typ.Key.Size_);
    }

    if (Ꮡm == nil || m.Used() == 0) {
        {
            var err = mapKeyError(Ꮡtyp, key); if (err != nil) {
                throw panic(err); // see issue 23734
            }
        }
        return (@unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0)), false);
    }

    if (m.writing != 0) {
        fatal(concurrentMapReadAndMapˢ);
    }

    var hash = typ.Hasher(key, m.seed);

    if (m.dirLen == 0) {
        var (_, elem, ok) = m.getWithKeySmall(Ꮡtyp, hash, key);
        if (!ok) {
            return (@unsafe.Pointer.FromPinnedBox(ᏑzeroVal.at<byte>(0)), false);
        }
        return (elem, true);
    }

    // Select table.
    var idx = m.directoryIndex(hash);
    var t = m.directoryAt(idx);

    // Probe table.
    var seq = makeProbeSeq(h1(hash), t.groups.lengthMask);
    var h2Hash = h2(hash);
    for (; ; seq = seq.next()) {
        var g = t.of(table.Ꮡgroups).group(Ꮡtyp, seq.offset);

        var match = (~g.ctrls()).matchH2(h2Hash);

        while (match != 0) {
            var i = match.first();

            @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, i);
            @unsafe.Pointer slotKeyOrig = slotKey;
            if (typ.IndirectKey()) {
                slotKey = ((ж<@unsafe.Pointer>)(uintptr)(slotKey)).Value;
            }
            if (typ.Key.Equal(key, slotKey)) {
                @unsafe.Pointer slotElem = (@unsafe.Pointer)((uintptr)slotKeyOrig + typ.ElemOff);
                if (typ.IndirectElem()) {
                    slotElem = ((ж<@unsafe.Pointer>)(uintptr)(slotElem)).Value;
                }
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

//go:linkname runtime_mapassign runtime.mapassign
internal static @unsafe.Pointer runtime_mapassign(ж<abiꓸMapType> Ꮡtyp, ж<ΔMap> Ꮡm, @unsafe.Pointer key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    if (Ꮡm == nil) {
        throw panic(errNilAssign);
    }
    if (race.Enabled) {
        var callerpc = sys.GetCallerPC();
        var pc = abi.FuncPCABIInternal(runtime_mapassign);
        race.WritePC(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, pc);
        race.ReadObjectPC(typ.Key, key, callerpc, pc);
    }
    if (msan.Enabled) {
        msan.Read(key, typ.Key.Size_);
    }
    if (asan.Enabled) {
        asan.Read(key, typ.Key.Size_);
    }
    if (m.writing != 0) {
        fatal(concurrentMapWritesˢ);
    }

    var hash = typ.Hasher(key, m.seed);

    // Set writing after calling Hasher, since Hasher may panic, in which
    // case we have not actually done a write.
    m.writing ^= (uint8)(1); // toggle, see comment on writing

    if (m.dirPtr == nil) {
        m.growToSmall(Ꮡtyp);
    }

    if (m.dirLen == 0) {
        if (m.used < abi.MapGroupSlots) {
            @unsafe.Pointer elem = (uintptr)m.putSlotSmall(Ꮡtyp, hash, key);

            if (m.writing == 0) {
                fatal(concurrentMapWritesˢ);
            }
            m.writing ^= (uint8)(1);

            return elem;
        }

        // Can't fit another entry, grow to full size map.
        m.growToTable(Ꮡtyp);
    }

    @unsafe.Pointer slotElem = default;
outer:
    for (;;) {
        // Select table.
        var idx = m.directoryIndex(hash);
        var t = m.directoryAt(idx);

        var seq = makeProbeSeq(h1(hash), t.groups.lengthMask);

        // As we look for a match, keep track of the first deleted slot
        // we find, which we'll use to insert the new entry if
        // necessary.
        groupReference firstDeletedGroup = default;
        uintptr firstDeletedSlot = default;

        var h2Hash = h2(hash);
        for (; ; seq = seq.next()) {
            var g = t.of(table.Ꮡgroups).group(Ꮡtyp, seq.offset);
            var match = (~g.ctrls()).matchH2(h2Hash);

            // Look for an existing slot containing this key.
            while (match != 0) {
                var i = match.first();

                @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, i);
                @unsafe.Pointer slotKeyOrig = slotKey;
                if (typ.IndirectKey()) {
                    slotKey = ((ж<@unsafe.Pointer>)(uintptr)(slotKey)).Value;
                }
                if (typ.Key.Equal(key, slotKey)) {
                    if (typ.NeedKeyUpdate()) {
                        typedmemmove(typ.Key, slotKey, key);
                    }

                    slotElem = (@unsafe.Pointer)((uintptr)slotKeyOrig + typ.ElemOff);
                    if (typ.IndirectElem()) {
                        slotElem = ((ж<@unsafe.Pointer>)(uintptr)(slotElem)).Value;
                    }

                    t.checkInvariants(Ꮡtyp, Ꮡm);
                    goto break_outer;
                }
                match = match.removeFirst();
            }

            // No existing slot for this key in this group. Is this the end
            // of the probe sequence?
            match = (~g.ctrls()).matchEmpty();
            if (match != 0) {
                // Finding an empty slot means we've reached the end of
                // the probe sequence.

                uintptr i = default;

                // If we found a deleted slot along the way, we
                // can replace it without consuming growthLeft.
                if (firstDeletedGroup.data != nil) {
                    g = firstDeletedGroup;
                    i = firstDeletedSlot;
                    t.growthLeft++; // will be decremented below to become a no-op.
                } else {
                    // Otherwise, use the empty slot.
                    i = match.first();
                }

                // If there is room left to grow, just insert the new entry.
                if (t.growthLeft > 0) {
                    @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, i);
                    @unsafe.Pointer slotKeyOrig = slotKey;
                    if (typ.IndirectKey()) {
                        @unsafe.Pointer kmem = (uintptr)newobject(typ.Key);
                        ((ж<@unsafe.Pointer>)(uintptr)(slotKey)).Value = kmem;
                        slotKey = kmem;
                    }
                    typedmemmove(typ.Key, slotKey, key);

                    slotElem = (@unsafe.Pointer)((uintptr)slotKeyOrig + typ.ElemOff);
                    if (typ.IndirectElem()) {
                        @unsafe.Pointer emem = (uintptr)newobject(typ.Elem);
                        ((ж<@unsafe.Pointer>)(uintptr)(slotElem)).Value = emem;
                        slotElem = emem;
                    }

                    g.ctrls().set(i, (ctrl)(uint8)h2Hash);
                    t.growthLeft--;
                    t.used++;
                    m.used++;

                    t.checkInvariants(Ꮡtyp, Ꮡm);
                    goto break_outer;
                }

                t.rehash(Ꮡtyp, Ꮡm);
                goto continue_outer;
            }

            // No empty slots in this group. Check for a deleted
            // slot, which we'll use if we don't find a match later
            // in the probe sequence.
            //
            // We only need to remember a single deleted slot.
            if (firstDeletedGroup.data == nil) {
                // Since we already checked for empty slots
                // above, matches here must be deleted slots.
                match = (~g.ctrls()).matchEmptyOrDeleted();
                if (match != 0) {
                    firstDeletedGroup = g;
                    firstDeletedSlot = match.first();
                }
            }
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

} // end maps_package
