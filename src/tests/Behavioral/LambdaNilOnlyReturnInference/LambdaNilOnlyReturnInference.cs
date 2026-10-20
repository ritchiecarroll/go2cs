namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Value {
    public nint N;
}

internal static slice<Value> apply(Func<slice<Value>, slice<Value>> f) {
    return f(default!);
}

internal static ж<Value> applyPtr(Func<ж<Value>> f) {
    return f();
}

internal static map<@string, nint> applyMap(Func<@string, map<@string, nint>> f) {
    return f("x");
}

internal static channel<nint> applyChan(Func<channel<nint>> f) {
    return f();
}

internal static Func<nint, nint> applyFunc(Func<Func<nint, nint>> f) {
    return f();
}

internal static error applyErr(Func<nint, error> f) {
    return f(1);
}

internal static void Main() {
    var g = slice<Value> (slice<Value> @in) => default!;
    fmt.Println("A1 slice result:", g(default!) == default!, len(g(default!)));

    var m = map<@string, nint> (@string k) => default!;
    fmt.Println("A2 map result:", applyMap(m) == default!, len(m("x")));

    var p = ж<Value> () => default!;
    fmt.Println("A3 pointer result:", p() == nil);

    var c = channel<nint> () => default!;
    fmt.Println("A4 chan result:", applyChan(c) == default!);

    var fn = Func<nint, nint> () => default!;
    fmt.Println("A5 func result:", applyFunc(fn) == default!);

    fmt.Println("A6 slice-result lambda passed to a consumer:", apply(g) == default!);
    fmt.Println("A7 pointer-result lambda passed to a consumer:", applyPtr(p) == nil);

    var e = error (nint x) => default!;
    fmt.Println("B1 error result:", applyErr(e) == default!);

    void v(@string a, @string b) {
        _ = a;
        _ = b;
    }
    v("x", "y");
    fmt.Println("B2 void lambda ran");

    slice<Value> r() => new Value[]{new(N: 7)}.slice();
    fmt.Println("B3 slice result non-nil:", r()[0].N);

    slice<Value> mix(bool b) {
        if (b) {
            return default!;
        }
        return new Value[]{new(N: 3)}.slice();
    }
    fmt.Println("B4 mixed arms:", mix(true) == default!, mix(false)[0].N);

    nint num(nint x) => x * 2;
    fmt.Println("B5 int result:", num(21));
}

} // end main_package
