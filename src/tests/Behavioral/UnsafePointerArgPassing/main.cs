namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static void add(this ref counter c, uintptr d) {
    c.n += (nint)d;
}

internal static partial @unsafe.Pointer add(@unsafe.Pointer p, uintptr x) {
    return (@unsafe.Pointer)((uintptr)p + x);
}

internal static void Main() {
    ref var @base = ref heap(new int64(), out var Ꮡbase);
    @base = 100;
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡbase);
    @unsafe.Pointer q = (uintptr)add(p, 16);
    @unsafe.Pointer r = (uintptr)add(p, 0);
    fmt.Println((uintptr)q - (uintptr)p == 16);
    fmt.Println((uintptr)r == (uintptr)p);
    var c = Ꮡ(new counter(nil));
    c.add(3);
    c.add(4);
    fmt.Println((~c).n);
}

} // end main_package
