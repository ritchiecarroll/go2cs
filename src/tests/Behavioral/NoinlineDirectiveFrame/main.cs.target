namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial @string callerName() {
    var (pc, _, _, ok) = runtime.Caller(1);
    if (!ok) {
        return "<no caller>";
    }
    return runtime.FuncForPC(pc).Name();
}

internal static partial @string keeper(slice<byte> b, nint i) {
    @string name = callerName();
    return fmt.Sprintf("%s read %d"u8, name, b[i]);
}

partial struct counter {
    internal nint n;
}

internal static partial @string bump(this ref counter c) {
    c.n++;
    return callerName();
}

internal static partial @string forward(slice<byte> b, nint i) {
    return keeper(b, i);
}

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

    fmt.Println("bumped", (~c).n);
}

} // end main_package
