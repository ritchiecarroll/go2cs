namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial void run() {
    uintptr n = 6;
    var a = new slice<byte>((nint)(n / 2));
    var b = new slice<uint64>((nint)(n));
    nuint u = 4;
    var c = new slice<nint>((nint)(u));
    uint32 w = 3;
    var d = new slice<int32>((nint)(w));
    uint64 q = 5;
    var e = new slice<byte>((nint)(q));
    var f = new slice<byte>((nint)(n / 2), (nint)(n));
    var g = new slice<byte>(2);
    nint k = 7;
    var h = new slice<nint>(k);
    var mp = new map<nint, nint>((nint)(n));
    mp[1] = 100;
    var ch = new channel<nint>((nint)(n));
    ch.ᐸꟷ(1);
    ch.ᐸꟷ(2);
    foreach (var (i, _) in a) {
        a[i] = (byte)i;
    }
    fmt.Println(len(a), len(b), len(c), len(d), len(e));
    fmt.Println(len(f), cap(f), len(g), len(h));
    fmt.Println(a[0], a[1], a[2]);
    fmt.Println(len(mp), cap(ch), len(ch));
}

internal static void Main() {
    run();
}

} // end main_package
