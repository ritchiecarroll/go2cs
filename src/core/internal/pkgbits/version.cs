// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class pkgbits_package {

partial struct ΔVersion /*num:uint32*/;

public static ΔVersion V0 => /* iota */ 0;
public static ΔVersion V1 => 1;
public static ΔVersion V2 => 2;
internal static UntypedInt numVersions => /* iota */ 3;

partial struct Field /*num:nint*/;

public static Field Flags => /* iota */ 0;
public static Field HasInit => 1;
public static Field DerivedFuncInstance => 2;
public static Field AliasTypeParamNames => 3;
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
