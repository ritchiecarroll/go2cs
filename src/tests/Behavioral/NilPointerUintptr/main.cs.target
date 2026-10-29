namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct T {
    internal nint x;
}

internal static readonly ж<T> Ꮡglobal = new StaticBox<T>(new T(x: 42));
internal static ref T global => ref ((StaticBox<T>)Ꮡglobal).ValueSlot;

internal static uintptr gPtr = (uintptr)Ꮡglobal;

internal static uintptr gNil = (uintptr)(@unsafe.Pointer)default;

internal static void Main() {
    fmt.Println(gPtr != 0);
    fmt.Println(gNil == 0);

    ж<T> p = default;
    fmt.Println((uintptr)p == 0);

    var q = Ꮡ(new T(x: 5));
    fmt.Println((uintptr)q != 0);
    fmt.Println(q.x);
}

} // end main_package
