namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct E {
    internal nint n;
}

public static void Bare(this ref E e) {
}

public static nint Value(this ref E e, nint a) {
    return a;
}

public static nint Pair(this ref E e, nint a, nint b) {
    return a + b;
}

internal static partial nint line() {
    var (_, _, l, _) = runtime.Caller(1);
    return l;
}

internal static array<nint> got = new(6);

internal static void record(nint i, nint l) {
    got[i] = l;
}

internal static void shapes() {
    var e = Ꮡ(new E());

    e.Bare();
    e.n++;
    record(0, line());

    nint r = e.Value(2);
    record(1, line() + r - r);

    e.Value(3);
    record(2, line());

    r = e.Pair(line(), 0);
    record(3, r);

    e.n += 2;
    record(4, line());

    e.Bare();
    record(5, line());

    e.Bare();
}

internal static void Main() {
    nint runs = 1;
    if (os.Getenv("GO2CS_CALLER_LINE_WARM") == "1"u8) {
        runs = 400;
    }
    for (nint i = 0; i < runs; i++) {
        shapes();
        if (i % 100 == 99) {
            time.Sleep(200 * time.Millisecond);
        }
    }
    fmt.Println(got[0], got[1], got[2], got[3], got[4], got[5]);
}

} // end main_package
