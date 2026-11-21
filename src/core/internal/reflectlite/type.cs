// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package reflectlite implements lightweight version of reflect, not using
// any package except for "runtime", "unsafe", and "internal/abi"

// A Kind represents the specific kind of type that a Type represents.
// The zero Kind is not a valid kind.
global using Kind = go.@internal.abi_package.Kind;
global using nameOff = go.@internal.abi_package.NameOff;
global using typeOff = go.@internal.abi_package.TypeOff;
global using textOff = go.@internal.abi_package.TextOff;

// uncommonType is present only for defined types or types with methods
// (if T is a defined type, the uncommonTypes for T and *T have methods).
// Using a pointer to this struct reduces the overall size required
// to describe a non-defined type with no methods.
global using uncommonType = go.@internal.abi_package.UncommonType;

// arrayType represents a fixed array type.
global using arrayType = go.@internal.abi_package.ArrayType;

// chanType represents a channel type.
global using chanType = go.@internal.abi_package.ChanType;
global using funcType = go.@internal.abi_package.FuncType;
global using interfaceType = go.@internal.abi_package.InterfaceType;

// ptrType represents a pointer type.
global using ptrType = go.@internal.abi_package.PtrType;

// sliceType represents a slice type.
global using sliceType = go.@internal.abi_package.SliceType;

// structType represents a struct type.
global using structType = go.@internal.abi_package.StructType;

namespace go.@internal;

using abi = go.@internal.abi_package;
using @unsafe = unsafe_package;
using go.@internal;
using static reflectlite_package;

partial class reflectlite_package {

// Type is the representation of a Go type.
//
// Not all methods apply to all kinds of types. Restrictions,
// if any, are noted in the documentation for each method.
// Use the Kind method to find out the kind of type before
// calling kind-specific methods. Calling a method
// inappropriate to the kind of type causes a run-time panic.
//
// Type values are comparable, such as with the == operator,
// so they can be used as map keys.
// Two Type values are equal if they represent identical types.
partial interface Type {
// Methods applicable to all types.

    // Name returns the type's name within its package for a defined type.
    // For other (non-defined) types it returns the empty string.
    @string Name();
    // PkgPath returns a defined type's package path, that is, the import path
    // that uniquely identifies the package, such as "encoding/base64".
    // If the type was predeclared (string, error) or not defined (*T, struct{},
    // []int, or A where A is an alias for a non-defined type), the package path
    // will be the empty string.
    @string PkgPath();
    // Size returns the number of bytes needed to store
    // a value of the given type; it is analogous to unsafe.Sizeof.
    uintptr Size();
    // Kind returns the specific kind of this type.
    Kind Kind();
    // Implements reports whether the type implements the interface type u.
    bool Implements(Type u);
    // AssignableTo reports whether a value of the type is assignable to type u.
    bool AssignableTo(Type u);
    // Comparable reports whether values of this type are comparable.
    bool Comparable();
    // String returns a string representation of the type.
    // The string representation may use shortened package names
    // (e.g., base64 instead of "encoding/base64") and is not
    // guaranteed to be unique among types. To test for type identity,
    // compare the Types directly.
    @string String();
    // Elem returns a type's element type.
    // It panics if the type's Kind is not Ptr.
    Type Elem();
    ж<abi.Type> common();
    ж<uncommonType> uncommon();
}

/*
 * These data structures are known to the compiler (../../cmd/internal/reflectdata/reflect.go).
 * A few are known to ../runtime/type.go to convey to debuggers.
 * They are also known to ../runtime/type.go.
 */
public static abi.Kind Ptr => /* abi.Pointer */ 22;

// Import-and-export these constants as necessary
public static abi.Kind Interface => /* abi.Interface */ 20;
public static abi.Kind Slice => /* abi.Slice */ 23;
public static abi.Kind String => /* abi.String */ 24;
public static abi.Kind Struct => /* abi.Struct */ 25;

partial struct rtype {
    /*embed*/ public ж<@internal.abi_package.Type> Type;
}

// name is an encoded type name with optional extra data.
//
// The first byte is a bit field containing:
//
//	1<<0 the name is exported
//	1<<1 tag data follows the name
//	1<<2 pkgPath nameOff follows the name and tag
//
// The next two bytes are the data length:
//
//	l := uint16(data[1])<<8 | uint16(data[2])
//
// Bytes [3:3+l] are the string data.
//
// If tag data follows then bytes 3+l and 3+l+1 are the tag length,
// with the data following.
//
// If the import path follows, then 4 bytes at the end of
// the data form a nameOff. The import path is only set for concrete
// methods that are defined in a different package than their type.
//
// If a name starts with "*", then the exported bit represents
// whether the pointed to type is exported.
partial struct name {
    internal ж<byte> bytes;
}

} partial class reflectlite_package {

[GoMethodBody("data")] internal static ж<byte> dataᴍ(name n, nint off, @string whySafe) {
    return @unsafe.Pointer.To<byte>(add(@unsafe.Pointer.FromPinnedBox(n.bytes), (uintptr)off, whySafe));
}

[GoMethodBody("isExported")] internal static bool isExportedᴍ(name n) {
    return (byte)((n.bytes.Value) & ((byte)(1 << 0))) != 0;
}

[GoMethodBody("hasTag")] internal static bool hasTagᴍ(name n) {
    return (byte)((n.bytes.Value) & ((byte)(1 << 1))) != 0;
}

[GoMethodBody("embedded")] internal static bool embeddedᴍ(name n) {
    return (byte)((n.bytes.Value) & ((byte)(1 << 3))) != 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string readVarintˢᴍname => ref readVarintˢᵇᴍname;
private static readonly @string readVarintˢᵇᴍname = "read varint"u8;

// readVarint parses a varint as encoded by encoding/binary.
// It returns the number of encoded bytes and the encoded value.
[GoMethodBody("readVarint")] internal static (nint, nint) readVarintᴍ(name n, nint off) {
    nint v = 0;
    for (nint i = 0; ; i++) {
        var x = n.data(off + i, readVarintˢᴍname).Value;
        v += ((nint)((byte)(x & 0x7f))).Lsh((int64)(7 * i));
        if ((byte)(x & 0x80) == 0) {
            return (i + 1, v);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nonEmptyStringˢᴍname => ref nonEmptyStringˢᵇᴍname;
private static readonly @string nonEmptyStringˢᵇᴍname = "non-empty string"u8;

[GoMethodBody("name")] internal static @string nameᴍ(name n) {
    if (n.bytes == nil) {
        return ""u8;
    }
    var (i, l) = n.readVarint(1);
    return @unsafe.String(n.data(1 + i, nonEmptyStringˢᴍname), l);
}

[GoMethodBody("tag")] internal static @string tagᴍ(name n) {
    if (!n.hasTag()) {
        return ""u8;
    }
    var (i, l) = n.readVarint(1);
    var (i2, l2) = n.readVarint(1 + i + l);
    return @unsafe.String(n.data(1 + i + l + i2, nonEmptyStringˢᴍname), l2);
}

} partial class reflectlite_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nameFlagFieldˢ => ref nameFlagFieldˢᵇ;
private static readonly @string nameFlagFieldˢᵇ = "name flag field"u8;
internal static ref readonly @string nameOffsetFieldˢ => ref nameOffsetFieldˢᵇ;
private static readonly @string nameOffsetFieldˢᵇ = "name offset field"u8;

internal static @string pkgPath(abi.Name n) {
    if (n.Bytes == nil || (byte)(n.DataChecked(0, nameFlagFieldˢ).Value & ((byte)(1 << 2))) == 0) {
        return ""u8;
    }
    var (i, l) = n.ReadVarint(1);
    nint off = 1 + i + l;
    if (n.HasTag()) {
        var (i2, l2) = n.ReadVarint(off);
        off += i2 + l2;
    }
    ref var nameOff = ref heap(new int32(), out var ᏑnameOff);
    // Note that this field may not be aligned in memory,
    // so we cannot use a direct int32 assignment here.
    @unsafe.Pointer.CopyInto<byte>(@unsafe.Pointer.FromPinnedBox(ᏑnameOff), 0, 4, array<byte>.AliasPointer(n.DataChecked(off, nameOffsetFieldˢ), 4).slice());
    var pkgPathName = new name(@unsafe.Pointer.To<byte>(resolveTypeOff(@unsafe.Pointer.FromPinnedBox(n.Bytes), nameOff)));
    return pkgPathName.name();
}

/*
 * The compiler knows the exact layout of all the data structures above.
 * The compiler does not know about the data structures and methods below.
 */

// resolveNameOff resolves a name offset from a base pointer.
// The (*rtype).nameOff method is a convenience wrapper for this function.
// Implemented in the runtime package.
//
//go:noescape
internal static partial @unsafe.Pointer resolveNameOff(@unsafe.Pointer ptrInModule, int32 off);

// resolveTypeOff resolves an *rtype offset from a base type.
// The (*rtype).typeOff method is a convenience wrapper for this function.
// Implemented in the runtime package.
//
//go:noescape
internal static partial @unsafe.Pointer resolveTypeOff(@unsafe.Pointer rtype, int32 off);

} partial class reflectlite_package {

[GoMethodBody("nameOff")] internal static abi.Name nameOffᴍ(rtype t, nameOff off) {
    return new abi.Name(Bytes: @unsafe.Pointer.To<byte>(resolveNameOff(@unsafe.Pointer.FromPinnedBox(t.Type), (int32)off)));
}

[GoMethodBody("typeOff")] internal static ж<abi.Type> typeOffᴍ(rtype t, typeOff off) {
    return @unsafe.Pointer.To<abi.Type>(resolveTypeOff(@unsafe.Pointer.FromPinnedBox(t.Type), (int32)off));
}

[GoMethodBody("uncommon")] internal static ж<uncommonType> uncommonᴍ(rtype t) {
    return t.Type.Uncommon();
}

// go2cs generated this placeholder — func String is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

[GoMethodBody("common")] internal static ж<abi.Type> commonᴍ(rtype t) { return t.Type; }

[GoMethodBody("exportedMethods")] internal static slice<abi.Method> exportedMethodsᴍ(rtype t) {
    var ut = t.uncommon();
    if (ut == nil) {
        return default;
    }
    return ut.ExportedMethods();
}

[GoMethodBody("NumMethod")] internal static nint NumMethodᴍ(rtype t) {
    var tt = t.Type.InterfaceType();
    if (tt != nil) {
        return tt.NumMethod();
    }
    return len(t.exportedMethods());
}

// go2cs generated this placeholder — func PkgPath is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

[GoMethodBody("Name")] internal static @string Nameᴍ(rtype t) {
    if (!t.Type.Value.HasName()) {
        return ""u8;
    }
    @string s = t.String();
    nint i = len(s) - 1;
    nint sqBrackets = 0;
    while (i >= 0 && (s[i] != '.' || sqBrackets != 0)) {
        switch (s[i]) {
        case (byte)']': {
            sqBrackets++;
            break;
        }
        case (byte)'[': {
            sqBrackets--;
            break;
        }}
        i--;
    }
    return s.slice(i + 1);
}

} partial class reflectlite_package {

internal static rtype toRType(ж<abi.Type> t) {
    return new rtype(t);
}

internal static ж<abi.Type> elem(ж<abi.Type> t) {
    var et = t.Elem();
    if (et != nil) {
        return et;
    }
    throw panic("reflect: Elem of invalid type " + toRType(t).String());
}

} partial class reflectlite_package {

// go2cs generated this placeholder — func Elem is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

[GoMethodBody("In")] internal static Type Inᴍ(rtype t, nint i) {
    var tt = t.Type.FuncType();
    if (tt == nil) {
        throw panic("reflect: In of non-func type");
    }
    return toType(tt.InSlice()[i]);
}

[GoMethodBody("Key")] internal static Type Keyᴍ(rtype t) {
    var tt = t.Type.MapType();
    if (tt == nil) {
        throw panic("reflect: Key of non-map type");
    }
    return toType((~tt).Key);
}

[GoMethodBody("Len")] internal static nint Lenᴍ(rtype t) {
    var tt = t.Type.ArrayType();
    if (tt == nil) {
        throw panic("reflect: Len of non-array type");
    }
    return (nint)(~tt).Len;
}

[GoMethodBody("NumField")] internal static nint NumFieldᴍ(rtype t) {
    var tt = t.Type.StructType();
    if (tt == nil) {
        throw panic("reflect: NumField of non-struct type");
    }
    return len(tt.Fields);
}

[GoMethodBody("NumIn")] internal static nint NumInᴍ(rtype t) {
    var tt = t.Type.FuncType();
    if (tt == nil) {
        throw panic("reflect: NumIn of non-func type");
    }
    return (nint)tt.InCount;
}

[GoMethodBody("NumOut")] internal static nint NumOutᴍ(rtype t) {
    var tt = t.Type.FuncType();
    if (tt == nil) {
        throw panic("reflect: NumOut of non-func type");
    }
    return tt.NumOut();
}

[GoMethodBody("Out")] internal static Type Outᴍ(rtype t, nint i) {
    var tt = t.Type.FuncType();
    if (tt == nil) {
        throw panic("reflect: Out of non-func type");
    }
    return toType(tt.OutSlice()[i]);
}

} partial class reflectlite_package {

// add returns p+x.
//
// The whySafe string is ignored, so that the function still inlines
// as efficiently as p+x, but all call sites should use the string to
// record why the addition is safe, which is to say why the addition
// does not cause x to advance to the very end of p's allocation
// and therefore point incorrectly at the next block in memory.
internal static @unsafe.Pointer add(@unsafe.Pointer p, uintptr x, @string whySafe) {
    return @unsafe.Add(p, x);
}

// TypeOf returns the reflection Type that represents the dynamic type of i.
// If i is a nil interface value, TypeOf returns nil.
public static Type TypeOf(any i) {
    return toType(abi.TypeOf(i));
}

} partial class reflectlite_package {

// go2cs generated this placeholder — func Implements is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

[GoMethodBody("AssignableTo")] internal static bool AssignableToᴍ(rtype t, Type u) {
    if (u == nil) {
        throw panic("reflect: nil type passed to Type.AssignableTo");
    }
    var uu = u.common();
    var tt = t.common();
    return directlyAssignable(uu, tt) || implements(uu, tt);
}

[GoMethodBody("Comparable")] internal static bool Comparableᴍ(rtype t) {
    return t.Equal != nil;
}

} partial class reflectlite_package {

// go2cs generated this placeholder — func implements is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// directlyAssignable reports whether a value x of type V can be directly
// assigned (using memmove) to a value of type T.
// https://golang.org/doc/go_spec.html#Assignability
// Ignoring the interface rules (implemented elsewhere)
// and the ideal constant rules (no ideal constants at run time).
internal static bool directlyAssignable(ж<abi.Type> ᏑT, ж<abi.Type> ᏑV) {
    ref var T = ref ᏑT.DerefOrNull();
    ref var V = ref ᏑV.DerefOrNull();

    // x's type V is identical to T?
    if (ᏑT == ᏑV) {
        return true;
    }

    // Otherwise at least one of T and V must not be defined
    // and they must have the same kind.
    if (T.HasName() && V.HasName() || T.Kind() != V.Kind()) {
        return false;
    }

    // x's type T and V must  have identical underlying types.
    return haveIdenticalUnderlyingType(ᏑT, ᏑV, true);
}

internal static bool haveIdenticalType(ж<abi.Type> ᏑT, ж<abi.Type> ᏑV, bool cmpTags) {
    ref var T = ref ᏑT.DerefOrNull();
    ref var V = ref ᏑV.DerefOrNull();

    if (cmpTags) {
        return ᏑT == ᏑV;
    }

    if (toRType(ᏑT).Name() != toRType(ᏑV).Name() || T.Kind() != V.Kind()) {
        return false;
    }

    return haveIdenticalUnderlyingType(ᏑT, ᏑV, false);
}

// go2cs generated this placeholder — func haveIdenticalUnderlyingType is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// toType converts from a *rtype to a Type that can be returned
// to the client of package reflect. In gc, the only concern is that
// a nil *rtype must be replaced by a nil Type, but in gccgo this
// function takes care of ensuring that multiple *rtype for the same
// type are coalesced into a single Type.
internal static Type toType(ж<abi.Type> t) {
    if (t == nil) {
        return default;
    }
    return toRType(t);
}

} // end reflectlite_package
