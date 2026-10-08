namespace go;

using fmt = fmt_package;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

partial struct Code /*num:nint*/;

public static Code A => /* iota */ 0;
internal static Code _ᴛ1ʗ => 1;
public static Code B => 2;
internal static Code _ᴛ2ʗ => 3;
public static Code C => 4;

internal static void _ᴛ3() {
    if (A + B + C < 0) {
        throw panic("unreachable");
    }
    Code x = A;
    _ = x;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object multiBlankOkˢ = (@string)"multiBlank ok"u8;

internal static void multiBlank() {
    nint a = 1;
    nint b = 2;
    nint c = 3;
    nint d = 4;
    _ = a;
    _ = b;
    _ = c;
    _ = d;
    fmt.Println(multiBlankOkˢ);
}

internal delegate stateFn stateFn(nint _Δp0);

internal static stateFn lexText(nint i) {
    return lexNumber;
}

internal static stateFn lexNumber(nint i) {
    return default!;
}

internal static (@string, error) pair(@string a, nint b) {
    return (a, default!);
}

internal static void sink(nint a) {
}

internal static nint count() {
    return 7;
}

internal static nint total(params ꓸꓸꓸnint aʗp) {
    var a = aʗp.sslice();

    nint n = 0;
    foreach (var (_, v) in a) {
        n += v;
    }
    return n;
}

partial struct counter {
    internal nint n;
}

internal static nint bump(this ref counter c, nint d) {
    c.n += d;
    return c.n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string pairˢ = "pair"u8;

internal static void blankFuncValues() {
    _ = (Func<@string, nint, (@string, error)>)(pair);
    _ = (Action<nint>)(sink);
    _ = (Func<nint>)(count);
    _ = (stateFn)(lexText);
    _ = (stateFn)(lexNumber);
    _ = (Funcꓸꓸꓸ<nint, nint>)(total);
    _ = (Funcꓸꓸꓸ<@string, any, @string>)(fmt.Sprintfᶠ);
    var c = Ꮡ(new counter(nil));
    
    var cʗ1 = c;
    _ = (Func<nint, nint>)((nint p1) => cʗ1.bump(p1));
    _ = (Func<@string, nint>)((@string sΔ1) => len(sΔ1));
    var f = pair;
    _ = f;
    stateFn state = lexText;
    state = lexNumber;
    _ = state;
    var (s, _) = f(pairˢ, 1);
    fmt.Println(s, count(), total(1, 2, 3), c.bump(5));
}

internal static void Main() {
    fmt.Println(A, B, C);
    multiBlank();
    blankFuncValues();
}

} // end main_package
