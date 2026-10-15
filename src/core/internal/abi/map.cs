// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using @unsafe = unsafe_package;

partial class abi_package {

// Map constants common to several packages
// runtime/runtime-gdb.py:MapTypePrinter contains its own copy
// Number of bits in the group.slot count.
public static UntypedInt MapGroupSlotsBits => 3;

// Number of slots in a group.
public static UntypedInt MapGroupSlots => /* 1 << MapGroupSlotsBits */ 8; // 8

// Maximum key or elem size to keep inline (instead of mallocing per element).
// Must fit in a uint8.
public static UntypedInt MapMaxKeyBytes => 128;
public static UntypedInt MapMaxElemBytes => 128;

internal static UntypedInt ctrlEmpty => 0b10000000;
internal static UntypedInt bitsetLSB => 0x0101010101010101;

// Value of control word with all empty slots.
public const uint64 MapCtrlEmpty = /* bitsetLSB * uint64(ctrlEmpty) */ 9259542123273814144;

partial struct ΔMapType {
    public partial ref Type Type { get; }
    public ж<Type> Key;
    public ж<Type> Elem;
    public ж<Type> Group; // internal type representing a slot group
    // function for hashing keys (ptr to key, seed) -> hash
    public Func<@unsafe.Pointer, uintptr, uintptr> Hasher;
    public uintptr GroupSize; // == Group.Size_
    public uintptr SlotSize; // size of key/elem slot
    public uintptr ElemOff; // offset of elem in key/elem slot
    public uint32 Flags;
}

// Flag values
public static UntypedInt MapNeedKeyUpdate => /* 1 << iota */ 1;
public static UntypedInt MapHashMightPanic => 2;
public static UntypedInt MapIndirectKey => 4;
public static UntypedInt MapIndirectElem => 8;

public static bool NeedKeyUpdate(this ref ΔMapType mt) {
    // true if we need to update key on an overwrite
    return (uint32)(mt.Flags & (uint32)MapNeedKeyUpdate) != 0;
}

public static bool HashMightPanic(this ref ΔMapType mt) {
    // true if hash function might panic
    return (uint32)(mt.Flags & (uint32)MapHashMightPanic) != 0;
}

public static bool IndirectKey(this ref ΔMapType mt) {
    // store ptr to key instead of key itself
    return (uint32)(mt.Flags & (uint32)MapIndirectKey) != 0;
}

public static bool IndirectElem(this ref ΔMapType mt) {
    // store ptr to elem instead of elem itself
    return (uint32)(mt.Flags & (uint32)MapIndirectElem) != 0;
}

} // end abi_package
