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

internal partial interface lener_R0 /*dyn*/ {
    nint Len();
}

internal static lener_R0 lener(any a) {
    var (v, _) = a._<lener_R0>(ᐧ);
    return v;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pointerSameAnyTwiceˢ = (@string)"pointer, same any twice:"u8;
private static readonly object pointerSameBoxInANewAnyˢ = (@string)"pointer, same box in a new any:"u8;
private static readonly object pointerTwoBoxesWithEqualˢ = (@string)"pointer, two boxes with equal contents:"u8;
private static readonly object valueSameAnyTwiceˢ = (@string)"value, same any twice:"u8;
private static readonly object valueEqualValuesInTwoˢ = (@string)"value, equal values in two anys:"u8;
private static readonly object valueDifferentValuesˢ = (@string)"value, different values:"u8;
private static readonly object pointerShellAgainstAnyPˢ = (@string)"pointer shell against any(p):"u8;
private static readonly object valueShellAgainstAnyˢ = (@string)"value shell against any(counter(3)):"u8;
private static readonly object valueShellAgainstAnyˢ2 = (@string)"value shell against any(counter(4)):"u8;
private static readonly object pairsEqualˢ = (@string)"pairs equal:"u8;
private static readonly object pairsUnequalˢ = (@string)"pairs unequal:"u8;

internal static void Main() {
    var p = Ꮡ(new box(n: 1));
    var q = Ꮡ(new box(n: 1));
    var values = new any[]{p.OrTypedNil(), q.OrTypedNil(), ((counter)3), ((counter)3), ((counter)4)}.slice();
    fmt.Println(pointerSameAnyTwiceˢ, AreEqual(lener(values[0]), lener(values[0])));
    fmt.Println(pointerSameBoxInANewAnyˢ, AreEqual(lener(values[0]), lener(((any)p.OrTypedNil()))));
    fmt.Println(pointerTwoBoxesWithEqualˢ, AreEqual(lener(values[0]), lener(values[1])));
    fmt.Println(valueSameAnyTwiceˢ, AreEqual(lener(values[2]), lener(values[2])));
    fmt.Println(valueEqualValuesInTwoˢ, AreEqual(lener(values[2]), lener(values[3])));
    fmt.Println(valueDifferentValuesˢ, AreEqual(lener(values[2]), lener(values[4])));
    fmt.Println(pointerShellAgainstAnyPˢ, AreEqual(((any)lener(values[0])), ((any)p.OrTypedNil())));
    fmt.Println(valueShellAgainstAnyˢ, AreEqual(((any)lener(values[2])), ((any)((counter)3))));
    fmt.Println(valueShellAgainstAnyˢ2, AreEqual(((any)lener(values[2])), ((any)((counter)4))));
    nint same = 0;
    nint different = 0;
    for (nint i = 0; i < 100; i++) {
        foreach (var (j, a) in values) {
            foreach (var (k, b) in values) {
                if (AreEqual(lener(a), lener(b))){
                    same++;
                } else 
                if (j != k) {
                    different++;
                }
            }
        }
    }
    fmt.Println(pairsEqualˢ, same, pairsUnequalˢ, different);
}

} // end main_package
