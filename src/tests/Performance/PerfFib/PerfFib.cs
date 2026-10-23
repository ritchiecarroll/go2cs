namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static nint fib(nint n) {
    if (n < 2) {
        return n;
    }
    return fib(n - 1) + fib(n - 2);
}

internal static void Main() {
    var start = time.Now().UnixNano();

    nint sum = 0;
    for (nint i = 0; i < 5; i++) {
        sum += fib(34);
    }

    var elapsed = time.Now().UnixNano() - start;
    fmt.Println("checksum:", sum);
    fmt.Println("elapsed_ns:", elapsed);
}

} // end main_package
