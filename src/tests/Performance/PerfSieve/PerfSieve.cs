namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static (nint, nint) sieve(nint n) {
    var composite = new slice<bool>(n);
    nint count = 0;
    nint sum = 0;
    for (nint i = 2; i < n; i++) {
        if (!composite[i]) {
            count++;
            sum += i;
            for (nint j = i * i; j < n; j += i) {
                composite[j] = true;
            }
        }
    }
    return (count, sum);
}

internal static void Main() {
    var start = time.Now().UnixNano();

    nint count = 0;
    nint sum = 0;
    for (nint r = 0; r < 3; r++) {
        var (c, s) = sieve(10000000);
        count += c;
        sum += s;
    }

    var elapsed = time.Now().UnixNano() - start;
    fmt.Println("checksum:", count, sum);
    fmt.Println("elapsed_ns:", elapsed);
}

} // end main_package
