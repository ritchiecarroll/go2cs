// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go;

using @unsafe = unsafe_package;
using static main_package;

partial class main_package {

partial struct Type {
    public uintptr Size_;
    public uintptr PtrBytes;
    public uint32 Hash;
    public TFlag TFlag;
    public uint8 Align_;
    public uint8 FieldAlign_;
    public Kind Kind_;
    public Func<@unsafe.Pointer, @unsafe.Pointer, bool> Equal;
    public ж<byte> GCData;
    public NameOff Str;
    public TypeOff PtrToThis;
}

partial struct Kind /*num:uint8*/;

public static Kind Invalid => /* iota */ 0;
public static Kind Bool => 1;
public static Kind Int => 2;
public static Kind Int8 => 3;
public static Kind Int16 => 4;
public static Kind Int32 => 5;
public static Kind Int64 => 6;
public static Kind Uint => 7;
public static Kind Uint8 => 8;
public static Kind Uint16 => 9;
public static Kind Uint32 => 10;
public static Kind Uint64 => 11;
public static Kind Uintptr => 12;
public static Kind Float32 => 13;
public static Kind Float64 => 14;
public static Kind Complex64 => 15;
public static Kind Complex128 => 16;
public static Kind Array => 17;
public static Kind Chan => 18;
public static Kind Func => 19;
public static Kind Interface => 20;
public static Kind Map => 21;
public static Kind Pointer => 22;
public static Kind Slice => 23;
public static Kind String => 24;
public static Kind Struct => 25;
public static Kind UnsafePointer => 26;

public static Kind KindDirectIface => /* 1 << 5 */ 32;
public static Kind KindGCProg => /* 1 << 6 */ 64;
public static Kind KindMask => /* (1 << 5) - 1 */ 31;

partial struct TFlag /*num:uint8*/;

public static TFlag TFlagUncommon => /* 1 << 0 */ 1;
public static TFlag TFlagExtraStar => /* 1 << 1 */ 2;
public static TFlag TFlagNamed => /* 1 << 2 */ 4;
public static TFlag TFlagRegularMemory => /* 1 << 3 */ 8;
public static TFlag TFlagUnrolledBitmap => /* 1 << 4 */ 16;

partial struct NameOff /*num:int32*/;

partial struct TypeOff /*num:int32*/;

partial struct TextOff /*num:int32*/;

} partial class mainᴛKind {

public static @string String(this Kind k) {
    if ((nint)k < len(kindNames)) {
        return kindNames[k];
    }
    return kindNames[0];
}

} partial class main_package {

[GoLazy] partial class lazyᴛkindNames { internal static slice<@string> kindNames = new golib.SparseArray<@string>{
    [Invalid] = "invalid",
    [Bool] = "bool",
    [Int] = "int",
    [Int8] = "int8",
    [Int16] = "int16",
    [Int32] = "int32",
    [Int64] = "int64",
    [Uint] = "uint",
    [Uint8] = "uint8",
    [Uint16] = "uint16",
    [Uint32] = "uint32",
    [Uint64] = "uint64",
    [Uintptr] = "uintptr",
    [Float32] = "float32",
    [Float64] = "float64",
    [Complex64] = "complex64",
    [Complex128] = "complex128",
    [Array] = "array",
    [Chan] = "chan",
    [Func] = "func",
    [Interface] = "interface",
    [Map] = "map",
    [Pointer] = "ptr",
    [Slice] = "slice",
    [String] = "string",
    [Struct] = "struct",
    [UnsafePointer] = "unsafe.Pointer"
}.slice(); }

public static ж<Type> TypeOf(any aʗp) {
    ref var a = ref heap(aʗp, out var Ꮡa);

    var eface = ~Ꮡa.Reinterpret<any, EmptyInterface>();
    return eface.Type;
}

public static ж<Type> TypeFor<T>() {
    T v = GoZero<T>();
    var t = TypeOf((v).OrTypedNilOf()); if (t != nil) {
        return t;
    }
    return TypeOf((ж<T>)nil).Elem();
}

} partial class mainᴛType {

public static Kind Kind(this ref Type t) { return (Kind)(t.Kind_ & KindMask); }

public static bool HasName(this ref Type t) {
    return (TFlag)(t.TFlag & TFlagNamed) != 0;
}

public static bool Pointers(this ref Type t) { return t.PtrBytes != 0; }

public static bool IfaceIndir(this ref Type t) {
    return (Kind)(t.Kind_ & KindDirectIface) == 0;
}

public static bool IsDirectIface(this ref Type t) {
    return (Kind)(t.Kind_ & KindDirectIface) != 0;
}

public static slice<byte> GcSlice(this ref Type t, uintptr begin, uintptr end) {
    return @unsafe.Slice(t.GCData, (nint)end).slice((nint)(begin));
}

} partial class main_package {

partial struct Method {
    public NameOff Name;
    public TypeOff Mtyp;
    public TextOff Ifn;
    public TextOff Tfn;
}

partial struct UncommonType {
    public NameOff PkgPath;
    public uint16 Mcount;
    public uint16 Xcount;
    public uint32 Moff;
    internal uint32 _;
}

} partial class mainᴛUncommonType {

public static unsafe slice<Method> Methods(this ж<UncommonType> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Mcount == 0) {
        return default;
    }
    return new slice<Method>(new ReadOnlySpan<Method>((Method*)(uintptr)(addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), (uintptr)t.Moff, "t.mcount > 0")), (int)(t.Mcount)));
}

public static unsafe slice<Method> ExportedMethods(this ж<UncommonType> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Xcount == 0) {
        return default;
    }
    return new slice<Method>(new ReadOnlySpan<Method>((Method*)(uintptr)(addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), (uintptr)t.Moff, "t.xcount > 0")), (int)(t.Xcount)));
}

} partial class main_package {

internal static @unsafe.Pointer addChecked(@unsafe.Pointer p, uintptr x, @string whySafe) {
    return (@unsafe.Pointer)((uintptr)p + x);
}

partial struct Imethod {
    public NameOff Name;
    public TypeOff Typ;
}

partial struct ArrayType {
    /*embed*/ public Type Type;
    public ж<Type> Elem;
    public ж<Type> Slice;
    public uintptr Len;
}

} partial class mainᴛType {

public static nint Len(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() == Array) {
        return (nint)(Ꮡt.Reinterpret<Type, ArrayType>()).Value.Len;
    }
    return 0;
}

public static ж<Type> Common(this ж<Type> Ꮡt) {
    return Ꮡt;
}

} partial class main_package {

partial struct ChanDir /*num:nint*/;

public static ChanDir RecvDir => /* 1 << iota */ 1;
public static ChanDir SendDir => 2;
public static ChanDir BothDir => /* RecvDir | SendDir */ 3;
public static ChanDir InvalidDir => 0;

partial struct ChanType {
    /*embed*/ public Type Type;
    public ж<Type> Elem;
    public ChanDir Dir;
}

partial struct structTypeUncommon {
    /*embed*/ public StructType StructType;
    internal UncommonType u;
}

} partial class mainᴛType {

public static ChanDir ChanDir(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() == Chan) {
        var ch = Ꮡt.Reinterpret<Type, ChanType>();
        return ch.Dir;
    }
    return InvalidDir;
}

} partial class main_package {

internal partial struct Uncommon_u /*dyn*/ {
    /*embed*/ public PtrType PtrType;
    internal UncommonType u;
}

internal partial struct Uncommon_uᴛ1 /*dyn*/ {
    /*embed*/ public FuncType FuncType;
    internal UncommonType u;
}

internal partial struct Uncommon_uᴛ2 /*dyn*/ {
    /*embed*/ public SliceType SliceType;
    internal UncommonType u;
}

internal partial struct Uncommon_uᴛ3 /*dyn*/ {
    /*embed*/ public ArrayType ArrayType;
    internal UncommonType u;
}

internal partial struct Uncommon_uᴛ4 /*dyn*/ {
    /*embed*/ public ChanType ChanType;
    internal UncommonType u;
}

internal partial struct Uncommon_uᴛ5 /*dyn*/ {
    /*embed*/ public MapType MapType;
    internal UncommonType u;
}

internal partial struct Uncommon_uᴛ6 /*dyn*/ {
    /*embed*/ public InterfaceType InterfaceType;
    internal UncommonType u;
}

internal partial struct Uncommon_uᴛ7 /*dyn*/ {
    /*embed*/ public Type Type;
    internal UncommonType u;
}

} partial class mainᴛType {

public static ж<UncommonType> Uncommon(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if ((TFlag)(t.TFlag & TFlagUncommon) == 0) {
        return default;
    }
    switch (t.Kind()) {
    case var ᴛ when ᴛ == Struct: {
        return Ꮡ((Ꮡt.Reinterpret<Type, structTypeUncommon>()).Value.u);
    }
    case var ᴛ when ᴛ == Pointer: {
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_u>()).Value.u);
    }
    case var ᴛ when ᴛ == Func: {
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_uᴛ1>()).Value.u);
    }
    case var ᴛ when ᴛ == Slice: {
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_uᴛ2>()).Value.u);
    }
    case var ᴛ when ᴛ == Array: {
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_uᴛ3>()).Value.u);
    }
    case var ᴛ when ᴛ == Chan: {
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_uᴛ4>()).Value.u);
    }
    case var ᴛ when ᴛ == Map: {
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_uᴛ5>()).Value.u);
    }
    case var ᴛ when ᴛ == Interface: {
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_uᴛ6>()).Value.u);
    }
    default: {
        return Ꮡ((Ꮡt.Reinterpret<Type, Uncommon_uᴛ7>()).Value.u);
    }}

}

public static ж<Type> Elem(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    switch (t.Kind()) {
    case var ᴛ when ᴛ == Array: {
        var tt = Ꮡt.Reinterpret<Type, ArrayType>();
        return tt.of(main_package.ArrayType.ᏑElem).Value;
    }
    case var ᴛ when ᴛ == Chan: {
        var tt = Ꮡt.Reinterpret<Type, ChanType>();
        return tt.of(ChanType.ᏑElem).Value;
    }
    case var ᴛ when ᴛ == Map: {
        var tt = Ꮡt.Reinterpret<Type, MapType>();
        return tt.of(main_package.MapType.ᏑElem).Value;
    }
    case var ᴛ when ᴛ == Pointer: {
        var tt = Ꮡt.Reinterpret<Type, PtrType>();
        return tt.of(PtrType.ᏑElem).Value;
    }
    case var ᴛ when ᴛ == Slice: {
        var tt = Ꮡt.Reinterpret<Type, SliceType>();
        return tt.of(SliceType.ᏑElem).Value;
    }}

    return default;
}

public static ж<StructType> StructType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Struct) {
        return default;
    }
    return Ꮡt.Reinterpret<Type, StructType>();
}

public static ж<MapType> MapType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Map) {
        return default;
    }
    return Ꮡt.Reinterpret<Type, MapType>();
}

public static ж<ArrayType> ArrayType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Array) {
        return default;
    }
    return Ꮡt.Reinterpret<Type, ArrayType>();
}

public static ж<FuncType> FuncType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Func) {
        return default;
    }
    return Ꮡt.Reinterpret<Type, FuncType>();
}

public static ж<InterfaceType> InterfaceType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Interface) {
        return default;
    }
    return Ꮡt.Reinterpret<Type, InterfaceType>();
}

public static uintptr Size(this ref Type t) { return t.Size_; }

public static nint Align(this ref Type t) { return (nint)t.Align_; }

public static nint FieldAlign(this ref Type t) { return (nint)t.FieldAlign_; }

} partial class main_package {

partial struct InterfaceType {
    /*embed*/ public Type Type;
    public Name PkgPath;
    public slice<Imethod> Methods;
}

} partial class mainᴛType {

public static slice<Method> ExportedMethods(this ж<Type> Ꮡt) {
    var ut = Ꮡt.Uncommon();
    if (ut == nil) {
        return default;
    }
    return ut.ExportedMethods();
}

public static nint NumMethod(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() == Interface) {
        var tt = Ꮡt.Reinterpret<Type, InterfaceType>();
        return tt.NumMethod();
    }
    return len(Ꮡt.ExportedMethods());
}

} partial class mainᴛInterfaceType {

public static nint NumMethod(this ref InterfaceType t) { return len(t.Methods); }

} partial class main_package {

partial struct MapType {
    /*embed*/ public Type Type;
    public ж<Type> Key;
    public ж<Type> Elem;
    public ж<Type> Bucket;
    public Func<@unsafe.Pointer, uintptr, uintptr> Hasher;
    public uint8 KeySize;
    public uint8 ValueSize;
    public uint16 BucketSize;
    public uint32 Flags;
}

} partial class mainᴛMapType {

public static bool IndirectKey(this ref MapType mt) {
    return (mt.Flags & 1) != 0;
}
public static bool IndirectElem(this ref MapType mt) {
    return (mt.Flags & 2) != 0;
}
public static bool ReflexiveKey(this ref MapType mt) {
    return (mt.Flags & 4) != 0;
}
public static bool NeedKeyUpdate(this ref MapType mt) {
    return (mt.Flags & 8) != 0;
}
public static bool HashMightPanic(this ref MapType mt) {
    return (mt.Flags & 16) != 0;
}

} partial class mainᴛType {

public static ж<Type> Key(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() == Map) {
        return (Ꮡt.Reinterpret<Type, MapType>()).Value.Key;
    }
    return default;
}

} partial class main_package {

partial struct SliceType {
    /*embed*/ public Type Type;
    public ж<Type> Elem;
}

partial struct FuncType {
    /*embed*/ public Type Type;
    public uint16 InCount;
    public uint16 OutCount;
}

} partial class mainᴛFuncType {

public static ж<Type> In(this ж<FuncType> Ꮡt, nint i) {
    return Ꮡt.InSlice()[i];
}

public static nint NumIn(this ref FuncType t) {
    return (nint)t.InCount;
}

public static nint NumOut(this ref FuncType t) {
    return (nint)((uint16)(t.OutCount & ((1 << 15) - 1)));
}

public static ж<Type> Out(this ж<FuncType> Ꮡt, nint i) {
    return (Ꮡt.OutSlice()[i]);
}

public static slice<ж<Type>> InSlice(this ж<FuncType> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var uadd = /* unsafe.Sizeof(*t) */ (uintptr)56;
    if ((TFlag)(t.TFlag & TFlagUncommon) != 0) {
        uadd += /* unsafe.Sizeof(UncommonType{}) */ (uintptr)16;
    }
    if (t.InCount == 0) {
        return default;
    }
    return (false ? default(slice<ж<Type>>) : throw panic("go2cs: a slice view (*[N]*Type)(p)[...] over a managed element has no managed aliasing pair (type.go:551)"));
}
public static slice<ж<Type>> OutSlice(this ж<FuncType> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var outCount = (uint16)t.NumOut();
    if (outCount == 0) {
        return default;
    }
    var uadd = /* unsafe.Sizeof(*t) */ (uintptr)56;
    if ((TFlag)(t.TFlag & TFlagUncommon) != 0) {
        uadd += /* unsafe.Sizeof(UncommonType{}) */ (uintptr)16;
    }
    return (false ? default(slice<ж<Type>>) : throw panic("go2cs: a slice view (*[N]*Type)(p)[...] over a managed element has no managed aliasing pair (type.go:562)"));
}

public static bool IsVariadic(this ref FuncType t) {
    return (uint16)(t.OutCount & ((uint16)(1 << 15))) != 0;
}

} partial class main_package {

partial struct PtrType {
    /*embed*/ public Type Type;
    public ж<Type> Elem;
}

partial struct StructField {
    public Name Name;
    public ж<Type> Typ;
    public uintptr Offset;
}

} partial class mainᴛStructField {

public static bool Embedded(this ref StructField f) {
    return f.Name.IsEmbedded();
}

} partial class main_package {

partial struct StructType {
    /*embed*/ public Type Type;
    public Name PkgPath;
    public slice<StructField> Fields;
}

partial struct Name {
    public ж<byte> Bytes;
}

} partial class mainᴛName {

public static ж<byte> DataChecked(this Name n, nint off, @string whySafe) {
    return (ж<byte>)(uintptr)(addChecked(@unsafe.Pointer.FromPinnedBox(n.Bytes), (uintptr)off, whySafe));
}

public static ж<byte> Data(this Name n, nint off) {
    return (ж<byte>)(uintptr)(addChecked(@unsafe.Pointer.FromPinnedBox(n.Bytes), (uintptr)off, "the runtime doesn't need to give you a reason"));
}

public static bool IsExported(this Name n) {
    return (byte)((n.Bytes.Value) & ((byte)(1 << 0))) != 0;
}

public static bool HasTag(this Name n) {
    return (byte)((n.Bytes.Value) & ((byte)(1 << 1))) != 0;
}

public static bool IsEmbedded(this Name n) {
    return (byte)((n.Bytes.Value) & ((byte)(1 << 3))) != 0;
}

public static (nint, nint) ReadVarint(this Name n, nint off) {
    nint v = 0;
    for (nint i = 0; ; i++) {
        var x = n.DataChecked(off + i, "read varint").Value;
        v += ((nint)((byte)(x & 0x7f))).Lsh((int64)(7 * i));
        if ((byte)(x & 0x80) == 0) {
            return (i + 1, v);
        }
    }
}

public static bool IsBlank(this Name n) {
    if (n.Bytes == nil) {
        return false;
    }
    var (_, l) = n.ReadVarint(1);
    return l == 1 && n.Data(2).Value == '_';
}

} partial class main_package {

internal static nint writeVarint(slice<byte> buf, nint n) {
    for (nint i = 0; ; i++) {
        var b = (byte)(n & 0x7f);
        n >>= 7;
        if (n == 0) {
            buf[i] = b;
            return i + 1;
        }
        buf[i] = (byte)(b | 0x80);
    }
}

} partial class mainᴛName {

public static @string Name(this Name n) {
    if (n.Bytes == nil) {
        return "";
    }
    var (i, l) = n.ReadVarint(1);
    return @unsafe.String(n.DataChecked(1 + i, "non-empty string"), l);
}

public static @string Tag(this Name n) {
    if (!n.HasTag()) {
        return "";
    }
    var (i, l) = n.ReadVarint(1);
    var (i2, l2) = n.ReadVarint(1 + i + l);
    return @unsafe.String(n.DataChecked(1 + i + l + i2, "non-empty string"), l2);
}

} partial class main_package {

public static Name NewName(@string n, @string tag, bool exported, bool embedded) {
    if (len(n) >= 1 << 29) {
        throw panic("abi.NewName: name too long: " + n[..1024] + "...");
    }
    if (len(tag) >= 1 << 29) {
        throw panic("abi.NewName: tag too long: " + tag[..1024] + "...");
    }
    array<byte> nameLen = new(10);
    array<byte> tagLen = new(10);
    nint nameLenLen = writeVarint(nameLen[..], len(n));
    nint tagLenLen = writeVarint(tagLen[..], len(tag));

    byte bits = default;
    nint l = 1 + nameLenLen + len(n);
    if (exported) {
        bits |= (byte)((byte)(1 << 0));
    }
    if (len(tag) > 0) {
        l += tagLenLen + len(tag);
        bits |= (byte)((byte)(1 << 1));
    }
    if (embedded) {
        bits |= (byte)((byte)(1 << 3));
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

    return new Name(Bytes: Ꮡ(b, 0));
}

public static UntypedInt TraceArgsLimit => 10;
public static UntypedInt TraceArgsMaxDepth => 5;
public static UntypedInt TraceArgsMaxLen => /* (TraceArgsMaxDepth*3+2)*TraceArgsLimit + 1 */ 171;

public static UntypedInt TraceArgsEndSeq => 0xff;
public static UntypedInt TraceArgsStartAgg => 0xfe;
public static UntypedInt TraceArgsEndAgg => 0xfd;
public static UntypedInt TraceArgsDotdotdot => 0xfc;
public static UntypedInt TraceArgsOffsetTooLarge => 0xfb;
public static UntypedInt TraceArgsSpecial => 0xf0;

public static UntypedInt MaxPtrmaskBytes => 2048;

} // end main_package
