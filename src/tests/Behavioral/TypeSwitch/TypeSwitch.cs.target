namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object heyˢ = (@string)"hey"u8;

internal static void Main() {
    void whatAmI(any i) {
        switch (i.type()) {
        case null: {
            fmt.Println("I'm nil");
            break;
        }
        case bool t: {
            fmt.Println("I'm a bool");
            break;
        }
        case nint _:
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
    whatAmI((nint)1);
    whatAmI((int64)2);
    whatAmI((uint64)2);
    whatAmI(heyˢ);
    whatAmI(default!);

    void classify(any i) {
        switch (i.type()) {
        case nint: {
            fmt.Println("int");
            break;
        }
        case int32: {
            fmt.Println("int32");
            break;
        }
        case nuint: {
            fmt.Println("uint");
            break;
        }
        case uint32: {
            fmt.Println("uint32");
            break;
        }
        default: {
            fmt.Println("other");
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
            fmt.Println("uint word");
            break;
        }
        case uintptr: {
            fmt.Println("uintptr word");
            break;
        }
        case @string: {
            fmt.Println("text");
            break;
        }
        default: {
            fmt.Println("other");
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

    fmt.Println(probe(true), probe((nint)7), probe("ab"));

    marker mk = default!;
    fmt.Println(mk.tag(new byte[]{7, 8}.slice()));
}

internal static @string probe(any x) {
    switch (x.type()) {
    case bool v: {
        _ = v;
        return "bool";
    }
    default: {
        var v = x;
        {
            nint vΔ1 = len(fmt.Sprint(v));
            switch (vΔ1) {
            case 1: {
                return "one";
            }
            default: {
                return "many";
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
