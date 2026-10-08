namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct iter {
    internal nint i, n;
}

internal static void start(this ref iter it, nint n) {
    it.i = 0;
    it.n = n;
}

internal static bool valid(this ref iter it) {
    return it.i < it.n;
}

internal static void next(this ref iter it) {
    it.i++;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string abcˢ = "abc"u8;

internal static void Main() {
    iter u = default!;
    nint sum = 0;
    for (u.start(3); u.valid(); u.next()) {
        sum += u.i;
    }
    fmt.Println(sum);
    u.start(4);
    nint count = 0;
    for (; u.valid(); u.next()) {
        count++;
    }
    fmt.Println(count);
    @string @out = ""u8;
    for (@string p = abcˢ; p != ""u8; ) {
        @out += p[..1];
        p = p[1..];
    }
    fmt.Println(@out);
}

} // end main_package
