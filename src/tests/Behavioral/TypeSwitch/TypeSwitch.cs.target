namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object iMNilˢ = (@string)"I'm nil"u8;
private static readonly object iMABoolˢ = (@string)"I'm a bool"u8;
private static readonly object heyˢ = (@string)"hey"u8;
private static readonly object intˢ = (@string)"int"u8;
private static readonly object int32ˢ = (@string)"int32"u8;
private static readonly object uintˢ = (@string)"uint"u8;
private static readonly object uint32ˢ = (@string)"uint32"u8;
private static readonly object otherˢ = (@string)"other"u8;
private static readonly object uintWordˢ = (@string)"uint word"u8;
private static readonly object uintptrWordˢ = (@string)"uintptr word"u8;
private static readonly object textˢ = (@string)"text"u8;

internal static void Main() {
    void whatAmI(any i) {
        switch (i.type()) {
        case null: {
            fmt.Println(iMNilˢ);
            break;
        }
        case bool t: {
            fmt.Println(iMABoolˢ);
            break;
        }
        case nint _:
        case int32 _:
        case int64 _:
        case uint64 _: {
            var t = i;
            fmt.Printf("I'm an int, specifically type %T\n"u8, t);
            break;
        }
        default: {
            var t = i;
            fmt.Printf("Don't know type %T\n"u8, t);
            break;
        }}
    }
    whatAmI(true);
    whatAmI((nint)(1));
    whatAmI((int64)2);
    whatAmI((uint64)2);
    whatAmI(heyˢ);
    whatAmI(default!);
    void classify(any i) {
        switch (i.type()) {
        case nint: {
            fmt.Println(intˢ);
            break;
        }
        case int32: {
            fmt.Println(int32ˢ);
            break;
        }
        case nuint: {
            fmt.Println(uintˢ);
            break;
        }
        case uint32: {
            fmt.Println(uint32ˢ);
            break;
        }
        default: {
            fmt.Println(otherˢ);
            break;
        }}

    }
    nint a = 1;
    int32 b = 2;
    nuint c = 3;
    uint32 d = 4;
    classify(a);
    classify(b);
    classify(c);
    classify(d);
    void kind(any i) {
        switch (i.type()) {
        case nuint: {
            fmt.Println(uintWordˢ);
            break;
        }
        case uint32: {
            fmt.Println(uintWordˢ);
            break;
        }
        case uintptr: {
            fmt.Println(uintptrWordˢ);
            break;
        }
        case @string: {
            fmt.Println(textˢ);
            break;
        }
        default: {
            fmt.Println(otherˢ);
            break;
        }}

    }
    nuint u = 5;
    uintptr p = 6;
    kind(u);
    kind(p);
    kind((@string)"x"u8);
    kind(3.14D);
    fmt.Println(sizeOf((int32)5), sizeOf((int64)7));
    ref var flag = ref heap(new bool(), out var Ꮡflag);
    ref var num = ref heap(new nint(), out var Ꮡnum);
    scanInto(Ꮡflag);
    scanInto(Ꮡnum);
    fmt.Println(flag, num);
    fmt.Println(probe(true), probe((nint)(7)), probe((@string)"ab"u8));
    marker mk = default!;
    fmt.Println(mk.tag(new byte[]{7, 8}.slice()));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string boolˢ = "bool"u8;
private static readonly @string oneˢ = "one"u8;
private static readonly @string manyˢ = "many"u8;

internal static @string probe(any x) {
    switch (x.type()) {
    case bool v: {
        _ = v;
        return boolˢ;
    }
    default: {
        var v = x;
        {
            nint vΔ1 = len(fmt.Sprint(v));
            switch (vΔ1) {
            case 1: {
                return oneˢ;
            }
            default: {
                return manyˢ;
            }}
        }

        break;
    }}
}

partial struct marker /*num:byte*/;

internal static byte tag(this marker _Δp0, slice<byte> b) {
    _ = b[1];
    return b[0];
}

internal static void scanInto(any v) {
    switch (v.type()) {
    case ж<bool> t: {
        t.Value = true;
        break;
    }
    case ж<nint> t: {
        t.Value = 42;
        break;
    }}
}

internal static nint sizeOf(any v) {
    switch (v.type()) {
    case int32 t: {
        nint sz = (nint)t + 1;
        return sz;
    }
    case int64 t: {
        nint sz = (nint)t + 2;
        return sz;
    }}
    return 0;
}

} // end main_package
