// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go;

using @unsafe = unsafe_package;

partial class main_package {

[GoType] partial struct Type {
    public uintptr Size_;
    public uintptr PtrBytes;
    public uint32 Hash;
    public TFlag TFlag;
    public uint8 Align_;
    public uint8 FieldAlign_;
    public ΔKind Kind_;
    public Func<@unsafe.Pointer, @unsafe.Pointer, bool> Equal;
    public ж<byte> GCData;
    public NameOff Str;
    public TypeOff PtrToThis;
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
public static ΔKind KindGCProg => /* 1 << 6 */ 64;
public static ΔKind KindMask => /* (1 << 5) - 1 */ 31;

[GoType("num:uint8")] partial struct TFlag;

public static TFlag TFlagUncommon => /* 1 << 0 */ 1;
public static TFlag TFlagExtraStar => /* 1 << 1 */ 2;
public static TFlag TFlagNamed => /* 1 << 2 */ 4;
public static TFlag TFlagRegularMemory => /* 1 << 3 */ 8;
public static TFlag TFlagUnrolledBitmap => /* 1 << 4 */ 16;

[GoType("num:int32")] partial struct NameOff;

[GoType("num:int32")] partial struct TypeOff;

[GoType("num:int32")] partial struct TextOff;

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

public static ж<Type> TypeOf(any aʗp) {
    ref var a = ref heap(aʗp, out var Ꮡa);

    var eface = ~Ꮡa.Reinterpret<any, EmptyInterface>();
    return eface.Type;
}

public static ж<Type> TypeFor<T>() {
    T v = default!;
    {
        var t = TypeOf(v); if (t != nil) {
            return t;
        }
    }
    return TypeOf(((ж<T>)nil)).Elem();
}

[GoRecv] public static ΔKind Kind(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindMask);
}

[GoRecv] public static bool HasName(this ref Type t) {
    return (TFlag)(t.TFlag & TFlagNamed) != 0;
}

[GoRecv] public static bool Pointers(this ref Type t) {
    return t.PtrBytes != 0;
}

[GoRecv] public static bool IfaceIndir(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindDirectIface) == 0;
}

[GoRecv] public static bool IsDirectIface(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindDirectIface) != 0;
}

[GoRecv] public static slice<byte> GcSlice(this ref Type t, uintptr begin, uintptr end) {
    return @unsafe.Slice(t.GCData, (nint)end)[(int)(begin)..];
}

[GoType] partial struct Method {
    public NameOff Name;
    public TypeOff Mtyp;
    public TextOff Ifn;
    public TextOff Tfn;
}

[GoType] partial struct UncommonType {
    public NameOff PkgPath;
    public uint16 Mcount;
    public uint16 Xcount;
    public uint32 Moff;
    internal uint32 _;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string tMcount0ˢ = "t.mcount > 0"u8;

public static unsafe slice<Method> Methods(this ж<UncommonType> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Mcount == 0) {
        return default!;
    }
    return new slice<Method>(new ReadOnlySpan<Method>((Method*)(uintptr)(addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), (uintptr)t.Moff, tMcount0ˢ)), (int)(t.Mcount)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string tXcount0ˢ = "t.xcount > 0"u8;

public static unsafe slice<Method> ExportedMethods(this ж<UncommonType> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Xcount == 0) {
        return default!;
    }
    return new slice<Method>(new ReadOnlySpan<Method>((Method*)(uintptr)(addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), (uintptr)t.Moff, tXcount0ˢ)), (int)(t.Xcount)));
}

internal static @unsafe.Pointer addChecked(@unsafe.Pointer p, uintptr x, @string whySafe) {
    return (@unsafe.Pointer)((uintptr)p + x);
}

[GoType] partial struct Imethod {
    public NameOff Name;
    public TypeOff Typ;
}

[GoType] partial struct ΔArrayType {
    public partial ref Type Type { get; }
    public ж<Type> Elem;
    public ж<Type> Slice;
    public uintptr Len;
}

public static nint Len(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() == Array) {
        return (nint)(Ꮡt.Reinterpret<Type, ΔArrayType>()).Value.Len;
    }
    return 0;
}

public static ж<Type> Common(this ж<Type> Ꮡt) {
    return Ꮡt;
}

[GoType("num:nint")] partial struct ΔChanDir;

public static ΔChanDir RecvDir => /* 1 << iota */ 1;
public static ΔChanDir SendDir => 2;
public static ΔChanDir BothDir => /* RecvDir | SendDir */ 3;
public static ΔChanDir InvalidDir => 0;

[GoType] partial struct ChanType {
    public partial ref Type Type { get; }
    public ж<Type> Elem;
    public ΔChanDir Dir;
}

[GoType] partial struct structTypeUncommon {
    public partial ref ΔStructType StructType { get; }
    internal UncommonType u;
}

public static ΔChanDir ChanDir(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() == Chan) {
        var ch = Ꮡt.Reinterpret<Type, ChanType>();
        return (~ch).Dir;
    }
    return InvalidDir;
}

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
    public partial ref ΔMapType MapType { get; }
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

public static ж<Type> Elem(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == Array) {
        var tt = Ꮡt.Reinterpret<Type, ΔArrayType>();
        return (~tt).Elem;
    }
    if (exprᴛ1 == Chan) {
        var tt = Ꮡt.Reinterpret<Type, ChanType>();
        return (~tt).Elem;
    }
    if (exprᴛ1 == Map) {
        var tt = Ꮡt.Reinterpret<Type, ΔMapType>();
        return (~tt).Elem;
    }
    if (exprᴛ1 == Pointer) {
        var tt = Ꮡt.Reinterpret<Type, PtrType>();
        return (~tt).Elem;
    }
    if (exprᴛ1 == Slice) {
        var tt = Ꮡt.Reinterpret<Type, SliceType>();
        return (~tt).Elem;
    }

    return default!;
}

public static ж<ΔStructType> StructType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Struct) {
        return default!;
    }
    return Ꮡt.Reinterpret<Type, ΔStructType>();
}

public static ж<ΔMapType> MapType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Map) {
        return default!;
    }
    return Ꮡt.Reinterpret<Type, ΔMapType>();
}

public static ж<ΔArrayType> ArrayType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Array) {
        return default!;
    }
    return Ꮡt.Reinterpret<Type, ΔArrayType>();
}

public static ж<ΔFuncType> FuncType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Func) {
        return default!;
    }
    return Ꮡt.Reinterpret<Type, ΔFuncType>();
}

public static ж<ΔInterfaceType> InterfaceType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Interface) {
        return default!;
    }
    return Ꮡt.Reinterpret<Type, ΔInterfaceType>();
}

[GoRecv] public static uintptr Size(this ref Type t) {
    return t.Size_;
}

[GoRecv] public static nint Align(this ref Type t) {
    return (nint)t.Align_;
}

[GoRecv] public static nint FieldAlign(this ref Type t) {
    return (nint)t.FieldAlign_;
}

[GoType] partial struct ΔInterfaceType {
    public partial ref Type Type { get; }
    public ΔName PkgPath;
    public slice<Imethod> Methods;
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

[GoRecv] public static nint NumMethod(this ref ΔInterfaceType t) {
    return len(t.Methods);
}

[GoType] partial struct ΔMapType {
    public partial ref Type Type { get; }
    public ж<Type> Key;
    public ж<Type> Elem;
    public ж<Type> Bucket;
    public Func<@unsafe.Pointer, uintptr, uintptr> Hasher;
    public uint8 KeySize;
    public uint8 ValueSize;
    public uint16 BucketSize;
    public uint32 Flags;
}

[GoRecv] public static bool IndirectKey(this ref ΔMapType mt) {
    return (uint32)(mt.Flags & 1) != 0;
}

[GoRecv] public static bool IndirectElem(this ref ΔMapType mt) {
    return (uint32)(mt.Flags & 2) != 0;
}

[GoRecv] public static bool ReflexiveKey(this ref ΔMapType mt) {
    return (uint32)(mt.Flags & 4) != 0;
}

[GoRecv] public static bool NeedKeyUpdate(this ref ΔMapType mt) {
    return (uint32)(mt.Flags & 8) != 0;
}

[GoRecv] public static bool HashMightPanic(this ref ΔMapType mt) {
    return (uint32)(mt.Flags & 16) != 0;
}

public static ж<Type> Key(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() == Map) {
        return (Ꮡt.Reinterpret<Type, ΔMapType>()).Value.Key;
    }
    return default!;
}

[GoType] partial struct SliceType {
    public partial ref Type Type { get; }
    public ж<Type> Elem;
}

[GoType] partial struct ΔFuncType {
    public partial ref Type Type { get; }
    public uint16 InCount;
    public uint16 OutCount;
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string tInCount0ˢ = "t.inCount > 0"u8;

public static unsafe slice<ж<Type>> InSlice(this ж<ΔFuncType> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var uadd = /* unsafe.Sizeof(*t) */ (uintptr)56;
    if ((TFlag)(t.TFlag & TFlagUncommon) != 0) {
        uadd += /* unsafe.Sizeof(UncommonType{}) */ (uintptr)16;
    }
    if (t.InCount == 0) {
        return default!;
    }
    return new slice<ж<Type>>(new ReadOnlySpan<ж<Type>>((Type**)(uintptr)(addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), uadd, tInCount0ˢ)), (int)(t.InCount)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string outCount0ˢ = "outCount > 0"u8;

public static unsafe slice<ж<Type>> OutSlice(this ж<ΔFuncType> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var outCount = (uint16)t.NumOut();
    if (outCount == 0) {
        return default!;
    }
    var uadd = /* unsafe.Sizeof(*t) */ (uintptr)56;
    if ((TFlag)(t.TFlag & TFlagUncommon) != 0) {
        uadd += /* unsafe.Sizeof(UncommonType{}) */ (uintptr)16;
    }
    return new slice<ж<Type>>(new ReadOnlySpan<ж<Type>>((Type**)(uintptr)(addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), uadd, outCount0ˢ)) + (int)(t.InCount), (int)(t.InCount + outCount) - (int)(t.InCount)));
}

[GoRecv] public static bool IsVariadic(this ref ΔFuncType t) {
    return (uint16)(t.OutCount & ((uint16)(1 << (int)(15)))) != 0;
}

[GoType] partial struct PtrType {
    public partial ref Type Type { get; }
    public ж<Type> Elem;
}

[GoType] partial struct StructField {
    public ΔName Name;
    public ж<Type> Typ;
    public uintptr Offset;
}

[GoRecv] public static bool Embedded(this ref StructField f) {
    return f.Name.IsEmbedded();
}

[GoType] partial struct ΔStructType {
    public partial ref Type Type { get; }
    public ΔName PkgPath;
    public slice<StructField> Fields;
}

[GoType] partial struct ΔName {
    public ж<byte> Bytes;
}

public static ж<byte> DataChecked(this ΔName n, nint off, @string whySafe) {
    return (ж<byte>)(uintptr)(addChecked(@unsafe.Pointer.FromPinnedBox(n.Bytes), (uintptr)off, whySafe));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string theRuntimeDoesnTNeedToˢ = "the runtime doesn't need to give you a reason"u8;

public static ж<byte> Data(this ΔName n, nint off) {
    return (ж<byte>)(uintptr)(addChecked(@unsafe.Pointer.FromPinnedBox(n.Bytes), (uintptr)off, theRuntimeDoesnTNeedToˢ));
}

public static bool IsExported(this ΔName n) {
    return (byte)((n.Bytes.Value) & ((byte)(1 << (int)(0)))) != 0;
}

public static bool HasTag(this ΔName n) {
    return (byte)((n.Bytes.Value) & ((byte)(1 << (int)(1)))) != 0;
}

public static bool IsEmbedded(this ΔName n) {
    return (byte)((n.Bytes.Value) & ((byte)(1 << (int)(3)))) != 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string readVarintˢ = "read varint"u8;

public static (nint, nint) ReadVarint(this ΔName n, nint off) {
    nint v = 0;
    for (nint i = 0; ᐧ ; i++) {
        var x = n.DataChecked(off + i, readVarintˢ).Value;
        v += ((nint)((byte)(x & 0x7f))).Lsh((uint64)((7 * i)));
        if ((byte)(x & 0x80) == 0) {
            return (i + 1, v);
        }
    }
}

public static bool IsBlank(this ΔName n) {
    if (n.Bytes == nil) {
        return false;
    }
    var (_, l) = n.ReadVarint(1);
    return l == 1 && n.Data(2).Value == (rune)'_';
}

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
private static readonly @string nonEmptyStringˢ = "non-empty string"u8;

public static @string Name(this ΔName n) {
    if (n.Bytes == nil) {
        return ""u8;
    }
    var (i, l) = n.ReadVarint(1);
    return @unsafe.String(n.DataChecked(1 + i, nonEmptyStringˢ), l);
}

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
    copy(b[1..], nameLen[..(int)(nameLenLen)]);
    copy(b[(int)(1 + nameLenLen)..], n);
    if (len(tag) > 0) {
        var tb = b[(int)(1 + nameLenLen + len(n))..];
        copy(tb, tagLen[..(int)(tagLenLen)]);
        copy(tb[(int)(tagLenLen)..], tag);
    }
    return new ΔName(Bytes: Ꮡ(b, 0));
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
