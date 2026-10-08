namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static @unsafe.Pointer noescape(@unsafe.Pointer p) {
    var x = (uintptr)p;
    return (@unsafe.Pointer)((uintptr)(x ^ 0));
}

partial struct builder {
    internal ж<builder> addr;
    internal slice<byte> buf;
}

internal static void copyCheck(this ж<builder> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (b.addr == nil){
        b.addr = Ꮡb;
    } else 
    if (b.addr != Ꮡb) {
        throw panic("builder: illegal use of non-zero builder copied by value");
    }
}

internal static void write(this ж<builder> Ꮡb, @string s) {
    ref var b = ref Ꮡb.DerefOrNull();

    Ꮡb.copyCheck();
    b.buf = append(b.buf, s.ꓸꓸꓸ);
}

internal static @string String(this ref builder b) {
    return ((@string)b.buf);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string helloˢ = "hello"u8;
private static readonly @string worldˢ = "world"u8;
private static readonly object caughtˢ = (@string)"caught:"u8;
private static readonly @string seedˢ = "seed"u8;
private static readonly @string moreˢ = "more"u8;
private static readonly object unreachableˢ = (@string)"unreachable"u8;

internal static void Main() {
    ref var b = ref heap(new builder(), out var Ꮡb);
    Ꮡb.write(helloˢ);
    Ꮡb.write(", "u8);
    Ꮡb.write(worldˢ);
    fmt.Println(b.String());
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != default!) {
                        fmt.Println(caughtˢ, r);
                    }
                }
            }, ref ᒐ);
            ref var src = ref heap(new builder(), out var Ꮡsrc);
            Ꮡsrc.write(seedˢ);
            ref var cp = ref heap<builder>(out var Ꮡcp);
            cp = src;
            Ꮡcp.write(moreˢ);
            fmt.Println(unreachableˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
}

} // end main_package
