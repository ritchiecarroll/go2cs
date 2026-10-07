namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoType] partial struct heap {
    internal nint count;
}

[GoRecv] internal static nint alloc(this ref heap h) {
    h.count++;
    return h.count;
}

internal static ж<heap> Ꮡmheap = new StandardBox<heap>(default(heap));
internal static ref heap mheap => ref Ꮡmheap.Value;

internal static partial void keep(ж<heap> Ꮡh) {
    _ = Ꮡh;
}

internal static partial void run(Action f) {
    f();
}

internal static partial nint boxedLocal() {
    ref var h = ref heap(new heap(), out var Ꮡh);
    var p = Ꮡh;
    p.Value.count += 7;
    return h.count;
}

internal static void Main() {
    keep(Ꮡmheap);
    nint got = default!;
    run(() => {
        got = mheap.alloc();
        var p = Ꮡmheap.of(heap.Ꮡcount);
        p.Value += 10;
    });
    fmt.Println(got, mheap.count);
    fmt.Println(boxedLocal());
}

} // end main_package
