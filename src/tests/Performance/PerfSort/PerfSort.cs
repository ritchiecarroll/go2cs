namespace go;

using fmt = fmt_package;
using sort = sort_package;
using time = time_package;

partial class main_package {

internal static nint run(nint n) {
    var a = new slice<nint>(n);
    uint64 x = 88172645463325252UL;

    for (nint i = 0; i < n; i++) {
        x ^= (uint64)((x << 13));
        x ^= (uint64)((x >> 7));
        x ^= (uint64)((x << 17));
        a[i] = (nint)(x % 1000000007);
    }

    sort.Ints(a);

    return a[0] + a[n / 4] + a[n / 2] + a[3 * n / 4] + a[n - 1];
}

internal static void Main() {
    var start = time.Now().UnixNano();

    nint total = run(2000000);

    var elapsed = time.Now().UnixNano() - start;
    fmt.Println("checksum:", total);
    fmt.Println("elapsed_ns:", elapsed);
}

} // end main_package
