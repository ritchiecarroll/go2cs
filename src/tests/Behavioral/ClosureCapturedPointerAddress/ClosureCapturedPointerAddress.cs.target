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

internal static void Main() {
    var head = Ꮡ(new node(v: 1, next: Ꮡ(new node(v: 2, next: Ꮡ(new node(v: 3, next: nil))))));
    ref var mToFlush = ref heap<ж<node>>(out var ᏑmToFlush);
    mToFlush = head;

    run(() => {
        while (ᏑmToFlush.ValueSlot != nil) {
            var prev = ᏑmToFlush;
            ᏑmToFlush.ValueSlot.v += 100;
            prev.ValueSlot = ᏑmToFlush.ValueSlot.next;
        }
    });

    fmt.Println("mToFlush is nil:", mToFlush == nil);

    nint sum = 0;
    for (var n = head; n != nil; n = n.next) {
        sum += n.v;
    }
    fmt.Println("sum:", sum);
}

} // end main_package
