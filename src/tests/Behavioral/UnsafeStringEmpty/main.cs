namespace go;

using fmt = fmt_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    utf16();
    unsafeStrings();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object utf16ToStringˢ = (@string)"-- UTF16ToString --"u8;
private static readonly @string allNul14ˢ = "all-NUL [14]"u8;
private static readonly @string allNul260ˢ = "all-NUL [260]"u8;
private static readonly @string readmeTxtˢ = "README.TXT"u8;
private static readonly @string terminatedˢ = "terminated"u8;
private static readonly @string unterminatedˢ = "unterminated"u8;
private static readonly @string noTerminatorˢ = "NoTerminator"u8;
private static readonly @string leadingNulˢ = "leading NUL"u8;
private static readonly @string emptySliceˢ = "empty slice"u8;
private static readonly @string nilSliceˢ = "nil slice"u8;

internal static void utf16() {
    fmt.Println(utf16ToStringˢ);

    array<uint16> alt = new(14);
    array<uint16> name = new(260);

    show(allNul14ˢ, syscall.UTF16ToString(alt[..]));
    show(allNul260ˢ, syscall.UTF16ToString(name[..]));

    copy(name[..], encode(readmeTxtˢ));
    show(terminatedˢ, syscall.UTF16ToString(name[..]));

    show(unterminatedˢ, syscall.UTF16ToString(encode(noTerminatorˢ)));

    var poisoned = new uint16[]{0, (rune)'x', (rune)'y', (rune)'z'}.array();
    show(leadingNulˢ, syscall.UTF16ToString(poisoned[..]));

    show(emptySliceˢ, syscall.UTF16ToString(new uint16[]{}.slice()));
    show(nilSliceˢ, syscall.UTF16ToString(default!));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object unsafeStringˢ = (@string)"-- unsafe.String --"u8;
private static readonly @string makeLen0ˢ = "make len 0"u8;
private static readonly @string literalˢ = "literal {}"u8;
private static readonly @string makeCap8ˢ = "make cap 8"u8;
private static readonly @string resliced0ˢ = "resliced [:0]"u8;
private static readonly @string emptyStringˢ = "empty string"u8;
private static readonly @string fullˢ = "full"u8;
private static readonly @string prefixˢ = "prefix"u8;
private static readonly object zeroLengthSliceLenˢ = (@string)"zero-length Slice len ="u8;

internal static void unsafeStrings() {
    fmt.Println(unsafeStringˢ);

    var empty = new slice<byte>(0);
    show(makeLen0ˢ, @unsafe.String(@unsafe.SliceData(empty), 0));
    show(literalˢ, @unsafe.String(@unsafe.SliceData(new byte[]{}.slice()), 0));

    show(makeCap8ˢ, @unsafe.String(@unsafe.SliceData(new slice<byte>(0, 8)), 0));

    var body = slice<byte>("hello"u8);
    show(resliced0ˢ, @unsafe.String(@unsafe.SliceData(body[..0]), 0));

    slice<byte> nilSlice = default!;
    show(nilSliceˢ, @unsafe.String(@unsafe.SliceData(nilSlice), 0));
    show(emptyStringˢ, @unsafe.String(@unsafe.StringData(""u8), 0));

    show(fullˢ, @unsafe.String(@unsafe.SliceData(body), len(body)));
    show(prefixˢ, @unsafe.String(@unsafe.SliceData(body), 3));

    fmt.Println(zeroLengthSliceLenˢ, len(@unsafe.Slice(@unsafe.SliceData(empty), 0)));
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
