namespace go;

using fmt = fmt_package;

partial class main_package {

public static T IfElse<T>(bool b, T whenTrue, T whenFalse) {
    if (b) {
        return whenTrue;
    }
    return whenFalse;
}

internal static T first<T>(params Span<T> xsʗp) {
    var xs = xsʗp.sslice();

    return xs[0];
}

partial struct NameGenerator {
    public Func<@string, bool, bool> IsUnique;
}

partial struct gen {
    internal map<@string, bool> used;
}

internal static bool isUnique(this ref gen g, @string name, bool @private) { return !g.used[name]; }

internal static @string makeName(@string @base, Func<@string, bool, bool> check) {
    if (check == nil || check(@base, false)) {
        return @base;
    }
    return @base + "_1"u8;
}

internal static bool isNilAny(any v) { return v == nil; }

internal static void pick(Func<nint> f, bool cond) {
    var g = IfElse(cond, f, (Func<nint>)(default));
    var h = first(f);
    fmt.Println("param:", g == nil, h == nil, isNilAny((f).OrTypedNilFunc()), isNilAny((g).OrTypedNilFunc()));
}

internal static void Main() {
    var used = new map<@string, bool>{["x"] = true};

    var usedʗ1 = used;
    var ng = Ꮡ(new NameGenerator(IsUnique: (@string name, bool @private) => !usedʗ1[name]));
    fmt.Println("field:", makeName("x", IfElse(true, ng.IsUnique, (Func<@string, bool, bool>)(default))), makeName("x", IfElse(false, ng.IsUnique, (Func<@string, bool, bool>)(default))));

    var g = Ꮡ(new gen(used: used));
    Func<@string, bool, bool> f = g.isUnique;
    fmt.Println("variable:", makeName("x", IfElse(true, f, (Func<@string, bool, bool>)(default))), makeName("x", IfElse(false, f, (Func<@string, bool, bool>)(default))));

    fmt.Println("method value:", makeName("x", IfElse<Func<@string, bool, bool>>(true, g.isUnique, (Func<@string, bool, bool>)(default))));
    fmt.Println("literal:", makeName("y", IfElse(true, (@string _Δp0, bool _Δp1) => false, (Func<@string, bool, bool>)(default))));

    pick(default, true);
    pick(() => 1, true);
    pick(() => 1, false);
}

} // end main_package
