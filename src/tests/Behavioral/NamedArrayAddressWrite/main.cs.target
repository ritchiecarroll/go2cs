namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct small /*[8]byte*/;

partial struct holder {
    internal int32 n;
    internal small buf;
}

internal static readonly ж<small> Ꮡglobal = new StaticBox<small>(default(small));
internal static ref small global => ref ((StaticBox<small>)Ꮡglobal).ValueSlot;

internal static void fill(@unsafe.Pointer p, byte b) {
    (@unsafe.Pointer.To<byte>(p)).Value = b;
}

internal static void fillNumber(uintptr p, byte b) {
    (@unsafe.Pointer.To<byte>((@unsafe.Pointer)p)).Value = b;
}

internal static void memcpy(uintptr dest, uintptr src, uintptr n) {
    copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)dest, 0, (nint)(n), (nint)(n), 1048576), @unsafe.Pointer.View<byte>((@unsafe.Pointer)src, 0, (nint)(n), (nint)(n), 1048576));
}

internal static void main() {
    ref var local = ref heap(new small(), out var Ꮡlocal);
    fill(@unsafe.Pointer.FromPinnedBox(Ꮡlocal), 41);
    fmt.Println("la", local[0]);

    fillNumber((uintptr)Ꮡglobal, 42);
    fmt.Println("lb", global[0]);

    var h = Ꮡ(new holder());
    fill(@unsafe.Pointer.FromPinnedBox(h.Ꮡbuf), 43);
    fmt.Println("lc", h.buf[0]);

    var x = @new<small>();
    fillNumber((uintptr)x, 44);
    fmt.Println("ld", x.Value[0]);

    ref var dst = ref heap(new small(), out var Ꮡdst);
    ref var src = ref heap<array<byte>>(array<byte>([1, 2, 3, 4, 5, 6, 7, 8]), out var Ꮡsrc);
    memcpy((uintptr)Ꮡdst, (uintptr)Ꮡsrc, 8);
    fmt.Println("le", dst);

    ref var used = ref heap(new small(), out var Ꮡused);
    used[7] = 9;
    fill(@unsafe.Pointer.FromPinnedBox(Ꮡused), 45);
    fmt.Println("lf", used[0], used[7]);

    ref var plain = ref heap(new array<byte>(8), out var Ꮡplain);
    fill(@unsafe.Pointer.FromPinnedBox(Ꮡplain), 46);
    fmt.Println("lg", plain[0]);
}

} // end main_package
