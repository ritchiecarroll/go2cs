namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal nint v;
    internal ж<node> next;
}

internal static void run(Action f) {
    f();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object mToFlushIsNilˢ = (@string)"mToFlush is nil:"u8;
private static readonly object sumˢ = (@string)"sum:"u8;

internal static void Main() {
    var head = Ꮡ(new node(v: 1, next: Ꮡ(new node(v: 2, next: Ꮡ(new node(v: 3, next: nil))))));
    ref var mToFlush = ref heap<ж<node>>(out var ᏑmToFlush);
    mToFlush = head;
    run(() => {
        while (ᏑmToFlush.ValueSlot != nil) {
            var prev = ᏑmToFlush;
            ᏑmToFlush.ValueSlot.Value.v += 100;
            prev.ValueSlot = ᏑmToFlush.ValueSlot.Value.next;
        }
    });
    fmt.Println(mToFlushIsNilˢ, mToFlush == nil);
    nint sum = 0;
    for (var n = head; n != nil; n = n.Value.next) {
        sum += n.Value.v;
    }
    fmt.Println(sumˢ, sum);
}

} // end main_package
