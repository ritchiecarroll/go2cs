namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using sync;

partial class main_package {

partial struct stdFunction /*global::go.unsafe_package.Pointer*/;

internal static stdFunction handler;

internal static stdFunction other;

partial struct counter /*global::go.sync.atomic_package.Uint32*/;

internal static uint32 Load(this ж<counter> Ꮡc) {
    return (Ꮡc.Reinterpret<counter, atomic.Uint32>()).Load();
}

internal static void Store(this ж<counter> Ꮡc, uint32 v) {
    (Ꮡc.Reinterpret<counter, atomic.Uint32>()).Store(v);
}

internal static uint32 Add(this ж<counter> Ꮡc, uint32 d) {
    return (Ꮡc.Reinterpret<counter, atomic.Uint32>()).Add(d);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object definedTypeMethodsˢ = (@string)"defined-type methods compiled"u8;

internal static void Main() {
    handler = other;
    fmt.Println(handler == other);
    ref var c = ref heap(new counter(), out var Ꮡc);
    Ꮡc.Store(10);
    _ = Ꮡc.Add(5);
    fmt.Println(definedTypeMethodsˢ);
    uintptr seed = 42;
    var h = ((handleT)seed);
    var k = openKey(h);
    fmt.Println(k == ((keyT)(uintptr)h), (uintptr)h);
    var back = ((handleT)(uintptr)k);
    fmt.Println(back == h);
}

partial struct handleT /*num:uintptr*/;

partial struct keyT /*num:uintptr*/;

internal static keyT openKey(handleT h) {
    return ((keyT)(uintptr)h);
}

} // end main_package
