// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package maps implements Go's builtin map type.
namespace go.@internal.runtime;

using abi = go.@internal.abi_package;
using goarch = go.@internal.goarch_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class maps_package {

// Maximum size of a table before it is split at the directory level.
//
// TODO: Completely made up value. This should be tuned for performance vs grow
// latency.
// TODO: This should likely be based on byte size, as copying costs will
// dominate grow latency for large objects.
internal static UntypedInt maxTableCapacity => 1024;

// Ensure the max capacity fits in uint16, used for capacity and growthLeft
// below.
internal static uint16 _ᴛ1ʗ = (uint16)maxTableCapacity;

// table is a Swiss table hash table structure.
//
// Each table is a complete hash table implementation.
//
// Map uses one or more tables to store entries. Extendible hashing (hash
// prefix) is used to select the table to use for a specific key. Using
// multiple tables enables incremental growth by growing only one table at a
// time.
[GoType] public partial struct table {
    // The number of filled slots (i.e. the number of elements in the table).
    internal uint16 used;
    // The total number of slots (always 2^N). Equal to
    // `(groups.lengthMask+1)*abi.SwissMapGroupSlots`.
    internal uint16 capacity;
    // The number of slots we can still fill without needing to rehash.
    //
    // We rehash when used + tombstones > loadFactor*capacity, including
    // tombstones so the table doesn't overfill with tombstones. This field
    // counts down remaining empty slots before the next rehash.
    internal uint16 growthLeft;
    // The number of bits used by directory lookups above this table. Note
    // that this may be less then globalDepth, if the directory has grown
    // but this table has not yet been split.
    internal uint8 localDepth;
    // Index of this table in the Map directory. This is the index of the
    // _first_ location in the directory. The table may occur in multiple
    // sequential indicies.
    //
    // index is -1 if the table is stale (no longer installed in the
    // directory).
    internal nint index;
    // groups is an array of slot groups. Each group holds abi.SwissMapGroupSlots
    // key/elem slots and their control bytes. A table has a fixed size
    // groups array. The table is replaced (in rehash) when more space is
    // required.
    //
    // TODO(prattmic): keys and elements are interleaved to maximize
    // locality, but it comes at the expense of wasted space for some types
    // (consider uint8 key, uint64 element). Consider placing all keys
    // together in these cases to save space.
    internal groupsReference groups;
}

internal static ж<table> newTable(ж<abi.SwissMapType> Ꮡtyp, uint64 capacity, nint index, uint8 localDepth) {
    if (capacity < abi.SwissMapGroupSlots) {
        capacity = abi.SwissMapGroupSlots;
    }
    var t = Ꮡ(new table(
        index: index,
        localDepth: localDepth
    ));
    if (capacity > maxTableCapacity) {
        throw panic("initial table capacity too large");
    }
    // N.B. group count must be a power of two for probeSeq to visit every
    // group.
    (capacity, var overflow) = alignUpPow2(capacity);
    if (overflow) {
        throw panic("rounded-up capacity overflows uint64");
    }
    t.reset(Ꮡtyp, (uint16)capacity);
    return t;
}

// reset resets the table with new, empty groups with the specified new total
// capacity.
[GoRecv] internal static void reset(this ref table t, ж<abi.SwissMapType> Ꮡtyp, uint16 capacity) {
    var groupCount = (uint64)capacity / (uint64)abi.SwissMapGroupSlots;
    t.groups = newGroups(ref (Ꮡtyp).DerefOrNull(), groupCount);
    t.capacity = capacity;
    t.resetGrowthLeft();
    for (var i = (uint64)0; i <= t.groups.lengthMask; i++) {
        var g = t.groups.group(Ꮡtyp, i);
        g.ctrls().setEmpty();
    }
}

// Preconditions: table must be empty.
[GoRecv] internal static void resetGrowthLeft(this ref table t) {
    uint16 growthLeft = default!;
    if (t.capacity == 0){
        // No real reason to support zero capacity table, since an
        // empty Map simply won't have a table.
        throw panic("table must have positive capacity");
    } else 
    if (t.capacity <= abi.SwissMapGroupSlots){
        // If the map fits in a single group then we're able to fill all of
        // the slots except 1 (an empty slot is needed to terminate find
        // operations).
        //
        // TODO(go.dev/issue/54766): With a special case in probing for
        // single-group tables, we could fill all slots.
        growthLeft = (uint16)(t.capacity - 1);
    } else {
        if ((uint16)(t.capacity * (uint16)maxAvgGroupLoad) < t.capacity) {
            // TODO(prattmic): Do something cleaner.
            throw panic("overflow");
        }
        growthLeft = (uint16)((t.capacity * (uint16)maxAvgGroupLoad) / (uint16)abi.SwissMapGroupSlots);
    }
    t.growthLeft = growthLeft;
}

[GoRecv] public static uint64 Used(this ref table t) {
    return (uint64)t.used;
}

// Get performs a lookup of the key that key points to. It returns a pointer to
// the element, or false if the key doesn't exist.
[GoRecv] public static (@unsafe.Pointer, bool) Get(this ref table t, ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm, @unsafe.Pointer key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    // TODO(prattmic): We could avoid hashing in a variety of special
    // cases.
    //
    // - One entry maps could just directly compare the single entry
    //   without hashing.
    // - String keys could do quick checks of a few bytes before hashing.
    var hash = typ.Hasher(key, m.seed);
    var (_, elem, ok) = t.getWithKey(Ꮡtyp, hash, key);
    return (elem, ok);
}

// getWithKey performs a lookup of key, returning a pointer to the version of
// the key in the map in addition to the element.
//
// This is relevant when multiple different key values compare equal (e.g.,
// +0.0 and -0.0). When a grow occurs during iteration, iteration perform a
// lookup of keys from the old group in the new group in order to correctly
// expose updated elements. For NeedsKeyUpdate keys, iteration also must return
// the new key value, not the old key value.
// hash must be the hash of the key.
[GoRecv] internal static (@unsafe.Pointer, @unsafe.Pointer, bool) getWithKey(this ref table t, ж<abi.SwissMapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    // To find the location of a key in the table, we compute hash(key). From
    // h1(hash(key)) and the capacity, we construct a probeSeq that visits
    // every group of slots in some interesting order. See [probeSeq].
    //
    // We walk through these indices. At each index, we select the entire
    // group starting with that index and extract potential candidates:
    // occupied slots with a control byte equal to h2(hash(key)). The key
    // at candidate slot i is compared with key; if key == g.slot(i).key
    // we are done and return the slot; if there is an empty slot in the
    // group, we stop and return an error; otherwise we continue to the
    // next probe index. Tombstones (ctrlDeleted) effectively behave like
    // full slots that never match the value we're looking for.
    //
    // The h2 bits ensure when we compare a key we are likely to have
    // actually found the object. That is, the chance is low that keys
    // compare false. Thus, when we search for an object, we are unlikely
    // to call Equal many times. This likelihood can be analyzed as follows
    // (assuming that h2 is a random enough hash function).
    //
    // Let's assume that there are k "wrong" objects that must be examined
    // in a probe sequence. For example, when doing a find on an object
    // that is in the table, k is the number of objects between the start
    // of the probe sequence and the final found object (not including the
    // final found object). The expected number of objects with an h2 match
    // is then k/128. Measurements and analysis indicate that even at high
    // load factors, k is less than 32, meaning that the number of false
    // positive comparisons we must perform is less than 1/8 per find.
    var seq = makeProbeSeq(h1(hash), t.groups.lengthMask);
    for (; ᐧ ; seq = seq.next()) {
        var g = t.groups.group(Ꮡtyp, seq.offset);
        var match = (~g.ctrls()).matchH2(h2(hash));
        while (match != 0) {
            var i = match.first();
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
            match = match.removeFirst();
        }
        match = (~g.ctrls()).matchEmpty();
        if (match != 0) {
            // Finding an empty slot means we've reached the end of
            // the probe sequence.
            return (default!, default!, false);
        }
    }
}

[GoRecv] internal static (@unsafe.Pointer, bool) getWithoutKey(this ref table t, ж<abi.SwissMapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    var seq = makeProbeSeq(h1(hash), t.groups.lengthMask);
    for (; ᐧ ; seq = seq.next()) {
        var g = t.groups.group(Ꮡtyp, seq.offset);
        var match = (~g.ctrls()).matchH2(h2(hash));
        while (match != 0) {
            var i = match.first();
            @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, i);
            if (typ.IndirectKey()) {
                slotKey = ((ж<@unsafe.Pointer>)(uintptr)(slotKey)).Value;
            }
            if ((~typ.Key).Equal(key, slotKey)) {
                @unsafe.Pointer slotElem = (uintptr)g.elem(Ꮡtyp, i);
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
            return (default!, false);
        }
    }
}

// PutSlot returns a pointer to the element slot where an inserted element
// should be written, and ok if it returned a valid slot.
//
// PutSlot returns ok false if the table was split and the Map needs to find
// the new table.
//
// hash must be the hash of key.
public static (@unsafe.Pointer, bool) PutSlot(this ж<table> Ꮡt, ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm, uintptr hash, @unsafe.Pointer key) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    var seq = makeProbeSeq(h1(hash), t.groups.lengthMask);
    // As we look for a match, keep track of the first deleted slot we
    // find, which we'll use to insert the new entry if necessary.
    groupReference firstDeletedGroup = default!;
    uintptr firstDeletedSlot = default!;
    for (; ᐧ ; seq = seq.next()) {
        var g = t.groups.group(Ꮡtyp, seq.offset);
        var match = (~g.ctrls()).matchH2(h2(hash));
        // Look for an existing slot containing this key.
        while (match != 0) {
            var iΔ1 = match.first();
            @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, iΔ1);
            if (typ.IndirectKey()) {
                slotKey = ((ж<@unsafe.Pointer>)(uintptr)(slotKey)).Value;
            }
            if ((~typ.Key).Equal(key, slotKey)) {
                if (typ.NeedKeyUpdate()) {
                    typedmemmove(typ.Key, slotKey, key);
                }
                @unsafe.Pointer slotElem = (uintptr)g.elem(Ꮡtyp, iΔ1);
                if (typ.IndirectElem()) {
                    slotElem = ((ж<@unsafe.Pointer>)(uintptr)(slotElem)).Value;
                }
                t.checkInvariants(Ꮡtyp, Ꮡm);
                return (slotElem, true);
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
            t.growthLeft++; // will be decremented below to become a no-op.
        }
        // If there is room left to grow, just insert the new entry.
        if (t.growthLeft > 0) {
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
            t.growthLeft--;
            t.used++;
            m.used++;
            t.checkInvariants(Ꮡtyp, Ꮡm);
            return (slotElem, true);
        }
        Ꮡt.rehash(Ꮡtyp, Ꮡm);
        return (default!, false);
    }
}

// uncheckedPutSlot inserts an entry known not to be in the table.
// This is used for grow/split where we are making a new table from
// entries in an existing table.
//
// Decrements growthLeft and increments used.
//
// Requires that the entry does not exist in the table, and that the table has
// room for another element without rehashing.
//
// Requires that there are no deleted entries in the table.
//
// For indirect keys and/or elements, the key and elem pointers can be
// put directly into the map, they do not need to be copied. This
// requires the caller to ensure that the referenced memory never
// changes (by sourcing those pointers from another indirect key/elem
// map).
[GoRecv] internal static void uncheckedPutSlot(this ref table t, ж<abi.SwissMapType> Ꮡtyp, uintptr hash, @unsafe.Pointer key, @unsafe.Pointer elem) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (t.growthLeft == 0) {
        throw panic("invariant failed: growthLeft is unexpectedly 0");
    }
    // Given key and its hash hash(key), to insert it, we construct a
    // probeSeq, and use it to find the first group with an unoccupied (empty
    // or deleted) slot. We place the key/value into the first such slot in
    // the group and mark it as full with key's H2.
    var seq = makeProbeSeq(h1(hash), t.groups.lengthMask);
    for (; ᐧ ; seq = seq.next()) {
        var g = t.groups.group(Ꮡtyp, seq.offset);
        var match = (~g.ctrls()).matchEmptyOrDeleted();
        if (match != 0) {
            var i = match.first();
            @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, i);
            if (typ.IndirectKey()){
                ((ж<@unsafe.Pointer>)(uintptr)(slotKey)).Value = key;
            } else {
                typedmemmove(typ.Key, slotKey, key);
            }
            @unsafe.Pointer slotElem = (uintptr)g.elem(Ꮡtyp, i);
            if (typ.IndirectElem()){
                ((ж<@unsafe.Pointer>)(uintptr)(slotElem)).Value = elem;
            } else {
                typedmemmove(typ.Elem, slotElem, elem);
            }
            t.growthLeft--;
            t.used++;
            g.ctrls().set(i, ((ctrl)(uint8)h2(hash)));
            return;
        }
    }
}

[GoRecv] public static void Delete(this ref table t, ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm, uintptr hash, @unsafe.Pointer key) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    var seq = makeProbeSeq(h1(hash), t.groups.lengthMask);
    for (; ᐧ ; seq = seq.next()) {
        var g = t.groups.group(Ꮡtyp, seq.offset);
        var match = (~g.ctrls()).matchH2(h2(hash));
        while (match != 0) {
            var i = match.first();
            @unsafe.Pointer slotKey = (uintptr)g.key(Ꮡtyp, i);
            @unsafe.Pointer origSlotKey = slotKey;
            if (typ.IndirectKey()) {
                slotKey = ((ж<@unsafe.Pointer>)(uintptr)(slotKey)).Value;
            }
            if ((~typ.Key).Equal(key, slotKey)) {
                t.used--;
                m.used--;
                if (typ.IndirectKey()){
                    // Clearing the pointer is sufficient.
                    ((ж<@unsafe.Pointer>)(uintptr)(origSlotKey)).Value = default!;
                } else 
                if (typ.Key.Pointers()) {
                    // Only bothing clear the key if there
                    // are pointers in it.
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
                // Only a full group can appear in the middle
                // of a probe sequence (a group with at least
                // one empty slot terminates probing). Once a
                // group becomes full, it stays full until
                // rehashing/resizing. So if the group isn't
                // full now, we can simply remove the element.
                // Otherwise, we create a tombstone to mark the
                // slot as deleted.
                if ((~g.ctrls()).matchEmpty() != 0){
                    g.ctrls().set(i, ctrlEmpty);
                    t.growthLeft++;
                } else {
                    g.ctrls().set(i, ctrlDeleted);
                }
                t.checkInvariants(Ꮡtyp, Ꮡm);
                return;
            }
            match = match.removeFirst();
        }
        match = (~g.ctrls()).matchEmpty();
        if (match != 0) {
            // Finding an empty slot means we've reached the end of
            // the probe sequence.
            return;
        }
    }
}

// tombstones returns the number of deleted (tombstone) entries in the table. A
// tombstone is a slot that has been deleted but is still considered occupied
// so as not to violate the probing invariant.
[GoRecv] internal static uint16 tombstones(this ref table t) {
    return (uint16)((t.capacity * (uint16)maxAvgGroupLoad) / (uint16)abi.SwissMapGroupSlots - t.used - t.growthLeft);
}

// Clear deletes all entries from the map resulting in an empty map.
[GoRecv] public static void Clear(this ref table t, ж<abi.SwissMapType> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    for (var i = (uint64)0; i <= t.groups.lengthMask; i++) {
        var g = t.groups.group(Ꮡtyp, i);
        typedmemclr(typ.Group, g.data);
        g.ctrls().setEmpty();
    }
    t.used = 0;
    t.resetGrowthLeft();
}

[GoType] partial struct Iter {
    internal @unsafe.Pointer key; // Must be in first position.  Write nil to indicate iteration end (see cmd/compile/internal/walk/range.go).
    internal @unsafe.Pointer elem; // Must be in second position (see cmd/compile/internal/walk/range.go).
    internal ж<abi.SwissMapType> typ;
    internal ж<ΔMap> m;
    // Randomize iteration order by starting iteration at a random slot
    // offset. The offset into the directory uses a separate offset, as it
    // must adjust when the directory grows.
    internal uint64 entryOffset;
    internal uint64 dirOffset;
    // Snapshot of Map.clearSeq at iteration initialization time. Used to
    // detect clear during iteration.
    internal uint64 clearSeq;
    // Value of Map.globalDepth during the last call to Next. Used to
    // detect directory grow during iteration.
    internal uint8 globalDepth;
    // dirIdx is the current directory index, prior to adjustment by
    // dirOffset.
    internal nint dirIdx;
    // tab is the table at dirIdx during the previous call to Next.
    internal ж<table> tab;
    // group is the group at entryIdx during the previous call to Next.
    internal groupReference group;
    // entryIdx is the current entry index, prior to adjustment by entryOffset.
    // The lower 3 bits of the index are the slot index, and the upper bits
    // are the group index.
    internal uint64 entryIdx;
}

// Init initializes Iter for iteration.
[GoRecv] public static void Init(this ref Iter it, ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    it.typ = Ꮡtyp;
    if (Ꮡm == nil || m.used == 0) {
        return;
    }
    nint dirIdx = 0;
    groupReference groupSmall = default!;
    if (m.dirLen <= 0) {
        // Use dirIdx == -1 as sentinel for small maps.
        dirIdx = -1;
        groupSmall.data = m.dirPtr;
    }
    it.m = Ꮡm;
    it.entryOffset = rand();
    it.dirOffset = rand();
    it.globalDepth = m.globalDepth;
    it.dirIdx = dirIdx;
    it.group = groupSmall;
    it.clearSeq = m.clearSeq;
}

[GoRecv] public static bool Initialized(this ref Iter it) {
    return it.typ != nil;
}

// Map returns the map this iterator is iterating over.
[GoRecv] public static ж<ΔMap> Map(this ref Iter it) {
    return it.m;
}

// Key returns a pointer to the current key. nil indicates end of iteration.
//
// Must not be called prior to Next.
[GoRecv] public static @unsafe.Pointer Key(this ref Iter it) {
    return it.key;
}

// Key returns a pointer to the current element. nil indicates end of
// iteration.
//
// Must not be called prior to Next.
[GoRecv] public static @unsafe.Pointer Elem(this ref Iter it) {
    return it.elem;
}

[GoRecv] internal static void nextDirIdx(this ref Iter it) {
    // Skip other entries in the directory that refer to the same
    // logical table. There are two cases of this:
    //
    // Consider this directory:
    //
    // - 0: *t1
    // - 1: *t1
    // - 2: *t2a
    // - 3: *t2b
    //
    // At some point, the directory grew to accommodate a split of
    // t2. t1 did not split, so entries 0 and 1 both point to t1.
    // t2 did split, so the two halves were installed in entries 2
    // and 3.
    //
    // If dirIdx is 0 and it.tab is t1, then we should skip past
    // entry 1 to avoid repeating t1.
    //
    // If dirIdx is 2 and it.tab is t2 (pre-split), then we should
    // skip past entry 3 because our pre-split t2 already covers
    // all keys from t2a and t2b (except for new insertions, which
    // iteration need not return).
    //
    // We can achieve both of these by using to difference between
    // the directory and table depth to compute how many entries
    // the table covers.
    nint entries = ((nint)1).Lsh((uint64)(((~it.m).globalDepth - (~it.tab).localDepth)));
    it.dirIdx += entries;
    it.tab = default!;
    it.group = new groupReference(nil);
    it.entryIdx = 0;
}

// Return the appropriate key/elem for key at slotIdx index within it.group, if
// any.
[GoRecv] internal static (@unsafe.Pointer, @unsafe.Pointer, bool) grownKeyElem(this ref Iter it, @unsafe.Pointer key, uintptr slotIdx) {
    var (newKey, newElem, ok) = it.m.getWithKey(it.typ, key);
    if (!ok) {
        // Key has likely been deleted, and
        // should be skipped.
        //
        // One exception is keys that don't
        // compare equal to themselves (e.g.,
        // NaN). These keys cannot be looked
        // up, so getWithKey will fail even if
        // the key exists.
        //
        // However, we are in luck because such
        // keys cannot be updated and they
        // cannot be deleted except with clear.
        // Thus if no clear has occurred, the
        // key/elem must still exist exactly as
        // in the old groups, so we can return
        // them from there.
        //
        // TODO(prattmic): Consider checking
        // clearSeq early. If a clear occurred,
        // Next could always return
        // immediately, as iteration doesn't
        // need to return anything added after
        // clear.
        if (it.clearSeq == (~it.m).clearSeq && !(~(~it.typ).Key).Equal(key, key)) {
            @unsafe.Pointer elem = (uintptr)it.group.elem(it.typ, slotIdx);
            if (it.typ.IndirectElem()) {
                elem = ((ж<@unsafe.Pointer>)(uintptr)(elem)).Value;
            }
            return (key, elem, true);
        }
        // This entry doesn't exist anymore.
        return (default!, default!, false);
    }
    return (newKey, newElem, true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string concurrentMapIterationˢ = "concurrent map iteration and map write"u8;

// Next proceeds to the next element in iteration, which can be accessed via
// the Key and Elem methods.
//
// The table can be mutated during iteration, though there is no guarantee that
// the mutations will be visible to the iteration.
//
// Init must be called prior to Next.
[GoRecv] public static void Next(this ref Iter it) {
    if (it.m == nil) {
        // Map was empty at Iter.Init.
        it.key = default!;
        it.elem = default!;
        return;
    }
    if ((~it.m).writing != 0) {
        fatal(concurrentMapIterationˢ);
        return;
    }
    if (it.dirIdx < 0) {
        // Map was small at Init.
        for (; it.entryIdx < abi.SwissMapGroupSlots; it.entryIdx++) {
            var k = (uintptr)(it.entryIdx + it.entryOffset) % (uintptr)abi.SwissMapGroupSlots;
            if (((ctrl)(it.group.ctrls().get(k) & ctrlEmpty)) == ctrlEmpty) {
                // Empty or deleted.
                continue;
            }
            @unsafe.Pointer key = (uintptr)it.group.key(it.typ, k);
            if (it.typ.IndirectKey()) {
                key = ((ж<@unsafe.Pointer>)(uintptr)(key)).Value;
            }
            // As below, if we have grown to a full map since Init,
            // we continue to use the old group to decide the keys
            // to return, but must look them up again in the new
            // tables.
            var grown = (~it.m).dirLen > 0;
            @unsafe.Pointer elem = default!;
            if (grown){
                bool ok = default!;
                (var newKey, var newElem, ok) = it.m.getWithKey(it.typ, key);
                if (!ok){
                    // See comment below.
                    if (it.clearSeq == (~it.m).clearSeq && !(~(~it.typ).Key).Equal(key, key)){
                        elem = (uintptr)it.group.elem(it.typ, k);
                        if (it.typ.IndirectElem()) {
                            elem = ((ж<@unsafe.Pointer>)(uintptr)(elem)).Value;
                        }
                    } else {
                        continue;
                    }
                } else {
                    key = newKey;
                    elem = newElem;
                }
            } else {
                elem = (uintptr)it.group.elem(it.typ, k);
                if (it.typ.IndirectElem()) {
                    elem = ((ж<@unsafe.Pointer>)(uintptr)(elem)).Value;
                }
            }
            it.entryIdx++;
            it.key = key;
            it.elem = elem;
            return;
        }
        it.key = default!;
        it.elem = default!;
        return;
    }
    if (it.globalDepth != (~it.m).globalDepth) {
        // Directory has grown since the last call to Next. Adjust our
        // directory index.
        //
        // Consider:
        //
        // Before:
        // - 0: *t1
        // - 1: *t2  <- dirIdx
        //
        // After:
        // - 0: *t1a (split)
        // - 1: *t1b (split)
        // - 2: *t2  <- dirIdx
        // - 3: *t2
        //
        // That is, we want to double the current index when the
        // directory size doubles (or quadruple when the directory size
        // quadruples, etc).
        //
        // The actual (randomized) dirIdx is computed below as:
        //
        // dirIdx := (it.dirIdx + it.dirOffset) % it.m.dirLen
        //
        // Multiplication is associative across modulo operations,
        // A * (B % C) = (A * B) % (A * C),
        // provided that A is positive.
        //
        // Thus we can achieve this by adjusting it.dirIdx,
        // it.dirOffset, and it.m.dirLen individually.
        var orders = (uint8)((~it.m).globalDepth - it.globalDepth);
        it.dirIdx.LshAssign((uint64)(orders));
        it.dirOffset.LshAssign((uint64)(orders));
        // it.m.dirLen was already adjusted when the directory grew.
        it.globalDepth = it.m.Value.globalDepth;
    }
    // Continue iteration until we find a full slot.
    for (; it.dirIdx < (~it.m).dirLen; it.nextDirIdx()) {
        // Resolve the table.
        if (it.tab == nil) {
            nint dirIdx = (nint)((uint64)(((uint64)it.dirIdx + it.dirOffset) & (uint64)((~it.m).dirLen - 1)));
            var newTab = it.m.directoryAt((uintptr)dirIdx);
            if ((~newTab).index != dirIdx) {
                // Normally we skip past all duplicates of the
                // same entry in the table (see updates to
                // it.dirIdx at the end of the loop below), so
                // this case wouldn't occur.
                //
                // But on the very first call, we have a
                // completely randomized dirIdx that may refer
                // to a middle of a run of tables in the
                // directory. Do a one-time adjustment of the
                // offset to ensure we start at first index for
                // newTable.
                nint diff = dirIdx - (~newTab).index;
                it.dirOffset -= (uint64)diff;
                dirIdx = newTab.Value.index;
            }
            it.tab = newTab;
        }
        // N.B. Use it.tab, not newTab. It is important to use the old
        // table for key selection if the table has grown. See comment
        // on grown below.
        var entryMask = (uint64)(~it.tab).capacity - 1;
        if (it.entryIdx > entryMask) {
            // Continue to next table.
            continue;
        }
        // Fast path: skip matching and directly check if entryIdx is a
        // full slot.
        //
        // In the slow path below, we perform an 8-slot match check to
        // look for full slots within the group.
        //
        // However, with a max load factor of 7/8, each slot in a
        // mostly full map has a high probability of being full. Thus
        // it is cheaper to check a single slot than do a full control
        // match.
        var entryIdx = (uint64)((it.entryIdx + it.entryOffset) & entryMask);
        var slotIdx = (uintptr)((uint64)(entryIdx & (uint64)((abi.SwissMapGroupSlots - 1))));
        if (slotIdx == 0 || it.group.data == nil) {
            // Only compute the group (a) when we switch
            // groups (slotIdx rolls over) and (b) on the
            // first iteration in this table (slotIdx may
            // not be zero due to entryOffset).
            var groupIdx = (entryIdx >> (int)(abi.SwissMapGroupSlotsBits));
            it.group = it.tab.of(table.Ꮡgroups).group(it.typ, groupIdx);
        }
        if (((ctrl)(it.group.ctrls().get(slotIdx) & ctrlEmpty)) == 0) {
            // Slot full.
            @unsafe.Pointer key = (uintptr)it.group.key(it.typ, slotIdx);
            if (it.typ.IndirectKey()) {
                key = ((ж<@unsafe.Pointer>)(uintptr)(key)).Value;
            }
            var grown = (~it.tab).index == -1;
            @unsafe.Pointer elem = default!;
            if (grown){
                var (newKey, newElem, ok) = it.grownKeyElem(key, slotIdx);
                if (!ok){
                    // This entry doesn't exist
                    // anymore. Continue to the
                    // next one.
                    goto next;
                } else {
                    key = newKey;
                    elem = newElem;
                }
            } else {
                elem = (uintptr)it.group.elem(it.typ, slotIdx);
                if (it.typ.IndirectElem()) {
                    elem = ((ж<@unsafe.Pointer>)(uintptr)(elem)).Value;
                }
            }
            it.entryIdx++;
            it.key = key;
            it.elem = elem;
            return;
        }
next:
        it.entryIdx++;
        // Slow path: use a match on the control word to jump ahead to
        // the next full slot.
        //
        // This is highly effective for maps with particularly low load
        // (e.g., map allocated with large hint but few insertions).
        //
        // For maps with medium load (e.g., 3-4 empty slots per group)
        // it also tends to work pretty well. Since slots within a
        // group are filled in order, then if there have been no
        // deletions, a match will allow skipping past all empty slots
        // at once.
        //
        // Note: it is tempting to cache the group match result in the
        // iterator to use across Next calls. However because entries
        // may be deleted between calls later calls would still need to
        // double-check the control value.
        bitset groupMatch = default!;
        while (it.entryIdx <= entryMask) {
            var entryIdxΔ1 = (uint64)((it.entryIdx + it.entryOffset) & entryMask);
            var slotIdxΔ1 = (uintptr)((uint64)(entryIdxΔ1 & (uint64)((abi.SwissMapGroupSlots - 1))));
            if (slotIdxΔ1 == 0 || it.group.data == nil) {
                // Only compute the group (a) when we switch
                // groups (slotIdx rolls over) and (b) on the
                // first iteration in this table (slotIdx may
                // not be zero due to entryOffset).
                var groupIdx = (entryIdxΔ1 >> (int)(abi.SwissMapGroupSlotsBits));
                it.group = it.tab.of(table.Ꮡgroups).group(it.typ, groupIdx);
            }
            if (groupMatch == 0) {
                groupMatch = (~it.group.ctrls()).matchFull();
                if (slotIdxΔ1 != 0) {
                    // Starting in the middle of the group.
                    // Ignore earlier groups.
                    groupMatch = groupMatch.removeBelow(slotIdxΔ1);
                }
                // Skip over groups that are composed of only empty or
                // deleted slots.
                if (groupMatch == 0) {
                    // Jump past remaining slots in this
                    // group.
                    it.entryIdx += (uint64)abi.SwissMapGroupSlots - (uint64)slotIdxΔ1;
                    continue;
                }
                var i = groupMatch.first();
                it.entryIdx += (uint64)(i - slotIdxΔ1);
                if (it.entryIdx > entryMask) {
                    // Past the end of this table's iteration.
                    continue;
                }
                entryIdxΔ1 += (uint64)(i - slotIdxΔ1);
                slotIdxΔ1 = i;
            }
            @unsafe.Pointer key = (uintptr)it.group.key(it.typ, slotIdxΔ1);
            if (it.typ.IndirectKey()) {
                key = ((ж<@unsafe.Pointer>)(uintptr)(key)).Value;
            }
            // If the table has changed since the last
            // call, then it has grown or split. In this
            // case, further mutations (changes to
            // key->elem or deletions) will not be visible
            // in our snapshot table. Instead we must
            // consult the new table by doing a full
            // lookup.
            //
            // We still use our old table to decide which
            // keys to lookup in order to avoid returning
            // the same key twice.
            var grown = (~it.tab).index == -1;
            @unsafe.Pointer elem = default!;
            if (grown){
                var (newKey, newElem, ok) = it.grownKeyElem(key, slotIdxΔ1);
                if (!ok){
                    // This entry doesn't exist anymore.
                    // Continue to the next one.
                    groupMatch = groupMatch.removeFirst();
                    if (groupMatch == 0) {
                        // No more entries in this
                        // group. Continue to next
                        // group.
                        it.entryIdx += (uint64)abi.SwissMapGroupSlots - (uint64)slotIdxΔ1;
                        continue;
                    }
                    // Next full slot.
                    var i = groupMatch.first();
                    it.entryIdx += (uint64)(i - slotIdxΔ1);
                    continue;
                } else {
                    key = newKey;
                    elem = newElem;
                }
            } else {
                elem = (uintptr)it.group.elem(it.typ, slotIdxΔ1);
                if (it.typ.IndirectElem()) {
                    elem = ((ж<@unsafe.Pointer>)(uintptr)(elem)).Value;
                }
            }
            // Jump ahead to the next full slot or next group.
            groupMatch = groupMatch.removeFirst();
            if (groupMatch == 0){
                // No more entries in
                // this group. Continue
                // to next group.
                it.entryIdx += (uint64)abi.SwissMapGroupSlots - (uint64)slotIdxΔ1;
            } else {
                // Next full slot.
                var i = groupMatch.first();
                it.entryIdx += (uint64)(i - slotIdxΔ1);
            }
            it.key = key;
            it.elem = elem;
            return;
        }
    }
    // Continue to next table.
    it.key = default!;
    it.elem = default!;
    return;
}

// Replaces the table with one larger table or two split tables to fit more
// entries. Since the table is replaced, t is now stale and should not be
// modified.
internal static void rehash(this ж<table> Ꮡt, ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm) {
    ref var t = ref Ꮡt.DerefOrNull();

    // TODO(prattmic): SwissTables typically perform a "rehash in place"
    // operation which recovers capacity consumed by tombstones without growing
    // the table by reordering slots as necessary to maintain the probe
    // invariant while eliminating all tombstones.
    //
    // However, it is unclear how to make rehash in place work with
    // iteration. Since iteration simply walks through all slots in order
    // (with random start offset), reordering the slots would break
    // iteration.
    //
    // As an alternative, we could do a "resize" to new groups allocation
    // of the same size. This would eliminate the tombstones, but using a
    // new allocation, so the existing grow support in iteration would
    // continue to work.
    var newCapacity = (uint16)(2 * t.capacity);
    if (newCapacity <= maxTableCapacity) {
        t.grow(Ꮡtyp, Ꮡm, newCapacity);
        return;
    }
    Ꮡt.split(Ꮡtyp, Ꮡm);
}

// Bitmask for the last selection bit at this depth.
internal static uintptr localDepthMask(uint8 localDepth) {
    if (goarch.PtrSize == 4) {
        return ((uintptr)1).Lsh((uint64)((32 - localDepth)));
    }
    return ((uintptr)1).Lsh((uint64)((64 - localDepth)));
}

// split the table into two, installing the new tables in the map directory.
internal static void split(this ж<table> Ꮡt, ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    var localDepth = t.localDepth;
    localDepth++;
    // TODO: is this the best capacity?
    var left = newTable(Ꮡtyp, maxTableCapacity, -1, localDepth);
    var right = newTable(Ꮡtyp, maxTableCapacity, -1, localDepth);
    // Split in half at the localDepth bit from the top.
    var mask = localDepthMask(localDepth);
    for (var i = (uint64)0; i <= t.groups.lengthMask; i++) {
        var g = t.groups.group(Ꮡtyp, i);
        for (var j = (uintptr)0; j < abi.SwissMapGroupSlots; j++) {
            if (((ctrl)(g.ctrls().get(j) & ctrlEmpty)) == ctrlEmpty) {
                // Empty or deleted
                continue;
            }
            @unsafe.Pointer key = (uintptr)g.key(Ꮡtyp, j);
            if (typ.IndirectKey()) {
                key = ((ж<@unsafe.Pointer>)(uintptr)(key)).Value;
            }
            @unsafe.Pointer elem = (uintptr)g.elem(Ꮡtyp, j);
            if (typ.IndirectElem()) {
                elem = ((ж<@unsafe.Pointer>)(uintptr)(elem)).Value;
            }
            var hash = typ.Hasher(key, m.seed);
            ж<table> newTableΔ1 = default!;
            if ((uintptr)(hash & mask) == 0){
                newTableΔ1 = left;
            } else {
                newTableΔ1 = right;
            }
            newTableΔ1.uncheckedPutSlot(Ꮡtyp, hash, key, elem);
        }
    }
    m.installTableSplit(Ꮡt, left, right);
    t.index = -1;
}

// grow the capacity of the table by allocating a new table with a bigger array
// and uncheckedPutting each element of the table into the new table (we know
// that no insertion here will Put an already-present value), and discard the
// old table.
[GoRecv] internal static void grow(this ref table t, ж<abi.SwissMapType> Ꮡtyp, ж<ΔMap> Ꮡm, uint16 newCapacity) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var m = ref Ꮡm.DerefOrNull();

    var newTableΔ1 = newTable(Ꮡtyp, (uint64)newCapacity, t.index, t.localDepth);
    if (t.capacity > 0) {
        for (var i = (uint64)0; i <= t.groups.lengthMask; i++) {
            var g = t.groups.group(Ꮡtyp, i);
            for (var j = (uintptr)0; j < abi.SwissMapGroupSlots; j++) {
                if (((ctrl)(g.ctrls().get(j) & ctrlEmpty)) == ctrlEmpty) {
                    // Empty or deleted
                    continue;
                }
                @unsafe.Pointer key = (uintptr)g.key(Ꮡtyp, j);
                if (typ.IndirectKey()) {
                    key = ((ж<@unsafe.Pointer>)(uintptr)(key)).Value;
                }
                @unsafe.Pointer elem = (uintptr)g.elem(Ꮡtyp, j);
                if (typ.IndirectElem()) {
                    elem = ((ж<@unsafe.Pointer>)(uintptr)(elem)).Value;
                }
                var hash = typ.Hasher(key, m.seed);
                newTableΔ1.uncheckedPutSlot(Ꮡtyp, hash, key, elem);
            }
        }
    }
    newTableΔ1.checkInvariants(Ꮡtyp, Ꮡm);
    m.replaceTable(newTableΔ1);
    t.index = -1;
}

// probeSeq maintains the state for a probe sequence that iterates through the
// groups in a table. The sequence is a triangular progression of the form
//
//	p(i) := (i^2 + i)/2 + hash (mod mask+1)
//
// The sequence effectively outputs the indexes of *groups*. The group
// machinery allows us to check an entire group with minimal branching.
//
// It turns out that this probe sequence visits every group exactly once if
// the number of groups is a power of two, since (i^2+i)/2 is a bijection in
// Z/(2^m). See https://en.wikipedia.org/wiki/Quadratic_probing
[GoType] partial struct probeSeq {
    internal uint64 mask;
    internal uint64 offset;
    internal uint64 index;
}

internal static probeSeq makeProbeSeq(uintptr hash, uint64 mask) {
    return new probeSeq(
        mask: mask,
        offset: (uint64)((uint64)hash & mask),
        index: 0
    );
}

internal static probeSeq next(this probeSeq s) {
    s.index++;
    s.offset = (uint64)((s.offset + s.index) & s.mask);
    return s;
}

} // end maps_package
