namespace go;

using fmt = fmt_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    utf16();
    unsafeStrings();
}

internal static void utf16() {
    fmt.Println("-- UTF16ToString --");

    array<uint16> alt = new(14);
    array<uint16> name = new(260);

    show("all-NUL [14]", syscall.UTF16ToString(alt[..]));
    show("all-NUL [260]", syscall.UTF16ToString(name[..]));

    copy(name[..], encode("README.TXT"));
    show("terminated", syscall.UTF16ToString(name[..]));

    show("unterminated", syscall.UTF16ToString(encode("NoTerminator")));

    var poisoned = new uint16[]{0, (rune)'x', (rune)'y', (rune)'z'}.array();
    show("leading NUL", syscall.UTF16ToString(poisoned[..]));

    show("empty slice", syscall.UTF16ToString(new uint16[]{}.slice()));
    show("nil slice", syscall.UTF16ToString(default));
}

internal static void unsafeStrings() {
    fmt.Println("-- unsafe.String --");

    var empty = new slice<byte>(0);
    show("make len 0", @unsafe.String(@unsafe.SliceData(empty), 0));
    show("literal {}", @unsafe.String(@unsafe.SliceData(new byte[]{}.slice()), 0));

    show("make cap 8", @unsafe.String(@unsafe.SliceData(new slice<byte>(0, 8)), 0));

    var body = slice<byte>("hello"u8);
    show("resliced [:0]", @unsafe.String(@unsafe.SliceData(body[..0]), 0));

    slice<byte> nilSlice = default;
    show("nil slice", @unsafe.String(@unsafe.SliceData(nilSlice), 0));
    show("empty string", @unsafe.String(@unsafe.StringData(""u8), 0));

    show("full", @unsafe.String(@unsafe.SliceData(body), len(body)));
    show("prefix", @unsafe.String(@unsafe.SliceData(body), 3));

    fmt.Println("zero-length Slice len =", len(@unsafe.Slice(@unsafe.SliceData(empty), 0)));
}

internal static slice<uint16> encode(@string s) {
    var @out = new slice<uint16>(0, len(s));
    foreach (var (_, r) in s) {
        @out = append(@out, (uint16)r);
    }
    return @out;
}

internal static void show(@string what, @string got) {
    fmt.Printf("%-14s %q len=%d\n"u8, what, got, len(got));
}

} // end main_package
