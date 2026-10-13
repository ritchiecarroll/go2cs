namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object aIsLessThan0ˢ = (@string)"a is less than 0"u8;
private static readonly object aIsGreaterThan0ˢ = (@string)"a is greater than 0"u8;
private static readonly object bBeforeˢ = (@string)"b before ="u8;
private static readonly object bNowˢ = (@string)"b now ="u8;
private static readonly object bIsLessThan0ˢ = (@string)"b is less than 0"u8;
private static readonly object bIsGreaterThan0ˢ = (@string)"b is greater than 0"u8;
private static readonly object bAfterˢ = (@string)"b after ="u8;

internal static void Main() {
    {
        nint a = -1; if (a < 0) {
            fmt.Println(aIsLessThan0ˢ);
        }
    }

    {
        nint a = 1; if (a > 0) {
            fmt.Println(aIsGreaterThan0ˢ);
        }
    }

    nint b = 99;
    fmt.Println(bBeforeˢ, b);

    {
        nint bΔ1 = -1; if (bΔ1 < 0) {
            fmt.Println(bNowˢ, bΔ1);
            fmt.Println(bIsLessThan0ˢ);
        }
    }

    {
        nint bΔ2 = 1; if (bΔ2 > 0) {
            fmt.Println(bNowˢ, bΔ2);
            fmt.Println(bIsGreaterThan0ˢ);
        }
    }

    fmt.Println(bAfterˢ, b);
}

} // end main_package
