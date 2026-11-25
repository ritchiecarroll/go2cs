namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string tmpstring(slice<byte> b) { return ""; }

public static byte LiteralByteAt(@string s) { return (byte)'?'; }

internal static void trySelect() { }

internal static void wrapperRecv() { }

partial interface I {
    nint M();
}

partial struct T {
    internal nint n;
}

public static nint M(this T t) { return t.n; }

internal static map<@string, nint> m = new map<@string, nint>{["key"] = 1};

internal static nint lookup(slice<byte> b) { return m[builtin.tmpstring(b)]; }

internal static nint sumIndexes(slice<nint> s) {
    [GoFunc("sumIndexes.func1")] nint len(slice<nint> x) => 1000;
    nint total = 0;

    foreach (var i in range(builtin.len(s))) {
        total += i * 100;
    }

    return total + len(s) / 1000;
}

internal static byte byteAt(nint i) { return builtin.LiteralByteAt("golib"u8, i); }

internal static @string poll(channel<nint> c) {
    var cᴛ1 = c;
    switch (builtin.trySelect(ᐸꟷ(cᴛ1, ꓸꓸꓸ))) {
    case 0 when cᴛ1.ꟷᐳ(out var v): {
        return fmt.Sprint("received ", v);
    }
    default: {
        return "nothing ready";
    }}
}

internal static void main() {
    trySelect();
    wrapperRecv();

    fmt.Println("lookup:", lookup(slice<byte>("key"u8)));
    fmt.Println("sum:", sumIndexes([1, 2, 3, 4, 5, 6, 7, 8]));
    fmt.Printf("byte: %c %c\n"u8, byteAt(2), LiteralByteAt("x"));

    var c = new channel<nint>(1);
    fmt.Println("poll:", poll(c));
    c.ᐸꟷ(5);
    fmt.Println("poll:", poll(c));

    var method = ((Func<I, nint>)([GoWrapper("I.M")] (p0) => builtin.wrapperRecv(p0).M()));
    fmt.Println("method:", method(new T(n: 7)));
}

} // end main_package
