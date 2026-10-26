namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static nint run(nint n) {
    nint total = 0;
    var buf = keyword();

    sstring bufᴛ1 = ((sstring)buf);
    for (nint i = 0; i < n; i++) {
        if (bufᴛ1 == "null"u8) {
            total++;
        }
        if (bufᴛ1 == "true"u8) {
            total += 2;
        }
        if (bufᴛ1 == "false"u8) {
            total += 4;
        }
    }

    return total;
}

internal static slice<byte> keyword() {
    @string src = "null";
    var b = new slice<byte>(len(src));
    for (nint i = 0; i < len(src); i++) {
        b[i] = src[i];
    }
    return b;
}

internal static void Main() {
    var start = time.Now().UnixNano();

    nint total = run(20000000);

    var elapsed = time.Now().UnixNano() - start;
    fmt.Println("checksum:", total);
    fmt.Println("elapsed_ns:", elapsed);
}

} // end main_package
