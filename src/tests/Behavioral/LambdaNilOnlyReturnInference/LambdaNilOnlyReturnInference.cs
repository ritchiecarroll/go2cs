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
    return f("x"u8);
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object a1SliceResultˢ = (@string)"A1 slice result:"u8;
private static readonly object a2MapResultˢ = (@string)"A2 map result:"u8;
private static readonly object a3PointerResultˢ = (@string)"A3 pointer result:"u8;
private static readonly object a4ChanResultˢ = (@string)"A4 chan result:"u8;
private static readonly object a5FuncResultˢ = (@string)"A5 func result:"u8;
private static readonly object a6SliceResultLambdaˢ = (@string)"A6 slice-result lambda passed to a consumer:"u8;
private static readonly object a7PointerResultLambdaˢ = (@string)"A7 pointer-result lambda passed to a consumer:"u8;
private static readonly object b1ErrorResultˢ = (@string)"B1 error result:"u8;
private static readonly object b2VoidLambdaRanˢ = (@string)"B2 void lambda ran"u8;
private static readonly object b3SliceResultNonNilˢ = (@string)"B3 slice result non-nil:"u8;
private static readonly object b4MixedArmsˢ = (@string)"B4 mixed arms:"u8;
private static readonly object b5IntResultˢ = (@string)"B5 int result:"u8;

internal static void Main() {
    var g = slice<Value> (slice<Value> @in) => default!;
    fmt.Println(a1SliceResultˢ, g(default!) == default!, len(g(default!)));
    var m = map<@string, nint> (@string k) => default!;
    fmt.Println(a2MapResultˢ, applyMap(m) == default!, len(m("x"u8)));
    var p = ж<Value> () => default!;
    fmt.Println(a3PointerResultˢ, p() == nil);
    var c = channel<nint> () => default!;
    fmt.Println(a4ChanResultˢ, applyChan(c) == default!);
    var fn = Func<nint, nint> () => default!;
    fmt.Println(a5FuncResultˢ, applyFunc(fn) == default!);
    fmt.Println(a6SliceResultLambdaˢ, apply(g) == default!);
    fmt.Println(a7PointerResultLambdaˢ, applyPtr(p) == nil);
    var e = error (nint x) => default!;
    fmt.Println(b1ErrorResultˢ, applyErr(e) == default!);
    void v(@string a, @string b) {
        _ = a;
        _ = b;
    }
    v("x"u8, "y"u8);
    fmt.Println(b2VoidLambdaRanˢ);
    slice<Value> r() => new Value[]{new(N: 7)}.slice();
    fmt.Println(b3SliceResultNonNilˢ, r()[0].N);
    slice<Value> mix(bool b) {
        if (b) {
            return default!;
        }
        return new Value[]{new(N: 3)}.slice();
    }
    fmt.Println(b4MixedArmsˢ, mix(true) == default!, mix(false)[0].N);
    nint num(nint x) => x * 2;
    fmt.Println(b5IntResultˢ, num(21));
}

} // end main_package
