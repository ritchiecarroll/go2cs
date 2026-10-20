namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct vec {
    internal uint64 x, y;
}

internal static void addTo(ref uint64 @out, uint64 v) {
    @out += v;
}

internal static void scale(ref vec v, uint64 k) {
    addTo(ref nonnil(ref v).x, k);
    addTo(ref nonnil(ref v).y, k);
    @double(ref v);
}

internal static void @double(ref vec v) {
    v.x *= 2;
    v.y *= 2;
}

internal static void showVec(ref vec v) {
    fmt.Println("shown:", v.x, v.y);
}

internal static void boxedBump(ж<uint64> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    @out++;
}

internal static uint64 guarded(ж<uint64> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (Ꮡp == nil) {
        return 42;
    }
    return p;
}

internal static void bump(ref uint64 p) {
    p++;
}

internal static void printVal(ref uint64 p) {
    fmt.Println("defer sees:", p);
}

internal static void setVal(ref uint64 p, uint64 v, channel<bool> done) {
    p = v;
    done.ᐸꟷ(true);
}

internal static void deferReads() {
    GoFrame ᒐ = default;
    try {
        ref var x = ref heap(new uint64(), out var Ꮡx);
        defer(ᴛ1 => printVal(ref ᴛ1.DerefOrNull()), Ꮡx, ref ᒐ);
        x = 7;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static uint64 /*result*/ deferWrites() {
    heap<uint64>(out var Ꮡresult);
    GoFrame ᒐ = default;
    try {
        ref var result = ref Ꮡresult.Value;

        defer(ᴛ1 => bump(ref ᴛ1.DerefOrNull()), Ꮡresult, ref ᒐ);
        result = 6;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return Ꮡresult.Value;
}

internal static partial uint64 goWrites() {
    ref var x = ref heap(new uint64(), out var Ꮡx);
    var done = new channel<bool>(0);
    goǃ((ᴛ1, ᴛ2, ᴛ3) => setVal(ref ᴛ1.DerefOrNull(), ᴛ2, ᴛ3), Ꮡx, (uint64)(21), done);
    ᐸꟷ(done);
    return x;
}

internal static void Main() {
    uint64 total = default!;
    addTo(ref total, 5);
    addTo(ref total, 7);
    fmt.Println("total:", total);

    ref var v = ref heap<vec>(out var Ꮡv);
    v = new vec(x: 1, y: 2);
    scale(ref v, 3);
    fmt.Println("vec:", v.x, v.y);

    var p = Ꮡv;
    @double(ref (p).DerefOrNull());
    fmt.Println("vec2:", v.x, v.y);

    var arr = new uint64[]{10, 20, 30}.array();
    addTo(ref arr[1], 1);
    fmt.Println("arr:", arr[0], arr[1], arr[2]);

    var ᴛ1 = new vec(x: 4, y: 5);
    showVec(ref ᴛ1);

    deferReads();
    fmt.Println("defer wrote:", deferWrites());
    fmt.Println("go set:", goWrites());

    var f = boxedBump;
    ref var n = ref heap(new uint64(), out var Ꮡn);
    n = 9;
    f(Ꮡn);
    boxedBump(Ꮡn);
    fmt.Println("n:", n);

    fmt.Println("guarded nil:", guarded(nil));
    ref var g = ref heap(new uint64(), out var Ꮡg);
    g = 8;
    fmt.Println("guarded val:", guarded(Ꮡg));
}

} // end main_package
