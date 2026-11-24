global using ptr = go.unsafe_package.Pointer;

namespace go;

using fmt = fmt_package;
using strings = strings_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct str {
    internal ptr p;
    internal nuint l;
}

partial struct bytes /*[]byte*/;

internal static void @try(@string name, Func<@string> f) {
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("try.func1")] () => {
            var r = recover(); if (r != nil) {
                @string text = fmt.Sprint(r);

                nint i = strings.Index(text, " ("); if (i >= 0) {
                    text = text.slice(0, i);
                }

                fmt.Printf("%s: panic: %s\n"u8, name, text);
            }
        }, ref ᒐ);

        fmt.Printf("%s: %s\n"u8, name, f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void main() {
    @try("string store", [GoFunc("main.func1")] () => {
        ref var cΔ1 = ref heap<slice<byte>>(slice<byte>("xyz"u8), out var ᏑcΔ1);
        throw panic("go2cs: store through a read-only view: a string read over a []byte cannot be assigned; Go would replace the slice's data pointer and length (see docs/Limitations.md)");
        return fmt.Sprint(len(cΔ1));
    });
    @try("string op-store", [GoFunc("main.func2")] () => {
        ref var cΔ2 = ref heap<slice<byte>>(slice<byte>("abcd"u8), out var ᏑcΔ2);
        throw panic("go2cs: store through a read-only view: a string read over a []byte cannot be assigned; Go would replace the slice's data pointer and length (see docs/Limitations.md)");
        return fmt.Sprint(len(cΔ2));
    });
    @try("header store", [GoFunc("main.func3")] () => {
        ref var cΔ3 = ref heap<slice<byte>>(slice<byte>("hello"u8), out var ᏑcΔ3);
        throw panic("go2cs: store through a read-only view: a main.str read over a []byte cannot be assigned; Go would replace the slice's data pointer and length (see docs/Limitations.md)");
        return fmt.Sprint(len(cΔ3));
    });
    @try("tuple store", [GoFunc("main.func4")] () => {
        ref var cΔ4 = ref heap<slice<byte>>(slice<byte>("xy"u8), out var ᏑcΔ4);
        @string s = default;
        throw panic("go2cs: store through a read-only view: a string read over a []byte cannot be assigned; Go would replace the slice's data pointer and length (see docs/Limitations.md)");
        return fmt.Sprint(len(cΔ4), s);
    });
    @try("uint32 slice as string", [GoFunc("main.func5")] () => {
        ref var w = ref heap<slice<uint32>>(slice<uint32>([0x64636261]), out var Ꮡw);
        @string v = ~Ꮡw.Reinterpret<slice<uint32>, @string>();
        return fmt.Sprint(len(v), v[0]);
    });
    @try("named slice as string", [GoFunc("main.func6")] () => {
        ref var nb = ref heap<bytes>((bytes)slice<byte>((@string)"named"u8), out var Ꮡnb);
        return ~Ꮡnb.Reinterpret<bytes, @string>();
    });
    @try("named slice header", [GoFunc("main.func7")] () => {
        ref var nb = ref heap<bytes>((bytes)slice<byte>((@string)"named"u8), out var Ꮡnb);
        var h = ~Ꮡnb.Reinterpret<bytes, str>();
        return fmt.Sprint(h.l);
    });

    @try("store through a variable, then read", [GoFunc("main.func8")] () => {
        ref var cΔ5 = ref heap<slice<byte>>(slice<byte>("xyz"u8), out var ᏑcΔ5);
        var p = ᏑcΔ5.Reinterpret<slice<byte>, @string>();
        p.Value = "q";
        return p.Value;
    });
    @try("store through a variable, last access", [GoFunc("main.func9")] () => {
        ref var cΔ6 = ref heap<slice<byte>>(slice<byte>("xyz"u8), out var ᏑcΔ6);
        var p = ᏑcΔ6.Reinterpret<slice<byte>, @string>();
        p.Value = "q";
        return fmt.Sprint(len(cΔ6));
    });

    ref var c = ref heap<slice<byte>>(slice<byte>("end"u8), out var Ꮡc);
    throw panic("go2cs: store through a read-only view: a string read over a []byte cannot be assigned; Go would replace the slice's data pointer and length (see docs/Limitations.md)");
    fmt.Println("not reached in the conversion:", len(c));
}

} // end main_package
