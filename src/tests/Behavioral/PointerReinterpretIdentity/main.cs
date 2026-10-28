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

    if (b.addr == nil) {
        b.addr = Ꮡb;
    } else if (b.addr != Ꮡb) {
        throw panic("builder: illegal use of non-zero builder copied by value");
    }
}

internal static void write(this ж<builder> Ꮡb, @string s) {
    ref var b = ref Ꮡb.DerefOrNull();

    Ꮡb.copyCheck();
    b.buf = append(b.buf, s.ꓸꓸꓸ);
}

internal static @string String(this ref builder b) {
    return (@string)b.buf;
}

internal static void Main() {
    ref var b = ref heap(new builder(), out var Ꮡb);
    Ꮡb.write("hello");
    Ꮡb.write(", ");
    Ꮡb.write("world");
    fmt.Println(b.String());

    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != nil) {
                        fmt.Println("caught:", r);
                    }
                }
            }, ref ᒐ);
            ref var src = ref heap(new builder(), out var Ꮡsrc);
            Ꮡsrc.write("seed");
            ref var cp = ref heap<builder>(out var Ꮡcp);
            cp = src;
            Ꮡcp.write("more");
            fmt.Println("unreachable");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
}

} // end main_package
