namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

partial struct holder {
    internal int32 before;
    internal atomic.Int32 i;
    internal int32 after;
}

partial struct ctr {
    internal int32 n;
}

internal static void inc(this ref ctr c) {
    c.n++;
}

partial struct wrap {
    internal ctr c;
}

internal static void localStruct() {
    ref var x = ref heap(new holder(), out var Ꮡx);
    var v = Ꮡx.of(holder.Ꮡi).Add(5);
    Ꮡx.of(holder.Ꮡi).Add(2);
    fmt.Println("local:", v, Ꮡx.of(holder.Ꮡi).Load(), x.before, x.after);
}

internal static void valueParam(holder xʗp) {
    ref var x = ref heap(xʗp, out var Ꮡx);

    Ꮡx.of(holder.Ꮡi).Store(3);
    Ꮡx.of(holder.Ꮡi).Add(4);
    fmt.Println("param:", Ꮡx.of(holder.Ꮡi).Load());
}

internal static void typeSwitchCase(any v) {
    switch (v.type()) {
    case holder tᴛ1: {
        ref var t = ref heap(tᴛ1, out var Ꮡt);
        Ꮡt.of(holder.Ꮡi).Add(9);
        fmt.Println("switch:", Ꮡt.of(holder.Ꮡi).Load());
        break;
    }}
}

internal partial struct anonStruct_x /*dyn*/ {
    internal atomic.Int32 i;
}

internal static void anonStruct() {
    ref var x = ref heap(new anonStruct_x(), out var Ꮡx);
    Ꮡx.of(anonStruct_x.Ꮡi).Store(7);
    fmt.Println("anon:", Ꮡx.of(anonStruct_x.Ꮡi).Add(1));
}

internal static void nonCaptureControl() {
    wrap w = default;
    w.c.inc();
    w.c.inc();
    fmt.Println("control:", w.c.n);
}

internal static void Main() {
    localStruct();
    valueParam(new holder());
    typeSwitchCase(new holder());
    anonStruct();
    nonCaptureControl();
}

} // end main_package
