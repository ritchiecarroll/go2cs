// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using @unsafe = unsafe_package;

partial class abi_package {

// Type is the runtime representation of a Go type.
//
// Be careful about accessing this type at build time, as the version
// of this type in the compiler/linker may not have the same layout
// as the version in the target binary, due to pointer width
// differences and any experiments. Use cmd/compile/internal/rttype
// or the functions in compiletype.go to access this type instead.
// (TODO: this admonition applies to every type in this package.
// Put it in some shared location?)
[GoType] partial struct Type {
    public uintptr Size_;
    public uintptr PtrBytes; // number of (prefix) bytes in the type that can contain pointers
    public uint32 Hash;  // hash of type; avoids computation in hash tables
    public TFlag TFlag;   // extra type information flags
    public uint8 Align_;   // alignment of variable with this type
    public uint8 FieldAlign_;   // alignment of struct field with this type
    public ΔKind Kind_;    // enumeration for C
    // function for comparing objects of this type
    // (ptr to object A, ptr to object B) -> ==?
    public Func<@unsafe.Pointer, @unsafe.Pointer, bool> Equal;
    // GCData stores the GC type data for the garbage collector.
    // Normally, GCData points to a bitmask that describes the
    // ptr/nonptr fields of the type. The bitmask will have at
    // least PtrBytes/ptrSize bits.
    // If the TFlagGCMaskOnDemand bit is set, GCData is instead a
    // **byte and the pointer to the bitmask is one dereference away.
    // The runtime will build the bitmask if needed.
    // (See runtime/type.go:getGCMask.)
    // Note: multiple types may have the same value of GCData,
    // including when TFlagGCMaskOnDemand is set. The types will, of course,
    // have the same pointer layout (but not necessarily the same size).
    public ж<byte> GCData;
    public NameOff Str; // string form
    public TypeOff PtrToThis; // type for pointer to this type, may be zero
}

[GoType("num:uint8")] partial struct ΔKind;

public static ΔKind Invalid => /* iota */ 0;
public static ΔKind Bool => 1;
public static ΔKind Int => 2;
public static ΔKind Int8 => 3;
public static ΔKind Int16 => 4;
public static ΔKind Int32 => 5;
public static ΔKind Int64 => 6;
public static ΔKind Uint => 7;
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
public static ΔKind Interface => 20;
public static ΔKind Map => 21;
public static ΔKind Pointer => 22;
public static ΔKind Slice => 23;
public static ΔKind ΔString => 24;
public static ΔKind Struct => 25;
public static ΔKind UnsafePointer => 26;

public static ΔKind KindDirectIface => /* 1 << 5 */ 32;
public static ΔKind KindMask => /* (1 << 5) - 1 */ 31;

[GoType("num:uint8")] partial struct TFlag;

public static TFlag TFlagUncommon => /* 1 << 0 */ 1;
public static TFlag TFlagExtraStar => /* 1 << 1 */ 2;
public static TFlag TFlagNamed => /* 1 << 2 */ 4;
public static TFlag TFlagRegularMemory => /* 1 << 3 */ 8;
public static TFlag TFlagGCMaskOnDemand => /* 1 << 4 */ 16;

[GoType("num:int32")] partial struct NameOff;

[GoType("num:int32")] partial struct TypeOff;

[GoType("num:int32")] partial struct TextOff;

// String returns the name of k.
public static @string String(this ΔKind k) {
    if ((nint)(uint8)k < len(kindNames)) {
        return kindNames[k];
    }
    return kindNames[0];
}

internal static slice<@string> kindNames = new golib.SparseArray<@string>{
    [Invalid] = "invalid"u8,
    [Bool] = "bool"u8,
    [Int] = "int"u8,
    [Int8] = "int8"u8,
    [Int16] = "int16"u8,
    [Int32] = "int32"u8,
    [Int64] = "int64"u8,
    [Uint] = "uint"u8,
    [Uint8] = "uint8"u8,
    [Uint16] = "uint16"u8,
    [Uint32] = "uint32"u8,
    [Uint64] = "uint64"u8,
    [Uintptr] = "uintptr"u8,
    [Float32] = "float32"u8,
    [Float64] = "float64"u8,
    [Complex64] = "complex64"u8,
    [Complex128] = "complex128"u8,
    [Array] = "array"u8,
    [Chan] = "chan"u8,
    [Func] = "func"u8,
    [Interface] = "interface"u8,
    [Map] = "map"u8,
    [Pointer] = "ptr"u8,
    [Slice] = "slice"u8,
    [ΔString] = "string"u8,
    [Struct] = "struct"u8,
    [UnsafePointer] = "unsafe.Pointer"u8
}.slice();

// go2cs generated this placeholder — func TypeOf is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// TypeFor returns the abi.Type for a type parameter.
public static ж<Type> TypeFor<T>() {
    T v = GoZero<T>();
    {
        var t = TypeOf(v); if (t != nil) {
            return t; // optimize for T being a non-interface kind
        }
    }
    return TypeOf(((ж<T>)nil)).Elem(); // only for an interface kind
}

[GoRecv] public static ΔKind Kind(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindMask);
}

[GoRecv] public static bool HasName(this ref Type t) {
    return (TFlag)(t.TFlag & TFlagNamed) != 0;
}

// Pointers reports whether t contains pointers.
[GoRecv] public static bool Pointers(this ref Type t) {
    return t.PtrBytes != 0;
}

// IfaceIndir reports whether t is stored indirectly in an interface value.
[GoRecv] public static bool IfaceIndir(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindDirectIface) == 0;
}

// isDirectIface reports whether t is stored directly in an interface value.
[GoRecv] public static bool IsDirectIface(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindDirectIface) != 0;
}

[GoRecv] public static slice<byte> GcSlice(this ref Type t, uintptr begin, uintptr end) {
    if ((TFlag)(t.TFlag & TFlagGCMaskOnDemand) != 0) {
        throw panic("GcSlice can't handle on-demand gcdata types");
    }
    return @unsafe.Slice(t.GCData, (nint)end).slice((nint)(begin));
}

// Method on non-interface type
[GoType] partial struct Method {
    public NameOff Name; // name of method
    public TypeOff Mtyp; // method type (without receiver)
    public TextOff Ifn; // fn used in interface call (one-word receiver)
    public TextOff Tfn; // fn used for normal method call
}

// UncommonType is present only for defined types or types with methods
// (if T is a defined type, the uncommonTypes for T and *T have methods).
// Using a pointer to this struct reduces the overall size required
// to describe a non-defined type with no methods.
[GoType] partial struct UncommonType {
    public NameOff PkgPath; // import path; empty for built-in types like int, string
    public uint16 Mcount;  // number of methods
    public uint16 Xcount;  // number of exported methods
    public uint32 Moff;  // offset from this uncommontype to [mcount]Method
    internal uint32 _;  // unused
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tMcount0ˢ = "t.mcount > 0"u8;

public static unsafe slice<Method> Methods(this ж<UncommonType> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Mcount == 0) {
        return default!;
    }
    return new slice<Method>(new ReadOnlySpan<Method>((Method*)(uintptr)(addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), (uintptr)t.Moff, tMcount0ˢ)), (int)(t.Mcount)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tXcount0ˢ = "t.xcount > 0"u8;

public static unsafe slice<Method> ExportedMethods(this ж<UncommonType> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Xcount == 0) {
        return default!;
    }
    return new slice<Method>(new ReadOnlySpan<Method>((Method*)(uintptr)(addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), (uintptr)t.Moff, tXcount0ˢ)), (int)(t.Xcount)));
}

// addChecked returns p+x.
//
// The whySafe string is ignored, so that the function still inlines
// as efficiently as p+x, but all call sites should use the string to
// record why the addition is safe, which is to say why the addition
// does not cause x to advance to the very end of p's allocation
// and therefore point incorrectly at the next block in memory.
internal static @unsafe.Pointer addChecked(@unsafe.Pointer p, uintptr x, @string whySafe) {
    return (@unsafe.Pointer)((uintptr)p + x);
}

// Imethod represents a method on an interface type
[GoType] partial struct Imethod {
    public NameOff Name; // name of method
    public TypeOff Typ; // .(*FuncType) underneath
}

// ArrayType represents a fixed array type.
[GoType] partial struct ΔArrayType {
    public partial ref Type Type { get; }
    public ж<Type> Elem; // array element type
    public ж<Type> Slice; // slice type
    public uintptr Len;
}

// go2cs generated this placeholder — func Len is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

public static ж<Type> Common(this ж<Type> Ꮡt) {
    return Ꮡt;
}

[GoType("num:nint")] partial struct ΔChanDir;

public static ΔChanDir RecvDir => /* 1 << iota */ 1;                // <-chan
public static ΔChanDir SendDir => 2;                // chan<-
public static ΔChanDir BothDir => /* RecvDir | SendDir */ 3; // chan
public static ΔChanDir InvalidDir => 0;

// ChanType represents a channel type
[GoType] partial struct ChanType {
    public partial ref Type Type { get; }
    public ж<Type> Elem;
    public ΔChanDir Dir;
}

[GoType] partial struct structTypeUncommon {
    public partial ref ΔStructType StructType { get; }
    internal UncommonType u;
}

// go2cs generated this placeholder — func ChanDir is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

[GoType("dyn")] internal partial struct Uncommon_u {
    public partial ref PtrType PtrType { get; }
    internal UncommonType u;
}

[GoType("dyn")] internal partial struct Uncommon_uᴛ1 {
    public partial ref ΔFuncType FuncType { get; }
    internal UncommonType u;
}

[GoType("dyn")] internal partial struct Uncommon_uᴛ2 {
    public partial ref SliceType SliceType { get; }
    internal UncommonType u;
}

[GoType("dyn")] internal partial struct Uncommon_uᴛ3 {
    public partial ref ΔArrayType ArrayType { get; }
    internal UncommonType u;
}

[GoType("dyn")] internal partial struct Uncommon_uᴛ4 {
    public partial ref ChanType ChanType { get; }
    internal UncommonType u;
}

[GoType("dyn")] internal partial struct Uncommon_uᴛ5 {
    internal partial ref mapType mapType { get; }
    internal UncommonType u;
}

[GoType("dyn")] internal partial struct Uncommon_uᴛ6 {
    public partial ref ΔInterfaceType InterfaceType { get; }
    internal UncommonType u;
}

[GoType("dyn")] internal partial struct Uncommon_uᴛ7 {
    public partial ref Type Type { get; }
    internal UncommonType u;
}

// Uncommon returns a pointer to T's "uncommon" data if there is any, otherwise nil
public static ж<UncommonType> Uncommon(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if ((TFlag)(t.TFlag & TFlagUncommon) == 0) {
        return default!;
    }
    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == Struct) {
        return Ꮡ((Ꮡt.Reinterpret<Type, structTypeUncommon>()).Value.u);
    }
    if (exprᴛ1 == Pointer) {
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_u>()).Value.u);
    }
    if (exprᴛ1 == Func) {
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_uᴛ1>()).Value.u);
    }
    if (exprᴛ1 == Slice) {
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_uᴛ2>()).Value.u);
    }
    if (exprᴛ1 == Array) {
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_uᴛ3>()).Value.u);
    }
    if (exprᴛ1 == Chan) {
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_uᴛ4>()).Value.u);
    }
    if (exprᴛ1 == Map) {
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_uᴛ5>()).Value.u);
    }
    if (exprᴛ1 == Interface) {
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_uᴛ6>()).Value.u);
    }
    { /* default: */
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_uᴛ7>()).Value.u);
    }

}

// go2cs generated this placeholder — func Elem is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func StructType is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func MapType is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func ArrayType is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func FuncType is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// InterfaceType returns t cast to a *InterfaceType, or nil if its tag does not match.
public static ж<ΔInterfaceType> InterfaceType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Interface) {
        return default!;
    }
    return Ꮡt.Reinterpret<Type, ΔInterfaceType>();
}

// Size returns the size of data with type t.
[GoRecv] public static uintptr Size(this ref Type t) {
    return t.Size_;
}

// Align returns the alignment of data with type t.
[GoRecv] public static nint Align(this ref Type t) {
    return (nint)t.Align_;
}

[GoRecv] public static nint FieldAlign(this ref Type t) {
    return (nint)t.FieldAlign_;
}

[GoType] partial struct ΔInterfaceType {
    public partial ref Type Type { get; }
    public ΔName PkgPath;      // import path
    public slice<Imethod> Methods; // sorted by hash
}

public static slice<Method> ExportedMethods(this ж<Type> Ꮡt) {
    var ut = Ꮡt.Uncommon();
    if (ut == nil) {
        return default!;
    }
    return ut.ExportedMethods();
}

public static nint NumMethod(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() == Interface) {
        var tt = Ꮡt.Reinterpret<Type, ΔInterfaceType>();
        return tt.NumMethod();
    }
    return len(Ꮡt.ExportedMethods());
}

// NumMethod returns the number of interface methods in the type's method set.
[GoRecv] public static nint NumMethod(this ref ΔInterfaceType t) {
    return len(t.Methods);
}

// go2cs generated this placeholder — func Key is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

[GoType] partial struct SliceType {
    public partial ref Type Type { get; }
    public ж<Type> Elem; // slice element type
}

// funcType represents a function type.
//
// A *Type for each in and out parameter is stored in an array that
// directly follows the funcType (and possibly its uncommonType). So
// a function type with one method, one input, and one output is:
//
//	struct {
//		funcType
//		uncommonType
//		[2]*rtype    // [0] is in, [1] is out
//	}
[GoType] partial struct ΔFuncType {
    public partial ref Type Type { get; }
    public uint16 InCount;
    public uint16 OutCount; // top bit is set if last input parameter is ...
}

public static ж<Type> In(this ж<ΔFuncType> Ꮡt, nint i) {
    return Ꮡt.InSlice()[i];
}

[GoRecv] public static nint NumIn(this ref ΔFuncType t) {
    return (nint)t.InCount;
}

[GoRecv] public static nint NumOut(this ref ΔFuncType t) {
    return (nint)((uint16)(t.OutCount & ((1 << (int)(15)) - 1)));
}

public static ж<Type> Out(this ж<ΔFuncType> Ꮡt, nint i) {
    return (Ꮡt.OutSlice()[i]);
}

// go2cs generated this placeholder — func InSlice is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func OutSlice is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

[GoRecv] public static bool IsVariadic(this ref ΔFuncType t) {
    return (uint16)(t.OutCount & ((uint16)(1 << (int)(15)))) != 0;
}

[GoType] partial struct PtrType {
    public partial ref Type Type { get; }
    public ж<Type> Elem; // pointer element (pointed at) type
}

[GoType] partial struct StructField {
    public ΔName Name;    // name is always non-empty
    public ж<Type> Typ; // type of field
    public uintptr Offset; // byte offset of field
}

[GoRecv] public static bool Embedded(this ref StructField f) {
    return f.Name.IsEmbedded();
}

[GoType] partial struct ΔStructType {
    public partial ref Type Type { get; }
    public ΔName PkgPath;
    public slice<StructField> Fields;
}

// Name is an encoded type Name with optional extra data.
//
// The first byte is a bit field containing:
//
//	1<<0 the name is exported
//	1<<1 tag data follows the name
//	1<<2 pkgPath nameOff follows the name and tag
//	1<<3 the name is of an embedded (a.k.a. anonymous) field
//
// Following that, there is a varint-encoded length of the name,
// followed by the name itself.
//
// If tag data is present, it also has a varint-encoded length
// followed by the tag itself.
//
// If the import path follows, then 4 bytes at the end of
// the data form a nameOff. The import path is only set for concrete
// methods that are defined in a different package than their type.
//
// If a name starts with "*", then the exported bit represents
// whether the pointed to type is exported.
//
// Note: this encoding must match here and in:
//   cmd/compile/internal/reflectdata/reflect.go
//   cmd/link/internal/ld/decodesym.go
[GoType] partial struct ΔName {
    public ж<byte> Bytes;
}

// DataChecked does pointer arithmetic on n's Bytes, and that arithmetic is asserted to
// be safe for the reason in whySafe (which can appear in a backtrace, etc.)
public static ж<byte> DataChecked(this ΔName n, nint off, @string whySafe) {
    return (ж<byte>)(uintptr)(addChecked(@unsafe.Pointer.FromPinnedBox(n.Bytes), (uintptr)off, whySafe));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string theRuntimeDoesnTNeedToˢ = "the runtime doesn't need to give you a reason"u8;

// Data does pointer arithmetic on n's Bytes, and that arithmetic is asserted to
// be safe because the runtime made the call (other packages use DataChecked)
public static ж<byte> Data(this ΔName n, nint off) {
    return (ж<byte>)(uintptr)(addChecked(@unsafe.Pointer.FromPinnedBox(n.Bytes), (uintptr)off, theRuntimeDoesnTNeedToˢ));
}

// IsExported returns "is n exported?"
public static bool IsExported(this ΔName n) {
    return (byte)((n.Bytes.Value) & ((byte)(1 << (int)(0)))) != 0;
}

// HasTag returns true iff there is tag data following this name
public static bool HasTag(this ΔName n) {
    return (byte)((n.Bytes.Value) & ((byte)(1 << (int)(1)))) != 0;
}

// IsEmbedded returns true iff n is embedded (an anonymous field).
public static bool IsEmbedded(this ΔName n) {
    return (byte)((n.Bytes.Value) & ((byte)(1 << (int)(3)))) != 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string readVarintˢ = "read varint"u8;

// ReadVarint parses a varint as encoded by encoding/binary.
// It returns the number of encoded bytes and the encoded value.
public static (nint, nint) ReadVarint(this ΔName n, nint off) {
    nint v = 0;
    for (nint i = 0; ᐧ ; i++) {
        var x = n.DataChecked(off + i, readVarintˢ).Value;
        v += ((nint)((byte)(x & 0x7f))).Lsh((int64)((7 * i)));
        if ((byte)(x & 0x80) == 0) {
            return (i + 1, v);
        }
    }
}

// IsBlank indicates whether n is "_".
public static bool IsBlank(this ΔName n) {
    if (n.Bytes == nil) {
        return false;
    }
    var (_, l) = n.ReadVarint(1);
    return l == 1 && n.Data(2).Value == (rune)'_';
}

// writeVarint writes n to buf in varint form. Returns the
// number of bytes written. n must be nonnegative.
// Writes at most 10 bytes.
internal static nint writeVarint(slice<byte> buf, nint n) {
    for (nint i = 0; ᐧ ; i++) {
        var b = (byte)((nint)(n & 0x7f));
        n >>= (int)(7);
        if (n == 0) {
            buf[i] = b;
            return i + 1;
        }
        buf[i] = (byte)(b | 0x80);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nonEmptyStringˢ = "non-empty string"u8;

// Name returns the tag string for n, or empty if there is none.
public static @string Name(this ΔName n) {
    if (n.Bytes == nil) {
        return ""u8;
    }
    var (i, l) = n.ReadVarint(1);
    return @unsafe.String(n.DataChecked(1 + i, nonEmptyStringˢ), l);
}

// Tag returns the tag string for n, or empty if there is none.
public static @string Tag(this ΔName n) {
    if (!n.HasTag()) {
        return ""u8;
    }
    var (i, l) = n.ReadVarint(1);
    var (i2, l2) = n.ReadVarint(1 + i + l);
    return @unsafe.String(n.DataChecked(1 + i + l + i2, nonEmptyStringˢ), l2);
}

public static ΔName NewName(@string n, @string tag, bool exported, bool embedded) {
    if (len(n) >= (1 << (int)(29))) {
        throw panic("abi.NewName: name too long: " + n[..1024] + "...");
    }
    if (len(tag) >= (1 << (int)(29))) {
        throw panic("abi.NewName: tag too long: " + tag[..1024] + "...");
    }
    array<byte> nameLen = new(10);
    array<byte> tagLen = new(10);
    nint nameLenLen = writeVarint(nameLen[..], len(n));
    nint tagLenLen = writeVarint(tagLen[..], len(tag));
    byte bits = default!;
    nint l = 1 + nameLenLen + len(n);
    if (exported) {
        bits |= (byte)((byte)(1 << (int)(0)));
    }
    if (len(tag) > 0) {
        l += tagLenLen + len(tag);
        bits |= (byte)((byte)(1 << (int)(1)));
    }
    if (embedded) {
        bits |= (byte)((byte)(1 << (int)(3)));
    }
    var b = new slice<byte>(l);
    b[0] = bits;
    copy(b[1..], nameLen.slice(0, nameLenLen));
    copy(b.slice(1 + nameLenLen), n);
    if (len(tag) > 0) {
        var tb = b.slice(1 + nameLenLen + len(n));
        copy(tb, tagLen.slice(0, tagLenLen));
        copy(tb.slice(tagLenLen), tag);
    }
    return new ΔName(Bytes: Ꮡ(b, 0));
}

public static UntypedInt TraceArgsLimit => 10; // print no more than 10 args/components
public static UntypedInt TraceArgsMaxDepth => 5; // no more than 5 layers of nesting
public static UntypedInt TraceArgsMaxLen => /* (TraceArgsMaxDepth*3+2)*TraceArgsLimit + 1 */ 171;

// Populate the data.
// The data is a stream of bytes, which contains the offsets and sizes of the
// non-aggregate arguments or non-aggregate fields/elements of aggregate-typed
// arguments, along with special "operators". Specifically,
//   - for each non-aggregate arg/field/element, its offset from FP (1 byte) and
//     size (1 byte)
//   - special operators:
//   - 0xff - end of sequence
//   - 0xfe - print { (at the start of an aggregate-typed argument)
//   - 0xfd - print } (at the end of an aggregate-typed argument)
//   - 0xfc - print ... (more args/fields/elements)
//   - 0xfb - print _ (offset too large)
public static UntypedInt TraceArgsEndSeq => 0xff;

public static UntypedInt TraceArgsStartAgg => 0xfe;

public static UntypedInt TraceArgsEndAgg => 0xfd;

public static UntypedInt TraceArgsDotdotdot => 0xfc;

public static UntypedInt TraceArgsOffsetTooLarge => 0xfb;

public static UntypedInt TraceArgsSpecial => 0xf0; // above this are operators, below this are ordinary offsets

// MaxPtrmaskBytes is the maximum length of a GC ptrmask bitmap,
// which holds 1-bit entries describing where pointers are in a given type.
// Above this length, the GC information is recorded as a GC program,
// which can express repetition compactly. In either form, the
// information is used by the runtime to initialize the heap bitmap,
// and for large types (like 128 or more words), they are roughly the
// same speed. GC programs are never much larger and often more
// compact. (If large arrays are involved, they can be arbitrarily
// more compact.)
//
// The cutoff must be large enough that any allocation large enough to
// use a GC program is large enough that it does not share heap bitmap
// bytes with any other objects, allowing the GC program execution to
// assume an aligned start and not use atomic operations. In the current
// runtime, this means all malloc size classes larger than the cutoff must
// be multiples of four words. On 32-bit systems that's 16 bytes, and
// all size classes >= 16 bytes are 16-byte aligned, so no real constraint.
// On 64-bit systems, that's 32 bytes, and 32-byte alignment is guaranteed
// for size classes >= 256 bytes. On a 64-bit system, 256 bytes allocated
// is 32 pointers, the bits for which fit in 4 bytes. So MaxPtrmaskBytes
// must be >= 4.
//
// We used to use 16 because the GC programs do have some constant overhead
// to get started, and processing 128 pointers seems to be enough to
// amortize that overhead well.
//
// To make sure that the runtime's chansend can call typeBitsBulkBarrier,
// we raised the limit to 2048, so that even 32-bit systems are guaranteed to
// use bitmaps for objects up to 64 kB in size.
public static UntypedInt MaxPtrmaskBytes => 2048;

} // end abi_package
