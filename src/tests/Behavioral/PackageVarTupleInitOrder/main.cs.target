namespace go;

using errors = errors_package;
using fmt = fmt_package;

partial class main_package {

partial struct Sum /*[4]byte*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string bWasReadBeforeItWasˢ = "B was read before it was initialized"u8;

internal static (Sum, error) build(/*[4]*/ array<byte> src) {
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

public static Sum A;
internal static void initᴛA() {
    var (ᴛ1, ᴛ2) = build(B);
    A = must(ᴛ1, ᴛ2);
}

public static ж<Sum> ᏑAddressed = new StandardBox<Sum>(default(Sum));
public static ref Sum Addressed => ref ᏑAddressed.Value;
internal static void initᴛAddressed() {
    var (ᴛ3, ᴛ4) = build(B);
    Addressed = must(ᴛ3, ᴛ4);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object addressedˢ = (@string)"Addressed:"u8;

internal static void Main() {
    var p = ᏑAddressed;
    fmt.Println((@string)"A:"u8, A);
    fmt.Println(addressedˢ, p.Value);
}

} // end main_package
