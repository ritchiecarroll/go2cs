// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package reflect implements run-time reflection, allowing a program to
// manipulate objects with arbitrary types. The typical use is to take a value
// with static type interface{} and extract its dynamic type information by
// calling TypeOf, which returns a Type.
//
// A call to ValueOf returns a Value representing the run-time data.
// Zero takes a Type and returns a Value representing a zero value
// for that type.
//
// See "The Laws of Reflection" for an introduction to reflection in Go:
// https://golang.org/doc/articles/laws_of_reflection.html
global using uncommonType = go.@internal.abi_package.UncommonType;
global using aNameOff = go.@internal.abi_package.NameOff;
global using aTypeOff = go.@internal.abi_package.TypeOff;
global using aTextOff = go.@internal.abi_package.TextOff;
global using arrayType = go.@internal.abi_package.ΔArrayType;
global using chanType = go.@internal.abi_package.ChanType;
global using funcType = go.@internal.abi_package.ΔFuncType;
global using structField = go.@internal.abi_package.StructField;

namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using strconv = strconv_package;
using Δsync = sync_package;
using Δunicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using @unsafe = unsafe_package;
using @internal;
using go.unicode;
using ꓸꓸꓸbyte = Span<byte>;

partial class reflect_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸstrconv() {
    builtin.initPackage(typeof(strconv_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸsync() {
    builtin.initPackage(typeof(sync_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸunicode() {
    builtin.initPackage(typeof(unicode_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() {
    builtin.initPackage(typeof(go.unicode.utf8_package));
}

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

    // Align returns the alignment in bytes of a value of
    // this type when allocated in memory.
    nint Align();
    // FieldAlign returns the alignment in bytes of a value of
    // this type when used as a field in a struct.
    nint FieldAlign();
    // Method returns the i'th method in the type's method set.
    // It panics if i is not in the range [0, NumMethod()).
    //
    // For a non-interface type T or *T, the returned Method's Type and Func
    // fields describe a function whose first argument is the receiver,
    // and only exported methods are accessible.
    //
    // For an interface type, the returned Method's Type field gives the
    // method signature, without a receiver, and the Func field is nil.
    //
    // Methods are sorted in lexicographic order.
    ΔMethod Method(nint _);
    // MethodByName returns the method with that name in the type's
    // method set and a boolean indicating if the method was found.
    //
    // For a non-interface type T or *T, the returned Method's Type and Func
    // fields describe a function whose first argument is the receiver.
    //
    // For an interface type, the returned Method's Type field gives the
    // method signature, without a receiver, and the Func field is nil.
    (ΔMethod, bool) MethodByName(@string _);
    // NumMethod returns the number of methods accessible using Method.
    //
    // For a non-interface type, it returns the number of exported methods.
    //
    // For an interface type, it returns the number of exported and unexported methods.
    nint NumMethod();
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
    // String returns a string representation of the type.
    // The string representation may use shortened package names
    // (e.g., base64 instead of "encoding/base64") and is not
    // guaranteed to be unique among types. To test for type identity,
    // compare the Types directly.
    @string String();
    // Kind returns the specific kind of this type.
    ΔKind Kind();
    // Implements reports whether the type implements the interface type u.
    bool Implements(ΔType u);
    // AssignableTo reports whether a value of the type is assignable to type u.
    bool AssignableTo(ΔType u);
    // ConvertibleTo reports whether a value of the type is convertible to type u.
    // Even if ConvertibleTo returns true, the conversion may still panic.
    // For example, a slice of type []T is convertible to *[N]T,
    // but the conversion will panic if its length is less than N.
    bool ConvertibleTo(ΔType u);
    // Comparable reports whether values of this type are comparable.
    // Even if Comparable returns true, the comparison may still panic.
    // For example, values of interface type are comparable,
    // but the comparison will panic if their dynamic type is not comparable.
    bool Comparable();
// Methods applicable only to some types, depending on Kind.
// The methods allowed for each kind are:
//
//	Int*, Uint*, Float*, Complex*: Bits
//	Array: Elem, Len
//	Chan: ChanDir, Elem
//	Func: In, NumIn, Out, NumOut, IsVariadic.
//	Map: Key, Elem
//	Pointer: Elem
//	Slice: Elem
//	Struct: Field, FieldByIndex, FieldByName, FieldByNameFunc, NumField

    // Bits returns the size of the type in bits.
    // It panics if the type's Kind is not one of the
    // sized or unsized Int, Uint, Float, or Complex kinds.
    nint Bits();
    // ChanDir returns a channel type's direction.
    // It panics if the type's Kind is not Chan.
    ΔChanDir ChanDir();
    // IsVariadic reports whether a function type's final input parameter
    // is a "..." parameter. If so, t.In(t.NumIn() - 1) returns the parameter's
    // implicit actual type []T.
    //
    // For concreteness, if t represents func(x int, y ... float64), then
    //
    //	t.NumIn() == 2
    //	t.In(0) is the reflect.Type for "int"
    //	t.In(1) is the reflect.Type for "[]float64"
    //	t.IsVariadic() == true
    //
    // IsVariadic panics if the type's Kind is not Func.
    bool IsVariadic();
    // Elem returns a type's element type.
    // It panics if the type's Kind is not Array, Chan, Map, Pointer, or Slice.
    ΔType Elem();
    // Field returns a struct type's i'th field.
    // It panics if the type's Kind is not Struct.
    // It panics if i is not in the range [0, NumField()).
    StructField Field(nint i);
    // FieldByIndex returns the nested field corresponding
    // to the index sequence. It is equivalent to calling Field
    // successively for each index i.
    // It panics if the type's Kind is not Struct.
    StructField FieldByIndex(slice<nint> index);
    // FieldByName returns the struct field with the given name
    // and a boolean indicating if the field was found.
    // If the returned field is promoted from an embedded struct,
    // then Offset in the returned StructField is the offset in
    // the embedded struct.
    (StructField, bool) FieldByName(@string name);
    // FieldByNameFunc returns the struct field with a name
    // that satisfies the match function and a boolean indicating if
    // the field was found.
    //
    // FieldByNameFunc considers the fields in the struct itself
    // and then the fields in any embedded structs, in breadth first order,
    // stopping at the shallowest nesting depth containing one or more
    // fields satisfying the match function. If multiple fields at that depth
    // satisfy the match function, they cancel each other
    // and FieldByNameFunc returns no match.
    // This behavior mirrors Go's handling of name lookup in
    // structs containing embedded fields.
    //
    // If the returned field is promoted from an embedded struct,
    // then Offset in the returned StructField is the offset in
    // the embedded struct.
    (StructField, bool) FieldByNameFunc(Func<@string, bool> match);
    // In returns the type of a function type's i'th input parameter.
    // It panics if the type's Kind is not Func.
    // It panics if i is not in the range [0, NumIn()).
    ΔType In(nint i);
    // Key returns a map type's key type.
    // It panics if the type's Kind is not Map.
    ΔType Key();
    // Len returns an array type's length.
    // It panics if the type's Kind is not Array.
    nint Len();
    // NumField returns a struct type's field count.
    // It panics if the type's Kind is not Struct.
    nint NumField();
    // NumIn returns a function type's input parameter count.
    // It panics if the type's Kind is not Func.
    nint NumIn();
    // NumOut returns a function type's output parameter count.
    // It panics if the type's Kind is not Func.
    nint NumOut();
    // Out returns the type of a function type's i'th output parameter.
    // It panics if the type's Kind is not Func.
    // It panics if i is not in the range [0, NumOut()).
    ΔType Out(nint i);
    // OverflowComplex reports whether the complex128 x cannot be represented by type t.
    // It panics if t's Kind is not Complex64 or Complex128.
    bool OverflowComplex(complex128 x);
    // OverflowFloat reports whether the float64 x cannot be represented by type t.
    // It panics if t's Kind is not Float32 or Float64.
    bool OverflowFloat(float64 x);
    // OverflowInt reports whether the int64 x cannot be represented by type t.
    // It panics if t's Kind is not Int, Int8, Int16, Int32, or Int64.
    bool OverflowInt(int64 x);
    // OverflowUint reports whether the uint64 x cannot be represented by type t.
    // It panics if t's Kind is not Uint, Uintptr, Uint8, Uint16, Uint32, or Uint64.
    bool OverflowUint(uint64 x);
    // CanSeq reports whether a [Value] with this type can be iterated over using [Value.Seq].
    bool CanSeq();
    // CanSeq2 reports whether a [Value] with this type can be iterated over using [Value.Seq2].
    bool CanSeq2();
    ж<abi.Type> common();
    ж<uncommonType> uncommon();
}

[GoType("num:nuint")] partial struct ΔKind;

// BUG(rsc): FieldByName and related functions consider struct field names to be equal
// if the names are equal, even if they are unexported names originating
// in different packages. The practical effect of this is that the result of
// t.FieldByName("x") is not well defined if the struct type t contains
// multiple fields named x (embedded from different packages).
// FieldByName may return one of the fields named x or may report that there are none.
// See https://golang.org/issue/4876 for more details.
/*
 * These data structures are known to the compiler (../cmd/compile/internal/reflectdata/reflect.go).
 * A few are known to ../runtime/type.go to convey to debuggers.
 * They are also known to ../runtime/type.go.
 */
public static ΔKind Invalid => /* iota */ 0;
public static ΔKind ΔBool => 1;
public static ΔKind ΔInt => 2;
public static ΔKind Int8 => 3;
public static ΔKind Int16 => 4;
public static ΔKind Int32 => 5;
public static ΔKind Int64 => 6;
public static ΔKind ΔUint => 7;
public static ΔKind Uint8 => 8;
public static ΔKind Uint16 => 9;
public static ΔKind Uint32 => 10;
public static ΔKind Uint64 => 11;
public static ΔKind Uintptr => 12;
public static ΔKind Float32 => 13;
public static ΔKind Float64 => 14;
public static ΔKind Complex64 => 15;
public static ΔKind Complex128 => 16;
public static ΔKind Array => 17;
public static ΔKind Chan => 18;
public static ΔKind Func => 19;
public static ΔKind ΔInterface => 20;
public static ΔKind Map => 21;
public static ΔKind ΔPointer => 22;
public static ΔKind ΔSlice => 23;
public static ΔKind ΔString => 24;
public static ΔKind Struct => 25;
public static ΔKind ΔUnsafePointer => 26;

// Ptr is the old name for the [Pointer] kind.
public static ΔKind Ptr => /* Pointer */ 22;

// Embed this type to get common/uncommon
[GoType] partial struct Δcommon {
    public partial ref @internal.abi_package.Type Type { get; }
}

// rtype is the common implementation of most values.
// It is embedded in other struct types.
[GoType] partial struct rtype {
    internal abi.Type t;
}

internal static ж<abi.Type> common(this ж<rtype> Ꮡt) {
    return Ꮡt.of(rtype.Ꮡt);
}

internal static ж<abi.UncommonType> uncommon(this ж<rtype> Ꮡt) {
    return Ꮡt.of(rtype.Ꮡt).Uncommon();
}

[GoType("num:nint")] partial struct ΔChanDir;

public static ΔChanDir RecvDir => /* 1 << iota */ 1;                 // <-chan
public static ΔChanDir SendDir => 2;                 // chan<-
public static ΔChanDir BothDir => /* RecvDir | SendDir */ 3; // chan

// interfaceType represents an interface type.
[GoType] partial struct interfaceType {
    public partial ref @internal.abi_package.ΔInterfaceType InterfaceType { get; } // can embed directly because not a public type.
}

internal static abiꓸName nameOff(this ж<interfaceType> Ꮡt, aNameOff off) {
    return toRType(Ꮡt.of(interfaceType.ᏑType)).nameOff(off);
}

internal static abiꓸName nameOffFor(ж<abi.Type> Ꮡt, aNameOff off) {
    return toRType(Ꮡt).nameOff(off);
}

internal static ж<abi.Type> typeOffFor(ж<abi.Type> Ꮡt, aTypeOff off) {
    return toRType(Ꮡt).typeOff(off);
}

internal static ж<abi.Type> typeOff(this ж<interfaceType> Ꮡt, aTypeOff off) {
    return toRType(Ꮡt.of(interfaceType.ᏑType)).typeOff(off);
}

internal static ж<abi.Type> common(this ж<interfaceType> Ꮡt) {
    return Ꮡt.of(interfaceType.ᏑType);
}

internal static ж<abi.UncommonType> uncommon(this ж<interfaceType> Ꮡt) {
    return Ꮡt.of(interfaceType.ᏑInterfaceType).of(abiꓸInterfaceType.ᏑType).Uncommon();
}

// mapType represents a map type.
[GoType] partial struct mapType {
    public partial ref @internal.abi_package.ΔMapType MapType { get; }
}

// ptrType represents a pointer type.
[GoType] partial struct ptrType {
    public partial ref @internal.abi_package.PtrType PtrType { get; }
}

// sliceType represents a slice type.
[GoType] partial struct sliceType {
    public partial ref @internal.abi_package.SliceType SliceType { get; }
}

// structType represents a struct type.
[GoType] partial struct structType {
    public partial ref @internal.abi_package.ΔStructType StructType { get; }
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
    copy((~(ж<array<byte>>)(uintptr)(new @unsafe.Pointer(ᏑnameOff)))[..], (~array<byte>.AliasPointer(n.DataChecked(off, nameOffsetFieldˢ), 4))[..]);
    var pkgPathName = new abiꓸName(Bytes: (ж<byte>)(uintptr)(resolveTypeOff(new @unsafe.Pointer(n.Bytes), nameOff)));
    return pkgPathName.Name();
}

internal static abiꓸName newName(@string n, @string tag, bool exported, bool embedded) {
    return abi.NewName(n, tag, exported, embedded);
}

/*
 * The compiler knows the exact layout of all the data structures above.
 * The compiler does not know about the data structures and methods below.
 */

// Method represents a single method.
[GoType] partial struct ΔMethod {
    // Name is the method name.
    public @string Name;
    // PkgPath is the package path that qualifies a lower case (unexported)
    // method name. It is empty for upper case (exported) method names.
    // The combination of PkgPath and Name uniquely identifies a method
    // in a method set.
    // See https://golang.org/ref/spec#Uniqueness_of_identifiers
    public @string PkgPath;
    public ΔType Type;  // method type
    public ΔValue Func; // func with receiver as first argument
    public nint Index;  // index for Type.Method
}

// IsExported reports whether the method is exported.
public static bool IsExported(this ΔMethod m) {
    return m.PkgPath == ""u8;
}

// String returns the name of k.
public static @string String(this ΔKind k) {
    if ((nuint)k < (nuint)len(kindNames)) {
        return kindNames[(nint)((nuint)k)];
    }
    return "kind"u8 + strconv.Itoa((nint)(nuint)k);
}

internal static slice<@string> kindNames = new golib.SparseArray<@string>{
    [(int)((nuint)Invalid)] = "invalid"u8,
    [(int)((nuint)ΔBool)] = "bool"u8,
    [(int)((nuint)ΔInt)] = "int"u8,
    [(int)((nuint)Int8)] = "int8"u8,
    [(int)((nuint)Int16)] = "int16"u8,
    [(int)((nuint)Int32)] = "int32"u8,
    [(int)((nuint)Int64)] = "int64"u8,
    [(int)((nuint)ΔUint)] = "uint"u8,
    [(int)((nuint)Uint8)] = "uint8"u8,
    [(int)((nuint)Uint16)] = "uint16"u8,
    [(int)((nuint)Uint32)] = "uint32"u8,
    [(int)((nuint)Uint64)] = "uint64"u8,
    [(int)((nuint)Uintptr)] = "uintptr"u8,
    [(int)((nuint)Float32)] = "float32"u8,
    [(int)((nuint)Float64)] = "float64"u8,
    [(int)((nuint)Complex64)] = "complex64"u8,
    [(int)((nuint)Complex128)] = "complex128"u8,
    [(int)((nuint)Array)] = "array"u8,
    [(int)((nuint)Chan)] = "chan"u8,
    [(int)((nuint)Func)] = "func"u8,
    [(int)((nuint)ΔInterface)] = "interface"u8,
    [(int)((nuint)Map)] = "map"u8,
    [(int)((nuint)ΔPointer)] = "ptr"u8,
    [(int)((nuint)ΔSlice)] = "slice"u8,
    [(int)((nuint)ΔString)] = "string"u8,
    [(int)((nuint)Struct)] = "struct"u8,
    [(int)((nuint)ΔUnsafePointer)] = "unsafe.Pointer"u8
}.slice();

// resolveNameOff resolves a name offset from a base pointer.
// The (*rtype).nameOff method is a convenience wrapper for this function.
// Implemented in the runtime package.
//
//go:noescape
internal static @unsafe.Pointer resolveNameOff(@unsafe.Pointer ptrInModule, int32 off) {
    return go.runtime_package.reflect_resolveNameOff(ptrInModule, off);
}

// resolveTypeOff resolves an *rtype offset from a base type.
// The (*rtype).typeOff method is a convenience wrapper for this function.
// Implemented in the runtime package.
//
//go:noescape
internal static @unsafe.Pointer resolveTypeOff(@unsafe.Pointer rtype, int32 off) {
    return go.runtime_package.reflect_resolveTypeOff(rtype, off);
}

// resolveTextOff resolves a function pointer offset from a base type.
// The (*rtype).textOff method is a convenience wrapper for this function.
// Implemented in the runtime package.
//
//go:noescape
internal static @unsafe.Pointer resolveTextOff(@unsafe.Pointer rtype, int32 off) {
    return go.runtime_package.reflect_resolveTextOff(rtype, off);
}

// addReflectOff adds a pointer to the reflection lookup map in the runtime.
// It returns a new ID that can be used as a typeOff or textOff, and will
// be resolved correctly. Implemented in the runtime package.
//
// addReflectOff should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/goplus/reflectx
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname addReflectOff
//go:noescape
internal static int32 addReflectOff(@unsafe.Pointer ptr) {
    return go.runtime_package.reflect_addReflectOff(ptr);
}

// resolveReflectName adds a name to the reflection lookup map in the runtime.
// It returns a new nameOff that can be used to refer to the pointer.
internal static aNameOff resolveReflectName(abiꓸName n) {
    return ((aNameOff)addReflectOff(new @unsafe.Pointer(n.Bytes)));
}

// resolveReflectType adds a *rtype to the reflection lookup map in the runtime.
// It returns a new typeOff that can be used to refer to the pointer.
internal static aTypeOff resolveReflectType(ж<abi.Type> Ꮡt) {
    return ((aTypeOff)addReflectOff(new @unsafe.Pointer(Ꮡt)));
}

// resolveReflectText adds a function pointer to the reflection lookup map in
// the runtime. It returns a new textOff that can be used to refer to the
// pointer.
internal static aTextOff resolveReflectText(@unsafe.Pointer ptr) {
    return ((aTextOff)addReflectOff(ptr));
}

internal static abiꓸName nameOff(this ж<rtype> Ꮡt, aNameOff off) {
    ref var t = ref Ꮡt.DerefOrNull();

    return new abiꓸName(Bytes: (ж<byte>)(uintptr)(resolveNameOff((uintptr)@unsafe.Pointer.FromRef(ref t), (int32)off)));
}

internal static ж<abi.Type> typeOff(this ж<rtype> Ꮡt, aTypeOff off) {
    ref var t = ref Ꮡt.DerefOrNull();

    return (ж<abi.Type>)(uintptr)(resolveTypeOff((uintptr)@unsafe.Pointer.FromRef(ref t), (int32)off));
}

internal static @unsafe.Pointer textOff(this ж<rtype> Ꮡt, aTextOff off) {
    ref var t = ref Ꮡt.DerefOrNull();

    return (uintptr)resolveTextOff((uintptr)@unsafe.Pointer.FromRef(ref t), (int32)off);
}

internal static @unsafe.Pointer textOffFor(ж<abi.Type> Ꮡt, aTextOff off) {
    return (uintptr)toRType(Ꮡt).textOff(off);
}

// go2cs generated this placeholder — func String is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

[GoRecv] internal static uintptr Size(this ref rtype t) {
    return t.t.Size();
}

internal static nint Bits(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Ꮡt == nil) {
        throw panic("reflect: Bits of nil Type");
    }
    ΔKind k = t.Kind();
    if (k < ΔInt || k > Complex128) {
        throw panic("reflect: Bits of non-arithmetic Type " + Ꮡt.String());
    }
    return (nint)t.t.Size_ * 8;
}

[GoRecv] internal static nint Align(this ref rtype t) {
    return t.t.Align();
}

[GoRecv] internal static nint FieldAlign(this ref rtype t) {
    return t.t.FieldAlign();
}

[GoRecv] internal static ΔKind Kind(this ref rtype t) {
    return ((ΔKind)(nuint)(uint8)t.t.Kind());
}

internal static slice<abi.Method> exportedMethods(this ж<rtype> Ꮡt) {
    var ut = Ꮡt.uncommon();
    if (ut == nil) {
        return default!;
    }
    return ut.ExportedMethods();
}

// go2cs generated this placeholder — func NumMethod is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Method is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func MethodByName is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func PkgPath is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static @string pkgPathFor(ж<abi.Type> Ꮡt) {
    return toRType(Ꮡt).PkgPath();
}

// go2cs generated this placeholder — func Name is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static @string nameFor(ж<abi.Type> Ꮡt) {
    return toRType(Ꮡt).Name();
}

// go2cs generated this placeholder — func ChanDir is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static ж<rtype> toRType(ж<abi.Type> Ꮡt) {
    return Ꮡt.Reinterpret<abi.Type, rtype>();
}

internal static ж<abi.Type> elem(ж<abi.Type> Ꮡt) {
    var et = Ꮡt.Elem();
    if (et != nil) {
        return et;
    }
    throw panic("reflect: Elem of invalid type " + stringFor(Ꮡt));
}

// go2cs generated this placeholder — func Elem is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Field is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func FieldByIndex is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func FieldByName is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static (StructField, bool) FieldByNameFunc(this ж<rtype> Ꮡt, Func<@string, bool> match) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Struct) {
        throw panic("reflect: FieldByNameFunc of non-struct type " + Ꮡt.String());
    }
    var tt = Ꮡt.Reinterpret<rtype, structType>();
    return tt.FieldByNameFunc(match);
}

// go2cs generated this placeholder — func Key is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Len is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func NumField is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func In is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func NumIn is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func NumOut is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Out is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func IsVariadic is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static bool OverflowComplex(this ж<rtype> Ꮡt, complex128 x) {
    ref var t = ref Ꮡt.DerefOrNull();

    ΔKind k = t.Kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == Complex64) {
        return overflowFloat32(real(x)) || overflowFloat32(imag(x));
    }
    if (exprᴛ1 == Complex128) {
        return false;
    }

    throw panic("reflect: OverflowComplex of non-complex type " + Ꮡt.String());
}

internal static bool OverflowFloat(this ж<rtype> Ꮡt, float64 x) {
    ref var t = ref Ꮡt.DerefOrNull();

    ΔKind k = t.Kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == Float32) {
        return overflowFloat32(x);
    }
    if (exprᴛ1 == Float64) {
        return false;
    }

    throw panic("reflect: OverflowFloat of non-float type " + Ꮡt.String());
}

internal static bool OverflowInt(this ж<rtype> Ꮡt, int64 x) {
    ref var t = ref Ꮡt.DerefOrNull();

    ΔKind k = t.Kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64) {
        var bitSize = t.Size() * 8;
        var trunc = (x.Lsh((uint64)((64 - bitSize)))).Rsh((uint64)((64 - bitSize)));
        return x != trunc;
    }

    throw panic("reflect: OverflowInt of non-int type " + Ꮡt.String());
}

internal static bool OverflowUint(this ж<rtype> Ꮡt, uint64 x) {
    ref var t = ref Ꮡt.DerefOrNull();

    ΔKind k = t.Kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == ΔUint || exprᴛ1 == Uintptr || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64) {
        var bitSize = t.Size() * 8;
        var trunc = (x.Lsh((uint64)((64 - bitSize)))).Rsh((uint64)((64 - bitSize)));
        return x != trunc;
    }

    throw panic("reflect: OverflowUint of non-uint type " + Ꮡt.String());
}

internal static bool CanSeq(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64 || exprᴛ1 == ΔInt || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == ΔUint || exprᴛ1 == Uintptr || exprᴛ1 == Array || exprᴛ1 == ΔSlice || exprᴛ1 == Chan || exprᴛ1 == ΔString || exprᴛ1 == Map) {
        return true;
    }
    if (exprᴛ1 == Func) {
        return canRangeFunc(Ꮡt.of(rtype.Ꮡt));
    }
    if (exprᴛ1 == ΔPointer) {
        return Ꮡt.Elem().Kind() == Array;
    }

    return false;
}

internal static bool canRangeFunc(ж<abi.Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != abi.Func) {
        return false;
    }
    var f = Ꮡt.FuncType();
    if ((~f).InCount != 1 || (~f).OutCount != 0) {
        return false;
    }
    var y = f.In(0);
    if (y.Kind() != abi.Func) {
        return false;
    }
    var yield = y.FuncType();
    return (~yield).InCount == 1 && (~yield).OutCount == 1 && yield.Out(0).Kind() == abi.Bool;
}

internal static bool CanSeq2(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == Array || exprᴛ1 == ΔSlice || exprᴛ1 == ΔString || exprᴛ1 == Map) {
        return true;
    }
    if (exprᴛ1 == Func) {
        return canRangeFunc2(Ꮡt.of(rtype.Ꮡt));
    }
    if (exprᴛ1 == ΔPointer) {
        return Ꮡt.Elem().Kind() == Array;
    }

    return false;
}

internal static bool canRangeFunc2(ж<abi.Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != abi.Func) {
        return false;
    }
    var f = Ꮡt.FuncType();
    if ((~f).InCount != 1 || (~f).OutCount != 0) {
        return false;
    }
    var y = f.In(0);
    if (y.Kind() != abi.Func) {
        return false;
    }
    var yield = y.FuncType();
    return (~yield).InCount == 2 && (~yield).OutCount == 1 && yield.Out(0).Kind() == abi.Bool;
}

// add returns p+x.
//
// The whySafe string is ignored, so that the function still inlines
// as efficiently as p+x, but all call sites should use the string to
// record why the addition is safe, which is to say why the addition
// does not cause x to advance to the very end of p's allocation
// and therefore point incorrectly at the next block in memory.
//
// add should be an internal detail (and is trivially copyable),
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/pinpoint-apm/pinpoint-go-agent
//   - github.com/vmware/govmomi
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname add
internal static @unsafe.Pointer add(@unsafe.Pointer p, uintptr x, @string whySafe) {
    return (@unsafe.Pointer)((uintptr)p + x);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string chanˢ = "chan<-"u8;
internal static readonly @string chanˢ2 = "<-chan"u8;
internal static readonly @string chanˢ3 = "chan"u8;

public static @string String(this ΔChanDir d) {
    var exprᴛ1 = d;
    if (exprᴛ1 == SendDir) {
        return chanˢ;
    }
    if (exprᴛ1 == RecvDir) {
        return chanˢ2;
    }
    if (exprᴛ1 == BothDir) {
        return chanˢ3;
    }

    return "ChanDir"u8 + strconv.Itoa((nint)d);
}

// Method returns the i'th method in the type's method set.
internal static ΔMethod /*m*/ Method(this ж<interfaceType> Ꮡt, nint i) {
    ΔMethod m = new();

    ref var t = ref Ꮡt.DerefOrNull();
    if (i < 0 || i >= len(t.Methods)) {
        return m;
    }
    var p = Ꮡ(t.Methods, i);
    var pname = Ꮡt.nameOff((~p).Name);
    m.Name = pname.Name();
    if (!pname.IsExported()) {
        m.PkgPath = pkgPath(pname);
        if (m.PkgPath == ""u8) {
            m.PkgPath = t.PkgPath.Name();
        }
    }
    m.Type = toType(Ꮡt.typeOff((~p).Typ));
    m.Index = i;
    return m;
}

// NumMethod returns the number of interface methods in the type's method set.
[GoRecv] internal static nint NumMethod(this ref interfaceType t) {
    return len(t.Methods);
}

// MethodByName method with the given name in the type's method set.
internal static (ΔMethod m, bool ok) MethodByName(this ж<interfaceType> Ꮡt, @string name) {
    ΔMethod m = new();
    bool ok = default!;

    ref var t = ref Ꮡt.DerefOrNull();
    if (Ꮡt == nil) {
        return (m, ok);
    }
    ж<abi.Imethod> p = default!;
    foreach (var (i, _) in t.Methods) {
        p = Ꮡ(t.Methods, i);
        if (Ꮡt.nameOff((~p).Name).Name() == name) {
            return (Ꮡt.Method(i), true);
        }
    }
    return (m, ok);
}

// A StructField describes a single field in a struct.
[GoType] partial struct StructField {
    // Name is the field name.
    public @string Name;
    // PkgPath is the package path that qualifies a lower case (unexported)
    // field name. It is empty for upper case (exported) field names.
    // See https://golang.org/ref/spec#Uniqueness_of_identifiers
    public @string PkgPath;
    public ΔType Type;      // field type
    public StructTag Tag; // field tag string
    public uintptr Offset;   // offset within struct, in bytes
    public slice<nint> Index; // index sequence for Type.FieldByIndex
    public bool Anonymous;      // is an embedded field
}

// IsExported reports whether the field is exported.
public static bool IsExported(this StructField f) {
    return f.PkgPath == ""u8;
}

[GoType("@string")] partial struct StructTag;

// Get returns the value associated with key in the tag string.
// If there is no such key in the tag, Get returns the empty string.
// If the tag does not have the conventional format, the value
// returned by Get is unspecified. To determine whether a tag is
// explicitly set to the empty string, use [StructTag.Lookup].
public static @string Get(this StructTag tag, @string key) {
    var (v, _) = tag.Lookup(key);
    return v;
}

// Lookup returns the value associated with key in the tag string.
// If the key is present in the tag the value (which may be empty)
// is returned. Otherwise the returned value will be the empty string.
// The ok return value reports whether the value was explicitly set in
// the tag string. If the tag does not have the conventional format,
// the value returned by Lookup is unspecified.
public static (@string value, bool ok) Lookup(this StructTag tag, @string key) {
    // When modifying this code, also update the validateStructTag code
    // in cmd/vet/structtag.go.
    while (tag != ""u8) {
        // Skip leading space.
        nint i = 0;
        while (i < len(tag) && tag[i] == (rune)' ') {
            i++;
        }
        tag = tag[(int)(i)..];
        if (tag == ""u8) {
            break;
        }
        // Scan to colon. A space, a quote or a control character is a syntax error.
        // Strictly speaking, control chars include the range [0x7f, 0x9f], not just
        // [0x00, 0x1f], but in practice, we ignore the multi-byte control characters
        // as it is simpler to inspect the tag's bytes than the tag's runes.
        i = 0;
        while (i < len(tag) && tag[i] > (rune)' ' && tag[i] != (rune)':' && tag[i] != (rune)'"' && tag[i] != 0x7f) {
            i++;
        }
        if (i == 0 || i + 1 >= len(tag) || tag[i] != (rune)':' || tag[i + 1] != (rune)'"') {
            break;
        }
        @string name = ((@string)(tag[..(int)(i)]));
        tag = tag[(int)(i + 1)..];
        // Scan quoted string to find value.
        i = 1;
        while (i < len(tag) && tag[i] != (rune)'"') {
            if (tag[i] == (rune)'\\') {
                i++;
            }
            i++;
        }
        if (i >= len(tag)) {
            break;
        }
        @string qvalue = ((@string)(tag[..(int)(i + 1)]));
        tag = tag[(int)(i + 1)..];
        if (key == name) {
            var (valueΔ1, err) = strconv.Unquote(qvalue);
            if (err != default!) {
                break;
            }
            return (valueΔ1, true);
        }
    }
    return ("", false);
}

// Field returns the i'th struct field.
[GoRecv] internal static StructField /*f*/ Field(this ref structType t, nint i) {
    StructField f = default!;

    if (i < 0 || i >= len(t.Fields)) {
        throw panic("reflect: Field index out of bounds");
    }
    var p = Ꮡ(t.Fields, i);
    f.Type = toType((~p).Typ);
    f.Name = (~p).Name.Name();
    f.Anonymous = p.Embedded();
    if (!(~p).Name.IsExported()) {
        f.PkgPath = t.PkgPath.Name();
    }
    {
        @string tag = (~p).Name.Tag(); if (tag != ""u8) {
            f.Tag = ((StructTag)tag);
        }
    }
    f.Offset = p.Value.Offset;
    // NOTE(rsc): This is the only allocation in the interface
    // presented by a reflect.Type. It would be nice to avoid,
    // at least in the common cases, but we need to make sure
    // that misbehaving clients of reflect cannot affect other
    // uses of reflect. One possibility is CL 5371098, but we
    // postponed that ugliness until there is a demonstrated
    // need for the performance. This is issue 2320.
    f.Index = new nint[]{i}.slice();
    return f;
}

// TODO(gri): Should there be an error/bool indicator if the index
// is wrong for FieldByIndex?

// FieldByIndex returns the nested field corresponding to index.
internal static StructField /*f*/ FieldByIndex(this ж<structType> Ꮡt, slice<nint> index) {
    StructField f = default!;

    f.Type = toType(Ꮡt.of(structType.ᏑType));
    foreach (var (i, x) in index) {
        if (i > 0) {
            var ft = f.Type;
            if (ft.Kind() == ΔPointer && ft.Elem().Kind() == Struct) {
                ft = ft.Elem();
            }
            f.Type = ft;
        }
        f = f.Type.Field(x);
    }
    return f;
}

// A fieldScan represents an item on the fieldByNameFunc scan work list.
[GoType] partial struct fieldScan {
    internal ж<structType> typ;
    internal slice<nint> index;
}

// FieldByNameFunc returns the struct field with a name that satisfies the
// match function and a boolean to indicate if the field was found.
internal static (StructField result, bool ok) FieldByNameFunc(this ж<structType> Ꮡt, Func<@string, bool> match) {
    StructField result = default!;
    bool ok = default!;

    ref var t = ref Ꮡt.DerefOrNull();
    // This uses the same condition that the Go language does: there must be a unique instance
    // of the match at a given depth level. If there are multiple instances of a match at the
    // same depth, they annihilate each other and inhibit any possible match at a lower level.
    // The algorithm is breadth first search, one depth level at a time.
    // The current and next slices are work queues:
    // current lists the fields to visit on this depth level,
    // and next lists the fields on the next lower level.
    var current = new fieldScan[]{}.slice();
    var next = new fieldScan[]{new(typ: Ꮡt)}.slice();
    // nextCount records the number of times an embedded type has been
    // encountered and considered for queueing in the 'next' slice.
    // We only queue the first one, but we increment the count on each.
    // If a struct type T can be reached more than once at a given depth level,
    // then it annihilates itself and need not be considered at all when we
    // process that next depth level.
    map<ж<structType>, nint> nextCount = default!;
    // visited records the structs that have been considered already.
    // Embedded pointer fields can create cycles in the graph of
    // reachable embedded types; visited avoids following those cycles.
    // It also avoids duplicated effort: if we didn't find the field in an
    // embedded type T at level 2, we won't find it in one at level 4 either.
    var visited = new map<ж<structType>, bool>{};
    while (len(next) > 0) {
        (current, next) = (next, current[..0]);
        var count = nextCount;
        nextCount = default!;
        // Process all the fields at this depth, now listed in 'current'.
        // The loop queues embedded fields found in 'next', for processing during the next
        // iteration. The multiplicity of the 'current' field counts is recorded
        // in 'count'; the multiplicity of the 'next' field counts is recorded in 'nextCount'.
        foreach (var (_, scan) in current) {
            var tΔ1 = scan.typ;
            if (visited[tΔ1]) {
                // We've looked through this type before, at a higher level.
                // That higher level would shadow the lower level we're now at,
                // so this one can't be useful to us. Ignore it.
                continue;
            }
            visited[tΔ1] = true;
            foreach (var (i, _) in (~tΔ1).Fields) {
                var f = Ꮡ((~tΔ1).Fields, i);
                // Find name and (for embedded field) type for field f.
                @string fname = (~f).Name.Name();
                ж<abi.Type> ntyp = default!;
                if (f.Embedded()) {
                    // Embedded field of type T or *T.
                    ntyp = f.Value.Typ;
                    if (ntyp.Kind() == abi.Pointer) {
                        ntyp = ntyp.Elem();
                    }
                }
                // Does it match?
                if (match(fname)) {
                    // Potential match
                    if (count[tΔ1] > 1 || ok) {
                        // Name appeared multiple times at this level: annihilate.
                        return (new StructField(nil), false);
                    }
                    result = tΔ1.Field(i);
                    result.Index = default!;
                    result.Index = builtin.appendꓸꓸꓸ(result.Index, scan.index);
                    result.Index = builtin.append(result.Index, i);
                    ok = true;
                    continue;
                }
                // Queue embedded struct fields for processing with next level,
                // but only if we haven't seen a match yet at this level and only
                // if the embedded types haven't already been queued.
                if (ok || ntyp == nil || ntyp.Kind() != abi.Struct) {
                    continue;
                }
                var styp = ntyp.Reinterpret<abi.Type, structType>();
                if (nextCount[styp] > 0) {
                    nextCount[styp] = 2; // exact multiple doesn't matter
                    continue;
                }
                if (nextCount == default!) {
                    nextCount = new map<ж<structType>, nint>{};
                }
                nextCount[styp] = 1;
                if (count[tΔ1] > 1) {
                    nextCount[styp] = 2; // exact multiple doesn't matter
                }
                slice<nint> index = default!;
                index = builtin.appendꓸꓸꓸ(index, scan.index);
                index = builtin.append(index, i);
                next = builtin.append(next, new fieldScan(styp, index));
            }
        }
        if (ok) {
            break;
        }
    }
    return (result, ok);
}

// FieldByName returns the struct field with the given name
// and a boolean to indicate if the field was found.
internal static (StructField f, bool present) FieldByName(this ж<structType> Ꮡt, @string name) {
    StructField f = default!;
    bool present = default!;

    ref var t = ref Ꮡt.DerefOrNull();
    // Quick check for top-level name, or struct without embedded fields.
    var hasEmbeds = false;
    if (name != ""u8) {
        foreach (var (i, _) in t.Fields) {
            var tf = Ꮡ(t.Fields, i);
            if ((~tf).Name.Name() == name) {
                return (t.Field(i), true);
            }
            if (tf.Embedded()) {
                hasEmbeds = true;
            }
        }
    }
    if (!hasEmbeds) {
        return (f, present);
    }
    return Ꮡt.FieldByNameFunc((@string s) => s == name);
}

// TypeOf returns the reflection [Type] that represents the dynamic type of i.
// If i is a nil interface value, TypeOf returns nil.
public static ΔType TypeOf(any i) {
    return toType(abi.TypeOf(i));
}

// rtypeOf directly extracts the *rtype of the provided value.
internal static ж<abi.Type> rtypeOf(any i) {
    return abi.TypeOf(i);
}

// ptrMap is the cache for PointerTo.
internal static ж<Δsync.Map> ᏑptrMap = new StandardBox<Δsync.Map>(default(Δsync.Map));
internal static ref Δsync.Map ptrMap => ref ᏑptrMap.Value; // map[*rtype]*ptrType

// PtrTo returns the pointer type with element t.
// For example, if t represents type Foo, PtrTo(t) represents *Foo.
//
// PtrTo is the old spelling of [PointerTo].
// The two functions behave identically.
//
// Deprecated: Superseded by [PointerTo].
public static ΔType PtrTo(ΔType t) {
    return PointerTo(t);
}

// go2cs generated this placeholder — func PointerTo is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static ж<abi.Type> ptrTo(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var at = Ꮡt.of(rtype.Ꮡt);
    if ((~at).PtrToThis != 0) {
        return Ꮡt.typeOff((~at).PtrToThis);
    }
    // Check the cache.
    {
        var (piΔ1, ok) = ᏑptrMap.Load(Ꮡt.OrTypedNil()); if (ok) {
            return piΔ1._<ж<ptrType>>().of(ptrType.ᏑType);
        }
    }
    // Look in known types.
    @string s = "*"u8 + Ꮡt.String();
    foreach (var (_, tt) in typesByString(s)) {
        var p = tt.Reinterpret<abi.Type, ptrType>();
        if ((~p).Elem != Ꮡt.of(rtype.Ꮡt)) {
            continue;
        }
        var (piΔ2, _) = ᏑptrMap.LoadOrStore(Ꮡt.OrTypedNil(), p.OrTypedNil());
        return piΔ2._<ж<ptrType>>().of(ptrType.ᏑType);
    }
    // Create a new ptrType starting with the description
    // of an *unsafe.Pointer.
    ref var iptr = ref heap<any>(out var Ꮡiptr);

    iptr = ((ж<@unsafe.Pointer>)nil);
    var prototype = ~Ꮡiptr.Reinterpret<any, ж<ptrType>>();
    ref var pp = ref heap<ptrType>(out var Ꮡpp);
    pp = prototype.Value;
    pp.Str = resolveReflectName(newName(s, ""u8, false, false));
    pp.PtrToThis = 0;
    // For the type structures linked into the binary, the
    // compiler provides a good hash of the string.
    // Create a good hash for the new string by using
    // the FNV-1 hash's mixing function to combine the
    // old hash and the new "*".
    pp.Hash = fnv1(t.t.Hash, (rune)'*');
    pp.Elem = at;
    var (pi, _) = ᏑptrMap.LoadOrStore(Ꮡt.OrTypedNil(), Ꮡpp);
    return pi._<ж<ptrType>>().of(ptrType.ᏑType);
}

internal static ж<abi.Type> ptrTo(ж<abi.Type> Ꮡt) {
    return toRType(Ꮡt).ptrTo();
}

// fnv1 incorporates the list of bytes into the hash x using the FNV-1 hash function.
internal static uint32 fnv1(uint32 x, params ꓸꓸꓸbyte listʗp) {
    var list = listʗp.sslice();

    foreach (var (_, b) in list) {
        x = (uint32)(x * 16777619 ^ (uint32)b);
    }
    return x;
}

// go2cs generated this placeholder — func Implements is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static bool AssignableTo(this ж<rtype> Ꮡt, ΔType u) {
    if (u == default!) {
        throw panic("reflect: nil type passed to Type.AssignableTo");
    }
    var uu = u.common();
    return directlyAssignable(uu, Ꮡt.common()) || implements(uu, Ꮡt.common());
}

internal static bool ConvertibleTo(this ж<rtype> Ꮡt, ΔType u) {
    if (u == default!) {
        throw panic("reflect: nil type passed to Type.ConvertibleTo");
    }
    return convertOp(u.common(), Ꮡt.common()) != default!;
}

[GoRecv] internal static bool Comparable(this ref rtype t) {
    return t.t.Equal != default!;
}

// go2cs generated this placeholder — func implements is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// specialChannelAssignability reports whether a value x of channel type V
// can be directly assigned (using memmove) to another channel type T.
// https://golang.org/doc/go_spec.html#Assignability
// T and V must be both of Chan kind.
internal static bool specialChannelAssignability(ж<abi.Type> ᏑT, ж<abi.Type> ᏑV) {
    ref var T = ref ᏑT.DerefOrNull();
    ref var V = ref ᏑV.DerefOrNull();

    // Special case:
    // x is a bidirectional channel value, T is a channel type,
    // x's type V and T have identical element types,
    // and at least one of V or T is not a defined type.
    return ᏑV.ChanDir() == abi.BothDir && (nameFor(ᏑT) == ""u8 || nameFor(ᏑV) == ""u8) && haveIdenticalType(ᏑT.Elem(), ᏑV.Elem(), true);
}

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
    if (T.Kind() == abi.Chan && specialChannelAssignability(ᏑT, ᏑV)) {
        return true;
    }
    // x's type T and V must have identical underlying types.
    return haveIdenticalUnderlyingType(ᏑT, ᏑV, true);
}

internal static bool haveIdenticalType(ж<abi.Type> ᏑT, ж<abi.Type> ᏑV, bool cmpTags) {
    ref var T = ref ᏑT.DerefOrNull();
    ref var V = ref ᏑV.DerefOrNull();

    if (cmpTags) {
        return ᏑT == ᏑV;
    }
    if (nameFor(ᏑT) != nameFor(ᏑV) || T.Kind() != V.Kind() || pkgPathFor(ᏑT) != pkgPathFor(ᏑV)) {
        return false;
    }
    return haveIdenticalUnderlyingType(ᏑT, ᏑV, false);
}

// go2cs generated this placeholder — func haveIdenticalUnderlyingType is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func typelinks is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sizeofRtype0ˢ = "sizeof(rtype) > 0"u8;

// rtypeOff should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/goccy/go-json
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname rtypeOff
internal static ж<abi.Type> rtypeOff(@unsafe.Pointer section, int32 off) {
    return (ж<abi.Type>)(uintptr)(add(section, (uintptr)off, sizeofRtype0ˢ));
}

// typesByString returns the subslice of typelinks() whose elements have
// the given string representation.
// It may be empty (no known types with that string) or may have
// multiple elements (multiple types with that string).
//
// typesByString should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/aristanetworks/goarista
//   - fortio.org/log
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname typesByString
internal static slice<ж<abi.Type>> typesByString(@string s) {
    var (sections, offset) = typelinks();
    slice<ж<abi.Type>> ret = default!;
    foreach (var (offsI, offs) in offset) {
        @unsafe.Pointer section = sections[offsI];
        // We are looking for the first index i where the string becomes >= s.
        // This is a copy of sort.Search, with f(h) replaced by (*typ[h].String() >= s).
        nint i = 0;
        nint j = len(offs);
        while (i < j) {
            nint h = (nint)(((nuint)(i + j) >> (int)(1))); // avoid overflow when computing h
            // i ≤ h < j
            if (!(stringFor(rtypeOff(section, offs[h])) >= s)){
                i = h + 1; // preserves f(i-1) == false
            } else {
                j = h; // preserves f(j) == true
            }
        }
        // i == j, f(i-1) == false, and f(j) (= f(i)) == true  =>  answer is i.
        // Having found the first, linear scan forward to find the last.
        // We could do a second binary search, but the caller is going
        // to do a linear scan anyway.
        for (nint jΔ1 = i; jΔ1 < len(offs); jΔ1++) {
            var typ = rtypeOff(section, offs[jΔ1]);
            if (stringFor(typ) != s) {
                break;
            }
            ret = builtin.append(ret, typ);
        }
    }
    return ret;
}

// The lookupCache caches ArrayOf, ChanOf, MapOf and SliceOf lookups.
internal static ж<Δsync.Map> ᏑlookupCache = new StandardBox<Δsync.Map>(default(Δsync.Map));
internal static ref Δsync.Map lookupCache => ref ᏑlookupCache.Value; // map[cacheKey]*rtype

// A cacheKey is the key for use in the lookupCache.
// Four values describe any of the types we are looking for:
// type kind, one or two subtypes, and an extra integer.
[GoType] partial struct cacheKey {
    internal ΔKind kind;
    internal ж<abi.Type> t1;
    internal ж<abi.Type> t2;
    internal uintptr extra;
}

// The funcLookupCache caches FuncOf lookups.
// FuncOf does not share the common lookupCache since cacheKey is not
// sufficient to represent functions unambiguously.

[GoType("dyn")] partial struct funcLookupCacheᴛ1 {
    public partial ref sync_package.Mutex Mutex { get; } // Guards stores (but not loads) on m.
    // m is a map[uint32][]*rtype keyed by the hash calculated in FuncOf.
    // Elements of m are append-only and thus safe for concurrent reading.
    internal Δsync.Map m;
}
internal static ж<funcLookupCacheᴛ1> ᏑfuncLookupCache = new StandardBox<funcLookupCacheᴛ1>(new funcLookupCacheᴛ1(nil));
internal static ref funcLookupCacheᴛ1 funcLookupCache => ref ᏑfuncLookupCache.Value;

// go2cs generated this placeholder — func ChanOf is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func MapOf is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static slice<ΔType> funcTypes;

internal static ж<Δsync.Mutex> ᏑfuncTypesMutex = new StandardBox<Δsync.Mutex>(default(Δsync.Mutex));
internal static ref Δsync.Mutex funcTypesMutex => ref ᏑfuncTypesMutex.Value;

internal static ΔType initFuncTypes(nint n) {
    GoFrame ᒐ = default;
    try {
        ᏑfuncTypesMutex.Lock();
        defer(ᏑfuncTypesMutex.Unlock, ref ᒐ);
        if (n >= len(funcTypes)) {
            var newFuncTypes = new slice<ΔType>(n + 1);
            copy(newFuncTypes, funcTypes);
            funcTypes = newFuncTypes;
        }
        if (funcTypes[n] != default!) {
            return funcTypes[n];
        }
        funcTypes[n] = StructOf(new StructField[]{
            new(
                Name: "FuncType"u8,
                Type: TypeOf(new abiꓸFuncType())
            ),
            new(
                Name: "Args"u8,
                Type: ArrayOf(n, TypeOf(Ꮡ(new rtype(nil))))
            )
        }.slice());
        return funcTypes[n];
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// go2cs generated this placeholder — func FuncOf is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static @string stringFor(ж<abi.Type> Ꮡt) {
    return toRType(Ꮡt).String();
}

// funcStr builds a string representation of a funcType.
internal static @string funcStr(ж<funcType> Ꮡft) {
    ref var ft = ref Ꮡft.DerefOrNull();

    var repr = new slice<byte>(0, 64);
    repr = builtin.append(repr, ((@string)"func("u8).ꓸꓸꓸ);
    foreach (var (i, t) in Ꮡft.InSlice()) {
        if (i > 0) {
            repr = builtin.append(repr, ((@string)", "u8).ꓸꓸꓸ);
        }
        if (ft.IsVariadic() && i == (nint)ft.InCount - 1){
            repr = builtin.append(repr, ((@string)"..."u8).ꓸꓸꓸ);
            repr = builtin.append(repr, stringFor((t.Reinterpret<abi.Type, sliceType>()).Value.Elem).ꓸꓸꓸ);
        } else {
            repr = builtin.append(repr, stringFor(t).ꓸꓸꓸ);
        }
    }
    repr = builtin.append(repr, (byte)((rune)')'));
    var @out = Ꮡft.OutSlice();
    if (len(@out) == 1){
        repr = builtin.append(repr, (byte)((rune)' '));
    } else 
    if (len(@out) > 1) {
        repr = builtin.append(repr, ((@string)" ("u8).ꓸꓸꓸ);
    }
    foreach (var (i, t) in @out) {
        if (i > 0) {
            repr = builtin.append(repr, ((@string)", "u8).ꓸꓸꓸ);
        }
        repr = builtin.append(repr, stringFor(t).ꓸꓸꓸ);
    }
    if (len(@out) > 1) {
        repr = builtin.append(repr, (byte)((rune)')'));
    }
    return ((@string)repr);
}

// isReflexive reports whether the == operation on the type is reflexive.
// That is, x == x for all values x of type t.
internal static bool isReflexive(ж<abi.Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var exprᴛ1 = ((ΔKind)(nuint)(uint8)t.Kind());
    if (exprᴛ1 == ΔBool || exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64 || exprᴛ1 == ΔUint || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == Uintptr || exprᴛ1 == Chan || exprᴛ1 == ΔPointer || exprᴛ1 == ΔString || exprᴛ1 == ΔUnsafePointer) {
        return true;
    }
    if (exprᴛ1 == Float32 || exprᴛ1 == Float64 || exprᴛ1 == Complex64 || exprᴛ1 == Complex128 || exprᴛ1 == ΔInterface) {
        return false;
    }
    if (exprᴛ1 == Array) {
        var tt = Ꮡt.Reinterpret<abi.Type, arrayType>();
        return isReflexive((~tt).Elem);
    }
    if (exprᴛ1 == Struct) {
        var tt = Ꮡt.Reinterpret<abi.Type, structType>();
        foreach (var (_, f) in (~tt).Fields) {
            if (!isReflexive(f.Typ)) {
                return false;
            }
        }
        return true;
    }
    { /* default: */
        throw panic("isReflexive called on non-key type " + stringFor(Ꮡt));
    }

}

// Func, Map, Slice, Invalid

// needKeyUpdate reports whether map overwrites require the key to be copied.
internal static bool needKeyUpdate(ж<abi.Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var exprᴛ1 = ((ΔKind)(nuint)(uint8)t.Kind());
    if (exprᴛ1 == ΔBool || exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64 || exprᴛ1 == ΔUint || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == Uintptr || exprᴛ1 == Chan || exprᴛ1 == ΔPointer || exprᴛ1 == ΔUnsafePointer) {
        return false;
    }
    if (exprᴛ1 == Float32 || exprᴛ1 == Float64 || exprᴛ1 == Complex64 || exprᴛ1 == Complex128 || exprᴛ1 == ΔInterface || exprᴛ1 == ΔString) {
        return true;
    }
    if (exprᴛ1 == Array) {
        var tt = Ꮡt.Reinterpret<abi.Type, arrayType>();
        return needKeyUpdate((~tt).Elem);
    }
    if (exprᴛ1 == Struct) {
        var tt = Ꮡt.Reinterpret<abi.Type, structType>();
        foreach (var (_, f) in (~tt).Fields) {
            // Float keys can be updated from +0 to -0.
            // String keys can be updated to use a smaller backing store.
            // Interfaces might have floats or strings in them.
            if (needKeyUpdate(f.Typ)) {
                return true;
            }
        }
        return false;
    }
    { /* default: */
        throw panic("needKeyUpdate called on non-key type " + stringFor(Ꮡt));
    }

}

// Func, Map, Slice, Invalid

// hashMightPanic reports whether the hash of a map key of type t might panic.
internal static bool hashMightPanic(ж<abi.Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var exprᴛ1 = ((ΔKind)(nuint)(uint8)t.Kind());
    if (exprᴛ1 == ΔInterface) {
        return true;
    }
    if (exprᴛ1 == Array) {
        var tt = Ꮡt.Reinterpret<abi.Type, arrayType>();
        return hashMightPanic((~tt).Elem);
    }
    if (exprᴛ1 == Struct) {
        var tt = Ꮡt.Reinterpret<abi.Type, structType>();
        foreach (var (_, f) in (~tt).Fields) {
            if (hashMightPanic(f.Typ)) {
                return true;
            }
        }
        return false;
    }
    { /* default: */
        return false;
    }

}

internal static ж<abi.Type> bucketOf(ж<abi.Type> Ꮡktyp, ж<abi.Type> Ꮡetyp) {
    ref var ktyp = ref Ꮡktyp.DerefOrNull();
    ref var etyp = ref Ꮡetyp.DerefOrNull();

    if (ktyp.Size_ > abi.MapMaxKeyBytes) {
        Ꮡktyp = ptrTo(Ꮡktyp); ktyp = ref Ꮡktyp.DerefOrNull();
    }
    if (etyp.Size_ > abi.MapMaxElemBytes) {
        Ꮡetyp = ptrTo(Ꮡetyp); etyp = ref Ꮡetyp.DerefOrNull();
    }
    // Prepare GC data if any.
    // A bucket is at most bucketSize*(1+maxKeySize+maxValSize)+ptrSize bytes,
    // or 2064 bytes, or 258 pointer-size words, or 33 bytes of pointer bitmap.
    // Note that since the key and value are known to be <= 128 bytes,
    // they're guaranteed to have bitmaps instead of GC programs.
    ж<byte> gcdata = default!;
    ref var ptrdata = ref heap(new uintptr(), out var Ꮡptrdata);
    ref var size = ref heap<uintptr>(out var Ꮡsize);
    size = (uintptr)abi.MapBucketCount * (1 + ktyp.Size_ + etyp.Size_) + (uintptr)goarch.PtrSize;
    if ((uintptr)(size & (uintptr)(ktyp.Align_ - 1)) != 0 || (uintptr)(size & (uintptr)(etyp.Align_ - 1)) != 0) {
        throw panic("reflect: bad size computation in MapOf");
    }
    if (ktyp.Pointers() || etyp.Pointers()) {
        var nptr = ((uintptr)abi.MapBucketCount * (1 + ktyp.Size_ + etyp.Size_) + (uintptr)goarch.PtrSize) / (uintptr)goarch.PtrSize;
        var n = (nptr + 7) / 8;
        // Runtime needs pointer masks to be a multiple of uintptr in size.
        n = (uintptr)((n + (uintptr)goarch.PtrSize - 1) & ~(uintptr)(goarch.PtrSize - 1));
        var mask = new slice<byte>((nint)(n));
        var @base = (uintptr)(abi.MapBucketCount / goarch.PtrSize);
        if (ktyp.Pointers()) {
            emitGCMask(mask, @base, Ꮡktyp, abi.MapBucketCount);
        }
        @base += (uintptr)abi.MapBucketCount * ktyp.Size_ / (uintptr)goarch.PtrSize;
        if (etyp.Pointers()) {
            emitGCMask(mask, @base, Ꮡetyp, abi.MapBucketCount);
        }
        @base += (uintptr)abi.MapBucketCount * etyp.Size_ / (uintptr)goarch.PtrSize;
        var word = @base;
        mask[(nint)(word / 8)] |= (byte)((byte)(1 << (int)((word % 8))));
        gcdata = Ꮡ(mask, 0);
        ptrdata = (word + 1) * (uintptr)goarch.PtrSize;
        // overflow word must be last
        if (ptrdata != size) {
            throw panic("reflect: bad layout computation in MapOf");
        }
    }
    var b = Ꮡ(new abi.Type(
        Align_: goarch.PtrSize,
        Size_: size,
        Kind_: abi.Struct,
        PtrBytes: ptrdata,
        GCData: gcdata
    ));
    @string s = "bucket("u8 + stringFor(Ꮡktyp) + ","u8 + stringFor(Ꮡetyp) + ")"u8;
    b.Value.Str = resolveReflectName(newName(s, ""u8, false, false));
    return b;
}

[GoRecv] internal static slice<byte> gcSlice(this ref rtype t, uintptr begin, uintptr end) {
    return (~array<byte>.AliasPointer(t.t.GCData, 1073741824)).slice((int)(begin), (int)(end), (int)(end));
}

// emitGCMask writes the GC mask for [n]typ into out, starting at bit
// offset base.
internal static void emitGCMask(slice<byte> @out, uintptr @base, ж<abi.Type> Ꮡtyp, uintptr n) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if ((abiꓸKind)(typ.Kind_ & abi.KindGCProg) != 0) {
        throw panic("reflect: unexpected GC program");
    }
    var ptrs = typ.PtrBytes / (uintptr)goarch.PtrSize;
    var words = typ.Size_ / (uintptr)goarch.PtrSize;
    var mask = typ.GcSlice(0, (ptrs + 7) / 8);
    for (var j = (uintptr)0; j < ptrs; j++) {
        if ((byte)(((mask[(nint)(j / 8)] >> (int)((j % 8)))) & 1) != 0) {
            for (var i = (uintptr)0; i < n; i++) {
                var k = @base + i * words + j;
                @out[(nint)(k / 8)] |= (byte)((byte)(1 << (int)((k % 8))));
            }
        }
    }
}

// appendGCProg appends the GC program for the first ptrdata bytes of
// typ to dst and returns the extended slice.
internal static slice<byte> appendGCProg(slice<byte> dst, ж<abi.Type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if ((abiꓸKind)(typ.Kind_ & abi.KindGCProg) != 0) {
        // Element has GC program; emit one element.
        var n = (uintptr)(~typ.GCData.Reinterpret<byte, uint32>());
        var prog = typ.GcSlice(4, 4 + n - 1);
        return builtin.appendꓸꓸꓸ(dst, prog);
    }
    // Element is small with pointer mask; use as literal bits.
    var ptrs = typ.PtrBytes / (uintptr)goarch.PtrSize;
    var mask = typ.GcSlice(0, (ptrs + 7) / 8);
    // Emit 120-bit chunks of full bytes (max is 127 but we avoid using partial bytes).
    for (; ptrs > 120; ptrs -= 120) {
        dst = builtin.append(dst, (byte)(120));
        dst = builtin.appendꓸꓸꓸ(dst, mask[..15]);
        mask = mask[15..];
    }
    dst = builtin.append(dst, (byte)ptrs);
    dst = builtin.appendꓸꓸꓸ(dst, mask);
    return dst;
}

// go2cs generated this placeholder — func SliceOf is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// The structLookupCache caches StructOf lookups.
// StructOf does not share the common lookupCache since we need to pin
// the memory associated with *structTypeFixedN.
internal static ж<funcLookupCacheᴛ1> ᏑstructLookupCache = new StandardBox<funcLookupCacheᴛ1>(new funcLookupCacheᴛ1(nil));
internal static ref funcLookupCacheᴛ1 structLookupCache => ref ᏑstructLookupCache.Value;

[GoType] partial struct structTypeUncommon {
    internal partial ref structType structType { get; }
    internal uncommonType u;
}

// isLetter reports whether a given 'rune' is classified as a Letter.
internal static bool isLetter(rune ch) {
    return (rune)'a' <= ch && ch <= (rune)'z' || (rune)'A' <= ch && ch <= (rune)'Z' || ch == (rune)'_' || ch >= utf8.RuneSelf && Δunicode.IsLetter(ch);
}

// isValidFieldName checks if a string is a valid (struct) field name or not.
//
// According to the language spec, a field name should be an identifier.
//
// identifier = letter { letter | unicode_digit } .
// letter = unicode_letter | "_" .
internal static bool isValidFieldName(@string fieldName) {
    foreach (var (i, c) in fieldName) {
        if (i == 0 && !isLetter(c)) {
            return false;
        }
        if (!(isLetter(c) || Δunicode.IsDigit(c))) {
            return false;
        }
    }
    return len(fieldName) > 0;
}

// This must match cmd/compile/internal/compare.IsRegularMemory
internal static bool isRegularMemory(ΔType t) {
    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == Array) {
        var elem = t.Elem();
        if (isRegularMemory(elem)) {
            return true;
        }
        return elem.Comparable() && t.Len() == 0;
    }
    if (exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64 || exprᴛ1 == ΔInt || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == ΔUint || exprᴛ1 == Uintptr || exprᴛ1 == Chan || exprᴛ1 == ΔPointer || exprᴛ1 == ΔBool || exprᴛ1 == ΔUnsafePointer) {
        return true;
    }
    if (exprᴛ1 == Struct) {
        nint num = t.NumField();
        switch (num) {
        case 0: {
            return true;
        }
        case 1: {
            var field = t.Field(0);
            if (field.Name == "_"u8) {
                return false;
            }
            return isRegularMemory(field.Type);
        }
        default: {
            foreach (var i in range(num)) {
                var field = t.Field(i);
                if (field.Name == "_"u8 || !isRegularMemory(field.Type) || isPaddedField(t, i)) {
                    return false;
                }
            }
            return true;
        }}

    }

    return false;
}

// isPaddedField reports whether the i'th field of struct type t is followed
// by padding.
internal static bool isPaddedField(ΔType t, nint i) {
    var field = t.Field(i);
    if (i + 1 < t.NumField()) {
        return field.Offset + field.Type.Size() != t.Field(i + 1).Offset;
    }
    return field.Offset + field.Type.Size() != t.Size();
}

// go2cs generated this placeholder — func StructOf is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static void embeddedIfaceMethStub() {
    throw panic("reflect: StructOf does not support methods of embedded interfaces");
}

// runtimeStructField takes a StructField value passed to StructOf and
// returns both the corresponding internal representation, of type
// structField, and the pkgpath value to use for this field.
internal static (structField, @string) runtimeStructField(StructField field) {
    if (field.Anonymous && field.PkgPath != ""u8) {
        throw panic("reflect.StructOf: field \"" + field.Name + "\" is anonymous but has PkgPath set");
    }
    if (field.IsExported()) {
        // Best-effort check for misuse.
        // Since this field will be treated as exported, not much harm done if Unicode lowercase slips through.
        var c = field.Name[0];
        if ((rune)'a' <= c && c <= (rune)'z' || c == (rune)'_') {
            throw panic("reflect.StructOf: field \"" + field.Name + "\" is unexported but missing PkgPath");
        }
    }
    resolveReflectType(field.Type.common()); // install in runtime
    var f = new structField(
        Name: newName(field.Name, ((@string)field.Tag), field.IsExported(), field.Anonymous),
        Typ: field.Type.common(),
        Offset: 0
    );
    return (f, field.PkgPath);
}

// typeptrdata returns the length in bytes of the prefix of t
// containing pointer data. Anything after this offset is scalar data.
// keep in sync with ../cmd/compile/internal/reflectdata/reflect.go
internal static uintptr typeptrdata(ж<abi.Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == abi.Struct) {
        var st = Ꮡt.Reinterpret<abi.Type, structType>();
        nint field = -1;
        foreach (var (i, _) in (~st).Fields) {
            // find the last field that has pointers.
            var ft = (~st).Fields[i].Typ;
            if (ft.Pointers()) {
                field = i;
            }
        }
        if (field == -1) {
            return 0;
        }
        var f = (~st).Fields[field];
        return f.Offset + (~f.Typ).PtrBytes;
    }
    { /* default: */
        throw panic("reflect.typeptrdata: unexpected type, " + stringFor(Ꮡt));
    }

}

// go2cs generated this placeholder — func ArrayOf is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static slice<byte> appendVarint(slice<byte> x, uintptr v) {
    for (; v >= 0x80; v >>= (int)(7)) {
        x = builtin.append(x, (byte)((uintptr)(v | 0x80)));
    }
    x = builtin.append(x, (byte)v);
    return x;
}

// go2cs generated this placeholder — func toType is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

[GoType] partial struct layoutKey {
    internal ж<funcType> ftyp; // function signature
    internal ж<abi.Type> rcvr; // receiver type, or nil if none
}

[GoType] partial struct layoutType {
    internal ж<abi.Type> t;
    internal ж<Δsync.Pool> framePool;
    internal abiDesc abid;
}

internal static ж<Δsync.Map> ᏑlayoutCache = new StandardBox<Δsync.Map>(default(Δsync.Map));
internal static ref Δsync.Map layoutCache => ref ᏑlayoutCache.Value; // map[layoutKey]layoutType

// funcLayout computes a struct type representing the layout of the
// stack-assigned function arguments and return values for the function
// type t.
// If rcvr != nil, rcvr specifies the type of the receiver.
// The returned type exists only for GC, so we only fill out GC relevant info.
// Currently, that's just size and the GC program. We also fill in
// the name for possible debugging use.
internal static (ж<abi.Type> frametype, ж<Δsync.Pool> framePool, abiDesc abid) funcLayout(ж<funcType> Ꮡt, ж<abi.Type> Ꮡrcvr) {
    ж<Δsync.Pool> framePool = default!;
    abiDesc abid = new();

    ref var t = ref Ꮡt.DerefOrNull();
    ref var rcvr = ref Ꮡrcvr.DerefOrNull();
    if (Ꮡt.of(funcType.ᏑType).Kind() != abi.Func) {
        throw panic("reflect: funcLayout of non-func type " + stringFor(Ꮡt.of(funcType.ᏑType)));
    }
    if (Ꮡrcvr != nil && rcvr.Kind() == abi.Interface) {
        throw panic("reflect: funcLayout with interface receiver " + stringFor(Ꮡrcvr));
    }
    var k = new layoutKey(Ꮡt, Ꮡrcvr);
    {
        var (ltiΔ1, ok) = ᏑlayoutCache.Load(k); if (ok) {
            var ltΔ1 = ltiΔ1._<layoutType>();
            return (ltΔ1.t, ltΔ1.framePool, ltΔ1.abid.ΔClone());
        }
    }
    // Compute the ABI layout.
    abid = newAbiDesc(Ꮡt, Ꮡrcvr);
    // build dummy rtype holding gc program
    var x = Ꮡ(new abi.Type(
        Align_: goarch.PtrSize, // Don't add spill space here; it's only necessary in
 // reflectcall's frame, not in the allocated frame.
 // TODO(mknyszek): Remove this comment when register
 // spill space in the frame is no longer required.

        Size_: align(abid.retOffset + abid.ret.stackBytes, goarch.PtrSize),
        PtrBytes: (uintptr)(~abid.stackPtrs).n * (uintptr)goarch.PtrSize
    ));
    if ((~abid.stackPtrs).n > 0) {
        x.Value.GCData = Ꮡ((~abid.stackPtrs).data, 0);
    }
    @string s = default!;
    if (Ꮡrcvr != nil){
        s = "methodargs("u8 + stringFor(Ꮡrcvr) + ")("u8 + stringFor(Ꮡt.of(funcType.ᏑType)) + ")"u8;
    } else {
        s = "funcargs("u8 + stringFor(Ꮡt.of(funcType.ᏑType)) + ")"u8;
    }
    x.Value.Str = resolveReflectName(newName(s, ""u8, false, false));
    // cache result for future callers
    var xʗ1 = x;
    framePool = Ꮡ(new Δsync.Pool(New: () => (uintptr)unsafe_New(xʗ1)
    ));
    var (lti, _) = ᏑlayoutCache.LoadOrStore(k, new layoutType(
        t: x,
        framePool: framePool,
        abid: abid.ΔClone()
    ));
    var lt = lti._<layoutType>();
    return (lt.t, lt.framePool, lt.abid.ΔClone());
}

// Note: this type must agree with runtime.bitvector.
[GoType] partial struct bitVector {
    internal uint32 n; // number of bits
    internal slice<byte> data;
}

// append a bit to the bitmap.
[GoRecv] internal static void append(this ref bitVector bv, uint8 bit) {
    if (bv.n % (uint32)(8 * goarch.PtrSize) == 0) {
        // Runtime needs pointer masks to be a multiple of uintptr in size.
        // Since reflect passes bv.data directly to the runtime as a pointer mask,
        // we append a full uintptr of zeros at a time.
        for (nint i = 0; i < goarch.PtrSize; i++) {
            bv.data = builtin.append(bv.data, (byte)(0));
        }
    }
    bv.data[(nint)(bv.n / 8)] |= (uint8)((uint8)(bit << (int)((bv.n % 8))));
    bv.n++;
}

// go2cs generated this placeholder — func addTypeBits is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// TypeFor returns the [Type] that represents the type argument T.
public static ΔType TypeFor<T>() {
    T v = default!;
    {
        var t = TypeOf(v); if (t != default!) {
            return t; // optimize for T being a non-interface kind
        }
    }
    return TypeOf(((ж<T>)nil)).Elem(); // only for an interface kind
}

} // end reflect_package
