namespace go;

using fmt = fmt_package;
using os = os_package;
using strconv = strconv_package;
using time = time_package;

partial class main_package {

internal static nint descend(nint n, channel<nint> c) {
    if (n == 0) {
        return ᐸꟷ(c);
    }

    return descend(n - 1, c) + 1;
}

internal static partial nint round(nint n) {
    var c = new channel<nint>(0);
    var e = new channel<nint>(0);
    var d = new channel<nint>(0);

    var cʗ1 = c;
    var dʗ1 = d;
    var eʗ1 = e;
    goǃ(() => {
        nint v = descend(n, cʗ1);
        v += ᐸꟷ(eʗ1);
        dʗ1.ᐸꟷ(v);
    });

    time.Sleep(10 * time.Millisecond);
    c.ᐸꟷ(1);
    time.Sleep(10 * time.Millisecond);
    e.ᐸꟷ(1);

    return ᐸꟷ(d);
}

internal static void main() {
    var depths = slice<nint>([1, 16, 256, 1024]);

    @string s = os.Getenv("MN_DEEP_N"); if (s != ""u8) {
        var (n, err) = strconv.Atoi(s);

        if (err != nil) {
            fmt.Println("MN_DEEP_N:", err);
            os.Exit(2);
        }

        depths = [n];
    }

    foreach (var (_, n) in range(depths)) {
        nint v = round(n);
        fmt.Printf("depth %d: %d\n"u8, n, v);
    }
}

} // end main_package
