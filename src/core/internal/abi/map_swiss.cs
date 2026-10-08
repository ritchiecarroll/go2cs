// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using @unsafe = unsafe_package;

partial class abi_package {

// Map constants common to several packages
// runtime/runtime-gdb.py:MapTypePrinter contains its own copy
public static UntypedInt SwissMapGroupSlotsBits => 3;

public static UntypedInt SwissMapGroupSlots => /* 1 << SwissMapGroupSlotsBits */ 8; // 8

public static UntypedInt SwissMapMaxKeyBytes => 128;

public static UntypedInt SwissMapMaxElemBytes => 128;

internal static UntypedInt ctrlEmpty => 0b10000000;

internal static UntypedInt bitsetLSB => 0x0101010101010101;

public const uint64 SwissMapCtrlEmpty = /* bitsetLSB * uint64(ctrlEmpty) */ 9259542123273814144;

partial struct SwissMapType {
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
public static UntypedInt SwissMapNeedKeyUpdate => /* 1 << iota */ 1;

public static UntypedInt SwissMapHashMightPanic => 2;

public static UntypedInt SwissMapIndirectKey => 4;

public static UntypedInt SwissMapIndirectElem => 8;

public static bool NeedKeyUpdate(this ref SwissMapType mt) {
    // true if we need to update key on an overwrite
    return (uint32)(mt.Flags & (uint32)SwissMapNeedKeyUpdate) != 0;
}

public static bool HashMightPanic(this ref SwissMapType mt) {
    // true if hash function might panic
    return (uint32)(mt.Flags & (uint32)SwissMapHashMightPanic) != 0;
}

public static bool IndirectKey(this ref SwissMapType mt) {
    // store ptr to key instead of key itself
    return (uint32)(mt.Flags & (uint32)SwissMapIndirectKey) != 0;
}

public static bool IndirectElem(this ref SwissMapType mt) {
    // store ptr to elem instead of elem itself
    return (uint32)(mt.Flags & (uint32)SwissMapIndirectElem) != 0;
}

} // end abi_package
