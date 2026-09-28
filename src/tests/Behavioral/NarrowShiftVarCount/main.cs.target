namespace go;

using fmt = fmt_package;

partial class main_package {

internal const byte tcb = 200;

internal static UntypedInt ucb => 200;

internal const uint16 tcw = 40000;

[GoType("num:byte")] partial struct nb;

internal static void Main() {
    nuint k = 1;
    nint ki = 9;
    byte b = 1;
    byte cb = 200;
    uint16 w = 40000;
    int8 s8 = -100;
    int16 s16 = -30000;
    nb n = 200;
    fmt.Println((byte)(b + cb.Lsh(k)));
    fmt.Println(cb.Lsh(k));
    fmt.Println(tcb.Lsh(k));
    fmt.Println((byte)(b + ((byte)ucb).Lsh(k)));
    fmt.Println((byte)(cb << (int)(3)));
    fmt.Println(cb.Lsh((int64)(ki)));
    fmt.Println(w.Lsh(k));
    fmt.Println(tcw.Lsh(k));
    fmt.Println(s8.Lsh(k));
    fmt.Println(s16.Lsh(k));
    fmt.Println((nb)(byte)(n << (int)(k)));
    fmt.Println(cb.Rsh(k));
    fmt.Println(s8.Rsh(k));
    var x = (byte)(cb.Lsh(k));
    fmt.Println((byte)(x + b));
}

} // end main_package
