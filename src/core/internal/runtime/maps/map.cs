// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package maps implements Go's builtin map type.
namespace go.@internal.runtime;

using abi = go.@internal.abi_package;
using goarch = go.@internal.goarch_package;
using math = go.@internal.runtime.math_package;
using sys = go.@internal.runtime.sys_package;
using @unsafe = unsafe_package;
using go.@internal;
using go.@internal.runtime;

partial class maps_package {

// This package contains the implementation of Go's builtin map type.
//
// The map design is based on Abseil's "Swiss Table" map design
// (https://abseil.io/about/design/swisstables), with additional modifications
// to cover Go's additional requirements, discussed below.
//
// Terminology:
// - Slot: A storage location of a single key/element pair.
// - Group: A group of abi.SwissMapGroupSlots (8) slots, plus a control word.
// - Control word: An 8-byte word which denotes whether each slot is empty,
//   deleted, or used. If a slot is used, its control byte also contains the
//   lower 7 bits of the hash (H2).
// - H1: Upper 57 bits of a hash.
// - H2: Lower 7 bits of a hash.
// - Table: A complete "Swiss Table" hash table. A table consists of one or
//   more groups for storage plus metadata to handle operation and determining
//   when to grow.
// - Map: The top-level Map type consists of zero or more tables for storage.
//   The upper bits of the hash select which table a key belongs to.
// - Directory: Array of the tables used by the map.
//
// At its core, the table design is similar to a traditional open-addressed
// hash table. Storage consists of an array of groups, which effectively means
// an array of key/elem slots with some control words interspersed. Lookup uses
// the hash to determine an initial group to check. If, due to collisions, this
// group contains no match, the probe sequence selects the next group to check
// (see below for more detail about the probe sequence).
//
// The key difference occurs within a group. In a standard open-addressed
// linear probed hash table, we would check each slot one at a time to find a
// match. A swiss table utilizes the extra control word to check all 8 slots in
// parallel.
//
// Each byte in the control word corresponds to one of the slots in the group.
// In each byte, 1 bit is used to indicate whether the slot is in use, or if it
// is empty/deleted. The other 7 bits contain the lower 7 bits of the hash for
// the key in that slot. See [ctrl] for the exact encoding.
//
// During lookup, we can use some clever bitwise manipulation to compare all 8
// 7-bit hashes against the input hash in parallel (see [ctrlGroup.matchH2]).
// That is, we effectively perform 8 steps of probing in a single operation.
// With SIMD instructions, this could be extended to 16 slots with a 16-byte
// control word.
//
// Since we only use 7 bits of the 64 bit hash, there is a 1 in 128 (~0.7%)
// probability of false positive on each slot, but that's fine: we always need
// double check each match with a standard key comparison regardless.
//
// Probing
//
// Probing is done using the upper 57 bits (H1) of the hash as an index into
// the groups array. Probing walks through the groups using quadratic probing
// until it finds a group with a match or a group with an empty slot. See
// [probeSeq] for specifics about the probe sequence. Note the probe
// invariants: the number of groups must be a power of two, and the end of a
// probe sequence must be a group with an empty slot (the table can never be
// 100% full).
//
// Deletion
//
// Probing stops when it finds a group with an empty slot. This affects
// deletion: when deleting from a completely full group, we must not mark the
// slot as empty, as there could be more slots used later in a probe sequence
// and this deletion would cause probing to stop too early. Instead, we mark
// such slots as "deleted" with a tombstone. If the group still has an empty
// slot, we don't need a tombstone and directly mark the slot empty. Insert
// prioritizes reuse of tombstones over filling an empty slots. Otherwise,
// tombstones are only completely cleared during grow, as an in-place cleanup
// complicates iteration.
//
// Growth
//
// The probe sequence depends on the number of groups. Thus, when growing the
// group count all slots must be reordered to match the new probe sequence. In
// other words, an entire table must be grown at once.
//
// In order to support incremental growth, the map splits its contents across
// multiple tables. Each table is still a full hash table, but an individual
// table may only service a subset of the hash space. Growth occurs on
// individual tables, so while an entire table must grow at once, each of these
// grows is only a small portion of a map. The maximum size of a single grow is
// limited by limiting the maximum size of a table before it is split into
// multiple tables.
//
// A map starts with a single table. Up to [maxTableCapacity], growth simply
// replaces this table with a replacement with double capacity. Beyond this
// limit, growth splits the table into two.
//
// The map uses "extendible hashing" to select which table to use. In
// extendible hashing, we use the upper bits of the hash as an index into an
// array of tables (called the "directory"). The number of bits uses increases
// as the number of tables increases. For example, when there is only 1 table,
// we use 0 bits (no selection necessary). When there are 2 tables, we use 1
// bit to select either the 0th or 1st table. [Map.globalDepth] is the number
// of bits currently used for table selection, and by extension (1 <<
// globalDepth), the size of the directory.
//
// Note that each table has its own load factor and grows independently. If the
// 1st bucket grows, it will split. We'll need 2 bits to select tables, though
// we'll have 3 tables total rather than 4. We support this by allowing
// multiple indicies to point to the same table. This example:
//
//	directory (globalDepth=2)
//	+----+
//	| 00 | --\
//	+----+    +--> table (localDepth=1)
//	| 01 | --/
//	+----+
//	| 10 | ------> table (localDepth=2)
//	+----+
//	| 11 | ------> table (localDepth=2)
//	+----+
//
// Tables track the depth they were created at (localDepth). It is necessary to
// grow the directory when splitting a table where globalDepth == localDepth.
//
// Iteration
//
// Iteration is the most complex part of the map due to Go's generous iteration
// semantics. A summary of semantics from the spec:
// 1. Adding and/or deleting entries during iteration MUST NOT cause iteration
//    to return the same entry more than once.
// 2. Entries added during iteration MAY be returned by iteration.
// 3. Entries modified during iteration MUST return their latest value.
// 4. Entries deleted during iteration MUST NOT be returned by iteration.
// 5. Iteration order is unspecified. In the implementation, it is explicitly
//    randomized.
//
// If the map never grows, these semantics are straightforward: just iterate
// over every table in the directory and every group and slot in each table.
// These semantics all land as expected.
//
// If the map grows during iteration, things complicate significantly. First
// and foremost, we need to track which entries we already returned to satisfy
// (1). There are three types of grow:
// a. A table replaced by a single larger table.
// b. A table split into two replacement tables.
// c. Growing the directory (occurs as part of (b) if necessary).
//
// For all of these cases, the replacement table(s) will have a different probe
// sequence, so simply tracking the current group and slot indices is not
// sufficient.
//
// For (a) and (b), note that grows of tables other than the one we are
// currently iterating over are irrelevant.
//
// We handle (a) and (b) by having the iterator keep a reference to the table
// it is currently iterating over, even after the table is replaced. We keep
// iterating over the original table to maintain the iteration order and avoid
// violating (1). Any new entries added only to the replacement table(s) will
// be skipped (allowed by (2)). To avoid violating (3) or (4), while we use the
// original table to select the keys, we must look them up again in the new
// table(s) to determine if they have been modified or deleted. There is yet
// another layer of complexity if the key does not compare equal itself. See
// [Iter.Next] for the gory details.
//
// Note that for (b) once we finish iterating over the old table we'll need to
// skip the next entry in the directory, as that contains the second split of
// the old table. We can use the old table's localDepth to determine the next
// logical index to use.
//
// For (b), we must adjust the current directory index when the directory
// grows. This is more straightforward, as the directory orders remains the
// same after grow, so we just double the index if the directory size doubles.

// Extracts the H1 portion of a hash: the 57 upper bits.
// TODO(prattmic): what about 32-bit systems?
internal static uintptr h1(uintptr h) {
    return (h >> (int)(7));
}

// Extracts the H2 portion of a hash: the 7 bits not used for h1.
//
// These are used as an occupied control byte.
internal static uintptr h2(uintptr h) {
    return (uintptr)(h & 0x7f);
}

partial struct ΔMap {
    // The number of filled slots (i.e. the number of elements in all
    // tables). Excludes deleted slots.
    // Must be first (known by the compiler, for len() builtin).
    internal uint64 used;
    // seed is the hash seed, computed as a unique random number per map.
    internal uintptr seed;
    // The directory of tables.
    //
    // Normally dirPtr points to an array of table pointers
    //
    // dirPtr *[dirLen]*table
    //
    // The length (dirLen) of this array is `1 << globalDepth`. Multiple
    // entries may point to the same table. See top-level comment for more
    // details.
    //
    // Small map optimization: if the map always contained
    // abi.SwissMapGroupSlots or fewer entries, it fits entirely in a
    // single group. In that case dirPtr points directly to a single group.
    //
    // dirPtr *group
    //
    // In this case, dirLen is 0. used counts the number of used slots in
    // the group. Note that small maps never have deleted slots (as there
    // is no probe sequence to maintain).
    internal @unsafe.Pointer dirPtr;
    internal nint dirLen;
    // The number of bits to use in table directory lookups.
    internal uint8 globalDepth;
    // The number of bits to shift out of the hash for directory lookups.
    // On 64-bit systems, this is 64 - globalDepth.
    internal uint8 globalShift;
    // writing is a flag that is toggled (XOR 1) while the map is being
    // written. Normally it is set to 1 when writing, but if there are
    // multiple concurrent writers, then toggling increases the probability
    // that both sides will detect the race.
    internal uint8 writing;
    // clearSeq is a sequence counter of calls to Clear. It is used to
    // detect map clears during iteration.
    internal uint64 clearSeq;
}

internal static uint8 depthToShift(uint8 depth) {
    if (goarch.PtrSize == 4) {
        return (uint8)(32 - depth);
    }
    return (uint8)(64 - depth);
}

// If m is non-nil, it should be used rather than allocating.
//
// maxAlloc should be runtime.maxAlloc.
//
// TODO(prattmic): Put maxAlloc somewhere accessible.
public static ж<ΔMap> NewMap(ж<abi.SwissMapType> Ꮡmt, uintptr hint, ж<ΔMap> Ꮡm, uintptr maxAlloc) {
    ref var mt = ref Ꮡmt.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    if (Ꮡm == nil) {
        Ꮡm = @new<ΔMap>(); m = ref Ꮡm.DerefOrNull();
    }
    m.seed = (uintptr)rand();
    if (hint <= abi.SwissMapGroupSlots) {
        // A small map can fill all 8 slots, so no need to increase
        // target capacity.
        //
        // In fact, since an 8 slot group is what the first assignment
        // to an empty map would allocate anyway, it doesn't matter if
        // we allocate here or on the first assignment.
        //
        // Thus we just return without allocating. (We'll save the
        // allocation completely if no assignment comes.)
        // Note that the compiler may have initialized m.dirPtr with a
        // pointer to a stack-allocated group, in which case we already
        // have a group. The control word is already initialized.
        return Ꮡm;
    }
    // Full size map.
    // Set initial capacity to hold hint entries without growing in the
    // average case.
    var targetCapacity = (hint * (uintptr)abi.SwissMapGroupSlots) / (uintptr)maxAvgGroupLoad;
    if (targetCapacity < hint) {
        // overflow
        return Ꮡm; // return an empty map.
    }
    var dirSize = ((uint64)targetCapacity + (uint64)maxTableCapacity - 1) / (uint64)maxTableCapacity;
    (dirSize, var overflow) = alignUpPow2(dirSize);
    if (overflow || dirSize > (uint64)math.MaxUintptr) {
        return Ꮡm; // return an empty map.
    }
    // Reject hints that are obviously too large.
    (var groups, overflow) = math.MulUintptr((uintptr)dirSize, maxTableCapacity);
    if (overflow){
        return Ꮡm; // return an empty map.
    } else {
        var (mem, overflowΔ1) = math.MulUintptr(groups, mt.GroupSize);
        if (overflowΔ1 || mem > maxAlloc) {
            return Ꮡm; // return an empty map.
        }
    }
    m.globalDepth = (uint8)sys.TrailingZeros64(dirSize);
    m.globalShift = depthToShift(m.globalDepth);
    var directory = new slice<ж<table>>((nint)(dirSize));
    foreach (var (i, _) in directory) {
        // TODO: Think more about initial table capacity.
        directory[i] = newTable(Ꮡmt, (uint64)targetCapacity / dirSize, i, m.globalDepth);
    }
    m.dirPtr = @unsafe.Pointer.FromBox(Ꮡ(directory, 0));
    m.dirLen = len(directory);
    return Ꮡm;
}

public static ж<ΔMap> NewEmptyMap() {
    var m = @new<ΔMap>();
    m.Value.seed = (uintptr)rand();
    // See comment in NewMap. No need to eager allocate a group.
    return m;
}

internal static uintptr directoryIndex(this ref ΔMap m, uintptr hash) {
    if (m.dirLen == 1) {
        return 0;
    }
    return (hash >> (int)(((uint8)(m.globalShift & 63))));
}

internal static ж<table> directoryAt(this ref ΔMap m, uintptr i) {
    return ~(ж<ж<table>>)(uintptr)((@unsafe.Pointer)((uintptr)m.dirPtr + (uintptr)goarch.PtrSize * i));
}

internal static void directorySet(this ref ΔMap m, uintptr i, ж<table> Ꮡnt) {
    ((ж<ж<table>>)(uintptr)((@unsafe.Pointer)((uintptr)m.dirPtr + (uintptr)goarch.PtrSize * i))).ValueSlot = Ꮡnt;
}

internal static void replaceTable(this ref ΔMap m, ж<table> Ꮡnt) {
    ref var nt = ref Ꮡnt.DerefOrNull();

    // The number of entries that reference the same table doubles for each
    // time the globalDepth grows without the table splitting.
    nint entries = ((nint)1).Lsh((uint64)((uint8)(m.globalDepth - nt.localDepth)));
    for (nint i = 0; i < entries; i++) {
        //m.directory[nt.index+i] = nt
        m.directorySet((uintptr)(nt.index + i), Ꮡnt);
    }
}

internal static void installTableSplit(this ref ΔMap m, ж<table> Ꮡold, ж<table> Ꮡleft, ж<table> Ꮡright) {
    ref var old = ref Ꮡold.DerefOrNull();
    ref var left = ref Ꮡleft.DerefOrNull();
    ref var right = ref Ꮡright.DerefOrNull();

    if (old.localDepth == m.globalDepth) {
        // No room for another level in the directory. Grow the
        // directory.
        var newDir = new slice<ж<table>>(m.dirLen * 2);
        foreach (var i in range(m.dirLen)) {
            var t = m.directoryAt((uintptr)i);
            newDir[2 * i] = t;
            newDir[2 * i + 1] = t;
            // t may already exist in multiple indicies. We should
            // only update t.index once. Since the index must
            // increase, seeing the original index means this must
            // be the first time we've encountered this table.
            if ((~t).index == i) {
                t.Value.index = 2 * i;
            }
        }
        m.globalDepth++;
        m.globalShift--;
        //m.directory = newDir
        m.dirPtr = @unsafe.Pointer.FromBox(Ꮡ(newDir, 0));
        m.dirLen = len(newDir);
    }
    // N.B. left and right may still consume multiple indicies if the
    // directory has grown multiple times since old was last split.
    left.index = old.index;
    m.replaceTable(Ꮡleft);
    nint entries = ((nint)1).Lsh((uint64)((uint8)(m.globalDepth - left.localDepth)));
    right.index = left.index + entries;
    m.replaceTable(Ꮡright);
}

public static uint64 Used(this ref ΔMap m) {
    return m.used;
}

// Get performs a lookup of the key that key points to. It returns a pointer to
// the element, or false if the key doesn't exist.
public static (@unsafe.Pointer, bool) Get(this ref ΔMap m, ж<abi.SwissMapType> Ꮡtyp, @unsafe.Pointer key) {
    return m.getWithoutKey(Ꮡtyp, key);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string concurrentMapReadAndMapˢ = "concurrent map read and map write"u8;

internal static (@unsafe.Pointer, @unsafe.Pointer, bool) getWithKey(this ref ΔMap m, ж<abi.SwissMapType> Ꮡtyp, @unsafe.Pointer key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (m.Used() == 0) {
        return (default!, default!, false);
    }
    if (m.writing != 0) {
        fatal(concurrentMapReadAndMapˢ);
    }
    var hash = typ.Hasher(key, m.seed);
    if (m.dirLen == 0) {
        return m.getWithKeySmall(Ꮡtyp, hash, key);
    }
    var idx = m.directoryIndex(hash);
    return m.directoryAt(idx).getWithKey(Ꮡtyp, hash, key);
}

internal static (@unsafe.Pointer, bool) getWithoutKey(this ref ΔMap m, ж<abi.SwissMapType> Ꮡtyp, @unsafe.Pointer key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (m.Used() == 0) {
        return (default!, false);
    }
    if (m.writing != 0) {
        fatal(concurrentMapReadAndMapˢ);
    }
    var hash = typ.Hasher(key, m.seed);
    if (m.dirLen == 0) {
        var (_, elem, ok) = m.getWithKeySmall(Ꮡtyp, hash, key);
        return (elem, ok);
    }
    var idx = m.directoryIndex(hash);
    return m.directoryAt(idx).getWithoutKey(Ꮡtyp, hash, key);
}

internal static (@unsafe.Pointer, @unsafe.Pointer, bool) getWithKeySmall(this ref ΔMap m, ж<abi.SwissMapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    var g = new groupReference(
        data: m.dirPtr
    );
    var h2Δ1 = (uint8)h2(hash);
    var ctrls = g.ctrls().Value;
    for (var i = (uintptr)0; i < abi.SwissMapGroupSlots; i++) {
        var c = (uint8)(uint64)ctrls;
        ctrls >>= (int)(8);
        if (c != h2Δ1) {
            continue;
        }
        @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, i);
        if (typ.IndirectKey()) {
            slotKey = ((ж<@unsafe.Pointer>)(uintptr)(slotKey)).Value;
        }
        if ((~typ.Key).Equal(key, slotKey)) {
            @unsafe.Pointer slotElem = (uintptr)g.elem(Ꮡtyp, i);
            if (typ.IndirectElem()) {
                slotElem = ((ж<@unsafe.Pointer>)(uintptr)(slotElem)).Value;
            }
            return (slotKey, slotElem, true);
        }
    }
    return (default!, default!, false);
}

public static void Put(this ж<ΔMap> Ꮡm, ж<abi.SwissMapType> Ꮡtyp, @unsafe.Pointer key, @unsafe.Pointer elem) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    @unsafe.Pointer slotElem = (uintptr)Ꮡm.PutSlot(Ꮡtyp, key);
    typedmemmove(typ.Elem, slotElem, elem);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string concurrentMapWritesˢ = "concurrent map writes"u8;

// PutSlot returns a pointer to the element slot where an inserted element
// should be written.
//
// PutSlot never returns nil.
public static @unsafe.Pointer PutSlot(this ж<ΔMap> Ꮡm, ж<abi.SwissMapType> Ꮡtyp, @unsafe.Pointer key) {
    ref var m = ref Ꮡm.DerefOrNull();
    ref var typ = ref Ꮡtyp.DerefOrNull();

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
        if (m.used < abi.SwissMapGroupSlots) {
            @unsafe.Pointer elem = (uintptr)m.putSlotSmall(Ꮡtyp, hash, key);
            if (m.writing == 0) {
                fatal(concurrentMapWritesˢ);
            }
            m.writing ^= (uint8)(1);
            return elem;
        }
        // Can't fit another entry, grow to full size map.
        //
        // TODO(prattmic): If this is an update to an existing key then
        // we actually don't need to grow.
        m.growToTable(Ꮡtyp);
    }
    while (ᐧ) {
        var idx = m.directoryIndex(hash);
        var (elem, ok) = m.directoryAt(idx).PutSlot(Ꮡtyp, Ꮡm, hash, key);
        if (!ok) {
            continue;
        }
        if (m.writing == 0) {
            fatal(concurrentMapWritesˢ);
        }
        m.writing ^= (uint8)(1);
        return elem;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string smallMapWithNoEmptySlotˢ = "small map with no empty slot (concurrent map writes?)"u8;

internal static @unsafe.Pointer putSlotSmall(this ref ΔMap m, ж<abi.SwissMapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    var g = new groupReference(
        data: m.dirPtr
    );
    var match = (~g.ctrls()).matchH2(h2(hash));
    // Look for an existing slot containing this key.
    while (match != 0) {
        var iΔ1 = match.first();
        @unsafe.Pointer slotKeyΔ1 = (uintptr)g.key(Ꮡtyp, iΔ1);
        if (typ.IndirectKey()) {
            slotKeyΔ1 = ((ж<@unsafe.Pointer>)(uintptr)(slotKeyΔ1)).Value;
        }
        if ((~typ.Key).Equal(key, slotKeyΔ1)) {
            if (typ.NeedKeyUpdate()) {
                typedmemmove(typ.Key, slotKeyΔ1, key);
            }
            @unsafe.Pointer slotElemΔ1 = (uintptr)g.elem(Ꮡtyp, iΔ1);
            if (typ.IndirectElem()) {
                slotElemΔ1 = ((ж<@unsafe.Pointer>)(uintptr)(slotElemΔ1)).Value;
            }
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
        return default!;
    }
    var i = match.first();
    @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, i);
    if (typ.IndirectKey()) {
        @unsafe.Pointer kmem = (uintptr)newobject(typ.Key);
        ((ж<@unsafe.Pointer>)(uintptr)(slotKey)).Value = kmem;
        slotKey = kmem;
    }
    typedmemmove(typ.Key, slotKey, key);
    @unsafe.Pointer slotElem = (uintptr)g.elem(Ꮡtyp, i);
    if (typ.IndirectElem()) {
        @unsafe.Pointer emem = (uintptr)newobject(typ.Elem);
        ((ж<@unsafe.Pointer>)(uintptr)(slotElem)).Value = emem;
        slotElem = emem;
    }
    g.ctrls().set(i, ((ctrl)(uint8)h2(hash)));
    m.used++;
    return slotElem;
}

internal static void growToSmall(this ref ΔMap m, ж<abi.SwissMapType> Ꮡtyp) {
    var grp = newGroups(ref (Ꮡtyp).DerefOrNull(), 1);
    m.dirPtr = grp.data;
    var g = new groupReference(
        data: m.dirPtr
    );
    g.ctrls().setEmpty();
}

internal static void growToTable(this ref ΔMap m, ж<abi.SwissMapType> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    var tab = newTable(Ꮡtyp, 2 * abi.SwissMapGroupSlots, 0, 0);
    var g = new groupReference(
        data: m.dirPtr
    );
    for (var i = (uintptr)0; i < abi.SwissMapGroupSlots; i++) {
        if (((ctrl)(g.ctrls().get(i) & ctrlEmpty)) == ctrlEmpty) {
            // Empty
            continue;
        }
        @unsafe.Pointer key = (uintptr)g.key(Ꮡtyp, i);
        if (typ.IndirectKey()) {
            key = ((ж<@unsafe.Pointer>)(uintptr)(key)).Value;
        }
        @unsafe.Pointer elem = (uintptr)g.elem(Ꮡtyp, i);
        if (typ.IndirectElem()) {
            elem = ((ж<@unsafe.Pointer>)(uintptr)(elem)).Value;
        }
        var hash = typ.Hasher(key, m.seed);
        tab.uncheckedPutSlot(Ꮡtyp, hash, key, elem);
    }
    var directory = new slice<ж<table>>(1);
    directory[0] = tab;
    m.dirPtr = @unsafe.Pointer.FromBox(Ꮡ(directory, 0));
    m.dirLen = len(directory);
    m.globalDepth = 0;
    m.globalShift = depthToShift(m.globalDepth);
}

public static void Delete(this ж<ΔMap> Ꮡm, ж<abi.SwissMapType> Ꮡtyp, @unsafe.Pointer key) {
    ref var m = ref Ꮡm.DerefOrNull();
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (Ꮡm == nil || m.Used() == 0) {
        {
            var err = mapKeyError(Ꮡtyp, key); if (err != default!) {
                throw panic(err); // see issue 23734
            }
        }
        return;
    }
    if (m.writing != 0) {
        fatal(concurrentMapWritesˢ);
    }
    var hash = typ.Hasher(key, m.seed);
    // Set writing after calling Hasher, since Hasher may panic, in which
    // case we have not actually done a write.
    m.writing ^= (uint8)(1); // toggle, see comment on writing
    if (m.dirLen == 0){
        m.deleteSmall(Ꮡtyp, hash, key);
    } else {
        var idx = m.directoryIndex(hash);
        m.directoryAt(idx).Delete(Ꮡtyp, Ꮡm, hash, key);
    }
    if (m.used == 0) {
        // Reset the hash seed to make it more difficult for attackers
        // to repeatedly trigger hash collisions. See
        // https://go.dev/issue/25237.
        m.seed = (uintptr)rand();
    }
    if (m.writing == 0) {
        fatal(concurrentMapWritesˢ);
    }
    m.writing ^= (uint8)(1);
}

internal static void deleteSmall(this ref ΔMap m, ж<abi.SwissMapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    var g = new groupReference(
        data: m.dirPtr
    );
    var match = (~g.ctrls()).matchH2(h2(hash));
    while (match != 0) {
        var i = match.first();
        @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, i);
        @unsafe.Pointer origSlotKey = slotKey;
        if (typ.IndirectKey()) {
            slotKey = ((ж<@unsafe.Pointer>)(uintptr)(slotKey)).Value;
        }
        if ((~typ.Key).Equal(key, slotKey)) {
            m.used--;
            if (typ.IndirectKey()){
                // Clearing the pointer is sufficient.
                ((ж<@unsafe.Pointer>)(uintptr)(origSlotKey)).Value = default!;
            } else 
            if (typ.Key.Pointers()) {
                // Only bother clearing if there are pointers.
                typedmemclr(typ.Key, slotKey);
            }
            @unsafe.Pointer slotElem = (uintptr)g.elem(Ꮡtyp, i);
            if (typ.IndirectElem()){
                // Clearing the pointer is sufficient.
                ((ж<@unsafe.Pointer>)(uintptr)(slotElem)).Value = default!;
            } else {
                // Unlike keys, always clear the elem (even if
                // it contains no pointers), as compound
                // assignment operations depend on cleared
                // deleted values. See
                // https://go.dev/issue/25936.
                typedmemclr(typ.Elem, slotElem);
            }
            // We only have 1 group, so it is OK to immediately
            // reuse deleted slots.
            g.ctrls().set(i, ctrlEmpty);
            return;
        }
        match = match.removeFirst();
    }
}

// Clear deletes all entries from the map resulting in an empty map.
public static void Clear(this ж<ΔMap> Ꮡm, ж<abi.SwissMapType> Ꮡtyp) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (Ꮡm == nil || m.Used() == 0) {
        return;
    }
    if (m.writing != 0) {
        fatal(concurrentMapWritesˢ);
    }
    m.writing ^= (uint8)(1); // toggle, see comment on writing
    if (m.dirLen == 0){
        m.clearSmall(Ꮡtyp);
    } else {
        ж<table> lastTab = default!;
        foreach (var i in range(m.dirLen)) {
            var t = m.directoryAt((uintptr)i);
            if (t == lastTab) {
                continue;
            }
            t.Clear(Ꮡtyp);
            lastTab = t;
        }
        m.used = 0;
        m.clearSeq++;
    }
    // TODO: shrink directory?
    // Reset the hash seed to make it more difficult for attackers to
    // repeatedly trigger hash collisions. See https://go.dev/issue/25237.
    m.seed = (uintptr)rand();
    if (m.writing == 0) {
        fatal(concurrentMapWritesˢ);
    }
    m.writing ^= (uint8)(1);
}

internal static void clearSmall(this ref ΔMap m, ж<abi.SwissMapType> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    var g = new groupReference(
        data: m.dirPtr
    );
    typedmemclr(typ.Group, g.data);
    g.ctrls().setEmpty();
    m.used = 0;
    m.clearSeq++;
}

} // end maps_package
