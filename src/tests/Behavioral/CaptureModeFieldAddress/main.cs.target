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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object localˢ = (@string)"local:"u8;

internal static void localStruct() {
    ref var x = ref heap(new holder(), out var Ꮡx);
    var v = Ꮡx.of(holder.Ꮡi).Add(5);
    Ꮡx.of(holder.Ꮡi).Add(2);
    fmt.Println(localˢ, v, Ꮡx.of(holder.Ꮡi).Load(), x.before, x.after);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object paramˢ = (@string)"param:"u8;

internal static void valueParam(holder xʗp) {
    ref var x = ref heap(xʗp, out var Ꮡx);

    Ꮡx.of(holder.Ꮡi).Store(3);
    Ꮡx.of(holder.Ꮡi).Add(4);
    fmt.Println(paramˢ, Ꮡx.of(holder.Ꮡi).Load());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object switchˢ = (@string)"switch:"u8;

internal static void typeSwitchCase(any v) {
    switch (v.type()) {
    case holder tᴛ1: {
        ref var t = ref heap(tᴛ1, out var Ꮡt);
        Ꮡt.of(holder.Ꮡi).Add(9);
        fmt.Println(switchˢ, Ꮡt.of(holder.Ꮡi).Load());
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object anonˢ = (@string)"anon:"u8;

internal partial struct anonStruct_x /*dyn*/ {
    internal atomic.Int32 i;
}

internal static void anonStruct() {
    ref var x = ref heap(new anonStruct_x(), out var Ꮡx);
    Ꮡx.of(anonStruct_x.Ꮡi).Store(7);
    fmt.Println(anonˢ, Ꮡx.of(anonStruct_x.Ꮡi).Add(1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object controlˢ = (@string)"control:"u8;

internal static void nonCaptureControl() {
    wrap w = default!;
    w.c.inc();
    w.c.inc();
    fmt.Println(controlˢ, w.c.n);
}

internal static void Main() {
    localStruct();
    valueParam(new holder(nil));
    typeSwitchCase(new holder(nil));
    anonStruct();
    nonCaptureControl();
}

} // end main_package
