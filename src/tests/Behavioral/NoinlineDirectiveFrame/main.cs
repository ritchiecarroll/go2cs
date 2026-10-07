namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noCallerˢ = "<no caller>"u8;

internal static partial @string callerName() {
    var (pc, _, _, ok) = runtime.Caller(1);
    if (!ok) {
        return noCallerˢ;
    }
    return runtime.FuncForPC(pc).Name();
}

internal static partial @string keeper(slice<byte> b, nint i) {
    @string name = callerName();
    return fmt.Sprintf("%s read %d"u8, name, b[i]);
}

[GoType] partial struct counter {
    internal nint n;
}

[GoRecv] internal static partial @string bump(this ref counter c) {
    c.n++;
    return callerName();
}

internal static partial @string forward(slice<byte> b, nint i) {
    return keeper(b, i);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object bumpedˢ = (@string)"bumped"u8;

internal static void Main() {
    var b = new byte[]{7, 9}.slice();
    var c = Ꮡ(new counter(nil));
    for (nint i = 0; i < 200; i++) {
        @string k = keeper(b, i % 2);
        @string m = c.bump();
        @string f = forward(b, 1);
        if (i == 0 || i == 199) {
            fmt.Println(k);
            fmt.Println(m);
            fmt.Println(f);
        }
    }
    fmt.Println(bumpedˢ, (~c).n);
}

} // end main_package
