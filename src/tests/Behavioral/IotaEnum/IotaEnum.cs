namespace go;

using fmt = fmt_package;
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

partial struct NameOff /*num:int32*/;
partial struct TypeOff /*num:int32*/;
partial struct TextOff /*num:int32*/;

public static Kind KindDirectIface => /* 1 << 5 */ 32;
public static Kind KindGCProg => /* 1 << 6 */ 64;
public static Kind KindMask => /* (1 << 5) - 1 */ 31;

partial struct TFlag /*num:uint8*/;

public static TFlag TFlagUncommon => /* 1 << 0 */ 1;
public static TFlag TFlagExtraStar => /* 1 << 1 */ 2;
public static TFlag TFlagNamed => /* 1 << 2 */ 4;
public static TFlag TFlagRegularMemory => /* 1 << 3 */ 8;
public static TFlag TFlagUnrolledBitmap => /* 1 << 4 */ 16;

public static @unsafe.Pointer NoEscape(@unsafe.Pointer p) {
    var x = (uintptr)p;
    return (@unsafe.Pointer)((uintptr)(x ^ 0));
}

partial struct EmptyInterface {
    public ж<Type> Type;
    public @unsafe.Pointer Data;
}

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

public static ж<Type> Elem(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    switch (t.Kind()) {
    case var ᴛ when ᴛ == Array: {
        var tt = Ꮡt.Reinterpret<Type, ArrayType>();
        return tt.of(main_package.ArrayType.ᏑElem).Value;
    }
    case var ᴛ when ᴛ == Map: {
        var tt = Ꮡt.Reinterpret<Type, MapType>();
        return tt.of(main_package.MapType.ᏑElem).Value;
    }}

    return default;
}

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

} partial class mainᴛType {

public static ж<MapType> MapType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Map) {
        return default;
    }
    return Ꮡt.Reinterpret<Type, MapType>();
}

} partial class main_package {

partial struct ArrayType {
    /*embed*/ public Type Type;
    public ж<Type> Elem;
    public ж<Type> Slice;
    public uintptr Len;
}

} partial class mainᴛType {

public static ж<ArrayType> ArrayType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Kind() != Array) {
        return default;
    }
    return Ꮡt.Reinterpret<Type, ArrayType>();
}

} partial class main_package {

internal const nint seqFirst = iota;
internal const nint seqSecond = 1;
internal const nint seqAgain = /* iota */ 2;

internal const int64 wideFirst = /* iota */ 0;
internal const int64 wideNext = 1;

internal static UntypedInt rawZero => iota;
internal static UntypedInt rawOne => /* iota */ 1;

internal static @string stateMachine(nint step) {
    const nint stateInit = iota;
    const nint stateDict = 1;

    const int64 wideLocal = /* iota */ 0;
    int64 offset = wideLocal;

    switch (step) {
    case var ᴛ when ᴛ == stateInit: {
        return fmt.Sprintf("init[%d]"u8, offset);
    }
    case var ᴛ when ᴛ == stateDict: {
        return fmt.Sprintf("dict[%d]"u8, offset);
    }}

    return "unknown";
}

internal static void main() {
    fmt.Printf("Slice Kind Value: %s [%d]\n"u8, Slice.String(), (nint)Slice);
    fmt.Println((nint)(seqFirst), (nint)(seqSecond), (nint)(seqAgain));
    fmt.Println(wideFirst, wideNext);
    fmt.Println((nint)(rawZero), (nint)(rawOne));
    fmt.Println(stateMachine(0));
    fmt.Println(stateMachine(1));
    fmt.Println(stateMachine(2));
}

} // end main_package
