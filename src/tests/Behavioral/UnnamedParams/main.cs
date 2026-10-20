namespace go;

using fmt = fmt_package;
using ꓸꓸꓸbyte = Span<byte>;
using ꓸꓸꓸnint = Span<nint>;
using ꓸꓸꓸstring = Span<@string>;

partial class main_package {

partial struct note {
    internal nint x;
}

internal static nint count;

internal static void setup(ж<note> _) {
    count += 1;
}

internal static void discard(nint _) {
    count += 10;
}

internal static void unnamedVariadic(params ꓸꓸꓸstring ʗp) {
    count += 100;
}

internal static void blankVariadic(params ꓸꓸꓸnint _ʗp) {
    count += 1000;
}

internal static void bothUnnamed(nint _Δp0, params ꓸꓸꓸstring ʗp) {
    count += 10000;
}

internal static @string label(@string tag, params ꓸꓸꓸnint _ʗp) {
    return tag + "!"u8;
}

internal static nint tally(this note n, params ꓸꓸꓸbyte ʗp) {
    return n.x;
}

internal static nint total(params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.sslice();

    nint sum = 0;
    foreach (var (_, v) in vals) {
        sum += v;
    }
    return sum;
}

internal static void Main() {
    ref var n = ref heap(new note(), out var Ꮡn);
    setup(Ꮡn);
    discard(5);
    unnamedVariadic("a", "b");
    blankVariadic(1, 2);
    bothUnnamed(3, "c");
    fmt.Println(count);
    fmt.Println(label("tag", 7, 8));
    fmt.Println(new note(9).tally(1, 2));
    fmt.Println(total(1, 2, 3, 4));

    @string litUnnamed(params ꓸꓸꓸnint _ʗp) => "lit-unnamed";
    @string litBlank(params ꓸꓸꓸstring _ʗp) => "lit-blank";
    nint litNamed(params ꓸꓸꓸstring partsʗp) {
        var parts = partsʗp.sslice();
        return len(parts);
    }
    fmt.Println(litUnnamed(1, 2), litBlank("x"), litNamed("p", "q", "r"));
}

} // end main_package
