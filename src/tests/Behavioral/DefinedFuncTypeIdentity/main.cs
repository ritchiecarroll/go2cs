global using Alias = System.Func<go.@string, go.error>;

namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using hooks = DefinedFuncTypeIdentity.hooks_package;
using DefinedFuncTypeIdentity;
using ꓸꓸꓸF = Span<main_package.F>;

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
internal static F retF(Func<@string, error> plain) { return NilSafeDelegateConversion<F, Func<@string, error>>(plain); }
internal static Func<@string, error> retPlain(F f) { return NilSafeDelegateConversion<Func<@string, error>, F>(f); }

internal static nint variadicF(params ꓸꓸꓸF fsʗp) {
    var fs = fsʗp.sslice();

    return len(fs);
}

internal static @string generic<T>(T v) { return fmt.Sprintf("%T"u8, (v).OrTypedNilOf()); }

partial struct stringer {
    internal @string s;
}

internal static error check(this stringer s, @string _) { return default; }

internal static void main() {
    F f = (@string _Δp0) => default;
    var plain = error (@string _Δp0) => default;

    show("F", (f).OrTypedNilFunc());
    F nilF = default;
    show("nil F", (nilF).OrTypedNilFunc());
    show("hooks.Handler", new hooks.Handler((nint x) => x));
    show("G", new G((nint x) => x));
    show("Alias", (((Alias)plain)).OrTypedNilFunc());
    show("unnamed", (plain).OrTypedNilFunc());

    any i = (f).OrTypedNilFunc();
    var (_, isF) = i._<F>(ᐧ);
    var (_, isPlain) = i._<Func<@string, error>>(ᐧ);
    fmt.Println("assert F", isF, "assert unnamed", isPlain);

    switch (i.type()) {
    case {} ᴛ0 when ᴛ0._<Func<@string, error>>(out var _): {
        fmt.Println("switch: unnamed");
        break;
    }
    case {} ᴛ0 when ᴛ0._<F>(out var _): {
        fmt.Println("switch: F");
        break;
    }}

    F a = NilSafeDelegateConversion<F, Func<@string, error>>(plain);
    Func<@string, error> b = NilSafeDelegateConversion<Func<@string, error>, F>(f);
    fmt.Println("assign", a("") == nil, b("") == nil);
    fmt.Println("argument", takeF(NilSafeDelegateConversion<F, Func<@string, error>>(plain)), takePlain(NilSafeDelegateConversion<Func<@string, error>, F>(f)));
    fmt.Println("return", retF(plain)("") == nil, retPlain(f)("") == nil);
    var h = new holder(f: NilSafeDelegateConversion<F, Func<@string, error>>(plain), fs: [NilSafeDelegateConversion<F, Func<@string, error>>(plain), f]);
    fmt.Println("composite", h.f("") == nil, len(h.fs));
    var ch = new channel<F>(1);
    ch.ᐸꟷ(NilSafeDelegateConversion<F, Func<@string, error>>(plain));
    fmt.Println("channel", (ᐸꟷ(ch))("") == nil);
    var fs = append(slice<F>([]), NilSafeDelegateConversion<F, Func<@string, error>>(plain), f);
    fmt.Println("append", len(fs), variadicF(NilSafeDelegateConversion<F, Func<@string, error>>(plain), f), variadicF(fs.ꓸꓸꓸ));
    fmt.Println("generic", generic(f), generic(plain), generic<F>(NilSafeDelegateConversion<F, Func<@string, error>>(plain)));
    var s = new stringer("m");

    var sʗ1 = s;
    F mv = (@string p1) => sʗ1.check(p1);
    var sʗ2 = s;
    fmt.Println("method value", mv("") == nil, NilSafeDelegateConversion<F, Func<@string, error>>((@string p1) => sʗ2.check(p1))("") == nil);
    F lit = (@string _Δp0) => fmt.Errorf("lit"u8);
    fmt.Println("func literal", lit("").Error());
    F none = default;
    fmt.Println("nil", none == nil, f != nil);
    fmt.Println("conversion", NilSafeDelegateConversion<F, Func<@string, error>>(plain)("") == nil, NilSafeDelegateConversion<Func<@string, error>, F>(f)("") == nil);

    fmt.Println("hooks", hooks.Apply((nint x) => x * 3, 2), hooks.Raw(NilSafeDelegateConversion<Func<nint, nint>, hooks.Handler>(hooks.Make(5)), 1), hooks.Apply(hooks.Make(1), 1));

    Alias al = NilSafeDelegateConversion<Alias, F>(f);
    fmt.Println("alias", al("") == nil);
    G g = (nint x) => x + 1;
    fmt.Println("method type", g.Twice(1));
}

} // end main_package
