namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt constLit => /* 'a' */ 97;

internal static UntypedInt constDiff => /* 'a' - 'A' */ 32;

partial struct letter /*num:rune*/;

internal static bool isByte(byte b) {
    return b == 'x';
}

internal static rune takesRune(rune r) {
    return r + 1;
}

internal static rune retRune() {
    return 'Z';
}

internal static byte retByte() {
    return (byte)'z';
}

internal static void Main() {
    var bsᴷ = new stackAppend<byte>();

    fmt.Println((rune)'a');
    fmt.Printf("%T %v\n"u8, (rune)'a', (rune)'a');
    any anyv = (rune)'w';
    fmt.Printf("%T %v\n"u8, anyv, anyv);

    var r = (rune)'a';
    fmt.Printf("%T %v\n"u8, r, r);
    rune typed = 'b';
    fmt.Println(typed);

    fmt.Printf("%T %v %T %v\n"u8, (int32)(constLit), (int32)(constLit), (int32)(constDiff), (int32)(constDiff));
    rune x = constDiff;
    fmt.Printf("%T %v\n"u8, x, x);

    byte b = (byte)'q';
    switch (b) {
    case (byte)'p' or (byte)'q': {
        fmt.Println("byte case");
        break;
    }}

    fmt.Println(isByte((byte)'x'), retByte());
    var bs = new byte[]{(byte)'h', (byte)'i'}.slice();
    bs[0] = (byte)'H';
    bs = append(bsᴷ, bs, (byte)'!');
    fmt.Println((@string)bs);
    int8 i8 = (int8)'A';
    int16 i16 = (int16)'B';
    fmt.Println(i8, i16);

    var astral = (rune)0x1F600;
    fmt.Printf("%T %v %c\n"u8, astral, astral, astral);

    fmt.Println(max((rune)'a', r), min(r, (rune)'b'));

    fmt.Println(takesRune('y'), retRune());
    if (r == 'a' && r < 'z') {
        fmt.Println("rune compare");
    }
    switch (r) {
    case 'a' or 'b': {
        fmt.Println("rune case");
        break;
    }}

    var digit = (byte)'7';
    fmt.Println((byte)(digit - '0'), (nint)((byte)(digit - '0')));

    var rs = new rune[]{'x', 'y'}.slice();
    rs[1] = 'Y';
    var m = new map<@string, rune>{["a"] = 'A'};
    var keys = new array<bool>(128){['a'] = true, ['e'] = true};
    fmt.Println((@string)rs, m["a"], keys[(rune)'a'], keys[(rune)'b']);

    fmt.Println((byte)'a', (rune)'b', (uint16)'c', (float64)'d');
    uint32 u32 = 'u';
    uint64 u64 = 'v';
    fmt.Println(u32 - 'a', u64 * 'b', (float32)'e' / 2F);

    letter l = (rune)'L';
    fmt.Printf("%T %v\n"u8, l, l);

    var ch = new channel<rune>(1);
    ch.ᐸꟷ('s');
    fmt.Println(ᐸꟷ(ch));
}

} // end main_package
