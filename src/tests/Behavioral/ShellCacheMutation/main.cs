namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct box {
    internal nint n;
}

internal static nint Len(this ref box b) {
    return b.n;
}

partial struct counter /*num:nint*/;

internal static nint Len(this counter c) {
    return (nint)c;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object firstAssertˢ = (@string)"first assert:"u8;
private static readonly object oldShellAfterWriteˢ = (@string)"old shell after write:"u8;
private static readonly object reAssertAfterWriteˢ = (@string)"re-assert after write:"u8;
private static readonly object otherBoxˢ = (@string)"other box:"u8;
private static readonly object otherBoxAfterWriteˢ = (@string)"other box after write:"u8;
private static readonly object firstBoxStillˢ = (@string)"first box still:"u8;
private static readonly object valueˢ = (@string)"value"u8;
private static readonly object sumˢ = (@string)"sum:"u8;
private static readonly object oldShellsAtTheEndˢ = (@string)"old shells at the end:"u8;

internal partial interface main_type /*dyn*/ {
    nint Len();
}

internal static void Main() {
    var p = Ꮡ(new box(n: 1));
    var q = Ꮡ(new box(n: 10));
    var values = new any[]{p.OrTypedNil(), q.OrTypedNil(), ((counter)3), ((counter)4)}.slice();
    var (first, ok) = values[0]._<main_type>(ᐧ);
    fmt.Println(firstAssertˢ, ok, first.Len());
    p.Value.n = 2;
    fmt.Println(oldShellAfterWriteˢ, first.Len());
    (var again, ok) = values[0]._<main_type>(ᐧ);
    fmt.Println(reAssertAfterWriteˢ, ok, again.Len());
    (var other, ok) = values[1]._<main_type>(ᐧ);
    fmt.Println(otherBoxˢ, ok, other.Len());
    q.Value.n = 11;
    fmt.Println(otherBoxAfterWriteˢ, other.Len(), firstBoxStillˢ, again.Len());
    for (nint i = 2; i < len(values); i++) {
        var (v, okΔ1) = values[i]._<main_type>(ᐧ);
        fmt.Println(valueˢ, i, okΔ1, v.Len());
    }
    nint sum = 0;
    for (nint i = 0; i < 1000; i++) {
        if (i == 500) {
            p.Value.n = 3;
        }
        foreach (var (_, a) in values) {
            {
                var (v, okΔ2) = a._<main_type>(ᐧ); if (okΔ2) {
                    sum += v.Len();
                }
            }
        }
    }
    fmt.Println(sumˢ, sum);
    fmt.Println(oldShellsAtTheEndˢ, first.Len(), again.Len(), other.Len());
}

} // end main_package
