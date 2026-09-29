namespace go;

using errors = errors_package;
using fmt = fmt_package;

partial class main_package {

[GoType("[4]byte")] partial struct Sum;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string bWasReadBeforeItWasˢ = "B was read before it was initialized"u8;

internal static (Sum, error) build([GoArrayDims(4)] array<byte> src) {
    src = src.Clone();

    if (src[0] == 0) {
        return (new Sum(new byte[4].array()), errors.New(bWasReadBeforeItWasˢ));
    }
    return (((Sum)src), default!);
}

internal static Sum must(Sum s, error err) {
    s = s.Clone();

    if (err != default!) {
        throw panic(err);
    }
    return s.Clone();
}

internal static (Sum, error) tupleᴛ1ʗ = build(B);
public static Sum A = must(tupleᴛ1ʗ.Item1, tupleᴛ1ʗ.Item2);

internal static (Sum, error) tupleᴛ2ʗ = build(B);
public static ж<Sum> ᏑAddressed = new StandardBox<Sum>(must(tupleᴛ2ʗ.Item1, tupleᴛ2ʗ.Item2));
public static ref Sum Addressed => ref ᏑAddressed.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object addressedˢ = (@string)"Addressed:"u8;

internal static void Main() {
    var p = ᏑAddressed;
    fmt.Println((@string)"A:"u8, A);
    fmt.Println(addressedˢ, p.Value);
}

} // end main_package
