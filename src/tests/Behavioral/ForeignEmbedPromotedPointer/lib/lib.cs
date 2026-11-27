namespace go.ForeignEmbedPromotedPointer;

partial class lib_package {

partial interface Named {
    @string Name();
}

partial interface Counter {
    @string Name();
    nint Inc();
}

partial struct T {
    internal nint n;
}

internal static @string Name⁔(/*this*/ T t) { return "lib.T"; }

internal static nint Inc⁔(/*this*/ ref T t) {
    t.n++;
    return t.n;
}

partial interface Adder {
    (nint, error) Add(nint a, nint b);
}

internal static (nint, error) Add⁔(/*this*/ ref T t, nint a, nint b) {
    t.n += a + b;

    if (t.n > 100) {
        return (t.n, errBig);
    }

    return (t.n, default);
}

partial struct bigError {
}

internal static @string Error⁔(/*this*/ bigError _) { return "too big"; }

internal static error errBig = new bigError();

partial interface Tagger {
    @string Tag();
}

partial struct Inner {
    internal @string tag;
}

internal static void SetTag⁔(/*this*/ ref Inner i, @string tag) { i.tag = tag; }
internal static @string Tag⁔(/*this*/ ref Inner i) { return "inner:"u8 + i.tag; }

partial struct V {
    /*embed*/ public Inner Inner;
}

} // end lib_package
