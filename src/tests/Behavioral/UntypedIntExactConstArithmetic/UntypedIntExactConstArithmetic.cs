namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt maxUint64 => /* 1<<64 - 1 */ 18446744073709551615;

internal static UntypedInt small => 10;

partial struct word /*num:nuint*/;

internal static UntypedInt wordMask => /* 1<<64 - 1 */ 18446744073709551615;

internal static array<uint64> quotients = new uint64[]{/* maxUint64 / 5 */ 3689348814741910323UL, /* maxUint64 / 5 / 5 */ 737869762948382064UL, /* maxUint64/10 + 1 */ 1844674407370955162UL}.array();

internal static slice<word> words = new word[]{0, 1, unchecked((nuint)(/* wordMask / 4 */ 4611686018427387903UL)), unchecked((nuint)(/* wordMask / 2 */ 9223372036854775807UL)), wordMask - 1, wordMask}.slice();

internal static nint trimmed() {
    UntypedInt div1e8m = 0xc767074b22e90e21;
    UntypedInt div1e4m = 0xd288ce703afb7e91;
    UntypedInt div1e2m = 0x8f5c28f5c28f5c29;
    UntypedInt div1e1m = 0xcccccccccccccccd;

    array<EmptyStruct> assert = new(1);
    _ = assert[(nint)(/* (div1e8m*5*5*5*5*5*5*5*5)%(1<<64) - 1 */ 0L)];
    _ = assert[(nint)(/* (div1e4m*5*5*5*5)%(1<<64) - 1 */ 0L)];
    _ = assert[(nint)(/* (div1e2m*5*5)%(1<<64) - 1 */ 0L)];
    _ = assert[(nint)(/* (div1e1m*5)%(1<<64) - 1 */ 0L)];

    return len(assert);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object assertsˢ = (@string)"asserts:"u8;

internal static void Main() {
    foreach (var (_, q) in quotients.ΔRangeSnapshot()) {
        fmt.Println(q);
    }

    foreach (var (_, w) in words) {
        fmt.Println((uint64)(nuint)w);
    }

    fmt.Println(assertsˢ, trimmed());

    uint64 wraps = maxUint64 - 1;
    int64 inRange = small * 3;
    fmt.Println(wraps, inRange);

    fmt.Println(0.5D, 1.25D, 3.0D, 1e21D, 123456.789D);
    fmt.Printf("%v %g %v\n"u8, 0.1D, 2.5e-8D, (float32)0.75F);
}

} // end main_package
