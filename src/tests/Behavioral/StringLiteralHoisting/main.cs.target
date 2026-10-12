namespace go;

using fmt = fmt_package;
using ꓸꓸꓸstring = Span<@string>;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string zeroValueReturnˢ = "zero value return"u8;
private static readonly @string otherValueReturnˢ = "other value return"u8;

internal static @string kind(nint n) {
    if (n == 0) {
        return zeroValueReturnˢ;
    }

    return otherValueReturnˢ;
}

internal static @string echo(@string s) {
    return s;
}

internal static @string joinAll(params ꓸꓸꓸstring partsʗp) {
    var parts = partsʗp.sslice();

    @string @out = ""u8;

    foreach (var (_, p) in parts) {
        @out = @out + p + "|"u8;
    }

    return @out;
}

internal static bool isSentinel(@string word) {
    return word == "comparison operand literal"u8;
}

internal static @string withSuffix(@string suffix) {
    return "concat left operand "u8 + suffix;
}

internal static nint mutableBytes() {
    var b = slice<byte>("byte slice source literal"u8);
    b[0] = (rune)'B';

    return len(b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sharedAcrossFilesˢ = "shared across files"u8;
private static readonly @string variadicElementOneˢ = "variadic element one"u8;
private static readonly @string variadicElementTwoˢ = "variadic element two"u8;
private static readonly @string structFieldAssignmentˢ = "struct field assignment"u8;
private static readonly @string structFieldSecondˢ = "struct field second"u8;
private static readonly @string namedStringConversionˢ = "named string conversion"u8;
private static readonly @string standaloneMapKeyˢ = "standalone map key"u8;
private static readonly object preBoxedAnyTargetˢ = (@string)"pre boxed any target"u8;
private static readonly @string mixedUseLiteralˢ = "mixed use literal"u8;
private static readonly @string comparisonOperandLiteralˢ = "comparison operand literal"u8;
private static readonly @string nopeˢ = "nope"u8;
private static readonly @string tailˢ = "tail"u8;
private static readonly object formatTrailingArgumentˢ = (@string)"format trailing argument"u8;
private static readonly @string yesˢ = "yes"u8;
private static readonly @string trueˢ = "true"u8;
private static readonly @string blake2s256ˢ = "BLAKE2s-256"u8;
private static readonly @string httpServerReadyˢ = "HTTPServer ready"u8;
private static readonly @string compositeKeyˢ = "composite key"u8;
private static readonly @string theQuickBrownFoxJumpsˢ = "the quick brown fox jumps over"u8;
private static readonly @string theQuickBrownFoxJumpsˢ2 = "the quick brown fox jumps under"u8;

internal static void Main() {
    fmt.Println(kind(0), kind(1));

    fmt.Println(echo(sharedAcrossFilesˢ));

    fmt.Println(joinAll(variadicElementOneˢ, variadicElementTwoˢ));

    box b = default!;
    b.name = structFieldAssignmentˢ;
    b.tag = structFieldSecondˢ;
    fmt.Println(b.name, b.tag);

    fmt.Println(((@string)((label)(@string)namedStringConversionˢ)));

    var counts = new map<@string, nint>{};
    counts[standaloneMapKeyˢ]++;
    counts[standaloneMapKeyˢ] += 2;
    fmt.Println(counts[standaloneMapKeyˢ]);

    fmt.Println(preBoxedAnyTargetˢ);

    fmt.Println(echo(mixedUseLiteralˢ), mixedUseLiteralˢ);

    fmt.Println(isSentinel(comparisonOperandLiteralˢ), isSentinel(nopeˢ));
    fmt.Println(withSuffix(tailˢ));
    fmt.Println(mutableBytes());

    fmt.Printf("format position literal %s\n"u8, formatTrailingArgumentˢ);

    fmt.Println(echo("OK"u8), echo(yesˢ), echo(trueˢ));

    fmt.Println(echo(blake2s256ˢ), echo(httpServerReadyˢ));

    fmt.Println(echo(""u8) == ""u8);

    var parts = new @string[]{"composite element one"u8, "composite element two"u8}.slice();
    var boxed = new box("struct composite element"u8, "struct composite tag"u8);
    var table = new map<@string, @string>{["composite key"u8] = "composite value"u8};
    fmt.Println(parts[0], parts[1], boxed.name, boxed.tag, table[compositeKeyˢ]);

    var anys = new any[]{(@string)"any composite element"u8, (nint)(7)}.slice();
    fmt.Println(anys[0], anys[1]);

    fmt.Println(pkgLevelLiteral);
    fmt.Println(derivedAtInit, len(derivedAtInit) > 0);

    fmt.Println(echo(theQuickBrownFoxJumpsˢ));
    fmt.Println(echo(theQuickBrownFoxJumpsˢ2));

    fmt.Println(echo("世界世界"u8));
}

} // end main_package
