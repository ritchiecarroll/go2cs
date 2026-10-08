namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct loc {
    internal @string name;
    internal nint offset;
}

internal static ж<loc> ᏑutcLoc = new StandardBox<loc>(new loc(name: "UTC"u8, offset: 0));
internal static ref loc utcLoc => ref ᏑutcLoc.Value;

internal static ж<loc> get(this ж<loc> Ꮡl) {
    if (Ꮡl == nil) {
        return ᏑutcLoc;
    }
    return Ꮡl;
}

internal static (@string name, nint offset) lookup(this ж<loc> Ꮡl, nint sec) {
    @string name = default!;
    nint offset = default!;

    ref var l = ref Ꮡl.DerefOrNull();
    Ꮡl = Ꮡl.get(); l = ref Ꮡl.DerefOrNull();
    name = l.name;
    offset = l.offset + sec;
    return (name, offset);
}

internal static @string rename(this ж<loc> Ꮡl, @string suffix) {
    ref var l = ref Ꮡl.DerefOrNull();

    Ꮡl = Ꮡl.get(); l = ref Ꮡl.DerefOrNull();
    l.name += suffix;
    return l.name;
}

internal static @string span(ж<loc> Ꮡl, nint width) {
    ref var l = ref Ꮡl.DerefOrNull();

    Ꮡl = Ꮡl.get(); l = ref Ꮡl.DerefOrNull();
    return fmt.Sprintf("%s/%d"u8, l.name, l.offset + width);
}

internal static @string firstUseReads(this ж<loc> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    @string n = l.name;
    Ꮡl = Ꮡl.get(); l = ref Ꮡl.DerefOrNull();
    return n + "/"u8 + l.name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object recoveredˢ = (@string)"recovered"u8;

internal static void Main() {
    ж<loc> nilLoc = default!;
    var real = Ꮡ(new loc(name: "MST"u8, offset: -7));
    var (ᴛ1, ᴛ2) = nilLoc.lookup(3);
    fmt.Println(ᴛ1, ᴛ2);
    var (ᴛ3, ᴛ4) = real.lookup(3);
    fmt.Println(ᴛ3, ᴛ4);
    fmt.Println(span(nilLoc, 10));
    fmt.Println(span(real, 10));
    fmt.Println(nilLoc.rename("!"u8));
    fmt.Println(utcLoc.name);
    fmt.Println(real.rename("?"u8));
    fmt.Println((~real).name);
    fmt.Println(real.firstUseReads());
    var nilLocʗ1 = nilLoc;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != default!) {
                        fmt.Println(recoveredˢ);
                    }
                }
            }, ref ᒐ);
            fmt.Println(nilLocʗ1.firstUseReads());
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
}

} // end main_package
