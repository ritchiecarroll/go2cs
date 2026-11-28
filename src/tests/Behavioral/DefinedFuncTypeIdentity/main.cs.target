global using Alias = System.Func<go.@string, go.error>;

namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using hooks = DefinedFuncTypeIdentity.hooks_package;
using DefinedFuncTypeIdentity;
using ꓸꓸꓸF = variadic<main_package.F>;

partial class main_package {

public delegate error F(@string _);

public delegate nint G(nint _);

public static nint Twice(this G g, nint x) { return g(g(x)); }

partial struct holder {
    internal F f;
    internal slice<F> fs;
}

internal static void show(@string label, any v) {
    var t = reflect.TypeOf(v);
    fmt.Printf("%s: %%T=%T name=%q string=%q kind=%v\n"u8, label, v, t.Name(), t.String(), t.Kind());
}

internal static @string takeF(F f) { return fmt.Sprint(f("x") == nil); }
internal static @string takePlain(Func<@string, error> f) { return fmt.Sprint(f("y") == nil); }
internal static F retF(Func<@string, error> plain) { return builtin.NilSafeDelegateConversion<F, Func<@string, error>>(plain); }
internal static Func<@string, error> retPlain(F f) { return builtin.NilSafeDelegateConversion<Func<@string, error>, F>(f); }

internal static nint variadicF(params ꓸꓸꓸF fsʗp) {
    var fs = fsʗp.sslice();

    return len(fs);
}

internal static @string generic<T>(T v) { return fmt.Sprintf("%T"u8, (v).OrTypedNilOf()); }

partial struct stringer {
    internal @string s;
}

internal static (nint, F) pairF() { return (1, [GoFunc("pairF.func1")] (@string _) => default); }

internal static @string /*out*/ deferF(F f) {
    @string @out = default;
    GoFrame ᒐ = default;
    try {
        defer(builtin.NilSafeDelegateConversion<Func<@string, error>, F>(f), (@string)"deferred", ref ᒐ);
        @out = "deferred call";
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return @out;
}

partial struct node {
    internal nint v;
}

internal static nint add(this ref node n, nint x) { return n.v + x; }
internal static nint mul(this ref node n, nint x) { return n.v * x; }

internal delegate nint handler(ж<node> _Δp0, nint _Δp1);

internal static error check(this stringer s, @string _) { return default; }

internal static void main() {
    F f = [GoFunc("main.func1")] (@string _Δp0) => default;
    var plain = [GoFunc("main.func2")] error (@string _Δp0) => default;

    show("F", (f).OrTypedNilFunc());
    F nilF = default;
    show("nil F", (nilF).OrTypedNilFunc());
    show("hooks.Handler", new hooks.Handler([GoFunc("main.func3")] (nint x) => x));
    show("G", new G([GoFunc("main.func4")] (nint x) => x));
    show("Alias", (((Alias)plain)).OrTypedNilFunc());
    show("unnamed", (plain).OrTypedNilFunc());

    any i = (f).OrTypedNilFunc();
    var (_, isF) = i._ᶠ<F>(ᐧ);
    var (_, isPlain) = i._ᶠ<Func<@string, error>>(ᐧ);
    fmt.Println("assert F", isF, "assert unnamed", isPlain);

    switch (i.type()) {
    case {} ᴛ0 when ᴛ0._ᶠ<Func<@string, error>>(out var _): {
        fmt.Println("switch: unnamed");
        break;
    }
    case {} ᴛ0 when ᴛ0._ᶠ<F>(out var _): {
        fmt.Println("switch: F");
        break;
    }}

    F a = builtin.NilSafeDelegateConversion<F, Func<@string, error>>(plain);
    Func<@string, error> b = builtin.NilSafeDelegateConversion<Func<@string, error>, F>(f);
    fmt.Println("assign", a("") == nil, b("") == nil);
    fmt.Println("argument", takeF(builtin.NilSafeDelegateConversion<F, Func<@string, error>>(plain)), takePlain(builtin.NilSafeDelegateConversion<Func<@string, error>, F>(f)));
    fmt.Println("return", retF(plain)("") == nil, retPlain(f)("") == nil);
    var h = new holder(f: builtin.NilSafeDelegateConversion<F, Func<@string, error>>(plain), fs: [builtin.NilSafeDelegateConversion<F, Func<@string, error>>(plain), f]);
    fmt.Println("composite", h.f("") == nil, len(h.fs));
    var ch = new channel<F>(1);
    ch.ᐸꟷ(builtin.NilSafeDelegateConversion<F, Func<@string, error>>(plain));
    fmt.Println("channel", (ᐸꟷ(ch))("") == nil);
    var fs = append(slice<F>([]), builtin.NilSafeDelegateConversion<F, Func<@string, error>>(plain), f);
    fmt.Println("append", len(fs), variadicF(builtin.NilSafeDelegateConversion<F, Func<@string, error>>(plain), f), variadicF(fs.ꓸꓸꓸ));
    fmt.Println("generic", generic(f), generic(plain), generic<F>(builtin.NilSafeDelegateConversion<F, Func<@string, error>>(plain)));
    var s = new stringer("m");

    var sʗ1 = s;
    F mv = (@string p1) => sʗ1.check(p1);
    var sʗ2 = s;
    fmt.Println("method value", mv("") == nil, builtin.NilSafeDelegateConversion<F, Func<@string, error>>((@string p1) => sʗ2.check(p1))("") == nil);
    F lit = [GoFunc("main.func5")] (@string _Δp0) => fmt.Errorf("lit"u8);
    fmt.Println("func literal", lit("").Error());
    F none = default;
    fmt.Println("nil", none == nil, f != nil);
    fmt.Println("conversion", builtin.NilSafeDelegateConversion<F, Func<@string, error>>(plain)("") == nil, builtin.NilSafeDelegateConversion<Func<@string, error>, F>(f)("") == nil);

    nint n = default;
    Func<@string, error> cancel = default;
    (n, var cancelᴛ1) = pairF();
    cancel = builtin.NilSafeDelegateConversion<Func<@string, error>, F>(cancelᴛ1);
    fmt.Println("tuple", n, cancel("") == nil);
    fmt.Println("defer", deferF(f));
    var table = new array<handler>(3){[1] = new handler(((Func<ж<node>, nint, nint>)(main_package.add))), [2] = new handler(((Func<ж<node>, nint, nint>)(main_package.mul)))};
    var byName = new map<@string, F>{["plain"] = builtin.NilSafeDelegateConversion<F, Func<@string, error>>(plain), ["f"] = f};
    fmt.Println("keyed", table[1](Ꮡ(new node(2)), 3), table[2](Ꮡ(new node(2)), 3), table[0] == nil, byName["plain"]("") == nil, len(byName));

    fmt.Println("hooks", hooks.Apply([GoFunc("main.func6")] (nint x) => x * 3, 2), hooks.Raw(builtin.NilSafeDelegateConversion<Func<nint, nint>, hooks.Handler>(hooks.Make(5)), 1), hooks.Apply(hooks.Make(1), 1));

    Alias al = builtin.NilSafeDelegateConversion<Alias, F>(f);
    fmt.Println("alias", al("") == nil);
    G g = [GoFunc("main.func7")] (nint x) => x + 1;
    fmt.Println("method type", g.Twice(1));
}

} // end main_package
