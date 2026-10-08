namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct htmlSig /*[]byte*/;

partial struct runeSig /*[]rune*/;

internal static nint sigLen(htmlSig s) {
    return len(s);
}

internal static void Main() {
    var sig = ((htmlSig)slice<byte>((@string)"<!DOCTYPE HTML"u8));
    fmt.Println(len(sig), ((@string)(slice<byte>)sig));
    var sigs = new htmlSig[]{
        ((htmlSig)slice<byte>((@string)"<HTML"u8)),
        ((htmlSig)slice<byte>((@string)"<!--"u8))
    }.slice();
    foreach (var (_, s) in sigs) {
        fmt.Println(len(s), ((@string)(slice<byte>)s));
    }
    fmt.Println(sig[0], sig[1], sigs[0][0]);
    fmt.Println(sigLen(((htmlSig)slice<byte>((@string)"<HEAD"u8))));
    var r = ((runeSig)slice<rune>((@string)"héllo, 世界"u8));
    fmt.Println(len(r), ((@string)(slice<rune>)r));
    fmt.Println(r[1], r[7]);
    var plain = slice<byte>("<?xml"u8);
    fmt.Println(len(plain), ((@string)plain));
}

} // end main_package
