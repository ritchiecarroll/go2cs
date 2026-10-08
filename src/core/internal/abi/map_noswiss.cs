// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using @unsafe = unsafe_package;

partial class abi_package {

// Map constants common to several packages
// runtime/runtime-gdb.py:MapTypePrinter contains its own copy
public static UntypedInt OldMapBucketCountBits => 3; // log2 of number of elements in a bucket.

public static UntypedInt OldMapBucketCount => /* 1 << OldMapBucketCountBits */ 8;

public static UntypedInt OldMapMaxKeyBytes => 128;

public static UntypedInt OldMapMaxElemBytes => 128; // Must fit in a uint8.

partial struct OldMapType {
    public partial ref Type Type { get; }
    public ж<Type> Key;
    public ж<Type> Elem;
    public ж<Type> Bucket; // internal type representing a hash bucket
    // function for hashing keys (ptr to key, seed) -> hash
    public Func<@unsafe.Pointer, uintptr, uintptr> Hasher;
    public uint8 KeySize;  // size of key slot
    public uint8 ValueSize;  // size of elem slot
    public uint16 BucketSize; // size of bucket
    public uint32 Flags;
}

// Note: flag values must match those used in the TMAP case
// in ../cmd/compile/internal/reflectdata/reflect.go:writeType.
public static bool IndirectKey(this ref OldMapType mt) {
    // store ptr to key instead of key itself
    return (uint32)(mt.Flags & 1) != 0;
}

public static bool IndirectElem(this ref OldMapType mt) {
    // store ptr to elem instead of elem itself
    return (uint32)(mt.Flags & 2) != 0;
}

public static bool ReflexiveKey(this ref OldMapType mt) {
    // true if k==k for all keys
    return (uint32)(mt.Flags & 4) != 0;
}

public static bool NeedKeyUpdate(this ref OldMapType mt) {
    // true if we need to update key on an overwrite
    return (uint32)(mt.Flags & 8) != 0;
}

public static bool HashMightPanic(this ref OldMapType mt) {
    // true if hash function might panic
    return (uint32)(mt.Flags & 16) != 0;
}

} // end abi_package
