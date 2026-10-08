// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class pkgbits_package {

partial struct RelocKind /*num:int32*/;

partial struct Index /*num:int32*/;

// A relocEnt (relocation entry) is an entry in an element's local
// reference table.
//
// TODO(mdempsky): Rename this too.
partial struct RelocEnt {
    public RelocKind Kind;
    public Index Idx;
}

// Reserved indices within the meta relocation section.
public static Index PublicRootIdx => 0;

public static Index PrivateRootIdx => 1;

public static RelocKind RelocString => /* iota */ 0;
public static RelocKind RelocMeta => 1;
public static RelocKind RelocPosBase => 2;
public static RelocKind RelocPkg => 3;
public static RelocKind RelocName => 4;
public static RelocKind RelocType => 5;
public static RelocKind RelocObj => 6;
public static RelocKind RelocObjExt => 7;
public static RelocKind RelocObjDict => 8;
public static RelocKind RelocBody => 9;
internal static UntypedInt numRelocs => /* iota */ 10;

} // end pkgbits_package
