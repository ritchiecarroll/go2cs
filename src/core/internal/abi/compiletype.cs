// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class abi_package {

// These functions are the build-time version of the Go type data structures.
// Their contents must be kept in sync with their definitions.
// Because the host and target type sizes can differ, the compiler and
// linker cannot use the host information that they might get from
// either unsafe.Sizeof and Alignof, nor runtime, reflect, or reflectlite.

// CommonSize returns sizeof(Type) for a compilation target with a given ptrSize
public static nint CommonSize(nint ptrSize) { return 4 * ptrSize + 8 + 8; }

// StructFieldSize returns sizeof(StructField) for a compilation target with a given ptrSize
public static nint StructFieldSize(nint ptrSize) { return 3 * ptrSize; }

// UncommonSize returns sizeof(UncommonType).  This currently does not depend on ptrSize.
// This exported function is in an internal package, so it may change to depend on ptrSize in the future.
public static uint64 UncommonSize() { return 4 + 2 + 2 + 4 + 4; }

// TFlagOff returns the offset of Type.TFlag for a compilation target with a given ptrSize
public static nint TFlagOff(nint ptrSize) { return 2 * ptrSize + 4; }

// ITabTypeOff returns the offset of ITab.Type for a compilation target with a given ptrSize
public static nint ITabTypeOff(nint ptrSize) { return ptrSize; }

} // end abi_package
