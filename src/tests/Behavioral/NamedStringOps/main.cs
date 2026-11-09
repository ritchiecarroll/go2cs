namespace go;

using cmp = cmp_package;
using fmt = fmt_package;
using slices = slices_package;
using strings = strings_package;

partial class main_package {

partial struct Path /*@string*/;

partial struct Tag /*@string*/;

internal static T maxOf<T>(params Span<T> xsʗp)
    where T : /* cmp.Ordered */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    var xs = xsʗp.slice();

    var m = xs[0];
    foreach (var (_, x) in range(xs[1..])) {
        if (x > m) {
            m = x;
        }
    }
    return m;
}

internal static @string describe(Tag t) {
    var exprᴛ1 = t;
    switch (ᐧ) {
    case {} when exprᴛ1 == (Tag)("alpha"u8): {
        return "first";
    }
    case {} when exprᴛ1 == (Tag)("omega"u8): {
        return "last";
    }}

    return "other";
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string appleˢ = "apple"u8;
private static readonly @string bananaˢ = "banana"u8;
private static readonly @string alphaˢ = "alpha"u8;
private static readonly @string omegaˢ = "omega"u8;
private static readonly @string betaˢ = "beta"u8;
private static readonly @string mixedˢ = "mixed"u8;

internal static void Main() {
    var ps = slice<Path>([(@string)"b/c", (@string)"a/z", (@string)"c", (@string)"a/b"]);
    slices.Sort<slice<Path>, Path>(ps);
    fmt.Println(ps);
    fmt.Println(slices.Max<slice<Path>, Path>(ps), slices.Min<slice<Path>, Path>(ps));
    var (i, found) = slices.BinarySearch(ps, (Path)(@string)"b/c");
    fmt.Println(i, found);
    fmt.Println(cmp.Compare((Path)(@string)"a", (Path)(@string)"b"), cmp.Compare((Path)(@string)"b", (Path)(@string)"a"), cmp.Compare((Path)(@string)"a", (Path)(@string)"a"));
    fmt.Println(cmp.Less((Path)(@string)"a", (Path)(@string)"b"), maxOf<Path>((@string)"m", (@string)"x", (@string)"c"));
    fmt.Println(min((Path)(@string)"q", (Path)(@string)"p"), max((Path)(@string)"q", (Path)(@string)"p", (Path)(@string)"r"));
    fmt.Println(slices.Index(ps, (@string)"c"), slices.Contains(ps, (@string)"a/z"));

    Path a = (Path)(@string)appleˢ;
    Path b = (Path)(@string)bananaˢ;
    fmt.Println(a < b, a <= b, a > b, a >= b, a == b, a != b, a < "b"u8, "z"u8 > b);
    Path joined = a + "/"u8 + b;
    fmt.Println(joined, len(joined));

    fmt.Println(joined[0], joined[1..4], joined.slice(len(joined) - 3));

    Path p = (Path)slice<byte>("/a/b"u8);
    Path q = (Path)slice<rune>(['x', 'é', 'y']);
    var bs = slice<byte>((@string)q);
    var rs = slice<rune>((@string)q);
    fmt.Println(p, q, len(q), bs, rs, (@string)p);

    nint n = 0;
    foreach (var (iΔ1, r) in p) {
        if (r == '/') {
            n++;
        }
        _ = iΔ1;
    }
    fmt.Println(n);
    foreach (var (iΔ2, r) in ((Path)(@string)((@string)(new byte[]{0x61, 0xff, 0xc3, 0xa9})))) {
        fmt.Print(iΔ2, ":", r, " ");
    }
    fmt.Println();
    nint count = 0;
    foreach (var (iΔ3, _) in q) {
        count += iΔ3;
    }
    fmt.Println(count);

    fmt.Println(describe(alphaˢ), describe(omegaˢ), describe(betaˢ));
    var m = new map<Tag, nint>{[(@string)"x"] = 1, [(@string)"y"] = 2};
    m[(@string)"x"] += 10;
    var keys = new slice<Tag>(0, len(m));
    foreach (var (k, _) in m) {
        keys = append(keys, k);
    }
    slices.Sort<slice<Tag>, Tag>(keys);
    fmt.Println(keys, m[(@string)"x"], m[(@string)"y"]);
    fmt.Println((Tag)strings.ToUpper((@string)((Tag)(@string)mixedˢ)));
}

} // end main_package
