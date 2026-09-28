// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package reflectlite implements lightweight version of reflect, not using
// any package except for "runtime", "unsafe", and "internal/abi"
global using Kind = go.@internal.abi_package.ΔKind;
global using nameOff = go.@internal.abi_package.NameOff;
global using typeOff = go.@internal.abi_package.TypeOff;
global using textOff = go.@internal.abi_package.TextOff;
global using uncommonType = go.@internal.abi_package.UncommonType;
global using arrayType = go.@internal.abi_package.ΔArrayType;
global using chanType = go.@internal.abi_package.ChanType;
global using funcType = go.@internal.abi_package.ΔFuncType;
global using interfaceType = go.@internal.abi_package.ΔInterfaceType;
global using ptrType = go.@internal.abi_package.PtrType;
global using sliceType = go.@internal.abi_package.SliceType;
global using structType = go.@internal.abi_package.ΔStructType;

namespace go.@internal;

using abi = go.@internal.abi_package;
using @unsafe = unsafe_package;
using go.@internal;

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
[GoType] partial interface ΔType {
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
    abiꓸKind Kind();
    // Implements reports whether the type implements the interface type u.
    bool Implements(ΔType u);
    // AssignableTo reports whether a value of the type is assignable to type u.
    bool AssignableTo(ΔType u);
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
    ΔType Elem();
    ж<abi.Type> common();
    ж<uncommonType> uncommon();
}

/*
 * These data structures are known to the compiler (../../cmd/internal/reflectdata/reflect.go).
 * A few are known to ../runtime/type.go to convey to debuggers.
 * They are also known to ../runtime/type.go.
 */
public static abiꓸKind Ptr => /* abi.Pointer */ 22;

public static abiꓸKind Interface => /* abi.Interface */ 20;
public static abiꓸKind Slice => /* abi.Slice */ 23;
public static abiꓸKind ΔString => /* abi.String */ 24;
public static abiꓸKind Struct => /* abi.Struct */ 25;

[GoType] partial struct rtype {
    public partial ref ж<@internal.abi_package.Type> Type { get; }
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
[GoType] partial struct Δname {
    internal ж<byte> bytes;
}

internal static ж<byte> data(this Δname n, nint off, @string whySafe) {
    return (ж<byte>)(uintptr)(add(@unsafe.Pointer.FromPinnedBox(n.bytes), (uintptr)off, whySafe));
}

internal static bool isExported(this Δname n) {
    return (byte)((n.bytes.Value) & ((byte)(1 << (int)(0)))) != 0;
}

internal static bool hasTag(this Δname n) {
    return (byte)((n.bytes.Value) & ((byte)(1 << (int)(1)))) != 0;
}

internal static bool embedded(this Δname n) {
    return (byte)((n.bytes.Value) & ((byte)(1 << (int)(3)))) != 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string readVarintˢ = "read varint"u8;

// readVarint parses a varint as encoded by encoding/binary.
// It returns the number of encoded bytes and the encoded value.
internal static (nint, nint) readVarint(this Δname n, nint off) {
    nint v = 0;
    for (nint i = 0; ᐧ ; i++) {
        var x = n.data(off + i, readVarintˢ).Value;
        v += ((nint)((byte)(x & 0x7f))).Lsh((int64)((7 * i)));
        if ((byte)(x & 0x80) == 0) {
            return (i + 1, v);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nonEmptyStringˢ = "non-empty string"u8;

internal static @string name(this Δname n) {
    if (n.bytes == nil) {
        return ""u8;
    }
    var (i, l) = n.readVarint(1);
    return @unsafe.String(n.data(1 + i, nonEmptyStringˢ), l);
}

internal static @string tag(this Δname n) {
    if (!n.hasTag()) {
        return ""u8;
    }
    var (i, l) = n.readVarint(1);
    var (i2, l2) = n.readVarint(1 + i + l);
    return @unsafe.String(n.data(1 + i + l + i2, nonEmptyStringˢ), l2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nameFlagFieldˢ = "name flag field"u8;
internal static readonly @string nameOffsetFieldˢ = "name offset field"u8;

internal static @string pkgPath(abiꓸName n) {
    if (n.Bytes == nil || (byte)(n.DataChecked(0, nameFlagFieldˢ).Value & ((byte)(1 << (int)(2)))) == 0) {
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
    copy((~(ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(ᏑnameOff)))[..], (~array<byte>.AliasPointer(n.DataChecked(off, nameOffsetFieldˢ), 4))[..]);
    var pkgPathName = new Δname((ж<byte>)(uintptr)(resolveTypeOff(@unsafe.Pointer.FromPinnedBox(n.Bytes), nameOff)));
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

internal static abiꓸName nameOff(this rtype t, nameOff off) {
    return new abiꓸName(Bytes: (ж<byte>)(uintptr)(resolveNameOff(@unsafe.Pointer.FromPinnedBox(t.Type), (int32)off)));
}

internal static ж<abi.Type> typeOff(this rtype t, typeOff off) {
    return (ж<abi.Type>)(uintptr)(resolveTypeOff(@unsafe.Pointer.FromPinnedBox(t.Type), (int32)off));
}

internal static ж<uncommonType> uncommon(this rtype t) {
    return t.Type.Uncommon();
}

// go2cs generated this placeholder — func String is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static ж<abi.Type> common(this rtype t) {
    return t.Type;
}

internal static slice<abi.Method> exportedMethods(this rtype t) {
    var ut = t.uncommon();
    if (ut == nil) {
        return default!;
    }
    return ut.ExportedMethods();
}

internal static nint NumMethod(this rtype t) {
    var tt = t.Type.InterfaceType();
    if (tt != nil) {
        return tt.NumMethod();
    }
    return len(t.exportedMethods());
}

// go2cs generated this placeholder — func PkgPath is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static @string Name(this rtype t) {
    if (!t.Type.Value.HasName()) {
        return ""u8;
    }
    @string s = t.String();
    nint i = len(s) - 1;
    nint sqBrackets = 0;
    while (i >= 0 && (s[i] != (rune)'.' || sqBrackets != 0)) {
        switch (s[i]) {
        case (rune)']': {
            sqBrackets++;
            break;
        }
        case (rune)'[': {
            sqBrackets--;
            break;
        }}

        i--;
    }
    return s[(int)(i + 1)..];
}

internal static rtype toRType(ж<abi.Type> Ꮡt) {
    return new rtype(Ꮡt);
}

internal static ж<abi.Type> elem(ж<abi.Type> Ꮡt) {
    var et = Ꮡt.Elem();
    if (et != nil) {
        return et;
    }
    throw panic("reflect: Elem of invalid type " + toRType(Ꮡt).String());
}

// go2cs generated this placeholder — func Elem is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static ΔType In(this rtype t, nint i) {
    var tt = t.Type.FuncType();
    if (tt == nil) {
        throw panic("reflect: In of non-func type");
    }
    return toType(tt.InSlice()[i]);
}

internal static ΔType Key(this rtype t) {
    var tt = t.Type.MapType();
    if (tt == nil) {
        throw panic("reflect: Key of non-map type");
    }
    return toType((~tt).Key);
}

internal static nint Len(this rtype t) {
    var tt = t.Type.ArrayType();
    if (tt == nil) {
        throw panic("reflect: Len of non-array type");
    }
    return (nint)(~tt).Len;
}

internal static nint NumField(this rtype t) {
    var tt = t.Type.StructType();
    if (tt == nil) {
        throw panic("reflect: NumField of non-struct type");
    }
    return len((~tt).Fields);
}

internal static nint NumIn(this rtype t) {
    var tt = t.Type.FuncType();
    if (tt == nil) {
        throw panic("reflect: NumIn of non-func type");
    }
    return (nint)(~tt).InCount;
}

internal static nint NumOut(this rtype t) {
    var tt = t.Type.FuncType();
    if (tt == nil) {
        throw panic("reflect: NumOut of non-func type");
    }
    return tt.NumOut();
}

internal static ΔType Out(this rtype t, nint i) {
    var tt = t.Type.FuncType();
    if (tt == nil) {
        throw panic("reflect: Out of non-func type");
    }
    return toType(tt.OutSlice()[i]);
}

// add returns p+x.
//
// The whySafe string is ignored, so that the function still inlines
// as efficiently as p+x, but all call sites should use the string to
// record why the addition is safe, which is to say why the addition
// does not cause x to advance to the very end of p's allocation
// and therefore point incorrectly at the next block in memory.
internal static @unsafe.Pointer add(@unsafe.Pointer p, uintptr x, @string whySafe) {
    return (@unsafe.Pointer)((uintptr)p + x);
}

// TypeOf returns the reflection Type that represents the dynamic type of i.
// If i is a nil interface value, TypeOf returns nil.
public static ΔType TypeOf(any i) {
    return toType(abi.TypeOf(i));
}

// go2cs generated this placeholder — func Implements is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static bool AssignableTo(this rtype t, ΔType u) {
    if (u == default!) {
        throw panic("reflect: nil type passed to Type.AssignableTo");
    }
    var uu = u.common();
    var tt = t.common();
    return directlyAssignable(uu, tt) || implements(uu, tt);
}

internal static bool Comparable(this rtype t) {
    return t.Equal != default!;
}

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
internal static ΔType toType(ж<abi.Type> Ꮡt) {
    if (Ꮡt == nil) {
        return default!;
    }
    return toRType(Ꮡt);
}

} // end reflectlite_package
