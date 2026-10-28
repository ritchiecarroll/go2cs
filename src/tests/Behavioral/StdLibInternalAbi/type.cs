// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go;

using @unsafe = unsafe_package;

[GoFieldAccess(typeof(ChanType))]
partial class main_package {

partial struct Type {
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

partial struct ΔKind /*num:uint8*/;

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

partial struct TFlag /*num:uint8*/;

public static TFlag TFlagUncommon => /* 1 << 0 */ 1;
public static TFlag TFlagExtraStar => /* 1 << 1 */ 2;
public static TFlag TFlagNamed => /* 1 << 2 */ 4;
public static TFlag TFlagRegularMemory => /* 1 << 3 */ 8;
public static TFlag TFlagUnrolledBitmap => /* 1 << 4 */ 16;

partial struct NameOff /*num:int32*/;

partial struct TypeOff /*num:int32*/;

partial struct TextOff /*num:int32*/;

public static @string String(this ΔKind k) {
    if ((nint)(uint8)k < len(kindNames)) {
        return kindNames[k];
    }
    return kindNames[0];
}

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
    [ΔString] = "string",
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
    {
        var t = TypeOf((v).OrTypedNilOf()); if (t != nil) {
            return t;
        }
    }
    return TypeOf((ж<T>)nil).Elem();
}

public static ΔKind Kind(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindMask);
}

public static bool HasName(this ref Type t) {
    return (TFlag)(t.TFlag & TFlagNamed) != 0;
}

public static bool Pointers(this ref Type t) {
    return t.PtrBytes != 0;
}

public static bool IfaceIndir(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindDirectIface) == 0;
}

public static bool IsDirectIface(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindDirectIface) != 0;
}

public static slice<byte> GcSlice(this ref Type t, uintptr begin, uintptr end) {
    return @unsafe.Slice(t.GCData, (nint)end).slice((nint)(begin));
}

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

internal static @unsafe.Pointer addChecked(@unsafe.Pointer p, uintptr x, @string whySafe) {
    return (@unsafe.Pointer)((uintptr)p + x);
}

partial struct Imethod {
    public NameOff Name;
    public TypeOff Typ;
}

partial struct ΔArrayType {
    /*embed*/ public Type Type;
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

partial struct ΔChanDir /*num:nint*/;

public static ΔChanDir RecvDir => /* 1 << iota */ 1;
public static ΔChanDir SendDir => 2;
public static ΔChanDir BothDir => /* RecvDir | SendDir */ 3;
public static ΔChanDir InvalidDir => 0;

partial struct ChanType {
    /*embed*/ public Type Type;
    public ж<Type> Elem;
    public ΔChanDir Dir;
}

partial struct structTypeUncommon {
    /*embed*/ public ΔStructType StructType;
    internal UncommonType u;
}

public static ΔChanDir ChanDir(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() == Chan) {
        var ch = Ꮡt.Reinterpret<Type, ChanType>();
        return ch.Dir;
    }
    return InvalidDir;
}

internal partial struct Uncommon_u /*dyn*/ {
    /*embed*/ public PtrType PtrType;
    internal UncommonType u;
}

internal partial struct Uncommon_uᴛ1 /*dyn*/ {
    /*embed*/ public ΔFuncType FuncType;
    internal UncommonType u;
}

internal partial struct Uncommon_uᴛ2 /*dyn*/ {
    /*embed*/ public SliceType SliceType;
    internal UncommonType u;
}

internal partial struct Uncommon_uᴛ3 /*dyn*/ {
    /*embed*/ public ΔArrayType ArrayType;
    internal UncommonType u;
}

internal partial struct Uncommon_uᴛ4 /*dyn*/ {
    /*embed*/ public ChanType ChanType;
    internal UncommonType u;
}

internal partial struct Uncommon_uᴛ5 /*dyn*/ {
    /*embed*/ public ΔMapType MapType;
    internal UncommonType u;
}

internal partial struct Uncommon_uᴛ6 /*dyn*/ {
    /*embed*/ public ΔInterfaceType InterfaceType;
    internal UncommonType u;
}

internal partial struct Uncommon_uᴛ7 /*dyn*/ {
    /*embed*/ public Type Type;
    internal UncommonType u;
}

public static ж<UncommonType> Uncommon(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if ((TFlag)(t.TFlag & TFlagUncommon) == 0) {
        return default;
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
        return tt.of(main_package.ΔArrayType.ᏑElem).Value;
    }
    if (exprᴛ1 == Chan) {
        var tt = Ꮡt.Reinterpret<Type, ChanType>();
        return tt.of(ChanType.ᏑElem).Value;
    }
    if (exprᴛ1 == Map) {
        var tt = Ꮡt.Reinterpret<Type, ΔMapType>();
        return tt.of(main_package.ΔMapType.ᏑElem).Value;
    }
    if (exprᴛ1 == Pointer) {
        var tt = Ꮡt.Reinterpret<Type, PtrType>();
        return tt.of(PtrType.ᏑElem).Value;
    }
    if (exprᴛ1 == Slice) {
        var tt = Ꮡt.Reinterpret<Type, SliceType>();
        return tt.of(SliceType.ᏑElem).Value;
    }

    return default;
}

public static ж<ΔStructType> StructType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Struct) {
        return default;
    }
    return Ꮡt.Reinterpret<Type, ΔStructType>();
}

public static ж<ΔMapType> MapType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Map) {
        return default;
    }
    return Ꮡt.Reinterpret<Type, ΔMapType>();
}

public static ж<ΔArrayType> ArrayType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Array) {
        return default;
    }
    return Ꮡt.Reinterpret<Type, ΔArrayType>();
}

public static ж<ΔFuncType> FuncType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Func) {
        return default;
    }
    return Ꮡt.Reinterpret<Type, ΔFuncType>();
}

public static ж<ΔInterfaceType> InterfaceType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Interface) {
        return default;
    }
    return Ꮡt.Reinterpret<Type, ΔInterfaceType>();
}

public static uintptr Size(this ref Type t) {
    return t.Size_;
}

public static nint Align(this ref Type t) {
    return (nint)t.Align_;
}

public static nint FieldAlign(this ref Type t) {
    return (nint)t.FieldAlign_;
}

partial struct ΔInterfaceType {
    /*embed*/ public Type Type;
    public ΔName PkgPath;
    public slice<Imethod> Methods;
}

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
        var tt = Ꮡt.Reinterpret<Type, ΔInterfaceType>();
        return tt.NumMethod();
    }
    return len(Ꮡt.ExportedMethods());
}

public static nint NumMethod(this ref ΔInterfaceType t) {
    return len(t.Methods);
}

partial struct ΔMapType {
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

public static bool IndirectKey(this ref ΔMapType mt) {
    return (mt.Flags & 1) != 0;
}

public static bool IndirectElem(this ref ΔMapType mt) {
    return (mt.Flags & 2) != 0;
}

public static bool ReflexiveKey(this ref ΔMapType mt) {
    return (mt.Flags & 4) != 0;
}

public static bool NeedKeyUpdate(this ref ΔMapType mt) {
    return (mt.Flags & 8) != 0;
}

public static bool HashMightPanic(this ref ΔMapType mt) {
    return (mt.Flags & 16) != 0;
}

public static ж<Type> Key(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() == Map) {
        return (Ꮡt.Reinterpret<Type, ΔMapType>()).Value.Key;
    }
    return default;
}

partial struct SliceType {
    /*embed*/ public Type Type;
    public ж<Type> Elem;
}

partial struct ΔFuncType {
    /*embed*/ public Type Type;
    public uint16 InCount;
    public uint16 OutCount;
}

public static ж<Type> In(this ж<ΔFuncType> Ꮡt, nint i) {
    return Ꮡt.InSlice()[i];
}

public static nint NumIn(this ref ΔFuncType t) {
    return (nint)t.InCount;
}

public static nint NumOut(this ref ΔFuncType t) {
    return (nint)((uint16)(t.OutCount & ((1 << 15) - 1)));
}

public static ж<Type> Out(this ж<ΔFuncType> Ꮡt, nint i) {
    return (Ꮡt.OutSlice()[i]);
}

public static slice<ж<Type>> InSlice(this ж<ΔFuncType> Ꮡt) {
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

public static slice<ж<Type>> OutSlice(this ж<ΔFuncType> Ꮡt) {
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

public static bool IsVariadic(this ref ΔFuncType t) {
    return (uint16)(t.OutCount & ((uint16)(1 << 15))) != 0;
}

partial struct PtrType {
    /*embed*/ public Type Type;
    public ж<Type> Elem;
}

partial struct StructField {
    public ΔName Name;
    public ж<Type> Typ;
    public uintptr Offset;
}

public static bool Embedded(this ref StructField f) {
    return f.Name.IsEmbedded();
}

partial struct ΔStructType {
    /*embed*/ public Type Type;
    public ΔName PkgPath;
    public slice<StructField> Fields;
}

partial struct ΔName {
    public ж<byte> Bytes;
}

public static ж<byte> DataChecked(this ΔName n, nint off, @string whySafe) {
    return (ж<byte>)(uintptr)(addChecked(@unsafe.Pointer.FromPinnedBox(n.Bytes), (uintptr)off, whySafe));
}

public static ж<byte> Data(this ΔName n, nint off) {
    return (ж<byte>)(uintptr)(addChecked(@unsafe.Pointer.FromPinnedBox(n.Bytes), (uintptr)off, "the runtime doesn't need to give you a reason"));
}

public static bool IsExported(this ΔName n) {
    return (byte)((n.Bytes.Value) & ((byte)(1 << 0))) != 0;
}

public static bool HasTag(this ΔName n) {
    return (byte)((n.Bytes.Value) & ((byte)(1 << 1))) != 0;
}

public static bool IsEmbedded(this ΔName n) {
    return (byte)((n.Bytes.Value) & ((byte)(1 << 3))) != 0;
}

public static (nint, nint) ReadVarint(this ΔName n, nint off) {
    nint v = 0;
    for (nint i = 0; ᐧ ; i++) {
        var x = n.DataChecked(off + i, "read varint").Value;
        v += ((nint)((byte)(x & 0x7f))).Lsh((int64)(7 * i));
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
        var b = (byte)(n & 0x7f);
        n >>= (int)(7);
        if (n == 0) {
            buf[i] = b;
            return i + 1;
        }
        buf[i] = (byte)(b | 0x80);
    }
}

public static @string Name(this ΔName n) {
    if (n.Bytes == nil) {
        return "";
    }
    var (i, l) = n.ReadVarint(1);
    return @unsafe.String(n.DataChecked(1 + i, "non-empty string"), l);
}

public static @string Tag(this ΔName n) {
    if (!n.HasTag()) {
        return "";
    }
    var (i, l) = n.ReadVarint(1);
    var (i2, l2) = n.ReadVarint(1 + i + l);
    return @unsafe.String(n.DataChecked(1 + i + l + i2, "non-empty string"), l2);
}

public static ΔName NewName(@string n, @string tag, bool exported, bool embedded) {
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
