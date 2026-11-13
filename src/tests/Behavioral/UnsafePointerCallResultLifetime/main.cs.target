namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static slice<slice<byte>> sink;

internal static partial @unsafe.Pointer added(slice<byte> b) { return (uintptr)@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0)), 4); }

internal static partial @unsafe.Pointer held(slice<byte> b) {
    @unsafe.Pointer p = (uintptr)@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0)), 8);
    return p;
}

internal static partial @unsafe.Pointer same(@unsafe.Pointer p) { return p; }

internal static partial @unsafe.Pointer own(slice<byte> b) { return (uintptr)same(@unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0))); }

internal static void collect() {
    for (nint i = 0; i < 200000; i++) {
        sink = append(sink, new slice<byte>(128));
    }

    sink = default;
    runtime.GC();
    runtime.GC();
}

internal static void main() {
    const nint rounds = 20;
    nint viaReturn = default;
    nint viaVariable = default;
    nint ownPointer = default;

    for (nint round = 0; round < rounds; round++) {
        var b = new slice<byte>(64);
        @unsafe.Pointer p = (uintptr)added(b);
        @unsafe.Pointer q = (uintptr)held(b);
        @unsafe.Pointer r = (uintptr)own(b);

        collect();

        ((ж<byte>)(uintptr)(p)).Value = 1;
        ((ж<byte>)(uintptr)(q)).Value = 2;
        ((ж<byte>)(uintptr)(r)).Value = 3;

        if (b[4] != 1) {
            viaReturn++;
        }

        if (b[8] != 2) {
            viaVariable++;
        }

        if (b[0] != 3) {
            ownPointer++;
        }
    }

    fmt.Println("unsafe.Add in a return: wrong", viaReturn, "of", (nint)(rounds));
    fmt.Println("unsafe.Add into a variable: wrong", viaVariable, "of", (nint)(rounds));
    fmt.Println("a function returning its own pointer: wrong", ownPointer, "of", (nint)(rounds));

    if (viaReturn + viaVariable + ownPointer != 0) {
        os.Exit(1);
    }
}

} // end main_package
