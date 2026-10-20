// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using abi = go.@internal.abi_package;
using goarch = go.@internal.goarch_package;
using sys = go.@internal.runtime.sys_package;
using @unsafe = unsafe_package;
using go.@internal;
using go.@internal.runtime;

partial class maps_package {

// Maximum load factor prior to growing.
//
// 7/8 is the same load factor used by Abseil, but Abseil defaults to
// 16 slots per group, so they get two empty slots vs our one empty
// slot. We may want to reevaluate if this is best for us.
internal static UntypedInt maxAvgGroupLoad => 7;

internal static ctrl ctrlEmpty => 0b10000000;
internal static ctrl ctrlDeleted => 0b11111110;

internal static UntypedInt bitsetLSB => 0x0101010101010101;
internal static UntypedInt bitsetMSB => 0x8080808080808080;
internal const uint64 bitsetEmpty = /* bitsetLSB * uint64(ctrlEmpty) */ 9259542123273814144;

// bitset represents a set of slots within a group.
//
// The underlying representation depends on GOARCH.
//
// On AMD64, bitset uses one bit per slot, where the bit is set if the slot is
// part of the set. All of the ctrlGroup.match* methods are replaced with
// intrinsics that return this packed representation.
//
// On other architectures, bitset uses one byte per slot, where each byte is
// either 0x80 if the slot is part of the set or 0x00 otherwise. This makes it
// convenient to calculate for an entire group at once using standard
// arithmetic instructions.
partial struct bitset /*num:uint64*/;

// first returns the relative index of the first control byte in the group that
// is in the set.
//
// Preconditions: b is not 0 (empty).
internal static uintptr first(this bitset b) {
    return bitsetFirst(b);
}

// Portable implementation of first.
//
// On AMD64, this is replaced with an intrisic that simply does
// TrailingZeros64. There is no need to shift as the bitset is packed.
internal static uintptr bitsetFirst(bitset b) {
    return (uintptr)sys.TrailingZeros64((uint64)b) >> 3;
}

// removeFirst clears the first set bit (that is, resets the least significant
// set bit to 0).
internal static bitset removeFirst(this bitset b) {
    return (bitset)(b & (b - 1));
}

// removeBelow clears all set bits below slot i (non-inclusive).
internal static bitset removeBelow(this bitset b, uintptr i) {
    return bitsetRemoveBelow(b, i);
}

// Portable implementation of removeBelow.
//
// On AMD64, this is replaced with an intrisic that clears the lower i bits.
internal static bitset bitsetRemoveBelow(bitset b, uintptr i) {
    // Clear all bits below slot i's byte.
    var mask = (((uint64)1).Lsh((8 * (uint64)i))) - 1;
    return (bitset)(b & ~((bitset)mask));
}

// lowestSet returns true if the bit is set for the lowest index in the bitset.
//
// This is intended for use with shiftOutLowest to loop over all entries in the
// bitset regardless of whether they are set.
internal static bool lowestSet(this bitset b) {
    return bitsetLowestSet(b);
}

// Portable implementation of lowestSet.
//
// On AMD64, this is replaced with an intrisic that checks the lowest bit.
internal static bool bitsetLowestSet(bitset b) {
    return (bitset)(b & ((bitset)((uint64)1 << 7))) != 0;
}

// shiftOutLowest shifts the lowest entry out of the bitset. Afterwards, the
// lowest entry in the bitset corresponds to the next slot.
internal static bitset shiftOutLowest(this bitset b) {
    return bitsetShiftOutLowest(b);
}

// Portable implementation of shiftOutLowest.
//
// On AMD64, this is replaced with an intrisic that shifts a single bit.
internal static bitset bitsetShiftOutLowest(bitset b) {
    return b >> 8;
}

// count returns the number of bits set in b.
internal static nint count(this bitset b) {
    // Note: works for both bitset representations (AMD64 and generic)
    return sys.OnesCount64((uint64)b);
}

// Each slot in the hash table has a control byte which can have one of three
// states: empty, deleted, and full. They have the following bit patterns:
//
//	  empty: 1 0 0 0 0 0 0 0
//	deleted: 1 1 1 1 1 1 1 0
//	   full: 0 h h h h h h h  // h represents the H2 hash bits
//
// TODO(prattmic): Consider inverting the top bit so that the zero value is empty.
partial struct ctrl /*num:uint8*/;

// ctrlGroup is a fixed size array of abi.MapGroupSlots control bytes
// stored in a uint64.
partial struct ctrlGroup /*num:uint64*/;

// get returns the i-th control byte.
internal static ctrl get(this ж<ctrlGroup> Ꮡg, uintptr i) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (goarch.BigEndian) {
        return ~(ж<ctrl>)(uintptr)(@unsafe.Add((uintptr)@unsafe.Pointer.FromRef(ref g), 7 - i));
    }
    return ~(ж<ctrl>)(uintptr)(@unsafe.Add((uintptr)@unsafe.Pointer.FromRef(ref g), i));
}

// set sets the i-th control byte.
internal static void set(this ж<ctrlGroup> Ꮡg, uintptr i, ctrl c) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (goarch.BigEndian) {
        ((ж<ctrl>)(uintptr)(@unsafe.Add((uintptr)@unsafe.Pointer.FromRef(ref g), 7 - i))).Value = c;
        return;
    }
    ((ж<ctrl>)(uintptr)(@unsafe.Add((uintptr)@unsafe.Pointer.FromRef(ref g), i))).Value = c;
}

// setEmpty sets all the control bytes to empty.
internal static void setEmpty(this ref ctrlGroup g) {
    g = (ctrlGroup)bitsetEmpty;
}

// matchH2 returns the set of slots which are full and for which the 7-bit hash
// matches the given value. May return false positives.
internal static bitset matchH2(this ctrlGroup g, uintptr h) {
    return ctrlGroupMatchH2(g, h);
}

// Portable implementation of matchH2.
//
// Note: On AMD64, this is an intrinsic implemented with SIMD instructions. See
// note on bitset about the packed intrinsified return value.
internal static bitset ctrlGroupMatchH2(ctrlGroup g, uintptr h) {
    // NB: This generic matching routine produces false positive matches when
    // h is 2^N and the control bytes have a seq of 2^N followed by 2^N+1. For
    // example: if ctrls==0x0302 and h=02, we'll compute v as 0x0100. When we
    // subtract off 0x0101 the first 2 bytes we'll become 0xffff and both be
    // considered matches of h. The false positive matches are not a problem,
    // just a rare inefficiency. Note that they only occur if there is a real
    // match and never occur on ctrlEmpty, or ctrlDeleted. The subsequent key
    // comparisons ensure that there is no correctness issue.
    var v = (uint64)g ^ ((uint64)bitsetLSB * (uint64)h);
    return (bitset)(((v - (uint64)bitsetLSB) & ~v) & (uint64)bitsetMSB);
}

// matchEmpty returns the set of slots in the group that are empty.
internal static bitset matchEmpty(this ctrlGroup g) {
    return ctrlGroupMatchEmpty(g);
}

// Portable implementation of matchEmpty.
//
// Note: On AMD64, this is an intrinsic implemented with SIMD instructions. See
// note on bitset about the packed intrinsified return value.
internal static bitset ctrlGroupMatchEmpty(ctrlGroup g) {
    // An empty slot is   1000 0000
    // A deleted slot is  1111 1110
    // A full slot is     0??? ????
    //
    // A slot is empty iff bit 7 is set and bit 1 is not. We could select any
    // of the other bits here (e.g. v << 1 would also work).
    var v = (uint64)g;
    return (bitset)((v & ~(v << 6)) & (uint64)bitsetMSB);
}

// matchEmptyOrDeleted returns the set of slots in the group that are empty or
// deleted.
internal static bitset matchEmptyOrDeleted(this ctrlGroup g) {
    return ctrlGroupMatchEmptyOrDeleted(g);
}

// Portable implementation of matchEmptyOrDeleted.
//
// Note: On AMD64, this is an intrinsic implemented with SIMD instructions. See
// note on bitset about the packed intrinsified return value.
internal static bitset ctrlGroupMatchEmptyOrDeleted(ctrlGroup g) {
    // An empty slot is  1000 0000
    // A deleted slot is 1111 1110
    // A full slot is    0??? ????
    //
    // A slot is empty or deleted iff bit 7 is set.
    var v = (uint64)g;
    return (bitset)(v & (uint64)bitsetMSB);
}

// matchFull returns the set of slots in the group that are full.
internal static bitset matchFull(this ctrlGroup g) {
    return ctrlGroupMatchFull(g);
}

// anyFull reports whether any slots in the group are full.
internal static bool anyFull(this ctrlGroup g) {
    // A slot is full iff bit 7 is unset. Test whether any slot has bit 7 unset.
    return (ctrlGroup)((~g) & (uint64)bitsetMSB) != 0;
}

// Portable implementation of matchFull.
//
// Note: On AMD64, this is an intrinsic implemented with SIMD instructions. See
// note on bitset about the packed intrinsified return value.
internal static bitset ctrlGroupMatchFull(ctrlGroup g) {
    // An empty slot is  1000 0000
    // A deleted slot is 1111 1110
    // A full slot is    0??? ????
    //
    // A slot is full iff bit 7 is unset.
    var v = (uint64)g;
    return (bitset)(~v & (uint64)bitsetMSB);
}

// groupReference is a wrapper type representing a single slot group stored at
// data.
//
// A group holds abi.MapGroupSlots slots (key/elem pairs) plus their
// control word.
partial struct groupReference {
    // data points to the group, which is described by typ.Group and has
    // layout:
    //
    // type group struct {
    // 	ctrls ctrlGroup
    // 	slots [abi.MapGroupSlots]slot
    // }
    //
    // type slot struct {
    // 	key  typ.Key
    // 	elem typ.Elem
    // }
    internal @unsafe.Pointer data; // data *typ.Group
}

internal static uintptr ctrlGroupsSize => /* unsafe.Sizeof(ctrlGroup(0)) */ 8;
internal static uintptr groupSlotsOffset => /* ctrlGroupsSize */ 8;

// alignUp rounds n up to a multiple of a. a must be a power of 2.
internal static uintptr alignUp(uintptr n, uintptr a) {
    return (uintptr)((n + a - 1) & ~(a - 1));
}

// alignUpPow2 rounds n up to the next power of 2.
//
// Returns true if round up causes overflow.
internal static (uint64, bool) alignUpPow2(uint64 n) {
    if (n == 0) {
        return (0, false);
    }
    var v = (((uint64)1).Lsh((int64)(sys.Len64(n - 1))));
    if (v == 0) {
        return (0, true);
    }
    return (v, false);
}

// ctrls returns the group control word.
internal static ж<ctrlGroup> ctrls(this ref groupReference g) {
    return (ж<ctrlGroup>)(uintptr)(g.data);
}

// key returns a pointer to the key at index i.
internal static @unsafe.Pointer key(this ref groupReference g, ж<abiꓸMapType> Ꮡtyp, uintptr i) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    var offset = groupSlotsOffset + i * typ.SlotSize;

    return (@unsafe.Pointer)((uintptr)g.data + offset);
}

// elem returns a pointer to the element at index i.
internal static @unsafe.Pointer elem(this ref groupReference g, ж<abiꓸMapType> Ꮡtyp, uintptr i) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    var offset = groupSlotsOffset + i * typ.SlotSize + typ.ElemOff;

    return (@unsafe.Pointer)((uintptr)g.data + offset);
}

// groupsReference is a wrapper type describing an array of groups stored at
// data.
partial struct groupsReference {
    // data points to an array of groups. See groupReference above for the
    // definition of group.
    internal @unsafe.Pointer data; // data *[length]typ.Group
    // lengthMask is the number of groups in data minus one (note that
    // length must be a power of two). This allows computing i%length
    // quickly using bitwise AND.
    internal uint64 lengthMask;
}

// newGroups allocates a new array of length groups.
//
// Length must be a power of two.
internal static groupsReference newGroups(ref abiꓸMapType typ, uint64 length) {
    return new groupsReference(
        // TODO: make the length type the same throughout.
        data: (uintptr)newarray(typ.Group, (nint)length),
        lengthMask: length - 1
    );
}

// group returns the group at index i.
internal static groupReference group(this ref groupsReference g, ж<abiꓸMapType> Ꮡtyp, uint64 i) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    // TODO(prattmic): Do something here about truncation on cast to
    // uintptr on 32-bit systems?
    var offset = (uintptr)i * typ.GroupSize;

    return new groupReference(
        data: (@unsafe.Pointer)((uintptr)g.data + offset)
    );
}

internal static void cloneGroup(ж<abiꓸMapType> Ꮡtyp, groupReference newGroup, groupReference oldGroup) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    typedmemmove(typ.Group, newGroup.data, oldGroup.data);
    if (typ.IndirectKey()) {
        // Deep copy keys if indirect.
        for (var i = (uintptr)0; i < abi.MapGroupSlots; i++) {
            @unsafe.Pointer oldKey = ~(ж<@unsafe.Pointer>)(uintptr)((uintptr)oldGroup.key(Ꮡtyp, i));
            if (oldKey == nil) {
                continue;
            }
            @unsafe.Pointer newKey = (uintptr)newobject(typ.Key);
            typedmemmove(typ.Key, newKey, oldKey);
            ((ж<@unsafe.Pointer>)(uintptr)((uintptr)newGroup.key(Ꮡtyp, i))).Value = newKey;
        }
    }
    if (typ.IndirectElem()) {
        // Deep copy elems if indirect.
        for (var i = (uintptr)0; i < abi.MapGroupSlots; i++) {
            @unsafe.Pointer oldElem = ~(ж<@unsafe.Pointer>)(uintptr)((uintptr)oldGroup.elem(Ꮡtyp, i));
            if (oldElem == nil) {
                continue;
            }
            @unsafe.Pointer newElem = (uintptr)newobject(typ.Elem);
            typedmemmove(typ.Elem, newElem, oldElem);
            ((ж<@unsafe.Pointer>)(uintptr)((uintptr)newGroup.elem(Ꮡtyp, i))).Value = newElem;
        }
    }
}

} // end maps_package
