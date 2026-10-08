// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class pkgbits_package {

// A Code is an enum value that can be encoded into bitstreams.
//
// Code types are preferable for enum types, because they allow
// Decoder to detect desyncs.
partial interface ΔCode {
    // Marker returns the SyncMarker for the Code's dynamic type.
    SyncMarker Marker();
    // Value returns the Code's ordinal value.
    nint Value();
}

partial struct CodeVal /*num:nint*/;

public static SyncMarker Marker(this CodeVal c) {
    return SyncVal;
}

public static nint Value(this CodeVal c) {
    return (nint)c;
}

// Note: These values are public and cannot be changed without
// updating the go/types importers.
public static CodeVal ValBool => /* iota */ 0;
public static CodeVal ValString => 1;
public static CodeVal ValInt64 => 2;
public static CodeVal ValBigInt => 3;
public static CodeVal ValBigRat => 4;
public static CodeVal ValBigFloat => 5;

partial struct CodeType /*num:nint*/;

public static SyncMarker Marker(this CodeType c) {
    return SyncType;
}

public static nint Value(this CodeType c) {
    return (nint)c;
}

// Note: These values are public and cannot be changed without
// updating the go/types importers.
public static CodeType TypeBasic => /* iota */ 0;
public static CodeType TypeNamed => 1;
public static CodeType TypePointer => 2;
public static CodeType TypeSlice => 3;
public static CodeType TypeArray => 4;
public static CodeType TypeChan => 5;
public static CodeType TypeMap => 6;
public static CodeType TypeSignature => 7;
public static CodeType TypeStruct => 8;
public static CodeType TypeInterface => 9;
public static CodeType TypeUnion => 10;
public static CodeType TypeTypeParam => 11;

partial struct CodeObj /*num:nint*/;

public static SyncMarker Marker(this CodeObj c) {
    return SyncCodeObj;
}

public static nint Value(this CodeObj c) {
    return (nint)c;
}

// Note: These values are public and cannot be changed without
// updating the go/types importers.
public static CodeObj ObjAlias => /* iota */ 0;
public static CodeObj ObjConst => 1;
public static CodeObj ObjType => 2;
public static CodeObj ObjFunc => 3;
public static CodeObj ObjVar => 4;
public static CodeObj ObjStub => 5;

} // end pkgbits_package
