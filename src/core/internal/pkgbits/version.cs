// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class pkgbits_package {

// Version indicates a version of a unified IR bitstream.
// Each Version indicates the addition, removal, or change of
// new data in the bitstream.
//
// These are serialized to disk and the interpretation remains fixed.
partial struct ΔVersion /*num:uint32*/;

// V0: initial prototype.
//
// All data that is not assigned a Field is in version V0
// and has not been deprecated.
public static ΔVersion V0 => /* iota */ 0;

// V1: adds the Flags uint32 word
public static ΔVersion V1 => 1;

// V2: removes unused legacy fields and supports type parameters for aliases.
// - remove the legacy "has init" bool from the public root
// - remove obj's "derived func instance" bool
// - add a TypeParamNames field to ObjAlias
// - remove derived info "needed" bool
public static ΔVersion V2 => 2;

internal static UntypedInt numVersions => /* iota */ 3;

// Field denotes a unit of data in the serialized unified IR bitstream.
// It is conceptually a like field in a structure.
//
// We only really need Fields when the data may or may not be present
// in a stream based on the Version of the bitstream.
//
// Unlike much of pkgbits, Fields are not serialized and
// can change values as needed.
partial struct Field /*num:nint*/;

// Flags in a uint32 in the header of a bitstream
// that is used to indicate whether optional features are enabled.
public static Field Flags => /* iota */ 0;

// Deprecated: HasInit was a bool indicating whether a package
// has any init functions.
public static Field HasInit => 1;

// Deprecated: DerivedFuncInstance was a bool indicating
// whether an object was a function instance.
public static Field DerivedFuncInstance => 2;

// ObjAlias has a list of TypeParamNames.
public static Field AliasTypeParamNames => 3;

// Deprecated: DerivedInfoNeeded was a bool indicating
// whether a type was a derived type.
public static Field DerivedInfoNeeded => 4;

internal static UntypedInt numFields => /* iota */ 5;

// introduced is the version a field was added.
internal static array<ΔVersion> introduced = new golib.SparseArray<ΔVersion>{
    [(int)Flags] = V1,
    [(int)AliasTypeParamNames] = V2
}.array(5);

// removed is the version a field was removed in or 0 for fields
// that have not yet been deprecated.
// (So removed[f]-1 is the last version it is included in.)
internal static array<ΔVersion> removed = new golib.SparseArray<ΔVersion>{
    [(int)HasInit] = V2,
    [(int)DerivedFuncInstance] = V2,
    [(int)DerivedInfoNeeded] = V2
}.array(5);

// Has reports whether field f is present in a bitstream at version v.
public static bool Has(this ΔVersion v, Field f) {
    return introduced[f] <= v && (v < removed[f] || removed[f] == V0);
}

} // end pkgbits_package
