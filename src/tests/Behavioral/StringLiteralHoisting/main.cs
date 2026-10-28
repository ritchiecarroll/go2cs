namespace go;

using fmt = fmt_package;
using ꓸꓸꓸstring = Span<@string>;

partial class main_package {

internal static @string kind(nint n) {
    if (n == 0) {
        return "zero value return";
    }

    return "other value return";
}

internal static @string echo(@string s) {
    return s;
}

internal static @string joinAll(params ꓸꓸꓸstring partsʗp) {
    var parts = partsʗp.sslice();

    @string @out = "";

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
    b[0] = (byte)'B';

    return len(b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string namedStringConversionˢ = "named string conversion"u8;

internal static void Main() {
    fmt.Println(kind(0), kind(1));

    fmt.Println(echo("shared across files"));

    fmt.Println(joinAll("variadic element one", "variadic element two"));

    box b = default;
    b.name = "struct field assignment";
    b.tag = "struct field second";
    fmt.Println(b.name, b.tag);

    fmt.Println((@string)((label)(@string)namedStringConversionˢ));

    var counts = new map<@string, nint>{};
    counts["standalone map key"]++;
    counts["standalone map key"] += 2;
    fmt.Println(counts["standalone map key"]);

    fmt.Println("pre boxed any target");

    fmt.Println(echo("mixed use literal"), "mixed use literal");

    fmt.Println(isSentinel("comparison operand literal"), isSentinel("nope"));
    fmt.Println(withSuffix("tail"));
    fmt.Println(mutableBytes());

    fmt.Printf("format position literal %s\n"u8, "format trailing argument");

    fmt.Println(echo("OK"), echo("yes"), echo("true"));

    fmt.Println(echo("BLAKE2s-256"), echo("HTTPServer ready"));

    fmt.Println(echo("") == ""u8);

    var parts = new @string[]{"composite element one", "composite element two"}.slice();
    var boxed = new box("struct composite element", "struct composite tag");
    var table = new map<@string, @string>{["composite key"] = "composite value"};
    fmt.Println(parts[0], parts[1], boxed.name, boxed.tag, table["composite key"]);

    var anys = new any[]{(@string)"any composite element"u8, (nint)7}.slice();
    fmt.Println(anys[0], anys[1]);

    fmt.Println(pkgLevelLiteral);
    fmt.Println(derivedAtInit, len(derivedAtInit) > 0);

    fmt.Println(echo("the quick brown fox jumps over"));
    fmt.Println(echo("the quick brown fox jumps under"));

    fmt.Println(echo("世界世界"));
}

} // end main_package
